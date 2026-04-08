using System;
using System.Collections.Generic;
using System.Linq;

using coderush.Controllers;
using coderush.Data;
using coderush.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.UnitTests
{
    /// <summary>
    /// Unit tests for the SalesOrderController class.
    /// </summary>
    [TestClass]
    public class SalesOrderControllerTests
    {
        /// <summary>
        /// Tests that the Index method returns a ViewResult.
        /// Verifies that the action returns a valid ViewResult instance when invoked.
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            SalesOrderController controller = new SalesOrderController(mockContext.Object);

            // Act
            IActionResult result = controller.Index();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
        }

        /// <summary>
        /// Tests that the Index method returns a ViewResult with a null ViewName.
        /// Verifies that the default view is used (ViewName should be null for default view).
        /// </summary>
        [TestMethod]
        public void Index_WhenCalled_ReturnsViewResultWithNullViewName()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            SalesOrderController controller = new SalesOrderController(mockContext.Object);

            // Act
            IActionResult result = controller.Index();

            // Assert
            ViewResult? viewResult = result as ViewResult;
            Assert.IsNotNull(viewResult);
            Assert.IsNull(viewResult.ViewName);
        }

        /// <summary>
        /// Tests that Detail returns a ViewResult with the correct SalesOrder model
        /// when a valid existing ID is provided.
        /// </summary>
        [TestMethod]
        public void Detail_ValidIdWithExistingSalesOrder_ReturnsViewResultWithModel()
        {
            // Arrange
            int testId = 1;
            SalesOrder expectedSalesOrder = new SalesOrder
            {
                SalesOrderId = testId,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.Now,
                DeliveryDate = DateTimeOffset.Now.AddDays(7)
            };

            List<SalesOrder> salesOrders = new List<SalesOrder> { expectedSalesOrder };
            IQueryable<SalesOrder> queryableSalesOrders = salesOrders.AsQueryable();

            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>> mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>>();

            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(queryableSalesOrders.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(queryableSalesOrders.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(queryableSalesOrders.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(queryableSalesOrders.GetEnumerator());

            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);

            SalesOrderController controller = new SalesOrderController(mockContext.Object);

            // Act
            IActionResult result = controller.Detail(testId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            Assert.IsInstanceOfType(viewResult.Model, typeof(SalesOrder));
            SalesOrder actualSalesOrder = (SalesOrder)viewResult.Model;
            Assert.AreEqual(expectedSalesOrder.SalesOrderId, actualSalesOrder.SalesOrderId);
            Assert.AreEqual(expectedSalesOrder.SalesOrderName, actualSalesOrder.SalesOrderName);
        }

        /// <summary>
        /// Tests that Detail returns NotFoundResult when the provided ID does not exist in the database.
        /// </summary>
        [TestMethod]
        public void Detail_IdNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            int nonExistentId = 999;
            List<SalesOrder> salesOrders = new List<SalesOrder>();
            IQueryable<SalesOrder> queryableSalesOrders = salesOrders.AsQueryable();

            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>> mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>>();

            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(queryableSalesOrders.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(queryableSalesOrders.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(queryableSalesOrders.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(queryableSalesOrders.GetEnumerator());

            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);

            SalesOrderController controller = new SalesOrderController(mockContext.Object);

            // Act
            IActionResult result = controller.Detail(nonExistentId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that Detail returns NotFoundResult for various edge case ID values
        /// including zero, negative values, int.MinValue, and int.MaxValue.
        /// </summary>
        /// <param name="id">The edge case ID to test.</param>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void Detail_EdgeCaseIds_ReturnsNotFoundResult(int id)
        {
            // Arrange
            List<SalesOrder> salesOrders = new List<SalesOrder>();
            IQueryable<SalesOrder> queryableSalesOrders = salesOrders.AsQueryable();

            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>> mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>>();

            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(queryableSalesOrders.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(queryableSalesOrders.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(queryableSalesOrders.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(queryableSalesOrders.GetEnumerator());

            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);

            SalesOrderController controller = new SalesOrderController(mockContext.Object);

            // Act
            IActionResult result = controller.Detail(id);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        }

        /// <summary>
        /// Tests that Detail returns ViewResult with correct model when ID exists
        /// among multiple SalesOrders in the database.
        /// </summary>
        [TestMethod]
        public void Detail_IdExistsAmongMultipleRecords_ReturnsCorrectViewResult()
        {
            // Arrange
            int targetId = 2;
            List<SalesOrder> salesOrders = new List<SalesOrder>
            {
                new SalesOrder { SalesOrderId = 1, SalesOrderName = "SO-001", CustomerId = 1 },
                new SalesOrder { SalesOrderId = 2, SalesOrderName = "SO-002", CustomerId = 2 },
                new SalesOrder { SalesOrderId = 3, SalesOrderName = "SO-003", CustomerId = 3 }
            };
            IQueryable<SalesOrder> queryableSalesOrders = salesOrders.AsQueryable();

            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>> mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<SalesOrder>>();

            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(queryableSalesOrders.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(queryableSalesOrders.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(queryableSalesOrders.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(queryableSalesOrders.GetEnumerator());

            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);

            SalesOrderController controller = new SalesOrderController(mockContext.Object);

            // Act
            IActionResult result = controller.Detail(targetId);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(ViewResult));
            ViewResult viewResult = (ViewResult)result;
            Assert.IsNotNull(viewResult.Model);
            SalesOrder actualSalesOrder = (SalesOrder)viewResult.Model;
            Assert.AreEqual(targetId, actualSalesOrder.SalesOrderId);
            Assert.AreEqual("SO-002", actualSalesOrder.SalesOrderName);
            Assert.AreEqual(2, actualSalesOrder.CustomerId);
        }
    }
}