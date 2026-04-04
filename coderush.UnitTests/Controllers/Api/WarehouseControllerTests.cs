using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
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
    /// Unit tests for the WarehouseController class.
    /// </summary>
    [TestClass]
    public class WarehouseControllerTests
    {
        /// <summary>
        /// Tests that Insert returns OkObjectResult with the warehouse when given a valid payload.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithWarehouse()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 1,
                WarehouseName = "Main Warehouse",
                Description = "Primary storage facility",
                BranchId = 100
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(warehouse, okResult.Value);
            mockDbSet.Verify(d => d.Add(warehouse), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles payload with null value property and calls Add with null.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var payload = new CrudViewModel<Warehouse>
            {
                value = null
            };
            var controller = new WarehouseController(mockContext.Object);
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
        /// Tests that Insert handles warehouse with minimal required fields.
        /// </summary>
        [TestMethod]
        public void Insert_WarehouseWithMinimalFields_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 0,
                WarehouseName = "W",
                Description = null,
                BranchId = 0
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(warehouse, okResult.Value);
            mockDbSet.Verify(d => d.Add(warehouse), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles warehouse with boundary values for integer fields.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue, int.MinValue)]
        [DataRow(int.MaxValue, int.MaxValue)]
        [DataRow(0, 0)]
        [DataRow(-1, -1)]
        public void Insert_WarehouseWithBoundaryIntegerValues_ReturnsOkResult(int warehouseId, int branchId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = warehouseId,
                WarehouseName = "Test Warehouse",
                Description = "Test Description",
                BranchId = branchId
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(warehouse), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles warehouse with various string edge cases for WarehouseName.
        /// </summary>
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("   ")]
        [DataRow("A")]
        public void Insert_WarehouseWithEdgeCaseStrings_ReturnsOkResult(string warehouseName)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 1,
                WarehouseName = warehouseName,
                Description = "Description",
                BranchId = 1
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(warehouse, okResult.Value);
            mockDbSet.Verify(d => d.Add(warehouse), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles warehouse with very long string values.
        /// </summary>
        [TestMethod]
        public void Insert_WarehouseWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var longString = new string ('A', 10000);
            var warehouse = new Warehouse
            {
                WarehouseId = 1,
                WarehouseName = longString,
                Description = longString,
                BranchId = 1
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(warehouse), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles warehouse with special characters in string fields.
        /// </summary>
        [TestMethod]
        public void Insert_WarehouseWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 1,
                WarehouseName = "Test<>\"'&\n\r\t\0",
                Description = "Special chars: !@#$%^&*()_+-=[]{}|;:',.<>?/~`",
                BranchId = 1
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(warehouse), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that GetWarehouse returns OkObjectResult with empty Items list and Count of 0 when database contains no warehouses.
        /// </summary>
        [TestMethod]
        public async Task GetWarehouse_EmptyDatabase_ReturnsOkResultWithEmptyItemsAndZeroCount()
        {
            // Arrange
            var emptyWarehouses = new List<Warehouse>();
            var mockDbSet = CreateMockDbSet(emptyWarehouses);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = await controller.GetWarehouse();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var valueType = okResult.Value.GetType();
            var itemsProperty = valueType.GetProperty("Items");
            var countProperty = valueType.GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(okResult.Value) as List<Warehouse>;
            var count = (int)countProperty.GetValue(okResult.Value)!;
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetWarehouse returns OkObjectResult with single Warehouse item and Count of 1 when database contains one warehouse.
        /// </summary>
        [TestMethod]
        public async Task GetWarehouse_SingleWarehouse_ReturnsOkResultWithOneItemAndCountOne()
        {
            // Arrange
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    WarehouseId = 1,
                    WarehouseName = "Main Warehouse",
                    Description = "Primary storage",
                    BranchId = 100
                }
            };
            var mockDbSet = CreateMockDbSet(warehouses);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = await controller.GetWarehouse();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var valueType = okResult.Value.GetType();
            var itemsProperty = valueType.GetProperty("Items");
            var countProperty = valueType.GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(okResult.Value) as List<Warehouse>;
            var count = (int)countProperty.GetValue(okResult.Value)!;
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].WarehouseId);
            Assert.AreEqual("Main Warehouse", items[0].WarehouseName);
        }

        /// <summary>
        /// Tests that GetWarehouse returns OkObjectResult with multiple Warehouse items and correct Count when database contains multiple warehouses.
        /// </summary>
        [TestMethod]
        public async Task GetWarehouse_MultipleWarehouses_ReturnsOkResultWithAllItemsAndCorrectCount()
        {
            // Arrange
            var warehouses = new List<Warehouse>
            {
                new Warehouse
                {
                    WarehouseId = 1,
                    WarehouseName = "Warehouse A",
                    Description = "First",
                    BranchId = 100
                },
                new Warehouse
                {
                    WarehouseId = 2,
                    WarehouseName = "Warehouse B",
                    Description = "Second",
                    BranchId = 200
                },
                new Warehouse
                {
                    WarehouseId = 3,
                    WarehouseName = "Warehouse C",
                    Description = "Third",
                    BranchId = 300
                }
            };
            var mockDbSet = CreateMockDbSet(warehouses);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = await controller.GetWarehouse();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var valueType = okResult.Value.GetType();
            var itemsProperty = valueType.GetProperty("Items");
            var countProperty = valueType.GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(okResult.Value) as List<Warehouse>;
            var count = (int)countProperty.GetValue(okResult.Value)!;
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual(1, items[0].WarehouseId);
            Assert.AreEqual(2, items[1].WarehouseId);
            Assert.AreEqual(3, items[2].WarehouseId);
        }

        /// <summary>
        /// Tests that GetWarehouse returns OkObjectResult with large number of warehouses and correct Count.
        /// </summary>
        [TestMethod]
        public async Task GetWarehouse_LargeNumberOfWarehouses_ReturnsOkResultWithAllItemsAndCorrectCount()
        {
            // Arrange
            var warehouses = new List<Warehouse>();
            for (int i = 1; i <= 1000; i++)
            {
                warehouses.Add(new Warehouse { WarehouseId = i, WarehouseName = $"Warehouse {i}", Description = $"Description {i}", BranchId = i * 10 });
            }

            var mockDbSet = CreateMockDbSet(warehouses);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = await controller.GetWarehouse();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var valueType = okResult.Value.GetType();
            var itemsProperty = valueType.GetProperty("Items");
            var countProperty = valueType.GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(okResult.Value) as List<Warehouse>;
            var count = (int)countProperty.GetValue(okResult.Value)!;
            Assert.IsNotNull(items);
            Assert.AreEqual(1000, items.Count);
            Assert.AreEqual(1000, count);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations.
        /// </summary>
        /// <param name = "sourceList">The source data for the DbSet.</param>
        /// <returns>A mocked DbSet.</returns>
        private static Mock<DbSet<Warehouse>> CreateMockDbSet(List<Warehouse> sourceList)
        {
            var queryable = sourceList.AsQueryable();
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockDbSet.As<IQueryable<Warehouse>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Warehouse>(queryable.Provider));
            mockDbSet.As<IQueryable<Warehouse>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<Warehouse>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<Warehouse>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            mockDbSet.As<IAsyncEnumerable<Warehouse>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Warehouse>(queryable.GetEnumerator()));
            return mockDbSet;
        }

        /// <summary>
        /// Helper class to support async query operations in mocked DbSet.
        /// </summary>
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
                return _inner.Execute(expression)!;
            }

            public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executeMethod = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) })!.MakeGenericMethod(resultType);
                var result = executeMethod.Invoke(_inner, new object[] { expression });
                return (TResult)Activator.CreateInstance(typeof(TResult), result)!;
            }
        }

        /// <summary>
        /// Helper class to support async enumeration in mocked DbSet.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
            {
            }

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
        /// Helper class to support async enumeration in mocked DbSet.
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
        /// Tests that Update returns OkObjectResult with the warehouse when given a valid payload.
        /// Input: Valid CrudViewModel with a valid Warehouse.
        /// Expected: Returns OkObjectResult containing the warehouse.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkObjectResultWithWarehouse()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 1,
                WarehouseName = "Main Warehouse",
                Description = "Primary storage",
                BranchId = 10
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(warehouse, okResult.Value);
        }

        /// <summary>
        /// Tests that Update calls the Update method on the DbSet with the correct warehouse.
        /// Input: Valid CrudViewModel with a warehouse.
        /// Expected: DbSet.Update is called exactly once with the warehouse.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsUpdateOnDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 2,
                WarehouseName = "Secondary Warehouse",
                Description = "Backup storage",
                BranchId = 20
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            controller.Update(payload);
            // Assert
            mockDbSet.Verify(d => d.Update(warehouse), Times.Once);
        }

        /// <summary>
        /// Tests that Update calls SaveChanges on the DbContext.
        /// Input: Valid CrudViewModel with a warehouse.
        /// Expected: SaveChanges is called exactly once.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 3,
                WarehouseName = "Storage A",
                Description = "East wing",
                BranchId = 5
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            controller.Update(payload);
            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests Update with warehouse having boundary values for integer properties.
        /// Input: Warehouse with WarehouseId = int.MaxValue and BranchId = int.MaxValue.
        /// Expected: Returns OkObjectResult with the warehouse.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue, int.MaxValue, DisplayName = "MaxValues")]
        [DataRow(int.MinValue, int.MinValue, DisplayName = "MinValues")]
        [DataRow(0, 0, DisplayName = "ZeroValues")]
        public void Update_WarehouseWithBoundaryIntegerValues_ReturnsOkResult(int warehouseId, int branchId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = warehouseId,
                WarehouseName = "Boundary Test Warehouse",
                Description = "Testing boundaries",
                BranchId = branchId
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(warehouse, okResult.Value);
        }

        /// <summary>
        /// Tests Update with warehouse having various string edge cases.
        /// Input: Warehouse with empty, whitespace, very long, or special character strings.
        /// Expected: Returns OkObjectResult with the warehouse.
        /// </summary>
        [TestMethod]
        [DataRow("", "", DisplayName = "EmptyStrings")]
        [DataRow("   ", "   ", DisplayName = "WhitespaceStrings")]
        [DataRow("A", "B", DisplayName = "SingleCharacterStrings")]
        public void Update_WarehouseWithStringEdgeCases_ReturnsOkResult(string warehouseName, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 100,
                WarehouseName = warehouseName,
                Description = description,
                BranchId = 1
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(warehouse, okResult.Value);
        }

        /// <summary>
        /// Tests Update with warehouse having very long string values.
        /// Input: Warehouse with very long WarehouseName and Description (1000+ characters).
        /// Expected: Returns OkObjectResult with the warehouse.
        /// </summary>
        [TestMethod]
        public void Update_WarehouseWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var longString = new string ('A', 10000);
            var warehouse = new Warehouse
            {
                WarehouseId = 500,
                WarehouseName = longString,
                Description = longString,
                BranchId = 50
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(warehouse, okResult.Value);
        }

        /// <summary>
        /// Tests Update with warehouse having special and control characters in strings.
        /// Input: Warehouse with special characters, newlines, tabs, and Unicode characters.
        /// Expected: Returns OkObjectResult with the warehouse.
        /// </summary>
        [TestMethod]
        public void Update_WarehouseWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Warehouse>>();
            mockContext.Setup(c => c.Warehouse).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var warehouse = new Warehouse
            {
                WarehouseId = 600,
                WarehouseName = "Test<>&\"'\n\t\r",
                Description = "Unicode: 你好世界 🚀 Ñoño",
                BranchId = 60
            };
            var payload = new CrudViewModel<Warehouse>
            {
                value = warehouse
            };
            var controller = new WarehouseController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreSame(warehouse, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove returns Ok with the warehouse when a valid payload with an existing warehouse id is provided.
        /// Input: Valid payload with key matching an existing warehouse.
        /// Expected: OkObjectResult containing the removed warehouse.
        /// </summary>
        [TestMethod]
        public void Remove_ValidPayloadWithExistingWarehouse_ReturnsOkWithWarehouse()
        {
            // Arrange
            var warehouseId = 1;
            var warehouse = new Warehouse
            {
                WarehouseId = warehouseId,
                WarehouseName = "Main Warehouse",
                Description = "Primary storage",
                BranchId = 1
            };
            var warehouses = new List<Warehouse>
            {
                warehouse
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Warehouse>>();
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.Provider).Returns(warehouses.Provider);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.Expression).Returns(warehouses.Expression);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.ElementType).Returns(warehouses.ElementType);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.GetEnumerator()).Returns(warehouses.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Warehouse).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new WarehouseController(mockContext.Object);
            var payload = new CrudViewModel<Warehouse>
            {
                key = warehouseId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(warehouse, okResult.Value);
            mockSet.Verify(m => m.Remove(warehouse), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully removes and saves a warehouse with negative id.
        /// Input: Payload with negative warehouse id that exists in database.
        /// Expected: OkObjectResult containing the removed warehouse.
        /// </summary>
        [TestMethod]
        public void Remove_NegativeWarehouseId_ReturnsOkWithWarehouse()
        {
            // Arrange
            var warehouseId = -5;
            var warehouse = new Warehouse
            {
                WarehouseId = warehouseId,
                WarehouseName = "Negative ID Warehouse",
                Description = "Test",
                BranchId = 1
            };
            var warehouses = new List<Warehouse>
            {
                warehouse
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Warehouse>>();
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.Provider).Returns(warehouses.Provider);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.Expression).Returns(warehouses.Expression);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.ElementType).Returns(warehouses.ElementType);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.GetEnumerator()).Returns(warehouses.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Warehouse).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new WarehouseController(mockContext.Object);
            var payload = new CrudViewModel<Warehouse>
            {
                key = warehouseId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(warehouse, okResult.Value);
            mockSet.Verify(m => m.Remove(warehouse), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles decimal key by converting it to integer.
        /// Input: Payload with decimal key value.
        /// Expected: Converts decimal to int and searches for warehouse.
        /// </summary>
        [TestMethod]
        public void Remove_DecimalKey_ConvertsToIntAndSearches()
        {
            // Arrange
            var warehouseId = 5;
            var warehouse = new Warehouse
            {
                WarehouseId = warehouseId,
                WarehouseName = "Test Warehouse",
                Description = "Test",
                BranchId = 1
            };
            var warehouses = new List<Warehouse>
            {
                warehouse
            }.AsQueryable();
            var mockSet = new Mock<DbSet<Warehouse>>();
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.Provider).Returns(warehouses.Provider);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.Expression).Returns(warehouses.Expression);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.ElementType).Returns(warehouses.ElementType);
            mockSet.As<IQueryable<Warehouse>>().Setup(m => m.GetEnumerator()).Returns(warehouses.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Warehouse).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new WarehouseController(mockContext.Object);
            var payload = new CrudViewModel<Warehouse>
            {
                key = 5.7m
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(warehouse), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }
    }
}