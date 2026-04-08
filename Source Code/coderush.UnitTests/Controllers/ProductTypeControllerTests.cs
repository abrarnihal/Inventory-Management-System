using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="ProductTypeController"/> class.
    /// </summary>
    [TestClass]
    public class ProductTypeControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a non-null ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new ProductTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult with default view name (null).
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithDefaultViewName()
        {
            // Arrange
            var controller = new ProductTypeController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult with null view data model.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNullModel()
        {
            // Arrange
            var controller = new ProductTypeController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewData.Model);
        }
    }
}