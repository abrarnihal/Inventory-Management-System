using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace coderush.Filters
{
    public class NotificationActionFilter : IAsyncActionFilter
    {
        private static readonly HashSet<string> SkipControllers = new(StringComparer.OrdinalIgnoreCase)
        {
            "Notification", "Captcha", "UploadProfilePicture", "ChatBot", "ChangePassword", "Health"
        };

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var executedContext = await next();

            if (executedContext.Result is not OkObjectResult)
                return;

            if (executedContext.ActionDescriptor is not ControllerActionDescriptor cad)
                return;

            string controllerName = cad.ControllerName;
            string actionName = cad.ActionName;

            if (SkipControllers.Contains(controllerName))
                return;

            string entityAction = actionName switch
            {
                "Insert" => "Added",
                "Update" => "Edited",
                "Remove" => "Deleted",
                _ => null
            };

            if (entityAction == null)
                return;

            try
            {
                var scopeFactory = context.HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                using var scope = scopeFactory.CreateScope();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                string roleName = notificationService.ResolveRoleFromControllerName(controllerName);

                if (roleName == null)
                    return;

                // Identify the user who performed the action so they always
                // see their own notification, even without the matching role.
                string performingUserId = context.HttpContext.User?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;

                string message = $"{roleName} record has been {entityAction.ToLower()}.";

                await notificationService.AddNotificationAsync(
                    message: message,
                    targetUserId: performingUserId,
                    targetRole: roleName,
                    entityName: roleName,
                    entityAction: entityAction
                );

                // When a user is created via the admin panel, also send a welcome notification
                if (string.Equals(controllerName, "User", StringComparison.OrdinalIgnoreCase)
                    && entityAction == "Added"
                    && executedContext.Result is OkObjectResult okResult
                    && okResult.Value is UserProfile userProfile
                    && !string.IsNullOrEmpty(userProfile.ApplicationUserId))
                {
                    await notificationService.AddNotificationAsync(
                        message: "Congratulations on joining us in Inventory Management",
                        targetUserId: userProfile.ApplicationUserId
                    );
                }
            }
            catch (Exception ex)
            {
                var logger = context.HttpContext.RequestServices.GetService<ILogger<NotificationActionFilter>>();
                logger?.LogError(ex, "Failed to create notification for {Controller}.{Action}", controllerName, actionName);
            }
        }
    }
}
