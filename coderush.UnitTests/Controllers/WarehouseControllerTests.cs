using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the WarehouseController class.
    /// </summary>
    [TestClass]
    public class WarehouseControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a non-null ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new WarehouseController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult with no view name specified (uses default).
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNoViewName()
        {
            // Arrange
            var controller = new WarehouseController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}