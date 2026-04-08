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
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the CashBankController class.
    /// </summary>
    [TestClass]
    public class CashBankControllerTests
    {
        /// <summary>
        /// Tests that GetCashBank returns an OkObjectResult with empty list and count of 0 when database contains no items.
        /// Input: Empty database.
        /// Expected: OkObjectResult with Items as empty list and Count as 0.
        /// </summary>
        [TestMethod]
        public async Task GetCashBank_EmptyDatabase_ReturnsOkWithEmptyListAndZeroCount()
        {
            // Arrange
            var data = new List<CashBank>();
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);
            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = await controller.GetCashBank();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = (List<CashBank>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetCashBank returns an OkObjectResult with a single item and count of 1 when database contains one item.
        /// Input: Database with one CashBank item.
        /// Expected: OkObjectResult with Items containing 1 element and Count as 1.
        /// </summary>
        [TestMethod]
        public async Task GetCashBank_SingleItem_ReturnsOkWithSingleItemAndCountOne()
        {
            // Arrange
            var data = new List<CashBank>
            {
                new CashBank { CashBankId = 1, CashBankName = "Test Bank", Description = "Test Description" }
            };
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);
            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = await controller.GetCashBank();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            var items = (List<CashBank>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("Test Bank", items[0].CashBankName);
        }

        /// <summary>
        /// Tests that GetCashBank returns an OkObjectResult with multiple items and correct count when database contains multiple items.
        /// Input: Database with multiple CashBank items.
        /// Expected: OkObjectResult with Items containing all elements and Count matching the number of items.
        /// </summary>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public async Task GetCashBank_MultipleItems_ReturnsOkWithAllItemsAndCorrectCount(int itemCount)
        {
            // Arrange
            var data = new List<CashBank>();
            for (int i = 1; i <= itemCount; i++)
            {
                data.Add(new CashBank
                {
                    CashBankId = i,
                    CashBankName = $"Bank {i}",
                    Description = $"Description {i}"
                });
            }
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);
            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = await controller.GetCashBank();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            var items = (List<CashBank>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(itemCount, items.Count);
            Assert.AreEqual(itemCount, count);
        }

        /// <summary>
        /// Tests that GetCashBank returns an OkObjectResult with items containing special characters in names and descriptions.
        /// Input: Database with CashBank items having special characters and edge case strings.
        /// Expected: OkObjectResult with Items correctly containing all special character strings and correct count.
        /// </summary>
        [TestMethod]
        public async Task GetCashBank_ItemsWithSpecialCharacters_ReturnsOkWithAllItems()
        {
            // Arrange
            var data = new List<CashBank>
            {
                new CashBank { CashBankId = 1, CashBankName = "Bank with spaces", Description = "Normal" },
                new CashBank { CashBankId = 2, CashBankName = "Bank-with-dashes", Description = "Has-dashes" },
                new CashBank { CashBankId = 3, CashBankName = "Bank@#$%", Description = "Special!@#$%" },
                new CashBank { CashBankId = 4, CashBankName = "", Description = "" },
                new CashBank { CashBankId = 5, CashBankName = "   ", Description = "   " },
                new CashBank { CashBankId = 6, CashBankName = "Very long bank name that exceeds normal length expectations to test handling of long strings in the system", Description = "Very long description that exceeds normal length" }
            };
            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);
            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = await controller.GetCashBank();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);

            var value = okResult.Value;
            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            var items = (List<CashBank>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(6, items.Count);
            Assert.AreEqual(6, count);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations for testing Entity Framework queries.
        /// </summary>
        /// <param name="sourceList">The source data to populate the mock DbSet.</param>
        /// <returns>A mock DbSet configured for async operations.</returns>
        private static Mock<DbSet<CashBank>> CreateMockDbSet(List<CashBank> sourceList)
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<CashBank>>();

            mockSet.As<IQueryable<CashBank>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<CashBank>(queryable.Provider));
            mockSet.As<IQueryable<CashBank>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<CashBank>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<CashBank>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());

            mockSet.As<IAsyncEnumerable<CashBank>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(() => new TestAsyncEnumerator<CashBank>(sourceList.GetEnumerator()));

            return mockSet;
        }

        /// <summary>
        /// Test async query provider for mocking Entity Framework async queries.
        /// </summary>
        private class TestAsyncQueryProvider<TEntity> : IQueryProvider, IAsyncQueryProvider
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
                var expectedResultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(System.Linq.Expressions.Expression) })!
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(this, new object[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(expectedResultType)
                    .Invoke(null, new object[] { executionResult })!;
            }
        }

        /// <summary>
        /// Test async enumerable for mocking Entity Framework async queries.
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

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        /// <summary>
        /// Test async enumerator for mocking Entity Framework async queries.
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
        /// Tests that Insert returns OkObjectResult with the CashBank entity when provided with a valid payload.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithCashBank()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CashBank>>();

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CashBankController(mockContext.Object);

            var cashBank = new CashBank
            {
                CashBankId = 1,
                CashBankName = "Test Bank",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<CashBank>
            {
                value = cashBank,
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
            Assert.AreEqual(cashBank, okResult.Value);
            mockDbSet.Verify(d => d.Add(cashBank), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert correctly adds the CashBank entity to the context and saves changes.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_CallsAddAndSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CashBank>>();

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CashBankController(mockContext.Object);

            var cashBank = new CashBank
            {
                CashBankId = 0,
                CashBankName = string.Empty,
                Description = string.Empty
            };

            var payload = new CrudViewModel<CashBank> { value = cashBank };

            // Act
            controller.Insert(payload);

            // Assert
            mockDbSet.Verify(d => d.Add(It.Is<CashBank>(c => c == cashBank)), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles a payload with null value property.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndSavesChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CashBank>>();

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CashBankController(mockContext.Object);

            var payload = new CrudViewModel<CashBank>
            {
                value = null,
                action = "insert"
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
        /// Tests Insert with various edge case values for CashBank properties.
        /// Validates behavior with extreme ID values, empty strings, whitespace, and special characters.
        /// </summary>
        /// <param name="cashBankId">The CashBank ID to test.</param>
        /// <param name="cashBankName">The CashBank name to test.</param>
        /// <param name="description">The description to test.</param>
        [TestMethod]
        [DataRow(int.MinValue, "", "", DisplayName = "Minimum int ID with empty strings")]
        [DataRow(int.MaxValue, "    ", "   ", DisplayName = "Maximum int ID with whitespace")]
        [DataRow(0, "Bank@#$%^&*()", "Desc<>?:|", DisplayName = "Zero ID with special characters")]
        [DataRow(-1, "Very Long Bank Name That Exceeds Normal Length Expectations With Many Characters To Test Boundary Conditions", "Very Long Description", DisplayName = "Negative ID with very long strings")]
        [DataRow(1, "\t\n\r", "\0", DisplayName = "Positive ID with control characters")]
        public void Insert_EdgeCaseValues_ReturnsOkResult(int cashBankId, string cashBankName, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CashBank>>();

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CashBankController(mockContext.Object);

            var cashBank = new CashBank
            {
                CashBankId = cashBankId,
                CashBankName = cashBankName,
                Description = description
            };

            var payload = new CrudViewModel<CashBank> { value = cashBank };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(cashBank, okResult.Value);
            mockDbSet.Verify(d => d.Add(It.Is<CashBank>(c =>
                c.CashBankId == cashBankId &&
                c.CashBankName == cashBankName &&
                c.Description == description)), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert returns the exact same CashBank instance that was in the payload.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsSameCashBankInstance()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CashBank>>();

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CashBankController(mockContext.Object);

            var cashBank = new CashBank
            {
                CashBankId = 42,
                CashBankName = "Specific Bank",
                Description = "Specific Description"
            };

            var payload = new CrudViewModel<CashBank> { value = cashBank };

            // Act
            var result = controller.Insert(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(cashBank, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method successfully updates a CashBank entity and returns an OkObjectResult
        /// when provided with a valid payload containing a CashBank object.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithCashBank()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CashBank>>();

            var cashBank = new CashBank
            {
                CashBankId = 1,
                CashBankName = "Test Bank",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<CashBank>
            {
                value = cashBank
            };

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<CashBank>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CashBank>?)null!);

            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(cashBank, okResult.Value);
            mockDbSet.Verify(d => d.Update(cashBank), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method calls DbSet.Update with null value and SaveChanges
        /// when payload.value is null, verifying no null-checking is performed.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_CallsUpdateWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CashBank>>();

            var payload = new CrudViewModel<CashBank>
            {
                value = null!
            };

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<CashBank>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CashBank>?)null!);

            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            mockDbSet.Verify(d => d.Update(null!), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method properly handles different CashBank property values
        /// including empty strings and special characters.
        /// </summary>
        [TestMethod]
        [DataRow(0, "", "")]
        [DataRow(int.MaxValue, "Bank with special chars !@#$%", "Description with\nnewlines")]
        [DataRow(-1, "   ", "   ")]
        [DataRow(999999, "Very long bank name that contains many characters to test the boundary limits of string handling", "Another very long description")]
        public void Update_VariousCashBankValues_ReturnsOkResult(int id, string name, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CashBank>>();

            var cashBank = new CashBank
            {
                CashBankId = id,
                CashBankName = name,
                Description = description
            };

            var payload = new CrudViewModel<CashBank>
            {
                value = cashBank
            };

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<CashBank>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CashBank>?)null!);

            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(cashBank, okResult.Value);
            mockDbSet.Verify(d => d.Update(cashBank), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method returns correct status code (200 OK)
        /// for a successful update operation.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_Returns200StatusCode()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CashBank>>();

            var cashBank = new CashBank
            {
                CashBankId = 1,
                CashBankName = "Test Bank",
                Description = "Test Description"
            };

            var payload = new CrudViewModel<CashBank>
            {
                value = cashBank
            };

            mockContext.Setup(c => c.CashBank).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<CashBank>())).Returns((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<CashBank>?)null!);

            var controller = new CashBankController(mockContext.Object);

            // Act
            var result = controller.Update(payload) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result.StatusCode);
        }

        /// <summary>
        /// Tests that Remove returns OkObjectResult with the removed entity when a valid existing ID is provided.
        /// </summary>
        [TestMethod]
        public void Remove_ValidExistingId_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            var cashBankToRemove = new CashBank { CashBankId = 1, CashBankName = "Test Bank", Description = "Test" };
            var data = new List<CashBank> { cashBankToRemove }.AsQueryable();

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);

            var controller = new CashBankController(mockContext.Object);
            var payload = new CrudViewModel<CashBank> { key = 1 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(cashBankToRemove, okResult.Value);
            mockSet.Verify(m => m.Remove(cashBankToRemove), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles string key that represents a valid integer.
        /// </summary>
        [TestMethod]
        public void Remove_StringKeyWithValidInteger_ReturnsOkWithRemovedEntity()
        {
            // Arrange
            var cashBankToRemove = new CashBank { CashBankId = 42, CashBankName = "Test Bank", Description = "Test" };
            var data = new List<CashBank> { cashBankToRemove }.AsQueryable();

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);

            var controller = new CashBankController(mockContext.Object);
            var payload = new CrudViewModel<CashBank> { key = "42" };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(cashBankToRemove), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles null key by converting it to 0 and searching for ID 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_SearchesForIdZero()
        {
            // Arrange
            var cashBankWithZeroId = new CashBank { CashBankId = 0, CashBankName = "Zero Bank", Description = "Test" };
            var data = new List<CashBank> { cashBankWithZeroId }.AsQueryable();

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);

            var controller = new CashBankController(mockContext.Object);
            var payload = new CrudViewModel<CashBank> { key = null };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(cashBankWithZeroId, okResult.Value);
            mockSet.Verify(m => m.Remove(cashBankWithZeroId), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles boundary value int.MinValue as key.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsIntMinValue_SearchesForMinValueId()
        {
            // Arrange
            var cashBank = new CashBank { CashBankId = int.MinValue, CashBankName = "Min Bank", Description = "Test" };
            var data = new List<CashBank> { cashBank }.AsQueryable();

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);

            var controller = new CashBankController(mockContext.Object);
            var payload = new CrudViewModel<CashBank> { key = int.MinValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(cashBank), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles boundary value int.MaxValue as key.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsIntMaxValue_SearchesForMaxValueId()
        {
            // Arrange
            var cashBank = new CashBank { CashBankId = int.MaxValue, CashBankName = "Max Bank", Description = "Test" };
            var data = new List<CashBank> { cashBank }.AsQueryable();

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);

            var controller = new CashBankController(mockContext.Object);
            var payload = new CrudViewModel<CashBank> { key = int.MaxValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(cashBank), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles negative ID values correctly.
        /// </summary>
        [TestMethod]
        public void Remove_NegativeId_SearchesForNegativeId()
        {
            // Arrange
            var cashBank = new CashBank { CashBankId = -100, CashBankName = "Negative Bank", Description = "Test" };
            var data = new List<CashBank> { cashBank }.AsQueryable();

            var mockSet = CreateMockDbSet(data);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.CashBank).Returns(mockSet.Object);

            var controller = new CashBankController(mockContext.Object);
            var payload = new CrudViewModel<CashBank> { key = -100 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(cashBank), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Helper method to create a mock DbSet with queryable support.
        /// </summary>
        private static Mock<DbSet<CashBank>> CreateMockDbSet(IQueryable<CashBank> data)
        {
            var mockSet = new Mock<DbSet<CashBank>>();
            mockSet.As<IQueryable<CashBank>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CashBank>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CashBank>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CashBank>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}