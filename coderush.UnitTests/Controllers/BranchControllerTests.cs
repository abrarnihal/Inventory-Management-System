using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the BranchController class.
    /// </summary>
    [TestClass]
    public class BranchControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a ViewResult.
        /// Verifies that the Index action correctly returns a view result for rendering.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new BranchController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult with no specific view name.
        /// Verifies that the default view (matching the action name) will be used.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNoViewName()
        {
            // Arrange
            var controller = new BranchController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}