using System;
using System.Linq;
using System.Reflection;
using coderush.Controllers;
using coderush.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the DashboardController class.
    /// </summary>
    [TestClass]
    public class DashboardControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult when called.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new DashboardController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult with null ViewName (default behavior).
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNullViewName()
        {
            // Arrange
            var controller = new DashboardController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }

        /// <summary>
        /// Tests that the Index method has the Authorize attribute with the correct role.
        /// </summary>
        [TestMethod]
        public void Index_HasAuthorizeAttribute_WithCorrectRole()
        {
            // Arrange
            var methodInfo = typeof(DashboardController).GetMethod("Index");

            // Act
            var authorizeAttribute = methodInfo?.GetCustomAttribute<AuthorizeAttribute>();

            // Assert
            Assert.IsNotNull(authorizeAttribute);
            Assert.AreEqual(MainMenu.Dashboard.RoleName, authorizeAttribute.Roles);
        }
    }
}