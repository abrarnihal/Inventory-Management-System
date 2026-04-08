using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="PurchaseTypeController"/> class.
    /// </summary>
    [TestClass]
    public class PurchaseTypeControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a non-null ViewResult.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new PurchaseTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult with default view name (null or empty).
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithDefaultViewName()
        {
            // Arrange
            var controller = new PurchaseTypeController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(string.IsNullOrEmpty(result.ViewName));
        }

        /// <summary>
        /// Tests that Index method executes without throwing any exceptions.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_DoesNotThrowException()
        {
            // Arrange
            var controller = new PurchaseTypeController();

            // Act & Assert
            try
            {
                var result = controller.Index();
                Assert.IsNotNull(result);
            }
            catch (System.Exception ex)
            {
                Assert.Fail($"Expected no exception, but got: {ex.Message}");
            }
        }
    }
}