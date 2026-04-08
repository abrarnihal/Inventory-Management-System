using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="BillController"/> class.
    /// </summary>
    [TestClass]
    public class BillControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult.
        /// Validates that the action returns the correct result type for rendering the view.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new BillController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult with no explicit view name.
        /// Validates that the default view resolution is used.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNoViewName()
        {
            // Arrange
            var controller = new BillController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}