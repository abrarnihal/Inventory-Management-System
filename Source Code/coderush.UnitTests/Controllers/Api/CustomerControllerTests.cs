#nullable enable

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
    /// Unit tests for the CustomerController class.
    /// </summary>
    [TestClass]
    public class CustomerControllerTests
    {
        /// <summary>
        /// Tests that GetCustomer returns an OK result with an empty list when no customers exist in the database.
        /// Input: Empty customer collection.
        /// Expected: Returns OkObjectResult with Items as empty list and Count as 0.
        /// </summary>
        [TestMethod]
        public async Task GetCustomer_WhenNoCustomersExist_ReturnsOkWithEmptyListAndZeroCount()
        {
            // Arrange
            var customers = new List<Customer>();
            var mockSet = CreateMockDbSet(customers);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);
            var controller = new CustomerController(mockContext.Object);

            // Act
            var result = await controller.GetCustomer();

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

            var items = (List<Customer>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetCustomer returns an OK result with a single customer when one customer exists.
        /// Input: Collection with one customer.
        /// Expected: Returns OkObjectResult with Items containing one customer and Count as 1.
        /// </summary>
        [TestMethod]
        public async Task GetCustomer_WhenOneCustomerExists_ReturnsOkWithSingleCustomerAndCountOne()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer
                {
                    CustomerId = 1,
                    CustomerName = "John Doe",
                    CustomerTypeId = 1,
                    Address = "123 Main St",
                    City = "Springfield",
                    State = "IL",
                    ZipCode = "62701",
                    Phone = "555-1234",
                    Email = "john@example.com",
                    ContactPerson = "John Doe"
                }
            };
            var mockSet = CreateMockDbSet(customers);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);
            var controller = new CustomerController(mockContext.Object);

            // Act
            var result = await controller.GetCustomer();

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

            var items = (List<Customer>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("John Doe", items[0].CustomerName);
        }

        /// <summary>
        /// Tests that GetCustomer returns an OK result with multiple customers when multiple customers exist.
        /// Input: Collection with multiple customers.
        /// Expected: Returns OkObjectResult with Items containing all customers and correct Count.
        /// </summary>
        [TestMethod]
        [DataRow(2)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public async Task GetCustomer_WhenMultipleCustomersExist_ReturnsOkWithAllCustomersAndCorrectCount(int customerCount)
        {
            // Arrange
            var customers = new List<Customer>();
            for (int i = 1; i <= customerCount; i++)
            {
                customers.Add(new Customer
                {
                    CustomerId = i,
                    CustomerName = $"Customer {i}",
                    CustomerTypeId = i % 3 + 1,
                    Address = $"{i} Main St",
                    City = "TestCity",
                    State = "TS",
                    ZipCode = "12345",
                    Phone = $"555-{i:D4}",
                    Email = $"customer{i}@example.com",
                    ContactPerson = $"Contact {i}"
                });
            }
            var mockSet = CreateMockDbSet(customers);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);
            var controller = new CustomerController(mockContext.Object);

            // Act
            var result = await controller.GetCustomer();

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

            var items = (List<Customer>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(customerCount, items.Count);
            Assert.AreEqual(customerCount, count);
        }

        /// <summary>
        /// Tests that GetCustomer returns correct data structure with properly populated Items and Count properties.
        /// Input: Collection with specific customers.
        /// Expected: Returns OkObjectResult with anonymous object containing Items list and Count matching the list count.
        /// </summary>
        [TestMethod]
        public async Task GetCustomer_ReturnsCorrectDataStructure_WithItemsAndCountProperties()
        {
            // Arrange
            var customers = new List<Customer>
            {
                new Customer { CustomerId = 1, CustomerName = "Alice", CustomerTypeId = 1 },
                new Customer { CustomerId = 2, CustomerName = "Bob", CustomerTypeId = 2 },
                new Customer { CustomerId = 3, CustomerName = "Charlie", CustomerTypeId = 1 }
            };
            var mockSet = CreateMockDbSet(customers);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);
            var controller = new CustomerController(mockContext.Object);

            // Act
            var result = await controller.GetCustomer();

            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");

            Assert.IsNotNull(itemsProperty, "Items property should exist");
            Assert.IsNotNull(countProperty, "Count property should exist");

            var items = (List<Customer>?)itemsProperty.GetValue(value);
            var count = (int?)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(3, count);
            Assert.AreEqual(items.Count, count, "Count should match Items.Count");

            Assert.AreEqual("Alice", items[0].CustomerName);
            Assert.AreEqual("Bob", items[1].CustomerName);
            Assert.AreEqual("Charlie", items[2].CustomerName);
        }

        /// <summary>
        /// Helper method to create a mock DbSet that supports async operations.
        /// </summary>
        /// <param name="sourceList">The source data for the DbSet.</param>
        /// <returns>A mocked DbSet supporting ToListAsync.</returns>
        private static Mock<DbSet<Customer>> CreateMockDbSet(List<Customer> sourceList)
        {
            var queryable = sourceList.AsQueryable();
            var mockSet = new Mock<DbSet<Customer>>();

            mockSet.As<IAsyncEnumerable<Customer>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Customer>(queryable.GetEnumerator()));

            mockSet.As<IQueryable<Customer>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Customer>(queryable.Provider));

            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

            return mockSet;
        }

        /// <summary>
        /// Helper class to provide async query provider for mocking Entity Framework async operations.
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
                return _inner.Execute(expression)!;
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
                        types: new[] { typeof(Expression) })!
                    .MakeGenericMethod(resultType)
                    .Invoke(this, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult })!;
            }
        }

        /// <summary>
        /// Helper class to provide async enumerable for mocking Entity Framework async operations.
        /// </summary>
        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable)
            {
            }

            public TestAsyncEnumerable(Expression expression) : base(expression)
            {
            }

            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            }

            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        /// <summary>
        /// Helper class to provide async enumerator for mocking Entity Framework async operations.
        /// </summary>
        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;

            public TestAsyncEnumerator(IEnumerator<T> inner)
            {
                _inner = inner;
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(_inner.MoveNext());
            }

            public T Current => _inner.Current;

            public ValueTask DisposeAsync()
            {
                _inner.Dispose();
                return new ValueTask();
            }
        }

        /// <summary>
        /// Tests that Update returns OkObjectResult with the updated customer when given a valid payload.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithCustomer()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var customer = new Customer
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CustomerTypeId = 1,
                Address = "123 Test St",
                City = "Test City",
                State = "TS",
                ZipCode = "12345",
                Phone = "123-456-7890",
                Email = "test@example.com",
                ContactPerson = "John Doe"
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            var controller = new CustomerController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
            mockDbSet.Verify(d => d.Update(customer), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles null customer value in payload.
        /// </summary>
        [TestMethod]
        public void Update_NullCustomerValue_CallsUpdateWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var payload = new CrudViewModel<Customer>
            {
                value = null
            };

            var controller = new CustomerController(mockContext.Object);

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
        /// Tests that Update correctly updates a customer with minimal required properties.
        /// </summary>
        [TestMethod]
        public void Update_CustomerWithMinimalProperties_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var customer = new Customer
            {
                CustomerId = 0,
                CustomerName = "Minimal Customer",
                CustomerTypeId = 0
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            var controller = new CustomerController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
            mockDbSet.Verify(d => d.Update(customer), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update correctly handles a customer with extreme property values.
        /// </summary>
        [TestMethod]
        public void Update_CustomerWithExtremeValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var customer = new Customer
            {
                CustomerId = int.MaxValue,
                CustomerName = new string('A', 1000),
                CustomerTypeId = int.MinValue,
                Address = string.Empty,
                City = "   ",
                State = null,
                ZipCode = "!@#$%",
                Phone = new string('9', 500),
                Email = "a@b",
                ContactPerson = "\n\r\t"
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            var controller = new CustomerController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
            mockDbSet.Verify(d => d.Update(customer), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests the Remove method with a valid customer ID.
        /// Verifies that the customer is removed from the context and SaveChanges is called.
        /// Expected result: Returns OkObjectResult with the removed customer.
        /// </summary>
        [TestMethod]
        public void Remove_ValidCustomerId_ReturnsOkResultWithCustomer()
        {
            // Arrange
            var customerId = 123;
            var customer = new Customer { CustomerId = customerId, CustomerName = "Test Customer" };
            var customers = new List<Customer> { customer }.AsQueryable();

            var mockSet = new Mock<DbSet<Customer>>();
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);

            var controller = new CustomerController(mockContext.Object);
            var payload = new CrudViewModel<Customer> { key = customerId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests the Remove method with a customer ID as a string.
        /// Verifies that the string key is properly converted to integer.
        /// Expected result: Returns OkObjectResult with the removed customer.
        /// </summary>
        [TestMethod]
        public void Remove_KeyAsString_ReturnsOkResultWithCustomer()
        {
            // Arrange
            var customerId = 456;
            var customer = new Customer { CustomerId = customerId, CustomerName = "Test Customer" };
            var customers = new List<Customer> { customer }.AsQueryable();

            var mockSet = new Mock<DbSet<Customer>>();
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);

            var controller = new CustomerController(mockContext.Object);
            var payload = new CrudViewModel<Customer> { key = "456" };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
            mockSet.Verify(m => m.Remove(It.IsAny<Customer>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests the Remove method with a null key in the payload.
        /// Verifies that Convert.ToInt32(null) returns 0 and searches for customer ID 0.
        /// Expected result: Searches for customer with ID 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_SearchesForCustomerIdZero()
        {
            // Arrange
            var customer = new Customer { CustomerId = 0, CustomerName = "Zero Customer" };
            var customers = new List<Customer> { customer }.AsQueryable();

            var mockSet = new Mock<DbSet<Customer>>();
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);

            var controller = new CustomerController(mockContext.Object);
            var payload = new CrudViewModel<Customer> { key = null };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
        }

        /// <summary>
        /// Tests the Remove method with boundary value int.MinValue.
        /// Verifies that the method handles minimum integer value correctly.
        /// Expected result: Returns OkObjectResult if customer with that ID exists.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsIntMinValue_HandlesCorrectly()
        {
            // Arrange
            var customerId = int.MinValue;
            var customer = new Customer { CustomerId = customerId, CustomerName = "Min Customer" };
            var customers = new List<Customer> { customer }.AsQueryable();

            var mockSet = new Mock<DbSet<Customer>>();
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);

            var controller = new CustomerController(mockContext.Object);
            var payload = new CrudViewModel<Customer> { key = customerId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
        }

        /// <summary>
        /// Tests the Remove method with boundary value int.MaxValue.
        /// Verifies that the method handles maximum integer value correctly.
        /// Expected result: Returns OkObjectResult if customer with that ID exists.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsIntMaxValue_HandlesCorrectly()
        {
            // Arrange
            var customerId = int.MaxValue;
            var customer = new Customer { CustomerId = customerId, CustomerName = "Max Customer" };
            var customers = new List<Customer> { customer }.AsQueryable();

            var mockSet = new Mock<DbSet<Customer>>();
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);

            var controller = new CustomerController(mockContext.Object);
            var payload = new CrudViewModel<Customer> { key = customerId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
        }

        /// <summary>
        /// Tests the Remove method with key value of zero.
        /// Verifies that the method searches for customer with ID 0.
        /// Expected result: Returns OkObjectResult if customer with ID 0 exists.
        /// </summary>
        [TestMethod]
        public void Remove_KeyIsZero_SearchesForCustomerIdZero()
        {
            // Arrange
            var customer = new Customer { CustomerId = 0, CustomerName = "Zero Customer" };
            var customers = new List<Customer> { customer }.AsQueryable();

            var mockSet = new Mock<DbSet<Customer>>();
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);

            var controller = new CustomerController(mockContext.Object);
            var payload = new CrudViewModel<Customer> { key = 0 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
        }

        /// <summary>
        /// Tests the Remove method with negative customer ID.
        /// Verifies that the method handles negative IDs correctly.
        /// Expected result: Returns OkObjectResult if customer with that negative ID exists.
        /// </summary>
        [TestMethod]
        public void Remove_NegativeCustomerId_HandlesCorrectly()
        {
            // Arrange
            var customerId = -42;
            var customer = new Customer { CustomerId = customerId, CustomerName = "Negative Customer" };
            var customers = new List<Customer> { customer }.AsQueryable();

            var mockSet = new Mock<DbSet<Customer>>();
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Provider).Returns(customers.Provider);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.Expression).Returns(customers.Expression);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.ElementType).Returns(customers.ElementType);
            mockSet.As<IQueryable<Customer>>().Setup(m => m.GetEnumerator()).Returns(customers.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Customer).Returns(mockSet.Object);

            var controller = new CustomerController(mockContext.Object);
            var payload = new CrudViewModel<Customer> { key = customerId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(customer, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert successfully adds a valid customer to the database and returns an OkObjectResult.
        /// Input: Valid CrudViewModel with a customer object.
        /// Expected: Customer is added, SaveChanges is called, and OkObjectResult with the customer is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithCustomer()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockCustomerSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockCustomerSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CustomerController(mockContext.Object);

            var customer = new Customer
            {
                CustomerId = 1,
                CustomerName = "Test Customer",
                CustomerTypeId = 1,
                Address = "123 Main St",
                City = "Test City",
                State = "TS",
                ZipCode = "12345",
                Phone = "555-1234",
                Email = "test@example.com",
                ContactPerson = "John Doe"
            };

            var payload = new CrudViewModel<Customer>
            {
                action = "insert",
                value = customer
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result;
            Assert.AreSame(customer, okResult.Value);

            mockCustomerSet.Verify(m => m.Add(customer), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert handles null customer value from payload.
        /// Input: CrudViewModel with null value property.
        /// Expected: Null is passed to Add method and SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndSavesChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockCustomerSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockCustomerSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CustomerController(mockContext.Object);

            var payload = new CrudViewModel<Customer>
            {
                action = "insert",
                value = null!
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));

            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);

            mockCustomerSet.Verify(m => m.Add(null!), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert correctly handles a customer with minimum required data.
        /// Input: CrudViewModel with customer containing only required CustomerName.
        /// Expected: Customer is added, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerWithMinimalData_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockCustomerSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockCustomerSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CustomerController(mockContext.Object);

            var customer = new Customer
            {
                CustomerId = 0,
                CustomerName = "Minimal Customer"
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockCustomerSet.Verify(m => m.Add(customer), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert correctly handles a customer with all fields populated with boundary values.
        /// Input: CrudViewModel with customer containing very long strings and edge case values.
        /// Expected: Customer is added, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerWithBoundaryValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockCustomerSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockCustomerSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CustomerController(mockContext.Object);

            var veryLongString = new string('a', 10000);
            var customer = new Customer
            {
                CustomerId = int.MaxValue,
                CustomerName = veryLongString,
                CustomerTypeId = int.MaxValue,
                Address = veryLongString,
                City = veryLongString,
                State = veryLongString,
                ZipCode = veryLongString,
                Phone = veryLongString,
                Email = veryLongString,
                ContactPerson = veryLongString
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockCustomerSet.Verify(m => m.Add(customer), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert correctly handles a customer with empty strings.
        /// Input: CrudViewModel with customer containing empty string values.
        /// Expected: Customer is added, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerWithEmptyStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockCustomerSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockCustomerSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CustomerController(mockContext.Object);

            var customer = new Customer
            {
                CustomerId = 0,
                CustomerName = "",
                Address = "",
                City = "",
                State = "",
                ZipCode = "",
                Phone = "",
                Email = "",
                ContactPerson = ""
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockCustomerSet.Verify(m => m.Add(customer), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert correctly handles a customer with special characters in string fields.
        /// Input: CrudViewModel with customer containing special and control characters.
        /// Expected: Customer is added, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockCustomerSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockCustomerSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CustomerController(mockContext.Object);

            var specialChars = "!@#$%^&*()_+{}|:\"<>?[];',./`~\r\n\t";
            var customer = new Customer
            {
                CustomerId = 1,
                CustomerName = specialChars,
                Address = specialChars,
                City = specialChars,
                State = specialChars,
                ZipCode = specialChars,
                Phone = specialChars,
                Email = specialChars,
                ContactPerson = specialChars
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockCustomerSet.Verify(m => m.Add(customer), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert correctly handles negative CustomerId values.
        /// Input: CrudViewModel with customer containing negative ID values.
        /// Expected: Customer is added, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_CustomerWithNegativeIds_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockCustomerSet = new Mock<DbSet<Customer>>();

            mockContext.Setup(c => c.Customer).Returns(mockCustomerSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new CustomerController(mockContext.Object);

            var customer = new Customer
            {
                CustomerId = -1,
                CustomerName = "Negative ID Customer",
                CustomerTypeId = -100
            };

            var payload = new CrudViewModel<Customer>
            {
                value = customer
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockCustomerSet.Verify(m => m.Add(customer), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }
    }
}