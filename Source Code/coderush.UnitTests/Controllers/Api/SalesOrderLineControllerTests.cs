using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Primitives;
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
    /// Unit tests for SalesOrderLineController.
    /// </summary>
    [TestClass]
    public class SalesOrderLineControllerTests
    {
        /// <summary>
        /// Tests that Insert method returns OkObjectResult with recalculated SalesOrderLine when valid payload is provided.
        /// Input: Valid CrudViewModel with SalesOrderLine having Quantity=10, Price=100, DiscountPercentage=10, TaxPercentage=5.
        /// Expected: Returns OkObjectResult with correctly recalculated values and SaveChanges is called twice.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkWithRecalculatedSalesOrderLine()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrder = new SalesOrder
            {
                Freight = 50
            };
            context.SalesOrder.Add(salesOrder);
            context.SaveChanges();
            var salesOrderId = salesOrder.SalesOrderId;

            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedLine = (SalesOrderLine)okResult.Value;
            Assert.AreEqual(1000, returnedLine.Amount);
            Assert.AreEqual(100, returnedLine.DiscountAmount);
            Assert.AreEqual(900, returnedLine.SubTotal);
            Assert.AreEqual(45, returnedLine.TaxAmount);
            Assert.AreEqual(945, returnedLine.Total);
            // Verify data was persisted correctly
            Assert.AreEqual(1, context.SalesOrderLine.Count());
        }

        /// <summary>
        /// Tests that Insert method correctly handles payload with zero values.
        /// Input: SalesOrderLine with Quantity=0, Price=0, DiscountPercentage=0, TaxPercentage=0.
        /// Expected: Returns OkObjectResult with all calculated values as zero.
        /// </summary>
        [TestMethod]
        [DataRow(0.0, 0.0, 0.0, 0.0, DisplayName = "All zeros")]
        [DataRow(0.0, 100.0, 0.0, 0.0, DisplayName = "Zero quantity")]
        [DataRow(10.0, 0.0, 0.0, 0.0, DisplayName = "Zero price")]
        [DataRow(10.0, 100.0, 100.0, 0.0, DisplayName = "100% discount")]
        [DataRow(10.0, 100.0, 0.0, 100.0, DisplayName = "100% tax")]
        public void Insert_EdgeCaseValues_ReturnsOkWithCorrectCalculations(double quantity, double price, double discountPct, double taxPct)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var salesOrderId = 1;
            var salesOrder = new SalesOrder
            {
                SalesOrderId = salesOrderId,
                Freight = 0
            };
            context.SalesOrder.Add(salesOrder);
            context.SaveChanges();

            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPct,
                TaxPercentage = taxPct
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedLine = (SalesOrderLine)okResult.Value;
            var expectedAmount = quantity * price;
            var expectedDiscountAmount = (discountPct * expectedAmount) / 100.0;
            var expectedSubTotal = expectedAmount - expectedDiscountAmount;
            var expectedTaxAmount = (taxPct * expectedSubTotal) / 100.0;
            var expectedTotal = expectedSubTotal + expectedTaxAmount;
            Assert.AreEqual(expectedAmount, returnedLine.Amount);
            Assert.AreEqual(expectedDiscountAmount, returnedLine.DiscountAmount);
            Assert.AreEqual(expectedSubTotal, returnedLine.SubTotal);
            Assert.AreEqual(expectedTaxAmount, returnedLine.TaxAmount);
            Assert.AreEqual(expectedTotal, returnedLine.Total);
            // Verify data was persisted correctly
            Assert.AreEqual(1, context.SalesOrderLine.Count());
        }

        /// <summary>
        /// Tests that Insert method correctly handles negative values in calculations.
        /// Input: SalesOrderLine with negative Quantity=-5, Price=100.
        /// Expected: Returns OkObjectResult with negative Amount=-500.
        /// </summary>
        [TestMethod]
        [DataRow(-5.0, 100.0, 0.0, 0.0, DisplayName = "Negative quantity")]
        [DataRow(10.0, -50.0, 0.0, 0.0, DisplayName = "Negative price")]
        [DataRow(-10.0, -50.0, 0.0, 0.0, DisplayName = "Both negative")]
        public void Insert_NegativeValues_ReturnsOkWithCorrectCalculations(double quantity, double price, double discountPct, double taxPct)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrder = new SalesOrder
            {
                Freight = 0
            };
            context.SalesOrder.Add(salesOrder);
            context.SaveChanges();
            var salesOrderId = salesOrder.SalesOrderId;

            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPct,
                TaxPercentage = taxPct
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedLine = (SalesOrderLine)okResult.Value;
            var expectedAmount = quantity * price;
            var expectedDiscountAmount = (discountPct * expectedAmount) / 100.0;
            var expectedSubTotal = expectedAmount - expectedDiscountAmount;
            var expectedTaxAmount = (taxPct * expectedSubTotal) / 100.0;
            var expectedTotal = expectedSubTotal + expectedTaxAmount;
            Assert.AreEqual(expectedAmount, returnedLine.Amount);
            Assert.AreEqual(expectedDiscountAmount, returnedLine.DiscountAmount);
            Assert.AreEqual(expectedSubTotal, returnedLine.SubTotal);
            Assert.AreEqual(expectedTaxAmount, returnedLine.TaxAmount);
            Assert.AreEqual(expectedTotal, returnedLine.Total);
            // Verify data was persisted correctly
            Assert.AreEqual(1, context.SalesOrderLine.Count());
        }

        /// <summary>
        /// Tests that Insert method handles extreme double values correctly.
        /// Input: SalesOrderLine with very large or extreme double values.
        /// Expected: Returns OkObjectResult with correctly calculated values without overflow.
        /// </summary>
        [TestMethod]
        [DataRow(double.MaxValue, 1.0, 0.0, 0.0, DisplayName = "MaxValue quantity")]
        [DataRow(1.0, double.MaxValue, 0.0, 0.0, DisplayName = "MaxValue price")]
        [DataRow(double.MinValue, 1.0, 0.0, 0.0, DisplayName = "MinValue quantity")]
        [DataRow(1.0, double.MinValue, 0.0, 0.0, DisplayName = "MinValue price")]
        public void Insert_ExtremeDoubleValues_ReturnsOkWithCalculations(double quantity, double price, double discountPct, double taxPct)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrder = new SalesOrder
            {
                Freight = 0
            };
            context.SalesOrder.Add(salesOrder);
            context.SaveChanges();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderId = salesOrder.SalesOrderId,
                ProductId = 1,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPct,
                TaxPercentage = taxPct
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(1, context.SalesOrderLine.Count());
            // Verify the sales order was updated (happens in the second SaveChanges call)
            var updatedSalesOrder = context.SalesOrder.First();
            Assert.IsNotNull(updatedSalesOrder);
        }

        /// <summary>
        /// Tests that Insert method handles special double values (NaN, Infinity).
        /// Input: SalesOrderLine with double.NaN, double.PositiveInfinity, or double.NegativeInfinity.
        /// Expected: Returns OkObjectResult with NaN or Infinity in calculations.
        /// </summary>
        [TestMethod]
        [DataRow(double.NaN, 100.0, 0.0, 0.0, DisplayName = "NaN quantity")]
        [DataRow(10.0, double.NaN, 0.0, 0.0, DisplayName = "NaN price")]
        [DataRow(double.PositiveInfinity, 100.0, 0.0, 0.0, DisplayName = "PositiveInfinity quantity")]
        [DataRow(10.0, double.PositiveInfinity, 0.0, 0.0, DisplayName = "PositiveInfinity price")]
        [DataRow(double.NegativeInfinity, 100.0, 0.0, 0.0, DisplayName = "NegativeInfinity quantity")]
        [DataRow(10.0, double.NegativeInfinity, 0.0, 0.0, DisplayName = "NegativeInfinity price")]
        public void Insert_SpecialDoubleValues_ReturnsOkWithSpecialValues(double quantity, double price, double discountPct, double taxPct)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrder = new SalesOrder
            {
                Freight = 0
            };
            context.SalesOrder.Add(salesOrder);
            context.SaveChanges();
            var salesOrderId = salesOrder.SalesOrderId;

            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPct,
                TaxPercentage = taxPct
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(1, context.SalesOrderLine.Count());
        }

        /// <summary>
        /// Tests that Insert method correctly updates parent SalesOrder when SalesOrder exists.
        /// Input: Valid payload with SalesOrderId that exists in database.
        /// Expected: UpdateSalesOrder is called and parent SalesOrder totals are updated.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayloadWithExistingSalesOrder_UpdatesParentSalesOrder()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrder = new SalesOrder
            {
                Freight = 25.5,
                Amount = 0,
                SubTotal = 0,
                Discount = 0,
                Tax = 0,
                Total = 0
            };
            context.SalesOrder.Add(salesOrder);
            context.SaveChanges();
            var salesOrderId = salesOrder.SalesOrderId;
            // Pre-seed an existing line for this SalesOrder
            var existingLine = new SalesOrderLine
            {
                SalesOrderId = salesOrderId,
                Amount = 500,
                SubTotal = 450,
                DiscountAmount = 50,
                TaxAmount = 22.5,
                Total = 472.5,
                Quantity = 10,
                Price = 50,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            context.SalesOrderLine.Add(existingLine);
            context.SaveChanges();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = 5,
                Price = 50,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            // After insert, UpdateSalesOrder aggregates all lines for this SalesOrder:
            // existingLine: Amount=500, SubTotal=450, DiscountAmount=50, TaxAmount=22.5, Total=472.5
            // newLine (recalculated): Amount=250, SubTotal=250, DiscountAmount=0, TaxAmount=0, Total=250
            var updatedSalesOrder = context.SalesOrder.First(so => so.SalesOrderId == salesOrderId);
            Assert.AreEqual(750, updatedSalesOrder.Amount);
            Assert.AreEqual(700, updatedSalesOrder.SubTotal);
            Assert.AreEqual(50, updatedSalesOrder.Discount);
            Assert.AreEqual(22.5, updatedSalesOrder.Tax);
            Assert.AreEqual(748, updatedSalesOrder.Total); // 25.5 (Freight) + 722.5 (sum of Total)
        }

        /// <summary>
        /// Tests that Insert method handles case when parent SalesOrder does not exist.
        /// Input: Valid payload with SalesOrderId that does not exist in database.
        /// Expected: Returns OkObjectResult without updating parent (UpdateSalesOrder finds null).
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayloadWithNonExistentSalesOrder_ReturnsOkWithoutUpdatingParent()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrderId = 999; // Non-existent SalesOrder
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            // Verify line was added
            Assert.AreEqual(1, context.SalesOrderLine.Count());
            // Verify no SalesOrder was created or updated (none existed)
            Assert.AreEqual(0, context.SalesOrder.Count());
        }

        /// <summary>
        /// Tests that Insert method preserves other properties of SalesOrderLine that are not recalculated.
        /// Input: SalesOrderLine with specific ProductId, Description, SalesOrderLineId.
        /// Expected: Returns OkObjectResult with preserved properties.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayloadWithAllProperties_PreservesNonCalculatedProperties()
        {
            // Arrange
            // Use InMemoryDatabase instead of mocking DbSet<T> because EF Core 10
            // seals the IQueryable interface members on DbSet<T>, preventing Moq from
            // overriding them via .As<IQueryable<T>>().
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_InsertPreservesProps_{Guid.NewGuid()}")
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrderId = 1;
            var salesOrder = new SalesOrder
            {
                SalesOrderId = salesOrderId,
                Freight = 0
            };
            context.SalesOrder.Add(salesOrder);
            context.SaveChanges();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 42,
                SalesOrderId = salesOrderId,
                ProductId = 999,
                Description = "Test Product Description",
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                action = "insert",
                key = 42,
                antiForgery = "token123",
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var returnedLine = (SalesOrderLine)okResult.Value;
            Assert.AreEqual(42, returnedLine.SalesOrderLineId);
            Assert.AreEqual(salesOrderId, returnedLine.SalesOrderId);
            Assert.AreEqual(999, returnedLine.ProductId);
            Assert.AreEqual("Test Product Description", returnedLine.Description);
            Assert.AreEqual(10, returnedLine.Quantity);
            Assert.AreEqual(100, returnedLine.Price);
            Assert.AreEqual(1, context.SalesOrderLine.Count());
        }

        /// <summary>
        /// Tests that Insert method handles boundary SalesOrderId values correctly.
        /// Input: SalesOrderLine with int.MinValue, int.MaxValue, and 0 as SalesOrderId.
        /// Expected: Returns OkObjectResult and attempts to update parent SalesOrder.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue, DisplayName = "MinValue SalesOrderId")]
        [DataRow(int.MaxValue, DisplayName = "MaxValue SalesOrderId")]
        [DataRow(0, DisplayName = "Zero SalesOrderId")]
        [DataRow(-1, DisplayName = "Negative SalesOrderId")]
        public void Insert_BoundarySalesOrderIdValues_ReturnsOk(int salesOrderId)
        {
            // Arrange
            // Use InMemoryDatabase instead of mocking DbSet<T> because EF Core 10
            // seals the IQueryable interface members on DbSet<T>, preventing Moq from
            // overriding them via .As<IQueryable<T>>().
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"TestDb_InsertBoundary_{salesOrderId}_{Guid.NewGuid()}")
                .Options;
            using var context = new ApplicationDbContext(options);
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = salesOrderId,
                ProductId = 1,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var controller = new SalesOrderLineController(context);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(1, context.SalesOrderLine.Count());
        }

        /// <summary>
        /// Helper method to setup controller context with request headers.
        /// </summary>
        private static void SetupControllerContext(Controller controller, string headerKey, string headerValue)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers[headerKey] = new StringValues(headerValue);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        /// <summary>
        /// Tests the Update method with valid payload to ensure it recalculates totals,
        /// updates the entity, saves changes, updates parent sales order, and returns Ok result.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithRecalculatedSalesOrderLine()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = 100,
                ProductId = 50,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 100,
                Freight = 20
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>
            {
                salesOrder
            }.AsQueryable();
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Provider).Returns(salesOrderLines.Provider);
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Expression).Returns(salesOrderLines.Expression);
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.ElementType).Returns(salesOrderLines.ElementType);
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.GetEnumerator()).Returns(salesOrderLines.GetEnumerator());
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrders.Provider);
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrders.Expression);
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrders.ElementType);
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(salesOrders.GetEnumerator());
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult.Value);
            var returnedLine = okResult.Value as SalesOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.AreEqual(1000.0, returnedLine.Amount);
            Assert.AreEqual(100.0, returnedLine.DiscountAmount);
            Assert.AreEqual(900.0, returnedLine.SubTotal);
            Assert.AreEqual(45.0, returnedLine.TaxAmount);
            Assert.AreEqual(945.0, returnedLine.Total);
            mockSalesOrderLineSet.Verify(m => m.Update(It.IsAny<SalesOrderLine>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests the Update method with zero quantity to verify correct recalculation with zero values.
        /// </summary>
        [TestMethod]
        public void Update_ZeroQuantity_ReturnsOkResultWithZeroCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = 100,
                ProductId = 50,
                Quantity = 0,
                Price = 100,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 100
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>
            {
                salesOrder
            }.AsQueryable();
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Provider).Returns(salesOrderLines.Provider);
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.Expression).Returns(salesOrderLines.Expression);
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.ElementType).Returns(salesOrderLines.ElementType);
            mockSalesOrderLineSet.As<IQueryable<SalesOrderLine>>().Setup(m => m.GetEnumerator()).Returns(salesOrderLines.GetEnumerator());
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.Provider).Returns(salesOrders.Provider);
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.Expression).Returns(salesOrders.Expression);
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.ElementType).Returns(salesOrders.ElementType);
            mockSalesOrderSet.As<IQueryable<SalesOrder>>().Setup(m => m.GetEnumerator()).Returns(salesOrders.GetEnumerator());
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            var okResult = result as OkObjectResult;
            var returnedLine = okResult.Value as SalesOrderLine;
            Assert.AreEqual(0.0, returnedLine.Amount);
            Assert.AreEqual(0.0, returnedLine.Total);
        }

        /// <summary>
        /// Tests the Update method with negative quantity to verify handling of negative values in calculations.
        /// </summary>
        [TestMethod]
        public void Update_NegativeQuantity_ReturnsOkResultWithNegativeCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = 100,
                Quantity = -5,
                Price = 100,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 100
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>
            {
                salesOrder
            }.AsQueryable();
            SetupQueryable(mockSalesOrderLineSet, salesOrderLines);
            SetupQueryable(mockSalesOrderSet, salesOrders);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            var okResult = result as OkObjectResult;
            var returnedLine = okResult.Value as SalesOrderLine;
            Assert.AreEqual(-500.0, returnedLine.Amount);
            Assert.AreEqual(-500.0, returnedLine.Total);
        }

        /// <summary>
        /// Tests the Update method with 100% discount to verify correct calculation with full discount.
        /// </summary>
        [TestMethod]
        public void Update_FullDiscount_ReturnsOkResultWithZeroSubTotal()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = 100,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 100,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 100
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>
            {
                salesOrder
            }.AsQueryable();
            SetupQueryable(mockSalesOrderLineSet, salesOrderLines);
            SetupQueryable(mockSalesOrderSet, salesOrders);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            var okResult = result as OkObjectResult;
            var returnedLine = okResult.Value as SalesOrderLine;
            Assert.AreEqual(1000.0, returnedLine.Amount);
            Assert.AreEqual(1000.0, returnedLine.DiscountAmount);
            Assert.AreEqual(0.0, returnedLine.SubTotal);
            Assert.AreEqual(0.0, returnedLine.TaxAmount);
            Assert.AreEqual(0.0, returnedLine.Total);
        }

        /// <summary>
        /// Tests the Update method with special double values (NaN, Infinity) to verify handling of edge case numeric values.
        /// </summary>
        /// <param name = "quantity">Quantity value to test.</param>
        /// <param name = "price">Price value to test.</param>
        [TestMethod]
        [DataRow(double.NaN, 100.0)]
        [DataRow(100.0, double.NaN)]
        [DataRow(double.PositiveInfinity, 100.0)]
        [DataRow(100.0, double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity, 100.0)]
        [DataRow(100.0, double.NegativeInfinity)]
        public void Update_SpecialDoubleValues_ReturnsOkResultWithSpecialCalculations(double quantity, double price)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = 100,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 100
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>
            {
                salesOrder
            }.AsQueryable();
            SetupQueryable(mockSalesOrderLineSet, salesOrderLines);
            SetupQueryable(mockSalesOrderSet, salesOrders);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult.Value);
        }

        /// <summary>
        /// Tests the Update method with extreme SalesOrderId values to verify handling of boundary integer values.
        /// </summary>
        /// <param name = "salesOrderId">SalesOrderId value to test.</param>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public void Update_ExtremeSalesOrderIdValues_ReturnsOkResult(int salesOrderId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = salesOrderId,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>().AsQueryable();
            SetupQueryable(mockSalesOrderLineSet, salesOrderLines);
            SetupQueryable(mockSalesOrderSet, salesOrders);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSalesOrderLineSet.Verify(m => m.Update(It.IsAny<SalesOrderLine>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests the Update method when parent SalesOrder is not found to verify graceful handling.
        /// </summary>
        [TestMethod]
        public void Update_ParentSalesOrderNotFound_ReturnsOkResultWithoutUpdatingParent()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = 999,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>().AsQueryable();
            SetupQueryable(mockSalesOrderLineSet, salesOrderLines);
            SetupQueryable(mockSalesOrderSet, salesOrders);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSalesOrderLineSet.Verify(m => m.Update(It.IsAny<SalesOrderLine>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
            mockContext.Verify(m => m.Update(It.IsAny<SalesOrder>()), Times.Never);
        }

        /// <summary>
        /// Tests the Update method with extreme discount and tax percentages to verify calculation behavior with unusual percentage values.
        /// </summary>
        /// <param name = "discountPercentage">Discount percentage to test.</param>
        /// <param name = "taxPercentage">Tax percentage to test.</param>
        [TestMethod]
        [DataRow(0.0, 0.0)]
        [DataRow(200.0, 200.0)]
        [DataRow(-10.0, -10.0)]
        [DataRow(double.MaxValue, 0.0)]
        [DataRow(0.0, double.MaxValue)]
        public void Update_ExtremePercentages_ReturnsOkResult(double discountPercentage, double taxPercentage)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockSalesOrderLineSet = new Mock<DbSet<SalesOrderLine>>();
            var mockSalesOrderSet = new Mock<DbSet<SalesOrder>>();
            var salesOrderLine = new SalesOrderLine
            {
                SalesOrderLineId = 1,
                SalesOrderId = 100,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = discountPercentage,
                TaxPercentage = taxPercentage
            };
            var payload = new CrudViewModel<SalesOrderLine>
            {
                value = salesOrderLine
            };
            var salesOrder = new SalesOrder
            {
                SalesOrderId = 100
            };
            var salesOrderLines = new List<SalesOrderLine>
            {
                salesOrderLine
            }.AsQueryable();
            var salesOrders = new List<SalesOrder>
            {
                salesOrder
            }.AsQueryable();
            SetupQueryable(mockSalesOrderLineSet, salesOrderLines);
            SetupQueryable(mockSalesOrderSet, salesOrders);
            mockContext.Setup(c => c.SalesOrderLine).Returns(mockSalesOrderLineSet.Object);
            mockContext.Setup(c => c.SalesOrder).Returns(mockSalesOrderSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesOrderLineController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Helper method to setup queryable behavior for DbSet mocks.
        /// </summary>
        private static void SetupQueryable<T>(Mock<DbSet<T>> mockSet, IQueryable<T> data)
            where T : class
        {
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
        }

        /// <summary>
        /// Creates a SalesOrderLineController instance with mocked dependencies and request headers.
        /// </summary>
        private SalesOrderLineController CreateController(ApplicationDbContext context, string shipmentIdHeaderValue)
        {
            var controller = new SalesOrderLineController(context);
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Headers["ShipmentId"] = shipmentIdHeaderValue;
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            return controller;
        }

        /// <summary>
        /// Creates a SalesOrderLineController instance without the ShipmentId header.
        /// </summary>
        private SalesOrderLineController CreateControllerWithoutHeader(ApplicationDbContext context)
        {
            var controller = new SalesOrderLineController(context);
            var httpContext = new DefaultHttpContext();
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
            return controller;
        }

    }
}