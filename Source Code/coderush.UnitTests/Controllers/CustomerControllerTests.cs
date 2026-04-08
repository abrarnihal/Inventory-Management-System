using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the CustomerController class.
    /// </summary>
    [TestClass]
    public class CustomerControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult with the default view.
        /// Input: None (method has no parameters).
        /// Expected: A non-null ViewResult with null view name (indicating default view).
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new CustomerController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<ViewResult>(result);
            var viewResult = (ViewResult)result;
            Assert.IsNull(viewResult.ViewName);
        }
    }
}