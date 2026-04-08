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
    /// Unit tests for the InvoiceTypeController class.
    /// </summary>
    [TestClass]
    public class InvoiceTypeControllerTests
    {
        /// <summary>
        /// Tests that GetInvoiceType returns an empty list with count of 0 when database has no invoice types.
        /// </summary>
        [TestMethod]
        public async Task GetInvoiceType_EmptyDatabase_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var testData = new List<InvoiceType>();
            var mockContext = CreateMockContext(testData);
            var controller = new InvoiceTypeController(mockContext.Object);
            // Act
            var result = await controller.GetInvoiceType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var countProperty = value?.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(value) as List<InvoiceType>;
            var count = (int? )countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetInvoiceType returns a list with single item and count of 1 when database has one invoice type.
        /// </summary>
        [TestMethod]
        public async Task GetInvoiceType_SingleInvoiceType_ReturnsSingleItemWithCountOne()
        {
            // Arrange
            var testData = new List<InvoiceType>
            {
                new InvoiceType
                {
                    InvoiceTypeId = 1,
                    InvoiceTypeName = "Standard",
                    Description = "Standard Invoice"
                }
            };
            var mockContext = CreateMockContext(testData);
            var controller = new InvoiceTypeController(mockContext.Object);
            // Act
            var result = await controller.GetInvoiceType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var countProperty = value?.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(value) as List<InvoiceType>;
            var count = (int? )countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].InvoiceTypeId);
            Assert.AreEqual("Standard", items[0].InvoiceTypeName);
        }

        /// <summary>
        /// Tests that GetInvoiceType returns multiple items with correct count when database has multiple invoice types.
        /// </summary>
        /// <param name = "numberOfItems">Number of invoice types in the test database.</param>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public async Task GetInvoiceType_MultipleInvoiceTypes_ReturnsAllItemsWithCorrectCount(int numberOfItems)
        {
            // Arrange
            var testData = new List<InvoiceType>();
            for (int i = 1; i <= numberOfItems; i++)
            {
                testData.Add(new InvoiceType { InvoiceTypeId = i, InvoiceTypeName = $"Type{i}", Description = $"Description{i}" });
            }

            var mockContext = CreateMockContext(testData);
            var controller = new InvoiceTypeController(mockContext.Object);
            // Act
            var result = await controller.GetInvoiceType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var countProperty = value?.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(value) as List<InvoiceType>;
            var count = (int? )countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(numberOfItems, items.Count);
            Assert.AreEqual(numberOfItems, count);
            for (int i = 0; i < numberOfItems; i++)
            {
                Assert.AreEqual(i + 1, items[i].InvoiceTypeId);
                Assert.AreEqual($"Type{i + 1}", items[i].InvoiceTypeName);
            }
        }

        /// <summary>
        /// Tests that GetInvoiceType handles invoice types with special characters and edge case string values correctly.
        /// </summary>
        [TestMethod]
        public async Task GetInvoiceType_InvoiceTypesWithSpecialCharacters_ReturnsItemsCorrectly()
        {
            // Arrange
            var testData = new List<InvoiceType>
            {
                new InvoiceType
                {
                    InvoiceTypeId = 1,
                    InvoiceTypeName = "Type with spaces",
                    Description = "Normal description"
                },
                new InvoiceType
                {
                    InvoiceTypeId = 2,
                    InvoiceTypeName = "Type-With-Dashes",
                    Description = string.Empty
                },
                new InvoiceType
                {
                    InvoiceTypeId = 3,
                    InvoiceTypeName = "Type_With_Underscores",
                    Description = "   "
                },
                new InvoiceType
                {
                    InvoiceTypeId = 4,
                    InvoiceTypeName = "Type!@#$%",
                    Description = "Special chars: <>&\"'"
                }
            };
            var mockContext = CreateMockContext(testData);
            var controller = new InvoiceTypeController(mockContext.Object);
            // Act
            var result = await controller.GetInvoiceType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var items = itemsProperty?.GetValue(value) as List<InvoiceType>;
            Assert.IsNotNull(items);
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual("Type with spaces", items[0].InvoiceTypeName);
            Assert.AreEqual("Type-With-Dashes", items[1].InvoiceTypeName);
            Assert.AreEqual(string.Empty, items[1].Description);
            Assert.AreEqual("Type_With_Underscores", items[2].InvoiceTypeName);
            Assert.AreEqual("Type!@#$%", items[3].InvoiceTypeName);
        }

        /// <summary>
        /// Tests that GetInvoiceType handles very long string values in invoice type properties correctly.
        /// </summary>
        [TestMethod]
        public async Task GetInvoiceType_InvoiceTypeWithVeryLongStrings_ReturnsItemsCorrectly()
        {
            // Arrange
            var longString = new string ('A', 10000);
            var testData = new List<InvoiceType>
            {
                new InvoiceType
                {
                    InvoiceTypeId = 1,
                    InvoiceTypeName = longString,
                    Description = longString
                }
            };
            var mockContext = CreateMockContext(testData);
            var controller = new InvoiceTypeController(mockContext.Object);
            // Act
            var result = await controller.GetInvoiceType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var items = itemsProperty?.GetValue(value) as List<InvoiceType>;
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(longString, items[0].InvoiceTypeName);
            Assert.AreEqual(longString, items[0].Description);
        }

        /// <summary>
        /// Tests that GetInvoiceType handles invoice types with boundary integer values correctly.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        [DataRow(-1)]
        public async Task GetInvoiceType_InvoiceTypeWithBoundaryIntegerIds_ReturnsItemsCorrectly(int invoiceTypeId)
        {
            // Arrange
            var testData = new List<InvoiceType>
            {
                new InvoiceType
                {
                    InvoiceTypeId = invoiceTypeId,
                    InvoiceTypeName = "Test",
                    Description = "Test Description"
                }
            };
            var mockContext = CreateMockContext(testData);
            var controller = new InvoiceTypeController(mockContext.Object);
            // Act
            var result = await controller.GetInvoiceType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = okResult.Value;
            var itemsProperty = value?.GetType().GetProperty("Items");
            var items = itemsProperty?.GetValue(value) as List<InvoiceType>;
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(invoiceTypeId, items[0].InvoiceTypeId);
        }

        /// <summary>
        /// Helper method to create a mock ApplicationDbContext with a mocked InvoiceType DbSet.
        /// </summary>
        /// <param name = "testData">The test data to return from the mocked DbSet.</param>
        /// <returns>A mock ApplicationDbContext configured with test data.</returns>
        private Mock<ApplicationDbContext> CreateMockContext(List<InvoiceType> testData)
        {
            var queryable = testData.AsQueryable();
            var mockSet = new Mock<DbSet<InvoiceType>>();
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<InvoiceType>(queryable.Provider));
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<InvoiceType>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<InvoiceType>(testData.GetEnumerator()));
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.InvoiceType).Returns(mockSet.Object);
            return mockContext;
        }

        /// <summary>
        /// Helper class to provide async query provider for testing EF Core async operations.
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
                var result = executeMethod?.Invoke(_inner, new object[] { expression });
                return (TResult)Activator.CreateInstance(typeof(TResult), result);
            }
        }

        /// <summary>
        /// Helper class to provide async enumerable for testing EF Core async operations.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
            {
            }

            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        /// <summary>
        /// Helper class to provide async enumerator for testing EF Core async operations.
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
        /// Tests that Insert method successfully adds a valid InvoiceType to the database context,
        /// saves changes, and returns an OkObjectResult with the entity.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkWithInvoiceType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Add(It.IsAny<InvoiceType>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<InvoiceType>? )null);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = "Test Invoice",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
            mockDbSet.Verify(d => d.Add(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method handles InvoiceType with boundary values correctly.
        /// Verifies that entities with extreme integer values and long strings are processed.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue, "A")]
        [DataRow(int.MaxValue, "Very long invoice type name with many characters that might exceed typical database field lengths to test boundary conditions")]
        [DataRow(0, "")]
        public void Insert_BoundaryValues_ReturnsOkWithInvoiceType(int invoiceTypeId, string invoiceTypeName)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Add(It.IsAny<InvoiceType>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<InvoiceType>? )null);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = invoiceTypeId,
                InvoiceTypeName = invoiceTypeName,
                Description = "Test"
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
            mockDbSet.Verify(d => d.Add(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method attempts to add null value to DbSet when payload.value is null.
        /// Verifies that Add and SaveChanges are called even with null entity.
        /// </summary>
        [TestMethod]
        public void Insert_NullInvoiceTypeValue_AddsNullToDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            mockDbSet.Setup(d => d.Add(It.IsAny<InvoiceType>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<InvoiceType>? )null);
            var controller = new InvoiceTypeController(mockContext.Object);
            var payload = new CrudViewModel<InvoiceType>
            {
                value = null!
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            mockDbSet.Verify(d => d.Add(null!), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method correctly handles InvoiceType with null or whitespace Description.
        /// Verifies that optional fields can be null or empty without causing errors.
        /// </summary>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        public void Insert_NullOrEmptyDescription_ReturnsOk(string? description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Add(It.IsAny<InvoiceType>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<InvoiceType>? )null);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = "Test",
                Description = description!
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
            mockDbSet.Verify(d => d.Add(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method correctly handles InvoiceType with special characters in strings.
        /// Verifies that strings with control characters, unicode, and special symbols are processed.
        /// </summary>
        [TestMethod]
        public void Insert_SpecialCharactersInStrings_ReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Add(It.IsAny<InvoiceType>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<InvoiceType>? )null);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = "Test<>&\"'\n\t\r\\",
                Description = "Unicode: ñ, é, 中文, 🚀"
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
            mockDbSet.Verify(d => d.Add(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update returns OkObjectResult with the updated invoice type when given a valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkObjectResultWithInvoiceType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = "Standard Invoice",
                Description = "Standard invoice type"
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                action = "update",
                value = invoiceType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
            mockDbSet.Verify(d => d.Update(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update processes payload with null value and attempts to update.
        /// Input: Payload with null value property.
        /// Expected: Update is called with null and SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_CallsUpdateWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new InvoiceTypeController(mockContext.Object);
            var payload = new CrudViewModel<InvoiceType>
            {
                action = "update",
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
        /// Tests Update with various edge case InvoiceType values.
        /// Input: Different InvoiceTypeId values including boundaries and extreme values.
        /// Expected: Update is called and OkObjectResult is returned for all cases.
        /// </summary>
        [TestMethod]
        [DataRow(0, "Zero ID")]
        [DataRow(-1, "Negative ID")]
        [DataRow(int.MaxValue, "MaxValue ID")]
        [DataRow(int.MinValue, "MinValue ID")]
        [DataRow(999999, "Large ID")]
        public void Update_EdgeCaseInvoiceTypeIds_ReturnsOkObjectResult(int invoiceTypeId, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = invoiceTypeId,
                InvoiceTypeName = "Test",
                Description = description
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests Update with various edge case string values for InvoiceTypeName.
        /// Input: Different string values including null, empty, whitespace, and special characters.
        /// Expected: Update is called and OkObjectResult is returned for all cases.
        /// </summary>
        [TestMethod]
        [DataRow(null, "Null name")]
        [DataRow("", "Empty name")]
        [DataRow("   ", "Whitespace name")]
        [DataRow("A", "Single character")]
        [DataRow("Very long invoice type name that exceeds normal expectations and might cause issues in some systems due to its excessive length", "Very long name")]
        [DataRow("Special!@#$%^&*()Characters", "Special characters")]
        [DataRow("\n\r\t", "Control characters")]
        public void Update_EdgeCaseInvoiceTypeNames_ReturnsOkObjectResult(string invoiceTypeName, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = invoiceTypeName,
                Description = description
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests Update with various edge case string values for Description.
        /// Input: Different description values including null, empty, whitespace, and special characters.
        /// Expected: Update is called and OkObjectResult is returned for all cases.
        /// </summary>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("A very long description that contains a lot of text and might be used to test boundary conditions in the database or application layer to ensure proper handling of large text fields")]
        [DataRow("Description with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        public void Update_EdgeCaseDescriptions_ReturnsOkObjectResult(string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = "Test",
                Description = description
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(invoiceType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update returns correct status code (200 OK).
        /// Input: Valid payload with invoice type.
        /// Expected: OkObjectResult with status code 200.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsStatusCode200()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<InvoiceType>>();
            mockContext.Setup(c => c.InvoiceType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = "Test",
                Description = "Test"
            };
            var payload = new CrudViewModel<InvoiceType>
            {
                value = invoiceType
            };
            // Act
            var result = controller.Update(payload) as OkObjectResult;
            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        /// <summary>
        /// Tests that Remove successfully removes and returns entity when valid key matches existing entity.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(999)]
        public void Remove_ValidKeyWithExistingEntity_RemovesAndReturnsEntity(int invoiceTypeId)
        {
            // Arrange
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = invoiceTypeId,
                InvoiceTypeName = "Test Invoice",
                Description = "Test Description"
            };
            var data = new List<InvoiceType>
            {
                invoiceType
            }.AsQueryable();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.InvoiceType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var payload = new CrudViewModel<InvoiceType>
            {
                key = invoiceTypeId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
            mockSet.Verify(m => m.Remove(invoiceType), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully converts string key to integer and removes entity.
        /// </summary>
        [TestMethod]
        [DataRow("1")]
        [DataRow("50")]
        [DataRow("100")]
        public void Remove_StringKeyWithExistingEntity_ConvertsAndRemovesEntity(string keyValue)
        {
            // Arrange
            int id = Convert.ToInt32(keyValue);
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = id,
                InvoiceTypeName = "Test Invoice",
                Description = "Test Description"
            };
            var data = new List<InvoiceType>
            {
                invoiceType
            }.AsQueryable();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.InvoiceType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var payload = new CrudViewModel<InvoiceType>
            {
                key = keyValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
            mockSet.Verify(m => m.Remove(invoiceType), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles boundary integer values correctly.
        /// Tests with zero, negative values, int.MaxValue, and int.MinValue.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public void Remove_BoundaryIntegerKeys_HandlesCorrectly(int keyValue)
        {
            // Arrange
            var invoiceType = new InvoiceType
            {
                InvoiceTypeId = keyValue,
                InvoiceTypeName = "Test Invoice",
                Description = "Test Description"
            };
            var data = new List<InvoiceType>
            {
                invoiceType
            }.AsQueryable();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.InvoiceType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var payload = new CrudViewModel<InvoiceType>
            {
                key = keyValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(invoiceType, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove correctly handles scenario with multiple entities in database.
        /// Only the matching entity should be removed.
        /// </summary>
        [TestMethod]
        public void Remove_MultipleEntitiesInDatabase_RemovesOnlyMatchingEntity()
        {
            // Arrange
            var targetInvoiceType = new InvoiceType
            {
                InvoiceTypeId = 2,
                InvoiceTypeName = "Target Invoice",
                Description = "Target"
            };
            var otherInvoiceType1 = new InvoiceType
            {
                InvoiceTypeId = 1,
                InvoiceTypeName = "Other Invoice 1",
                Description = "Other 1"
            };
            var otherInvoiceType2 = new InvoiceType
            {
                InvoiceTypeId = 3,
                InvoiceTypeName = "Other Invoice 2",
                Description = "Other 2"
            };
            var data = new List<InvoiceType>
            {
                otherInvoiceType1,
                targetInvoiceType,
                otherInvoiceType2
            }.AsQueryable();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.InvoiceType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new InvoiceTypeController(mockContext.Object);
            var payload = new CrudViewModel<InvoiceType>
            {
                key = 2
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(targetInvoiceType, okResult.Value);
            mockSet.Verify(m => m.Remove(targetInvoiceType), Times.Once);
            mockSet.Verify(m => m.Remove(otherInvoiceType1), Times.Never);
            mockSet.Verify(m => m.Remove(otherInvoiceType2), Times.Never);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports LINQ operations.
        /// </summary>
        /// <param name = "data">The queryable data source.</param>
        /// <returns>A mock DbSet configured for LINQ queries.</returns>
        private static Mock<DbSet<InvoiceType>> CreateMockDbSet(IQueryable<InvoiceType> data)
        {
            var mockSet = new Mock<DbSet<InvoiceType>>();
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<InvoiceType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            mockSet.Setup(m => m.Remove(It.IsAny<InvoiceType>())).Callback<InvoiceType>(entity =>
            {
                if (entity == null)
                    throw new ArgumentNullException(nameof(entity));
            });
            return mockSet;
        }
    }
}