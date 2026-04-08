using System;
using coderush.Data;
using coderush.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace coderush.Data.UnitTests
{
    /// <summary>
    /// Unit tests for the ApplicationDbContext class.
    /// </summary>
    [TestClass]
    public class ApplicationDbContextTests
    {
        /// <summary>
        /// Tests that OnModelCreating executes successfully with a valid ModelBuilder,
        /// properly delegating to the base IdentityDbContext implementation.
        /// </summary>
        [TestMethod]
        public void OnModelCreating_ValidModelBuilder_ExecutesWithoutException()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new TestableApplicationDbContext(options);
            var modelBuilder = new ModelBuilder();

            // Act & Assert
            context.TestOnModelCreating(modelBuilder);
            // If no exception is thrown, the test passes
        }

        /// <summary>
        /// Tests that the ApplicationDbContext can be instantiated and that model creation
        /// is triggered successfully during context initialization.
        /// </summary>
        [TestMethod]
        public void OnModelCreating_ContextInitialization_CompletesSuccessfully()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            // Act
            using var context = new ApplicationDbContext(options);
            var model = context.Model;

            // Assert
            Assert.IsNotNull(model);
        }

        /// <summary>
        /// Helper class that exposes the protected OnModelCreating method for testing purposes.
        /// </summary>
        private class TestableApplicationDbContext : ApplicationDbContext
        {
            public TestableApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public void TestOnModelCreating(ModelBuilder builder)
            {
                OnModelCreating(builder);
            }
        }
    }
}