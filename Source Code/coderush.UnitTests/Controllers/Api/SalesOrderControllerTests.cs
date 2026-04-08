using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using coderush.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the SalesOrderController class.
    /// </summary>
    [TestClass]
    public class SalesOrderControllerTests
    {
        /// <summary>
        /// Tests that Update method with valid payload successfully updates the sales order,
        /// saves changes to the database, and returns OkObjectResult with the sales order.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkObjectResultWithSalesOrder()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 1,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1,
                Amount = 1000.0,
                SubTotal = 900.0,
                Discount = 50.0,
                Tax = 100.0,
                Freight = 50.0,
                Total = 1000.0
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                action = "update",
                value = salesOrder
            };
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SalesOrder>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(salesOrder, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesOrder), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles sales order with minimum integer ID value correctly.
        /// </summary>
        [TestMethod]
        public void Update_SalesOrderWithMinIntId_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var salesOrder = new SalesOrder
            {
                SalesOrderId = int.MinValue,
                SalesOrderName = "SO-MIN"
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SalesOrder>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(salesOrder, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles sales order with maximum integer ID value correctly.
        /// </summary>
        [TestMethod]
        public void Update_SalesOrderWithMaxIntId_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var salesOrder = new SalesOrder
            {
                SalesOrderId = int.MaxValue,
                SalesOrderName = "SO-MAX"
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SalesOrder>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(salesOrder, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles sales order with zero ID value correctly.
        /// </summary>
        [TestMethod]
        public void Update_SalesOrderWithZeroId_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 0,
                SalesOrderName = "SO-ZERO"
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SalesOrder>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(salesOrder, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles sales order with negative amount values correctly.
        /// </summary>
        [TestMethod]
        public void Update_SalesOrderWithNegativeAmounts_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 1,
                Amount = -1000.0,
                SubTotal = -900.0,
                Discount = -50.0,
                Tax = -100.0,
                Freight = -50.0,
                Total = -1000.0
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SalesOrder>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(salesOrder, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method handles sales order with special double values (NaN, Infinity) correctly.
        /// </summary>
        [TestMethod]
        [DataRow(double.NaN, DisplayName = "NaN")]
        [DataRow(double.PositiveInfinity, DisplayName = "PositiveInfinity")]
        [DataRow(double.NegativeInfinity, DisplayName = "NegativeInfinity")]
        public void Update_SalesOrderWithSpecialDoubleValues_ReturnsOkObjectResult(double specialValue)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 1,
                Amount = specialValue,
                Total = specialValue
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SalesOrder>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(salesOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Update method returns zero when SaveChanges affects no rows.
        /// </summary>
        [TestMethod]
        public void Update_SaveChangesReturnsZero_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 1,
                SalesOrderName = "SO-001"
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<SalesOrder>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that GetById returns OkObjectResult when a valid id is provided and the sales order exists.
        /// Input: Valid positive id.
        /// Expected: Returns OkObjectResult containing the SalesOrder with SalesOrderLines included.
        /// </summary>
        [TestMethod]
        public async Task GetById_ValidIdWithExistingRecord_ReturnsOkResultWithSalesOrder()
        {
            // Arrange
            var salesOrderId = 1;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetById_ValidId_" + Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var salesOrder = new SalesOrder
            {
                SalesOrderId = salesOrderId,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.Now,
                DeliveryDate = DateTimeOffset.Now.AddDays(7),
                CurrencyId = 1,
                SalesTypeId = 1,
                Amount = 1000.0,
                Total = 1000.0
            };
            context.SalesOrder.Add(salesOrder);
            context.SalesOrderLine.Add(new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = salesOrderId
            });
            await context.SaveChangesAsync();
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new SalesOrderController(context, mockNumberSequence.Object);
            // Act
            var result = await controller.GetById(salesOrderId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult.Value);
            var returnedOrder = okResult.Value as SalesOrder;
            Assert.IsNotNull(returnedOrder);
            Assert.AreEqual(salesOrderId, returnedOrder.SalesOrderId);
            Assert.AreEqual("SO-001", returnedOrder.SalesOrderName);
            Assert.IsNotNull(returnedOrder.SalesOrderLines);
            Assert.AreEqual(1, returnedOrder.SalesOrderLines.Count);
        }

        /// <summary>
        /// Tests that GetById returns OkObjectResult with null when the id does not exist in the database.
        /// Input: Valid id that doesn't match any existing record.
        /// Expected: Returns OkObjectResult with null value.
        /// </summary>
        [TestMethod]
        public async Task GetById_ValidIdWithNoMatchingRecord_ReturnsOkResultWithNull()
        {
            // Arrange
            var nonExistentId = 9999;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetById_NoMatch_" + Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.SalesOrder.Add(new SalesOrder
            {
                SalesOrderId = 1,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1
            });
            await context.SaveChangesAsync();
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new SalesOrderController(context, mockNumberSequence.Object);
            // Act
            var result = await controller.GetById(nonExistentId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that GetById handles boundary value zero correctly.
        /// Input: id = 0 (boundary value).
        /// Expected: Returns OkObjectResult, likely with null since 0 is typically not a valid database id.
        /// </summary>
        /// <remarks>
        /// Note: This test requires mocking Entity Framework Core's async query operations.
        /// See remarks in GetById_ValidIdWithExistingRecord_ReturnsOkResultWithSalesOrder for details.
        /// </remarks>
        [TestMethod]
        public async Task GetById_IdZero_ReturnsOkResult()
        {
            // Arrange
            var zeroId = 0;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetById_IdZero_" + Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.SalesOrder.Add(new SalesOrder
            {
                SalesOrderId = 1,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1
            });
            await context.SaveChangesAsync();
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new SalesOrderController(context, mockNumberSequence.Object);
            // Act
            var result = await controller.GetById(zeroId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that GetById handles negative id values correctly.
        /// Input: Negative id value.
        /// Expected: Returns OkObjectResult with null since negative ids are not valid.
        /// </summary>
        /// <remarks>
        /// Note: This test requires mocking Entity Framework Core's async query operations.
        /// See remarks in GetById_ValidIdWithExistingRecord_ReturnsOkResultWithSalesOrder for details.
        /// </remarks>
        [TestMethod]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        public async Task GetById_NegativeId_ReturnsOkResultWithNull(int negativeId)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetById_NegativeId_" + Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.SalesOrder.Add(new SalesOrder
            {
                SalesOrderId = 1,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1
            });
            await context.SaveChangesAsync();
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new SalesOrderController(context, mockNumberSequence.Object);
            // Act
            var result = await controller.GetById(negativeId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that GetById handles maximum integer value correctly.
        /// Input: int.MaxValue as id.
        /// Expected: Returns OkObjectResult, likely with null since such a large id is unlikely to exist.
        /// </summary>
        /// <remarks>
        /// Note: This test requires mocking Entity Framework Core's async query operations.
        /// See remarks in GetById_ValidIdWithExistingRecord_ReturnsOkResultWithSalesOrder for details.
        /// </remarks>
        [TestMethod]
        public async Task GetById_MaxIntValue_ReturnsOkResult()
        {
            // Arrange
            var maxId = int.MaxValue;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetById_MaxInt_" + Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            context.SalesOrder.Add(new SalesOrder
            {
                SalesOrderId = 1,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1
            });
            await context.SaveChangesAsync();
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new SalesOrderController(context, mockNumberSequence.Object);
            // Act
            var result = await controller.GetById(maxId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that GetById properly includes SalesOrderLines navigation property when retrieving a sales order.
        /// Input: Valid id with an existing record that has associated sales order lines.
        /// Expected: Returns OkObjectResult with SalesOrder containing populated SalesOrderLines collection.
        /// </summary>
        /// <remarks>
        /// Note: This test requires mocking Entity Framework Core's async query operations including the Include operation.
        /// See remarks in GetById_ValidIdWithExistingRecord_ReturnsOkResultWithSalesOrder for details.
        /// </remarks>
        [TestMethod]
        public async Task GetById_ValidId_IncludesSalesOrderLines()
        {
            // Arrange
            var salesOrderId = 5;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_GetById_IncludesLines_" + Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var salesOrder = new SalesOrder
            {
                SalesOrderId = salesOrderId,
                SalesOrderName = "SO-005",
                BranchId = 1,
                CustomerId = 1
            };
            context.SalesOrder.Add(salesOrder);
            context.SalesOrderLine.Add(new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = 2,
                Price = 100.0,
                Amount = 200.0
            });
            context.SalesOrderLine.Add(new SalesOrderLine
            {
                SalesOrderLineId = 2,
                SalesOrderId = salesOrderId,
                ProductId = 2,
                Quantity = 1,
                Price = 50.0,
                Amount = 50.0
            });
            await context.SaveChangesAsync();
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new SalesOrderController(context, mockNumberSequence.Object);
            // Act
            var result = await controller.GetById(salesOrderId);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult.Value);
            var returnedOrder = okResult.Value as SalesOrder;
            Assert.IsNotNull(returnedOrder);
            Assert.AreEqual(salesOrderId, returnedOrder.SalesOrderId);
            Assert.IsNotNull(returnedOrder.SalesOrderLines);
            Assert.AreEqual(2, returnedOrder.SalesOrderLines.Count);
        }

        /// <summary>
        /// Tests that Insert method returns OkObjectResult with the sales order when valid payload is provided.
        /// Input: Valid CrudViewModel with SalesOrder value.
        /// Expected: OkObjectResult containing the sales order with SalesOrderName set.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkObjectResultWithSalesOrder()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockSalesOrderLineDbSet = new Mock<DbSet<SalesOrderLine>>();
            var expectedOrderNumber = "SO-2024-001";
            mockNumberSequence.Setup(ns => ns.GetNumberSequence("SO")).Returns(expectedOrderNumber);
            var salesOrderList = new List<SalesOrder>();
            var salesOrderLineList = new List<SalesOrderLine>();
            mockDbSet.Setup(m => m.Add(It.IsAny<SalesOrder>())).Callback<SalesOrder>(s => salesOrderList.Add(s));
            var salesOrderQueryable = salesOrderList.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderQueryable.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderQueryable.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderQueryable.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderList.GetEnumerator());
            var salesOrderLineQueryable = salesOrderLineList.AsQueryable();
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Provider).Returns(salesOrderLineQueryable.Provider);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Expression).Returns(salesOrderLineQueryable.Expression);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.ElementType).Returns(salesOrderLineQueryable.ElementType);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderLineList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 1,
                BranchId = 1,
                CustomerId = 1,
                OrderDate = DateTimeOffset.Now,
                DeliveryDate = DateTimeOffset.Now.AddDays(7),
                CurrencyId = 1,
                SalesTypeId = 1,
                Amount = 1000.0,
                SubTotal = 1000.0,
                Discount = 0.0,
                Tax = 100.0,
                Freight = 50.0,
                Total = 1150.0
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(salesOrder, okResult.Value);
            Assert.AreEqual(expectedOrderNumber, salesOrder.SalesOrderName);
            mockNumberSequence.Verify(ns => ns.GetNumberSequence("SO"), Times.Once);
            mockDbSet.Verify(m => m.Add(It.IsAny<SalesOrder>()), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that Insert method correctly sets SalesOrderName from number sequence service.
        /// Input: Valid payload with SalesOrder that has no SalesOrderName set.
        /// Expected: SalesOrderName is set to the value returned by GetNumberSequence.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_SetsSalesOrderNameFromNumberSequence()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SetsSalesOrderName_" + Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            var expectedOrderNumber = "SO-12345";
            mockNumberSequence.Setup(ns => ns.GetNumberSequence("SO")).Returns(expectedOrderNumber);
            var controller = new SalesOrderController(context, mockNumberSequence.Object);
            var salesOrder = new SalesOrder
            {
                BranchId = 2,
                CustomerId = 3
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            // Act
            controller.Insert(payload);
            // Assert
            Assert.AreEqual(expectedOrderNumber, salesOrder.SalesOrderName);
        }

        /// <summary>
        /// Tests that Insert method calls Add on DbSet with the sales order.
        /// Input: Valid payload with SalesOrder.
        /// Expected: DbSet.Add is called exactly once with the sales order.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsAddOnDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockSalesOrderLineDbSet = new Mock<DbSet<SalesOrderLine>>();
            mockNumberSequence.Setup(ns => ns.GetNumberSequence("SO")).Returns("SO-001");
            var salesOrderList = new List<SalesOrder>();
            var salesOrderLineList = new List<SalesOrderLine>();
            mockDbSet.Setup(m => m.Add(It.IsAny<SalesOrder>())).Callback<SalesOrder>(s => salesOrderList.Add(s));
            var salesOrderQueryable = salesOrderList.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderQueryable.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderQueryable.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderQueryable.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderList.GetEnumerator());
            var salesOrderLineQueryable = salesOrderLineList.AsQueryable();
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Provider).Returns(salesOrderLineQueryable.Provider);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Expression).Returns(salesOrderLineQueryable.Expression);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.ElementType).Returns(salesOrderLineQueryable.ElementType);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderLineList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineDbSet.Object);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var salesOrder = new SalesOrder
            {
                CustomerId = 1
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            // Act
            controller.Insert(payload);
            // Assert
            mockDbSet.Verify(m => m.Add(salesOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method calls SaveChanges on the context.
        /// Input: Valid payload with SalesOrder.
        /// Expected: Context.SaveChanges is called at least once.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockSalesOrderLineDbSet = new Mock<DbSet<SalesOrderLine>>();
            mockNumberSequence.Setup(ns => ns.GetNumberSequence("SO")).Returns("SO-001");
            var salesOrderList = new List<SalesOrder>();
            var salesOrderLineList = new List<SalesOrderLine>();
            mockDbSet.Setup(m => m.Add(It.IsAny<SalesOrder>())).Callback<SalesOrder>(s => salesOrderList.Add(s));
            var salesOrderQueryable = salesOrderList.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderQueryable.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderQueryable.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderQueryable.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var salesOrderLineQueryable = salesOrderLineList.AsQueryable();
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Provider).Returns(salesOrderLineQueryable.Provider);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Expression).Returns(salesOrderLineQueryable.Expression);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.ElementType).Returns(salesOrderLineQueryable.ElementType);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderLineList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineDbSet.Object);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var salesOrder = new SalesOrder
            {
                CustomerId = 1
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            // Act
            controller.Insert(payload);
            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that Insert method handles empty string returned by GetNumberSequence.
        /// Input: Valid payload, GetNumberSequence returns empty string.
        /// Expected: SalesOrderName is set to empty string.
        /// </summary>
        [TestMethod]
        public void Insert_GetNumberSequenceReturnsEmpty_SetsSalesOrderNameToEmpty()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockSalesOrderLineDbSet = new Mock<DbSet<SalesOrderLine>>();
            mockNumberSequence.Setup(ns => ns.GetNumberSequence("SO")).Returns(string.Empty);
            var salesOrderList = new List<SalesOrder>();
            var salesOrderLineList = new List<SalesOrderLine>();
            mockDbSet.Setup(m => m.Add(It.IsAny<SalesOrder>())).Callback<SalesOrder>(s => salesOrderList.Add(s));
            var salesOrderQueryable = salesOrderList.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderQueryable.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderQueryable.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderQueryable.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderList.GetEnumerator());
            var salesOrderLineQueryable = salesOrderLineList.AsQueryable();
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Provider).Returns(salesOrderLineQueryable.Provider);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Expression).Returns(salesOrderLineQueryable.Expression);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.ElementType).Returns(salesOrderLineQueryable.ElementType);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderLineList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var salesOrder = new SalesOrder
            {
                CustomerId = 1
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            // Act
            controller.Insert(payload);
            // Assert
            Assert.AreEqual(string.Empty, salesOrder.SalesOrderName);
        }

        /// <summary>
        /// Tests that Insert method handles null returned by GetNumberSequence.
        /// Input: Valid payload, GetNumberSequence returns null.
        /// Expected: SalesOrderName is set to null.
        /// </summary>
        [TestMethod]
        public void Insert_GetNumberSequenceReturnsNull_SetsSalesOrderNameToNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var mockSalesOrderLineDbSet = new Mock<DbSet<SalesOrderLine>>();
            mockNumberSequence.Setup(ns => ns.GetNumberSequence("SO")).Returns((string)null!);
            var salesOrderList = new List<SalesOrder>();
            var salesOrderLineList = new List<SalesOrderLine>();
            mockDbSet.Setup(m => m.Add(It.IsAny<SalesOrder>())).Callback<SalesOrder>(s => salesOrderList.Add(s));
            var salesOrderQueryable = salesOrderList.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderQueryable.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderQueryable.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderQueryable.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderList.GetEnumerator());
            var salesOrderLineQueryable = salesOrderLineList.AsQueryable();
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Provider).Returns(salesOrderLineQueryable.Provider);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Expression).Returns(salesOrderLineQueryable.Expression);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.ElementType).Returns(salesOrderLineQueryable.ElementType);
            mockSalesOrderLineDbSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.GetEnumerator()).Returns(() => salesOrderLineList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineDbSet.Object);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var salesOrder = new SalesOrder
            {
                CustomerId = 1
            };
            var payload = new CrudViewModel<SalesOrder>
            {
                value = salesOrder
            };
            // Act
            controller.Insert(payload);
            // Assert
            Assert.IsNull(salesOrder.SalesOrderName);
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing SalesOrder and returns OkObjectResult.
        /// Input: Valid payload with key matching an existing SalesOrder.
        /// Expected: SalesOrder is removed from context, SaveChanges is called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(int.MaxValue)]
        public void Remove_ValidPayloadWithExistingSalesOrder_ReturnsOkResultWithRemovedOrder(int salesOrderId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var existingSalesOrder = new SalesOrder
            {
                SalesOrderId = salesOrderId,
                SalesOrderName = "SO-001",
                BranchId = 1,
                CustomerId = 1
            };
            var salesOrderList = new List<SalesOrder>
            {
                existingSalesOrder
            }.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderList.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderList.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderList.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(salesOrderList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<SalesOrder>
            {
                key = salesOrderId,
                action = "remove"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(existingSalesOrder, okResult.Value);
            mockDbSet.Verify(m => m.Remove(It.IsAny<SalesOrder>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles string key values that can be converted to int.
        /// Input: Payload with string key "123".
        /// Expected: Convert.ToInt32 successfully converts string to int, queries and removes SalesOrder.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithStringKeyValue_ConvertsAndRemoves()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var existingSalesOrder = new SalesOrder
            {
                SalesOrderId = 123,
                SalesOrderName = "SO-123"
            };
            var salesOrderList = new List<SalesOrder>
            {
                existingSalesOrder
            }.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderList.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderList.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderList.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(salesOrderList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<SalesOrder>
            {
                key = "123",
                action = "remove"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(m => m.Remove(It.IsAny<SalesOrder>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove verifies the correct SalesOrder is removed when multiple exist.
        /// Input: Valid payload with key matching one of multiple SalesOrders.
        /// Expected: Only the SalesOrder with matching ID is removed, SaveChanges is called once.
        /// </summary>
        [TestMethod]
        public void Remove_MultipleSalesOrdersExist_RemovesCorrectOne()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<SalesOrder>>();
            var targetSalesOrder = new SalesOrder
            {
                SalesOrderId = 2,
                SalesOrderName = "SO-002"
            };
            var salesOrderList = new List<SalesOrder>
            {
                new SalesOrder
                {
                    SalesOrderId = 1,
                    SalesOrderName = "SO-001"
                },
                targetSalesOrder,
                new SalesOrder
                {
                    SalesOrderId = 3,
                    SalesOrderName = "SO-003"
                }
            }.AsQueryable();
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrderList.Provider);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrderList.Expression);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrderList.ElementType);
            mockDbSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(salesOrderList.GetEnumerator());
            mockContext.Setup(c => c.SalesOrder).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<SalesOrder>
            {
                key = 2,
                action = "remove"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(targetSalesOrder, okResult.Value);
            mockDbSet.Verify(m => m.Remove(targetSalesOrder), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }
    }
}