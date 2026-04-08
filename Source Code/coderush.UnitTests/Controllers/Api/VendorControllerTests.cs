using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Http;
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
    /// Tests for the VendorController class.
    /// </summary>
    [TestClass]
    public class VendorControllerTests
    {
        /// <summary>
        /// Tests that Update method with a valid payload returns an OkObjectResult with the vendor.
        /// Input: Valid CrudViewModel containing a Vendor object.
        /// Expected: Returns OkObjectResult containing the same vendor.
        /// </summary>
        [TestMethod]
        public async Task Update_ValidPayload_ReturnsOkObjectResultWithVendor()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockVendorDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockVendorDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            Vendor vendor = new Vendor
            {
                VendorId = 1,
                VendorName = "Test Vendor",
                VendorTypeId = 1,
                Address = "123 Test St",
                City = "Test City",
                State = "TS",
                ZipCode = "12345",
                Phone = "123-456-7890",
                Email = "test@test.com",
                ContactPerson = "John Doe"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                action = "update",
                value = vendor
            };
            VendorController controller = new VendorController(mockContext.Object);
            // Act
            IActionResult result = await controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(vendor, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method calls Update on the context's Vendor DbSet.
        /// Input: Valid CrudViewModel containing a Vendor object.
        /// Expected: Update method is called once on the Vendor DbSet with the vendor.
        /// </summary>
        [TestMethod]
        public async Task Update_ValidPayload_CallsUpdateOnVendorDbSet()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockVendorDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockVendorDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            Vendor vendor = new Vendor
            {
                VendorId = 2,
                VendorName = "Another Vendor"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                action = "update",
                value = vendor
            };
            VendorController controller = new VendorController(mockContext.Object);
            // Act
            IActionResult result = await controller.Update(payload);
            // Assert
            mockVendorDbSet.Verify(db => db.Update(vendor), Times.Once);
        }

        /// <summary>
        /// Tests that Update method calls SaveChanges on the context.
        /// Input: Valid CrudViewModel containing a Vendor object.
        /// Expected: SaveChanges is called once on the context.
        /// </summary>
        [TestMethod]
        public async Task Update_ValidPayload_CallsSaveChanges()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockVendorDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockVendorDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            Vendor vendor = new Vendor
            {
                VendorId = 3,
                VendorName = "Third Vendor"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                action = "update",
                value = vendor
            };
            VendorController controller = new VendorController(mockContext.Object);
            // Act
            IActionResult result = await controller.Update(payload);
            // Assert
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Update method with null vendor value in payload processes without exception.
        /// Input: CrudViewModel with null value property.
        /// Expected: Returns OkObjectResult with null value.
        /// </summary>
        [TestMethod]
        public async Task Update_NullVendorInPayload_ReturnsOkWithNull()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockVendorDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockVendorDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(0);
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                action = "update",
                value = null
            };
            VendorController controller = new VendorController(mockContext.Object);
            // Act
            IActionResult result = await controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that Update method with vendor having boundary values processes correctly.
        /// Input: Vendor with VendorId = int.MaxValue and other edge case values.
        /// Expected: Returns OkObjectResult with the vendor.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue, "Vendor MaxValue")]
        [DataRow(int.MinValue, "Vendor MinValue")]
        [DataRow(0, "")]
        [DataRow(1, "A")]
        public async Task Update_VendorWithBoundaryValues_ReturnsOkResult(int vendorId, string vendorName)
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockVendorDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockVendorDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            Vendor vendor = new Vendor
            {
                VendorId = vendorId,
                VendorName = vendorName
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                action = "update",
                value = vendor
            };
            VendorController controller = new VendorController(mockContext.Object);
            // Act
            IActionResult result = await controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(vendor, okResult.Value);
            mockVendorDbSet.Verify(db => db.Update(vendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Update method with vendor having special characters in string fields processes correctly.
        /// Input: Vendor with special characters, whitespace, and long strings.
        /// Expected: Returns OkObjectResult with the vendor.
        /// </summary>
        [TestMethod]
        public async Task Update_VendorWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockVendorDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockVendorDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            Vendor vendor = new Vendor
            {
                VendorId = 100,
                VendorName = "Vendor with Special <>&\"' Characters",
                Address = "   ",
                City = "\t\n\r",
                State = "@@",
                ZipCode = "!@#$%",
                Phone = "(123) 456-7890 ext. 999",
                Email = "test+tag@sub.domain.com",
                ContactPerson = new string ('A', 1000)
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                action = "update",
                value = vendor
            };
            VendorController controller = new VendorController(mockContext.Object);
            // Act
            IActionResult result = await controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(vendor, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove returns Ok result with the removed vendor when a valid vendor ID is provided.
        /// Input: Valid payload with existing vendor ID.
        /// Expected: Returns OkObjectResult with the deleted vendor.
        /// </summary>
        [TestMethod]
        public async Task Remove_ValidVendorId_ReturnsOkResultWithVendor()
        {
            // Arrange
            var vendorId = 1;
            var vendor = new Vendor
            {
                VendorId = vendorId,
                VendorName = "Test Vendor"
            };
            var payload = new CrudViewModel<Vendor>
            {
                key = vendorId
            };
            var mockSet = new Mock<DbSet<Vendor>>();
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns(new ValueTask<Vendor?>(vendor));
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Vendor).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new VendorController(mockContext.Object);
            // Act
            var result = await controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendor, okResult.Value);
            mockSet.Verify(m => m.Remove(vendor), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles string key values by converting them to integers.
        /// Input: Payload with string representation of integer as key.
        /// Expected: Returns OkObjectResult with the deleted vendor.
        /// </summary>
        [TestMethod]
        public async Task Remove_StringKeyValue_ConvertsToIntAndReturnsOkResult()
        {
            // Arrange
            var vendorId = 5;
            var vendor = new Vendor
            {
                VendorId = vendorId,
                VendorName = "Vendor 5"
            };
            var payload = new CrudViewModel<Vendor>
            {
                key = "5"
            };
            var mockSet = new Mock<DbSet<Vendor>>();
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns(new ValueTask<Vendor?>(vendor));
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Vendor).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new VendorController(mockContext.Object);
            // Act
            var result = await controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(vendor), Times.Once);
            mockContext.Verify(m => m.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that Remove correctly removes vendor with boundary integer ID when it exists.
        /// Input: Payload with int.MaxValue as key and matching vendor exists.
        /// Expected: Returns OkObjectResult with the deleted vendor.
        /// </summary>
        [TestMethod]
        public async Task Remove_MaxIntVendorIdExists_ReturnsOkResult()
        {
            // Arrange
            var vendorId = int.MaxValue;
            var vendor = new Vendor
            {
                VendorId = vendorId,
                VendorName = "Max ID Vendor"
            };
            var payload = new CrudViewModel<Vendor>
            {
                key = vendorId
            };
            var mockSet = new Mock<DbSet<Vendor>>();
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns(new ValueTask<Vendor?>(vendor));
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Vendor).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new VendorController(mockContext.Object);
            // Act
            var result = await controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendor, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove correctly removes vendor with zero ID when it exists.
        /// Input: Payload with 0 as key and matching vendor exists.
        /// Expected: Returns OkObjectResult with the deleted vendor.
        /// </summary>
        [TestMethod]
        public async Task Remove_ZeroVendorIdExists_ReturnsOkResult()
        {
            // Arrange
            var vendorId = 0;
            var vendor = new Vendor
            {
                VendorId = vendorId,
                VendorName = "Zero ID Vendor"
            };
            var payload = new CrudViewModel<Vendor>
            {
                key = vendorId
            };
            var mockSet = new Mock<DbSet<Vendor>>();
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns(new ValueTask<Vendor?>(vendor));
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Vendor).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new VendorController(mockContext.Object);
            // Act
            var result = await controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendor, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove correctly removes vendor with negative ID when it exists.
        /// Input: Payload with negative integer as key and matching vendor exists.
        /// Expected: Returns OkObjectResult with the deleted vendor.
        /// </summary>
        [TestMethod]
        public async Task Remove_NegativeVendorIdExists_ReturnsOkResult()
        {
            // Arrange
            var vendorId = -5;
            var vendor = new Vendor
            {
                VendorId = vendorId,
                VendorName = "Negative ID Vendor"
            };
            var payload = new CrudViewModel<Vendor>
            {
                key = vendorId
            };
            var mockSet = new Mock<DbSet<Vendor>>();
            mockSet.Setup(m => m.FindAsync(It.IsAny<object[]>())).Returns(new ValueTask<Vendor?>(vendor));
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Vendor).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new VendorController(mockContext.Object);
            // Act
            var result = await controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendor, okResult.Value);
        }

        /// <summary>
        /// Helper method to create a mockable DbSet with LINQ query support.
        /// </summary>
        private static Mock<DbSet<Vendor>> CreateMockDbSet(IQueryable<Vendor> data)
        {
            var mockSet = new Mock<DbSet<Vendor>>();
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        /// <summary>
        /// Tests that Insert adds vendor to context, saves changes, and returns OkObjectResult with the vendor.
        /// Input: Valid payload with a vendor object.
        /// Expected: Vendor is added, SaveChanges is called, and OkObjectResult containing the vendor is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_ValidPayload_AddsVendorSavesChangesAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = "Test Vendor",
                VendorTypeId = 1,
                Address = "123 Main St",
                City = "Test City",
                State = "TS",
                ZipCode = "12345",
                Phone = "555-1234",
                Email = "test@vendor.com",
                ContactPerson = "John Doe"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(testVendor, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert correctly handles payload with minimal vendor data.
        /// Input: Payload with vendor containing only required fields.
        /// Expected: Vendor is added, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_MinimalVendorData_AddsVendorAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 0,
                VendorName = "Minimal Vendor"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert correctly handles vendor with special characters in string fields.
        /// Input: Payload with vendor containing special characters, unicode, and long strings.
        /// Expected: Vendor is added with all special characters preserved and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_VendorWithSpecialCharacters_AddsVendorAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = "Test <>&\"' Vendor 测试",
                Address = "123 Main St\r\n\tApt 4",
                City = "City's \"Best\"",
                Email = "test+tag@vendor-company.co.uk",
                ContactPerson = "O'Brien, José"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(testVendor, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert correctly handles vendor with empty strings in optional fields.
        /// Input: Payload with vendor containing empty strings for optional fields.
        /// Expected: Vendor is added with empty strings preserved and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_VendorWithEmptyStrings_AddsVendorAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = "Test Vendor",
                Address = "",
                City = "",
                State = "",
                ZipCode = "",
                Phone = "",
                Email = "",
                ContactPerson = ""
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert correctly handles vendor with whitespace-only strings.
        /// Input: Payload with vendor containing whitespace-only strings in fields.
        /// Expected: Vendor is added with whitespace preserved and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_VendorWithWhitespaceStrings_AddsVendorAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = "   ",
                Address = "  \t  ",
                City = " "
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert correctly handles vendor with boundary VendorId values.
        /// Input: Multiple test cases with VendorId at int boundaries (0, negative, max, min).
        /// Expected: Vendor is added with the specified VendorId and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(1)]
        public async Task Insert_VendorWithBoundaryVendorId_AddsVendorAndReturnsOkResult(int vendorId)
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = vendorId,
                VendorName = "Test Vendor"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert correctly handles vendor with boundary VendorTypeId values.
        /// Input: Multiple test cases with VendorTypeId at int boundaries.
        /// Expected: Vendor is added with the specified VendorTypeId and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        public async Task Insert_VendorWithBoundaryVendorTypeId_AddsVendorAndReturnsOkResult(int vendorTypeId)
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = "Test Vendor",
                VendorTypeId = vendorTypeId
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert adds null vendor when payload.value is null.
        /// Input: Payload with null value property.
        /// Expected: Add is called with null, SaveChanges is called, and OkObjectResult with null is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_PayloadWithNullValue_AddsNullAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = null
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that Insert correctly handles payload with additional CrudViewModel properties set.
        /// Input: Payload with action, key, and antiForgery properties populated.
        /// Expected: Only the value property is used, vendor is added, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_PayloadWithAdditionalProperties_UsesOnlyValueProperty()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = "Test Vendor"
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                action = "insert",
                key = 123,
                antiForgery = "token123",
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(testVendor, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert correctly handles vendor with very long string values.
        /// Input: Payload with vendor containing extremely long strings in all fields.
        /// Expected: Vendor is added with long strings preserved and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_VendorWithVeryLongStrings_AddsVendorAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            string longString = new string ('A', 10000);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = longString,
                Address = longString,
                City = longString,
                State = longString,
                ZipCode = longString,
                Phone = longString,
                Email = longString,
                ContactPerson = longString
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert correctly handles vendor with null values in optional string fields.
        /// Input: Payload with vendor containing null for optional string properties.
        /// Expected: Vendor is added with null values preserved and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public async Task Insert_VendorWithNullOptionalFields_AddsVendorAndReturnsOkResult()
        {
            // Arrange
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>();
            Mock<DbSet<Vendor>> mockDbSet = new Mock<DbSet<Vendor>>();
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            VendorController controller = new VendorController(mockContext.Object);
            Vendor testVendor = new Vendor
            {
                VendorId = 1,
                VendorName = "Test Vendor",
                Address = null,
                City = null,
                State = null,
                ZipCode = null,
                Phone = null,
                Email = null,
                ContactPerson = null
            };
            CrudViewModel<Vendor> payload = new CrudViewModel<Vendor>
            {
                value = testVendor
            };
            // Act
            IActionResult result = await controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(testVendor), Times.Once);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that GetVendor returns Ok result with empty list when no vendors exist.
        /// </summary>
        [TestMethod]
        public async Task GetVendor_EmptyDatabase_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyVendorList = new List<Vendor>();
            var mockDbSet = CreateMockDbSet(emptyVendorList);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            var controller = new VendorController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            // Act
            var result = await controller.GetVendor();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = (List<Vendor>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);
            Assert.AreEqual(0, count);
            Assert.AreEqual(0, items.Count);
        }

        /// <summary>
        /// Tests that GetVendor returns Ok result with single vendor when one vendor exists.
        /// </summary>
        [TestMethod]
        public async Task GetVendor_SingleVendor_ReturnsOkWithOneItem()
        {
            // Arrange
            var vendorList = new List<Vendor>
            {
                new Vendor
                {
                    VendorId = 1,
                    VendorName = "Test Vendor",
                    VendorTypeId = 1,
                    Address = "123 Main St",
                    City = "TestCity",
                    State = "TS",
                    ZipCode = "12345",
                    Phone = "555-0100",
                    Email = "test@example.com",
                    ContactPerson = "John Doe"
                }
            };
            var mockDbSet = CreateMockDbSet(vendorList);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            var controller = new VendorController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            // Act
            var result = await controller.GetVendor();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            var items = (List<Vendor>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("Test Vendor", items[0].VendorName);
            Assert.AreEqual(1, items[0].VendorId);
        }

        /// <summary>
        /// Tests that GetVendor returns Ok result with multiple vendors when multiple vendors exist.
        /// </summary>
        [TestMethod]
        public async Task GetVendor_MultipleVendors_ReturnsOkWithAllItems()
        {
            // Arrange
            var vendorList = new List<Vendor>
            {
                new Vendor
                {
                    VendorId = 1,
                    VendorName = "Vendor A",
                    VendorTypeId = 1,
                    Address = "123 Main St",
                    City = "City A",
                    State = "AA",
                    ZipCode = "11111",
                    Phone = "555-0100",
                    Email = "a@example.com",
                    ContactPerson = "Person A"
                },
                new Vendor
                {
                    VendorId = 2,
                    VendorName = "Vendor B",
                    VendorTypeId = 2,
                    Address = "456 Oak Ave",
                    City = "City B",
                    State = "BB",
                    ZipCode = "22222",
                    Phone = "555-0200",
                    Email = "b@example.com",
                    ContactPerson = "Person B"
                },
                new Vendor
                {
                    VendorId = 3,
                    VendorName = "Vendor C",
                    VendorTypeId = 1,
                    Address = "789 Pine Rd",
                    City = "City C",
                    State = "CC",
                    ZipCode = "33333",
                    Phone = "555-0300",
                    Email = "c@example.com",
                    ContactPerson = "Person C"
                }
            };
            var mockDbSet = CreateMockDbSet(vendorList);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Vendor).Returns(mockDbSet.Object);
            var controller = new VendorController(mockContext.Object);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
            // Act
            var result = await controller.GetVendor();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            var items = (List<Vendor>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);
            Assert.AreEqual(3, count);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual("Vendor A", items[0].VendorName);
            Assert.AreEqual("Vendor B", items[1].VendorName);
            Assert.AreEqual("Vendor C", items[2].VendorName);
        }

        /// <summary>
        /// Helper method to create a mock DbSet for testing with async enumeration support.
        /// </summary>
        /// <param name = "sourceList">The source data to use for the mock DbSet.</param>
        /// <returns>A mock DbSet configured for async operations.</returns>
        private Mock<DbSet<Vendor>> CreateMockDbSet(List<Vendor> sourceList)
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<Vendor>>();
            mockSet.As<IAsyncEnumerable<Vendor>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Vendor>(queryable.GetEnumerator()));
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Vendor>(queryable.Provider));
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Vendor>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return mockSet;
        }

        /// <summary>
        /// Test async query provider for Entity Framework Core async operations.
        /// </summary>
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider).GetMethod(name: nameof(IQueryProvider.Execute), genericParameterCount: 1, types: new[] { typeof(Expression) }).MakeGenericMethod(resultType).Invoke(this, new[] { expression });
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(resultType).Invoke(null, new[] { executionResult });
            }
        }

        /// <summary>
        /// Test async enumerable for Entity Framework Core async operations.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
            {
            }

            public TestAsyncEnumerable(Expression expression) : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider
            {
                get
                {
                    return new TestAsyncQueryProvider<T>(this);
                }
            }
        }

        /// <summary>
        /// Test async enumerator for Entity Framework Core async operations.
        /// </summary>
        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public T Current
            {
                get
                {
                    return _inner.Current;
                }
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }
        }
    }
}