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
    /// Contains unit tests for the VendorTypeController class.
    /// </summary>
    [TestClass]
    public class VendorTypeControllerTests
    {
        /// <summary>
        /// Tests that GetVendorType returns an empty list with count 0 when no vendor types exist in the database.
        /// </summary>
        [TestMethod]
        public async Task GetVendorType_EmptyDatabase_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var emptyData = new List<VendorType>().AsQueryable();
            var mockSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.VendorType).Returns(mockSet.Object);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = await controller.GetVendorType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(value) as List<VendorType>;
            var count = (int? )countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetVendorType returns a single vendor type with count 1 when database contains one item.
        /// </summary>
        [TestMethod]
        public async Task GetVendorType_SingleItem_ReturnsSingleItemWithCount()
        {
            // Arrange
            var testData = new List<VendorType>
            {
                new VendorType
                {
                    VendorTypeId = 1,
                    VendorTypeName = "Supplier",
                    Description = "Main supplier"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.VendorType).Returns(mockSet.Object);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = await controller.GetVendorType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(value) as List<VendorType>;
            var count = (int? )countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].VendorTypeId);
            Assert.AreEqual("Supplier", items[0].VendorTypeName);
        }

        /// <summary>
        /// Tests that GetVendorType returns all vendor types with correct count when database contains multiple items.
        /// </summary>
        [TestMethod]
        public async Task GetVendorType_MultipleItems_ReturnsAllItemsWithCorrectCount()
        {
            // Arrange
            var testData = new List<VendorType>
            {
                new VendorType
                {
                    VendorTypeId = 1,
                    VendorTypeName = "Supplier",
                    Description = "Main supplier"
                },
                new VendorType
                {
                    VendorTypeId = 2,
                    VendorTypeName = "Distributor",
                    Description = "Distribution network"
                },
                new VendorType
                {
                    VendorTypeId = 3,
                    VendorTypeName = "Manufacturer",
                    Description = "Product manufacturer"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.VendorType).Returns(mockSet.Object);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = await controller.GetVendorType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(value) as List<VendorType>;
            var count = (int? )countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual("Supplier", items[0].VendorTypeName);
            Assert.AreEqual("Distributor", items[1].VendorTypeName);
            Assert.AreEqual("Manufacturer", items[2].VendorTypeName);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations.
        /// </summary>
        /// <typeparam name = "T">The entity type.</typeparam>
        /// <param name = "sourceData">The source data to populate the mock DbSet.</param>
        /// <returns>A mock DbSet configured for async operations.</returns>
        private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> sourceData)
            where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(sourceData.Provider));
            mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(sourceData.Expression);
            mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(sourceData.ElementType);
            mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(sourceData.GetEnumerator());
            mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(sourceData.GetEnumerator()));
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
                var result = executeMethod?.Invoke(this, new object[] { expression });
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(resultType).Invoke(null, new[] { result })!;
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

            public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
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
        /// Tests that Insert method successfully adds a valid VendorType to the context,
        /// saves changes, and returns an OkObjectResult with the VendorType.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayloadWithVendorType_ReturnsOkResultWithVendorType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var vendorType = new VendorType
            {
                VendorTypeId = 1,
                VendorTypeName = "Test Vendor",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<VendorType>
            {
                action = "insert",
                value = vendorType
            };
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
            mockDbSet.Verify(d => d.Add(vendorType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method handles a VendorType with minimal properties (just required fields).
        /// </summary>
        [TestMethod]
        public void Insert_VendorTypeWithMinimalProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var vendorType = new VendorType
            {
                VendorTypeName = "Minimal Vendor"
            };
            var payload = new CrudViewModel<VendorType>
            {
                value = vendorType
            };
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(vendorType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method handles a VendorType with boundary values for VendorTypeId.
        /// </summary>
        /// <param name = "vendorTypeId">The vendor type ID to test.</param>
        [TestMethod]
        [DataRow(0)]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(-1)]
        public void Insert_VendorTypeWithBoundaryVendorTypeId_ReturnsOkResult(int vendorTypeId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var vendorType = new VendorType
            {
                VendorTypeId = vendorTypeId,
                VendorTypeName = "Test Vendor"
            };
            var payload = new CrudViewModel<VendorType>
            {
                value = vendorType
            };
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(vendorType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method handles VendorType with various string edge cases for VendorTypeName.
        /// </summary>
        /// <param name = "vendorTypeName">The vendor type name to test.</param>
        [TestMethod]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("A")]
        [DataRow("VeryLongVendorTypeNameThatExceedsNormalLengthExpectationsAndContainsManyCharactersToTestBoundaryConditionsForStringFields")]
        [DataRow("Special!@#$%^&*()Characters")]
        [DataRow("Unicode中文字符")]
        public void Insert_VendorTypeWithVariousVendorTypeNames_ReturnsOkResult(string vendorTypeName)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var vendorType = new VendorType
            {
                VendorTypeId = 1,
                VendorTypeName = vendorTypeName
            };
            var payload = new CrudViewModel<VendorType>
            {
                value = vendorType
            };
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(vendorType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method handles VendorType with various string edge cases for Description.
        /// </summary>
        /// <param name = "description">The description to test.</param>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow(" ")]
        [DataRow("Valid Description")]
        [DataRow("VeryLongDescriptionThatExceedsNormalLengthExpectationsAndContainsManyCharactersToTestBoundaryConditions")]
        public void Insert_VendorTypeWithVariousDescriptions_ReturnsOkResult(string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var vendorType = new VendorType
            {
                VendorTypeId = 1,
                VendorTypeName = "Test Vendor",
                Description = description
            };
            var payload = new CrudViewModel<VendorType>
            {
                value = vendorType
            };
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Add(vendorType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method adds a VendorType with null value property to the context.
        /// This tests the edge case where payload.value is null.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullToContextAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var payload = new CrudViewModel<VendorType>
            {
                value = null
            };
            var controller = new VendorTypeController(mockContext.Object);
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
        /// Tests that Insert method verifies SaveChanges is called exactly once.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsSaveChangesOnce()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var vendorType = new VendorType
            {
                VendorTypeId = 100,
                VendorTypeName = "SaveChanges Test"
            };
            var payload = new CrudViewModel<VendorType>
            {
                value = vendorType
            };
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            controller.Insert(payload);
            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method verifies Add is called with the correct VendorType.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsAddWithCorrectVendorType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<VendorType>>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var vendorType = new VendorType
            {
                VendorTypeId = 200,
                VendorTypeName = "Add Test"
            };
            var payload = new CrudViewModel<VendorType>
            {
                value = vendorType
            };
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(It.Is<VendorType>(v => v == vendorType)), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing VendorType entity and returns OkObjectResult.
        /// Input: Valid payload with key matching an existing VendorType.
        /// Expected: Entity is removed, SaveChanges is called, and OkObjectResult is returned with the removed entity.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKeyMatchingEntity_RemovesEntityAndReturnsOk()
        {
            // Arrange
            var vendorTypeId = 1;
            var vendorType = new VendorType
            {
                VendorTypeId = vendorTypeId,
                VendorTypeName = "Test Vendor",
                Description = "Test Description"
            };
            var data = new List<VendorType>
            {
                vendorType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<VendorType>>();
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockSet.Object);
            var controller = new VendorTypeController(mockContext.Object);
            var payload = new CrudViewModel<VendorType>
            {
                key = vendorTypeId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<VendorType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove correctly handles various numeric key values including boundary cases.
        /// Input: Payload with different numeric key values (positive, zero, negative, boundary values).
        /// Expected: Method processes the key correctly and attempts to find matching VendorType.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(-1)]
        [DataRow(100)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public void Remove_VariousNumericKeyValues_ProcessesCorrectly(int keyValue)
        {
            // Arrange
            var vendorType = new VendorType
            {
                VendorTypeId = keyValue,
                VendorTypeName = "Test Vendor",
                Description = "Test Description"
            };
            var data = new List<VendorType>
            {
                vendorType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<VendorType>>();
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockSet.Object);
            var controller = new VendorTypeController(mockContext.Object);
            var payload = new CrudViewModel<VendorType>
            {
                key = keyValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(It.IsAny<VendorType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove correctly converts string numeric keys to integers and processes them.
        /// Input: Payload with numeric string keys.
        /// Expected: Convert.ToInt32 successfully converts the string and finds matching VendorType.
        /// </summary>
        [TestMethod]
        [DataRow("1")]
        [DataRow("0")]
        [DataRow("-1")]
        [DataRow("999")]
        public void Remove_NumericStringKeys_ConvertsAndProcesses(string keyValue)
        {
            // Arrange
            var vendorTypeId = Convert.ToInt32(keyValue);
            var vendorType = new VendorType
            {
                VendorTypeId = vendorTypeId,
                VendorTypeName = "Test Vendor",
                Description = "Test Description"
            };
            var data = new List<VendorType>
            {
                vendorType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<VendorType>>();
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<VendorType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockSet.Object);
            var controller = new VendorTypeController(mockContext.Object);
            var payload = new CrudViewModel<VendorType>
            {
                key = keyValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(It.IsAny<VendorType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Update method with valid payload successfully updates the vendor type,
        /// saves changes to the database, and returns an OkObjectResult with the updated entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithVendorType()
        {
            // Arrange
            var vendorType = new VendorType
            {
                VendorTypeId = 1,
                VendorTypeName = "Test Vendor",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<VendorType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = vendorType
            };
            var mockDbSet = new Mock<DbSet<VendorType>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
            mockDbSet.Verify(d => d.Update(vendorType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method with payload containing null value property
        /// passes null to the Update method and SaveChanges.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_CallsUpdateWithNull()
        {
            // Arrange
            var payload = new CrudViewModel<VendorType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = null
            };
            var mockDbSet = new Mock<DbSet<VendorType>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new VendorTypeController(mockContext.Object);
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
        /// Tests that Update method with payload containing VendorType with minimum int value for ID
        /// successfully processes the update.
        /// </summary>
        [TestMethod]
        public void Update_VendorTypeWithMinIntId_ReturnsOkResult()
        {
            // Arrange
            var vendorType = new VendorType
            {
                VendorTypeId = int.MinValue,
                VendorTypeName = "Min ID Vendor",
                Description = "Min ID Description"
            };
            var payload = new CrudViewModel<VendorType>
            {
                action = "update",
                key = int.MinValue,
                antiForgery = "token",
                value = vendorType
            };
            var mockDbSet = new Mock<DbSet<VendorType>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
            mockDbSet.Verify(d => d.Update(vendorType), Times.Once);
        }

        /// <summary>
        /// Tests that Update method with payload containing VendorType with maximum int value for ID
        /// successfully processes the update.
        /// </summary>
        [TestMethod]
        public void Update_VendorTypeWithMaxIntId_ReturnsOkResult()
        {
            // Arrange
            var vendorType = new VendorType
            {
                VendorTypeId = int.MaxValue,
                VendorTypeName = "Max ID Vendor",
                Description = "Max ID Description"
            };
            var payload = new CrudViewModel<VendorType>
            {
                action = "update",
                key = int.MaxValue,
                antiForgery = "token",
                value = vendorType
            };
            var mockDbSet = new Mock<DbSet<VendorType>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
            mockDbSet.Verify(d => d.Update(vendorType), Times.Once);
        }

        /// <summary>
        /// Tests that Update method with VendorType containing empty string properties
        /// successfully processes the update.
        /// </summary>
        [TestMethod]
        public void Update_VendorTypeWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var vendorType = new VendorType
            {
                VendorTypeId = 1,
                VendorTypeName = string.Empty,
                Description = string.Empty
            };
            var payload = new CrudViewModel<VendorType>
            {
                action = "update",
                key = 1,
                antiForgery = string.Empty,
                value = vendorType
            };
            var mockDbSet = new Mock<DbSet<VendorType>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method with VendorType containing very long strings
        /// successfully processes the update.
        /// </summary>
        [TestMethod]
        public void Update_VendorTypeWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var longString = new string ('A', 10000);
            var vendorType = new VendorType
            {
                VendorTypeId = 1,
                VendorTypeName = longString,
                Description = longString
            };
            var payload = new CrudViewModel<VendorType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = vendorType
            };
            var mockDbSet = new Mock<DbSet<VendorType>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method with VendorType containing special characters in strings
        /// successfully processes the update.
        /// </summary>
        [TestMethod]
        public void Update_VendorTypeWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var vendorType = new VendorType
            {
                VendorTypeId = 1,
                VendorTypeName = "Test<>\"'&Vendor\t\n\r",
                Description = "Special chars: !@#$%^&*()[]{}|\\;:',.<>?/`~"
            };
            var payload = new CrudViewModel<VendorType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = vendorType
            };
            var mockDbSet = new Mock<DbSet<VendorType>>();
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.VendorType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new VendorTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(vendorType, okResult.Value);
        }
    }
}