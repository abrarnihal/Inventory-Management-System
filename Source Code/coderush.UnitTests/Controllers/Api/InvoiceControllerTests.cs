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
    /// Unit tests for the InvoiceController class.
    /// </summary>
    [TestClass]
    public class InvoiceControllerTests
    {
        /// <summary>
        /// Tests that Update method successfully updates an invoice and returns Ok result with the invoice.
        /// Input: Valid payload with a valid invoice object.
        /// Expected: Update is called, SaveChanges is called, and OkObjectResult is returned with the invoice.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithInvoice()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = 1,
                InvoiceName = "INV-001",
                ShipmentId = 100,
                InvoiceDate = DateTimeOffset.Now,
                InvoiceDueDate = DateTimeOffset.Now.AddDays(30),
                InvoiceTypeId = 1
            };
            var payload = new CrudViewModel<Invoice>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = invoice
            };
            mockContext.Setup(c => c.Invoice).Returns(mockInvoiceDbSet.Object);
            mockInvoiceDbSet.Setup(d => d.Update(It.IsAny<Invoice>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Invoice>)null!);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoice, okResult.Value);
            mockInvoiceDbSet.Verify(d => d.Update(invoice), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles invoices with different property values correctly.
        /// Input: Payloads with various invoice property values including edge cases.
        /// Expected: Update and SaveChanges are called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        [DataRow(0, "", 0, 0, DisplayName = "Zero and empty values")]
        [DataRow(int.MaxValue, "Very long invoice name with special chars !@#$%", int.MaxValue, int.MaxValue, DisplayName = "Maximum values")]
        [DataRow(-1, null, -1, -1, DisplayName = "Negative and null values")]
        public void Update_InvoiceWithVariousPropertyValues_ReturnsOkResult(int invoiceId, string invoiceName, int shipmentId, int invoiceTypeId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                InvoiceName = invoiceName,
                ShipmentId = shipmentId,
                InvoiceDate = DateTimeOffset.MinValue,
                InvoiceDueDate = DateTimeOffset.MaxValue,
                InvoiceTypeId = invoiceTypeId
            };
            var payload = new CrudViewModel<Invoice>
            {
                value = invoice
            };
            mockContext.Setup(c => c.Invoice).Returns(mockInvoiceDbSet.Object);
            mockInvoiceDbSet.Setup(d => d.Update(It.IsAny<Invoice>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Invoice>)null!);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoice, okResult.Value);
            mockInvoiceDbSet.Verify(d => d.Update(invoice), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method returns Ok result with status code 200.
        /// Input: Valid payload with valid invoice.
        /// Expected: OkObjectResult with status code 200 is returned.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithStatusCode200()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = 42,
                InvoiceName = "TEST-INV",
                ShipmentId = 999,
                InvoiceDate = DateTimeOffset.UtcNow,
                InvoiceDueDate = DateTimeOffset.UtcNow.AddMonths(1),
                InvoiceTypeId = 5
            };
            var payload = new CrudViewModel<Invoice>
            {
                value = invoice
            };
            mockContext.Setup(c => c.Invoice).Returns(mockInvoiceDbSet.Object);
            mockInvoiceDbSet.Setup(d => d.Update(It.IsAny<Invoice>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<Invoice>)null!);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload) as OkObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        /// <summary>
        /// Tests Remove method with a valid invoice ID that exists in the database.
        /// Expects the invoice to be removed and returned in an OkObjectResult.
        /// </summary>
        [TestMethod]
        public void Remove_ValidInvoiceIdExists_ReturnsOkWithRemovedInvoice()
        {
            // Arrange
            var invoiceId = 1;
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                InvoiceName = "INV-001"
            };
            var invoices = new List<Invoice>
            {
                invoice
            }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = invoiceId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(invoice, okResult.Value);
            mockDbSet.Verify(m => m.Remove(invoice), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests Remove method with a string representation of a valid invoice ID.
        /// Expects Convert.ToInt32 to parse the string and successfully remove the invoice.
        /// </summary>
        [TestMethod]
        public void Remove_ValidInvoiceIdAsString_ReturnsOkWithRemovedInvoice()
        {
            // Arrange
            var invoiceId = 42;
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                InvoiceName = "INV-042"
            };
            var invoices = new List<Invoice>
            {
                invoice
            }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = "42"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(invoice, okResult.Value);
            mockDbSet.Verify(m => m.Remove(invoice), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests Remove method when the invoice with the specified ID does not exist.
        /// Expects FirstOrDefault to return null and DbSet.Remove to be called with null.
        /// </summary>
        [TestMethod]
        public void Remove_InvoiceNotFound_AttemptsToRemoveNull()
        {
            // Arrange
            var invoiceId = 999;
            var invoices = new List<Invoice>().AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = invoiceId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(m => m.Remove(null), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests Remove method when payload.key is null.
        /// Expects Convert.ToInt32(null) to return 0 and search for invoice with ID 0.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadKeyIsNull_SearchesForInvoiceWithIdZero()
        {
            // Arrange
            var invoice = new Invoice
            {
                InvoiceId = 0,
                InvoiceName = "INV-000"
            };
            var invoices = new List<Invoice>
            {
                invoice
            }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = null
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(invoice, okResult.Value);
            mockDbSet.Verify(m => m.Remove(invoice), Times.Once);
        }

        /// <summary>
        /// Tests Remove method with int.MinValue as the invoice ID.
        /// Expects the method to search for invoice with ID int.MinValue.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadKeyIsIntMinValue_SearchesForInvoiceWithMinValue()
        {
            // Arrange
            var invoiceId = int.MinValue;
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                InvoiceName = "INV-MIN"
            };
            var invoices = new List<Invoice>
            {
                invoice
            }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = invoiceId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(invoice, okResult.Value);
            mockDbSet.Verify(m => m.Remove(invoice), Times.Once);
        }

        /// <summary>
        /// Tests Remove method with int.MaxValue as the invoice ID.
        /// Expects the method to search for invoice with ID int.MaxValue.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadKeyIsIntMaxValue_SearchesForInvoiceWithMaxValue()
        {
            // Arrange
            var invoiceId = int.MaxValue;
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                InvoiceName = "INV-MAX"
            };
            var invoices = new List<Invoice>
            {
                invoice
            }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = invoiceId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(invoice, okResult.Value);
            mockDbSet.Verify(m => m.Remove(invoice), Times.Once);
        }

        /// <summary>
        /// Tests Remove method with a negative invoice ID.
        /// Expects the method to search for invoice with the negative ID value.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadKeyIsNegative_SearchesForInvoiceWithNegativeId()
        {
            // Arrange
            var invoiceId = -100;
            var invoice = new Invoice
            {
                InvoiceId = invoiceId,
                InvoiceName = "INV-NEG"
            };
            var invoices = new List<Invoice>
            {
                invoice
            }.AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = invoiceId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(invoice, okResult.Value);
            mockDbSet.Verify(m => m.Remove(invoice), Times.Once);
        }

        /// <summary>
        /// Tests Remove method with zero as the invoice ID.
        /// Expects the method to search for invoice with ID 0.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadKeyIsZero_SearchesForInvoiceWithIdZero()
        {
            // Arrange
            var invoiceId = 0;
            var invoices = new List<Invoice>().AsQueryable();
            var mockDbSet = new Mock<DbSet<Invoice>>();
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Provider).Returns(invoices.Provider);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.Expression).Returns(invoices.Expression);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.ElementType).Returns(invoices.ElementType);
            mockDbSet.As<IQueryable<Invoice>>().Setup(m => m.GetEnumerator()).Returns(invoices.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Invoice).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<Invoice>
            {
                key = invoiceId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(m => m.Remove(null), Times.Once);
        }

        /// <summary>
        /// Tests that Insert adds the invoice to the context, saves changes, and returns Ok result with the invoice.
        /// Input: Valid payload with a valid invoice.
        /// Expected: Invoice is added to context, SaveChanges is called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithInvoice()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = 1,
                ShipmentId = 100,
                InvoiceDate = DateTimeOffset.Now,
                InvoiceDueDate = DateTimeOffset.Now.AddDays(30),
                InvoiceTypeId = 1
            };
            var payload = new CrudViewModel<Invoice>
            {
                action = "insert",
                value = invoice
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("INV")).Returns("INV-001");
            mockContext.Setup(x => x.Invoice).Returns(mockInvoiceDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreSame(invoice, okResult.Value);
            Assert.AreEqual("INV-001", invoice.InvoiceName);
            mockInvoiceDbSet.Verify(x => x.Add(invoice), Times.Once);
            mockContext.Verify(x => x.SaveChanges(), Times.Once);
            mockNumberSequence.Verify(x => x.GetNumberSequence("INV"), Times.Once);
        }

        /// <summary>
        /// Tests that Insert correctly sets the InvoiceName property from the number sequence service.
        /// Input: Valid payload with invoice, number sequence returns specific value.
        /// Expected: Invoice's InvoiceName property is set to the returned value.
        /// </summary>
        [TestMethod]
        [DataRow("INV-001")]
        [DataRow("INV-999999")]
        [DataRow("")]
        [DataRow("   ")]
        public void Insert_VariousNumberSequenceValues_SetsInvoiceName(string sequenceValue)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = 1,
                ShipmentId = 100,
                InvoiceDate = DateTimeOffset.Now,
                InvoiceDueDate = DateTimeOffset.Now.AddDays(30),
                InvoiceTypeId = 1
            };
            var payload = new CrudViewModel<Invoice>
            {
                value = invoice
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("INV")).Returns(sequenceValue);
            mockContext.Setup(x => x.Invoice).Returns(mockInvoiceDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.AreEqual(sequenceValue, invoice.InvoiceName);
        }

        /// <summary>
        /// Tests that Insert handles invoice with boundary values for integer properties.
        /// Input: Invoice with int.MaxValue for numeric properties.
        /// Expected: Invoice is processed successfully.
        /// </summary>
        [TestMethod]
        public void Insert_InvoiceWithBoundaryValues_ProcessesSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = int.MaxValue,
                ShipmentId = int.MaxValue,
                InvoiceDate = DateTimeOffset.MaxValue,
                InvoiceDueDate = DateTimeOffset.MaxValue,
                InvoiceTypeId = int.MaxValue
            };
            var payload = new CrudViewModel<Invoice>
            {
                value = invoice
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("INV")).Returns("INV-MAX");
            mockContext.Setup(x => x.Invoice).Returns(mockInvoiceDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            mockInvoiceDbSet.Verify(x => x.Add(invoice), Times.Once);
            mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles invoice with minimum values for integer properties.
        /// Input: Invoice with 0 or int.MinValue for numeric properties.
        /// Expected: Invoice is processed successfully.
        /// </summary>
        [TestMethod]
        public void Insert_InvoiceWithMinimumValues_ProcessesSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = 0,
                ShipmentId = 0,
                InvoiceDate = DateTimeOffset.MinValue,
                InvoiceDueDate = DateTimeOffset.MinValue,
                InvoiceTypeId = 0
            };
            var payload = new CrudViewModel<Invoice>
            {
                value = invoice
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("INV")).Returns("INV-MIN");
            mockContext.Setup(x => x.Invoice).Returns(mockInvoiceDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            mockInvoiceDbSet.Verify(x => x.Add(invoice), Times.Once);
            mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles invoice with negative values for numeric properties.
        /// Input: Invoice with negative ShipmentId and InvoiceTypeId.
        /// Expected: Invoice is processed successfully (validation may occur at different layer).
        /// </summary>
        [TestMethod]
        public void Insert_InvoiceWithNegativeValues_ProcessesSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = -1,
                ShipmentId = -100,
                InvoiceDate = DateTimeOffset.Now,
                InvoiceDueDate = DateTimeOffset.Now,
                InvoiceTypeId = -5
            };
            var payload = new CrudViewModel<Invoice>
            {
                value = invoice
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("INV")).Returns("INV-NEG");
            mockContext.Setup(x => x.Invoice).Returns(mockInvoiceDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            mockInvoiceDbSet.Verify(x => x.Add(invoice), Times.Once);
            mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert returns the same invoice object that was added.
        /// Input: Valid invoice in payload.
        /// Expected: The exact same invoice object reference is returned in Ok result.
        /// </summary>
        [TestMethod]
        public void Insert_ValidInvoice_ReturnsSameInvoiceObject()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockInvoiceDbSet = new Mock<DbSet<Invoice>>();
            var invoice = new Invoice
            {
                InvoiceId = 42,
                ShipmentId = 200,
                InvoiceDate = DateTimeOffset.Now,
                InvoiceDueDate = DateTimeOffset.Now.AddDays(15),
                InvoiceTypeId = 3
            };
            var payload = new CrudViewModel<Invoice>
            {
                value = invoice
            };
            mockNumberSequence.Setup(x => x.GetNumberSequence("INV")).Returns("INV-042");
            mockContext.Setup(x => x.Invoice).Returns(mockInvoiceDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var controller = new InvoiceController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(invoice, okResult.Value);
        }
    }
}