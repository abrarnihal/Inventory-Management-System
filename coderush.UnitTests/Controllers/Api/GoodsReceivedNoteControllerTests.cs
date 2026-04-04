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
using System.Threading;
using System.Threading.Tasks;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the GoodsReceivedNoteController class.
    /// </summary>
    [TestClass]
    public class GoodsReceivedNoteControllerTests
    {
        /// <summary>
        /// Tests that Insert method successfully adds a GoodsReceivedNote with valid payload,
        /// generates a number sequence, saves to database, and returns Ok result.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkWithGoodsReceivedNote()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            var expectedSequence = "GRN-001";
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns(expectedSequence);
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                VendorDONumber = "DO-123",
                VendorInvoiceNumber = "INV-456",
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "insert",
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsInstanceOfType(okResult.Value, typeof(GoodsReceivedNote));
            var returnedNote = okResult.Value as GoodsReceivedNote;
            Assert.IsNotNull(returnedNote);
            Assert.AreEqual(expectedSequence, returnedNote.GoodsReceivedNoteName);
            mockNumberSequence.Verify(x => x.GetNumberSequence("GRN"), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method correctly assigns the generated number sequence
        /// to the GoodsReceivedNoteName property.
        /// </summary>
        [TestMethod]
        [DataRow("GRN-001")]
        [DataRow("GRN-999")]
        [DataRow("")]
        [DataRow("   ")]
        public void Insert_VariousNumberSequences_AssignsCorrectly(string numberSequence)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns(numberSequence);
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                WarehouseId = 1
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedNote = okResult.Value as GoodsReceivedNote;
            Assert.IsNotNull(returnedNote);
            Assert.AreEqual(numberSequence, returnedNote.GoodsReceivedNoteName);
        }

        /// <summary>
        /// Tests that Insert method correctly handles GoodsReceivedNote with boundary date values.
        /// </summary>
        [TestMethod]
        public void Insert_BoundaryDateValues_SavesSuccessfully()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.MinValue,
                WarehouseId = 1
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method correctly handles GoodsReceivedNote with boundary integer values.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        public void Insert_BoundaryIntegerValues_SavesSuccessfully(int value)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = value,
                GRNDate = DateTimeOffset.UtcNow,
                WarehouseId = value
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method correctly handles GoodsReceivedNote with null string properties.
        /// </summary>
        [TestMethod]
        public void Insert_NullStringProperties_SavesSuccessfully()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                VendorDONumber = null,
                VendorInvoiceNumber = null,
                WarehouseId = 1
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedNote = okResult.Value as GoodsReceivedNote;
            Assert.IsNotNull(returnedNote);
            Assert.AreEqual("GRN-001", returnedNote.GoodsReceivedNoteName);
        }

        /// <summary>
        /// Tests that Insert method correctly handles GoodsReceivedNote with empty string properties.
        /// </summary>
        [TestMethod]
        public void Insert_EmptyStringProperties_SavesSuccessfully()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                VendorDONumber = string.Empty,
                VendorInvoiceNumber = string.Empty,
                WarehouseId = 1
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method correctly handles GoodsReceivedNote with very long string properties.
        /// </summary>
        [TestMethod]
        public void Insert_VeryLongStringProperties_SavesSuccessfully()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var longString = new string ('A', 10000);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                VendorDONumber = longString,
                VendorInvoiceNumber = longString,
                WarehouseId = 1
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method correctly handles GoodsReceivedNote with special characters in string properties.
        /// </summary>
        [TestMethod]
        public void Insert_SpecialCharactersInStrings_SavesSuccessfully()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                VendorDONumber = "!@#$%^&*()_+-=[]{}|;':\"<>?,./",
                VendorInvoiceNumber = "\r\n\t\0",
                WarehouseId = 1
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method correctly handles IsFullReceive flag with both true and false values.
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Insert_IsFullReceiveFlag_SavesCorrectly(bool isFullReceive)
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                WarehouseId = 1,
                IsFullReceive = isFullReceive
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedNote = okResult.Value as GoodsReceivedNote;
            Assert.IsNotNull(returnedNote);
            Assert.AreEqual(isFullReceive, returnedNote.IsFullReceive);
        }

        /// <summary>
        /// Tests that Insert method verifies GetNumberSequence is called with exactly "GRN" parameter.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsGetNumberSequenceWithGRN()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()).Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("GRN")).Returns("GRN-001");
            var controller = new GoodsReceivedNoteController(context, mockNumberSequence.Object);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.UtcNow,
                WarehouseId = 1
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                value = goodsReceivedNote
            };
            // Act
            controller.Insert(payload);
            // Assert
            mockNumberSequence.Verify(x => x.GetNumberSequence("GRN"), Times.Once);
            mockNumberSequence.VerifyNoOtherCalls();
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing entity and returns Ok result.
        /// Input: Valid payload with key matching an existing GoodsReceivedNote.
        /// Expected: Entity is removed, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKeyWithExistingEntity_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            var goodsReceivedNoteId = 42;
            var existingEntity = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = goodsReceivedNoteId,
                GoodsReceivedNoteName = "GRN-001"
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                key = goodsReceivedNoteId
            };
            var mockSet = new Mock<DbSet<GoodsReceivedNote>>();
            var data = new List<GoodsReceivedNote>
            {
                existingEntity
            }.AsQueryable();
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(existingEntity, okResult.Value);
            mockSet.Verify(m => m.Remove(existingEntity), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles entity not found scenario by attempting to remove null.
        /// Input: Valid payload with key that doesn't match any existing entity.
        /// Expected: Remove is called with null, SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKeyWithNonExistingEntity_RemovesNullAndReturnsOk()
        {
            // Arrange
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                key = 999
            };
            var mockSet = new Mock<DbSet<GoodsReceivedNote>>();
            var data = new List<GoodsReceivedNote>().AsQueryable();
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<GoodsReceivedNote>()), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles null key by converting to 0 and attempting to find entity with ID 0.
        /// Input: Payload with null key.
        /// Expected: Convert.ToInt32(null) returns 0, query executes for ID 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_ConvertsToZeroAndProcesses()
        {
            // Arrange
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                key = null
            };
            var mockSet = new Mock<DbSet<GoodsReceivedNote>>();
            var data = new List<GoodsReceivedNote>().AsQueryable();
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles string key that can be converted to int.
        /// Input: Payload with string key "123".
        /// Expected: String is converted to int 123 and entity with that ID is searched.
        /// </summary>
        [TestMethod]
        public void Remove_StringKeyConvertibleToInt_ConvertsAndProcesses()
        {
            // Arrange
            var existingEntity = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 123,
                GoodsReceivedNoteName = "GRN-123"
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                key = "123"
            };
            var mockSet = new Mock<DbSet<GoodsReceivedNote>>();
            var data = new List<GoodsReceivedNote>
            {
                existingEntity
            }.AsQueryable();
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(existingEntity), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles boundary value int.MaxValue.
        /// Input: Payload with key equal to int.MaxValue.
        /// Expected: Query executes for int.MaxValue, no exception is thrown.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        [DataRow(0)]
        [DataRow(-1)]
        public void Remove_BoundaryIntValues_ProcessesSuccessfully(int keyValue)
        {
            // Arrange
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                key = keyValue
            };
            var mockSet = new Mock<DbSet<GoodsReceivedNote>>();
            var data = new List<GoodsReceivedNote>().AsQueryable();
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles double key value by converting to int.
        /// Input: Payload with double key 123.45.
        /// Expected: Double is converted to int 123 and processing continues.
        /// </summary>
        [TestMethod]
        public void Remove_DoubleKey_ConvertsToIntAndProcesses()
        {
            // Arrange
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                key = 123.45
            };
            var mockSet = new Mock<DbSet<GoodsReceivedNote>>();
            var data = new List<GoodsReceivedNote>().AsQueryable();
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<GoodsReceivedNote>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method successfully updates a GoodsReceivedNote with valid payload and returns OkObjectResult.
        /// Input: Valid CrudViewModel with a valid GoodsReceivedNote.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned with the correct entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithUpdatedEntity()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = "GRN-001",
                PurchaseOrderId = 100,
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = "DO-001",
                VendorInvoiceNumber = "INV-001",
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(goodsReceivedNote, okResult.Value);
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles null value in payload and calls Update with null.
        /// Input: Valid CrudViewModel with null value property.
        /// Expected: Update is called with null and SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Update_NullPayloadValue_UpdatesWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = null
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(d => d.Update(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly handles GoodsReceivedNote with minimum integer ID.
        /// Input: CrudViewModel with GoodsReceivedNote having GoodsReceivedNoteId = int.MinValue.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithMinIntId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = int.MinValue,
                GoodsReceivedNoteName = "GRN-MIN",
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.MinValue,
                VendorDONumber = "DO-MIN",
                VendorInvoiceNumber = "INV-MIN",
                WarehouseId = 1,
                IsFullReceive = false
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = int.MinValue,
                antiForgery = "token",
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly handles GoodsReceivedNote with maximum integer ID.
        /// Input: CrudViewModel with GoodsReceivedNote having GoodsReceivedNoteId = int.MaxValue.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithMaxIntId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = int.MaxValue,
                GoodsReceivedNoteName = "GRN-MAX",
                PurchaseOrderId = int.MaxValue,
                GRNDate = DateTimeOffset.MaxValue,
                VendorDONumber = "DO-MAX",
                VendorInvoiceNumber = "INV-MAX",
                WarehouseId = int.MaxValue,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = int.MaxValue,
                antiForgery = "token",
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles GoodsReceivedNote with empty string properties.
        /// Input: CrudViewModel with GoodsReceivedNote having empty string values for string properties.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = string.Empty,
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = string.Empty,
                VendorInvoiceNumber = string.Empty,
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = 1,
                antiForgery = string.Empty,
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles GoodsReceivedNote with null string properties.
        /// Input: CrudViewModel with GoodsReceivedNote having null string values for string properties.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithNullStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = null,
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = null,
                VendorInvoiceNumber = null,
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = 1,
                antiForgery = null,
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles GoodsReceivedNote with very long string values.
        /// Input: CrudViewModel with GoodsReceivedNote having very long strings for string properties.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var longString = new string ('A', 10000);
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = longString,
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = longString,
                VendorInvoiceNumber = longString,
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = longString,
                key = 1,
                antiForgery = longString,
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles GoodsReceivedNote with special characters in string properties.
        /// Input: CrudViewModel with GoodsReceivedNote having special characters in string properties.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var specialString = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~\t\n\r";
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = specialString,
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = specialString,
                VendorInvoiceNumber = specialString,
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = specialString,
                key = 1,
                antiForgery = specialString,
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles GoodsReceivedNote with zero and negative IDs.
        /// Input: CrudViewModel with GoodsReceivedNote having zero and negative values for ID properties.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        [DataRow(0, 0, 0)]
        [DataRow(-1, -1, -1)]
        [DataRow(-100, -200, -300)]
        public void Update_GoodsReceivedNoteWithBoundaryIds_ReturnsOkResult(int grnId, int poId, int warehouseId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = grnId,
                GoodsReceivedNoteName = "GRN-Test",
                PurchaseOrderId = poId,
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = "DO-Test",
                VendorInvoiceNumber = "INV-Test",
                WarehouseId = warehouseId,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = grnId,
                antiForgery = "token",
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles both IsFullReceive boolean values.
        /// Input: CrudViewModel with GoodsReceivedNote having true and false IsFullReceive values.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Update_GoodsReceivedNoteWithBooleanValues_ReturnsOkResult(bool isFullReceive)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = "GRN-Test",
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.Now,
                VendorDONumber = "DO-Test",
                VendorInvoiceNumber = "INV-Test",
                WarehouseId = 1,
                IsFullReceive = isFullReceive
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var returnedNote = (GoodsReceivedNote)okResult.Value;
            Assert.AreEqual(isFullReceive, returnedNote.IsFullReceive);
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles GoodsReceivedNote with boundary DateTimeOffset values.
        /// Input: CrudViewModel with GoodsReceivedNote having DateTimeOffset.MinValue and DateTimeOffset.MaxValue.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithMinDateTimeOffset_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = "GRN-Min",
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.MinValue,
                VendorDONumber = "DO-Min",
                VendorInvoiceNumber = "INV-Min",
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles GoodsReceivedNote with DateTimeOffset.MaxValue.
        /// Input: CrudViewModel with GoodsReceivedNote having DateTimeOffset.MaxValue.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_GoodsReceivedNoteWithMaxDateTimeOffset_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<GoodsReceivedNote>>();
            var goodsReceivedNote = new GoodsReceivedNote
            {
                GoodsReceivedNoteId = 1,
                GoodsReceivedNoteName = "GRN-Max",
                PurchaseOrderId = 1,
                GRNDate = DateTimeOffset.MaxValue,
                VendorDONumber = "DO-Max",
                VendorInvoiceNumber = "INV-Max",
                WarehouseId = 1,
                IsFullReceive = true
            };
            var payload = new CrudViewModel<GoodsReceivedNote>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = goodsReceivedNote
            };
            mockContext.Setup(c => c.GoodsReceivedNote).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new GoodsReceivedNoteController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(goodsReceivedNote), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }
    }
}