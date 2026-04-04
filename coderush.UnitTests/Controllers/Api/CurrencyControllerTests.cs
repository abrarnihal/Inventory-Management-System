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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the CurrencyController class.
    /// </summary>
    [TestClass]
    public class CurrencyControllerTests
    {
        /// <summary>
        /// Tests that Update returns OkObjectResult with updated currency when valid payload is provided.
        /// Input: Valid CrudViewModel with a Currency object.
        /// Expected: OkObjectResult containing the updated currency, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithCurrency()
        {
            // Arrange
            var currency = new Currency
            {
                CurrencyId = 1,
                CurrencyName = "US Dollar",
                CurrencyCode = "USD",
                Description = "United States Dollar"
            };
            var payload = new CrudViewModel<Currency>
            {
                action = "update",
                key = 1,
                value = currency
            };

            var mockSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(currency, okResult.Value);
            mockSet.Verify(s => s.Update(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update calls Update and SaveChanges with null currency when payload.value is null.
        /// Input: CrudViewModel with null value property.
        /// Expected: Update is called with null, SaveChanges is called, OkObjectResult with null value is returned.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_CallsUpdateAndSaveChangesWithNull()
        {
            // Arrange
            var payload = new CrudViewModel<Currency>
            {
                action = "update",
                key = 1,
                value = null
            };

            var mockSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new CurrencyController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockSet.Verify(s => s.Update(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles currency with minimum valid values.
        /// Input: Currency with minimum/boundary values (CurrencyId = 0, empty strings for optional fields).
        /// Expected: OkObjectResult with the currency, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_CurrencyWithMinimumValues_ReturnsOkResult()
        {
            // Arrange
            var currency = new Currency
            {
                CurrencyId = 0,
                CurrencyName = "A",
                CurrencyCode = "B",
                Description = ""
            };
            var payload = new CrudViewModel<Currency>
            {
                value = currency
            };

            var mockSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(s => s.Update(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles currency with maximum/extreme values.
        /// Input: Currency with int.MaxValue for CurrencyId and very long strings.
        /// Expected: OkObjectResult with the currency, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_CurrencyWithMaximumValues_ReturnsOkResult()
        {
            // Arrange
            var longString = new string('X', 10000);
            var currency = new Currency
            {
                CurrencyId = int.MaxValue,
                CurrencyName = longString,
                CurrencyCode = longString,
                Description = longString
            };
            var payload = new CrudViewModel<Currency>
            {
                value = currency
            };

            var mockSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(s => s.Update(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles currency with negative CurrencyId.
        /// Input: Currency with negative CurrencyId value.
        /// Expected: OkObjectResult with the currency, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_CurrencyWithNegativeId_ReturnsOkResult()
        {
            // Arrange
            var currency = new Currency
            {
                CurrencyId = -1,
                CurrencyName = "Test",
                CurrencyCode = "TST",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<Currency>
            {
                value = currency
            };

            var mockSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(s => s.Update(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles currency with special characters in string properties.
        /// Input: Currency with special characters, control characters, and Unicode in string fields.
        /// Expected: OkObjectResult with the currency, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_CurrencyWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var currency = new Currency
            {
                CurrencyId = 1,
                CurrencyName = "Currency!@#$%^&*()_+-=[]{}|;':\",./<>?",
                CurrencyCode = "\t\n\r\0",
                Description = "日本円€£¥"
            };
            var payload = new CrudViewModel<Currency>
            {
                value = currency
            };

            var mockSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(s => s.Update(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully removes a currency when a valid integer key is provided.
        /// </summary>
        [TestMethod]
        public void Remove_ValidIntegerKey_ReturnsOkWithRemovedCurrency()
        {
            // Arrange
            var currencyId = 5;
            var currency = new Currency { CurrencyId = currencyId, CurrencyName = "US Dollar", CurrencyCode = "USD" };
            var currencies = new List<Currency> { currency }.AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = currencyId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            Assert.AreEqual(currency, okResult.Value);
            mockSet.Verify(m => m.Remove(currency), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove successfully removes a currency when a valid string key is provided.
        /// </summary>
        [TestMethod]
        public void Remove_ValidStringKey_ReturnsOkWithRemovedCurrency()
        {
            // Arrange
            var currencyId = 10;
            var currency = new Currency { CurrencyId = currencyId, CurrencyName = "Euro", CurrencyCode = "EUR" };
            var currencies = new List<Currency> { currency }.AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = "10" };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(currency, okResult.Value);
            mockSet.Verify(m => m.Remove(currency), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles the case when currency is not found (FirstOrDefault returns null).
        /// </summary>
        [TestMethod]
        public void Remove_CurrencyNotFound_CallsRemoveWithNull()
        {
            // Arrange
            var currencies = new List<Currency>().AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = 999 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null!), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles null key by converting it to 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_ConvertsToZeroAndProcesses()
        {
            // Arrange
            var currency = new Currency { CurrencyId = 0, CurrencyName = "Default", CurrencyCode = "DEF" };
            var currencies = new List<Currency> { currency }.AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = null! };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            mockSet.Verify(m => m.Remove(It.IsAny<Currency>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles int.MaxValue as key correctly.
        /// </summary>
        [TestMethod]
        public void Remove_KeyAsMaxInt_ProcessesCorrectly()
        {
            // Arrange
            var currencyId = int.MaxValue;
            var currency = new Currency { CurrencyId = currencyId, CurrencyName = "Max Currency", CurrencyCode = "MAX" };
            var currencies = new List<Currency> { currency }.AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = int.MaxValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(currency, okResult.Value);
            mockSet.Verify(m => m.Remove(currency), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles int.MinValue as key correctly.
        /// </summary>
        [TestMethod]
        public void Remove_KeyAsMinInt_ProcessesCorrectly()
        {
            // Arrange
            var currencyId = int.MinValue;
            var currency = new Currency { CurrencyId = currencyId, CurrencyName = "Min Currency", CurrencyCode = "MIN" };
            var currencies = new List<Currency> { currency }.AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = int.MinValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(currency, okResult.Value);
            mockSet.Verify(m => m.Remove(currency), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles zero as key correctly.
        /// </summary>
        [TestMethod]
        public void Remove_KeyAsZero_ProcessesCorrectly()
        {
            // Arrange
            var currencies = new List<Currency>().AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = 0 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            mockSet.Verify(m => m.Remove(It.IsAny<Currency>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles negative currency IDs correctly.
        /// </summary>
        [TestMethod]
        public void Remove_NegativeKey_ProcessesCorrectly()
        {
            // Arrange
            var currencyId = -100;
            var currency = new Currency { CurrencyId = currencyId, CurrencyName = "Negative Currency", CurrencyCode = "NEG" };
            var currencies = new List<Currency> { currency }.AsQueryable();

            var mockSet = CreateMockDbSet(currencies);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Currency).Returns(mockSet.Object);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency> { key = -100 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(currency, okResult.Value);
            mockSet.Verify(m => m.Remove(currency), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports LINQ operations.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sourceList">The source queryable collection.</param>
        /// <returns>A mock DbSet configured for LINQ operations.</returns>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> sourceList) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(sourceList.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(sourceList.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(sourceList.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(sourceList.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Tests that Insert method successfully adds a valid currency to the database and returns OkObjectResult.
        /// Input: Valid payload with a valid Currency object.
        /// Expected: Currency is added to context, SaveChanges is called, and OkObjectResult is returned with the currency.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithCurrency()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);
            var currency = new Currency
            {
                CurrencyId = 1,
                CurrencyName = "US Dollar",
                CurrencyCode = "USD",
                Description = "United States Dollar"
            };
            var payload = new CrudViewModel<Currency>
            {
                value = currency,
                action = "insert",
                key = null,
                antiForgery = "token"
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(currency, okResult.Value);
            mockDbSet.Verify(d => d.Add(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method adds null currency when payload.value is null.
        /// Input: Payload with null value property.
        /// Expected: Null is passed to DbSet.Add and SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Insert_NullPayloadValue_AddsNullAndReturnOk()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new CurrencyController(mockContext.Object);
            var payload = new CrudViewModel<Currency>
            {
                value = null,
                action = "insert",
                key = null,
                antiForgery = "token"
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method successfully handles currency with minimum integer ID.
        /// Input: Valid payload with CurrencyId = int.MinValue.
        /// Expected: Currency is added and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CurrencyWithMinIntId_ReturnsOkResult()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);
            var currency = new Currency
            {
                CurrencyId = int.MinValue,
                CurrencyName = "Test Currency",
                CurrencyCode = "TST",
                Description = "Test"
            };
            var payload = new CrudViewModel<Currency> { value = currency };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method successfully handles currency with maximum integer ID.
        /// Input: Valid payload with CurrencyId = int.MaxValue.
        /// Expected: Currency is added and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CurrencyWithMaxIntId_ReturnsOkResult()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);
            var currency = new Currency
            {
                CurrencyId = int.MaxValue,
                CurrencyName = "Test Currency",
                CurrencyCode = "TST",
                Description = "Test"
            };
            var payload = new CrudViewModel<Currency> { value = currency };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method successfully handles currency with empty strings.
        /// Input: Valid payload with empty string properties.
        /// Expected: Currency is added and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CurrencyWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);
            var currency = new Currency
            {
                CurrencyId = 0,
                CurrencyName = string.Empty,
                CurrencyCode = string.Empty,
                Description = string.Empty
            };
            var payload = new CrudViewModel<Currency> { value = currency };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method successfully handles currency with very long strings.
        /// Input: Valid payload with very long string properties (1000+ characters).
        /// Expected: Currency is added and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CurrencyWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);
            var longString = new string('A', 1000);
            var currency = new Currency
            {
                CurrencyId = 1,
                CurrencyName = longString,
                CurrencyCode = longString,
                Description = longString
            };
            var payload = new CrudViewModel<Currency> { value = currency };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method successfully handles currency with special characters in strings.
        /// Input: Valid payload with special characters, control characters, and unicode in string properties.
        /// Expected: Currency is added and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CurrencyWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CurrencyController(mockContext.Object);
            var currency = new Currency
            {
                CurrencyId = 1,
                CurrencyName = "Currency!@#$%^&*()_+-=[]{}|;':\",./<>?",
                CurrencyCode = "€¥£₹₽",
                Description = "Test\r\n\t\0Unicode: 你好"
            };
            var payload = new CrudViewModel<Currency> { value = currency };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method adds currency when SaveChanges returns 0 (no rows affected).
        /// Input: Valid payload but SaveChanges returns 0.
        /// Expected: Currency is added and OkObjectResult is returned even with 0 rows affected.
        /// </summary>
        [TestMethod]
        public void Insert_SaveChangesReturnsZero_ReturnsOkResult()
        {
            // Arrange
            var mockDbSet = new Mock<DbSet<Currency>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Currency).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new CurrencyController(mockContext.Object);
            var currency = new Currency
            {
                CurrencyId = 1,
                CurrencyName = "US Dollar",
                CurrencyCode = "USD",
                Description = "Test"
            };
            var payload = new CrudViewModel<Currency> { value = currency };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(currency), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }
    }
}