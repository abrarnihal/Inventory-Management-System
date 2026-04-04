using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the ProductTypeController class.
    /// </summary>
    [TestClass]
    public class ProductTypeControllerTests
    {
        /// <summary>
        /// Tests that Insert method successfully adds a ProductType to the database context,
        /// saves changes, and returns an OkObjectResult with the added ProductType when given a valid payload.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayloadWithCompleteProductType_ReturnsOkResultWithProductType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var productType = new ProductType
            {
                ProductTypeId = 1,
                ProductTypeName = "Test Product Type",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType,
                action = "insert",
                key = 1,
                antiForgery = "token"
            };

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(productType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(productType, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method successfully adds a ProductType with minimal data (no description)
        /// to the database context and returns an OkObjectResult.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayloadWithMinimalProductType_ReturnsOkResultWithProductType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var productType = new ProductType
            {
                ProductTypeId = 0,
                ProductTypeName = "Minimal Type"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(productType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles various string values in ProductType properties correctly,
        /// including empty strings, whitespace, and special characters.
        /// </summary>
        /// <param name="productTypeName">The product type name to test.</param>
        /// <param name="description">The description to test.</param>
        [TestMethod]
        [DataRow("", "")]
        [DataRow("   ", "   ")]
        [DataRow("Name with special chars !@#$%^&*()", "Description with special chars <>&\"'")]
        [DataRow("VeryLongProductTypeNameThatExceedsNormalLengthExpectationsForTestingPurposesWithMoreThan100Characters", "VeryLongDescriptionThatExceedsNormalLengthExpectationsForTestingPurposesWithMoreThan100CharactersToTestBoundaries")]
        public void Insert_ValidPayloadWithVariousStringValues_ReturnsOkResult(string productTypeName, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var productType = new ProductType
            {
                ProductTypeId = 1,
                ProductTypeName = productTypeName,
                Description = description
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(It.Is<ProductType>(p =>
                p.ProductTypeName == productTypeName &&
                p.Description == description)), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles extreme integer values for ProductTypeId correctly.
        /// </summary>
        /// <param name="productTypeId">The product type ID to test.</param>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(999999999)]
        public void Insert_ValidPayloadWithVariousProductTypeIds_ReturnsOkResult(int productTypeId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var productType = new ProductType
            {
                ProductTypeId = productTypeId,
                ProductTypeName = "Test Type"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(It.Is<ProductType>(p => p.ProductTypeId == productTypeId)), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method attempts to add null to the DbSet when payload.value is null.
        /// This tests the behavior when the value property of the payload is null.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullToDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var payload = new CrudViewModel<ProductType>
            {
                value = null,
                action = "insert"
            };

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method returns OkObjectResult with the exact same ProductType instance
        /// that was provided in the payload, verifying reference equality.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsSameProductTypeInstance()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var productType = new ProductType
            {
                ProductTypeId = 1,
                ProductTypeName = "Test Type",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(productType, okResult.Value);
        }

        /// <summary>
        /// Tests that GetProductType returns OkObjectResult with empty list when database contains no ProductType records.
        /// Input: Empty ProductType DbSet.
        /// Expected: OkObjectResult containing Items = empty list and Count = 0.
        /// </summary>
        [TestMethod]
        public async Task GetProductType_EmptyDatabase_ReturnsOkResultWithEmptyListAndZeroCount()
        {
            // Arrange
            var emptyData = new List<ProductType>().AsQueryable();
            var mockSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.ProductType).Returns(mockSet.Object);
            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = await controller.GetProductType();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Result should be OkObjectResult");
            Assert.IsNotNull(okResult.Value, "Result value should not be null");

            var resultValue = okResult.Value;
            var itemsProperty = resultValue.GetType().GetProperty("Items");
            var countProperty = resultValue.GetType().GetProperty("Count");

            Assert.IsNotNull(itemsProperty, "Result should have Items property");
            Assert.IsNotNull(countProperty, "Result should have Count property");

            var items = itemsProperty.GetValue(resultValue) as List<ProductType>;
            var count = (int)countProperty.GetValue(resultValue);

            Assert.IsNotNull(items, "Items should not be null");
            Assert.AreEqual(0, items.Count, "Items list should be empty");
            Assert.AreEqual(0, count, "Count should be 0");
        }

        /// <summary>
        /// Tests that GetProductType returns OkObjectResult with single item when database contains one ProductType record.
        /// Input: DbSet with one ProductType.
        /// Expected: OkObjectResult containing Items with one element and Count = 1.
        /// </summary>
        [TestMethod]
        public async Task GetProductType_SingleRecord_ReturnsOkResultWithSingleItemAndCountOne()
        {
            // Arrange
            var testData = new List<ProductType>
            {
                new ProductType { ProductTypeId = 1, ProductTypeName = "Electronics", Description = "Electronic items" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.ProductType).Returns(mockSet.Object);
            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = await controller.GetProductType();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Result should be OkObjectResult");
            Assert.IsNotNull(okResult.Value, "Result value should not be null");

            var resultValue = okResult.Value;
            var itemsProperty = resultValue.GetType().GetProperty("Items");
            var countProperty = resultValue.GetType().GetProperty("Count");

            var items = itemsProperty.GetValue(resultValue) as List<ProductType>;
            var count = (int)countProperty.GetValue(resultValue);

            Assert.IsNotNull(items, "Items should not be null");
            Assert.AreEqual(1, items.Count, "Items list should contain one element");
            Assert.AreEqual(1, count, "Count should be 1");
            Assert.AreEqual("Electronics", items[0].ProductTypeName, "ProductTypeName should match");
        }

        /// <summary>
        /// Tests that GetProductType returns OkObjectResult with multiple items when database contains multiple ProductType records.
        /// Input: DbSet with multiple ProductType records.
        /// Expected: OkObjectResult containing Items with all elements and Count matching the number of records.
        /// </summary>
        [TestMethod]
        public async Task GetProductType_MultipleRecords_ReturnsOkResultWithAllItemsAndCorrectCount()
        {
            // Arrange
            var testData = new List<ProductType>
            {
                new ProductType { ProductTypeId = 1, ProductTypeName = "Electronics", Description = "Electronic items" },
                new ProductType { ProductTypeId = 2, ProductTypeName = "Clothing", Description = "Apparel" },
                new ProductType { ProductTypeId = 3, ProductTypeName = "Food", Description = "Groceries" }
            }.AsQueryable();

            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.ProductType).Returns(mockSet.Object);
            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = await controller.GetProductType();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult, "Result should be OkObjectResult");
            Assert.IsNotNull(okResult.Value, "Result value should not be null");

            var resultValue = okResult.Value;
            var itemsProperty = resultValue.GetType().GetProperty("Items");
            var countProperty = resultValue.GetType().GetProperty("Count");

            var items = itemsProperty.GetValue(resultValue) as List<ProductType>;
            var count = (int)countProperty.GetValue(resultValue);

            Assert.IsNotNull(items, "Items should not be null");
            Assert.AreEqual(3, items.Count, "Items list should contain three elements");
            Assert.AreEqual(3, count, "Count should be 3");
            Assert.AreEqual("Electronics", items[0].ProductTypeName, "First ProductTypeName should match");
            Assert.AreEqual("Clothing", items[1].ProductTypeName, "Second ProductTypeName should match");
            Assert.AreEqual("Food", items[2].ProductTypeName, "Third ProductTypeName should match");
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations.
        /// Note: This is a simplified mock setup. In production scenarios with complex EF Core async operations,
        /// consider using an in-memory database or MockQueryable library for more robust testing.
        /// </summary>
        /// <param name="sourceList">The source data to use for the mock DbSet.</param>
        /// <returns>A mocked DbSet that supports ToListAsync operations.</returns>
        private static Mock<DbSet<ProductType>> CreateMockDbSet(IQueryable<ProductType> sourceList)
        {
            var mockSet = new Mock<DbSet<ProductType>>();

            mockSet.As<IAsyncEnumerable<ProductType>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<ProductType>(sourceList.GetEnumerator()));

            mockSet.As<IQueryable<ProductType>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ProductType>(sourceList.Provider));
            mockSet.As<IQueryable<ProductType>>().Setup(m => m.Expression).Returns(sourceList.Expression);
            mockSet.As<IQueryable<ProductType>>().Setup(m => m.ElementType).Returns(sourceList.ElementType);
            mockSet.As<IQueryable<ProductType>>().Setup(m => m.GetEnumerator()).Returns(sourceList.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Test async query provider to support Entity Framework Core async operations in unit tests.
        /// This is an internal helper class required for mocking DbSet async operations.
        /// </summary>
        private class TestAsyncQueryProvider<TEntity> : IQueryProvider
        {
            private readonly IQueryProvider _inner;

            internal TestAsyncQueryProvider(IQueryProvider inner)
            {
                _inner = inner;
            }

            public IQueryable CreateQuery(System.Linq.Expressions.Expression expression)
            {
                return new TestAsyncEnumerable<TEntity>(expression);
            }

            public IQueryable<TElement> CreateQuery<TElement>(System.Linq.Expressions.Expression expression)
            {
                return new TestAsyncEnumerable<TElement>(expression);
            }

            public object Execute(System.Linq.Expressions.Expression expression)
            {
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }
        }

        /// <summary>
        /// Test async enumerable to support Entity Framework Core async enumeration in unit tests.
        /// This is an internal helper class required for mocking DbSet async operations.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(System.Linq.Expressions.Expression expression)
                : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }
        }

        /// <summary>
        /// Test async enumerator to support Entity Framework Core async enumeration in unit tests.
        /// This is an internal helper class required for mocking DbSet async operations.
        /// </summary>
        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current => _inner.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }
        }

        /// <summary>
        /// Tests that Update returns OkObjectResult with the updated ProductType when given a valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkObjectResultWithProductType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var productType = new ProductType
            {
                ProductTypeId = 1,
                ProductTypeName = "Test Product Type",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<ProductType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = productType
            };

            mockDbSet.Setup(m => m.Update(It.IsAny<ProductType>()));
            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(productType, okResult.Value);
        }

        /// <summary>
        /// Tests that Update calls Update on the DbSet with the correct ProductType.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsDbSetUpdateWithCorrectProductType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var productType = new ProductType
            {
                ProductTypeId = 2,
                ProductTypeName = "Electronics",
                Description = "Electronic items"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            mockDbSet.Setup(m => m.Update(It.IsAny<ProductType>()));
            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            controller.Update(payload);

            // Assert
            mockDbSet.Verify(m => m.Update(productType), Times.Once);
        }

        /// <summary>
        /// Tests that Update calls SaveChanges on the context exactly once.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsSaveChangesOnce()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var productType = new ProductType
            {
                ProductTypeId = 3,
                ProductTypeName = "Books",
                Description = "Book category"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            mockDbSet.Setup(m => m.Update(It.IsAny<ProductType>()));
            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            controller.Update(payload);

            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles payload with null value property.
        /// The method passes null to DbSet.Update which may throw or handle according to EF behavior.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_PassesNullToUpdate()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var payload = new CrudViewModel<ProductType>
            {
                action = "update",
                value = null!
            };

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);

            var controller = new ProductTypeController(mockContext.Object);

            // Act & Assert
            try
            {
                controller.Update(payload);
            }
            catch
            {
                // Expected behavior - Update with null may throw
            }

            // Verify that Update was called with null
            mockDbSet.Verify(m => m.Update(null!), Times.Once);
        }

        /// <summary>
        /// Tests that Update works correctly with ProductType having minimum valid values.
        /// ProductTypeId of 0 and empty description.
        /// </summary>
        [TestMethod]
        public void Update_ProductTypeWithMinimumValidValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var productType = new ProductType
            {
                ProductTypeId = 0,
                ProductTypeName = "A",
                Description = ""
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(productType, okResult?.Value);
        }

        /// <summary>
        /// Tests that Update works correctly with ProductType having maximum boundary values.
        /// Tests with int.MaxValue for ProductTypeId and very long strings.
        /// </summary>
        [TestMethod]
        public void Update_ProductTypeWithMaximumBoundaryValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var longString = new string('X', 10000);
            var productType = new ProductType
            {
                ProductTypeId = int.MaxValue,
                ProductTypeName = longString,
                Description = longString
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Update works with ProductType having negative ProductTypeId.
        /// </summary>
        [TestMethod]
        public void Update_ProductTypeWithNegativeId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var productType = new ProductType
            {
                ProductTypeId = -1,
                ProductTypeName = "Negative ID Test",
                Description = "Testing negative ID"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Update works with ProductType having special characters in string properties.
        /// </summary>
        [TestMethod]
        public void Update_ProductTypeWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ProductType>>();

            var productType = new ProductType
            {
                ProductTypeId = 5,
                ProductTypeName = "Test<>&\"'\n\r\t\0",
                Description = "Special!@#$%^&*(){}[]|\\:;\"'<>,.?/~`"
            };

            var payload = new CrudViewModel<ProductType>
            {
                value = productType
            };

            mockContext.Setup(c => c.ProductType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(productType, okResult?.Value);
        }

        /// <summary>
        /// Tests that Remove method returns OkObjectResult with the removed ProductType when a valid key is provided.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKey_ReturnsOkWithRemovedProductType()
        {
            // Arrange
            var productTypeId = 1;
            var productType = new ProductType
            {
                ProductTypeId = productTypeId,
                ProductTypeName = "Test Type",
                Description = "Test Description"
            };

            var data = new List<ProductType> { productType }.AsQueryable();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.ProductType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);
            var payload = new CrudViewModel<ProductType> { key = productTypeId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(productType, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<ProductType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method calls SaveChanges on the context.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKey_CallsSaveChanges()
        {
            // Arrange
            var productTypeId = 5;
            var productType = new ProductType
            {
                ProductTypeId = productTypeId,
                ProductTypeName = "Sample Type",
                Description = "Sample Description"
            };

            var data = new List<ProductType> { productType }.AsQueryable();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.ProductType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);
            var payload = new CrudViewModel<ProductType> { key = productTypeId };

            // Act
            controller.Remove(payload);

            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method with string key that is a valid number converts successfully.
        /// </summary>
        [TestMethod]
        public void Remove_KeyAsStringNumber_ConvertsSuccessfully()
        {
            // Arrange
            var productTypeId = 10;
            var productType = new ProductType
            {
                ProductTypeId = productTypeId,
                ProductTypeName = "Test Type",
                Description = "Test Description"
            };

            var data = new List<ProductType> { productType }.AsQueryable();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.ProductType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductTypeController(mockContext.Object);
            var payload = new CrudViewModel<ProductType> { key = "10" };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(productType, okResult.Value);
        }
    }
}