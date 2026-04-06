using System.Collections.Generic;
using System.Linq;

using coderush.Controllers.Api.UnitTests;
using coderush.Data;
using coderush.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the NumberSequence service class.
    /// </summary>
    [TestClass]
    public class NumberSequenceServiceTests
    {
        private static ApplicationDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(options);
        }

        /// <summary>
        /// Tests that GetNumberSequence creates a new sequence record when the module does not exist.
        /// </summary>
        [TestMethod]
        public void GetNumberSequence_ModuleDoesNotExist_CreatesNewSequenceAndReturnsFirst()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NumberSequence(context);

            // Act
            var result = service.GetNumberSequence("SO");

            // Assert
            Assert.AreEqual("SO00001", result);
            Assert.AreEqual(1, context.NumberSequence.Count());
            var stored = context.NumberSequence.First();
            Assert.AreEqual("SO", stored.Module);
            Assert.AreEqual("SO", stored.Prefix);
            Assert.AreEqual("SO", stored.NumberSequenceName);
            Assert.AreEqual(1, stored.LastNumber);
        }

        /// <summary>
        /// Tests that GetNumberSequence increments an existing sequence.
        /// </summary>
        [TestMethod]
        public void GetNumberSequence_ModuleExists_IncrementsAndReturnsNext()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.NumberSequence.Add(new Models.NumberSequence
            {
                Module = "INV",
                Prefix = "INV",
                NumberSequenceName = "INV",
                LastNumber = 5
            });
            context.SaveChanges();

            var service = new NumberSequence(context);

            // Act
            var result = service.GetNumberSequence("INV");

            // Assert
            Assert.AreEqual("INV00006", result);
            var stored = context.NumberSequence.First(x => x.Module == "INV");
            Assert.AreEqual(6, stored.LastNumber);
        }

        /// <summary>
        /// Tests that calling GetNumberSequence multiple times increments sequentially.
        /// </summary>
        [TestMethod]
        public void GetNumberSequence_CalledMultipleTimes_IncrementsSequentially()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NumberSequence(context);

            // Act
            var result1 = service.GetNumberSequence("PO");
            var result2 = service.GetNumberSequence("PO");
            var result3 = service.GetNumberSequence("PO");

            // Assert
            Assert.AreEqual("PO00001", result1);
            Assert.AreEqual("PO00002", result2);
            Assert.AreEqual("PO00003", result3);
        }

        /// <summary>
        /// Tests that GetNumberSequence pads the number to 5 digits.
        /// </summary>
        [TestMethod]
        public void GetNumberSequence_NumberPadding_PadsToFiveDigits()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.NumberSequence.Add(new Models.NumberSequence
            {
                Module = "SHP",
                Prefix = "SHP",
                NumberSequenceName = "SHP",
                LastNumber = 99
            });
            context.SaveChanges();

            var service = new NumberSequence(context);

            // Act
            var result = service.GetNumberSequence("SHP");

            // Assert
            Assert.AreEqual("SHP00100", result);
        }

        /// <summary>
        /// Tests that GetNumberSequence handles large numbers correctly.
        /// </summary>
        [TestMethod]
        public void GetNumberSequence_LargeNumber_FormatsCorrectly()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.NumberSequence.Add(new Models.NumberSequence
            {
                Module = "BIG",
                Prefix = "BIG",
                NumberSequenceName = "BIG",
                LastNumber = 99999
            });
            context.SaveChanges();

            var service = new NumberSequence(context);

            // Act
            var result = service.GetNumberSequence("BIG");

            // Assert
            Assert.AreEqual("BIG100000", result);
        }

        /// <summary>
        /// Tests that GetNumberSequence handles different modules independently.
        /// </summary>
        [TestMethod]
        public void GetNumberSequence_DifferentModules_MaintainSeparateCounters()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            var service = new NumberSequence(context);

            // Act
            var so1 = service.GetNumberSequence("SO");
            var po1 = service.GetNumberSequence("PO");
            var so2 = service.GetNumberSequence("SO");

            // Assert
            Assert.AreEqual("SO00001", so1);
            Assert.AreEqual("PO00001", po1);
            Assert.AreEqual("SO00002", so2);
            Assert.AreEqual(2, context.NumberSequence.Count());
        }

        /// <summary>
        /// Tests that GetNumberSequence uses the prefix from the stored record.
        /// </summary>
        [TestMethod]
        public void GetNumberSequence_ExistingRecordWithCustomPrefix_UsesStoredPrefix()
        {
            // Arrange
            using var context = CreateInMemoryContext();
            context.NumberSequence.Add(new Models.NumberSequence
            {
                Module = "GRN",
                Prefix = "GRN",
                NumberSequenceName = "GRN",
                LastNumber = 0
            });
            context.SaveChanges();

            var service = new NumberSequence(context);

            // Act
            var result = service.GetNumberSequence("GRN");

            // Assert
            Assert.AreEqual("GRN00001", result);
        }
    }
}
