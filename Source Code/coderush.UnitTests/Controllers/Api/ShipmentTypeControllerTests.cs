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
    /// Unit tests for the ShipmentTypeController class.
    /// </summary>
    [TestClass]
    public class ShipmentTypeControllerTests
    {
        /// <summary>
        /// Tests that Insert method successfully adds a valid ShipmentType and returns OkObjectResult.
        /// Input: Valid CrudViewModel with valid ShipmentType.
        /// Expected: ShipmentType is added to context, SaveChanges is called, and OkObjectResult is returned with the ShipmentType.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithShipmentType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 1,
                ShipmentTypeName = "Express",
                Description = "Express shipping"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType,
                action = "insert",
                key = 1,
                antiForgery = "token"
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.AreEqual(shipmentType, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles null payload.value by passing it to Add method.
        /// Input: Valid payload with null value property.
        /// Expected: Add is called with null and SaveChanges is invoked.
        /// </summary>
        [TestMethod]
        public void Insert_NullPayloadValue_CallsAddWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                value = null,
                action = "insert",
                key = 1,
                antiForgery = "token"
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method adds ShipmentType with minimal required properties.
        /// Input: ShipmentType with only required ShipmentTypeName property set.
        /// Expected: ShipmentType is added successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_MinimalShipmentType_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeName = "Standard"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles ShipmentType with special characters in properties.
        /// Input: ShipmentType with special characters in ShipmentTypeName and Description.
        /// Expected: ShipmentType is added successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ShipmentTypeWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 100,
                ShipmentTypeName = "Test<>\"'&Name",
                Description = "Description with \n\r\t special chars"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles ShipmentType with boundary values for Id.
        /// Input: ShipmentType with int.MaxValue as ShipmentTypeId.
        /// Expected: ShipmentType is added successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ShipmentTypeWithMaxId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = int.MaxValue,
                ShipmentTypeName = "MaxIdTest"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles ShipmentType with empty string properties.
        /// Input: ShipmentType with empty strings for ShipmentTypeName and Description.
        /// Expected: ShipmentType is added successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ShipmentTypeWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 5,
                ShipmentTypeName = "",
                Description = ""
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles ShipmentType with very long strings.
        /// Input: ShipmentType with very long strings for ShipmentTypeName and Description.
        /// Expected: ShipmentType is added successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ShipmentTypeWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var longString = new string ('A', 10000);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 6,
                ShipmentTypeName = longString,
                Description = longString
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that GetShipmentType returns an empty list with count zero when no shipment types exist in the database.
        /// Input: Empty database (no ShipmentType records).
        /// Expected: Returns OkObjectResult with empty Items list and Count = 0.
        /// </summary>
        [TestMethod]
        public async Task GetShipmentType_EmptyDatabase_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var emptyData = new List<ShipmentType>();
            var mockSet = CreateMockDbSet(emptyData.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            var controller = new ShipmentTypeController(mockContext.Object);
            // Act
            var result = await controller.GetShipmentType();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.Items);
            Assert.AreEqual(0, value.Count);
            Assert.AreEqual(0, ((List<ShipmentType>)value.Items).Count);
        }

        /// <summary>
        /// Tests that GetShipmentType returns a single item with count one when exactly one shipment type exists.
        /// Input: Database with one ShipmentType record.
        /// Expected: Returns OkObjectResult with Items list containing one element and Count = 1.
        /// </summary>
        [TestMethod]
        public async Task GetShipmentType_SingleItem_ReturnsSingleItemWithCountOne()
        {
            // Arrange
            var singleItem = new ShipmentType
            {
                ShipmentTypeId = 1,
                ShipmentTypeName = "Standard",
                Description = "Standard Shipment"
            };
            var data = new List<ShipmentType>
            {
                singleItem
            };
            var mockSet = CreateMockDbSet(data.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            var controller = new ShipmentTypeController(mockContext.Object);
            // Act
            var result = await controller.GetShipmentType();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.Items);
            Assert.AreEqual(1, value.Count);
            var items = (List<ShipmentType>)value.Items;
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, items[0].ShipmentTypeId);
            Assert.AreEqual("Standard", items[0].ShipmentTypeName);
            Assert.AreEqual("Standard Shipment", items[0].Description);
        }

        /// <summary>
        /// Tests that GetShipmentType returns all items with correct count when multiple shipment types exist.
        /// Input: Database with multiple ShipmentType records.
        /// Expected: Returns OkObjectResult with Items list containing all elements and Count matching the number of items.
        /// </summary>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public async Task GetShipmentType_MultipleItems_ReturnsAllItemsWithCorrectCount(int itemCount)
        {
            // Arrange
            var data = new List<ShipmentType>();
            for (int i = 1; i <= itemCount; i++)
            {
                data.Add(new ShipmentType { ShipmentTypeId = i, ShipmentTypeName = $"Type{i}", Description = $"Description{i}" });
            }

            var mockSet = CreateMockDbSet(data.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            var controller = new ShipmentTypeController(mockContext.Object);
            // Act
            var result = await controller.GetShipmentType();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.IsNotNull(value.Items);
            Assert.AreEqual(itemCount, value.Count);
            var items = (List<ShipmentType>)value.Items;
            Assert.AreEqual(itemCount, items.Count);
        }

        /// <summary>
        /// Tests that GetShipmentType returns items with special characters and edge case strings in their properties.
        /// Input: Database with ShipmentType records containing empty strings, special characters, and very long strings.
        /// Expected: Returns OkObjectResult with all items correctly preserved.
        /// </summary>
        [TestMethod]
        public async Task GetShipmentType_ItemsWithSpecialCharactersAndEdgeCaseStrings_ReturnsAllItemsCorrectly()
        {
            // Arrange
            var data = new List<ShipmentType>
            {
                new ShipmentType
                {
                    ShipmentTypeId = 1,
                    ShipmentTypeName = "",
                    Description = ""
                },
                new ShipmentType
                {
                    ShipmentTypeId = 2,
                    ShipmentTypeName = "   ",
                    Description = "  \t\n  "
                },
                new ShipmentType
                {
                    ShipmentTypeId = 3,
                    ShipmentTypeName = "Type with special chars: !@#$%^&*()",
                    Description = "Description with unicode: 测试 テスト 🚀"
                },
                new ShipmentType
                {
                    ShipmentTypeId = 4,
                    ShipmentTypeName = new string ('A', 1000),
                    Description = new string ('B', 5000)
                }
            };
            var mockSet = CreateMockDbSet(data.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            var controller = new ShipmentTypeController(mockContext.Object);
            // Act
            var result = await controller.GetShipmentType();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.AreEqual(4, value.Count);
            var items = (List<ShipmentType>)value.Items;
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual("", items[0].ShipmentTypeName);
            Assert.AreEqual("   ", items[1].ShipmentTypeName);
            Assert.AreEqual("Type with special chars: !@#$%^&*()", items[2].ShipmentTypeName);
            Assert.AreEqual(1000, items[3].ShipmentTypeName.Length);
        }

        /// <summary>
        /// Tests that GetShipmentType returns items with boundary integer values for ShipmentTypeId.
        /// Input: Database with ShipmentType records having boundary integer IDs (int.MinValue, int.MaxValue, 0).
        /// Expected: Returns OkObjectResult with all items correctly preserved with their boundary ID values.
        /// </summary>
        [TestMethod]
        public async Task GetShipmentType_ItemsWithBoundaryIntegerIds_ReturnsAllItemsCorrectly()
        {
            // Arrange
            var data = new List<ShipmentType>
            {
                new ShipmentType
                {
                    ShipmentTypeId = int.MinValue,
                    ShipmentTypeName = "MinValue",
                    Description = "Min ID"
                },
                new ShipmentType
                {
                    ShipmentTypeId = 0,
                    ShipmentTypeName = "Zero",
                    Description = "Zero ID"
                },
                new ShipmentType
                {
                    ShipmentTypeId = int.MaxValue,
                    ShipmentTypeName = "MaxValue",
                    Description = "Max ID"
                }
            };
            var mockSet = CreateMockDbSet(data.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            var controller = new ShipmentTypeController(mockContext.Object);
            // Act
            var result = await controller.GetShipmentType();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            Assert.AreEqual(3, value.Count);
            var items = (List<ShipmentType>)value.Items;
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(int.MinValue, items[0].ShipmentTypeId);
            Assert.AreEqual(0, items[1].ShipmentTypeId);
            Assert.AreEqual(int.MaxValue, items[2].ShipmentTypeId);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations.
        /// </summary>
        /// <param name = "data">The queryable data to be returned by the mock DbSet.</param>
        /// <returns>A mock DbSet configured for async operations.</returns>
        private Mock<DbSet<ShipmentType>> CreateMockDbSet(IQueryable<ShipmentType> data)
        {
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<ShipmentType>(data.Provider));
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.As<IAsyncEnumerable<ShipmentType>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<ShipmentType>(data.GetEnumerator()));
            return mockSet;
        }

        /// <summary>
        /// Helper class to enable async query operations for in-memory test data.
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
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(System.Linq.Expressions.Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(System.Linq.Expressions.Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider).GetMethod(name: nameof(IQueryProvider.Execute), genericParameterCount: 1, types: new[] { typeof(System.Linq.Expressions.Expression) }).MakeGenericMethod(resultType).Invoke(this, new[] { expression });
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(resultType).Invoke(null, new[] { executionResult });
            }
        }

        /// <summary>
        /// Helper class to enable async enumeration for in-memory test data.
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
        /// Helper class to implement async enumerator for in-memory test data.
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
        /// Tests that Remove method successfully removes an existing entity and returns Ok result.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKeyWithExistingEntity_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>
            {
                new ShipmentType
                {
                    ShipmentTypeId = 1,
                    ShipmentTypeName = "Air",
                    Description = "Air shipping"
                },
                new ShipmentType
                {
                    ShipmentTypeId = 2,
                    ShipmentTypeName = "Sea",
                    Description = "Sea shipping"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = 1
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedShipment = okResult.Value as ShipmentType;
            Assert.IsNotNull(returnedShipment);
            Assert.AreEqual(1, returnedShipment.ShipmentTypeId);
            mockSet.Verify(m => m.Remove(It.IsAny<ShipmentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles string key conversion correctly.
        /// Input: String representation of integer key for existing entity.
        /// Expected: Successfully converts, removes entity, and returns Ok result.
        /// </summary>
        [TestMethod]
        public void Remove_StringKeyWithExistingEntity_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>
            {
                new ShipmentType
                {
                    ShipmentTypeId = 5,
                    ShipmentTypeName = "Ground",
                    Description = "Ground shipping"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = "5"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            mockSet.Verify(m => m.Remove(It.IsAny<ShipmentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles null key by converting to 0.
        /// Input: Payload with null key.
        /// Expected: Searches for entity with ShipmentTypeId == 0, removes if found or null.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_SearchesForZeroId()
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>().AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = null
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockSet.Verify(m => m.Remove(It.IsAny<ShipmentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method removes null when entity is not found.
        /// Input: Valid payload with key that doesn't exist in database.
        /// Expected: FirstOrDefault returns null, Remove(null) is called.
        /// </summary>
        [TestMethod]
        public void Remove_NonExistentKey_RemovesNull()
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>
            {
                new ShipmentType
                {
                    ShipmentTypeId = 1,
                    ShipmentTypeName = "Air",
                    Description = "Air shipping"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = 999
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles boundary numeric values correctly.
        /// Input: Boundary numeric keys (int.MinValue, int.MaxValue, 0, negative).
        /// Expected: Converts and searches for corresponding ShipmentTypeId.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-999)]
        public void Remove_BoundaryNumericKeys_HandlesCorrectly(int key)
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>().AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = key
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockSet.Verify(m => m.Remove(It.IsAny<ShipmentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles numeric string boundaries correctly.
        /// Input: String representations of boundary numeric values.
        /// Expected: Successfully converts and searches for corresponding ShipmentTypeId.
        /// </summary>
        [TestMethod]
        [DataRow("2147483647")] // int.MaxValue
        [DataRow("-2147483648")] // int.MinValue
        [DataRow("0")]
        public void Remove_BoundaryNumericStringKeys_HandlesCorrectly(string key)
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>().AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = key
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockSet.Verify(m => m.Remove(It.IsAny<ShipmentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method handles double key by converting to int.
        /// Input: Payload with double value as key.
        /// Expected: Converts double to int (truncates decimal) and proceeds.
        /// </summary>
        [TestMethod]
        [DataRow(5.9)]
        [DataRow(10.1)]
        [DataRow(-3.7)]
        public void Remove_DoubleKey_TruncatesAndConverts(double key)
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>().AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = key
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockSet.Verify(m => m.Remove(It.IsAny<ShipmentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method verifies SaveChanges is called after removal.
        /// Input: Valid payload with existing entity key.
        /// Expected: SaveChanges is called exactly once.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKey_CallsSaveChanges()
        {
            // Arrange
            var shipmentTypes = new List<ShipmentType>
            {
                new ShipmentType
                {
                    ShipmentTypeId = 10,
                    ShipmentTypeName = "Express",
                    Description = "Express shipping"
                }
            }.AsQueryable();
            var mockSet = new Mock<DbSet<ShipmentType>>();
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Provider).Returns(shipmentTypes.Provider);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.Expression).Returns(shipmentTypes.Expression);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.ElementType).Returns(shipmentTypes.ElementType);
            mockSet.As<IQueryable<ShipmentType>>().Setup(m => m.GetEnumerator()).Returns(shipmentTypes.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                key = 10
            };
            // Act
            controller.Remove(payload);
            // Assert
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Update method successfully updates a ShipmentType entity and returns OkObjectResult with the entity.
        /// Input: Valid payload with a valid ShipmentType.
        /// Expected: Update and SaveChanges are called, and Ok result is returned with the ShipmentType.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithShipmentType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 1,
                ShipmentTypeName = "Express",
                Description = "Express shipping"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = shipmentType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(shipmentType, okResult.Value);
            mockDbSet.Verify(db => db.Update(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles null value in payload.
        /// Input: Valid payload with null value property.
        /// Expected: Update is called with null and SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_UpdatesWithNullValue()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new ShipmentTypeController(mockContext.Object);
            var payload = new CrudViewModel<ShipmentType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = null
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(db => db.Update(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles ShipmentType with minimum valid ID value.
        /// Input: ShipmentType with ShipmentTypeId set to int.MinValue.
        /// Expected: Update and SaveChanges are called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Update_ShipmentTypeWithMinIntId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = int.MinValue,
                ShipmentTypeName = "Test",
                Description = "Test description"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(db => db.Update(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles ShipmentType with maximum valid ID value.
        /// Input: ShipmentType with ShipmentTypeId set to int.MaxValue.
        /// Expected: Update and SaveChanges are called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Update_ShipmentTypeWithMaxIntId_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = int.MaxValue,
                ShipmentTypeName = "Test",
                Description = "Test description"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(db => db.Update(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles ShipmentType with empty string properties.
        /// Input: ShipmentType with empty ShipmentTypeName and Description.
        /// Expected: Update and SaveChanges are called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Update_ShipmentTypeWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 1,
                ShipmentTypeName = string.Empty,
                Description = string.Empty
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var returnedShipmentType = (ShipmentType)okResult.Value;
            Assert.AreEqual(string.Empty, returnedShipmentType.ShipmentTypeName);
            Assert.AreEqual(string.Empty, returnedShipmentType.Description);
            mockDbSet.Verify(db => db.Update(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles ShipmentType with very long string properties.
        /// Input: ShipmentType with very long ShipmentTypeName and Description (1000 characters each).
        /// Expected: Update and SaveChanges are called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Update_ShipmentTypeWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var longString = new string ('A', 1000);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 1,
                ShipmentTypeName = longString,
                Description = longString
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(db => db.Update(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles ShipmentType with special characters in string properties.
        /// Input: ShipmentType with special characters including unicode, newlines, and control characters.
        /// Expected: Update and SaveChanges are called, and Ok result is returned.
        /// </summary>
        [TestMethod]
        public void Update_ShipmentTypeWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 1,
                ShipmentTypeName = "Test\n\r\t<>\"'&;{}[]",
                Description = "Test™®€£¥©中文日本語"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(db => db.Update(shipmentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method correctly returns the same ShipmentType instance that was passed in.
        /// Input: Valid payload with a ShipmentType.
        /// Expected: The exact same ShipmentType reference is returned in the Ok result.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsSameShipmentTypeInstance()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<ShipmentType>>();
            mockContext.Setup(c => c.ShipmentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new ShipmentTypeController(mockContext.Object);
            var shipmentType = new ShipmentType
            {
                ShipmentTypeId = 99,
                ShipmentTypeName = "Priority",
                Description = "Priority shipping"
            };
            var payload = new CrudViewModel<ShipmentType>
            {
                value = shipmentType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            var okResult = (OkObjectResult)result;
            Assert.AreSame(shipmentType, okResult.Value);
        }
    }
}