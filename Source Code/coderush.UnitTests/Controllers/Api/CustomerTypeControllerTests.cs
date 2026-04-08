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
    /// Unit tests for the CustomerTypeController class.
    /// </summary>
    [TestClass]
    public class CustomerTypeControllerTests
    {
        /// <summary>
        /// Tests that GetCustomerType returns OkObjectResult with empty Items and Count of 0 when database contains no CustomerType records.
        /// </summary>
        [TestMethod]
        public async Task GetCustomerType_EmptyDatabase_ReturnsOkWithEmptyListAndZeroCount()
        {
            // Arrange
            var customerTypes = new List<CustomerType>();
            var mockSet = CreateMockDbSet(customerTypes.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            var controller = new CustomerTypeController(mockContext.Object);
            // Act
            var result = await controller.GetCustomerType();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<CustomerType>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetCustomerType returns OkObjectResult with single item and Count of 1 when database contains one CustomerType record.
        /// </summary>
        [TestMethod]
        public async Task GetCustomerType_SingleItem_ReturnsOkWithSingleItemAndCountOne()
        {
            // Arrange
            var customerTypes = new List<CustomerType>
            {
                new CustomerType
                {
                    CustomerTypeId = 1,
                    CustomerTypeName = "Retail",
                    Description = "Retail customers"
                }
            };
            var mockSet = CreateMockDbSet(customerTypes.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            var controller = new CustomerTypeController(mockContext.Object);
            // Act
            var result = await controller.GetCustomerType();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<CustomerType>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].CustomerTypeId);
            Assert.AreEqual("Retail", items[0].CustomerTypeName);
        }

        /// <summary>
        /// Tests that GetCustomerType returns OkObjectResult with multiple items and correct Count when database contains multiple CustomerType records.
        /// </summary>
        [TestMethod]
        public async Task GetCustomerType_MultipleItems_ReturnsOkWithAllItemsAndCorrectCount()
        {
            // Arrange
            var customerTypes = new List<CustomerType>
            {
                new CustomerType
                {
                    CustomerTypeId = 1,
                    CustomerTypeName = "Retail",
                    Description = "Retail customers"
                },
                new CustomerType
                {
                    CustomerTypeId = 2,
                    CustomerTypeName = "Wholesale",
                    Description = "Wholesale customers"
                },
                new CustomerType
                {
                    CustomerTypeId = 3,
                    CustomerTypeName = "Corporate",
                    Description = "Corporate customers"
                }
            };
            var mockSet = CreateMockDbSet(customerTypes.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            var controller = new CustomerTypeController(mockContext.Object);
            // Act
            var result = await controller.GetCustomerType();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            var items = value.Items as List<CustomerType>;
            var count = (int)value.Count;
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual("Retail", items[0].CustomerTypeName);
            Assert.AreEqual("Wholesale", items[1].CustomerTypeName);
            Assert.AreEqual("Corporate", items[2].CustomerTypeName);
        }

        /// <summary>
        /// Tests that GetCustomerType returns OkObjectResult with correct structure containing Items and Count properties.
        /// </summary>
        [TestMethod]
        public async Task GetCustomerType_ValidRequest_ReturnsOkObjectResultWithCorrectStructure()
        {
            // Arrange
            var customerTypes = new List<CustomerType>
            {
                new CustomerType
                {
                    CustomerTypeId = 1,
                    CustomerTypeName = "Test Type",
                    Description = "Test Description"
                }
            };
            var mockSet = CreateMockDbSet(customerTypes.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            var controller = new CustomerTypeController(mockContext.Object);
            // Act
            var result = await controller.GetCustomerType();
            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic value = okResult.Value;
            Assert.IsNotNull(value);
            // Verify the anonymous type has the expected properties
            var valueType = value.GetType();
            var itemsProperty = valueType.GetProperty("Items");
            var countProperty = valueType.GetProperty("Count");
            Assert.IsNotNull(itemsProperty, "Items property should exist");
            Assert.IsNotNull(countProperty, "Count property should exist");
        }

        /// <summary>
        /// Tests that GetCustomerType handles large collections correctly.
        /// </summary>
        [TestMethod]
        public async Task GetCustomerType_LargeCollection_ReturnsOkWithAllItemsAndCorrectCount()
        {
            // Arrange
            var customerTypes = new List<CustomerType>();
            for (int i = 1; i <= 100; i++)
            {
                customerTypes.Add(new CustomerType { CustomerTypeId = i, CustomerTypeName = $"Type{i}", Description = $"Description{i}" });
            }

            var mockSet = CreateMockDbSet(customerTypes.AsQueryable());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            var controller = new CustomerTypeController(mockContext.Object);
            // Act
            var result = await controller.GetCustomerType();
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            dynamic value = okResult.Value;
            var items = value.Items as List<CustomerType>;
            var count = (int)value.Count;
            Assert.AreEqual(100, items.Count);
            Assert.AreEqual(100, count);
        }

        /// <summary>
        /// Helper method to create a mock DbSet with async enumeration support.
        /// </summary>
        /// <param name = "sourceList">The source queryable data.</param>
        /// <returns>A mocked DbSet.</returns>
        private Mock<DbSet<CustomerType>> CreateMockDbSet(IQueryable<CustomerType> sourceList)
        {
            var mockSet = new Mock<DbSet<CustomerType>>();
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<CustomerType>(sourceList.Provider));
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Expression).Returns(sourceList.Expression);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.ElementType).Returns(sourceList.ElementType);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.GetEnumerator()).Returns(sourceList.GetEnumerator());
            mockSet.As<IAsyncEnumerable<CustomerType>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<CustomerType>(sourceList.GetEnumerator()));
            return mockSet;
        }

        /// <summary>
        /// Helper class to support async query operations for testing.
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
        /// Helper class to support async enumeration for testing.
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
        /// Helper class to support async enumerator for testing.
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
        /// Tests that Insert method successfully adds a CustomerType entity to the database
        /// and returns an OkObjectResult with the added entity when given a valid payload.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithCustomerType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = 1,
                CustomerTypeName = "Retail",
                Description = "Retail customers"
            };
            var payload = new CrudViewModel<CustomerType>
            {
                action = "insert",
                value = customerType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles a payload with null value property.
        /// This tests the behavior when the value property of the payload is null.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                action = "insert",
                value = null
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests Insert with a CustomerType containing empty strings for string properties.
        /// Validates that the method handles edge case string values correctly.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerTypeWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = 0,
                CustomerTypeName = "",
                Description = ""
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests Insert with a CustomerType containing very long string values.
        /// Validates that the method handles boundary conditions for string length.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerTypeWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var longString = new string ('A', 10000);
            var customerType = new CustomerType
            {
                CustomerTypeId = int.MaxValue,
                CustomerTypeName = longString,
                Description = longString
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests Insert with a CustomerType containing special characters and whitespace.
        /// Validates that the method handles special characters in string properties.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerTypeWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = -1,
                CustomerTypeName = "!@#$%^&*()_+-={}[]|\\:\";<>?,./\n\r\t",
                Description = "   \t\n\r   "
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests Insert with boundary values for numeric CustomerTypeId property.
        /// Validates that the method handles minimum, maximum, and zero values correctly.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(1)]
        public void Insert_CustomerTypeWithBoundaryIds_ReturnsOkResult(int customerId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = customerId,
                CustomerTypeName = "Test",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
        }

        /// <summary>
        /// Tests that Update returns OkObjectResult with the customer type when given a valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkWithCustomerType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = 1,
                CustomerTypeName = "Retail",
                Description = "Retail customers"
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType,
                action = "update",
                key = 1,
                antiForgery = "token"
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
            mockDbSet.Verify(d => d.Update(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles null value in payload by passing null to Update method.
        /// </summary>
        [TestMethod]
        public void Update_NullValueInPayload_PassesNullToUpdate()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                value = null,
                action = "update",
                key = 1,
                antiForgery = "token"
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
        /// Tests that Update handles customer type with minimum valid data.
        /// </summary>
        [TestMethod]
        public void Update_MinimumValidData_ReturnsOkWithCustomerType()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = 0,
                CustomerTypeName = "A",
                Description = null
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
            mockDbSet.Verify(d => d.Update(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles customer type with boundary values for Id.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(int.MaxValue)]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(1)]
        public void Update_BoundaryCustomerTypeId_ReturnsOkWithCustomerType(int customerTypeId)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = customerTypeId,
                CustomerTypeName = "TestType",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
            Assert.AreEqual(customerTypeId, ((CustomerType)okResult.Value).CustomerTypeId);
            mockDbSet.Verify(d => d.Update(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles customer type with edge case string values.
        /// </summary>
        [TestMethod]
        [DataRow("", "")]
        [DataRow("   ", "   ")]
        [DataRow("VeryLongCustomerTypeNameThatExceedsNormalLengthExpectationsAndCouldPotentiallyCauseDatabaseIssues", "VeryLongDescriptionThatExceedsNormalLengthExpectationsAndCouldPotentiallyCauseDatabaseIssuesWithVeryLongText")]
        [DataRow("Special!@#$%^&*()Characters", "Description with special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
        [DataRow("\n\r\t", "\n\r\t")]
        public void Update_EdgeCaseStringValues_ReturnsOkWithCustomerType(string customerTypeName, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = 1,
                CustomerTypeName = customerTypeName,
                Description = description
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
            mockDbSet.Verify(d => d.Update(customerType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update verifies SaveChanges is called exactly once.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsSaveChangesOnce()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = 5,
                CustomerTypeName = "Premium",
                Description = "Premium tier customers"
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            controller.Update(payload);
            // Assert
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update verifies Update method on DbSet is called exactly once with correct entity.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_CallsUpdateOnDbSetOnceWithCorrectEntity()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<CustomerType>>();
            mockContext.Setup(c => c.CustomerType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var customerType = new CustomerType
            {
                CustomerTypeId = 10,
                CustomerTypeName = "Corporate",
                Description = "Corporate clients"
            };
            var payload = new CrudViewModel<CustomerType>
            {
                value = customerType
            };
            // Act
            controller.Update(payload);
            // Assert
            mockDbSet.Verify(d => d.Update(It.Is<CustomerType>(ct => ct.CustomerTypeId == 10 && ct.CustomerTypeName == "Corporate" && ct.Description == "Corporate clients")), Times.Once);
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing CustomerType and returns OkObjectResult.
        /// Input: Valid payload with existing CustomerType ID
        /// Expected: CustomerType is removed, SaveChanges is called, and OkObjectResult is returned
        /// </summary>
        [TestMethod]
        public void Remove_ValidKey_RemovesCustomerTypeAndReturnsOk()
        {
            // Arrange
            var customerTypeId = 1;
            var customerType = new CustomerType
            {
                CustomerTypeId = customerTypeId,
                CustomerTypeName = "Premium",
                Description = "Premium customers"
            };
            var data = new List<CustomerType>
            {
                customerType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<CustomerType>>();
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                key = customerTypeId
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
            mockSet.Verify(m => m.Remove(customerType), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove accepts a valid string key and converts it correctly.
        /// Input: Payload with valid numeric string key
        /// Expected: CustomerType is removed and OkObjectResult is returned
        /// </summary>
        [TestMethod]
        public void Remove_ValidStringKey_RemovesCustomerTypeAndReturnsOk()
        {
            // Arrange
            var customerTypeId = 42;
            var customerType = new CustomerType
            {
                CustomerTypeId = customerTypeId,
                CustomerTypeName = "Standard",
                Description = "Standard customers"
            };
            var data = new List<CustomerType>
            {
                customerType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<CustomerType>>();
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                key = "42"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customerType, okResult.Value);
            mockSet.Verify(m => m.Remove(customerType), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles key value of zero correctly.
        /// Input: Payload with key = 0
        /// Expected: Queries for CustomerType with ID 0
        /// </summary>
        [TestMethod]
        public void Remove_KeyValueZero_QueriesWithZeroId()
        {
            // Arrange
            var customerType = new CustomerType
            {
                CustomerTypeId = 0,
                CustomerTypeName = "Default",
                Description = "Default type"
            };
            var data = new List<CustomerType>
            {
                customerType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<CustomerType>>();
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                key = 0
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(customerType), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles negative key values correctly.
        /// Input: Payload with negative key value
        /// Expected: Queries for CustomerType with negative ID
        /// </summary>
        [TestMethod]
        public void Remove_NegativeKey_QueriesWithNegativeId()
        {
            // Arrange
            var customerTypeId = -1;
            var customerType = new CustomerType
            {
                CustomerTypeId = customerTypeId,
                CustomerTypeName = "Negative",
                Description = "Negative ID"
            };
            var data = new List<CustomerType>
            {
                customerType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<CustomerType>>();
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                key = -1
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(customerType), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles maximum integer value correctly.
        /// Input: Payload with key = int.MaxValue
        /// Expected: Queries for CustomerType with ID = int.MaxValue
        /// </summary>
        [TestMethod]
        public void Remove_MaxIntKey_QueriesWithMaxIntId()
        {
            // Arrange
            var customerType = new CustomerType
            {
                CustomerTypeId = int.MaxValue,
                CustomerTypeName = "MaxInt",
                Description = "Max integer ID"
            };
            var data = new List<CustomerType>
            {
                customerType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<CustomerType>>();
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                key = int.MaxValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(customerType), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles minimum integer value correctly.
        /// Input: Payload with key = int.MinValue
        /// Expected: Queries for CustomerType with ID = int.MinValue
        /// </summary>
        [TestMethod]
        public void Remove_MinIntKey_QueriesWithMinIntId()
        {
            // Arrange
            var customerType = new CustomerType
            {
                CustomerTypeId = int.MinValue,
                CustomerTypeName = "MinInt",
                Description = "Min integer ID"
            };
            var data = new List<CustomerType>
            {
                customerType
            }.AsQueryable();
            var mockSet = new Mock<DbSet<CustomerType>>();
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<CustomerType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.CustomerType).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new CustomerTypeController(mockContext.Object);
            var payload = new CrudViewModel<CustomerType>
            {
                key = int.MinValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(customerType), Times.Once);
        }
    }
}