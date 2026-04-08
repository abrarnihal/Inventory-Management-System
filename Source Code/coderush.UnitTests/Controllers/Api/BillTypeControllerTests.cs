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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the BillTypeController class.
    /// </summary>
    [TestClass]
    public class BillTypeControllerTests
    {
        /// <summary>
        /// Tests that Remove searches for BillTypeId == 0 when payload.key is null
        /// (Convert.ToInt32(null) returns 0) and returns OkObjectResult with null when not found.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_SearchesForIdZeroAndReturnsOkWithNull()
        {
            // Arrange
            var billTypes = new List<BillType>().AsQueryable();
            var mockDbSet = CreateMockDbSet(billTypes);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new BillTypeController(mockContext.Object);
            var payload = new CrudViewModel<BillType> { key = null };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(m => m.Remove(null!), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully removes a BillType when a valid key is provided
        /// and the BillType exists in the database, returning OkObjectResult with the removed entity.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(999)]
        public void Remove_ValidKeyBillTypeFound_RemovesBillTypeAndReturnsOk(int billTypeId)
        {
            // Arrange
            var billType = new BillType { BillTypeId = billTypeId, BillTypeName = "Test", Description = "Test Description" };
            var billTypes = new List<BillType> { billType }.AsQueryable();
            var mockDbSet = CreateMockDbSet(billTypes);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new BillTypeController(mockContext.Object);
            var payload = new CrudViewModel<BillType> { key = billTypeId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.AreEqual(billType, okResult.Value);
            mockDbSet.Verify(m => m.Remove(billType), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove returns OkObjectResult with null when the BillType with the specified key is not found,
        /// and calls Remove with null (potential bug in the original code).
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(999)]
        public void Remove_ValidKeyBillTypeNotFound_CallsRemoveWithNullAndReturnsOk(int billTypeId)
        {
            // Arrange
            var billTypes = new List<BillType>().AsQueryable();
            var mockDbSet = CreateMockDbSet(billTypes);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new BillTypeController(mockContext.Object);
            var payload = new CrudViewModel<BillType> { key = billTypeId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(m => m.Remove(null!), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles boundary integer values correctly.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-999)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public void Remove_BoundaryIntegerKeys_ReturnsOkWithNull(int keyValue)
        {
            // Arrange
            var billTypes = new List<BillType>().AsQueryable();
            var mockDbSet = CreateMockDbSet(billTypes);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new BillTypeController(mockContext.Object);
            var payload = new CrudViewModel<BillType> { key = keyValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(m => m.Remove(null!), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully processes when key is provided as a string containing a valid integer.
        /// </summary>
        [TestMethod]
        [DataRow("1", 1)]
        [DataRow("123", 123)]
        [DataRow("0", 0)]
        [DataRow("-5", -5)]
        public void Remove_StringKeyWithValidInteger_RemovesSuccessfully(string keyString, int expectedId)
        {
            // Arrange
            var billType = new BillType { BillTypeId = expectedId, BillTypeName = "Test", Description = "Test" };
            var billTypes = new List<BillType> { billType }.AsQueryable();
            var mockDbSet = CreateMockDbSet(billTypes);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new BillTypeController(mockContext.Object);
            var payload = new CrudViewModel<BillType> { key = keyString };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(billType, okResult.Value);
            mockDbSet.Verify(m => m.Remove(billType), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove correctly handles when multiple BillTypes exist but only the matching one is removed.
        /// </summary>
        [TestMethod]
        public void Remove_MultipleBillTypesExist_RemovesOnlyMatchingOne()
        {
            // Arrange
            var targetBillType = new BillType { BillTypeId = 2, BillTypeName = "Target", Description = "Target" };
            var billTypes = new List<BillType>
            {
                new BillType { BillTypeId = 1, BillTypeName = "First", Description = "First" },
                targetBillType,
                new BillType { BillTypeId = 3, BillTypeName = "Third", Description = "Third" }
            }.AsQueryable();

            var mockDbSet = CreateMockDbSet(billTypes);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new BillTypeController(mockContext.Object);
            var payload = new CrudViewModel<BillType> { key = 2 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(targetBillType, okResult.Value);
            mockDbSet.Verify(m => m.Remove(targetBillType), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports LINQ operations.
        /// </summary>
        /// <param name="sourceList">The source list to be used as the data source.</param>
        /// <returns>A mock DbSet configured to support LINQ queries.</returns>
        private static Mock<DbSet<BillType>> CreateMockDbSet(IQueryable<BillType> sourceList)
        {
            var mockSet = new Mock<DbSet<BillType>>();
            mockSet.As<IQueryable<BillType>>().Setup(m => m.Provider).Returns(sourceList.Provider);
            mockSet.As<IQueryable<BillType>>().Setup(m => m.Expression).Returns(sourceList.Expression);
            mockSet.As<IQueryable<BillType>>().Setup(m => m.ElementType).Returns(sourceList.ElementType);
            mockSet.As<IQueryable<BillType>>().Setup(m => m.GetEnumerator()).Returns(sourceList.GetEnumerator());
            mockSet.Setup(m => m.Remove(It.IsAny<BillType>())).Returns((BillType b) => null!);
            return mockSet;
        }

        /// <summary>
        /// Tests that Insert method with a valid payload adds the BillType to the context,
        /// saves changes, and returns an OkObjectResult with the BillType.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithBillType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = 1,
                BillTypeName = "Invoice",
                Description = "Standard Invoice"
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(billType, okResult.Value);
            mockDbSet.Verify(d => d.Add(billType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method with a payload containing a null value
        /// adds null to the context and returns an OkObjectResult with null.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var payload = new CrudViewModel<BillType>
            {
                value = null
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method with various BillType properties correctly processes
        /// different data scenarios including empty strings and special characters.
        /// </summary>
        /// <param name="billTypeId">The BillType identifier.</param>
        /// <param name="billTypeName">The BillType name.</param>
        /// <param name="description">The BillType description.</param>
        [TestMethod]
        [DataRow(0, "", "", DisplayName = "Empty strings")]
        [DataRow(int.MaxValue, "A", null, DisplayName = "Max ID with minimal name and null description")]
        [DataRow(int.MinValue, "Very Long Bill Type Name With Special Characters !@#$%^&*()", "Description with\nnewlines\tand\ttabs", DisplayName = "Min ID with special characters")]
        [DataRow(999, "   ", "   ", DisplayName = "Whitespace only")]
        public void Insert_VariousBillTypeProperties_ReturnsOkResultWithBillType(int billTypeId, string billTypeName, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = billTypeId,
                BillTypeName = billTypeName,
                Description = description
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(billType, okResult.Value);
            var resultBillType = (BillType)okResult.Value;
            Assert.AreEqual(billTypeId, resultBillType.BillTypeId);
            Assert.AreEqual(billTypeName, resultBillType.BillTypeName);
            Assert.AreEqual(description, resultBillType.Description);
            mockDbSet.Verify(d => d.Add(billType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method successfully updates a BillType and returns OkObjectResult with the updated entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithBillType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = 1,
                BillTypeName = "Test Bill Type",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<BillType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(billType, okResult.Value);
            mockDbSet.Verify(d => d.Update(billType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly calls DbSet.Update with the provided BillType entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsUpdateWithCorrectEntity()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = 42,
                BillTypeName = "Updated Type",
                Description = "Updated Description"
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            controller.Update(payload);

            // Assert
            mockDbSet.Verify(d => d.Update(It.Is<BillType>(bt =>
                bt.BillTypeId == 42 &&
                bt.BillTypeName == "Updated Type" &&
                bt.Description == "Updated Description")),
                Times.Once);
        }

        /// <summary>
        /// Tests that Update method calls SaveChanges exactly once after updating the entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsSaveChangesOnce()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = 1,
                BillTypeName = "Test",
                Description = "Test"
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            controller.Update(payload);

            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests Update method with boundary values for BillTypeId (int.MaxValue).
        /// </summary>
        [TestMethod]
        public void Update_BillTypeIdMaxValue_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = int.MaxValue,
                BillTypeName = "Max ID Type",
                Description = "Testing max value"
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(It.Is<BillType>(bt => bt.BillTypeId == int.MaxValue)), Times.Once);
        }

        /// <summary>
        /// Tests Update method with boundary values for BillTypeId (int.MinValue).
        /// </summary>
        [TestMethod]
        public void Update_BillTypeIdMinValue_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = int.MinValue,
                BillTypeName = "Min ID Type",
                Description = "Testing min value"
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(It.Is<BillType>(bt => bt.BillTypeId == int.MinValue)), Times.Once);
        }

        /// <summary>
        /// Tests Update method with empty string values for BillTypeName and Description.
        /// </summary>
        [TestMethod]
        public void Update_EmptyStringProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = 1,
                BillTypeName = "",
                Description = ""
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(It.Is<BillType>(bt => bt.BillTypeName == "" && bt.Description == "")), Times.Once);
        }

        /// <summary>
        /// Tests Update method with very long string values for BillTypeName and Description.
        /// </summary>
        [TestMethod]
        public void Update_VeryLongStringProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var longString = new string('A', 10000);
            var billType = new BillType
            {
                BillTypeId = 1,
                BillTypeName = longString,
                Description = longString
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(billType), Times.Once);
        }

        /// <summary>
        /// Tests Update method with special characters in string properties.
        /// </summary>
        [TestMethod]
        public void Update_SpecialCharactersInStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = 1,
                BillTypeName = "Type<>&\"'",
                Description = "Desc\r\n\t\0"
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(billType), Times.Once);
        }

        /// <summary>
        /// Tests Update method with whitespace-only string values.
        /// </summary>
        [TestMethod]
        public void Update_WhitespaceOnlyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var billType = new BillType
            {
                BillTypeId = 1,
                BillTypeName = "   ",
                Description = "\t\t\t"
            };

            var payload = new CrudViewModel<BillType>
            {
                value = billType
            };

            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(billType), Times.Once);
        }

        /// <summary>
        /// Tests that GetBillType returns an empty list with zero count when no BillType items exist in the database.
        /// </summary>
        [TestMethod]
        public async Task GetBillType_WhenNoItemsExist_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var emptyData = new List<BillType>();
            var mockDbSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = await controller.GetBillType();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<BillType>;
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetBillType returns a list with one item and count of one when a single BillType exists in the database.
        /// </summary>
        [TestMethod]
        public async Task GetBillType_WhenSingleItemExists_ReturnsListWithOneItemAndCountOne()
        {
            // Arrange
            var singleItemData = new List<BillType>
            {
                new BillType { BillTypeId = 1, BillTypeName = "Invoice", Description = "Standard Invoice" }
            };
            var mockDbSet = CreateMockDbSet(singleItemData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = await controller.GetBillType();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<BillType>;
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].BillTypeId);
            Assert.AreEqual("Invoice", items[0].BillTypeName);
        }

        /// <summary>
        /// Tests that GetBillType returns a list with all items and correct count when multiple BillType items exist in the database.
        /// </summary>
        [TestMethod]
        public async Task GetBillType_WhenMultipleItemsExist_ReturnsListWithAllItemsAndCorrectCount()
        {
            // Arrange
            var multipleItemsData = new List<BillType>
            {
                new BillType { BillTypeId = 1, BillTypeName = "Invoice", Description = "Standard Invoice" },
                new BillType { BillTypeId = 2, BillTypeName = "Receipt", Description = "Payment Receipt" },
                new BillType { BillTypeId = 3, BillTypeName = "Credit Note", Description = "Credit Note" }
            };
            var mockDbSet = CreateMockDbSet(multipleItemsData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = await controller.GetBillType();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = itemsProperty.GetValue(value) as List<BillType>;
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual("Invoice", items[0].BillTypeName);
            Assert.AreEqual("Receipt", items[1].BillTypeName);
            Assert.AreEqual("Credit Note", items[2].BillTypeName);
        }

        /// <summary>
        /// Tests that GetBillType returns OK result with correct structure for large dataset.
        /// </summary>
        [TestMethod]
        public async Task GetBillType_WhenLargeDatasetExists_ReturnsListWithAllItemsAndCorrectCount()
        {
            // Arrange
            var largeData = new List<BillType>();
            for (int i = 1; i <= 100; i++)
            {
                largeData.Add(new BillType
                {
                    BillTypeId = i,
                    BillTypeName = $"BillType{i}",
                    Description = $"Description {i}"
                });
            }
            var mockDbSet = CreateMockDbSet(largeData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.BillType).Returns(mockDbSet.Object);
            var controller = new BillTypeController(mockContext.Object);

            // Act
            var result = await controller.GetBillType();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            var items = itemsProperty.GetValue(value) as List<BillType>;
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(100, items.Count);
            Assert.AreEqual(100, count);
        }

        private Mock<DbSet<BillType>> CreateMockDbSet(List<BillType> data)
        {
            var queryableData = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<BillType>>();

            mockDbSet.As<IAsyncEnumerable<BillType>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<BillType>(data.GetEnumerator()));

            mockDbSet.As<IQueryable<BillType>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<BillType>(queryableData.Provider));

            mockDbSet.As<IQueryable<BillType>>()
                .Setup(m => m.Expression)
                .Returns(queryableData.Expression);

            mockDbSet.As<IQueryable<BillType>>()
                .Setup(m => m.ElementType)
                .Returns(queryableData.ElementType);

            mockDbSet.As<IQueryable<BillType>>()
                .Setup(m => m.GetEnumerator())
                .Returns(queryableData.GetEnumerator());

            return mockDbSet;
        }

        private class TestAsyncQueryProvider<TEntity> : IQueryProvider
        {
            private readonly IQueryProvider innerProvider;

            internal TestAsyncQueryProvider(IQueryProvider innerProvider)
            {
                this.innerProvider = innerProvider;
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
                return innerProvider.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return innerProvider.Execute<TResult>(expression);
            }
        }

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

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                this.inner = inner;
            }

            public T Current => inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                inner.Dispose();
                return default;
            }
        }
    }
}