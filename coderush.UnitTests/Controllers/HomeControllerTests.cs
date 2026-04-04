using System.Diagnostics;

using coderush.Controllers;
using coderush.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Contains unit tests for the <see cref="HomeController"/> class.
    /// </summary>
    [TestClass]
    public class HomeControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a RedirectToActionResult
        /// that redirects to the UserProfile action in the UserRole controller.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsRedirectToUserProfileAction()
        {
            // Arrange
            var controller = new HomeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(RedirectToActionResult));

            var redirectResult = (RedirectToActionResult)result;
            Assert.AreEqual("UserProfile", redirectResult.ActionName);
            Assert.AreEqual("UserRole", redirectResult.ControllerName);
        }

        /// <summary>
        /// Tests that the Error method returns a ViewResult with ErrorViewModel
        /// when Activity.Current is null and uses HttpContext.TraceIdentifier.
        /// </summary>
        [TestMethod]
        public void Error_ActivityCurrentIsNull_ReturnsViewResultWithTraceIdentifier()
        {
            // Arrange
            var controller = new HomeController();
            var mockHttpContext = new Mock<HttpContext>();
            var expectedTraceId = "test-trace-id-12345";
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns(expectedTraceId);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Ensure Activity.Current is null
            Activity.Current = null;

            // Act
            var result = controller.Error();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(ErrorViewModel));
            var model = (ErrorViewModel)viewResult.Model;
            Assert.AreEqual(expectedTraceId, model.RequestId);
        }

        /// <summary>
        /// Tests that the Error method returns a ViewResult with ErrorViewModel
        /// when Activity.Current exists but its Id is null, using HttpContext.TraceIdentifier.
        /// </summary>
        [TestMethod]
        public void Error_ActivityCurrentIdIsNull_ReturnsViewResultWithTraceIdentifier()
        {
            // Arrange
            var controller = new HomeController();
            var mockHttpContext = new Mock<HttpContext>();
            var expectedTraceId = "trace-id-when-activity-id-null";
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns(expectedTraceId);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Create an activity but ensure its Id is null (activity not started)
            var activity = new Activity("test-operation");
            Activity.Current = activity;

            try
            {
                // Act
                var result = controller.Error();

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var viewResult = (ViewResult)result;
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(ErrorViewModel));
                var model = (ErrorViewModel)viewResult.Model;
                Assert.AreEqual(expectedTraceId, model.RequestId);
            }
            finally
            {
                // Cleanup
                Activity.Current = null;
            }
        }

        /// <summary>
        /// Tests that the Error method returns a ViewResult with ErrorViewModel
        /// when Activity.Current exists with a valid Id, using Activity.Current.Id.
        /// </summary>
        [TestMethod]
        public void Error_ActivityCurrentHasId_ReturnsViewResultWithActivityId()
        {
            // Arrange
            var controller = new HomeController();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns("should-not-be-used");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Create and start an activity to ensure it has an Id
            var activity = new Activity("test-operation");
            activity.Start();
            var expectedActivityId = activity.Id;

            try
            {
                // Act
                var result = controller.Error();

                // Assert
                Assert.IsNotNull(result);
                Assert.IsInstanceOfType(result, typeof(ViewResult));
                var viewResult = (ViewResult)result;
                Assert.IsNotNull(viewResult.Model);
                Assert.IsInstanceOfType(viewResult.Model, typeof(ErrorViewModel));
                var model = (ErrorViewModel)viewResult.Model;
                Assert.AreEqual(expectedActivityId, model.RequestId);
            }
            finally
            {
                // Cleanup
                activity.Stop();
                Activity.Current = null;
            }
        }

        /// <summary>
        /// Tests that the Error method handles various TraceIdentifier values correctly
        /// including empty string, whitespace, and special characters when Activity.Current is null.
        /// </summary>
        /// <param name="traceId">The trace identifier to test.</param>
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("normal-trace-id")]
        [DataRow("trace-with-special-chars-!@#$%")]
        [DataRow("very-long-trace-id-" + "abcdefghijklmnopqrstuvwxyz0123456789")]
        public void Error_VariousTraceIdentifierValues_ReturnsViewResultWithCorrectValue(string traceId)
        {
            // Arrange
            var controller = new HomeController();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns(traceId);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };

            // Ensure Activity.Current is null
            Activity.Current = null;

            // Act
            var result = controller.Error();

            // Assert
            Assert.IsNotNull(result);
            var viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            var model = viewResult.Model as ErrorViewModel;
            Assert.IsNotNull(model);
            Assert.AreEqual(traceId, model.RequestId);
        }

        /// <summary>
        /// Tests that the Error method returns a ViewResult (not null)
        /// ensuring it does not throw exceptions under normal conditions.
        /// </summary>
        [TestMethod]
        public void Error_NormalExecution_DoesNotThrowException()
        {
            // Arrange
            var controller = new HomeController();
            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(c => c.TraceIdentifier).Returns("test-trace");
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = mockHttpContext.Object
            };
            Activity.Current = null;

            // Act & Assert
            var result = controller.Error();
            Assert.IsNotNull(result);
        }
    }
}