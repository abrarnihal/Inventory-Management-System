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
    /// Unit tests for the UnitOfMeasureController class.
    /// </summary>
    [TestClass]
    public class UnitOfMeasureControllerTests
    {
        /// <summary>
        /// Tests that GetUnitOfMeasure returns an empty list with count zero when the database is empty.
        /// </summary>
        [TestMethod]
        public async Task GetUnitOfMeasure_EmptyDatabase_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var emptyData = new List<UnitOfMeasure>().AsQueryable();
            var mockSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);
            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = await controller.GetUnitOfMeasure();

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
            var items = (List<UnitOfMeasure>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetUnitOfMeasure returns a list with one item and count one when the database contains a single record.
        /// </summary>
        [TestMethod]
        public async Task GetUnitOfMeasure_SingleItem_ReturnsListWithOneItemAndCountOne()
        {
            // Arrange
            var testData = new List<UnitOfMeasure>
            {
                new UnitOfMeasure { UnitOfMeasureId = 1, UnitOfMeasureName = "Piece", Description = "Individual item" }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);
            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = await controller.GetUnitOfMeasure();

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
            var items = (List<UnitOfMeasure>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("Piece", items[0].UnitOfMeasureName);
        }

        /// <summary>
        /// Tests that GetUnitOfMeasure returns all items with the correct count when the database contains multiple records.
        /// </summary>
        [TestMethod]
        public async Task GetUnitOfMeasure_MultipleItems_ReturnsListWithAllItemsAndCorrectCount()
        {
            // Arrange
            var testData = new List<UnitOfMeasure>
            {
                new UnitOfMeasure { UnitOfMeasureId = 1, UnitOfMeasureName = "Piece", Description = "Individual item" },
                new UnitOfMeasure { UnitOfMeasureId = 2, UnitOfMeasureName = "Kilogram", Description = "Weight measurement" },
                new UnitOfMeasure { UnitOfMeasureId = 3, UnitOfMeasureName = "Liter", Description = "Volume measurement" }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);
            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = await controller.GetUnitOfMeasure();

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
            var items = (List<UnitOfMeasure>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual("Piece", items[0].UnitOfMeasureName);
            Assert.AreEqual("Kilogram", items[1].UnitOfMeasureName);
            Assert.AreEqual("Liter", items[2].UnitOfMeasureName);
        }

        /// <summary>
        /// Creates a mock DbSet for testing Entity Framework Core operations.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sourceList">The source data as IQueryable.</param>
        /// <returns>A mocked DbSet.</returns>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> sourceList) where T : class
        {
            var mockSet = new Mock<DbSet<T>>();

            mockSet.As<IAsyncEnumerable<T>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<T>(sourceList.GetEnumerator()));

            mockSet.As<IQueryable<T>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<T>(sourceList.Provider));

            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(sourceList.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(sourceList.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(sourceList.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Helper class to enable async query operations on mocked DbSet.
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
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(Expression) })
                    .MakeGenericMethod(resultType)
                    .Invoke(this, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult });
            }
        }

        /// <summary>
        /// Helper class to enable async enumeration on mocked DbSet.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable)
                : base(enumerable)
            { }

            public TestAsyncEnumerable(Expression expression)
                : base(expression)
            { }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider
            {
                get { return new TestAsyncQueryProvider<T>(this); }
            }
        }

        /// <summary>
        /// Helper class to provide async enumerator functionality for mocked DbSet.
        /// </summary>
        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public T Current
            {
                get { return _inner.Current; }
            }

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
        /// Tests that Insert adds a valid UnitOfMeasure to the context, saves changes, and returns an Ok result with the entity.
        /// Input: Valid payload with a valid UnitOfMeasure.
        /// Expected: UnitOfMeasure is added to the context, SaveChanges is called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithUnitOfMeasure()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = 1,
                UnitOfMeasureName = "Kilogram",
                Description = "Unit of mass"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = unitOfMeasure
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert adds a null UnitOfMeasure to the context when payload.value is null.
        /// Input: Payload with null value property.
        /// Expected: Null is added to the context and SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndSavesChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = null
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that Insert handles UnitOfMeasure with minimal required properties correctly.
        /// Input: Payload with UnitOfMeasure containing only required properties.
        /// Expected: Entity is added, saved, and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_MinimalRequiredProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureName = "Meter"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = unitOfMeasure
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert handles UnitOfMeasure with special characters in string properties.
        /// Input: Payload with UnitOfMeasure containing special characters.
        /// Expected: Entity with special characters is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_SpecialCharactersInProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = 100,
                UnitOfMeasureName = "Special<>Characters&\"'",
                Description = "Line1\nLine2\tTab\r\nNewline"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = unitOfMeasure
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert handles UnitOfMeasure with empty string properties correctly.
        /// Input: Payload with UnitOfMeasure containing empty strings.
        /// Expected: Entity with empty strings is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_EmptyStringProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureName = "",
                Description = ""
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = unitOfMeasure
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert handles boundary value for UnitOfMeasureId (int.MaxValue).
        /// Input: Payload with UnitOfMeasure having UnitOfMeasureId set to int.MaxValue.
        /// Expected: Entity is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_MaxIntUnitOfMeasureId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = int.MaxValue,
                UnitOfMeasureName = "MaxId"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = unitOfMeasure
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert handles boundary value for UnitOfMeasureId (int.MinValue).
        /// Input: Payload with UnitOfMeasure having UnitOfMeasureId set to int.MinValue.
        /// Expected: Entity is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_MinIntUnitOfMeasureId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = int.MinValue,
                UnitOfMeasureName = "MinId"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = unitOfMeasure
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert handles very long string properties correctly.
        /// Input: Payload with UnitOfMeasure containing very long string values.
        /// Expected: Entity with long strings is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_VeryLongStringProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var longString = new string('A', 10000);
            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureName = longString,
                Description = longString
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                value = unitOfMeasure
            };

            var controller = new UnitOfMeasureController(mockContext.Object);

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            var okResult = result as OkObjectResult;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method successfully updates a UnitOfMeasure entity,
        /// saves changes to the database, and returns an OkObjectResult with the updated entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithUpdatedEntity()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = 1,
                UnitOfMeasureName = "Kilogram",
                Description = "Unit of mass"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = 1,
                antiForgery = "test",
                value = unitOfMeasure
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
            mockDbSet.Verify(d => d.Update(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles null value property in payload.
        /// Expected to pass null to Update method and potentially cause database error.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_CallsUpdateWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = 1,
                antiForgery = "test",
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
        /// Tests that Update method handles UnitOfMeasure with minimal required properties.
        /// Only UnitOfMeasureName is required, Description can be null/empty.
        /// </summary>
        [TestMethod]
        public void Update_MinimalUnitOfMeasure_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = 0,
                UnitOfMeasureName = "Unit",
                Description = null
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = 0,
                antiForgery = "",
                value = unitOfMeasure
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
            mockDbSet.Verify(d => d.Update(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles UnitOfMeasure with boundary UnitOfMeasureId values.
        /// Tests with int.MaxValue to ensure no overflow issues.
        /// </summary>
        [TestMethod]
        public void Update_MaxIntId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = int.MaxValue,
                UnitOfMeasureName = "TestUnit",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = int.MaxValue,
                antiForgery = "token",
                value = unitOfMeasure
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
            mockDbSet.Verify(d => d.Update(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles UnitOfMeasure with empty string properties.
        /// </summary>
        [TestMethod]
        public void Update_EmptyStringProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = 5,
                UnitOfMeasureName = "",
                Description = ""
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = 5,
                antiForgery = "",
                value = unitOfMeasure
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
            mockDbSet.Verify(d => d.Update(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles UnitOfMeasure with very long string properties.
        /// Tests boundary condition for string length.
        /// </summary>
        [TestMethod]
        public void Update_VeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var longString = new string('A', 10000);
            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = 10,
                UnitOfMeasureName = longString,
                Description = longString
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = 10,
                antiForgery = "test",
                value = unitOfMeasure
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
            mockDbSet.Verify(d => d.Update(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles UnitOfMeasure with special characters in string properties.
        /// </summary>
        [TestMethod]
        public void Update_SpecialCharactersInStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = 15,
                UnitOfMeasureName = "Test<>\"'&;/\\|",
                Description = "Special chars: !@#$%^&*()_+-={}[]|\\:\";<>?,./"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = 15,
                antiForgery = "test",
                value = unitOfMeasure
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
            mockDbSet.Verify(d => d.Update(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles negative UnitOfMeasureId values.
        /// Although typically IDs are positive, the method doesn't validate this.
        /// </summary>
        [TestMethod]
        public void Update_NegativeId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<UnitOfMeasure>>();

            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new UnitOfMeasureController(mockContext.Object);

            var unitOfMeasure = new UnitOfMeasure
            {
                UnitOfMeasureId = -1,
                UnitOfMeasureName = "NegativeId",
                Description = "Test with negative ID"
            };

            var payload = new CrudViewModel<UnitOfMeasure>
            {
                action = "update",
                key = -1,
                antiForgery = "test",
                value = unitOfMeasure
            };

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(unitOfMeasure, okResult.Value);
            mockDbSet.Verify(d => d.Update(unitOfMeasure), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove method searches for UnitOfMeasureId equal to 0 when payload.key is null.
        /// Convert.ToInt32(null) returns 0, so it queries for entity with id 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_SearchesForIdZeroAndReturnsOkWithNull()
        {
            // Arrange
            var data = new List<UnitOfMeasure>
            {
                new UnitOfMeasure { UnitOfMeasureId = 1, UnitOfMeasureName = "Unit1" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<UnitOfMeasure>>();
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);

            var controller = new UnitOfMeasureController(mockContext.Object);
            var payload = new CrudViewModel<UnitOfMeasure> { key = null };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null!), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method returns Ok with null value when entity is not found.
        /// FirstOrDefault returns null when no matching entity exists.
        /// </summary>
        [TestMethod]
        public void Remove_EntityNotFound_ReturnsOkWithNull()
        {
            // Arrange
            var data = new List<UnitOfMeasure>
            {
                new UnitOfMeasure { UnitOfMeasureId = 1, UnitOfMeasureName = "Unit1" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<UnitOfMeasure>>();
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);

            var controller = new UnitOfMeasureController(mockContext.Object);
            var payload = new CrudViewModel<UnitOfMeasure> { key = 999 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null!), Times.Once());
            mockContext.Verify(c => c.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method successfully removes entity and returns Ok with the removed entity.
        /// Tests with various valid key types including int, string representation of int.
        /// </summary>
        /// <param name="key">The key value to test.</param>
        /// <param name="expectedId">The expected UnitOfMeasureId to be queried.</param>
        [TestMethod]
        [DataRow(1, 1, DisplayName = "Integer key")]
        [DataRow("2", 2, DisplayName = "String key")]
        [DataRow(0, 0, DisplayName = "Zero key")]
        [DataRow(-1, -1, DisplayName = "Negative key")]
        public void Remove_ValidKey_RemovesEntityAndReturnsOk(object key, int expectedId)
        {
            // Arrange
            var entityToRemove = new UnitOfMeasure
            {
                UnitOfMeasureId = expectedId,
                UnitOfMeasureName = $"Unit{expectedId}",
                Description = "Test Description"
            };

            var data = new List<UnitOfMeasure>
            {
                entityToRemove,
                new UnitOfMeasure { UnitOfMeasureId = 999, UnitOfMeasureName = "OtherUnit" }
            }.AsQueryable();

            var mockSet = new Mock<DbSet<UnitOfMeasure>>();
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);

            var controller = new UnitOfMeasureController(mockContext.Object);
            var payload = new CrudViewModel<UnitOfMeasure> { key = key };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(UnitOfMeasure));

            var removedEntity = (UnitOfMeasure)okResult.Value;
            Assert.AreEqual(expectedId, removedEntity.UnitOfMeasureId);
            Assert.AreEqual($"Unit{expectedId}", removedEntity.UnitOfMeasureName);

            mockSet.Verify(m => m.Remove(It.Is<UnitOfMeasure>(u => u.UnitOfMeasureId == expectedId)), Times.Once());
            mockContext.Verify(c => c.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles boundary integer values for key parameter.
        /// Tests int.MinValue and int.MaxValue edge cases.
        /// </summary>
        /// <param name="key">The boundary key value to test.</param>
        [TestMethod]
        [DataRow(int.MinValue, DisplayName = "MinValue key")]
        [DataRow(int.MaxValue, DisplayName = "MaxValue key")]
        public void Remove_BoundaryIntegerKey_SearchesForEntityAndReturnsOk(int key)
        {
            // Arrange
            var data = new List<UnitOfMeasure>().AsQueryable();

            var mockSet = new Mock<DbSet<UnitOfMeasure>>();
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);

            var controller = new UnitOfMeasureController(mockContext.Object);
            var payload = new CrudViewModel<UnitOfMeasure> { key = key };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null!), Times.Once());
            mockContext.Verify(c => c.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles double values that can be converted to int.
        /// </summary>
        [TestMethod]
        public void Remove_DoubleKey_ConvertsToIntAndRemovesEntity()
        {
            // Arrange
            var entityToRemove = new UnitOfMeasure
            {
                UnitOfMeasureId = 5,
                UnitOfMeasureName = "Unit5",
                Description = "Test"
            };

            var data = new List<UnitOfMeasure> { entityToRemove }.AsQueryable();

            var mockSet = new Mock<DbSet<UnitOfMeasure>>();
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<UnitOfMeasure>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.UnitOfMeasure).Returns(mockSet.Object);

            var controller = new UnitOfMeasureController(mockContext.Object);
            var payload = new CrudViewModel<UnitOfMeasure> { key = 5.7 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(UnitOfMeasure));

            var removedEntity = (UnitOfMeasure)okResult.Value;
            Assert.AreEqual(5, removedEntity.UnitOfMeasureId);

            mockSet.Verify(m => m.Remove(It.IsAny<UnitOfMeasure>()), Times.Once());
            mockContext.Verify(c => c.SaveChanges(), Times.Once());
        }
    }
}