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
    /// Test class for PurchaseTypeController.
    /// </summary>
    [TestClass]
    public class PurchaseTypeControllerTests
    {
        /// <summary>
        /// Tests that GetPurchaseType returns OkObjectResult with empty list when database contains no records.
        /// Input: Empty database.
        /// Expected: OkObjectResult with Items as empty list and Count as 0.
        /// </summary>
        [TestMethod]
        public async Task GetPurchaseType_EmptyDatabase_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyList = new List<PurchaseType>();
            var mockDbSet = CreateMockDbSet(emptyList);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            var controller = new PurchaseTypeController(mockContext.Object);
            // Act
            var result = await controller.GetPurchaseType();
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
            var items = itemsProperty.GetValue(value) as List<PurchaseType>;
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetPurchaseType returns OkObjectResult with single item when database contains one record.
        /// Input: Database with one PurchaseType record.
        /// Expected: OkObjectResult with Items containing one element and Count as 1.
        /// </summary>
        [TestMethod]
        public async Task GetPurchaseType_SingleRecord_ReturnsOkWithSingleItem()
        {
            // Arrange
            var singleItem = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = 1,
                    PurchaseTypeName = "Type1",
                    Description = "Description1"
                }
            };
            var mockDbSet = CreateMockDbSet(singleItem);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            var controller = new PurchaseTypeController(mockContext.Object);
            // Act
            var result = await controller.GetPurchaseType();
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
            var items = itemsProperty.GetValue(value) as List<PurchaseType>;
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual(1, items[0].PurchaseTypeId);
            Assert.AreEqual("Type1", items[0].PurchaseTypeName);
        }

        /// <summary>
        /// Tests that GetPurchaseType returns OkObjectResult with multiple items when database contains multiple records.
        /// Input: Database with multiple PurchaseType records.
        /// Expected: OkObjectResult with Items containing all elements and correct Count.
        /// </summary>
        [TestMethod]
        public async Task GetPurchaseType_MultipleRecords_ReturnsOkWithAllItems()
        {
            // Arrange
            var multipleItems = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = 1,
                    PurchaseTypeName = "Type1",
                    Description = "Description1"
                },
                new PurchaseType
                {
                    PurchaseTypeId = 2,
                    PurchaseTypeName = "Type2",
                    Description = "Description2"
                },
                new PurchaseType
                {
                    PurchaseTypeId = 3,
                    PurchaseTypeName = "Type3",
                    Description = "Description3"
                }
            };
            var mockDbSet = CreateMockDbSet(multipleItems);
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            var controller = new PurchaseTypeController(mockContext.Object);
            // Act
            var result = await controller.GetPurchaseType();
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
            var items = itemsProperty.GetValue(value) as List<PurchaseType>;
            var count = (int)countProperty.GetValue(value);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual(1, items[0].PurchaseTypeId);
            Assert.AreEqual(2, items[1].PurchaseTypeId);
            Assert.AreEqual(3, items[2].PurchaseTypeId);
        }

        /// <summary>
        /// Helper method to create a mock DbSet for testing.
        /// </summary>
        /// <param name = "data">The data to populate the mock DbSet.</param>
        /// <returns>A mock DbSet configured for async operations.</returns>
        private Mock<DbSet<PurchaseType>> CreateMockDbSet(List<PurchaseType> data)
        {
            var queryable = data.AsQueryable();
            var mockDbSet = new Mock<DbSet<PurchaseType>>();
            mockDbSet.As<IQueryable<PurchaseType>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<PurchaseType>(queryable.Provider));
            mockDbSet.As<IQueryable<PurchaseType>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<PurchaseType>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<PurchaseType>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockDbSet.As<IAsyncEnumerable<PurchaseType>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<PurchaseType>(queryable.GetEnumerator()));
            return mockDbSet;
        }

        /// <summary>
        /// Helper class to support async query operations in tests.
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
                var executeMethod = typeof(IQueryProvider).GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(System.Linq.Expressions.Expression) }).MakeGenericMethod(resultType);
                var result = executeMethod.Invoke(_inner, new object[] { expression });
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult)).MakeGenericMethod(resultType).Invoke(null, new[] { result });
            }
        }

        /// <summary>
        /// Helper class to support async enumerable operations in tests.
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
        /// Helper class to support async enumerator operations in tests.
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
        /// Tests that Insert method with valid payload adds the entity to the context,
        /// calls SaveChanges, and returns an OkObjectResult with the purchase type.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_AddsToContextSavesAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<PurchaseType>>();
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var purchaseType = new PurchaseType
            {
                PurchaseTypeId = 1,
                PurchaseTypeName = "Test Purchase Type",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<PurchaseType>
            {
                action = "insert",
                value = purchaseType
            };
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(purchaseType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(purchaseType, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles payload with null value by adding null to context
        /// and calling SaveChanges.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndCallsSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<PurchaseType>>();
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
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
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that Update method successfully updates a purchase type and returns OkObjectResult with the updated entity.
        /// Input: Valid payload containing a valid PurchaseType.
        /// Expected: Update and SaveChanges are called, and OkObjectResult containing the purchaseType is returned.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayload_ReturnsOkResultWithUpdatedEntity()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<PurchaseType>>();
            var purchaseType = new PurchaseType
            {
                PurchaseTypeId = 1,
                PurchaseTypeName = "Test Type",
                Description = "Test Description"
            };
            var payload = new CrudViewModel<PurchaseType>
            {
                value = purchaseType
            };
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<PurchaseType>()));
            var controller = new PurchaseTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(purchaseType, okResult.Value);
            mockDbSet.Verify(d => d.Update(purchaseType), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles null value in payload by passing null to Update method.
        /// Input: Payload with null value property.
        /// Expected: Update is called with null and SaveChanges is called.
        /// </summary>
        [TestMethod]
        public void Update_NullValue_PassesNullToUpdate()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<PurchaseType>>();
            var payload = new CrudViewModel<PurchaseType>
            {
                value = null
            };
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            mockDbSet.Setup(d => d.Update(It.IsAny<PurchaseType>()));
            var controller = new PurchaseTypeController(mockContext.Object);
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
        /// Tests that Update method correctly extracts and uses purchaseType from payload with different property values.
        /// Input: Payloads with various PurchaseType property values including edge cases.
        /// Expected: Correct value is extracted and returned in OkObjectResult.
        /// </summary>
        [TestMethod]
        [DataRow(0, "", "")]
        [DataRow(int.MaxValue, "Very Long Name That Exceeds Normal Length Expectations For Testing Purposes", "Very Long Description")]
        [DataRow(int.MinValue, "Special!@#$%^&*()Characters", "Description\nWith\nNewlines")]
        [DataRow(-1, " ", null)]
        public void Update_VariousPropertyValues_ReturnsOkResultWithCorrectValue(int id, string name, string description)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<PurchaseType>>();
            var purchaseType = new PurchaseType
            {
                PurchaseTypeId = id,
                PurchaseTypeName = name,
                Description = description
            };
            var payload = new CrudViewModel<PurchaseType>
            {
                value = purchaseType
            };
            mockContext.Setup(c => c.PurchaseType).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            mockDbSet.Setup(d => d.Update(It.IsAny<PurchaseType>()));
            var controller = new PurchaseTypeController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedType = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedType);
            Assert.AreEqual(id, returnedType.PurchaseTypeId);
            Assert.AreEqual(name, returnedType.PurchaseTypeName);
            Assert.AreEqual(description, returnedType.Description);
        }

        /// <summary>
        /// Tests that Remove successfully removes an existing entity and returns OkObjectResult with the entity.
        /// </summary>
        [TestMethod]
        public void Remove_ExistingEntity_RemovesEntityAndReturnsOk()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = 1,
                    PurchaseTypeName = "Type1",
                    Description = "Desc1"
                },
                new PurchaseType
                {
                    PurchaseTypeId = 2,
                    PurchaseTypeName = "Type2",
                    Description = "Desc2"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = 1
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEntity = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(1, returnedEntity.PurchaseTypeId);
            mockSet.Verify(m => m.Remove(It.IsAny<PurchaseType>()), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles non-existing entity by calling Remove with null and returning Ok with null.
        /// </summary>
        [TestMethod]
        public void Remove_NonExistingEntity_CallsRemoveWithNullAndReturnsOk()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = 1,
                    PurchaseTypeName = "Type1",
                    Description = "Desc1"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(0);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = 999
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles null key by converting it to 0 and searching for entity with Id 0.
        /// Convert.ToInt32(null) returns 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_SearchesForIdZero()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = 0,
                    PurchaseTypeName = "Type0",
                    Description = "Desc0"
                },
                new PurchaseType
                {
                    PurchaseTypeId = 1,
                    PurchaseTypeName = "Type1",
                    Description = "Desc1"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = null
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEntity = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(0, returnedEntity.PurchaseTypeId);
            mockSet.Verify(m => m.Remove(It.IsAny<PurchaseType>()), Times.Once());
        }

        /// <summary>
        /// Tests that Remove successfully converts string numeric key to int and removes entity.
        /// </summary>
        [TestMethod]
        public void Remove_StringNumericKey_ConvertsAndRemovesEntity()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = 42,
                    PurchaseTypeName = "Type42",
                    Description = "Desc42"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = "42"
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEntity = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(42, returnedEntity.PurchaseTypeId);
            mockSet.Verify(m => m.Remove(It.IsAny<PurchaseType>()), Times.Once());
        }

        /// <summary>
        /// Tests that Remove handles negative key values correctly.
        /// </summary>
        [TestMethod]
        public void Remove_NegativeKey_SearchesForNegativeId()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = -1,
                    PurchaseTypeName = "TypeNeg",
                    Description = "DescNeg"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = -1
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEntity = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(-1, returnedEntity.PurchaseTypeId);
        }

        /// <summary>
        /// Tests that Remove handles int.MaxValue key correctly.
        /// </summary>
        [TestMethod]
        public void Remove_MaxIntKey_SearchesForMaxInt()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = int.MaxValue,
                    PurchaseTypeName = "TypeMax",
                    Description = "DescMax"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = int.MaxValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEntity = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(int.MaxValue, returnedEntity.PurchaseTypeId);
        }

        /// <summary>
        /// Tests that Remove handles int.MinValue key correctly.
        /// </summary>
        [TestMethod]
        public void Remove_MinIntKey_SearchesForMinInt()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = int.MinValue,
                    PurchaseTypeName = "TypeMin",
                    Description = "DescMin"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = int.MinValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEntity = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(int.MinValue, returnedEntity.PurchaseTypeId);
        }

        /// <summary>
        /// Tests that Remove handles double key by converting to int (truncating decimal part).
        /// </summary>
        [TestMethod]
        public void Remove_DoubleKey_TruncatesAndRemovesEntity()
        {
            // Arrange
            var testData = new List<PurchaseType>
            {
                new PurchaseType
                {
                    PurchaseTypeId = 5,
                    PurchaseTypeName = "Type5",
                    Description = "Desc5"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(testData);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(m => m.PurchaseType).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);
            var controller = new PurchaseTypeController(mockContext.Object);
            var payload = new CrudViewModel<PurchaseType>
            {
                key = 5.9
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            var returnedEntity = okResult.Value as PurchaseType;
            Assert.IsNotNull(returnedEntity);
            Assert.AreEqual(5, returnedEntity.PurchaseTypeId);
        }

        /// <summary>
        /// Helper method to create a mock DbSet with IQueryable support for LINQ operations.
        /// </summary>
        private static Mock<DbSet<PurchaseType>> CreateMockDbSet(IQueryable<PurchaseType> data)
        {
            var mockSet = new Mock<DbSet<PurchaseType>>();
            mockSet.As<IQueryable<PurchaseType>>().Setup(m => m.Provider).Returns(data.Provider);
            mockSet.As<IQueryable<PurchaseType>>().Setup(m => m.Expression).Returns(data.Expression);
            mockSet.As<IQueryable<PurchaseType>>().Setup(m => m.ElementType).Returns(data.ElementType);
            mockSet.As<IQueryable<PurchaseType>>().Setup(m => m.GetEnumerator()).Returns(data.GetEnumerator());
            return mockSet;
        }
    }
}