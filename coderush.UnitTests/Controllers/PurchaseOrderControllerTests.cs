using System.Collections.Generic;
using System.Linq;

using coderush.Controllers;
using coderush.Data;
using coderush.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.UnitTests
{
    [TestClass]
    public class PurchaseOrderControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult when called.
        /// This test verifies the basic functionality of the Index action method,
        /// ensuring it returns the expected view result for rendering the index page.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            // Note: ApplicationDbContext typically requires DbContextOptions in its constructor.
            // Creating a mock with in-memory options to satisfy the constructor requirement.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            var context = new ApplicationDbContext(options);
            var controller = new PurchaseOrderController(context);

            // Act
            var result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult with null ViewName,
        /// indicating the default view should be used.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithDefaultViewName()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase2")
                .Options;
            var context = new ApplicationDbContext(options);
            var controller = new PurchaseOrderController(context);

            // Act
            var result = controller.Index() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.ViewName);
        }

        /// <summary>
        /// Tests that Detail returns ViewResult with correct purchase order when valid ID exists.
        /// Input: Valid purchase order ID (1) that exists in the database.
        /// Expected: ViewResult containing the matching PurchaseOrder.
        /// </summary>
        [TestMethod]
        public void Detail_ValidIdExists_ReturnsViewResultWithPurchaseOrder()
        {
            // Arrange
            var purchaseOrderId = 1;
            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = purchaseOrderId, PurchaseOrderName = "PO-001" };
            var data = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();

            var mockSet = new Mock<DbSet<PurchaseOrder>>();
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockSet.Object);

            var controller = new PurchaseOrderController(mockContext.Object);

            // Act
            var result = controller.Detail(purchaseOrderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(PurchaseOrder));
            var resultModel = (PurchaseOrder)viewResult.Model;
            Assert.AreEqual(purchaseOrderId, resultModel.PurchaseOrderId);
        }

        /// <summary>
        /// Tests that Detail returns NotFoundResult when purchase order with given ID does not exist.
        /// Input: ID (999) that does not exist in the database.
        /// Expected: NotFoundResult (404 response).
        /// </summary>
        [TestMethod]
        public void Detail_IdDoesNotExist_ReturnsNotFoundResult()
        {
            // Arrange
            var nonExistentId = 999;
            var data = new List<PurchaseOrder>().AsQueryable();

            var mockSet = new Mock<DbSet<PurchaseOrder>>();
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockSet.Object);

            var controller = new PurchaseOrderController(mockContext.Object);

            // Act
            var result = controller.Detail(nonExistentId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests Detail with various boundary and edge case ID values.
        /// Input: Zero, negative, int.MinValue, and int.MaxValue IDs that don't exist.
        /// Expected: NotFoundResult for all non-existent IDs.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void Detail_BoundaryIdValues_ReturnsNotFoundResult(int id)
        {
            // Arrange
            var data = new List<PurchaseOrder>().AsQueryable();

            var mockSet = new Mock<DbSet<PurchaseOrder>>();
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockSet.Object);

            var controller = new PurchaseOrderController(mockContext.Object);

            // Act
            var result = controller.Detail(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests Detail with extreme boundary values that exist in the database.
        /// Input: int.MaxValue ID that exists in the database.
        /// Expected: ViewResult containing the matching PurchaseOrder.
        /// </summary>
        [TestMethod]
        public void Detail_MaxIntIdExists_ReturnsViewResultWithPurchaseOrder()
        {
            // Arrange
            var purchaseOrderId = int.MaxValue;
            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = purchaseOrderId, PurchaseOrderName = "PO-MAX" };
            var data = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();

            var mockSet = new Mock<DbSet<PurchaseOrder>>();
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockSet.Object);

            var controller = new PurchaseOrderController(mockContext.Object);

            // Act
            var result = controller.Detail(purchaseOrderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            var resultModel = (PurchaseOrder)viewResult.Model;
            Assert.AreEqual(purchaseOrderId, resultModel.PurchaseOrderId);
        }

        /// <summary>
        /// Tests Detail when multiple purchase orders exist but only one matches the ID.
        /// Input: Valid ID (2) among multiple purchase orders.
        /// Expected: ViewResult containing only the purchase order with matching ID.
        /// </summary>
        [TestMethod]
        public void Detail_MultipleRecordsOneMatches_ReturnsCorrectPurchaseOrder()
        {
            // Arrange
            var targetId = 2;
            var purchaseOrders = new List<PurchaseOrder>
            {
                new PurchaseOrder { PurchaseOrderId = 1, PurchaseOrderName = "PO-001" },
                new PurchaseOrder { PurchaseOrderId = targetId, PurchaseOrderName = "PO-002" },
                new PurchaseOrder { PurchaseOrderId = 3, PurchaseOrderName = "PO-003" }
            };
            var data = purchaseOrders.AsQueryable();

            var mockSet = new Mock<DbSet<PurchaseOrder>>();
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockSet.Object);

            var controller = new PurchaseOrderController(mockContext.Object);

            // Act
            var result = controller.Detail(targetId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            var resultModel = (PurchaseOrder)viewResult.Model;
            Assert.AreEqual(targetId, resultModel.PurchaseOrderId);
            Assert.AreEqual("PO-002", resultModel.PurchaseOrderName);
        }

        /// <summary>
        /// Tests Detail with negative ID when a matching negative ID exists.
        /// Input: Negative ID (-5) that exists in the database.
        /// Expected: ViewResult containing the matching PurchaseOrder.
        /// </summary>
        [TestMethod]
        public void Detail_NegativeIdExists_ReturnsViewResultWithPurchaseOrder()
        {
            // Arrange
            var purchaseOrderId = -5;
            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = purchaseOrderId, PurchaseOrderName = "PO-NEG" };
            var data = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();

            var mockSet = new Mock<DbSet<PurchaseOrder>>();
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockSet.Object);

            var controller = new PurchaseOrderController(mockContext.Object);

            // Act
            var result = controller.Detail(purchaseOrderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            var resultModel = (PurchaseOrder)viewResult.Model;
            Assert.AreEqual(purchaseOrderId, resultModel.PurchaseOrderId);
        }

        /// <summary>
        /// Tests Detail with zero ID when a matching zero ID exists.
        /// Input: Zero ID that exists in the database.
        /// Expected: ViewResult containing the matching PurchaseOrder.
        /// </summary>
        [TestMethod]
        public void Detail_ZeroIdExists_ReturnsViewResultWithPurchaseOrder()
        {
            // Arrange
            var purchaseOrderId = 0;
            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = purchaseOrderId, PurchaseOrderName = "PO-ZERO" };
            var data = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();

            var mockSet = new Mock<DbSet<PurchaseOrder>>();
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockSet.Object);

            var controller = new PurchaseOrderController(mockContext.Object);

            // Act
            var result = controller.Detail(purchaseOrderId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            var viewResult = (ViewResult)result;
            var resultModel = (PurchaseOrder)viewResult.Model;
            Assert.AreEqual(purchaseOrderId, resultModel.PurchaseOrderId);
        }
    }
}