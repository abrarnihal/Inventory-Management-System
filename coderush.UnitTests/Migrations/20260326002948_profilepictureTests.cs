using System;

using coderush.Migrations;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Migrations.UnitTests
{
    /// <summary>
    /// Unit tests for the ProfilePicture migration class.
    /// </summary>
    [TestClass]
    public class ProfilePictureTests
    {
        /// <summary>
        /// Tests that the Up method calls AddColumn with correct parameters when given a valid MigrationBuilder.
        /// </summary>
        [TestMethod]
        public void Up_ValidMigrationBuilder_CallsAddColumnWithCorrectParameters()
        {
            // Arrange
            var mockMigrationBuilder = new Mock<MigrationBuilder>(MockBehavior.Strict, "SqlServer");
            mockMigrationBuilder
                .Setup(m => m.AddColumn<string>(
                    "ProfilePicture",
                    "UserProfile",
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int?>(),
                    false,
                    It.IsAny<string?>(),
                    true,
                    It.IsAny<object?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool?>()))
                .Returns(new Mock<OperationBuilder<Microsoft.EntityFrameworkCore.Migrations.Operations.AddColumnOperation>>(new Microsoft.EntityFrameworkCore.Migrations.Operations.AddColumnOperation()).Object);

            var migration = new TestableProfilePicture();

            // Act
            migration.TestUp(mockMigrationBuilder.Object);

            // Assert
            mockMigrationBuilder.Verify(
                m => m.AddColumn<string>(
                    "ProfilePicture",
                    "UserProfile",
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<int?>(),
                    false,
                    It.IsAny<string?>(),
                    true,
                    It.IsAny<object?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<bool?>(),
                    It.IsAny<string?>(),
                    It.IsAny<string?>(),
                    It.IsAny<int?>(),
                    It.IsAny<int?>(),
                    It.IsAny<bool?>()),
                Times.Once);
        }

        /// <summary>
        /// Helper class to expose the protected Up method for testing.
        /// </summary>
        private class TestableProfilePicture : ProfilePicture
        {
            public void TestUp(MigrationBuilder migrationBuilder)
            {
                Up(migrationBuilder);
            }
        }

    }
}