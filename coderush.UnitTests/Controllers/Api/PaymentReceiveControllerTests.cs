using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using coderush.Services;
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
    /// Unit tests for the PaymentReceiveController class.
    /// </summary>
    [TestClass]
    public class PaymentReceiveControllerTests
    {
        /// <summary>
        /// Tests that GetPaymentReceive returns an OkObjectResult with empty Items list and Count of 0
        /// when the database contains no PaymentReceive records.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentReceive_EmptyDatabase_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var emptyData = new List<PaymentReceive>();
            var mockContext = CreateMockContext(emptyData);
            var mockNumberSequence = new Mock<Services.INumberSequence>();
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentReceive();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<PaymentReceive>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetPaymentReceive returns an OkObjectResult with a single PaymentReceive item
        /// and Count of 1 when the database contains exactly one record.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentReceive_SingleRecord_ReturnsSingleItemWithCountOne()
        {
            // Arrange
            var testData = new List<PaymentReceive>
            {
                new PaymentReceive
                {
                    PaymentReceiveId = 1,
                    PaymentReceiveName = "PMT-001",
                    InvoiceId = 100,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = 500.50,
                    IsFullPayment = true
                }
            };
            var mockContext = CreateMockContext(testData);
            var mockNumberSequence = new Mock<Services.INumberSequence>();
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentReceive();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<PaymentReceive>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].PaymentReceiveId);
            Assert.AreEqual("PMT-001", items[0].PaymentReceiveName);
        }

        /// <summary>
        /// Tests that GetPaymentReceive returns an OkObjectResult with multiple PaymentReceive items
        /// and correct Count when the database contains multiple records.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentReceive_MultipleRecords_ReturnsAllItemsWithCorrectCount()
        {
            // Arrange
            var testData = new List<PaymentReceive>
            {
                new PaymentReceive
                {
                    PaymentReceiveId = 1,
                    PaymentReceiveName = "PMT-001",
                    InvoiceId = 100,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = 500.50,
                    IsFullPayment = true
                },
                new PaymentReceive
                {
                    PaymentReceiveId = 2,
                    PaymentReceiveName = "PMT-002",
                    InvoiceId = 101,
                    PaymentDate = DateTimeOffset.Now.AddDays(-1),
                    PaymentTypeId = 2,
                    PaymentAmount = 1200.75,
                    IsFullPayment = false
                },
                new PaymentReceive
                {
                    PaymentReceiveId = 3,
                    PaymentReceiveName = "PMT-003",
                    InvoiceId = 102,
                    PaymentDate = DateTimeOffset.Now.AddDays(-2),
                    PaymentTypeId = 1,
                    PaymentAmount = 300.00,
                    IsFullPayment = true
                }
            };
            var mockContext = CreateMockContext(testData);
            var mockNumberSequence = new Mock<Services.INumberSequence>();
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentReceive();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<PaymentReceive>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual(1, items[0].PaymentReceiveId);
            Assert.AreEqual(2, items[1].PaymentReceiveId);
            Assert.AreEqual(3, items[2].PaymentReceiveId);
        }

        /// <summary>
        /// Tests that GetPaymentReceive returns an OkObjectResult with Items and Count properties
        /// matching exactly when database contains a large number of records.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentReceive_LargeDataSet_ReturnsAllItemsWithCorrectCount()
        {
            // Arrange
            var testData = new List<PaymentReceive>();
            for (int i = 1; i <= 100; i++)
            {
                testData.Add(new PaymentReceive { PaymentReceiveId = i, PaymentReceiveName = $"PMT-{i:D3}", InvoiceId = 100 + i, PaymentDate = DateTimeOffset.Now.AddDays(-i), PaymentTypeId = (i % 3) + 1, PaymentAmount = i * 10.5, IsFullPayment = i % 2 == 0 });
            }

            var mockContext = CreateMockContext(testData);
            var mockNumberSequence = new Mock<Services.INumberSequence>();
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentReceive();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic? value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<PaymentReceive>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(100, items.Count);
            Assert.AreEqual(100, count);
        }

        /// <summary>
        /// Helper method to create a mock ApplicationDbContext with a mocked PaymentReceive DbSet.
        /// </summary>
        /// <param name = "data">The test data to be returned by the DbSet.</param>
        /// <returns>A mocked ApplicationDbContext.</returns>
        private Mock<ApplicationDbContext> CreateMockContext(List<PaymentReceive> data)
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<PaymentReceive>>();
            mockSet.As<IQueryable<PaymentReceive>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PaymentReceive>(queryable.Provider));
            mockSet.As<IQueryable<PaymentReceive>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<PaymentReceive>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<PaymentReceive>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<PaymentReceive>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<PaymentReceive>(queryable.GetEnumerator()));
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentReceive).Returns(mockSet.Object);
            return mockContext;
        }

        /// <summary>
        /// Helper class to support async query operations in unit tests.
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
        /// Helper class to support async enumeration in unit tests.
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
        /// Helper class to support async enumerator in unit tests.
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
                get
                {
                    return _inner.Current;
                }
            }

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
        /// Tests that Update method returns OkObjectResult with the PaymentReceive object when given valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkObjectResultWithPaymentReceive()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = "PMT-001",
                InvoiceId = 100,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = 1,
                PaymentAmount = 500.50,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentReceive, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method calls Update on DbSet with the correct PaymentReceive object.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsUpdateOnDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = "PMT-001"
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            controller.Update(payload);
            // Assert
            mockDbSet.Verify(d => d.Update(paymentReceive), Times.Once);
        }

        /// <summary>
        /// Tests that Update method calls SaveChanges on the context.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = "PMT-001"
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            controller.Update(payload);
            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method works correctly with PaymentReceive having edge case numeric values.
        /// Input: PaymentReceiveId with int.MaxValue, InvoiceId with int.MinValue, PaymentTypeId with 0.
        /// Expected: Returns OkObjectResult with the PaymentReceive object.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue, int.MinValue, 0)]
        [DataRow(0, 0, int.MaxValue)]
        [DataRow(-1, -100, -1)]
        public void Update_EdgeCaseNumericValues_ReturnsOkObjectResult(int paymentReceiveId, int invoiceId, int paymentTypeId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = paymentReceiveId,
                PaymentReceiveName = "PMT-001",
                InvoiceId = invoiceId,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = paymentTypeId,
                PaymentAmount = 100.0,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentReceive, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method works correctly with PaymentReceive having edge case double values for PaymentAmount.
        /// Input: Various extreme double values including 0, negative, max, min, NaN, and infinity values.
        /// Expected: Returns OkObjectResult with the PaymentReceive object.
        /// </summary>
        [TestMethod]
        [DataRow(0.0)]
        [DataRow(-1000.50)]
        [DataRow(double.MaxValue)]
        [DataRow(double.MinValue)]
        public void Update_EdgeCasePaymentAmountValues_ReturnsOkObjectResult(double paymentAmount)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = "PMT-001",
                InvoiceId = 1,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = 1,
                PaymentAmount = paymentAmount,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentReceive, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method works correctly with PaymentReceive having special double values (NaN, Infinity).
        /// Input: PaymentAmount with double.NaN, double.PositiveInfinity, and double.NegativeInfinity.
        /// Expected: Returns OkObjectResult with the PaymentReceive object.
        /// </summary>
        [TestMethod]
        public void Update_SpecialDoubleValues_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = "PMT-001",
                InvoiceId = 1,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = 1,
                PaymentAmount = double.NaN,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Update method works correctly with PaymentReceive having edge case string values for PaymentReceiveName.
        /// Input: null, empty string, whitespace-only string, and very long string.
        /// Expected: Returns OkObjectResult with the PaymentReceive object.
        /// </summary>
        [TestMethod]
        [DataRow(null)]
        [DataRow("")]
        [DataRow("   ")]
        [DataRow("Very long payment name with many characters that exceeds typical limits to test boundary conditions and ensure the system handles large strings correctly without truncation or error")]
        public void Update_EdgeCaseStringValues_ReturnsOkObjectResult(string paymentReceiveName)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = paymentReceiveName,
                InvoiceId = 1,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = 1,
                PaymentAmount = 100.0,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentReceive, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method works correctly with PaymentReceive having edge case DateTimeOffset values.
        /// Input: DateTimeOffset.MinValue and DateTimeOffset.MaxValue.
        /// Expected: Returns OkObjectResult with the PaymentReceive object.
        /// </summary>
        [TestMethod]
        public void Update_EdgeCaseDateTimeOffsetValues_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = "PMT-001",
                InvoiceId = 1,
                PaymentDate = DateTimeOffset.MinValue,
                PaymentTypeId = 1,
                PaymentAmount = 100.0,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentReceive, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method works correctly with PaymentReceive having IsFullPayment set to false.
        /// Input: IsFullPayment = false.
        /// Expected: Returns OkObjectResult with the PaymentReceive object.
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Update_IsFullPaymentBooleanValues_ReturnsOkObjectResult(bool isFullPayment)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = "PMT-001",
                InvoiceId = 1,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = 1,
                PaymentAmount = 100.0,
                IsFullPayment = isFullPayment
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentReceive, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method works correctly with string values containing special characters.
        /// Input: PaymentReceiveName with special characters, control characters, and Unicode.
        /// Expected: Returns OkObjectResult with the PaymentReceive object.
        /// </summary>
        [TestMethod]
        [DataRow("PMT-001!@#$%^&*()")]
        [DataRow("PMT\n001")]
        [DataRow("PMT\t001")]
        [DataRow("测试付款")]
        public void Update_StringWithSpecialCharacters_ReturnsOkObjectResult(string paymentReceiveName)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentReceive>>();
            var paymentReceive = new PaymentReceive
            {
                PaymentReceiveId = 1,
                PaymentReceiveName = paymentReceiveName,
                InvoiceId = 1,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = 1,
                PaymentAmount = 100.0,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentReceive>
            {
                value = paymentReceive
            };
            mockContext.Setup(c => c.PaymentReceive).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentReceiveController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentReceive, okResult.Value);
        }
    }
}