using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    /// Unit tests for the SalesTypeController class.
    /// </summary>
    [TestClass]
    public class SalesTypeControllerTests
    {
        /// <summary>
        /// Tests that Update method returns OkObjectResult with the updated SalesType when provided with a valid payload.
        /// Input: Valid CrudViewModel with a valid SalesType.
        /// Expected: Returns OkObjectResult containing the SalesType, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithSalesType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = "Retail",
                Description = "Retail sales"
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = salesType
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method calls Update with null entity when payload.value is null.
        /// Input: CrudViewModel with null value property.
        /// Expected: Update is called with null, SaveChanges is called, returns OkObjectResult with null.
        /// </summary>
        [TestMethod]
        public void Update_NullValueInPayload_UpdatesWithNullEntity()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = null
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(d => d.Update(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles SalesType with boundary value for SalesTypeId (int.MinValue).
        /// Input: Valid payload with SalesTypeId = int.MinValue.
        /// Expected: Returns OkObjectResult with the SalesType, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_SalesTypeIdMinValue_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var salesType = new SalesType
            {
                SalesTypeId = int.MinValue,
                SalesTypeName = "Test",
                Description = "Boundary test"
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = int.MinValue,
                antiForgery = "token",
                value = salesType
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles SalesType with boundary value for SalesTypeId (int.MaxValue).
        /// Input: Valid payload with SalesTypeId = int.MaxValue.
        /// Expected: Returns OkObjectResult with the SalesType, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_SalesTypeIdMaxValue_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var salesType = new SalesType
            {
                SalesTypeId = int.MaxValue,
                SalesTypeName = "Test",
                Description = "Boundary test"
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = int.MaxValue,
                antiForgery = "token",
                value = salesType
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles SalesType with empty string properties.
        /// Input: Valid payload with empty strings for SalesTypeName and Description.
        /// Expected: Returns OkObjectResult with the SalesType, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_EmptyStringProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = string.Empty,
                Description = string.Empty
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = salesType
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles SalesType with null string properties.
        /// Input: Valid payload with null strings for SalesTypeName and Description.
        /// Expected: Returns OkObjectResult with the SalesType, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_NullStringProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = null,
                Description = null
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = salesType
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles SalesType with special characters in string properties.
        /// Input: Valid payload with special characters in SalesTypeName and Description.
        /// Expected: Returns OkObjectResult with the SalesType, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_SpecialCharactersInStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = "<script>alert('xss')</script>",
                Description = "Special chars: !@#$%^&*()_+{}[]|\\:;\"'<>,.?/~`"
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = salesType
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles SalesType with very long strings.
        /// Input: Valid payload with very long strings for SalesTypeName and Description.
        /// Expected: Returns OkObjectResult with the SalesType, Update and SaveChanges are called.
        /// </summary>
        [TestMethod]
        public void Update_VeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<SalesType>())).Returns((EntityEntry<SalesType>)null);
            var longString = new string ('A', 10000);
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = longString,
                Description = longString
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = salesType
            };
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Update(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that GetSalesType returns an empty list with count zero when no SalesType records exist in the database.
        /// </summary>
        [TestMethod]
        public async Task GetSalesType_EmptyDatabase_ReturnsEmptyListWithCountZero()
        {
            // Arrange
            var emptyData = new List<SalesType>();
            var mockSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = await controller.GetSalesType();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<SalesType>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetSalesType returns a list with one item and count of one when a single SalesType record exists.
        /// </summary>
        [TestMethod]
        public async Task GetSalesType_SingleRecord_ReturnsListWithOneItemAndCountOne()
        {
            // Arrange
            var singleData = new List<SalesType>
            {
                new SalesType
                {
                    SalesTypeId = 1,
                    SalesTypeName = "Retail",
                    Description = "Retail sales"
                }
            };
            var mockSet = CreateMockDbSet(singleData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = await controller.GetSalesType();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<SalesType>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].SalesTypeId);
            Assert.AreEqual("Retail", items[0].SalesTypeName);
        }

        /// <summary>
        /// Tests that GetSalesType returns a list with multiple items and correct count when multiple SalesType records exist.
        /// </summary>
        /// <param name = "recordCount">The number of SalesType records to test with.</param>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        public async Task GetSalesType_MultipleRecords_ReturnsListWithAllItemsAndCorrectCount(int recordCount)
        {
            // Arrange
            var multipleData = new List<SalesType>();
            for (int i = 1; i <= recordCount; i++)
            {
                multipleData.Add(new SalesType { SalesTypeId = i, SalesTypeName = $"SalesType{i}", Description = $"Description{i}" });
            }

            var mockSet = CreateMockDbSet(multipleData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            var controller = new SalesTypeController(mockContext.Object);
            // Act
            var result = await controller.GetSalesType();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<SalesType>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(recordCount, items.Count);
            Assert.AreEqual(recordCount, count);
            for (int i = 0; i < recordCount; i++)
            {
                Assert.AreEqual(i + 1, items[i].SalesTypeId);
                Assert.AreEqual($"SalesType{i + 1}", items[i].SalesTypeName);
            }
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations for testing.
        /// </summary>
        /// <typeparam name = "T">The entity type.</typeparam>
        /// <param name = "sourceList">The source data list.</param>
        /// <returns>A mocked DbSet.</returns>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> sourceList)
            where T : class
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(queryable.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(queryable.GetEnumerator()));
            return mockSet;
        }

        /// <summary>
        /// Helper class to support async query operations in tests.
        /// </summary>
        /// <typeparam name = "TEntity">The entity type.</typeparam>
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
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

            public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executeMethod = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) })?.MakeGenericMethod(resultType);
                var result = executeMethod?.Invoke(_inner, new object[] { expression });
                return (TResult)Activator.CreateInstance(typeof(TResult), result);
            }
        }

        /// <summary>
        /// Helper class to support async enumerable operations in tests.
        /// </summary>
        /// <typeparam name = "T">The entity type.</typeparam>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        /// <summary>
        /// Helper class to support async enumerator operations in tests.
        /// </summary>
        /// <typeparam name = "T">The entity type.</typeparam>
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
                return default;
            }
        }

        /// <summary>
        /// Tests that Remove returns Ok with the removed entity when a valid payload with an existing SalesType ID is provided.
        /// </summary>
        [TestMethod]
        public void Remove_ValidPayloadWithExistingSalesTypeId_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            int salesTypeId = 5;
            SalesType salesType = new SalesType
            {
                SalesTypeId = salesTypeId,
                SalesTypeName = "Retail",
                Description = "Retail sales"
            };
            List<SalesType> salesTypeList = new List<SalesType>
            {
                salesType
            };
            CrudViewModel<SalesType> payload = new CrudViewModel<SalesType>
            {
                key = salesTypeId
            };
            Mock<DbSet<SalesType>> mockSet = CreateMockDbSet(salesTypeList.AsQueryable());
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            SalesTypeController controller = new SalesTypeController(mockContext.Object);
            // Act
            IActionResult result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(salesType, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<SalesType>()), Times.Once());
            mockContext.Verify(c => c.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove attempts to remove null when no SalesType matches the provided ID.
        /// This tests the behavior when FirstOrDefault returns null.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithNonExistentSalesTypeId_AttemptsToRemoveNull()
        {
            // Arrange
            int nonExistentId = 999;
            List<SalesType> salesTypeList = new List<SalesType>();
            CrudViewModel<SalesType> payload = new CrudViewModel<SalesType>
            {
                key = nonExistentId
            };
            Mock<DbSet<SalesType>> mockSet = CreateMockDbSet(salesTypeList.AsQueryable());
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            SalesTypeController controller = new SalesTypeController(mockContext.Object);
            // Act
            IActionResult result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null), Times.Once());
            mockContext.Verify(c => c.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove converts null key to 0 and queries for SalesTypeId == 0.
        /// Convert.ToInt32(null) returns 0.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithNullKey_QueriesForSalesTypeIdZero()
        {
            // Arrange
            SalesType salesType = new SalesType
            {
                SalesTypeId = 0,
                SalesTypeName = "Default",
                Description = "Default sales"
            };
            List<SalesType> salesTypeList = new List<SalesType>
            {
                salesType
            };
            CrudViewModel<SalesType> payload = new CrudViewModel<SalesType>
            {
                key = null
            };
            Mock<DbSet<SalesType>> mockSet = CreateMockDbSet(salesTypeList.AsQueryable());
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            SalesTypeController controller = new SalesTypeController(mockContext.Object);
            // Act
            IActionResult result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(salesType, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove successfully converts a string key to int and removes the entity.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithStringKey_ConvertsAndRemovesEntity()
        {
            // Arrange
            int salesTypeId = 42;
            SalesType salesType = new SalesType
            {
                SalesTypeId = salesTypeId,
                SalesTypeName = "Wholesale",
                Description = "Wholesale sales"
            };
            List<SalesType> salesTypeList = new List<SalesType>
            {
                salesType
            };
            CrudViewModel<SalesType> payload = new CrudViewModel<SalesType>
            {
                key = "42"
            };
            Mock<DbSet<SalesType>> mockSet = CreateMockDbSet(salesTypeList.AsQueryable());
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            SalesTypeController controller = new SalesTypeController(mockContext.Object);
            // Act
            IActionResult result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(salesType, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove handles int.MinValue as a valid key value.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithMinIntKey_HandlesEdgeCaseValue()
        {
            // Arrange
            int salesTypeId = int.MinValue;
            SalesType salesType = new SalesType
            {
                SalesTypeId = salesTypeId,
                SalesTypeName = "MinValue",
                Description = "Edge case"
            };
            List<SalesType> salesTypeList = new List<SalesType>
            {
                salesType
            };
            CrudViewModel<SalesType> payload = new CrudViewModel<SalesType>
            {
                key = int.MinValue
            };
            Mock<DbSet<SalesType>> mockSet = CreateMockDbSet(salesTypeList.AsQueryable());
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            SalesTypeController controller = new SalesTypeController(mockContext.Object);
            // Act
            IActionResult result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(salesType, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove handles int.MaxValue as a valid key value.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithMaxIntKey_HandlesEdgeCaseValue()
        {
            // Arrange
            int salesTypeId = int.MaxValue;
            SalesType salesType = new SalesType
            {
                SalesTypeId = salesTypeId,
                SalesTypeName = "MaxValue",
                Description = "Edge case"
            };
            List<SalesType> salesTypeList = new List<SalesType>
            {
                salesType
            };
            CrudViewModel<SalesType> payload = new CrudViewModel<SalesType>
            {
                key = int.MaxValue
            };
            Mock<DbSet<SalesType>> mockSet = CreateMockDbSet(salesTypeList.AsQueryable());
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            SalesTypeController controller = new SalesTypeController(mockContext.Object);
            // Act
            IActionResult result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(salesType, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove handles negative key values correctly.
        /// </summary>
        [TestMethod]
        public void Remove_PayloadWithNegativeKey_QueriesForNegativeId()
        {
            // Arrange
            int salesTypeId = -5;
            SalesType salesType = new SalesType
            {
                SalesTypeId = salesTypeId,
                SalesTypeName = "Negative",
                Description = "Negative ID"
            };
            List<SalesType> salesTypeList = new List<SalesType>
            {
                salesType
            };
            CrudViewModel<SalesType> payload = new CrudViewModel<SalesType>
            {
                key = -5
            };
            Mock<DbSet<SalesType>> mockSet = CreateMockDbSet(salesTypeList.AsQueryable());
            Mock<ApplicationDbContext> mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.SalesType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            SalesTypeController controller = new SalesTypeController(mockContext.Object);
            // Act
            IActionResult result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            OkObjectResult okResult = (OkObjectResult)result;
            Assert.AreEqual(salesType, okResult.Value);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data)
            where T : class
        {
            Mock<DbSet<T>> mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }

        /// <summary>
        /// Tests that Insert returns OkObjectResult with the SalesType when given a valid payload.
        /// Verifies that Add and SaveChanges are called on the context.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithSalesTypeAndSavesChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesTypeController(mockContext.Object);
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = "Retail",
                Description = "Retail sales"
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "insert",
                value = salesType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Add(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles null value in payload by adding null to DbSet.
        /// Verifies that Add is called with null and SaveChanges is invoked.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndCallsSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new SalesTypeController(mockContext.Object);
            var payload = new CrudViewModel<SalesType>
            {
                action = "insert",
                value = null
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert properly handles payload with empty strings in SalesType properties.
        /// Verifies behavior with boundary string values.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesTypeController(mockContext.Object);
            var salesType = new SalesType
            {
                SalesTypeId = 0,
                SalesTypeName = string.Empty,
                Description = string.Empty
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "insert",
                value = salesType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles SalesType with extreme integer values.
        /// Verifies behavior with boundary numeric values for SalesTypeId.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        public void Insert_SalesTypeWithExtremeIds_ReturnsOkResult(int salesTypeId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesTypeController(mockContext.Object);
            var salesType = new SalesType
            {
                SalesTypeId = salesTypeId,
                SalesTypeName = "Test",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "insert",
                value = salesType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(salesType, okResult.Value);
            mockDbSet.Verify(d => d.Add(It.Is<SalesType>(s => s.SalesTypeId == salesTypeId)), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles very long strings in SalesType properties.
        /// Verifies behavior with edge case string lengths.
        /// </summary>
        [TestMethod]
        public void Insert_SalesTypeWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesTypeController(mockContext.Object);
            var longString = new string ('A', 10000);
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = longString,
                Description = longString
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "insert",
                value = salesType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(salesType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles strings with special and control characters.
        /// Verifies behavior with non-standard string content.
        /// </summary>
        [TestMethod]
        public void Insert_SalesTypeWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<SalesType>>();
            mockContext.Setup(c => c.SalesType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new SalesTypeController(mockContext.Object);
            var specialString = "Test\0\r\n\t<>&\"'";
            var salesType = new SalesType
            {
                SalesTypeId = 1,
                SalesTypeName = specialString,
                Description = specialString
            };
            var payload = new CrudViewModel<SalesType>
            {
                action = "insert",
                value = salesType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(It.Is<SalesType>(s => s.SalesTypeName == specialString)), Times.Once);
        }
    }
}