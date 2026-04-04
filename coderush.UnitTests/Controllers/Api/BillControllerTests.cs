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
    /// Unit tests for the BillController class.
    /// </summary>
    [TestClass]
    public class BillControllerTests
    {
        /// <summary>
        /// Tests that Update successfully updates a bill and returns OkObjectResult with valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithBill()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 1,
                BillName = "BILL-001",
                GoodsReceivedNoteId = 100,
                VendorDONumber = "VDO-001",
                VendorInvoiceNumber = "VINV-001",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "update",
                key = 1,
                value = bill
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(bill, okResult.Value);
            mockDbSet.Verify(d => d.Update(bill), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles null bill value in payload.
        /// </summary>
        [TestMethod]
        public void Update_NullBillValue_CallsUpdateWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill>
            {
                action = "update",
                key = 1,
                value = null
            };

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
        /// Tests that Update handles bill with minimum valid properties.
        /// </summary>
        [TestMethod]
        public void Update_BillWithMinimumProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 0,
                BillName = null,
                VendorDONumber = null,
                VendorInvoiceNumber = null
            };

            var payload = new CrudViewModel<Bill>
            {
                value = bill
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(bill), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles bill with extreme BillId values.
        /// </summary>
        /// <param name="billId">The bill ID to test.</param>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(1)]
        public void Update_BillWithExtremeBillId_ReturnsOkResult(int billId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = billId,
                BillName = "BILL-001",
                GoodsReceivedNoteId = 1,
                VendorDONumber = "VDO-001",
                VendorInvoiceNumber = "VINV-001",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                value = bill
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(bill, okResult.Value);
            mockDbSet.Verify(d => d.Update(bill), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles bill with empty string properties.
        /// </summary>
        [TestMethod]
        public void Update_BillWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 1,
                BillName = "",
                VendorDONumber = "",
                VendorInvoiceNumber = "",
                GoodsReceivedNoteId = 1,
                BillDate = DateTimeOffset.MinValue,
                BillDueDate = DateTimeOffset.MaxValue,
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                value = bill
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(bill), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles bill with very long string values.
        /// </summary>
        [TestMethod]
        public void Update_BillWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var longString = new string('A', 10000);
            var bill = new Bill
            {
                BillId = 1,
                BillName = longString,
                VendorDONumber = longString,
                VendorInvoiceNumber = longString,
                GoodsReceivedNoteId = 1,
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now,
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                value = bill
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(bill), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles bill with special characters in string properties.
        /// </summary>
        [TestMethod]
        public void Update_BillWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 1,
                BillName = "BILL-001<>&\"'",
                VendorDONumber = "VDO\n\r\t\0",
                VendorInvoiceNumber = "VINV-001 !@#$%^&*()",
                GoodsReceivedNoteId = 1,
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now,
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                value = bill
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(bill), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update returns 200 OK status code.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_Returns200StatusCode()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Bill>>();
            mockContext.Setup(c => c.Bill).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var bill = new Bill { BillId = 1 };
            var payload = new CrudViewModel<Bill> { value = bill };

            // Act
            var result = controller.Update(payload) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        /// <summary>
        /// Tests Insert method with valid payload.
        /// Should add bill to context, save changes, assign BillName from number sequence, and return Ok result.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithBill()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-001");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 1,
                GoodsReceivedNoteId = 100,
                VendorDONumber = "VDO-001",
                VendorInvoiceNumber = "INV-001",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-001", returnedBill.BillName);
            Assert.AreEqual(1, context.Bill.Count());

            mockNumberSequence.Verify(x => x.GetNumberSequence("BILL"), Times.Once);
        }

        /// <summary>
        /// Tests Insert method when number sequence returns null.
        /// Should set BillName to null and complete the operation successfully.
        /// </summary>
        [TestMethod]
        public void Insert_NumberSequenceReturnsNull_SetsBillNameToNull()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns((string)null);

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 1,
                GoodsReceivedNoteId = 100,
                VendorDONumber = "VDO-001",
                VendorInvoiceNumber = "INV-001",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.IsNull(returnedBill.BillName);
        }

        /// <summary>
        /// Tests Insert method when number sequence returns empty string.
        /// Should set BillName to empty string and complete the operation successfully.
        /// </summary>
        [TestMethod]
        public void Insert_NumberSequenceReturnsEmpty_SetsBillNameToEmpty()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns(string.Empty);

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 1,
                GoodsReceivedNoteId = 100,
                VendorDONumber = "VDO-001",
                VendorInvoiceNumber = "INV-001",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 1
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual(string.Empty, returnedBill.BillName);
        }

        /// <summary>
        /// Tests Insert method with bill having minimum integer values.
        /// Should handle edge case numeric values correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithMinIntegerValues_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-MIN");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = int.MinValue,
                GoodsReceivedNoteId = int.MinValue,
                VendorDONumber = "VDO-MIN",
                VendorInvoiceNumber = "INV-MIN",
                BillDate = DateTimeOffset.MinValue,
                BillDueDate = DateTimeOffset.MinValue,
                BillTypeId = int.MinValue
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-MIN", returnedBill.BillName);
        }

        /// <summary>
        /// Tests Insert method with bill having maximum integer values.
        /// Should handle edge case numeric values correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithMaxIntegerValues_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-MAX");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = int.MaxValue,
                GoodsReceivedNoteId = int.MaxValue,
                VendorDONumber = "VDO-MAX",
                VendorInvoiceNumber = "INV-MAX",
                BillDate = DateTimeOffset.MaxValue,
                BillDueDate = DateTimeOffset.MaxValue,
                BillTypeId = int.MaxValue
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-MAX", returnedBill.BillName);
        }

        /// <summary>
        /// Tests Insert method with bill having null string properties.
        /// Should handle null string values correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithNullStringProperties_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-002");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 2,
                GoodsReceivedNoteId = 200,
                VendorDONumber = null,
                VendorInvoiceNumber = null,
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 2
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-002", returnedBill.BillName);
            Assert.IsNull(returnedBill.VendorDONumber);
            Assert.IsNull(returnedBill.VendorInvoiceNumber);
        }

        /// <summary>
        /// Tests Insert method with bill having empty string properties.
        /// Should handle empty string values correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithEmptyStringProperties_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-003");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 3,
                GoodsReceivedNoteId = 300,
                VendorDONumber = string.Empty,
                VendorInvoiceNumber = string.Empty,
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 3
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-003", returnedBill.BillName);
            Assert.AreEqual(string.Empty, returnedBill.VendorDONumber);
            Assert.AreEqual(string.Empty, returnedBill.VendorInvoiceNumber);
        }

        /// <summary>
        /// Tests Insert method with bill having very long string properties.
        /// Should handle very long string values correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithVeryLongStringProperties_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-004");

            var controller = new BillController(context, mockNumberSequence.Object);

            var longString = new string('A', 10000);

            var bill = new Bill
            {
                BillId = 4,
                GoodsReceivedNoteId = 400,
                VendorDONumber = longString,
                VendorInvoiceNumber = longString,
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 4
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-004", returnedBill.BillName);
            Assert.AreEqual(longString, returnedBill.VendorDONumber);
        }

        /// <summary>
        /// Tests Insert method with bill having special characters in string properties.
        /// Should handle special characters correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithSpecialCharactersInStrings_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-005");

            var controller = new BillController(context, mockNumberSequence.Object);

            var specialString = "!@#$%^&*()_+-=[]{}|;':\",./<>?`~";

            var bill = new Bill
            {
                BillId = 5,
                GoodsReceivedNoteId = 500,
                VendorDONumber = specialString,
                VendorInvoiceNumber = specialString,
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 5
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-005", returnedBill.BillName);
            Assert.AreEqual(specialString, returnedBill.VendorDONumber);
        }

        /// <summary>
        /// Tests Insert method verifies SaveChanges is called.
        /// Should call SaveChanges exactly once to persist the bill.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsSaveChangesOnce()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-006");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 6,
                GoodsReceivedNoteId = 600,
                VendorDONumber = "VDO-006",
                VendorInvoiceNumber = "INV-006",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 6
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, context.Bill.Count());
            var savedBill = context.Bill.First();
            Assert.AreEqual("BILL-006", savedBill.BillName);
        }

        /// <summary>
        /// Tests Insert method with zero values for integer properties.
        /// Should handle zero values correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithZeroValues_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-007");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 0,
                GoodsReceivedNoteId = 0,
                VendorDONumber = "VDO-007",
                VendorInvoiceNumber = "INV-007",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 0
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-007", returnedBill.BillName);
            Assert.AreEqual(1, returnedBill.BillId);
            Assert.AreEqual(0, returnedBill.GoodsReceivedNoteId);
            Assert.AreEqual(0, returnedBill.BillTypeId);
        }

        /// <summary>
        /// Tests Insert method with whitespace-only string properties.
        /// Should handle whitespace strings correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BillWithWhitespaceStringProperties_ReturnsOkResult()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var context = new ApplicationDbContext(options);
            var mockNumberSequence = new Mock<INumberSequence>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("BILL")).Returns("BILL-008");

            var controller = new BillController(context, mockNumberSequence.Object);

            var bill = new Bill
            {
                BillId = 8,
                GoodsReceivedNoteId = 800,
                VendorDONumber = "   ",
                VendorInvoiceNumber = "\t\n\r",
                BillDate = DateTimeOffset.Now,
                BillDueDate = DateTimeOffset.Now.AddDays(30),
                BillTypeId = 8
            };

            var payload = new CrudViewModel<Bill>
            {
                action = "insert",
                value = bill
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var returnedBill = okResult.Value as Bill;
            Assert.IsNotNull(returnedBill);
            Assert.AreEqual("BILL-008", returnedBill.BillName);
            Assert.AreEqual("   ", returnedBill.VendorDONumber);
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing bill and returns OkObjectResult with the removed bill.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKeyAndBillExists_ReturnsOkWithRemovedBill()
        {
            // Arrange
            var billId = 123;
            var bill = new Bill { BillId = billId, BillName = "Test Bill" };
            var bills = new List<Bill> { bill };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = billId.ToString() };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill, okResult.Value);
            mockSet.Verify(m => m.Remove(bill), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles null key by converting it to 0 and querying with BillId = 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_ConvertsToZeroAndQueriesBillWithIdZero()
        {
            // Arrange
            var bill = new Bill { BillId = 0, BillName = "Bill with ID 0" };
            var bills = new List<Bill> { bill };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = null };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill, okResult.Value);
            mockSet.Verify(m => m.Remove(bill), Times.Once);
        }

        /// <summary>
        /// Tests that Remove works correctly with boundary value int.MaxValue as key.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsIntMaxValue_RemovesBillSuccessfully()
        {
            // Arrange
            var billId = int.MaxValue;
            var bill = new Bill { BillId = billId, BillName = "Max Value Bill" };
            var bills = new List<Bill> { bill };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = int.MaxValue.ToString() };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill, okResult.Value);
            mockSet.Verify(m => m.Remove(bill), Times.Once);
        }

        /// <summary>
        /// Tests that Remove works correctly with boundary value int.MinValue as key.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsIntMinValue_RemovesBillSuccessfully()
        {
            // Arrange
            var billId = int.MinValue;
            var bill = new Bill { BillId = billId, BillName = "Min Value Bill" };
            var bills = new List<Bill> { bill };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = int.MinValue.ToString() };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill, okResult.Value);
            mockSet.Verify(m => m.Remove(bill), Times.Once);
        }

        /// <summary>
        /// Tests that Remove works correctly with negative key value.
        /// </summary>
        [TestMethod]
        public void Remove_NegativeKey_RemovesBillSuccessfully()
        {
            // Arrange
            var billId = -42;
            var bill = new Bill { BillId = billId, BillName = "Negative ID Bill" };
            var bills = new List<Bill> { bill };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = billId.ToString() };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove handles key as integer object correctly.
        /// </summary>
        [TestMethod]
        public void Remove_KeyAsIntegerObject_RemovesBillSuccessfully()
        {
            // Arrange
            var billId = 456;
            var bill = new Bill { BillId = billId, BillName = "Integer Key Bill" };
            var bills = new List<Bill> { bill };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = billId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove finds and removes the correct bill when multiple bills exist.
        /// </summary>
        [TestMethod]
        public void Remove_MultipleBillsExist_RemovesCorrectBill()
        {
            // Arrange
            var targetBillId = 5;
            var bill1 = new Bill { BillId = 3, BillName = "Bill 3" };
            var bill2 = new Bill { BillId = targetBillId, BillName = "Bill 5" };
            var bill3 = new Bill { BillId = 7, BillName = "Bill 7" };
            var bills = new List<Bill> { bill1, bill2, bill3 };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = targetBillId.ToString() };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill2, okResult.Value);
            mockSet.Verify(m => m.Remove(bill2), Times.Once);
            mockSet.Verify(m => m.Remove(bill1), Times.Never);
            mockSet.Verify(m => m.Remove(bill3), Times.Never);
        }

        /// <summary>
        /// Tests that Remove correctly handles zero as key value.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsZero_RemovesBillWithIdZero()
        {
            // Arrange
            var bill = new Bill { BillId = 0, BillName = "Zero ID Bill" };
            var bills = new List<Bill> { bill };

            var mockSet = CreateMockDbSet(bills.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Bill).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new BillController(mockContext.Object, mockNumberSequence.Object);

            var payload = new CrudViewModel<Bill> { key = "0" };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(bill, okResult.Value);
        }

        /// <summary>
        /// Helper method to create a mock DbSet with queryable support.
        /// </summary>
        private Mock<DbSet<Bill>> CreateMockDbSet(IQueryable<Bill> data)
        {
            var mockSet = new Mock<DbSet<Bill>>();
            mockSet.As<IQueryable<Bill>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<Bill>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<Bill>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<Bill>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}