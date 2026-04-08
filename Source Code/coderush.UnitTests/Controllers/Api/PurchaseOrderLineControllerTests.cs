using System;
using System.Collections.Generic;
using System.Linq;

using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for PurchaseOrderLineController.
    /// </summary>
    [TestClass]
    public class PurchaseOrderLineControllerTests
    {
        /// <summary>
        /// Tests that Insert method successfully adds a valid purchase order line to the database
        /// and returns an OK result with the recalculated purchase order line.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkWithRecalculatedLine()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var returnedLine = okResult.Value as PurchaseOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.AreEqual(1000.0, returnedLine.Amount, 0.001);
            Assert.AreEqual(100.0, returnedLine.DiscountAmount, 0.001);
            Assert.AreEqual(900.0, returnedLine.SubTotal, 0.001);
            Assert.AreEqual(45.0, returnedLine.TaxAmount, 0.001);
            Assert.AreEqual(945.0, returnedLine.Total, 0.001);
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
            mockContext.Verify(m => m.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that Insert method correctly handles zero values for all numeric properties.
        /// </summary>
        [TestMethod]
        public void Insert_ZeroValues_ReturnsOkWithZeroCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 0,
                Quantity = 0,
                Price = 0,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedLine = okResult.Value as PurchaseOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.AreEqual(0.0, returnedLine.Amount);
            Assert.AreEqual(0.0, returnedLine.Total);
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles maximum integer value for PurchaseOrderId.
        /// </summary>
        [TestMethod]
        public void Insert_MaxPurchaseOrderId_ReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = int.MaxValue,
                Quantity = 1,
                Price = 1,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles minimum integer value for PurchaseOrderId.
        /// </summary>
        [TestMethod]
        public void Insert_MinPurchaseOrderId_ReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = int.MinValue,
                Quantity = 1,
                Price = 1,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles negative values for Quantity and Price.
        /// Expected behavior: calculations proceed but result in negative amounts.
        /// </summary>
        [TestMethod]
        public void Insert_NegativeQuantityAndPrice_ReturnsOkWithNegativeCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = -5,
                Price = -10,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedLine = okResult.Value as PurchaseOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.AreEqual(50.0, returnedLine.Amount, 0.001);
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles special double values like NaN for Quantity.
        /// Expected behavior: calculations result in NaN propagation.
        /// </summary>
        [TestMethod]
        public void Insert_NaNQuantity_ReturnsOkWithNaNCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = double.NaN,
                Price = 100,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedLine = okResult.Value as PurchaseOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.IsTrue(double.IsNaN(returnedLine.Amount));
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles positive infinity for Price.
        /// Expected behavior: calculations result in infinity propagation.
        /// </summary>
        [TestMethod]
        public void Insert_PositiveInfinityPrice_ReturnsOkWithInfinityCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = 1,
                Price = double.PositiveInfinity,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedLine = okResult.Value as PurchaseOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.IsTrue(double.IsPositiveInfinity(returnedLine.Amount));
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles negative infinity for Quantity.
        /// Expected behavior: calculations result in negative infinity propagation.
        /// </summary>
        [TestMethod]
        public void Insert_NegativeInfinityQuantity_ReturnsOkWithNegativeInfinityCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = double.NegativeInfinity,
                Price = 1,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedLine = okResult.Value as PurchaseOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.IsTrue(double.IsNegativeInfinity(returnedLine.Amount));
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles very large double values without overflow.
        /// </summary>
        [TestMethod]
        public void Insert_LargeDoubleValues_ReturnsOkWithCalculations()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = double.MaxValue,
                Price = 1,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method handles 100% discount correctly (SubTotal should be 0).
        /// </summary>
        [TestMethod]
        public void Insert_HundredPercentDiscount_ReturnsOkWithZeroSubTotal()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = 10,
                Price = 100,
                DiscountPercentage = 100,
                TaxPercentage = 10
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedLine = okResult.Value as PurchaseOrderLine;
            Assert.IsNotNull(returnedLine);
            Assert.AreEqual(1000.0, returnedLine.Amount, 0.001);
            Assert.AreEqual(1000.0, returnedLine.DiscountAmount, 0.001);
            Assert.AreEqual(0.0, returnedLine.SubTotal, 0.001);
            Assert.AreEqual(0.0, returnedLine.TaxAmount, 0.001);
            Assert.AreEqual(0.0, returnedLine.Total, 0.001);
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count);
        }

        /// <summary>
        /// Tests that Insert method calls SaveChanges on the context.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = 5,
                Price = 20,
                DiscountPercentage = 0,
                TaxPercentage = 0
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            controller.Insert(payload);

            // Assert
            mockContext.Verify(m => m.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that Insert method adds the purchase order line to the DbSet.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_AddsToDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderId = 1,
                Quantity = 3,
                Price = 50,
                DiscountPercentage = 5,
                TaxPercentage = 10
            };
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            controller.Insert(payload);

            // Assert
            Assert.AreEqual(1, purchaseOrderLineSet.AddedEntities.Count(p => p.PurchaseOrderId == 1));
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing purchase order line and returns Ok result.
        /// Input: Valid payload with existing purchase order line ID.
        /// Expected: Purchase order line is removed, SaveChanges is called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Remove_ValidPayloadWithExistingId_RemovesItemAndReturnsOk()
        {
            // Arrange
            var purchaseOrderLineId = 1;
            var purchaseOrderId = 100;
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = purchaseOrderLineId,
                PurchaseOrderId = purchaseOrderId,
                Amount = 100.0,
                SubTotal = 90.0,
                DiscountAmount = 10.0
            };

            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);

            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = purchaseOrderId,
                Freight = 0
            };
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>()));

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { key = purchaseOrderLineId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.AreEqual(1, purchaseOrderLineSet.RemovedEntities.Count(p => p.PurchaseOrderLineId == purchaseOrderLineId));
            mockContext.Verify(m => m.SaveChanges(), Times.AtLeastOnce);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(purchaseOrderLine, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove successfully handles various numeric key types.
        /// Input: Payload with key as different numeric types (int, long, double, string).
        /// Expected: Key is converted to int and item is removed successfully.
        /// </summary>
        [TestMethod]
        [DataRow(1, DisplayName = "Integer key")]
        [DataRow(1L, DisplayName = "Long key")]
        [DataRow(1.0, DisplayName = "Double key")]
        [DataRow("1", DisplayName = "String key")]
        public void Remove_VariousNumericKeyTypes_SuccessfullyConvertsAndRemoves(object keyValue)
        {
            // Arrange
            var purchaseOrderLineId = 1;
            var purchaseOrderId = 100;
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = purchaseOrderLineId,
                PurchaseOrderId = purchaseOrderId,
                Amount = 100.0
            };

            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);

            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = purchaseOrderId,
                Freight = 0
            };
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { key = keyValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.AreEqual(1, purchaseOrderLineSet.RemovedEntities.Count);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Remove handles key with maximum integer value that exists in database.
        /// Input: Payload with int.MaxValue as key that exists.
        /// Expected: Item is removed successfully.
        /// </summary>
        [TestMethod]
        public void Remove_MaxIntegerKeyExists_RemovesSuccessfully()
        {
            // Arrange
            var purchaseOrderLineId = int.MaxValue;
            var purchaseOrderId = 100;
            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = purchaseOrderLineId,
                PurchaseOrderId = purchaseOrderId,
                Amount = 100.0
            };

            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);

            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = purchaseOrderId,
                Freight = 0
            };
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { key = purchaseOrderLineId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.AreEqual(1, purchaseOrderLineSet.RemovedEntities.Count(p => p.PurchaseOrderLineId == purchaseOrderLineId));
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that the Update method successfully updates a purchase order line with valid data and returns OkObjectResult.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithUpdatedLine()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 100,
                ProductId = 1,
                Quantity = 10,
                Price = 50,
                DiscountPercentage = 10,
                TaxPercentage = 5
            };

            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = 100,
                Freight = 10
            };

            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine>
            {
                value = purchaseOrderLine
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(PurchaseOrderLine));
            var updatedLine = (PurchaseOrderLine)okResult.Value;
            Assert.AreEqual(500, updatedLine.Amount);
            Assert.AreEqual(50, updatedLine.DiscountAmount);
            Assert.AreEqual(450, updatedLine.SubTotal);
            Assert.AreEqual(22.5, updatedLine.TaxAmount);
            Assert.AreEqual(472.5, updatedLine.Total);
        }

        /// <summary>
        /// Tests that the Update method correctly calls DbSet Update method.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsDbSetUpdate()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 100,
                Quantity = 5,
                Price = 20
            };

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = 100 };
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            controller.Update(payload);

            // Assert
            Assert.AreEqual(1, purchaseOrderLineSet.UpdatedEntities.Count);
        }

        /// <summary>
        /// Tests that the Update method calls SaveChanges on the database context twice (once for line, once for purchase order).
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsSaveChangesTwice()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 100,
                Quantity = 5,
                Price = 20
            };

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = 100 };
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            controller.Update(payload);

            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that the Update method correctly recalculates values with zero quantity.
        /// </summary>
        [TestMethod]
        [DataRow(0.0, 100.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)]
        [DataRow(10.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0)]
        [DataRow(10.0, 50.0, 0.0, 0.0, 500.0, 0.0, 500.0, 0.0)]
        [DataRow(10.0, 50.0, 10.0, 5.0, 500.0, 50.0, 450.0, 22.5)]
        public void Update_VariousNumericValues_RecalculatesCorrectly(
            double quantity,
            double price,
            double discountPercentage,
            double taxPercentage,
            double expectedAmount,
            double expectedDiscountAmount,
            double expectedSubTotal,
            double expectedTaxAmount)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 100,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPercentage,
                TaxPercentage = taxPercentage
            };

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = 100 };
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Update(payload);

            // Assert
            var okResult = (OkObjectResult)result;
            var updatedLine = (PurchaseOrderLine)okResult.Value!;
            Assert.AreEqual(expectedAmount, updatedLine.Amount, 0.001);
            Assert.AreEqual(expectedDiscountAmount, updatedLine.DiscountAmount, 0.001);
            Assert.AreEqual(expectedSubTotal, updatedLine.SubTotal, 0.001);
            Assert.AreEqual(expectedTaxAmount, updatedLine.TaxAmount, 0.001);
        }

        /// <summary>
        /// Tests that the Update method handles negative values correctly.
        /// </summary>
        [TestMethod]
        [DataRow(-10.0, 50.0, 10.0, 5.0)]
        [DataRow(10.0, -50.0, 10.0, 5.0)]
        [DataRow(10.0, 50.0, -10.0, 5.0)]
        [DataRow(10.0, 50.0, 10.0, -5.0)]
        public void Update_NegativeValues_RecalculatesWithNegativeResults(
            double quantity,
            double price,
            double discountPercentage,
            double taxPercentage)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 100,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPercentage,
                TaxPercentage = taxPercentage
            };

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = 100 };
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that the Update method handles extreme numeric boundary values.
        /// </summary>
        [TestMethod]
        [DataRow(double.MaxValue, 1.0, 0.0, 0.0)]
        [DataRow(1.0, double.MaxValue, 0.0, 0.0)]
        [DataRow(double.MinValue, 1.0, 0.0, 0.0)]
        [DataRow(1.0, double.MinValue, 0.0, 0.0)]
        public void Update_ExtremeNumericValues_HandlesWithoutException(
            double quantity,
            double price,
            double discountPercentage,
            double taxPercentage)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 100,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPercentage,
                TaxPercentage = taxPercentage
            };

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = 100 };
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that the Update method handles special double values like NaN and Infinity.
        /// </summary>
        [TestMethod]
        [DataRow(double.NaN, 50.0, 10.0, 5.0)]
        [DataRow(10.0, double.NaN, 10.0, 5.0)]
        [DataRow(double.PositiveInfinity, 50.0, 10.0, 5.0)]
        [DataRow(10.0, double.PositiveInfinity, 10.0, 5.0)]
        [DataRow(double.NegativeInfinity, 50.0, 10.0, 5.0)]
        [DataRow(10.0, double.NegativeInfinity, 10.0, 5.0)]
        public void Update_SpecialDoubleValues_HandlesWithoutException(
            double quantity,
            double price,
            double discountPercentage,
            double taxPercentage)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 100,
                Quantity = quantity,
                Price = price,
                DiscountPercentage = discountPercentage,
                TaxPercentage = taxPercentage
            };

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = 100 };
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that the Update method does not update parent PurchaseOrder when it doesn't exist.
        /// </summary>
        [TestMethod]
        public void Update_NonExistentPurchaseOrder_DoesNotThrowException()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = 999,
                Quantity = 10,
                Price = 50
            };

            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>();
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>();

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that the Update method correctly updates PurchaseOrder with boundary PurchaseOrderId values.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(int.MaxValue)]
        public void Update_BoundaryPurchaseOrderId_HandlesCorrectly(int purchaseOrderId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();

            var purchaseOrderLine = new PurchaseOrderLine
            {
                PurchaseOrderLineId = 1,
                PurchaseOrderId = purchaseOrderId,
                Quantity = 10,
                Price = 50
            };

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = purchaseOrderId };
            var purchaseOrderLineSet = new TestableDbSet<PurchaseOrderLine>([purchaseOrderLine]);
            var purchaseOrderSet = new TestableDbSet<PurchaseOrder>([purchaseOrder]);

            mockContext.Setup(c => c.PurchaseOrderLine).Returns(purchaseOrderLineSet);
            mockContext.Setup(c => c.PurchaseOrder).Returns(purchaseOrderSet);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockContext.Setup(c => c.Update(It.IsAny<PurchaseOrder>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PurchaseOrder>)null!);

            var controller = new PurchaseOrderLineController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseOrderLine> { value = purchaseOrderLine };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }
    }
}