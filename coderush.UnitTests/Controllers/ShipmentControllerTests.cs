using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="ShipmentController"/> class.
    /// </summary>
    [TestClass]
    public class ShipmentControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a non-null IActionResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsNonNullResult()
        {
            // Arrange
            var controller = new ShipmentController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new ShipmentController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult with no explicit view name,
        /// indicating it should render the default view for the action.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithDefaultViewName()
        {
            // Arrange
            var controller = new ShipmentController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}