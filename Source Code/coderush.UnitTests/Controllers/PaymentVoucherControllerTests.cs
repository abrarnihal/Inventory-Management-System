using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the PaymentVoucherController class.
    /// </summary>
    [TestClass]
    public class PaymentVoucherControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a non-null ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new PaymentVoucherController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<ViewResult>(result);
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult with no specific view name set (defaults to action name).
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNullViewName()
        {
            // Arrange
            var controller = new PaymentVoucherController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}