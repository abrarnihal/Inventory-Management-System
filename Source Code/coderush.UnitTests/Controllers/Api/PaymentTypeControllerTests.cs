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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the PaymentTypeController class.
    /// </summary>
    [TestClass]
    public class PaymentTypeControllerTests
    {
        /// <summary>
        /// Tests that GetPaymentType returns an OkObjectResult with empty Items list and Count of 0 when the database is empty.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentType_EmptyDatabase_ReturnsEmptyListWithCountZero()
        {
            // Arrange
            var emptyData = new List<PaymentType>();
            var mockSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.PaymentType).Returns(mockSet.Object);

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = await controller.GetPaymentType();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            var items = value.Items as List<PaymentType>;
            int count = value.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetPaymentType returns an OkObjectResult with a single item and Count of 1 when the database contains one PaymentType.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentType_SingleItem_ReturnsListWithOneItemAndCountOne()
        {
            // Arrange
            var singleItemData = new List<PaymentType>
            {
                new PaymentType { PaymentTypeId = 1, PaymentTypeName = "Cash", Description = "Cash payment" }
            };
            var mockSet = CreateMockDbSet(singleItemData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.PaymentType).Returns(mockSet.Object);

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = await controller.GetPaymentType();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            var items = value.Items as List<PaymentType>;
            int count = value.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].PaymentTypeId);
            Assert.AreEqual("Cash", items[0].PaymentTypeName);
            Assert.AreEqual("Cash payment", items[0].Description);
        }

        /// <summary>
        /// Tests that GetPaymentType returns an OkObjectResult with multiple items and correct Count when the database contains multiple PaymentTypes.
        /// </summary>
        /// <param name="itemCount">The number of items to test with.</param>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public async Task GetPaymentType_MultipleItems_ReturnsAllItemsWithCorrectCount(int itemCount)
        {
            // Arrange
            var multipleItemsData = new List<PaymentType>();
            for (int i = 1; i <= itemCount; i++)
            {
                multipleItemsData.Add(new PaymentType
                {
                    PaymentTypeId = i,
                    PaymentTypeName = $"PaymentType{i}",
                    Description = $"Description{i}"
                });
            }

            var mockSet = CreateMockDbSet(multipleItemsData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.PaymentType).Returns(mockSet.Object);

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = await controller.GetPaymentType();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            var items = value.Items as List<PaymentType>;
            int count = value.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(itemCount, items.Count);
            Assert.AreEqual(itemCount, count);

            for (int i = 0; i < itemCount; i++)
            {
                Assert.AreEqual(i + 1, items[i].PaymentTypeId);
                Assert.AreEqual($"PaymentType{i + 1}", items[i].PaymentTypeName);
                Assert.AreEqual($"Description{i + 1}", items[i].Description);
            }
        }

        /// <summary>
        /// Tests that GetPaymentType correctly handles special characters and edge case values in PaymentType properties.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentType_ItemsWithSpecialCharacters_ReturnsAllItemsCorrectly()
        {
            // Arrange
            var specialData = new List<PaymentType>
            {
                new PaymentType { PaymentTypeId = 1, PaymentTypeName = "Credit/Debit Card", Description = "Card payment with special chars: @#$%^&*()" },
                new PaymentType { PaymentTypeId = 2, PaymentTypeName = "PayPal™", Description = "Online payment © 2024" },
                new PaymentType { PaymentTypeId = 3, PaymentTypeName = "", Description = "" },
                new PaymentType { PaymentTypeId = int.MaxValue, PaymentTypeName = "Max ID", Description = "Maximum integer ID" }
            };

            var mockSet = CreateMockDbSet(specialData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.PaymentType).Returns(mockSet.Object);

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = await controller.GetPaymentType();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            var items = value.Items as List<PaymentType>;
            int count = value.Count;

            Assert.IsNotNull(items);
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual(4, count);
            Assert.AreEqual("Credit/Debit Card", items[0].PaymentTypeName);
            Assert.AreEqual("PayPal™", items[1].PaymentTypeName);
            Assert.AreEqual("", items[2].PaymentTypeName);
            Assert.AreEqual(int.MaxValue, items[3].PaymentTypeId);
        }

        /// <summary>
        /// Tests that GetPaymentType returns correct result structure with Items and Count properties.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentType_AnyData_ReturnsOkResultWithItemsAndCountProperties()
        {
            // Arrange
            var testData = new List<PaymentType>
            {
                new PaymentType { PaymentTypeId = 1, PaymentTypeName = "Test", Description = "Test Description" }
            };

            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.PaymentType).Returns(mockSet.Object);

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = await controller.GetPaymentType();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);

            // Verify the anonymous type has the expected properties
            var valueType = value.GetType();
            var itemsProperty = valueType.GetProperty("Items");
            var countProperty = valueType.GetProperty("Count");

            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
        }

        /// <summary>
        /// Creates a mock DbSet for testing Entity Framework operations.
        /// </summary>
        /// <param name="sourceList">The source data to populate the mock DbSet.</param>
        /// <returns>A mock DbSet configured for async operations.</returns>
        private static Mock<DbSet<PaymentType>> CreateMockDbSet(List<PaymentType> sourceList)
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<PaymentType>>();

            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PaymentType>(queryable.Provider));
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockSet.As<IAsyncEnumerable<PaymentType>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<PaymentType>(queryable.GetEnumerator()));

            return mockSet;
        }

        /// <summary>
        /// Helper class to enable async query provider functionality for in-memory testing.
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
        /// Helper class to provide async enumerable functionality for in-memory testing.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(System.Linq.Expressions.Expression expression) : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }
        }

        /// <summary>
        /// Helper class to provide async enumerator functionality for in-memory testing.
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
                return default;
            }
        }

        /// <summary>
        /// Tests that Insert method successfully adds a valid PaymentType to the database
        /// and returns an OkObjectResult with the added entity.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithPaymentType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<PaymentType>>();

            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PaymentTypeController(mockContext.Object);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 1,
                PaymentTypeName = "Credit Card",
                Description = "Payment via credit card"
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType,
                action = "insert",
                key = 1,
                antiForgery = "token"
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
            mockDbSet.Verify(d => d.Add(paymentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method handles payload with null value property.
        /// The method should attempt to add null to the DbSet.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullToDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<PaymentType>>();

            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new PaymentTypeController(mockContext.Object);

            var payload = new CrudViewModel<PaymentType>
            {
                value = null,
                action = "insert",
                key = 1,
                antiForgery = "token"
            };

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
        /// Tests Insert method with various PaymentType property values including edge cases.
        /// </summary>
        /// <param name="paymentTypeId">The PaymentType ID.</param>
        /// <param name="paymentTypeName">The PaymentType name.</param>
        /// <param name="description">The description.</param>
        [TestMethod]
        [DataRow(0, "", "", DisplayName = "Empty strings")]
        [DataRow(int.MaxValue, "Very long payment type name that exceeds normal length expectations for testing edge cases in string handling", "Very long description", DisplayName = "Max int and long strings")]
        [DataRow(int.MinValue, "Test", "Test", DisplayName = "Min int value")]
        [DataRow(-1, "Cash", "Payment in cash", DisplayName = "Negative ID")]
        [DataRow(1, "Payment\nWith\nNewlines", "Description\twith\ttabs", DisplayName = "Special characters")]
        [DataRow(1, null, null, DisplayName = "Null strings")]
        [DataRow(1, "   ", "   ", DisplayName = "Whitespace strings")]
        public void Insert_VariousPaymentTypeValues_AddsAndReturnsOkResult(int paymentTypeId, string paymentTypeName, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<PaymentType>>();

            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PaymentTypeController(mockContext.Object);

            var paymentType = new PaymentType
            {
                PaymentTypeId = paymentTypeId,
                PaymentTypeName = paymentTypeName,
                Description = description
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
            mockDbSet.Verify(d => d.Add(paymentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method returns OkObjectResult with status code 200.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithStatusCode200()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<PaymentType>>();

            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new PaymentTypeController(mockContext.Object);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 1,
                PaymentTypeName = "Debit Card"
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            // Act
            var result = controller.Insert(payload) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        /// <summary>
        /// Tests that Insert method handles SaveChanges returning zero (no rows affected).
        /// </summary>
        [TestMethod]
        public void Insert_SaveChangesReturnsZero_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<PaymentType>>();

            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new PaymentTypeController(mockContext.Object);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 1,
                PaymentTypeName = "Check"
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that the Update method returns an OkObjectResult with the updated PaymentType
        /// when provided with a valid payload containing a PaymentType entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkObjectResultWithPaymentType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 1,
                PaymentTypeName = "Credit Card",
                Description = "Payment by credit card"
            };

            var payload = new CrudViewModel<PaymentType>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = paymentType
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            IActionResult result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
        }

        /// <summary>
        /// Tests that the Update method calls DbSet.Update with the correct PaymentType entity
        /// when provided with a valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsDbSetUpdateWithPaymentType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 2,
                PaymentTypeName = "Cash",
                Description = "Cash payment"
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            controller.Update(payload);

            // Assert
            mockDbSet.Verify(db => db.Update(paymentType), Times.Once);
        }

        /// <summary>
        /// Tests that the Update method calls SaveChanges on the database context
        /// when provided with a valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 3,
                PaymentTypeName = "Debit Card",
                Description = "Debit card payment"
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            controller.Update(payload);

            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that the Update method calls DbSet.Update with null and returns OkObjectResult with null
        /// when the payload.value is null.
        /// </summary>
        [TestMethod]
        public void Update_NullPaymentTypeValue_CallsUpdateWithNullAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var payload = new CrudViewModel<PaymentType>
            {
                action = "update",
                key = 0,
                antiForgery = "token",
                value = null!
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            IActionResult result = controller.Update(payload);

            // Assert
            mockDbSet.Verify(db => db.Update(null!), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that the Update method correctly handles a PaymentType with minimal data
        /// (only required fields populated).
        /// </summary>
        [TestMethod]
        public void Update_PaymentTypeWithMinimalData_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 0,
                PaymentTypeName = "Test",
                Description = null!
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            IActionResult result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
        }

        /// <summary>
        /// Tests that the Update method correctly handles a PaymentType with boundary values
        /// such as empty strings and extreme integer values.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue, "", "")]
        [DataRow(int.MaxValue, "X", "Y")]
        [DataRow(0, "   ", "   ")]
        public void Update_PaymentTypeWithBoundaryValues_ReturnsOkResult(int id, string name, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var paymentType = new PaymentType
            {
                PaymentTypeId = id,
                PaymentTypeName = name,
                Description = description
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            IActionResult result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
            mockDbSet.Verify(db => db.Update(paymentType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that the Update method correctly handles a PaymentType with very long string values
        /// to ensure no truncation or errors occur.
        /// </summary>
        [TestMethod]
        public void Update_PaymentTypeWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var longString = new string('A', 10000);
            var paymentType = new PaymentType
            {
                PaymentTypeId = 100,
                PaymentTypeName = longString,
                Description = longString
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            IActionResult result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(db => db.Update(paymentType), Times.Once);
        }

        /// <summary>
        /// Tests that the Update method correctly handles a PaymentType with special characters
        /// in string fields.
        /// </summary>
        [TestMethod]
        public void Update_PaymentTypeWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<Microsoft.EntityFrameworkCore.DbSet<PaymentType>>();
            mockContext.Setup(c => c.PaymentType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var paymentType = new PaymentType
            {
                PaymentTypeId = 50,
                PaymentTypeName = "Test<>&\"'\0\n\r\t",
                Description = "Special chars: !@#$%^&*()_+-=[]{}|;:',.<>?/~`"
            };

            var payload = new CrudViewModel<PaymentType>
            {
                value = paymentType
            };

            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            IActionResult result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove method successfully removes an existing payment type and returns OkObjectResult with the removed entity.
        /// Input: Valid payload with existing key.
        /// Expected: Entity is removed, SaveChanges is called, and OkObjectResult is returned with the removed entity.
        /// </summary>
        [TestMethod]
        [DataRow(1)]
        [DataRow(100)]
        [DataRow(int.MaxValue)]
        public void Remove_ValidKeyExists_ReturnsOkWithRemovedEntity(int keyValue)
        {
            // Arrange
            var paymentType = new PaymentType { PaymentTypeId = keyValue, PaymentTypeName = "Test Payment", Description = "Test Description" };
            var data = new List<PaymentType> { paymentType }.AsQueryable();

            var mockSet = new Mock<DbSet<PaymentType>>();
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentType).Returns(mockSet.Object);

            var payload = new CrudViewModel<PaymentType> { key = keyValue };
            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<PaymentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method correctly handles string key that represents a valid integer.
        /// Input: Payload with string key representing valid integer.
        /// Expected: Entity is removed successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        [DataRow("42")]
        [DataRow("1")]
        [DataRow("-5")]
        public void Remove_StringKeyValidInteger_ReturnsOkWithRemovedEntity(string keyValue)
        {
            // Arrange
            var intKey = Convert.ToInt32(keyValue);
            var paymentType = new PaymentType { PaymentTypeId = intKey, PaymentTypeName = "Test Payment", Description = "Test Description" };
            var data = new List<PaymentType> { paymentType }.AsQueryable();

            var mockSet = new Mock<DbSet<PaymentType>>();
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentType).Returns(mockSet.Object);

            var payload = new CrudViewModel<PaymentType> { key = keyValue };
            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<PaymentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method correctly processes negative key values.
        /// Input: Payload with negative integer key that exists.
        /// Expected: Entity is removed successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        [DataRow(-1)]
        [DataRow(-100)]
        [DataRow(int.MinValue)]
        public void Remove_NegativeKeyExists_ReturnsOkWithRemovedEntity(int keyValue)
        {
            // Arrange
            var paymentType = new PaymentType { PaymentTypeId = keyValue, PaymentTypeName = "Test Payment", Description = "Test Description" };
            var data = new List<PaymentType> { paymentType }.AsQueryable();

            var mockSet = new Mock<DbSet<PaymentType>>();
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentType).Returns(mockSet.Object);

            var payload = new CrudViewModel<PaymentType> { key = keyValue };
            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<PaymentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove method correctly handles zero as key value.
        /// Input: Payload with key value of zero that exists.
        /// Expected: Entity is removed successfully and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Remove_ZeroKeyExists_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            var paymentType = new PaymentType { PaymentTypeId = 0, PaymentTypeName = "Test Payment", Description = "Test Description" };
            var data = new List<PaymentType> { paymentType }.AsQueryable();

            var mockSet = new Mock<DbSet<PaymentType>>();
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentType).Returns(mockSet.Object);

            var payload = new CrudViewModel<PaymentType> { key = 0 };
            var controller = new PaymentTypeController(mockContext.Object);

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentType, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<PaymentType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }
    }
}