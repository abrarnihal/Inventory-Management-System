using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for PaymentTypeController class.
    /// </summary>
    [TestClass]
    public class PaymentTypeControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a ViewResult.
        /// Input: None (parameterless method).
        /// Expected: Returns a non-null ViewResult with no explicit view name.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new PaymentTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = result as ViewResult;
            Assert.IsNull(viewResult.ViewName);
        }
    }
}