using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="GoodsReceivedNoteController"/> class.
    /// </summary>
    [TestClass]
    public class GoodsReceivedNoteControllerTests
    {
        /// <summary>
        /// Tests that Index returns a ViewResult.
        /// Verifies that the Index action method successfully returns a non-null ViewResult instance.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new GoodsReceivedNoteController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<ViewResult>(result);
        }

        /// <summary>
        /// Tests that Index returns a ViewResult with null view name (uses convention-based routing).
        /// Verifies that the ViewResult uses the default convention-based view name.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNullViewName()
        {
            // Arrange
            var controller = new GoodsReceivedNoteController();

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }
    }
}