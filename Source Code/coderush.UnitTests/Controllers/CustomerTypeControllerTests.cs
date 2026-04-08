using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the CustomerTypeController class.
    /// </summary>
    [TestClass]
    public class CustomerTypeControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult.
        /// Verifies that the action method successfully returns a view.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new CustomerTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result, "Index should return a non-null result.");
            Assert.IsInstanceOfType(result, typeof(ViewResult), "Index should return a ViewResult.");
        }
    }
}