using coderush.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="UnitOfMeasureController"/> class.
    /// </summary>
    [TestClass]
    public class UnitOfMeasureControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult.
        /// Input: None (no parameters)
        /// Expected: Returns a non-null ViewResult object
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            var controller = new UnitOfMeasureController();

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }
    }
}