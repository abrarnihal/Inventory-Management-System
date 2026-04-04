using System;

using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Migrations.UnitTests
{
    /// <summary>
    /// Unit tests for the AddChatLog migration class.
    /// </summary>
    [TestClass]
    public partial class AddChatLogTests
    {
        /// <summary>
        /// Tests that the Down method executes without throwing exceptions when called with a valid MigrationBuilder.
        /// Note: Full verification of table drop operations cannot be performed in unit tests because MigrationBuilder
        /// is a concrete class with non-virtual methods that cannot be mocked with Moq. This test verifies the method
        /// can execute without exceptions. Full migration testing should be done through integration tests.
        /// Input: Valid MigrationBuilder instance.
        /// Expected: Method executes without exceptions.
        /// </summary>
        [TestMethod]
        public void Down_ValidMigrationBuilder_ExecutesWithoutException()
        {
            // Arrange
            var migration = new TestableAddChatLog();
            var activeProvider = "Microsoft.EntityFrameworkCore.SqlServer";
            var migrationBuilder = new MigrationBuilder(activeProvider);

            // Act & Assert
            // Note: We cannot verify the specific DropTable calls because MigrationBuilder methods are not virtual.
            // This test only verifies that the method executes without throwing exceptions.
            migration.InvokeDown(migrationBuilder);

            // Verify that operations were added to the builder
            Assert.IsNotNull(migrationBuilder.Operations);
            Assert.AreEqual(2, migrationBuilder.Operations.Count, "Expected two drop table operations to be added.");
        }

        /// <summary>
        /// Helper class to expose the protected Down method for testing.
        /// This is necessary because the Down method is protected and cannot be directly invoked from tests.
        /// </summary>
        private class TestableAddChatLog : AddChatLog
        {
            public void InvokeDown(MigrationBuilder migrationBuilder)
            {
                Down(migrationBuilder);
            }
        }

    }
}