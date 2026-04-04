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
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the BranchController class.
    /// </summary>
    [TestClass]
    public class BranchControllerTests
    {
        /// <summary>
        /// Tests that Remove successfully removes a branch and returns OkObjectResult when branch exists.
        /// </summary>
        [TestMethod]
        public void Remove_ExistingBranch_RemovesAndReturnsOk()
        {
            // Arrange
            var targetBranch = new Branch
            {
                BranchId = 5,
                BranchName = "Test Branch"
            };
            var branches = new List<Branch>
            {
                new Branch
                {
                    BranchId = 1,
                    BranchName = "Branch 1"
                },
                targetBranch,
                new Branch
                {
                    BranchId = 10,
                    BranchName = "Branch 10"
                }
            }.AsQueryable();
            var mockSet = CreateMockDbSet(branches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            var payload = new CrudViewModel<Branch>
            {
                key = 5
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(targetBranch, okResult.Value);
            mockSet.Verify(m => m.Remove(It.Is<Branch>(b => b.BranchId == 5)), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles boundary values correctly.
        /// </summary>
        /// <param name = "keyValue">The boundary key value to test.</param>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(-1)]
        [DataRow(int.MaxValue)]
        [DataRow(int.MinValue)]
        public void Remove_BoundaryValues_HandlesCorrectly(int keyValue)
        {
            // Arrange
            var targetBranch = new Branch
            {
                BranchId = keyValue,
                BranchName = "Test Branch"
            };
            var branches = new List<Branch>
            {
                targetBranch
            }.AsQueryable();
            var mockSet = CreateMockDbSet(branches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            var payload = new CrudViewModel<Branch>
            {
                key = keyValue
            };
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(targetBranch, okResult.Value);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles double values by converting them to integers (truncating decimals).
        /// </summary>
        [TestMethod]
        public void Remove_DoubleKey_TruncatesToInteger()
        {
            // Arrange
            var targetBranch = new Branch
            {
                BranchId = 42,
                BranchName = "Test Branch"
            };
            var branches = new List<Branch>
            {
                targetBranch
            }.AsQueryable();
            var mockSet = CreateMockDbSet(branches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            var payload = new CrudViewModel<Branch>
            {
                key = 42.9
            }; // Should truncate to 42
            // Act
            var result = controller.Remove(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(targetBranch, okResult.Value);
            mockSet.Verify(m => m.Remove(It.Is<Branch>(b => b.BranchId == 42)), Times.Once);
        }

        /// <summary>
        /// Creates a mock DbSet with queryable support for testing.
        /// </summary>
        /// <param name = "sourceList">The source list to create the mock from.</param>
        /// <returns>A mock DbSet with LINQ support.</returns>
        private static Mock<DbSet<Branch>> CreateMockDbSet(IQueryable<Branch> sourceList)
        {
            var mockSet = new Mock<DbSet<Branch>>();
            mockSet.As<IQueryable<Branch>>().Setup(m => m.Provider).Returns(sourceList.Provider);
            mockSet.As<IQueryable<Branch>>().Setup(m => m.Expression).Returns(sourceList.Expression);
            mockSet.As<IQueryable<Branch>>().Setup(m => m.ElementType).Returns(sourceList.ElementType);
            mockSet.As<IQueryable<Branch>>().Setup(m => m.GetEnumerator()).Returns(sourceList.GetEnumerator());
            return mockSet;
        }

        /// <summary>
        /// Tests that Update successfully updates a branch and returns OK result with the branch.
        /// Input: Valid payload with a valid Branch object.
        /// Expected: Update is called on DbSet, SaveChanges is called, and OkObjectResult is returned with the branch.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayloadWithValidBranch_ReturnsOkResultWithBranch()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = 1,
                BranchName = "Main Branch",
                Description = "Main office branch",
                CurrencyId = 1,
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Phone = "555-1234",
                Email = "main@branch.com",
                ContactPerson = "John Doe"
            };
            var payload = new CrudViewModel<Branch>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<Branch>()));
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(branch, okResult.Value);
            mockDbSet.Verify(d => d.Update(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles null branch value in payload.
        /// Input: Payload with null value property.
        /// Expected: Update is called with null, SaveChanges is called, and OkObjectResult is returned with null.
        /// </summary>
        [TestMethod]
        public void Update_PayloadWithNullValue_CallsUpdateWithNull()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Branch>>();
            var payload = new CrudViewModel<Branch>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = null
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<Branch>()));
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.IsNull(okResult.Value);
            mockDbSet.Verify(d => d.Update(null), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles branch with minimum valid data.
        /// Input: Payload with branch containing only required fields.
        /// Expected: Update is called, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_BranchWithMinimalData_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = 0,
                BranchName = "B"
            };
            var payload = new CrudViewModel<Branch>
            {
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<Branch>()));
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles branch with boundary values for numeric fields.
        /// Input: Payload with branch containing int.MaxValue and int.MinValue for numeric fields.
        /// Expected: Update is called, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_BranchWithBoundaryNumericValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = int.MaxValue,
                BranchName = "Branch",
                CurrencyId = int.MinValue
            };
            var payload = new CrudViewModel<Branch>
            {
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<Branch>()));
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(branch, okResult.Value);
            mockDbSet.Verify(d => d.Update(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update handles branch with special characters in string fields.
        /// Input: Payload with branch containing special characters, empty strings, and very long strings.
        /// Expected: Update is called, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Update_BranchWithSpecialCharactersInStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>();
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = 1,
                BranchName = "Branch<>\"'&\n\r\t",
                Description = "",
                Address = "   ",
                City = new string ('A', 10000),
                State = "!@#$%^&*()",
                ZipCode = null,
                Phone = "\u0000\u0001\u0002",
                Email = "test@example.com",
                ContactPerson = null
            };
            var payload = new CrudViewModel<Branch>
            {
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockDbSet.Setup(d => d.Update(It.IsAny<Branch>()));
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Update(payload);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockDbSet.Verify(d => d.Update(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method with a valid payload successfully adds the branch to context,
        /// saves changes, and returns an Ok result with the branch.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_AddsToContextSavesAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = 1,
                BranchName = "Main Branch",
                Description = "Main office branch",
                CurrencyId = 1,
                Address = "123 Main St",
                City = "New York",
                State = "NY",
                ZipCode = "10001",
                Phone = "123-456-7890",
                Email = "main@example.com",
                ContactPerson = "John Doe"
            };
            var payload = new CrudViewModel<Branch>
            {
                action = "insert",
                key = null,
                antiForgery = "token",
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(branch, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles a payload with null value property by adding null to context.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullBranchAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Branch>>();
            var payload = new CrudViewModel<Branch>
            {
                action = "insert",
                key = null,
                antiForgery = "token",
                value = null!
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(null!), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles branch with empty string properties correctly.
        /// </summary>
        [TestMethod]
        public void Insert_BranchWithEmptyStrings_AddsToContextAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = 0,
                BranchName = "",
                Description = "",
                CurrencyId = 0,
                Address = "",
                City = "",
                State = "",
                ZipCode = "",
                Phone = "",
                Email = "",
                ContactPerson = ""
            };
            var payload = new CrudViewModel<Branch>
            {
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles branch with special characters in string properties.
        /// </summary>
        [TestMethod]
        public void Insert_BranchWithSpecialCharacters_AddsToContextAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = 1,
                BranchName = "Test <Branch> & \"Special\"",
                Description = "Line1\nLine2\tTabbed",
                Address = "123 Main St. #456",
                Email = "test+tag@example.com",
                ContactPerson = "O'Brien"
            };
            var payload = new CrudViewModel<Branch>
            {
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(branch, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles branch with negative and extreme numeric values.
        /// </summary>
        [TestMethod]
        [DataRow(int.MinValue)]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MaxValue)]
        public void Insert_BranchWithVariousNumericValues_AddsToContextAndReturnsOk(int id)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Branch>>();
            var branch = new Branch
            {
                BranchId = id,
                BranchName = "Test Branch",
                CurrencyId = id
            };
            var payload = new CrudViewModel<Branch>
            {
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles branch with very long string values.
        /// </summary>
        [TestMethod]
        public void Insert_BranchWithVeryLongStrings_AddsToContextAndReturnsOk()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<Branch>>();
            var longString = new string ('A', 10000);
            var branch = new Branch
            {
                BranchId = 1,
                BranchName = longString,
                Description = longString,
                Address = longString
            };
            var payload = new CrudViewModel<Branch>
            {
                value = branch
            };
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = controller.Insert(payload);
            // Assert
            mockDbSet.Verify(d => d.Add(branch), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that GetBranch returns OkObjectResult with empty list when database contains no branches.
        /// </summary>
        [TestMethod]
        public async Task GetBranch_EmptyDatabase_ReturnsOkWithEmptyList()
        {
            // Arrange
            var emptyBranches = new List<Branch>();
            var mockDbSet = CreateMockDbSet(emptyBranches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = await controller.GetBranch();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultValue = okResult.Value;
            var itemsProperty = resultValue.GetType().GetProperty("Items");
            var countProperty = resultValue.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);
            var items = (List<Branch>)itemsProperty.GetValue(resultValue);
            var count = (int)countProperty.GetValue(resultValue);
            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetBranch returns OkObjectResult with single branch when database contains one branch.
        /// </summary>
        [TestMethod]
        public async Task GetBranch_SingleBranch_ReturnsOkWithOneItem()
        {
            // Arrange
            var branches = new List<Branch>
            {
                new Branch
                {
                    BranchId = 1,
                    BranchName = "Main Branch",
                    Description = "Main office",
                    Address = "123 Main St",
                    City = "New York",
                    State = "NY",
                    ZipCode = "10001",
                    Phone = "555-0100",
                    Email = "main@example.com",
                    ContactPerson = "John Doe",
                    CurrencyId = 1
                }
            };
            var mockDbSet = CreateMockDbSet(branches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = await controller.GetBranch();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultValue = okResult.Value;
            var itemsProperty = resultValue.GetType().GetProperty("Items");
            var countProperty = resultValue.GetType().GetProperty("Count");
            var items = (List<Branch>)itemsProperty.GetValue(resultValue);
            var count = (int)countProperty.GetValue(resultValue);
            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("Main Branch", items[0].BranchName);
            Assert.AreEqual(1, items[0].BranchId);
        }

        /// <summary>
        /// Tests that GetBranch returns OkObjectResult with all branches when database contains multiple branches.
        /// </summary>
        [TestMethod]
        public async Task GetBranch_MultipleBranches_ReturnsOkWithAllItems()
        {
            // Arrange
            var branches = new List<Branch>
            {
                new Branch
                {
                    BranchId = 1,
                    BranchName = "Main Branch",
                    Description = "Main office",
                    CurrencyId = 1
                },
                new Branch
                {
                    BranchId = 2,
                    BranchName = "East Branch",
                    Description = "East office",
                    CurrencyId = 1
                },
                new Branch
                {
                    BranchId = 3,
                    BranchName = "West Branch",
                    Description = "West office",
                    CurrencyId = 2
                }
            };
            var mockDbSet = CreateMockDbSet(branches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = await controller.GetBranch();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNotNull(okResult.Value);
            var resultValue = okResult.Value;
            var itemsProperty = resultValue.GetType().GetProperty("Items");
            var countProperty = resultValue.GetType().GetProperty("Count");
            var items = (List<Branch>)itemsProperty.GetValue(resultValue);
            var count = (int)countProperty.GetValue(resultValue);
            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual("Main Branch", items[0].BranchName);
            Assert.AreEqual("East Branch", items[1].BranchName);
            Assert.AreEqual("West Branch", items[2].BranchName);
        }

        /// <summary>
        /// Tests that GetBranch returns correct count matching the number of items.
        /// </summary>
        [TestMethod]
        public async Task GetBranch_VariousCounts_CountMatchesItemsCount()
        {
            // Arrange
            var branches = new List<Branch>
            {
                new Branch
                {
                    BranchId = 1,
                    BranchName = "Branch1",
                    CurrencyId = 1
                },
                new Branch
                {
                    BranchId = 2,
                    BranchName = "Branch2",
                    CurrencyId = 1
                },
                new Branch
                {
                    BranchId = 3,
                    BranchName = "Branch3",
                    CurrencyId = 1
                },
                new Branch
                {
                    BranchId = 4,
                    BranchName = "Branch4",
                    CurrencyId = 1
                },
                new Branch
                {
                    BranchId = 5,
                    BranchName = "Branch5",
                    CurrencyId = 1
                }
            };
            var mockDbSet = CreateMockDbSet(branches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = await controller.GetBranch();
            // Assert
            var okResult = (OkObjectResult)result;
            var resultValue = okResult.Value;
            var itemsProperty = resultValue.GetType().GetProperty("Items");
            var countProperty = resultValue.GetType().GetProperty("Count");
            var items = (List<Branch>)itemsProperty.GetValue(resultValue);
            var count = (int)countProperty.GetValue(resultValue);
            Assert.AreEqual(items.Count, count);
            Assert.AreEqual(5, count);
        }

        /// <summary>
        /// Tests that GetBranch returns OkObjectResult with status code 200.
        /// </summary>
        [TestMethod]
        public async Task GetBranch_ValidCall_ReturnsOkObjectResult()
        {
            // Arrange
            var branches = new List<Branch>
            {
                new Branch
                {
                    BranchId = 1,
                    BranchName = "Test Branch",
                    CurrencyId = 1
                }
            };
            var mockDbSet = CreateMockDbSet(branches);
            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Branch).Returns(mockDbSet.Object);
            var controller = new BranchController(mockContext.Object);
            // Act
            var result = await controller.GetBranch();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(200, okResult.StatusCode);
        }

        /// <summary>
        /// Helper method to create a mock DbSet with async query support.
        /// </summary>
        /// <param name = "sourceList">The source data for the mock DbSet.</param>
        /// <returns>A mock DbSet configured for async operations.</returns>
        private static Mock<DbSet<Branch>> CreateMockDbSet(List<Branch> sourceList)
        {
            var queryable = sourceList.AsQueryable();
            var mockDbSet = new Mock<DbSet<Branch>>();
            mockDbSet.As<IAsyncEnumerable<Branch>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Branch>(queryable.GetEnumerator()));
            mockDbSet.As<IQueryable<Branch>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Branch>(queryable.Provider));
            mockDbSet.As<IQueryable<Branch>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<Branch>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<Branch>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            return mockDbSet;
        }

        /// <summary>
        /// Helper class to support async enumeration for testing.
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
        /// Helper class to support async query provider for testing.
        /// </summary>
        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            public TestAsyncQueryProvider(IQueryProvider inner)
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
                return _inner.Execute(expression);
            }

            public TResult Execute<TResult>(Expression expression)
            {
                return _inner.Execute<TResult>(expression);
            }

            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var executionResult = typeof(IQueryProvider).GetMethod(name: nameof(IQueryProvider.Execute), genericParameterCount: 1, types: new[] { typeof(Expression) }).MakeGenericMethod(resultType).Invoke(this, new[] { expression });
                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))?.MakeGenericMethod(resultType).Invoke(null, new[] { executionResult });
            }
        }

        /// <summary>
        /// Helper class to support async enumerable for testing.
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
    }
}