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
    /// Unit tests for the ShipmentController.Update method.
    /// </summary>
    [TestClass]
    public class ShipmentControllerTests
    {
        /// <summary>
        /// Tests that Update returns OkObjectResult with the shipment when valid payload is provided.
        /// Input: Valid CrudViewModel with valid Shipment.
        /// Expected: Returns OkObjectResult containing the shipment, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkObjectResultWithShipment()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = 1,
                ShipmentName = "SHIP-001",
                SalesOrderId = 10,
                ShipmentDate = DateTimeOffset.Now,
                ShipmentTypeId = 1,
                WarehouseId = 5,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(shipment, okResult.Value);
            mockShipmentDbSet.Verify(db => db.Update(shipment), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles minimal shipment data correctly.
        /// Input: CrudViewModel with Shipment containing only required fields.
        /// Expected: Returns OkObjectResult, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_MinimalShipmentData_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = 0,
                ShipmentName = null,
                SalesOrderId = 0,
                ShipmentDate = default(DateTimeOffset),
                ShipmentTypeId = 0,
                WarehouseId = 0,
                IsFullShipment = false
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockShipmentDbSet.Verify(db => db.Update(shipment), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles extreme ShipmentId values correctly.
        /// Input: Shipment with extreme int values for ShipmentId.
        /// Expected: Returns OkObjectResult, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        public void Update_ExtremeShipmentIdValues_ReturnsOkObjectResult(int shipmentId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = shipmentId,
                ShipmentName = "SHIP-TEST",
                SalesOrderId = 1,
                ShipmentDate = DateTimeOffset.Now,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockShipmentDbSet.Verify(db => db.Update(shipment), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles various string values for ShipmentName.
        /// Input: Shipment with empty, whitespace, and special character ShipmentName.
        /// Expected: Returns OkObjectResult, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("SHIP-12345678901234567890123456789012345678901234567890")]
        [DataRow("SHIP\r\n\t")]
        [DataRow("<script>alert('xss')</script>")]
        public void Update_VariousShipmentNameValues_ReturnsOkObjectResult(string shipmentName)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = 1,
                ShipmentName = shipmentName,
                SalesOrderId = 1,
                ShipmentDate = DateTimeOffset.Now,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockShipmentDbSet.Verify(db => db.Update(shipment), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles extreme DateTimeOffset values correctly.
        /// Input: Shipment with MinValue and MaxValue DateTimeOffset.
        /// Expected: Returns OkObjectResult, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_ExtremeDateTimeOffsetValues_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = 1,
                ShipmentName = "SHIP-001",
                SalesOrderId = 1,
                ShipmentDate = DateTimeOffset.MinValue,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockShipmentDbSet.Verify(db => db.Update(shipment), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles SaveChanges returning zero (no records affected).
        /// Input: Valid payload, SaveChanges returns 0.
        /// Expected: Returns OkObjectResult with shipment regardless of affected count.
        /// </summary>
        [TestMethod]
        public void Update_SaveChangesReturnsZero_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = 1,
                ShipmentName = "SHIP-001",
                SalesOrderId = 1,
                ShipmentDate = DateTimeOffset.Now,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(shipment, okResult.Value);
        }

        /// <summary>
        /// Tests that Update properly passes the exact shipment instance to Update method.
        /// Input: Valid payload with specific shipment instance.
        /// Expected: The exact same instance is passed to DbSet.Update and returned in OkObjectResult.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_PassesExactShipmentInstance()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = 42,
                ShipmentName = "SHIP-042",
                SalesOrderId = 100,
                ShipmentDate = new DateTimeOffset(2023, 6, 15, 10, 30, 0, TimeSpan.Zero),
                ShipmentTypeId = 2,
                WarehouseId = 3,
                IsFullShipment = false
            };
            var payload = new CrudViewModel<Shipment>
            {
                action = "update",
                key = 42,
                value = shipment
            };
            Shipment capturedShipment = null;
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockShipmentDbSet.Setup(db => db.Update(It.IsAny<Shipment>())).Callback<Shipment>(s => capturedShipment = s);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.AreSame(shipment, capturedShipment);
            var okResult = (OkObjectResult)result;
            Assert.AreSame(shipment, okResult.Value);
        }

        /// <summary>
        /// Tests that Update handles negative values for all integer properties.
        /// Input: Shipment with negative values for SalesOrderId, ShipmentTypeId, WarehouseId.
        /// Expected: Returns OkObjectResult, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_NegativeIntegerValues_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = -999,
                ShipmentName = "SHIP-NEG",
                SalesOrderId = -1,
                ShipmentDate = DateTimeOffset.Now,
                ShipmentTypeId = -100,
                WarehouseId = -50,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockShipmentDbSet.Verify(db => db.Update(shipment), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles both true and false values for IsFullShipment.
        /// Input: Shipment with IsFullShipment set to true and false.
        /// Expected: Returns OkObjectResult for both cases.
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Update_IsFullShipmentBooleanValues_ReturnsOkObjectResult(bool isFullShipment)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var shipment = new Shipment
            {
                ShipmentId = 1,
                ShipmentName = "SHIP-001",
                SalesOrderId = 1,
                ShipmentDate = DateTimeOffset.Now,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = isFullShipment
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = shipment
            };
            mockContext.Setup(c => c.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockShipmentDbSet.Verify(db => db.Update(shipment), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method successfully adds a shipment with valid payload
        /// and returns OkObjectResult containing the shipment.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithShipment()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = 1,
                SalesOrderId = 100,
                ShipmentDate = DateTimeOffset.Now,
                ShipmentTypeId = 1,
                WarehouseId = 1,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                action = "insert",
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("DO")).Returns("DO-00001");
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(testShipment, okResult.Value);
            Assert.AreEqual("DO-00001", testShipment.ShipmentName);
            mockNumberSequence.Verify(x => x.GetNumberSequence("DO"), Times.Once);
            mockShipmentDbSet.Verify(x => x.Add(testShipment), Times.Once);
            mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method correctly assigns ShipmentName when GetNumberSequence returns empty string.
        /// </summary>
        [TestMethod]
        public void Insert_GetNumberSequenceReturnsEmptyString_AssignsEmptyStringToShipmentName()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = 1,
                SalesOrderId = 100
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("DO")).Returns(string.Empty);
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.AreEqual(string.Empty, testShipment.ShipmentName);
            mockNumberSequence.Verify(x => x.GetNumberSequence("DO"), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method correctly assigns ShipmentName when GetNumberSequence returns null.
        /// </summary>
        [TestMethod]
        public void Insert_GetNumberSequenceReturnsNull_AssignsNullToShipmentName()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = 1,
                SalesOrderId = 100
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("DO")).Returns((string)null);
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            Assert.IsNull(testShipment.ShipmentName);
            mockNumberSequence.Verify(x => x.GetNumberSequence("DO"), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method with various ShipmentName sequences generates correct shipment names.
        /// </summary>
        /// <param name = "generatedSequence">The sequence generated by the number sequence service.</param>
        [DataRow("DO-00001")]
        [DataRow("DO-99999")]
        [DataRow("DO-00000")]
        [DataRow("SHIP-12345")]
        [TestMethod]
        public void Insert_VariousNumberSequences_AssignsCorrectShipmentName(string generatedSequence)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = 1,
                SalesOrderId = 100
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("DO")).Returns(generatedSequence);
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(generatedSequence, testShipment.ShipmentName);
        }

        /// <summary>
        /// Tests that Insert method correctly handles shipment with all properties set to boundary values.
        /// </summary>
        [TestMethod]
        public void Insert_ShipmentWithBoundaryValues_SuccessfullyInsertsAndReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = int.MaxValue,
                SalesOrderId = int.MaxValue,
                ShipmentDate = DateTimeOffset.MaxValue,
                ShipmentTypeId = int.MaxValue,
                WarehouseId = int.MaxValue,
                IsFullShipment = false
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("DO")).Returns("DO-MAX");
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(testShipment, okResult.Value);
            Assert.AreEqual("DO-MAX", testShipment.ShipmentName);
        }

        /// <summary>
        /// Tests that Insert method correctly handles shipment with minimum boundary values.
        /// </summary>
        [TestMethod]
        public void Insert_ShipmentWithMinimumValues_SuccessfullyInsertsAndReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = int.MinValue,
                SalesOrderId = int.MinValue,
                ShipmentDate = DateTimeOffset.MinValue,
                ShipmentTypeId = int.MinValue,
                WarehouseId = int.MinValue,
                IsFullShipment = true
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("DO")).Returns("DO-MIN");
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(testShipment, okResult.Value);
            Assert.AreEqual("DO-MIN", testShipment.ShipmentName);
        }

        /// <summary>
        /// Tests that Insert method calls SaveChanges exactly once.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsSaveChangesOnce()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = 1
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("DO")).Returns("DO-00001");
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            controller.Insert(payload);
            // Assert
            mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method passes exactly "DO" as module parameter to GetNumberSequence.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_PassesCorrectModuleToGetNumberSequence()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockShipmentDbSet = new Mock<DbSet<Shipment>>();
            var testShipment = new Shipment
            {
                ShipmentId = 1
            };
            var payload = new CrudViewModel<Shipment>
            {
                value = testShipment
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence(It.IsAny<string>())).Returns("DO-00001");
            mockShipmentDbSet.Setup(x => x.Add(It.IsAny<Shipment>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Shipment>)null);
            mockContext.Setup(x => x.Shipment).Returns(mockShipmentDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            // Act
            controller.Insert(payload);
            // Assert
            mockNumberSequence.Verify(x => x.GetNumberSequence("DO"), Times.Once);
            mockNumberSequence.Verify(x => x.GetNumberSequence(It.Is<string>(s => s != "DO")), Times.Never);
        }

        /// <summary>
        /// Tests that Remove method successfully removes and returns a shipment when valid payload with existing shipment ID is provided.
        /// </summary>
        [TestMethod]
        public void Remove_ValidPayloadWithExistingShipment_ReturnsOkResultWithShipment()
        {
            // Arrange
            var shipments = new List<Shipment>
            {
                new Shipment
                {
                    ShipmentId = 1,
                    ShipmentName = "Shipment1"
                },
                new Shipment
                {
                    ShipmentId = 2,
                    ShipmentName = "Shipment2"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Shipment>>();
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Provider).Returns(shipments.Provider);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Expression).Returns(shipments.Expression);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.ElementType).Returns(shipments.ElementType);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.GetEnumerator()).Returns(shipments.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Shipment).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Shipment>
            {
                key = 1
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(Shipment));
            var shipment = okResult.Value as Shipment;
            Assert.AreEqual(1, shipment.ShipmentId);
            mockSet.Verify(m => m.Remove(It.IsAny<Shipment>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles null key by converting it to 0 and attempts to find shipment with ID 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_ConvertsToZeroAndSearches()
        {
            // Arrange
            var shipments = new List<Shipment>
            {
                new Shipment
                {
                    ShipmentId = 0,
                    ShipmentName = "ShipmentZero"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Shipment>>();
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Provider).Returns(shipments.Provider);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Expression).Returns(shipments.Expression);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.ElementType).Returns(shipments.ElementType);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.GetEnumerator()).Returns(shipments.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Shipment).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Shipment>
            {
                key = null
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Remove method handles boundary value int.MaxValue for key.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIntMaxValue_SearchesForShipmentWithMaxId()
        {
            // Arrange
            var shipments = new List<Shipment>
            {
                new Shipment
                {
                    ShipmentId = int.MaxValue,
                    ShipmentName = "MaxShipment"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Shipment>>();
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Provider).Returns(shipments.Provider);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Expression).Returns(shipments.Expression);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.ElementType).Returns(shipments.ElementType);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.GetEnumerator()).Returns(shipments.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Shipment).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Shipment>
            {
                key = int.MaxValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var shipment = okResult.Value as Shipment;
            Assert.AreEqual(int.MaxValue, shipment.ShipmentId);
        }

        /// <summary>
        /// Tests that Remove method handles boundary value int.MinValue for key.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIntMinValue_SearchesForShipmentWithMinId()
        {
            // Arrange
            var shipments = new List<Shipment>
            {
                new Shipment
                {
                    ShipmentId = int.MinValue,
                    ShipmentName = "MinShipment"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Shipment>>();
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Provider).Returns(shipments.Provider);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Expression).Returns(shipments.Expression);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.ElementType).Returns(shipments.ElementType);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.GetEnumerator()).Returns(shipments.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Shipment).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Shipment>
            {
                key = int.MinValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var shipment = okResult.Value as Shipment;
            Assert.AreEqual(int.MinValue, shipment.ShipmentId);
        }

        /// <summary>
        /// Tests that Remove method handles zero as key value.
        /// </summary>
        [TestMethod]
        public void Remove_KeyZero_SearchesForShipmentWithZeroId()
        {
            // Arrange
            var shipments = new List<Shipment>
            {
                new Shipment
                {
                    ShipmentId = 0,
                    ShipmentName = "ZeroShipment"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Shipment>>();
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Provider).Returns(shipments.Provider);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Expression).Returns(shipments.Expression);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.ElementType).Returns(shipments.ElementType);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.GetEnumerator()).Returns(shipments.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Shipment).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Shipment>
            {
                key = 0
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var shipment = okResult.Value as Shipment;
            Assert.AreEqual(0, shipment.ShipmentId);
        }

        /// <summary>
        /// Tests that Remove method handles negative key value.
        /// </summary>
        [TestMethod]
        public void Remove_NegativeKey_SearchesForShipmentWithNegativeId()
        {
            // Arrange
            var shipments = new List<Shipment>
            {
                new Shipment
                {
                    ShipmentId = -5,
                    ShipmentName = "NegativeShipment"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Shipment>>();
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Provider).Returns(shipments.Provider);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Expression).Returns(shipments.Expression);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.ElementType).Returns(shipments.ElementType);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.GetEnumerator()).Returns(shipments.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Shipment).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Shipment>
            {
                key = -5
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var shipment = okResult.Value as Shipment;
            Assert.AreEqual(-5, shipment.ShipmentId);
        }

        /// <summary>
        /// Tests that Remove method successfully converts numeric string key to integer.
        /// </summary>
        [TestMethod]
        public void Remove_NumericStringKey_ConvertsAndRemovesShipment()
        {
            // Arrange
            var shipments = new List<Shipment>
            {
                new Shipment
                {
                    ShipmentId = 42,
                    ShipmentName = "Shipment42"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Shipment>>();
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Provider).Returns(shipments.Provider);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.Expression).Returns(shipments.Expression);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.ElementType).Returns(shipments.ElementType);
            mockSet.As<IQueryable<Shipment>>().Setup(m => m.GetEnumerator()).Returns(shipments.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Shipment).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new ShipmentController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Shipment>
            {
                key = "42"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            var shipment = okResult.Value as Shipment;
            Assert.AreEqual(42, shipment.ShipmentId);
            mockSet.Verify(m => m.Remove(It.IsAny<Shipment>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

    }
}