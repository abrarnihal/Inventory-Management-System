using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the ProductController class.
    /// </summary>
    [TestClass]
    public class ProductControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a non-null ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new ProductController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult with no explicit view name,
        /// indicating it should use the default view.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithDefaultViewName()
        {
            // Arrange
            var controller = new ProductController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}