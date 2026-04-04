using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="InvoiceTypeController"/> class.
    /// </summary>
    [TestClass]
    public class InvoiceTypeControllerTests
    {
        /// <summary>
        /// Tests that Index method returns a non-null IActionResult.
        /// This verifies that the method executes successfully and returns a valid result.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsNonNullResult()
        {
            // Arrange
            var controller = new InvoiceTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult.
        /// This verifies that the method returns the expected type of action result
        /// for rendering the Index view.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new InvoiceTypeController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that Index method returns a ViewResult with null view name.
        /// This verifies that the default view (Index) is used when no specific view name is provided.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNullViewName()
        {
            // Arrange
            var controller = new InvoiceTypeController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}