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
    /// Unit tests for PaymentVoucherController class.
    /// </summary>
    [TestClass]
    public class PaymentVoucherControllerTests
    {
        /// <summary>
        /// Tests that Insert method successfully creates a payment voucher with valid payload.
        /// Input: Valid CrudViewModel with PaymentVoucher.
        /// Expected: Returns OkObjectResult with the payment voucher and proper voucher name assigned.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithPaymentVoucher()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var expectedVoucherName = "PAYVCH-001";
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns(expectedVoucherName);
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = 100,
                PaymentDate = DateTimeOffset.Now,
                PaymentTypeId = 1,
                PaymentAmount = 500.00,
                CashBankId = 1,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                action = "insert",
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            Assert.IsInstanceOfType(okResult.Value, typeof(PaymentVoucher));
            var returnedVoucher = (PaymentVoucher)okResult.Value;
            Assert.AreEqual(expectedVoucherName, returnedVoucher.PaymentVoucherName);
            mockNumberSequence.Verify(x => x.GetNumberSequence("PAYVCH"), Times.Once);
            mockDbSet.Verify(x => x.Add(paymentVoucher), Times.Once);
            mockContext.Verify(x => x.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method assigns voucher name from number sequence service.
        /// Input: Valid payload with various voucher name patterns from number sequence.
        /// Expected: PaymentVoucherName is set to the value returned by GetNumberSequence.
        /// </summary>
        [TestMethod]
        [DataRow("PAYVCH-001")]
        [DataRow("PAYVCH-999")]
        [DataRow("")]
        [DataRow("SPECIAL-VOUCHER-123")]
        public void Insert_VariousVoucherNames_AssignsCorrectVoucherName(string voucherName)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns(voucherName);
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = 100,
                PaymentAmount = 100.0
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            var okResult = (OkObjectResult)result;
            var returnedVoucher = (PaymentVoucher)okResult.Value;
            Assert.AreEqual(voucherName, returnedVoucher.PaymentVoucherName);
        }

        /// <summary>
        /// Tests that Insert method handles edge case amounts correctly.
        /// Input: PaymentVoucher with various edge case payment amounts.
        /// Expected: Successfully processes and returns voucher with the specified amount.
        /// </summary>
        [TestMethod]
        [DataRow(0.0)]
        [DataRow(double.MaxValue)]
        [DataRow(0.01)]
        [DataRow(999999999.99)]
        public void Insert_EdgeCaseAmounts_ProcessesSuccessfully(double amount)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns("PAYVCH-001");
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = 100,
                PaymentAmount = amount
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            var returnedVoucher = (PaymentVoucher)okResult.Value;
            Assert.AreEqual(amount, returnedVoucher.PaymentAmount);
        }

        /// <summary>
        /// Tests that Insert method handles special numeric values for PaymentAmount.
        /// Input: PaymentVoucher with NaN, PositiveInfinity, and NegativeInfinity amounts.
        /// Expected: Successfully processes and returns voucher with the specified special value.
        /// </summary>
        [TestMethod]
        [DataRow(double.NaN)]
        [DataRow(double.PositiveInfinity)]
        [DataRow(double.NegativeInfinity)]
        public void Insert_SpecialDoubleValues_ProcessesSuccessfully(double amount)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns("PAYVCH-001");
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = 100,
                PaymentAmount = amount
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = (OkObjectResult)result;
            var returnedVoucher = (PaymentVoucher)okResult.Value;
            if (double.IsNaN(amount))
            {
                Assert.IsTrue(double.IsNaN(returnedVoucher.PaymentAmount));
            }
            else
            {
                Assert.AreEqual(amount, returnedVoucher.PaymentAmount);
            }
        }

        /// <summary>
        /// Tests that Insert method handles edge case BillId values correctly.
        /// Input: PaymentVoucher with various edge case BillId values.
        /// Expected: Successfully processes and returns voucher with the specified BillId.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        [DataRow(-1)]
        public void Insert_EdgeCaseBillIds_ProcessesSuccessfully(int billId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns("PAYVCH-001");
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = billId,
                PaymentAmount = 100.0
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = (OkObjectResult)result;
            var returnedVoucher = (PaymentVoucher)okResult.Value;
            Assert.AreEqual(billId, returnedVoucher.BillId);
        }

        /// <summary>
        /// Tests that Insert method correctly handles IsFullPayment flag values.
        /// Input: PaymentVoucher with true and false IsFullPayment values.
        /// Expected: Successfully processes and returns voucher preserving the IsFullPayment flag.
        /// </summary>
        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public void Insert_IsFullPaymentFlag_PreservesValue(bool isFullPayment)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns("PAYVCH-001");
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = 100,
                PaymentAmount = 100.0,
                IsFullPayment = isFullPayment
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            var okResult = (OkObjectResult)result;
            var returnedVoucher = (PaymentVoucher)okResult.Value;
            Assert.AreEqual(isFullPayment, returnedVoucher.IsFullPayment);
        }

        /// <summary>
        /// Tests that Insert method assigns null when GetNumberSequence returns null.
        /// Input: Valid payload when GetNumberSequence returns null.
        /// Expected: PaymentVoucherName is set to null.
        /// </summary>
        [TestMethod]
        public void Insert_GetNumberSequenceReturnsNull_AssignsNullVoucherName()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns((string)null);
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockContext.Setup(x => x.SaveChanges()).Returns(1);
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = 100,
                PaymentAmount = 100.0
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            var okResult = (OkObjectResult)result;
            var returnedVoucher = (PaymentVoucher)okResult.Value;
            Assert.IsNull(returnedVoucher.PaymentVoucherName);
        }

        /// <summary>
        /// Tests that Insert method verifies all dependencies are called in correct order.
        /// Input: Valid payload.
        /// Expected: GetNumberSequence, Add, and SaveChanges are called once each in proper sequence.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsDependenciesInCorrectOrder()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var callOrder = new System.Collections.Generic.List<string>();
            mockNumberSequence.Setup(x => x.GetNumberSequence("PAYVCH")).Returns("PAYVCH-001").Callback(() => callOrder.Add("GetNumberSequence"));
            mockContext.Setup(x => x.PaymentVoucher).Returns(mockDbSet.Object);
            mockDbSet.Setup(x => x.Add(It.IsAny<PaymentVoucher>())).Callback(() => callOrder.Add("Add")).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentVoucher>)null);
            mockContext.Setup(x => x.SaveChanges()).Returns(1).Callback(() => callOrder.Add("SaveChanges"));
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                BillId = 100,
                PaymentAmount = 100.0
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            controller.Insert(payload);
            // Assert
            Assert.AreEqual(3, callOrder.Count);
            Assert.AreEqual("GetNumberSequence", callOrder[0]);
            Assert.AreEqual("Add", callOrder[1]);
            Assert.AreEqual("SaveChanges", callOrder[2]);
        }

        /// <summary>
        /// Tests that GetPaymentVoucher returns an OkObjectResult with empty list when no payment vouchers exist in the database.
        /// Input: Empty database with no PaymentVoucher records.
        /// Expected: Returns OkObjectResult with Items as empty list and Count as 0.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentVoucher_EmptyDatabase_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyData = new List<PaymentVoucher>();
            var mockSet = CreateMockDbSet(emptyData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentVoucher();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = itemsProperty.GetValue(value) as List<PaymentVoucher>;
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetPaymentVoucher returns an OkObjectResult with correct single payment voucher when one record exists.
        /// Input: Database containing one PaymentVoucher record.
        /// Expected: Returns OkObjectResult with Items containing one element and Count as 1.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentVoucher_SingleItem_ReturnsOkWithSingleItem()
        {
            // Arrange
            var testData = new List<PaymentVoucher>
            {
                new PaymentVoucher
                {
                    PaymentvoucherId = 1,
                    PaymentVoucherName = "PV001",
                    BillId = 100,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = 1000.50,
                    CashBankId = 1,
                    IsFullPayment = true
                }
            };
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentVoucher();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            var items = itemsProperty.GetValue(value) as List<PaymentVoucher>;
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("PV001", items[0].PaymentVoucherName);
            Assert.AreEqual(1000.50, items[0].PaymentAmount);
        }

        /// <summary>
        /// Tests that GetPaymentVoucher returns an OkObjectResult with all payment vouchers when multiple records exist.
        /// Input: Database containing multiple PaymentVoucher records.
        /// Expected: Returns OkObjectResult with Items containing all elements and correct Count.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentVoucher_MultipleItems_ReturnsOkWithAllItems()
        {
            // Arrange
            var testData = new List<PaymentVoucher>
            {
                new PaymentVoucher
                {
                    PaymentvoucherId = 1,
                    PaymentVoucherName = "PV001",
                    BillId = 100,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = 1000.50,
                    CashBankId = 1,
                    IsFullPayment = true
                },
                new PaymentVoucher
                {
                    PaymentvoucherId = 2,
                    PaymentVoucherName = "PV002",
                    BillId = 101,
                    PaymentDate = DateTimeOffset.Now.AddDays(-1),
                    PaymentTypeId = 2,
                    PaymentAmount = 2500.75,
                    CashBankId = 2,
                    IsFullPayment = false
                },
                new PaymentVoucher
                {
                    PaymentvoucherId = 3,
                    PaymentVoucherName = "PV003",
                    BillId = 102,
                    PaymentDate = DateTimeOffset.Now.AddDays(-2),
                    PaymentTypeId = 1,
                    PaymentAmount = 500.00,
                    CashBankId = 1,
                    IsFullPayment = true
                }
            };
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentVoucher();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = okResult.Value;
            Assert.IsNotNull(value);
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            var items = itemsProperty.GetValue(value) as List<PaymentVoucher>;
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual("PV001", items[0].PaymentVoucherName);
            Assert.AreEqual("PV002", items[1].PaymentVoucherName);
            Assert.AreEqual("PV003", items[2].PaymentVoucherName);
        }

        /// <summary>
        /// Tests that GetPaymentVoucher returns OkObjectResult with correct status code 200.
        /// Input: Database with payment vouchers.
        /// Expected: Returns OkObjectResult with StatusCode 200.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentVoucher_WhenCalled_ReturnsOkStatusCode()
        {
            // Arrange
            var testData = new List<PaymentVoucher>
            {
                new PaymentVoucher
                {
                    PaymentvoucherId = 1,
                    PaymentVoucherName = "PV001",
                    BillId = 100,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = 1000.00,
                    CashBankId = 1,
                    IsFullPayment = true
                }
            };
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentVoucher();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Tests that GetPaymentVoucher with edge case payment amounts returns correct data.
        /// Input: Database with payment vouchers having edge case amounts (zero, negative, very large, decimal precision).
        /// Expected: Returns OkObjectResult with correct Items and Count including edge case values.
        /// </summary>
        [TestMethod]
        public async Task GetPaymentVoucher_EdgeCaseAmounts_ReturnsCorrectData()
        {
            // Arrange
            var testData = new List<PaymentVoucher>
            {
                new PaymentVoucher
                {
                    PaymentvoucherId = 1,
                    PaymentVoucherName = "PV_ZERO",
                    BillId = 100,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = 0.0,
                    CashBankId = 1,
                    IsFullPayment = true
                },
                new PaymentVoucher
                {
                    PaymentvoucherId = 2,
                    PaymentVoucherName = "PV_NEGATIVE",
                    BillId = 101,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = -100.50,
                    CashBankId = 1,
                    IsFullPayment = false
                },
                new PaymentVoucher
                {
                    PaymentvoucherId = 3,
                    PaymentVoucherName = "PV_LARGE",
                    BillId = 102,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = double.MaxValue,
                    CashBankId = 1,
                    IsFullPayment = true
                },
                new PaymentVoucher
                {
                    PaymentvoucherId = 4,
                    PaymentVoucherName = "PV_PRECISION",
                    BillId = 103,
                    PaymentDate = DateTimeOffset.Now,
                    PaymentTypeId = 1,
                    PaymentAmount = 0.123456789,
                    CashBankId = 1,
                    IsFullPayment = true
                }
            };
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = await controller.GetPaymentVoucher();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var items = itemsProperty.GetValue(value) as List<PaymentVoucher>;
            Assert.IsNotNull(items);
            Assert.AreEqual(4, items.Count);
            Assert.AreEqual(0.0, items[0].PaymentAmount);
            Assert.AreEqual(-100.50, items[1].PaymentAmount);
            Assert.AreEqual(double.MaxValue, items[2].PaymentAmount);
            Assert.AreEqual(0.123456789, items[3].PaymentAmount);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations.
        /// </summary>
        private Mock<DbSet<PaymentVoucher>> CreateMockDbSet(List<PaymentVoucher> data)
        {
            var queryable = data.AsQueryable();
            var mockSet = new Mock<DbSet<PaymentVoucher>>();
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PaymentVoucher>(queryable.Provider));
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockSet.As<IAsyncEnumerable<PaymentVoucher>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<PaymentVoucher>(queryable.GetEnumerator()));
            return mockSet;
        }

        /// <summary>
        /// Helper class to enable async query operations for testing.
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
        /// Helper class to enable async enumerable operations for testing.
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

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        /// <summary>
        /// Helper class to enable async enumerator operations for testing.
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
        /// Tests that Update method successfully updates a payment voucher and returns OkObjectResult with the updated entity.
        /// Input: Valid CrudViewModel with valid PaymentVoucher.
        /// Expected: Update and SaveChanges are called, returns OkObjectResult containing the payment voucher.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithPaymentVoucher()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                PaymentVoucherName = "PV-001",
                BillId = 100,
                PaymentDate = DateTimeOffset.UtcNow,
                PaymentTypeId = 1,
                PaymentAmount = 1000.50,
                CashBankId = 1,
                IsFullPayment = true
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = paymentVoucher
            };
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<PaymentVoucher>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentVoucher>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentVoucher, okResult.Value);
            mockDbSet.Verify(d => d.Update(paymentVoucher), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles null value property in payload.
        /// Input: CrudViewModel with null value property.
        /// Expected: Update is called with null and SaveChanges is invoked.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_CallsUpdateWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var payload = new CrudViewModel<PaymentVoucher>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = null
            };
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<PaymentVoucher>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentVoucher>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            mockDbSet.Verify(d => d.Update(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles PaymentVoucher with extreme numeric values.
        /// Input: PaymentVoucher with int.MaxValue, double.MaxValue, and extreme dates.
        /// Expected: Update and SaveChanges are called successfully.
        /// </summary>
        [TestMethod]
        public void Update_PaymentVoucherWithExtremeValues_UpdatesSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = int.MaxValue,
                PaymentVoucherName = "PV-MAX",
                BillId = int.MaxValue,
                PaymentDate = DateTimeOffset.MaxValue,
                PaymentTypeId = int.MaxValue,
                PaymentAmount = double.MaxValue,
                CashBankId = int.MaxValue,
                IsFullPayment = false
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<PaymentVoucher>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentVoucher>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(paymentVoucher), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles PaymentVoucher with minimum numeric values.
        /// Input: PaymentVoucher with int.MinValue, negative amounts, and minimum dates.
        /// Expected: Update and SaveChanges are called successfully.
        /// </summary>
        [TestMethod]
        public void Update_PaymentVoucherWithMinimumValues_UpdatesSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = int.MinValue,
                PaymentVoucherName = string.Empty,
                BillId = int.MinValue,
                PaymentDate = DateTimeOffset.MinValue,
                PaymentTypeId = int.MinValue,
                PaymentAmount = double.MinValue,
                CashBankId = int.MinValue,
                IsFullPayment = false
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<PaymentVoucher>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentVoucher>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(paymentVoucher), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles PaymentVoucher with special double values.
        /// Input: PaymentVoucher with PaymentAmount set to double.NaN.
        /// Expected: Update and SaveChanges are called successfully.
        /// </summary>
        [TestMethod]
        public void Update_PaymentVoucherWithNaNAmount_UpdatesSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                PaymentVoucherName = "PV-NaN",
                PaymentAmount = double.NaN
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<PaymentVoucher>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentVoucher>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            mockDbSet.Verify(d => d.Update(paymentVoucher), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles PaymentVoucher with zero values.
        /// Input: PaymentVoucher with all numeric properties set to zero.
        /// Expected: Update and SaveChanges are called successfully.
        /// </summary>
        [TestMethod]
        public void Update_PaymentVoucherWithZeroValues_UpdatesSuccessfully()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockNumberSequence = new Mock<INumberSequence>();
            var mockDbSet = new Mock<DbSet<PaymentVoucher>>();
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 0,
                PaymentVoucherName = null,
                BillId = 0,
                PaymentDate = default,
                PaymentTypeId = 0,
                PaymentAmount = 0.0,
                CashBankId = 0,
                IsFullPayment = false
            };
            var payload = new CrudViewModel<PaymentVoucher>
            {
                value = paymentVoucher
            };
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<PaymentVoucher>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<PaymentVoucher>)null);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(paymentVoucher), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully removes and returns a PaymentVoucher when a valid ID is provided.
        /// Input: Valid payload with existing PaymentVoucher ID.
        /// Expected: Returns OkObjectResult with the removed PaymentVoucher.
        /// </summary>
        [TestMethod]
        public void Remove_ValidId_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 1,
                PaymentVoucherName = "PV-001",
                BillId = 100,
                PaymentAmount = 1000.0
            };
            var data = new List<PaymentVoucher>
            {
                paymentVoucher
            }.AsQueryable();
            var mockSet = new Mock<DbSet<PaymentVoucher>>();
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<PaymentVoucher>
            {
                key = 1
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentVoucher, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<PaymentVoucher>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles entity not found scenario.
        /// Input: Payload with ID that doesn't exist in the database.
        /// Expected: Remove is called with null, which typically throws or causes error.
        /// </summary>
        [TestMethod]
        public void Remove_EntityNotFound_CallsRemoveWithNull()
        {
            // Arrange
            var data = new List<PaymentVoucher>().AsQueryable();
            var mockSet = new Mock<DbSet<PaymentVoucher>>();
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<PaymentVoucher>
            {
                key = 999
            };
            // Act & Assert
            // Note: The current implementation will call Remove(null) which may throw
            // This test demonstrates the bug - no null check before Remove
            mockSet.Verify(m => m.Remove(null), Times.Never());
            var result = controller.Remove(payload);
            mockSet.Verify(m => m.Remove(null), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles null key by converting it to 0.
        /// Input: Payload with null key.
        /// Expected: Convert.ToInt32(null) returns 0, searches for ID 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_ConvertsToZero()
        {
            // Arrange
            var data = new List<PaymentVoucher>().AsQueryable();
            var mockSet = new Mock<DbSet<PaymentVoucher>>();
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<PaymentVoucher>
            {
                key = null
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockSet.Verify(m => m.Remove(null), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles string key that can be converted to int.
        /// Input: Payload with string key "42".
        /// Expected: Successfully converts to int and searches for ID 42.
        /// </summary>
        [TestMethod]
        public void Remove_StringKey_ConvertsSuccessfully()
        {
            // Arrange
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 42,
                PaymentVoucherName = "PV-042",
                BillId = 200,
                PaymentAmount = 2000.0
            };
            var data = new List<PaymentVoucher>
            {
                paymentVoucher
            }.AsQueryable();
            var mockSet = new Mock<DbSet<PaymentVoucher>>();
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<PaymentVoucher>
            {
                key = "42"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(paymentVoucher, okResult.Value);
        }

        /// <summary>
        /// Tests that Remove handles boundary values for integer keys.
        /// Input: Payloads with int.MaxValue and int.MinValue keys.
        /// Expected: Successfully converts and searches for the boundary values.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        [DataRow(0)]
        [DataRow(-1)]
        public void Remove_BoundaryIntegerKeys_HandlesCorrectly(int keyValue)
        {
            // Arrange
            var data = new List<PaymentVoucher>().AsQueryable();
            var mockSet = new Mock<DbSet<PaymentVoucher>>();
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<PaymentVoucher>
            {
                key = keyValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            mockSet.Verify(m => m.Remove(null), Times.Once());
        }

        /// <summary>
        /// Tests that Remove calls SaveChanges exactly once on successful removal.
        /// Input: Valid payload with existing PaymentVoucher ID.
        /// Expected: SaveChanges is called once.
        /// </summary>
        [TestMethod]
        public void Remove_ValidId_CallsSaveChangesOnce()
        {
            // Arrange
            var paymentVoucher = new PaymentVoucher
            {
                PaymentvoucherId = 123,
                PaymentVoucherName = "PV-123"
            };
            var data = new List<PaymentVoucher>
            {
                paymentVoucher
            }.AsQueryable();
            var mockSet = new Mock<DbSet<PaymentVoucher>>();
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PaymentVoucher>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.PaymentVoucher).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var mockNumberSequence = new Mock<INumberSequence>();
            var controller = new PaymentVoucherController(mockContext.Object, mockNumberSequence.Object);
            var payload = new CrudViewModel<PaymentVoucher>
            {
                key = 123
            };
            // Act
            controller.Remove(payload);
            // Assert
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }
    }
}