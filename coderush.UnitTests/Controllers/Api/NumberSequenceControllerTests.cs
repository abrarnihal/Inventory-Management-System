using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for the NumberSequenceController class.
    /// </summary>
    [TestClass]
    public class NumberSequenceControllerTests
    {
        /// <summary>
        /// Tests that PostNumberSequence returns OkObjectResult with the number sequence
        /// when ModelState is valid and a valid NumberSequence is provided.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_ValidModelState_ReturnsOkObjectResultWithNumberSequence()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test Sequence",
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = 100
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(numberSequence, okResult.Value);
        }

        /// <summary>
        /// Tests that PostNumberSequence returns BadRequestObjectResult with ModelState
        /// when ModelState is invalid.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_InvalidModelState_ReturnsBadRequestWithModelState()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var controller = new NumberSequenceController(mockContext.Object);
            controller.ModelState.AddModelError("NumberSequenceName", "The NumberSequenceName field is required.");
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = 100
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(Microsoft.AspNetCore.Mvc.SerializableError));
        }

        /// <summary>
        /// Tests that PostNumberSequence calls Add on the DbSet
        /// when a valid NumberSequence is provided.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_ValidModelState_CallsAddOnDbSet()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test Sequence",
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = 100
            };
            // Act
            await controller.PostNumberSequence(numberSequence);
            // Assert
            mockDbSet.Verify(d => d.Add(numberSequence), Times.Once);
        }

        /// <summary>
        /// Tests that PostNumberSequence calls SaveChangesAsync on the context
        /// when a valid NumberSequence is provided.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_ValidModelState_CallsSaveChangesAsync()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test Sequence",
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = 100
            };
            // Act
            await controller.PostNumberSequence(numberSequence);
            // Assert
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that PostNumberSequence does not call Add or SaveChangesAsync
        /// when ModelState is invalid.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_InvalidModelState_DoesNotCallAddOrSaveChanges()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            var controller = new NumberSequenceController(mockContext.Object);
            controller.ModelState.AddModelError("NumberSequenceName", "The NumberSequenceName field is required.");
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = 100
            };
            // Act
            await controller.PostNumberSequence(numberSequence);
            // Assert
            mockDbSet.Verify(d => d.Add(It.IsAny<NumberSequence>()), Times.Never);
            mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that PostNumberSequence handles NumberSequence with minimum integer values correctly.
        /// Verifies edge case handling for NumberSequenceId and LastNumber set to int.MinValue.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_NumberSequenceWithMinIntValues_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = int.MinValue,
                NumberSequenceName = "Test Sequence",
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = int.MinValue
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostNumberSequence handles NumberSequence with maximum integer values correctly.
        /// Verifies edge case handling for NumberSequenceId and LastNumber set to int.MaxValue.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_NumberSequenceWithMaxIntValues_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = int.MaxValue,
                NumberSequenceName = "Test Sequence",
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = int.MaxValue
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostNumberSequence handles NumberSequence with zero values correctly.
        /// Verifies edge case handling for NumberSequenceId and LastNumber set to zero.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_NumberSequenceWithZeroValues_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 0,
                NumberSequenceName = "Test Sequence",
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = 0
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostNumberSequence handles NumberSequence with very long string values.
        /// Verifies handling of edge case where string properties contain lengthy text.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_NumberSequenceWithVeryLongStrings_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var longString = new string ('A', 10000);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = longString,
                Module = longString,
                Prefix = longString,
                LastNumber = 100
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostNumberSequence handles NumberSequence with special characters in strings.
        /// Verifies handling of edge case where string properties contain special and control characters.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_NumberSequenceWithSpecialCharacters_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var specialString = "!@#$%^&*()_+-=[]{}|;':\"<>,.?/\\~`\t\n\r";
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = specialString,
                Module = specialString,
                Prefix = specialString,
                LastNumber = 100
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostNumberSequence handles negative values for LastNumber correctly.
        /// Verifies edge case handling for negative integer values.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_NumberSequenceWithNegativeLastNumber_ReturnsOkObjectResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockDbSet = new Mock<DbSet<NumberSequence>>();
            mockContext.Setup(c => c.NumberSequence).Returns(mockDbSet.Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test Sequence",
                Module = "TestModule",
                Prefix = "TEST",
                LastNumber = -999
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostNumberSequence returns BadRequest when multiple validation errors exist.
        /// Verifies proper handling of multiple ModelState errors.
        /// </summary>
        [TestMethod]
        public async Task PostNumberSequence_MultipleModelStateErrors_ReturnsBadRequest()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var controller = new NumberSequenceController(mockContext.Object);
            controller.ModelState.AddModelError("NumberSequenceName", "The NumberSequenceName field is required.");
            controller.ModelState.AddModelError("Module", "The Module field is required.");
            controller.ModelState.AddModelError("Prefix", "The Prefix field is required.");
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                LastNumber = 100
            };
            // Act
            var result = await controller.PostNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            var modelState = badRequestResult.Value as Microsoft.AspNetCore.Mvc.SerializableError;
            Assert.IsNotNull(modelState);
            Assert.IsTrue(modelState.Count >= 3);
        }

        /// <summary>
        /// Tests that PutNumberSequence returns BadRequest when ModelState is invalid.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_InvalidModelState_ReturnsBadRequest()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test",
                Module = "TestModule",
                Prefix = "T",
                LastNumber = 100
            };
            controller.ModelState.AddModelError("TestKey", "Test error");
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(Microsoft.AspNetCore.Mvc.SerializableError));
        }

        /// <summary>
        /// Tests that PutNumberSequence marks entity as Modified, saves changes, and returns Ok when ModelState is valid.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_ValidModelState_MarksEntityAsModifiedAndReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_ValidModelState_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test",
                Module = "TestModule",
                Prefix = "T",
                LastNumber = 100
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with minimum integer values correctly.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithMinimumIntegerValues_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_MinIntValues_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceName = "Test",
                Module = "TestModule",
                Prefix = "T",
                LastNumber = int.MinValue
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with maximum integer values correctly.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithMaximumIntegerValues_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_MaxIntValues_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = int.MaxValue,
                NumberSequenceName = "Test",
                Module = "TestModule",
                Prefix = "T",
                LastNumber = int.MaxValue
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with empty strings correctly when ModelState is valid.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithEmptyStrings_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_EmptyStrings_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = string.Empty,
                Module = string.Empty,
                Prefix = string.Empty,
                LastNumber = 0
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with very long strings correctly.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithVeryLongStrings_ReturnsOk()
        {
            // Arrange
            var longString = new string('A', 10000);
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_LongStrings_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = longString,
                Module = longString,
                Prefix = longString,
                LastNumber = 100
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with special characters in strings correctly.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithSpecialCharacters_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_SpecialChars_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test!@#$%^&*()",
                Module = "<script>alert('xss')</script>",
                Prefix = "🎉✨🔥",
                LastNumber = 100
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with zero values correctly.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithZeroValues_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_ZeroValues_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceName = "Test",
                Module = "TestModule",
                Prefix = "T",
                LastNumber = 0
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with negative LastNumber correctly.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithNegativeLastNumber_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_NegativeLastNumber_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceName = "Test",
                Module = "TestModule",
                Prefix = "T",
                LastNumber = -100
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence handles entity with whitespace-only strings correctly.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_EntityWithWhitespaceStrings_ReturnsOk()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "PutNumberSequence_WhitespaceStrings_" + Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(options);
            var numberSequence = new NumberSequence
            {
                NumberSequenceName = "   ",
                Module = "\t\t",
                Prefix = "\n\n",
                LastNumber = 100
            };
            context.NumberSequence.Add(numberSequence);
            await context.SaveChangesAsync();
            context.Entry(numberSequence).State = EntityState.Detached;
            var controller = new NumberSequenceController(context);
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkResult));
            Assert.AreEqual(EntityState.Unchanged, context.Entry(numberSequence).State);
        }

        /// <summary>
        /// Tests that PutNumberSequence returns BadRequest when ModelState has multiple errors.
        /// </summary>
        [TestMethod]
        public async Task PutNumberSequence_ModelStateWithMultipleErrors_ReturnsBadRequest()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var controller = new NumberSequenceController(mockContext.Object);
            var numberSequence = new NumberSequence
            {
                NumberSequenceId = 1,
                NumberSequenceName = "Test",
                Module = "TestModule",
                Prefix = "T",
                LastNumber = 100
            };
            controller.ModelState.AddModelError("NumberSequenceName", "Name is required");
            controller.ModelState.AddModelError("Module", "Module is required");
            controller.ModelState.AddModelError("Prefix", "Prefix is required");
            // Act
            var result = await controller.PutNumberSequence(numberSequence);
            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = result as BadRequestObjectResult;
            Assert.IsNotNull(badRequestResult);
            Assert.IsInstanceOfType(badRequestResult.Value, typeof(Microsoft.AspNetCore.Mvc.SerializableError));
        }
    }
}