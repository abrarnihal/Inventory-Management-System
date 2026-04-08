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
    /// Unit tests for ProductController class.
    /// </summary>
    [TestClass]
    public class ProductControllerTests
    {
        /// <summary>
        /// Tests that Remove successfully removes an existing product and returns OkObjectResult with the removed product.
        /// Input: Valid payload with key matching an existing product.
        /// Expected: Product is removed, SaveChanges is called, and OkObjectResult is returned with the product.
        /// </summary>
        [TestMethod]
        public void Remove_ValidKeyAndProductExists_ReturnsOkWithRemovedProduct()
        {
            // Arrange
            var productId = 42;
            var product = new Product { ProductId = productId, ProductName = "Test Product" };
            var products = new List<Product> { product }.AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = productId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product, okResult.Value);
            mockSet.Verify(m => m.Remove(product), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles various key formats that can be converted to integers.
        /// Input: Different key types (string, int, decimal) that convert to valid ProductIds.
        /// Expected: Product is found and removed successfully for all convertible key types.
        /// </summary>
        [TestMethod]
        [DataRow(10, DisplayName = "Integer key")]
        [DataRow("20", DisplayName = "String numeric key")]
        [DataRow(30.0, DisplayName = "Double key")]
        public void Remove_DifferentKeyTypes_ConvertsAndRemovesProduct(object keyValue)
        {
            // Arrange
            var expectedId = Convert.ToInt32(keyValue);
            var product = new Product { ProductId = expectedId, ProductName = "Test Product" };
            var products = new List<Product> { product }.AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = keyValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(product), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles boundary integer values correctly.
        /// Input: Keys with int.MaxValue, int.MinValue, and 0.
        /// Expected: Product is searched with the boundary value and removed if found.
        /// </summary>
        [TestMethod]
        [DataRow(int.MaxValue, DisplayName = "Maximum integer value")]
        [DataRow(int.MinValue, DisplayName = "Minimum integer value")]
        [DataRow(0, DisplayName = "Zero value")]
        [DataRow(-1, DisplayName = "Negative value")]
        public void Remove_BoundaryKeyValues_ProcessesCorrectly(int keyValue)
        {
            // Arrange
            var product = new Product { ProductId = keyValue, ProductName = "Boundary Test" };
            var products = new List<Product> { product }.AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = keyValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(product), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles the case when the product is not found.
        /// Input: Payload with a key that doesn't match any existing product.
        /// Expected: FirstOrDefault returns null, Remove is called with null, SaveChanges is called, returns Ok with null.
        /// </summary>
        [TestMethod]
        public void Remove_ProductNotFound_RemovesNullAndReturnsOk()
        {
            // Arrange
            var products = new List<Product>().AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = 999 };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockSet.Verify(m => m.Remove(null), Times.Once);
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles null key by converting it to 0 and searching for ProductId == 0.
        /// Input: Payload with null key.
        /// Expected: Convert.ToInt32(null) returns 0, searches for product with ProductId == 0.
        /// </summary>
        [TestMethod]
        public void Remove_NullKey_ConvertsToZeroAndSearches()
        {
            // Arrange
            var product = new Product { ProductId = 0, ProductName = "Zero ID Product" };
            var products = new List<Product> { product }.AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = null };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product, okResult.Value);
            mockSet.Verify(m => m.Remove(product), Times.Once);
        }

        /// <summary>
        /// Tests that Remove handles multiple products in the database and removes only the matching one.
        /// Input: Payload with key matching one of several products.
        /// Expected: Only the product with matching ProductId is removed.
        /// </summary>
        [TestMethod]
        public void Remove_MultipleProductsExist_RemovesOnlyMatchingProduct()
        {
            // Arrange
            var targetProductId = 2;
            var product1 = new Product { ProductId = 1, ProductName = "Product 1" };
            var product2 = new Product { ProductId = 2, ProductName = "Product 2" };
            var product3 = new Product { ProductId = 3, ProductName = "Product 3" };
            var products = new List<Product> { product1, product2, product3 }.AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = targetProductId };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product2, okResult.Value);
            mockSet.Verify(m => m.Remove(product2), Times.Once);
            mockSet.Verify(m => m.Remove(product1), Times.Never);
            mockSet.Verify(m => m.Remove(product3), Times.Never);
        }

        /// <summary>
        /// Tests that Remove handles decimal key values that can be converted to integers.
        /// Input: Payload with decimal key values.
        /// Expected: Decimal is truncated to integer and matching product is removed.
        /// </summary>
        [TestMethod]
        [DataRow(5.0, 5, DisplayName = "Exact decimal")]
        [DataRow(5.9, 5, DisplayName = "Decimal rounded down")]
        [DataRow(5.1, 5, DisplayName = "Decimal with fraction")]
        public void Remove_DecimalKey_TruncatesAndRemovesProduct(double keyValue, int expectedProductId)
        {
            // Arrange
            var product = new Product { ProductId = expectedProductId, ProductName = "Test Product" };
            var products = new List<Product> { product }.AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = keyValue };

            // Act
            var result = controller.Remove(payload);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockSet.Verify(m => m.Remove(product), Times.Once);
        }

        /// <summary>
        /// Tests that Remove verifies SaveChanges is always called even when product is not found.
        /// Input: Payload with non-existent product key.
        /// Expected: SaveChanges is called even though no product exists.
        /// </summary>
        [TestMethod]
        public void Remove_ProductNotFound_StillCallsSaveChanges()
        {
            // Arrange
            var products = new List<Product>().AsQueryable();

            var mockSet = new Mock<DbSet<Product>>();
            mockSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(products.Provider);
            mockSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(products.Expression);
            mockSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(products.ElementType);
            mockSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(products.GetEnumerator());

            var mockContext = new Mock<ApplicationDbContext>();
            mockContext.Setup(c => c.Product).Returns(mockSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var controller = new ProductController(mockContext.Object);
            var payload = new CrudViewModel<Product> { key = 12345 };

            // Act
            controller.Remove(payload);

            // Assert
            mockContext.Verify(m => m.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method returns OkObjectResult with updated product when valid payload is provided.
        /// </summary>
        [TestMethod]
        public void Update_ValidPayloadWithProduct_ReturnsOkResultWithProduct()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Test Product",
                ProductCode = "TP001",
                Barcode = "123456",
                Description = "Test Description",
                ProductImageUrl = "http://test.com/image.jpg",
                UnitOfMeasureId = 1,
                DefaultBuyingPrice = 10.0,
                DefaultSellingPrice = 15.0,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                action = "update",
                key = 1,
                antiForgery = "token",
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product, okResult.Value);
            mockProductDbSet.Verify(db => db.Update(product), Times.Once);
            mockContext.Verify(c => c.SaveChanges(), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles SaveChanges returning zero (no rows affected).
        /// </summary>
        [TestMethod]
        public void Update_SaveChangesReturnsZero_ReturnsOkResultWithProduct()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var product = new Product
            {
                ProductId = 2,
                ProductName = "Another Product",
                ProductCode = "AP001",
                UnitOfMeasureId = 2,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method handles null product value in payload.
        /// </summary>
        [TestMethod]
        public void Update_NullProductValue_PassesNullToUpdate()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(0);

            var payload = new CrudViewModel<Product>
            {
                action = "update",
                value = null
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
            mockProductDbSet.Verify(db => db.Update(null), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles product with minimum valid values.
        /// </summary>
        [TestMethod]
        public void Update_ProductWithMinimumValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var product = new Product
            {
                ProductId = 0,
                ProductName = string.Empty,
                UnitOfMeasureId = 0,
                DefaultBuyingPrice = 0.0,
                DefaultSellingPrice = 0.0,
                BranchId = 0,
                CurrencyId = 0
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method handles product with extreme numeric values.
        /// </summary>
        [TestMethod]
        public void Update_ProductWithExtremeValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var product = new Product
            {
                ProductId = int.MaxValue,
                ProductName = "Product",
                UnitOfMeasureId = int.MaxValue,
                DefaultBuyingPrice = double.MaxValue,
                DefaultSellingPrice = double.MaxValue,
                BranchId = int.MaxValue,
                CurrencyId = int.MaxValue
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockProductDbSet.Verify(db => db.Update(product), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles product with negative numeric values.
        /// </summary>
        [TestMethod]
        public void Update_ProductWithNegativeValues_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var product = new Product
            {
                ProductId = -1,
                ProductName = "Product",
                UnitOfMeasureId = -1,
                DefaultBuyingPrice = -100.5,
                DefaultSellingPrice = -200.75,
                BranchId = -1,
                CurrencyId = -1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method handles product with special double values (NaN, Infinity).
        /// </summary>
        [DataRow(double.NaN, double.NaN)]
        [DataRow(double.PositiveInfinity, double.NegativeInfinity)]
        [DataRow(double.NegativeInfinity, double.PositiveInfinity)]
        [TestMethod]
        public void Update_ProductWithSpecialDoubleValues_ReturnsOkResult(double buyingPrice, double sellingPrice)
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Product",
                UnitOfMeasureId = 1,
                DefaultBuyingPrice = buyingPrice,
                DefaultSellingPrice = sellingPrice,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockProductDbSet.Verify(db => db.Update(product), Times.Once);
        }

        /// <summary>
        /// Tests that Update method handles product with very long string values.
        /// </summary>
        [TestMethod]
        public void Update_ProductWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var longString = new string('A', 10000);
            var product = new Product
            {
                ProductId = 1,
                ProductName = longString,
                ProductCode = longString,
                Barcode = longString,
                Description = longString,
                ProductImageUrl = longString,
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual(product, okResult.Value);
        }

        /// <summary>
        /// Tests that Update method handles product with special characters in string values.
        /// </summary>
        [TestMethod]
        public void Update_ProductWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var mockProductDbSet = new Mock<DbSet<Product>>();
            mockContext.Setup(c => c.Product).Returns(mockProductDbSet.Object);
            mockContext.Setup(c => c.SaveChanges()).Returns(1);

            var specialString = "Test<>\"'&\n\r\t\0";
            var product = new Product
            {
                ProductId = 1,
                ProductName = specialString,
                ProductCode = specialString,
                Barcode = specialString,
                Description = specialString,
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            var controller = new ProductController(mockContext.Object);

            // Act
            var result = controller.Update(payload);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            mockProductDbSet.Verify(db => db.Update(product), Times.Once);
        }

        /// <summary>
        /// Tests that Insert method successfully adds a product and returns OkObjectResult with the product.
        /// Input: Valid payload with a valid Product.
        /// Expected: Product is added to DbSet, SaveChanges is called, and OkObjectResult is returned.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsOkResultWithProduct()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Test Product",
                ProductCode = "TP001",
                Barcode = "123456789",
                Description = "Test Description",
                ProductImageUrl = "http://example.com/image.jpg",
                UnitOfMeasureId = 1,
                DefaultBuyingPrice = 10.0,
                DefaultSellingPrice = 15.0,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                action = "insert",
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(product), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreSame(product, okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles null product value from payload.
        /// Input: Payload with null value property.
        /// Expected: Add is called with null, SaveChanges is called, OkObjectResult is returned with null.
        /// </summary>
        [TestMethod]
        public void Insert_PayloadWithNullValue_AddsNullAndReturnsOkResult()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var payload = new CrudViewModel<Product>
            {
                action = "insert",
                value = null!
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(null!), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.IsNull(okResult.Value);
        }

        /// <summary>
        /// Tests that Insert method handles product with minimum integer values.
        /// Input: Product with int.MinValue for integer properties.
        /// Expected: Product is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_ProductWithMinIntegerValues_ReturnsOkResult()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var product = new Product
            {
                ProductId = int.MinValue,
                ProductName = "Min Value Product",
                UnitOfMeasureId = int.MinValue,
                BranchId = int.MinValue,
                CurrencyId = int.MinValue
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(product), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles product with maximum integer values.
        /// Input: Product with int.MaxValue for integer properties.
        /// Expected: Product is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_ProductWithMaxIntegerValues_ReturnsOkResult()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var product = new Product
            {
                ProductId = int.MaxValue,
                ProductName = "Max Value Product",
                UnitOfMeasureId = int.MaxValue,
                BranchId = int.MaxValue,
                CurrencyId = int.MaxValue
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(product), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles product with special double values.
        /// Input: Product with NaN, PositiveInfinity, and NegativeInfinity for price properties.
        /// Expected: Product is added and returned successfully.
        /// </summary>
        [TestMethod]
        [DataRow(double.NaN, double.NaN, DisplayName = "NaN values")]
        [DataRow(double.PositiveInfinity, double.NegativeInfinity, DisplayName = "Infinity values")]
        [DataRow(double.MinValue, double.MaxValue, DisplayName = "Min/Max values")]
        [DataRow(0.0, 0.0, DisplayName = "Zero values")]
        [DataRow(-100.5, -200.75, DisplayName = "Negative values")]
        public void Insert_ProductWithSpecialDoubleValues_ReturnsOkResult(double buyingPrice, double sellingPrice)
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Special Price Product",
                DefaultBuyingPrice = buyingPrice,
                DefaultSellingPrice = sellingPrice,
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(product), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles product with various string edge cases.
        /// Input: Product with empty strings, whitespace strings, and very long strings.
        /// Expected: Product is added and returned successfully.
        /// </summary>
        [TestMethod]
        [DataRow("", "", "", "", "", DisplayName = "Empty strings")]
        [DataRow("   ", "   ", "   ", "   ", "   ", DisplayName = "Whitespace strings")]
        [DataRow(null, null, null, null, null, DisplayName = "Null strings")]
        public void Insert_ProductWithStringEdgeCases_ReturnsOkResult(string? productName, string? productCode, string? barcode, string? description, string? imageUrl)
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var product = new Product
            {
                ProductId = 1,
                ProductName = productName!,
                ProductCode = productCode,
                Barcode = barcode,
                Description = description,
                ProductImageUrl = imageUrl,
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(product), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles product with very long strings.
        /// Input: Product with strings exceeding typical length limits.
        /// Expected: Product is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_ProductWithVeryLongStrings_ReturnsOkResult()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var longString = new string('A', 10000);

            var product = new Product
            {
                ProductId = 1,
                ProductName = longString,
                ProductCode = longString,
                Barcode = longString,
                Description = longString,
                ProductImageUrl = longString,
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(product), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method handles product with special characters in strings.
        /// Input: Product with special characters, unicode, control characters.
        /// Expected: Product is added and returned successfully.
        /// </summary>
        [TestMethod]
        public void Insert_ProductWithSpecialCharacters_ReturnsOkResult()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var product = new Product
            {
                ProductId = 1,
                ProductName = "Test\n\r\t<>&\"'产品名称🎉",
                ProductCode = "!@#$%^&*()_+-={}[]|\\:;\"'<>?,./",
                Barcode = "\0\u0001\u001F",
                Description = "Line1\nLine2\rLine3\tTab",
                ProductImageUrl = "http://example.com/image?param=value&other=value",
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            mockSet.Verify(m => m.Add(product), Times.Once());
            mockContext.Verify(m => m.SaveChanges(), Times.Once());
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that Insert method returns the exact same product instance passed in.
        /// Input: Valid payload with a product.
        /// Expected: The returned OkObjectResult contains the same product instance.
        /// </summary>
        [TestMethod]
        public void Insert_ValidPayload_ReturnsSameProductInstance()
        {
            // Arrange
            var mockSet = new Mock<DbSet<Product>>();
            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(m => m.Product).Returns(mockSet.Object);
            mockContext.Setup(m => m.SaveChanges()).Returns(1);

            var controller = new ProductController(mockContext.Object);

            var product = new Product
            {
                ProductId = 42,
                ProductName = "Test Product",
                UnitOfMeasureId = 1,
                BranchId = 1,
                CurrencyId = 1
            };

            var payload = new CrudViewModel<Product>
            {
                value = product
            };

            // Act
            var result = controller.Insert(payload);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreSame(product, okResult.Value);
            Assert.AreEqual(42, ((Product)okResult.Value!).ProductId);
        }

        /// <summary>
        /// Tests that GetProduct returns OkObjectResult with empty list when database contains no products.
        /// Input: Empty product list.
        /// Expected: Returns OK result with empty Items list and Count of 0.
        /// </summary>
        [TestMethod]
        public async Task GetProduct_EmptyDatabase_ReturnsOkWithEmptyList()
        {
            // Arrange
            var products = new List<Product>();
            var mockContext = CreateMockContext(products);
            var controller = new ProductController(mockContext.Object);

            // Act
            var result = await controller.GetProduct();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = (List<Product>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(0, items.Count);
            Assert.AreEqual(0, count);
        }

        /// <summary>
        /// Tests that GetProduct returns OkObjectResult with single product when database contains one product.
        /// Input: Single product in database.
        /// Expected: Returns OK result with Items list containing one product and Count of 1.
        /// </summary>
        [TestMethod]
        public async Task GetProduct_SingleProduct_ReturnsOkWithSingleItem()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "Test Product",
                    ProductCode = "TP001",
                    Barcode = "123456789",
                    Description = "Test Description",
                    ProductImageUrl = "http://test.com/image.jpg",
                    UnitOfMeasureId = 1,
                    DefaultBuyingPrice = 10.0,
                    DefaultSellingPrice = 15.0,
                    BranchId = 1,
                    CurrencyId = 1
                }
            };
            var mockContext = CreateMockContext(products);
            var controller = new ProductController(mockContext.Object);

            // Act
            var result = await controller.GetProduct();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = (List<Product>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual(1, count);
            Assert.AreEqual("Test Product", items[0].ProductName);
            Assert.AreEqual("TP001", items[0].ProductCode);
        }

        /// <summary>
        /// Tests that GetProduct returns OkObjectResult with multiple products when database contains many products.
        /// Input: Multiple products in database.
        /// Expected: Returns OK result with Items list containing all products and correct Count.
        /// </summary>
        [TestMethod]
        public async Task GetProduct_MultipleProducts_ReturnsOkWithAllItems()
        {
            // Arrange
            var products = new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    ProductName = "Product 1",
                    ProductCode = "P001",
                    Barcode = "111111111",
                    Description = "Description 1",
                    ProductImageUrl = "http://test.com/image1.jpg",
                    UnitOfMeasureId = 1,
                    DefaultBuyingPrice = 10.0,
                    DefaultSellingPrice = 15.0,
                    BranchId = 1,
                    CurrencyId = 1
                },
                new Product
                {
                    ProductId = 2,
                    ProductName = "Product 2",
                    ProductCode = "P002",
                    Barcode = "222222222",
                    Description = "Description 2",
                    ProductImageUrl = "http://test.com/image2.jpg",
                    UnitOfMeasureId = 2,
                    DefaultBuyingPrice = 20.0,
                    DefaultSellingPrice = 30.0,
                    BranchId = 2,
                    CurrencyId = 2
                },
                new Product
                {
                    ProductId = 3,
                    ProductName = "Product 3",
                    ProductCode = "P003",
                    Barcode = "333333333",
                    Description = "Description 3",
                    ProductImageUrl = "http://test.com/image3.jpg",
                    UnitOfMeasureId = 3,
                    DefaultBuyingPrice = 30.0,
                    DefaultSellingPrice = 45.0,
                    BranchId = 3,
                    CurrencyId = 3
                }
            };
            var mockContext = CreateMockContext(products);
            var controller = new ProductController(mockContext.Object);

            // Act
            var result = await controller.GetProduct();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(200, okResult.StatusCode);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = (List<Product>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(3, items.Count);
            Assert.AreEqual(3, count);
            Assert.AreEqual("Product 1", items[0].ProductName);
            Assert.AreEqual("Product 2", items[1].ProductName);
            Assert.AreEqual("Product 3", items[2].ProductName);
        }

        /// <summary>
        /// Tests that GetProduct returns count matching the number of items in the list.
        /// Input: List of products.
        /// Expected: Count property matches Items.Count.
        /// </summary>
        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(5)]
        [DataRow(10)]
        [DataRow(100)]
        public async Task GetProduct_VariousItemCounts_ReturnsMatchingCount(int productCount)
        {
            // Arrange
            var products = new List<Product>();
            for (int i = 1; i <= productCount; i++)
            {
                products.Add(new Product
                {
                    ProductId = i,
                    ProductName = $"Product {i}",
                    ProductCode = $"P{i:D3}",
                    Barcode = $"{i:D9}",
                    Description = $"Description {i}",
                    ProductImageUrl = $"http://test.com/image{i}.jpg",
                    UnitOfMeasureId = i,
                    DefaultBuyingPrice = i * 10.0,
                    DefaultSellingPrice = i * 15.0,
                    BranchId = i,
                    CurrencyId = i
                });
            }
            var mockContext = CreateMockContext(products);
            var controller = new ProductController(mockContext.Object);

            // Act
            var result = await controller.GetProduct();

            // Assert
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var value = okResult.Value;
            Assert.IsNotNull(value);

            var itemsProperty = value.GetType().GetProperty("Items");
            var countProperty = value.GetType().GetProperty("Count");
            Assert.IsNotNull(itemsProperty);
            Assert.IsNotNull(countProperty);

            var items = (List<Product>)itemsProperty.GetValue(value);
            var count = (int)countProperty.GetValue(value);

            Assert.IsNotNull(items);
            Assert.AreEqual(productCount, items.Count);
            Assert.AreEqual(productCount, count);
            Assert.AreEqual(items.Count, count);
        }

        /// <summary>
        /// Helper method to create a mock ApplicationDbContext with a mocked Product DbSet.
        /// </summary>
        /// <param name="products">List of products to be returned by the mocked DbSet.</param>
        /// <returns>Mocked ApplicationDbContext.</returns>
        private Mock<ApplicationDbContext> CreateMockContext(List<Product> products)
        {
            var queryable = products.AsQueryable();

            var mockDbSet = new Mock<DbSet<Product>>();
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<Product>(queryable.Provider));
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.Expression).Returns(queryable.Expression);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            mockDbSet.As<IQueryable<Product>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());
            mockDbSet.As<IAsyncEnumerable<Product>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<Product>(queryable.GetEnumerator()));

            var mockContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            mockContext.Setup(c => c.Product).Returns(mockDbSet.Object);

            return mockContext;
        }

        /// <summary>
        /// Helper class to provide async query provider for testing EF Core async operations.
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
                var executionResult = typeof(IQueryProvider)
                    .GetMethod(
                        name: nameof(IQueryProvider.Execute),
                        genericParameterCount: 1,
                        types: new[] { typeof(System.Linq.Expressions.Expression) })!
                    .MakeGenericMethod(resultType)
                    .Invoke(this, new[] { expression });

                return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
                    .MakeGenericMethod(resultType)
                    .Invoke(null, new[] { executionResult })!;
            }
        }

        /// <summary>
        /// Helper class to provide async enumerable for testing EF Core async operations.
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
        /// Helper class to provide async enumerator for testing EF Core async operations.
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
                return default;
            }
        }
    }
}