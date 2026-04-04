using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the SalesTypeController class.
    /// </summary>
    [TestClass]
    public class SalesTypeControllerTests
    {
        /// <summary>
        /// Tests that Index returns a non-null IActionResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsNonNullResult()
        {
            // Arrange
            var controller = new SalesTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that Index returns a ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new SalesTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that Index returns a ViewResult with default view name (null).
        /// This verifies that the default view resolution is used.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithDefaultViewName()
        {
            // Arrange
            var controller = new SalesTypeController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}