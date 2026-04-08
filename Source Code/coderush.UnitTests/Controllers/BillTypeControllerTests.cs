using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the BillTypeController class.
    /// </summary>
    [TestClass]
    public class BillTypeControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a ViewResult.
        /// Expected: The method should return a non-null ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new BillTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<ViewResult>(result);
        }

        /// <summary>
        /// Tests that Index method returns an IActionResult.
        /// Expected: The method should return a result that is assignable to IActionResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsIActionResult()
        {
            // Arrange
            var controller = new BillTypeController();

            // Act
            IActionResult result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<IActionResult>(result);
        }
    }
}