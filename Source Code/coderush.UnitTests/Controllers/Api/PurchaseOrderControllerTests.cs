#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for PurchaseOrderController API endpoints.
    /// </summary>
    [TestClass]
    public class PurchaseOrderControllerTests
    {
        /// <summary>
        /// Tests that GetNotReceivedYet returns all purchase orders when no goods received notes exist.
        /// Input: Empty GoodsReceivedNote table, multiple PurchaseOrders exist.
        /// Expected: Returns OkObjectResult with all purchase orders.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_NoGoodsReceivedNotes_ReturnsAllPurchaseOrders()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var purchaseOrder1 = new PurchaseOrder { PurchaseOrderId = 1, PurchaseOrderName = "PO001" };
            var purchaseOrder2 = new PurchaseOrder { PurchaseOrderId = 2, PurchaseOrderName = "PO002" };
            var purchaseOrder3 = new PurchaseOrder { PurchaseOrderId = 3, PurchaseOrderName = "PO003" };

            context.PurchaseOrder.AddRange(purchaseOrder1, purchaseOrder2, purchaseOrder3);
            await context.SaveChangesAsync();

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(3, returnedOrders.Count);
            Assert.IsTrue(returnedOrders.Any(po => po.PurchaseOrderId == 1));
            Assert.IsTrue(returnedOrders.Any(po => po.PurchaseOrderId == 2));
            Assert.IsTrue(returnedOrders.Any(po => po.PurchaseOrderId == 3));
        }

        /// <summary>
        /// Tests that GetNotReceivedYet returns empty list when all purchase orders have been received.
        /// Input: All PurchaseOrders have corresponding GoodsReceivedNotes.
        /// Expected: Returns OkObjectResult with empty list.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_AllPurchaseOrdersReceived_ReturnsEmptyList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var purchaseOrder1 = new PurchaseOrder { PurchaseOrderId = 1, PurchaseOrderName = "PO001" };
            var purchaseOrder2 = new PurchaseOrder { PurchaseOrderId = 2, PurchaseOrderName = "PO002" };

            var grn1 = new GoodsReceivedNote { GoodsReceivedNoteId = 1, PurchaseOrderId = 1, GoodsReceivedNoteName = "GRN001" };
            var grn2 = new GoodsReceivedNote { GoodsReceivedNoteId = 2, PurchaseOrderId = 2, GoodsReceivedNoteName = "GRN002" };

            context.PurchaseOrder.AddRange(purchaseOrder1, purchaseOrder2);
            context.GoodsReceivedNote.AddRange(grn1, grn2);
            await context.SaveChangesAsync();

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(0, returnedOrders.Count);
        }

        /// <summary>
        /// Tests that GetNotReceivedYet returns only unreceived purchase orders when some have been received.
        /// Input: Mixed scenario - some PurchaseOrders with GoodsReceivedNotes, some without.
        /// Expected: Returns OkObjectResult with only unreceived purchase orders.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_MixedScenario_ReturnsOnlyUnreceivedOrders()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var purchaseOrder1 = new PurchaseOrder { PurchaseOrderId = 1, PurchaseOrderName = "PO001" };
            var purchaseOrder2 = new PurchaseOrder { PurchaseOrderId = 2, PurchaseOrderName = "PO002" };
            var purchaseOrder3 = new PurchaseOrder { PurchaseOrderId = 3, PurchaseOrderName = "PO003" };
            var purchaseOrder4 = new PurchaseOrder { PurchaseOrderId = 4, PurchaseOrderName = "PO004" };

            var grn1 = new GoodsReceivedNote { GoodsReceivedNoteId = 1, PurchaseOrderId = 1, GoodsReceivedNoteName = "GRN001" };
            var grn3 = new GoodsReceivedNote { GoodsReceivedNoteId = 2, PurchaseOrderId = 3, GoodsReceivedNoteName = "GRN003" };

            context.PurchaseOrder.AddRange(purchaseOrder1, purchaseOrder2, purchaseOrder3, purchaseOrder4);
            context.GoodsReceivedNote.AddRange(grn1, grn3);
            await context.SaveChangesAsync();

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(2, returnedOrders.Count);
            Assert.IsTrue(returnedOrders.Any(po => po.PurchaseOrderId == 2));
            Assert.IsTrue(returnedOrders.Any(po => po.PurchaseOrderId == 4));
            Assert.IsFalse(returnedOrders.Any(po => po.PurchaseOrderId == 1));
            Assert.IsFalse(returnedOrders.Any(po => po.PurchaseOrderId == 3));
        }

        /// <summary>
        /// Tests that GetNotReceivedYet handles empty database correctly.
        /// Input: Empty GoodsReceivedNote table and empty PurchaseOrder table.
        /// Expected: Returns OkObjectResult with empty list.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(0, returnedOrders.Count);
        }

        /// <summary>
        /// Tests that GetNotReceivedYet handles duplicate goods received notes correctly.
        /// Input: Multiple GoodsReceivedNotes referencing the same PurchaseOrderId.
        /// Expected: Returns OkObjectResult excluding the purchase order with duplicate GRNs.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_DuplicateGoodsReceivedNotes_ExcludesPurchaseOrderCorrectly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var purchaseOrder1 = new PurchaseOrder { PurchaseOrderId = 1, PurchaseOrderName = "PO001" };
            var purchaseOrder2 = new PurchaseOrder { PurchaseOrderId = 2, PurchaseOrderName = "PO002" };

            var grn1 = new GoodsReceivedNote { GoodsReceivedNoteId = 1, PurchaseOrderId = 1, GoodsReceivedNoteName = "GRN001" };
            var grn2 = new GoodsReceivedNote { GoodsReceivedNoteId = 2, PurchaseOrderId = 1, GoodsReceivedNoteName = "GRN002" };
            var grn3 = new GoodsReceivedNote { GoodsReceivedNoteId = 3, PurchaseOrderId = 1, GoodsReceivedNoteName = "GRN003" };

            context.PurchaseOrder.AddRange(purchaseOrder1, purchaseOrder2);
            context.GoodsReceivedNote.AddRange(grn1, grn2, grn3);
            await context.SaveChangesAsync();

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(1, returnedOrders.Count);
            Assert.AreEqual(2, returnedOrders[0].PurchaseOrderId);
        }

        /// <summary>
        /// Tests that GetNotReceivedYet handles large datasets efficiently.
        /// Input: Large number of PurchaseOrders with some having GoodsReceivedNotes.
        /// Expected: Returns OkObjectResult with correct unreceived purchase orders.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_LargeDataset_ReturnsCorrectResults()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var purchaseOrders = new List<PurchaseOrder>();
            var grns = new List<GoodsReceivedNote>();

            for (int i = 1; i <= 100; i++)
            {
                purchaseOrders.Add(new PurchaseOrder { PurchaseOrderId = i, PurchaseOrderName = $"PO{i:D3}" });

                if (i % 2 == 0)
                {
                    grns.Add(new GoodsReceivedNote { GoodsReceivedNoteId = i, PurchaseOrderId = i, GoodsReceivedNoteName = $"GRN{i:D3}" });
                }
            }

            context.PurchaseOrder.AddRange(purchaseOrders);
            context.GoodsReceivedNote.AddRange(grns);
            await context.SaveChangesAsync();

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(50, returnedOrders.Count);
            Assert.IsTrue(returnedOrders.All(po => po.PurchaseOrderId % 2 != 0));
        }

        /// <summary>
        /// Tests that GetNotReceivedYet handles boundary case with single purchase order not received.
        /// Input: Single PurchaseOrder without GoodsReceivedNote.
        /// Expected: Returns OkObjectResult with that single purchase order.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_SingleUnreceivedOrder_ReturnsSingleOrder()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var purchaseOrder = new PurchaseOrder { PurchaseOrderId = 1, PurchaseOrderName = "PO001" };

            context.PurchaseOrder.Add(purchaseOrder);
            await context.SaveChangesAsync();

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(1, returnedOrders.Count);
            Assert.AreEqual(1, returnedOrders[0].PurchaseOrderId);
            Assert.AreEqual("PO001", returnedOrders[0].PurchaseOrderName);
        }

        /// <summary>
        /// Tests that GetNotReceivedYet handles extreme PurchaseOrderId values correctly.
        /// Input: PurchaseOrders with int.MaxValue and other boundary IDs.
        /// Expected: Returns OkObjectResult with correct filtering.
        /// </summary>
        [TestMethod]
        public async Task GetNotReceivedYet_ExtremePurchaseOrderIds_HandlesCorrectly()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            var purchaseOrder1 = new PurchaseOrder { PurchaseOrderId = int.MaxValue, PurchaseOrderName = "POMax" };
            var purchaseOrder2 = new PurchaseOrder { PurchaseOrderId = 1, PurchaseOrderName = "PO1" };
            var purchaseOrder3 = new PurchaseOrder { PurchaseOrderId = int.MinValue, PurchaseOrderName = "POMin" };

            var grn = new GoodsReceivedNote { GoodsReceivedNoteId = 1, PurchaseOrderId = 1, GoodsReceivedNoteName = "GRN001" };

            context.PurchaseOrder.AddRange(purchaseOrder1, purchaseOrder2, purchaseOrder3);
            context.GoodsReceivedNote.Add(grn);
            await context.SaveChangesAsync();

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(context, mockNumberSequence.Object);

            // Act
            var result = await controller.GetNotReceivedYet();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedOrders = okResult.Value as List<PurchaseOrder>;
            Assert.IsNotNull(returnedOrders);
            Assert.AreEqual(2, returnedOrders.Count);
            Assert.IsTrue(returnedOrders.Any(po => po.PurchaseOrderId == int.MaxValue));
            Assert.IsTrue(returnedOrders.Any(po => po.PurchaseOrderId == int.MinValue));
            Assert.IsFalse(returnedOrders.Any(po => po.PurchaseOrderId == 1));
        }

        /// <summary>
        /// Tests that Insert method successfully creates a purchase order with valid input.
        /// Verifies that the purchase order name is assigned from number sequence,
        /// the purchase order is added to context, changes are saved, and an OK result is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithPurchaseOrder()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var testPurchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                BranchId = 1,
                VendorId = 1
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = testPurchaseOrder
            };

            var emptyLines = Enumerable.Empty<PurchaseOrderLine>().AsQueryable();
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Provider).Returns(emptyLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Expression).Returns(emptyLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.ElementType).Returns(emptyLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.GetEnumerator()).Returns(emptyLines.GetEnumerator());

            var testOrders = new[] { testPurchaseOrder }.AsQueryable();
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(testOrders.Provider);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(testOrders.Expression);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(testOrders.ElementType);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(testOrders.GetEnumerator());

            mockNumberSequence.Setup(x => x.GetNumberSequence("PO")).Returns("PO-12345");
            mockContext.Setup(x => x.PurchaseOrder).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(testPurchaseOrder, okResult.Value);
            Assert.AreEqual("PO-12345", testPurchaseOrder.PurchaseOrderName);
            mockDbSet.Verify(x => x.Add(testPurchaseOrder), Times.Once);
            mockContext.Verify(x => x.SaveChanges(), Times.AtLeastOnce);
            mockNumberSequence.Verify(x => x.GetNumberSequence("PO"), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method handles null or empty number sequence value.
        /// Verifies that the purchase order name is set to null when GetNumberSequence returns null.
        /// </summary>
        [TestMethod]
        [DataRow(null, DisplayName = "Null number sequence")]
        [DataRow("", DisplayName = "Empty number sequence")]
        public void Insert_GetNumberSequenceReturnsNullOrEmpty_SetsPurchaseOrderNameToValue(string numberSequence)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var testPurchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                BranchId = 1,
                VendorId = 1
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = testPurchaseOrder
            };

            var emptyLines = Enumerable.Empty<PurchaseOrderLine>().AsQueryable();
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Provider).Returns(emptyLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Expression).Returns(emptyLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.ElementType).Returns(emptyLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.GetEnumerator()).Returns(emptyLines.GetEnumerator());

            var testOrders = new[] { testPurchaseOrder }.AsQueryable();
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(testOrders.Provider);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(testOrders.Expression);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(testOrders.ElementType);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(testOrders.GetEnumerator());

            mockNumberSequence.Setup(x => x.GetNumberSequence("PO")).Returns(numberSequence);
            mockContext.Setup(x => x.PurchaseOrder).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(numberSequence, testPurchaseOrder.PurchaseOrderName);
            mockNumberSequence.Verify(x => x.GetNumberSequence("PO"), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method correctly sets PurchaseOrderName before adding to context.
        /// Verifies the order of operations: set name, add, save.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_SetsPurchaseOrderNameBeforeAddingToContext()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();
            string capturedName = null;

            var testPurchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                BranchId = 1,
                VendorId = 1
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = testPurchaseOrder
            };

            var emptyLines = Enumerable.Empty<PurchaseOrderLine>().AsQueryable();
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Provider).Returns(emptyLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Expression).Returns(emptyLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.ElementType).Returns(emptyLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.GetEnumerator()).Returns(emptyLines.GetEnumerator());

            var testOrders = new[] { testPurchaseOrder }.AsQueryable();
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(testOrders.Provider);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(testOrders.Expression);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(testOrders.ElementType);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(testOrders.GetEnumerator());

            mockNumberSequence.Setup(x => x.GetNumberSequence("PO")).Returns("PO-99999");
            mockContext.Setup(x => x.PurchaseOrder).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);

            mockDbSet.Setup(x => x.Add(It.IsAny<PurchaseOrder>()))
                .Callback<PurchaseOrder>(po => capturedName = po.PurchaseOrderName);

            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            controller.Insert(payload);

            // Assert
            Assert.AreEqual("PO-99999", capturedName);
        }

        /// <summary>
        /// Tests that Insert method handles purchase order with various boundary values.
        /// Verifies proper handling of extreme numeric values for purchase order properties.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue, DisplayName = "MinValue PurchaseOrderId")]
        [DataRow(int.MaxValue, DisplayName = "MaxValue PurchaseOrderId")]
        [DataRow(0, DisplayName = "Zero PurchaseOrderId")]
        public void Insert_PurchaseOrderWithBoundaryValues_SuccessfullyProcesses(int purchaseOrderId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var testPurchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = purchaseOrderId,
                BranchId = 1,
                VendorId = 1
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = testPurchaseOrder
            };

            var emptyLines = Enumerable.Empty<PurchaseOrderLine>().AsQueryable();
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Provider).Returns(emptyLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.Expression).Returns(emptyLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.ElementType).Returns(emptyLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>().Setup(m => m.GetEnumerator()).Returns(emptyLines.GetEnumerator());

            var testOrders = new[] { testPurchaseOrder }.AsQueryable();
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Provider).Returns(testOrders.Provider);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.Expression).Returns(testOrders.Expression);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.ElementType).Returns(testOrders.ElementType);
            mockDbSet.As<IQueryable<PurchaseOrder>>().Setup(m => m.GetEnumerator()).Returns(testOrders.GetEnumerator());

            mockNumberSequence.Setup(x => x.GetNumberSequence("PO")).Returns("PO-TEST");
            mockContext.Setup(x => x.PurchaseOrder).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);

            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(purchaseOrderId, testPurchaseOrder.PurchaseOrderId);
        }

        /// <summary>
        /// Tests that Update method successfully updates purchase order and returns OkObjectResult.
        /// Input: valid payload with purchase order
        /// Expected: OkObjectResult containing the updated purchase order, SaveChanges called twice
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_UpdatesAndReturnsOkResult()
        {
            // Arrange
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = 1,
                PurchaseOrderName = "PO-001",
                BranchId = 1,
                VendorId = 1,
                OrderDate = DateTimeOffset.Now,
                DeliveryDate = DateTimeOffset.Now.AddDays(7),
                CurrencyId = 1,
                PurchaseTypeId = 1,
                Remarks = "Test remarks",
                Amount = 1000,
                SubTotal = 900,
                Discount = 50,
                Tax = 100,
                Freight = 50,
                Total = 1000,
                PurchaseOrderLines = new List<PurchaseOrderLine>()
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                action = "update",
                value = purchaseOrder
            };

            var mockPurchaseOrderDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var purchaseOrders = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrders.Provider);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrders.Expression);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrders.ElementType);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrders.GetEnumerator());

            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrderLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrderLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrderLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrderLines.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockPurchaseOrderDbSet.Object);
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(purchaseOrder, okResult.Value);
            mockPurchaseOrderDbSet.Verify(m => m.Update(purchaseOrder), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that Update method with zero PurchaseOrderId still processes correctly.
        /// Input: valid payload with PurchaseOrderId = 0
        /// Expected: OkObjectResult returned, SaveChanges called
        /// </summary>
        [TestMethod]
        public void Update_PurchaseOrderIdZero_ProcessesSuccessfully()
        {
            // Arrange
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = 0,
                PurchaseOrderName = "PO-000",
                Amount = 100,
                Total = 100,
                PurchaseOrderLines = new List<PurchaseOrderLine>()
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = purchaseOrder
            };

            var mockPurchaseOrderDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var purchaseOrders = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrders.Provider);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrders.Expression);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrders.ElementType);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrders.GetEnumerator());

            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrderLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrderLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrderLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrderLines.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockPurchaseOrderDbSet.Object);
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockContext.Verify(c => c.SaveChanges(), Times.Exactly(2));
        }

        /// <summary>
        /// Tests that Update method with negative PurchaseOrderId processes correctly.
        /// Input: valid payload with PurchaseOrderId = -1
        /// Expected: OkObjectResult returned, update operations performed
        /// </summary>
        [TestMethod]
        public void Update_NegativePurchaseOrderId_ProcessesSuccessfully()
        {
            // Arrange
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = -1,
                PurchaseOrderName = "PO-NEG",
                Amount = 500,
                Total = 500,
                PurchaseOrderLines = new List<PurchaseOrderLine>()
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = purchaseOrder
            };

            var mockPurchaseOrderDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var purchaseOrders = new List<PurchaseOrder>().AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrders.Provider);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrders.Expression);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrders.ElementType);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrders.GetEnumerator());

            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrderLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrderLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrderLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrderLines.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockPurchaseOrderDbSet.Object);
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockPurchaseOrderDbSet.Verify(m => m.Update(purchaseOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Update method with maximum integer PurchaseOrderId processes correctly.
        /// Input: valid payload with PurchaseOrderId = int.MaxValue
        /// Expected: OkObjectResult returned, operations complete successfully
        /// </summary>
        [TestMethod]
        public void Update_MaxIntegerPurchaseOrderId_ProcessesSuccessfully()
        {
            // Arrange
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = int.MaxValue,
                PurchaseOrderName = "PO-MAX",
                Amount = 999.99,
                Total = 999.99,
                PurchaseOrderLines = new List<PurchaseOrderLine>()
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = purchaseOrder
            };

            var mockPurchaseOrderDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var purchaseOrders = new List<PurchaseOrder>().AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrders.Provider);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrders.Expression);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrders.ElementType);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrders.GetEnumerator());

            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrderLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrderLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrderLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrderLines.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockPurchaseOrderDbSet.Object);
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockPurchaseOrderDbSet.Verify(m => m.Update(purchaseOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Update method with minimum integer PurchaseOrderId processes correctly.
        /// Input: valid payload with PurchaseOrderId = int.MinValue
        /// Expected: OkObjectResult returned, operations complete successfully
        /// </summary>
        [TestMethod]
        public void Update_MinIntegerPurchaseOrderId_ProcessesSuccessfully()
        {
            // Arrange
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = int.MinValue,
                PurchaseOrderName = "PO-MIN",
                Amount = 0,
                Total = 0,
                PurchaseOrderLines = new List<PurchaseOrderLine>()
            };

            var payload = new CrudViewModel<PurchaseOrder>
            {
                value = purchaseOrder
            };

            var mockPurchaseOrderDbSet = new Mock<DbSet<PurchaseOrder>>();
            var mockPurchaseOrderLineDbSet = new Mock<DbSet<PurchaseOrderLine>>();

            var purchaseOrders = new List<PurchaseOrder>().AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrders.Provider);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrders.Expression);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrders.ElementType);
            mockPurchaseOrderDbSet.As<IQueryable<PurchaseOrder>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrders.GetEnumerator());

            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Provider)
                .Returns(purchaseOrderLines.Provider);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.Expression)
                .Returns(purchaseOrderLines.Expression);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.ElementType)
                .Returns(purchaseOrderLines.ElementType);
            mockPurchaseOrderLineDbSet.As<IQueryable<PurchaseOrderLine>>()
                .Setup(m => m.GetEnumerator())
                .Returns(purchaseOrderLines.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseOrder).Returns(mockPurchaseOrderDbSet.Object);
            mockContext.Setup(c => c.PurchaseOrderLine).Returns(mockPurchaseOrderLineDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockPurchaseOrderDbSet.Verify(m => m.Update(purchaseOrder), Times.Once);
        }

        /// <summary>
        /// Tests that Remove method successfully removes an existing purchase order and returns OK result.
        /// Input: Valid payload with existing purchase order ID.
        /// Expected: Purchase order is removed, SaveChanges is called, and OK result is returned with the purchase order.
        /// </summary>
        [TestMethod]
        public void Remove_ValidPayloadWithExistingPurchaseOrder_ReturnsOkResultWithPurchaseOrder()
        {
            // Arrange
            var purchaseOrderId = 1;
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = purchaseOrderId,
                PurchaseOrderName = "PO-001",
                BranchId = 1,
                VendorId = 1
            };

            var purchaseOrders = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            var mockPurchaseOrderSet = CreateMockDbSet(purchaseOrders);
            var mockPurchaseOrderLineSet = CreateMockDbSet(purchaseOrderLines);
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();

            mockContext.Setup(m => m.PurchaseOrder).Returns(mockPurchaseOrderSet.Object);
            mockContext.Setup(m => m.PurchaseOrderLine).Returns(mockPurchaseOrderLineSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var payload = new CrudViewModel<PurchaseOrder>
            {
                key = purchaseOrderId
            };

            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(purchaseOrder, okResult.Value);
            mockPurchaseOrderSet.Verify(m => m.Remove(It.IsAny<PurchaseOrder>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Tests that Remove method handles payload with string key that can be converted to int.
        /// Input: Payload with key as string "42".
        /// Expected: String is converted to int and purchase order is queried.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithStringKey_ConvertsToIntAndProcesses()
        {
            // Arrange
            var purchaseOrderId = 42;
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = purchaseOrderId,
                PurchaseOrderName = "PO-042"
            };

            var purchaseOrders = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            var mockPurchaseOrderSet = CreateMockDbSet(purchaseOrders);
            var mockPurchaseOrderLineSet = CreateMockDbSet(purchaseOrderLines);
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();

            mockContext.Setup(m => m.PurchaseOrder).Returns(mockPurchaseOrderSet.Object);
            mockContext.Setup(m => m.PurchaseOrderLine).Returns(mockPurchaseOrderLineSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var payload = new CrudViewModel<PurchaseOrder>
            {
                key = "42"
            };

            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            mockPurchaseOrderSet.Verify(m => m.Remove(It.IsAny<PurchaseOrder>()), Times.Once);
        }

        /// <summary>
        /// Tests that Remove method verifies SaveChanges is called after removal.
        /// Input: Valid payload with existing purchase order.
        /// Expected: SaveChanges is called to persist the removal.
        /// </summary>
        [TestMethod]
        public void Remove_ValidPayload_CallsSaveChanges()
        {
            // Arrange
            var purchaseOrderId = 5;
            var purchaseOrder = new PurchaseOrder
            {
                PurchaseOrderId = purchaseOrderId,
                PurchaseOrderName = "PO-005"
            };

            var purchaseOrders = new List<PurchaseOrder> { purchaseOrder }.AsQueryable();
            var purchaseOrderLines = new List<PurchaseOrderLine>().AsQueryable();

            var mockPurchaseOrderSet = CreateMockDbSet(purchaseOrders);
            var mockPurchaseOrderLineSet = CreateMockDbSet(purchaseOrderLines);
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();

            mockContext.Setup(m => m.PurchaseOrder).Returns(mockPurchaseOrderSet.Object);
            mockContext.Setup(m => m.PurchaseOrderLine).Returns(mockPurchaseOrderLineSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var payload = new CrudViewModel<PurchaseOrder>
            {
                key = purchaseOrderId
            };

            var controller = new PurchaseOrderController(mockContext.Object, mockNumberSequence.Object);

            // Act
            controller.Remove(payload);

            // Assert
            mockContext.Verify(m => m.SaveChanges(), Times.AtLeastOnce);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports LINQ operations.
        /// </summary>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

    }
}