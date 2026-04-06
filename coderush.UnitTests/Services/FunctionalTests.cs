using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the Functional class.
    /// </summary>
    [TestClass]
    public class FunctionalTests
    {
        private static Mock<UserManager<coderush.Models.ApplicationUser>> CreateMockUserManager()
        {
            var store = new Mock<IUserStore<coderush.Models.ApplicationUser>>();
            return new Mock<UserManager<coderush.Models.ApplicationUser>>(
                store.Object, null, null, null, null, null, null, null, null);
        }

        private static Mock<RoleManager<IdentityRole>> CreateMockRoleManager()
        {
            var store = new Mock<IRoleStore<IdentityRole>>();
            return new Mock<RoleManager<IdentityRole>>(
                store.Object, null, null, null, null);
        }

        private static Mock<SignInManager<coderush.Models.ApplicationUser>> CreateMockSignInManager(
            UserManager<coderush.Models.ApplicationUser> userManager)
        {
            var contextAccessor = new Mock<Microsoft.AspNetCore.Http.IHttpContextAccessor>();
            var claimsFactory = new Mock<IUserClaimsPrincipalFactory<coderush.Models.ApplicationUser>>();
            return new Mock<SignInManager<coderush.Models.ApplicationUser>>(
                userManager, contextAccessor.Object, claimsFactory.Object, null, null, null, null);
        }

        private static Mock<DbSet<T>> CreateMockDbSet<T>() where T : class
        {
            var mockSet = new Mock<DbSet<T>>();
            mockSet.Setup(m => m.AddAsync(It.IsAny<T>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((T entity, CancellationToken token) => null as EntityEntry<T>);
            mockSet.Setup(m => m.AddRangeAsync(It.IsAny<IEnumerable<T>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return mockSet;
        }

        private static void SetupMinimalMocksForContext(Mock<ApplicationDbContext> mockContext)
        {
            mockContext.Setup(c => c.BillType).Returns(CreateMockDbSet<BillType>().Object);
            mockContext.Setup(c => c.Branch).Returns(CreateMockDbSet<Branch>().Object);
            mockContext.Setup(c => c.Warehouse).Returns(CreateMockDbSet<Warehouse>().Object);
            mockContext.Setup(c => c.CashBank).Returns(CreateMockDbSet<CashBank>().Object);
            mockContext.Setup(c => c.Currency).Returns(CreateMockDbSet<Currency>().Object);
            mockContext.Setup(c => c.InvoiceType).Returns(CreateMockDbSet<InvoiceType>().Object);
            mockContext.Setup(c => c.PaymentType).Returns(CreateMockDbSet<PaymentType>().Object);
            mockContext.Setup(c => c.PurchaseType).Returns(CreateMockDbSet<PurchaseType>().Object);
            mockContext.Setup(c => c.SalesType).Returns(CreateMockDbSet<SalesType>().Object);
            mockContext.Setup(c => c.ShipmentType).Returns(CreateMockDbSet<ShipmentType>().Object);
            mockContext.Setup(c => c.UnitOfMeasure).Returns(CreateMockDbSet<UnitOfMeasure>().Object);
            mockContext.Setup(c => c.ProductType).Returns(CreateMockDbSet<ProductType>().Object);
            mockContext.Setup(c => c.Product).Returns(CreateMockDbSet<Product>().Object);
            mockContext.Setup(c => c.CustomerType).Returns(CreateMockDbSet<CustomerType>().Object);
            mockContext.Setup(c => c.Customer).Returns(CreateMockDbSet<Customer>().Object);
            mockContext.Setup(c => c.VendorType).Returns(CreateMockDbSet<VendorType>().Object);
            mockContext.Setup(c => c.Vendor).Returns(CreateMockDbSet<Vendor>().Object);
            mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        }

        private static bool VerifyProductList(IEnumerable<Product> products)
        {
            var productList = new List<Product>(products);
            return productList.Count == 20 &&
                   productList.Exists(p => p.ProductName == "Chai") &&
                   productList.Exists(p => p.ProductName == "Tofu") &&
                   productList.Exists(p => p.ProductName == "Sir Rodney's Marmalade");
        }

        private static bool VerifyCustomerList(IEnumerable<Customer> customers)
        {
            var customerList = new List<Customer>(customers);
            return customerList.Count == 20 &&
                   customerList.Exists(c => c.CustomerName == "Hanari Carnes") &&
                   customerList.Exists(c => c.CustomerName == "Old World Delicatessen");
        }

        private static bool VerifyVendorList(IEnumerable<Vendor> vendors)
        {
            var vendorList = new List<Vendor>(vendors);
            return vendorList.Count == 19 &&
                   vendorList.Exists(v => v.VendorName == "Exotic Liquids") &&
                   vendorList.Exists(v => v.VendorName == "New England Seafood Cannery");
        }

        /// <summary>
        /// Tests that CreateDefaultSuperAdmin successfully creates a super admin user with profile and adds to roles when user creation succeeds.
        /// </summary>
        [TestMethod]
        public async Task CreateDefaultSuperAdmin_WhenUserCreationSucceeds_CreatesUserProfileAndAddsToRoles()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            var rolesMock = new Mock<IRoles>();
            var options = new SuperAdminDefaultOptions
            {
                Email = "superadmin@test.com",
                Password = "SuperAdmin123!"
            };
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var userProfileDbSetMock = new Mock<DbSet<UserProfile>>();
            contextMock.Setup(c => c.UserProfile).Returns(userProfileDbSetMock.Object);

            ApplicationUser capturedUser = null;
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((user, password) =>
                {
                    capturedUser = user;
                    user.Id = "test-user-id-123";
                })
                .ReturnsAsync(IdentityResult.Success);

            rolesMock.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            rolesMock.Setup(r => r.AddToRoles(It.IsAny<string>())).Returns(Task.CompletedTask);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                contextMock.Object,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.CreateDefaultSuperAdmin();

            // Assert
            rolesMock.Verify(r => r.GenerateRolesFromPagesAsync(), Times.Once);
            userManagerMock.Verify(um => um.CreateAsync(
                It.Is<ApplicationUser>(u => u.Email == "superadmin@test.com" && u.UserName == "superadmin@test.com" && u.EmailConfirmed == true),
                "SuperAdmin123!"), Times.Once);
            userProfileDbSetMock.Verify(db => db.AddAsync(
                It.Is<UserProfile>(p => p.FirstName == "Super" && p.LastName == "Admin" && p.Email == "superadmin@test.com" && p.ApplicationUserId == "test-user-id-123"),
                It.IsAny<CancellationToken>()), Times.Once);
            contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            rolesMock.Verify(r => r.AddToRoles("test-user-id-123"), Times.Once);
        }

        /// <summary>
        /// Tests that CreateDefaultSuperAdmin does not create user profile or add to roles when user creation fails.
        /// </summary>
        [TestMethod]
        public async Task CreateDefaultSuperAdmin_WhenUserCreationFails_DoesNotCreateUserProfile()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            var rolesMock = new Mock<IRoles>();
            var options = new SuperAdminDefaultOptions
            {
                Email = "superadmin@test.com",
                Password = "SuperAdmin123!"
            };
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var userProfileDbSetMock = new Mock<DbSet<UserProfile>>();
            contextMock.Setup(c => c.UserProfile).Returns(userProfileDbSetMock.Object);

            var failedResult = IdentityResult.Failed(new IdentityError { Description = "User creation failed" });
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(failedResult);

            rolesMock.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                contextMock.Object,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.CreateDefaultSuperAdmin();

            // Assert
            rolesMock.Verify(r => r.GenerateRolesFromPagesAsync(), Times.Once);
            userManagerMock.Verify(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Once);
            userProfileDbSetMock.Verify(db => db.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()), Times.Never);
            contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
            rolesMock.Verify(r => r.AddToRoles(It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that CreateDefaultSuperAdmin uses the correct email and password from SuperAdminDefaultOptions.
        /// </summary>
        /// <param name="email">The email to test with.</param>
        /// <param name="password">The password to test with.</param>
        [TestMethod]
        [DataRow("admin@example.com", "Password123!")]
        [DataRow("test@domain.org", "Test@Pass456")]
        [DataRow("user@site.net", "ComplexP@ssw0rd")]
        public async Task CreateDefaultSuperAdmin_WithVariousEmailAndPassword_UsesCorrectCredentials(string email, string password)
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            var rolesMock = new Mock<IRoles>();
            var options = new SuperAdminDefaultOptions
            {
                Email = email,
                Password = password
            };
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var userProfileDbSetMock = new Mock<DbSet<UserProfile>>();
            contextMock.Setup(c => c.UserProfile).Returns(userProfileDbSetMock.Object);

            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((user, pwd) => user.Id = "test-id")
                .ReturnsAsync(IdentityResult.Success);

            rolesMock.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            rolesMock.Setup(r => r.AddToRoles(It.IsAny<string>())).Returns(Task.CompletedTask);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                contextMock.Object,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.CreateDefaultSuperAdmin();

            // Assert
            userManagerMock.Verify(um => um.CreateAsync(
                It.Is<ApplicationUser>(u => u.Email == email && u.UserName == email && u.EmailConfirmed == true),
                password), Times.Once);
        }

        /// <summary>
        /// Tests that CreateDefaultSuperAdmin always calls GenerateRolesFromPagesAsync regardless of user creation result.
        /// </summary>
        [TestMethod]
        public async Task CreateDefaultSuperAdmin_Always_CallsGenerateRolesFromPagesAsync()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            var rolesMock = new Mock<IRoles>();
            var options = new SuperAdminDefaultOptions
            {
                Email = "admin@test.com",
                Password = "Pass123!"
            };
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed());

            rolesMock.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                contextMock.Object,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.CreateDefaultSuperAdmin();

            // Assert
            rolesMock.Verify(r => r.GenerateRolesFromPagesAsync(), Times.Once);
        }

        /// <summary>
        /// Tests that CreateDefaultSuperAdmin sets EmailConfirmed to true for the created user.
        /// </summary>
        [TestMethod]
        public async Task CreateDefaultSuperAdmin_Always_SetsEmailConfirmedToTrue()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            var rolesMock = new Mock<IRoles>();
            var options = new SuperAdminDefaultOptions
            {
                Email = "admin@test.com",
                Password = "Pass123!"
            };
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var userProfileDbSetMock = new Mock<DbSet<UserProfile>>();
            contextMock.Setup(c => c.UserProfile).Returns(userProfileDbSetMock.Object);

            ApplicationUser capturedUser = null;
            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((user, pwd) => capturedUser = user)
                .ReturnsAsync(IdentityResult.Success);

            rolesMock.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            rolesMock.Setup(r => r.AddToRoles(It.IsAny<string>())).Returns(Task.CompletedTask);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            userProfileDbSetMock.Setup(db => db.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<UserProfile>)null);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                contextMock.Object,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.CreateDefaultSuperAdmin();

            // Assert
            Assert.IsNotNull(capturedUser);
            Assert.IsTrue(capturedUser.EmailConfirmed);
        }

        /// <summary>
        /// Tests that CreateDefaultSuperAdmin creates UserProfile with hardcoded FirstName "Super" and LastName "Admin".
        /// </summary>
        [TestMethod]
        public async Task CreateDefaultSuperAdmin_WhenUserCreationSucceeds_CreatesProfileWithHardcodedNames()
        {
            // Arrange
            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            var userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);
            var roleManagerMock = new Mock<RoleManager<IdentityRole>>(
                Mock.Of<IRoleStore<IdentityRole>>(), null, null, null, null);
            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            var signInManagerMock = new Mock<SignInManager<ApplicationUser>>(
                userManagerMock.Object, Mock.Of<Microsoft.AspNetCore.Http.IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(), null, null, null, null);
            var rolesMock = new Mock<IRoles>();
            var options = new SuperAdminDefaultOptions
            {
                Email = "admin@test.com",
                Password = "Pass123!"
            };
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(options);

            var userProfileDbSetMock = new Mock<DbSet<UserProfile>>();
            contextMock.Setup(c => c.UserProfile).Returns(userProfileDbSetMock.Object);

            userManagerMock.Setup(um => um.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .Callback<ApplicationUser, string>((user, pwd) => user.Id = "test-id")
                .ReturnsAsync(IdentityResult.Success);

            rolesMock.Setup(r => r.GenerateRolesFromPagesAsync()).Returns(Task.CompletedTask);
            rolesMock.Setup(r => r.AddToRoles(It.IsAny<string>())).Returns(Task.CompletedTask);
            contextMock.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

            UserProfile capturedProfile = null;
            userProfileDbSetMock.Setup(db => db.AddAsync(It.IsAny<UserProfile>(), It.IsAny<CancellationToken>()))
                .Callback<UserProfile, CancellationToken>((profile, token) => capturedProfile = profile)
                .ReturnsAsync((Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<UserProfile>)null);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                contextMock.Object,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.CreateDefaultSuperAdmin();

            // Assert
            Assert.IsNotNull(capturedProfile);
            Assert.AreEqual("Super", capturedProfile.FirstName);
            Assert.AreEqual("Admin", capturedProfile.LastName);
        }

        #region InitAppData

        /// <summary>
        /// Tests that InitAppData seeds all expected reference data into the database.
        /// </summary>
        [TestMethod]
        public async Task InitAppData_WhenCalled_SeedsAllReferenceTypes()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(dbOptions);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                context,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.InitAppData();

            // Assert
            Assert.AreEqual(1, context.BillType.Count());
            Assert.AreEqual(1, context.Branch.Count());
            Assert.AreEqual(1, context.Warehouse.Count());
            Assert.AreEqual(1, context.CashBank.Count());
            Assert.AreEqual(1, context.Currency.Count());
            Assert.AreEqual(1, context.InvoiceType.Count());
            Assert.AreEqual(1, context.PaymentType.Count());
            Assert.AreEqual(1, context.PurchaseType.Count());
            Assert.AreEqual(1, context.SalesType.Count());
            Assert.AreEqual(1, context.ShipmentType.Count());
            Assert.AreEqual(1, context.UnitOfMeasure.Count());
            Assert.AreEqual(1, context.ProductType.Count());
            Assert.AreEqual(1, context.CustomerType.Count());
            Assert.AreEqual(1, context.VendorType.Count());
        }

        /// <summary>
        /// Tests that InitAppData seeds 20 products.
        /// </summary>
        [TestMethod]
        public async Task InitAppData_WhenCalled_Seeds20Products()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(dbOptions);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                context,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.InitAppData();

            // Assert
            Assert.AreEqual(20, context.Product.Count());
            Assert.IsTrue(context.Product.Any(p => p.ProductName == "Chai"));
            Assert.IsTrue(context.Product.Any(p => p.ProductName == "Sir Rodney's Marmalade"));
        }

        /// <summary>
        /// Tests that InitAppData seeds 20 customers.
        /// </summary>
        [TestMethod]
        public async Task InitAppData_WhenCalled_Seeds20Customers()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(dbOptions);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                context,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.InitAppData();

            // Assert
            Assert.AreEqual(20, context.Customer.Count());
            Assert.IsTrue(context.Customer.Any(c => c.CustomerName == "Hanari Carnes"));
            Assert.IsTrue(context.Customer.Any(c => c.CustomerName == "Old World Delicatessen"));
        }

        /// <summary>
        /// Tests that InitAppData seeds 19 vendors.
        /// </summary>
        [TestMethod]
        public async Task InitAppData_WhenCalled_Seeds19Vendors()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(dbOptions);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                context,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.InitAppData();

            // Assert
            Assert.AreEqual(19, context.Vendor.Count());
            Assert.IsTrue(context.Vendor.Any(v => v.VendorName == "Exotic Liquids"));
            Assert.IsTrue(context.Vendor.Any(v => v.VendorName == "New England Seafood Cannery"));
        }

        /// <summary>
        /// Tests that InitAppData seeds a currency with the correct code.
        /// </summary>
        [TestMethod]
        public async Task InitAppData_WhenCalled_SeedsCurrencyWithUSD()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;
            using var context = new ApplicationDbContext(dbOptions);

            var functional = new Functional(
                userManagerMock.Object,
                roleManagerMock.Object,
                context,
                signInManagerMock.Object,
                rolesMock.Object,
                optionsMock.Object);

            // Act
            await functional.InitAppData();

            // Assert
            var currency = context.Currency.First();
            Assert.AreEqual("Default", currency.CurrencyName);
            Assert.AreEqual("USD", currency.CurrencyCode);
        }

        #endregion

        #region UploadFile

        /// <summary>
        /// Tests that UploadFile uploads a file and returns the generated filename.
        /// </summary>
        [TestMethod]
        public async Task UploadFile_ValidFile_UploadsAndReturnsFileName()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            var tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var mockEnv = new Mock<IWebHostEnvironment>();
                mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

                var fileContent = new byte[] { 1, 2, 3, 4, 5 };
                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(fileContent.Length);
                mockFile.Setup(f => f.FileName).Returns("test.png");
                mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                    .Callback<Stream, CancellationToken>((stream, ct) => stream.Write(fileContent, 0, fileContent.Length))
                    .Returns(Task.CompletedTask);

                var functional = new Functional(
                    userManagerMock.Object,
                    roleManagerMock.Object,
                    contextMock.Object,
                    signInManagerMock.Object,
                    rolesMock.Object,
                    optionsMock.Object);

                // Act
                var result = await functional.UploadFile(new List<IFormFile> { mockFile.Object }, mockEnv.Object, "uploads");

                // Assert
                Assert.IsFalse(string.IsNullOrEmpty(result));
                Assert.IsTrue(result.EndsWith(".png"));
                Assert.IsTrue(System.Guid.TryParse(Path.GetFileNameWithoutExtension(result), out _));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Tests that UploadFile returns empty string when the file list is empty.
        /// </summary>
        [TestMethod]
        public async Task UploadFile_EmptyFileList_ReturnsEmptyString()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            var tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var mockEnv = new Mock<IWebHostEnvironment>();
                mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

                var functional = new Functional(
                    userManagerMock.Object,
                    roleManagerMock.Object,
                    contextMock.Object,
                    signInManagerMock.Object,
                    rolesMock.Object,
                    optionsMock.Object);

                // Act
                var result = await functional.UploadFile(new List<IFormFile>(), mockEnv.Object, "uploads");

                // Assert
                Assert.AreEqual("", result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Tests that UploadFile skips files with zero length.
        /// </summary>
        [TestMethod]
        public async Task UploadFile_ZeroLengthFile_SkipsFile()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            var tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var mockEnv = new Mock<IWebHostEnvironment>();
                mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

                var mockFile = new Mock<IFormFile>();
                mockFile.Setup(f => f.Length).Returns(0);
                mockFile.Setup(f => f.FileName).Returns("empty.txt");

                var functional = new Functional(
                    userManagerMock.Object,
                    roleManagerMock.Object,
                    contextMock.Object,
                    signInManagerMock.Object,
                    rolesMock.Object,
                    optionsMock.Object);

                // Act
                var result = await functional.UploadFile(new List<IFormFile> { mockFile.Object }, mockEnv.Object, "uploads");

                // Assert
                Assert.AreEqual("", result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Tests that UploadFile with multiple files returns the last file's name.
        /// </summary>
        [TestMethod]
        public async Task UploadFile_MultipleFiles_ReturnsLastFileName()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            var tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var mockEnv = new Mock<IWebHostEnvironment>();
                mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

                var mockFile1 = CreateMockFormFile("file1.png", new byte[] { 1, 2 });
                var mockFile2 = CreateMockFormFile("file2.jpg", new byte[] { 3, 4 });

                var functional = new Functional(
                    userManagerMock.Object,
                    roleManagerMock.Object,
                    contextMock.Object,
                    signInManagerMock.Object,
                    rolesMock.Object,
                    optionsMock.Object);

                // Act
                var result = await functional.UploadFile(
                    new List<IFormFile> { mockFile1.Object, mockFile2.Object }, mockEnv.Object, "uploads");

                // Assert
                Assert.IsTrue(result.EndsWith(".jpg"), "Should return the last uploaded file name");
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        /// <summary>
        /// Tests that UploadFile creates the upload directory if it doesn't exist.
        /// </summary>
        [TestMethod]
        public async Task UploadFile_UploadDirectoryDoesNotExist_CreatesDirectory()
        {
            // Arrange
            var userManagerMock = CreateMockUserManager();
            var roleManagerMock = CreateMockRoleManager();
            var signInManagerMock = CreateMockSignInManager(userManagerMock.Object);
            var rolesMock = new Mock<IRoles>();
            var optionsMock = new Mock<IOptions<SuperAdminDefaultOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new SuperAdminDefaultOptions());

            var contextMock = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());

            var tempDir = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);

            try
            {
                var mockEnv = new Mock<IWebHostEnvironment>();
                mockEnv.Setup(e => e.WebRootPath).Returns(tempDir);

                var mockFile = CreateMockFormFile("test.png", new byte[] { 1 });

                var functional = new Functional(
                    userManagerMock.Object,
                    roleManagerMock.Object,
                    contextMock.Object,
                    signInManagerMock.Object,
                    rolesMock.Object,
                    optionsMock.Object);

                var uploadFolder = "newFolder";

                // Act
                await functional.UploadFile(new List<IFormFile> { mockFile.Object }, mockEnv.Object, uploadFolder);

                // Assert
                Assert.IsTrue(Directory.Exists(Path.Combine(tempDir, uploadFolder)));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        private static Mock<IFormFile> CreateMockFormFile(string fileName, byte[] content)
        {
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(content.Length);
            mockFile.Setup(f => f.FileName).Returns(fileName);
            mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), default))
                .Callback<Stream, CancellationToken>((stream, ct) => stream.Write(content, 0, content.Length))
                .Returns(Task.CompletedTask);
            return mockFile;
        }

        #endregion
    }
}