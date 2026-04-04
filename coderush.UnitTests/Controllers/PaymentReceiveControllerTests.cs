using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="PaymentReceiveController"/> class.
    /// </summary>
    [TestClass]
    public class PaymentReceiveControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a non-null IActionResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsNonNullResult()
        {
            // Arrange
            var controller = new PaymentReceiveController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new PaymentReceiveController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult with null ViewName,
        /// indicating the default view name convention will be used.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNullViewName()
        {
            // Arrange
            var controller = new PaymentReceiveController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}