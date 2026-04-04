using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using coderush.Controllers.Api;
using coderush.Data;
using coderush.Models;
using coderush.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;


namespace coderush.Controllers.Api.UnitTests
{
    /// <summary>
    /// Unit tests for UploadProfilePictureController.
    /// </summary>
    [TestClass]
    public class UploadProfilePictureControllerTests
    {
        private Mock<IFunctional>? _functionalServiceMock;
        private Mock<IWebHostEnvironment>? _envMock;
        private Mock<UserManager<ApplicationUser>>? _userManagerMock;
        private Mock<ApplicationDbContext>? _contextMock;
        private Mock<DbSet<UserProfile>>? _userProfileDbSetMock;
        private UploadProfilePictureController? _controller;

        [TestInitialize]
        public void Setup()
        {
            _functionalServiceMock = new Mock<IFunctional>();
            _envMock = new Mock<IWebHostEnvironment>();

            var userStoreMock = new Mock<IUserStore<ApplicationUser>>();
            _userManagerMock = new Mock<UserManager<ApplicationUser>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _contextMock = new Mock<ApplicationDbContext>(options);

            _userProfileDbSetMock = new Mock<DbSet<UserProfile>>();
            // Register IQueryable<UserProfile> interface BEFORE .Object is accessed
            // so that Castle.DynamicProxy generates interceptable interface dispatch
            var defaultProfiles = new List<UserProfile>().AsQueryable();
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.Provider).Returns(defaultProfiles.Provider);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.Expression).Returns(defaultProfiles.Expression);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.ElementType).Returns(defaultProfiles.ElementType);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.GetEnumerator()).Returns(defaultProfiles.GetEnumerator());
            _contextMock.Setup(c => c.UserProfile).Returns(_userProfileDbSetMock.Object);

            _controller = new UploadProfilePictureController(
                _functionalServiceMock.Object,
                _envMock.Object,
                _userManagerMock.Object,
                _contextMock.Object);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns BadRequest when file has no extension.
        /// Input: File with no extension.
        /// Expected: BadRequest with appropriate message.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_FileWithNoExtension_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("filewithoutext");
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.IsNotNull(badRequestResult.Value);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns BadRequest when file has empty extension.
        /// Input: File with empty string extension.
        /// Expected: BadRequest with appropriate message.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_FileWithEmptyExtension_ReturnsBadRequest()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file.");
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns BadRequest for disallowed file extensions.
        /// Input: Files with extensions not in the allowed list.
        /// Expected: BadRequest with appropriate message.
        /// </summary>
        [TestMethod]
        [DataRow(".txt")]
        [DataRow(".pdf")]
        [DataRow(".exe")]
        [DataRow(".doc")]
        [DataRow(".zip")]
        [DataRow(".svg")]
        public async Task PostUploadProfilePicture_DisallowedExtension_ReturnsBadRequest(string extension)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns($"file{extension}");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            var badRequestResult = (BadRequestObjectResult)result;
            var value = badRequestResult.Value;
            Assert.IsNotNull(value);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture accepts all allowed image extensions regardless of case.
        /// Input: Files with allowed extensions in various cases.
        /// Expected: Proceeds past validation without BadRequest.
        /// </summary>
        [TestMethod]
        [DataRow(".jpg")]
        [DataRow(".JPG")]
        [DataRow(".jpeg")]
        [DataRow(".JPEG")]
        [DataRow(".png")]
        [DataRow(".PNG")]
        [DataRow(".gif")]
        [DataRow(".GIF")]
        [DataRow(".bmp")]
        [DataRow(".BMP")]
        [DataRow(".webp")]
        [DataRow(".WEBP")]
        public async Task PostUploadProfilePicture_AllowedExtensions_PassesValidation(string extension)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns($"file{extension}");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            var user = new ApplicationUser { Id = "user123" };
            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                ApplicationUserId = "user123"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();

            _userProfileDbSetMock!.As<IQueryable<UserProfile>>()
                .Setup(m => m.Provider).Returns(userProfiles.Provider);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.Expression).Returns(userProfiles.Expression);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.ElementType).Returns(userProfiles.ElementType);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.GetEnumerator()).Returns(userProfiles.GetEnumerator());

            _contextMock!.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns BadRequest when ContentType does not start with "image/".
        /// Input: File with non-image ContentType.
        /// Expected: BadRequest with appropriate message.
        /// </summary>
        [TestMethod]
        [DataRow("application/pdf")]
        [DataRow("text/plain")]
        [DataRow("application/octet-stream")]
        [DataRow("video/mp4")]
        [DataRow("audio/mp3")]
        public async Task PostUploadProfilePicture_NonImageContentType_ReturnsBadRequest(string contentType)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file.jpg");
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            var files = new List<IFormFile> { fileMock.Object };

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture accepts valid image ContentTypes.
        /// Input: File with valid image ContentType.
        /// Expected: Proceeds past validation.
        /// </summary>
        [TestMethod]
        [DataRow("image/jpeg")]
        [DataRow("image/png")]
        [DataRow("image/gif")]
        [DataRow("image/bmp")]
        [DataRow("image/webp")]
        [DataRow("Image/JPEG")]
        [DataRow("IMAGE/PNG")]
        public async Task PostUploadProfilePicture_ValidImageContentType_PassesValidation(string contentType)
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file.jpg");
            fileMock.Setup(f => f.ContentType).Returns(contentType);
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns BadRequest when first file in list is invalid.
        /// Input: Multiple files with first file having invalid extension.
        /// Expected: BadRequest without processing remaining files.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_MultipleFilesFirstInvalid_ReturnsBadRequest()
        {
            // Arrange
            var invalidFileMock = new Mock<IFormFile>();
            invalidFileMock.Setup(f => f.FileName).Returns("file.txt");
            invalidFileMock.Setup(f => f.ContentType).Returns("text/plain");

            var validFileMock = new Mock<IFormFile>();
            validFileMock.Setup(f => f.FileName).Returns("file.jpg");
            validFileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            var files = new List<IFormFile> { invalidFileMock.Object, validFileMock.Object };

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _functionalServiceMock!.Verify(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns BadRequest when last file in list is invalid.
        /// Input: Multiple files with last file having invalid ContentType.
        /// Expected: BadRequest without calling upload service.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_MultipleFilesLastInvalid_ReturnsBadRequest()
        {
            // Arrange
            var validFileMock = new Mock<IFormFile>();
            validFileMock.Setup(f => f.FileName).Returns("file.jpg");
            validFileMock.Setup(f => f.ContentType).Returns("image/jpeg");

            var invalidFileMock = new Mock<IFormFile>();
            invalidFileMock.Setup(f => f.FileName).Returns("file.png");
            invalidFileMock.Setup(f => f.ContentType).Returns("application/pdf");

            var files = new List<IFormFile> { validFileMock.Object, invalidFileMock.Object };

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestObjectResult));
            _functionalServiceMock!.Verify(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()), Times.Never);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture handles empty file list.
        /// Input: Empty list of files.
        /// Expected: Calls UploadFile with empty list and returns Ok.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_EmptyFileList_ReturnsOk()
        {
            // Arrange
            var files = new List<IFormFile>();

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual("uploaded-file.jpg", okResult.Value);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture successfully uploads and updates profile when user and profile exist.
        /// Input: Valid file, existing user, existing profile.
        /// Expected: Profile picture updated and saved, returns Ok with filename.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_ValidFileUserAndProfileExist_UpdatesProfileAndReturnsOk()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                "upload"))
                .ReturnsAsync("uploaded-123.jpg");

            var user = new ApplicationUser { Id = "user123" };
            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                ApplicationUserId = "user123",
                ProfilePicture = "/upload/old.jpg"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();

            _userProfileDbSetMock!.As<IQueryable<UserProfile>>()
                .Setup(m => m.Provider).Returns(userProfiles.Provider);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.Expression).Returns(userProfiles.Expression);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.ElementType).Returns(userProfiles.ElementType);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.GetEnumerator()).Returns(userProfiles.GetEnumerator());

            _contextMock!.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual("uploaded-123.jpg", okResult.Value);
            Assert.AreEqual("/upload/uploaded-123.jpg", userProfile.ProfilePicture);
            _userProfileDbSetMock.Verify(db => db.Update(userProfile), Times.Once);
            _contextMock.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture uploads file but skips profile update when user is null.
        /// Input: Valid file, no authenticated user.
        /// Expected: File uploaded, no profile update, returns Ok with filename.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_ValidFileButUserIsNull_UploadsFileWithoutProfileUpdate()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.png");
            fileMock.Setup(f => f.ContentType).Returns("image/png");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                "upload"))
                .ReturnsAsync("uploaded-456.png");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual("uploaded-456.png", okResult.Value);
            _userProfileDbSetMock!.Verify(db => db.Update(It.IsAny<UserProfile>()), Times.Never);
            _contextMock!.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture uploads file but skips update when user exists but profile does not.
        /// Input: Valid file, existing user, no user profile.
        /// Expected: File uploaded, no profile update, returns Ok with filename.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_ValidFileUserExistsButProfileIsNull_UploadsFileWithoutProfileUpdate()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.gif");
            fileMock.Setup(f => f.ContentType).Returns("image/gif");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                "upload"))
                .ReturnsAsync("uploaded-789.gif");

            var user = new ApplicationUser { Id = "user456" };
            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var userProfiles = new List<UserProfile>().AsQueryable();

            _userProfileDbSetMock!.As<IQueryable<UserProfile>>()
                .Setup(m => m.Provider).Returns(userProfiles.Provider);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.Expression).Returns(userProfiles.Expression);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.ElementType).Returns(userProfiles.ElementType);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.GetEnumerator()).Returns(userProfiles.GetEnumerator());

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            var okResult = (OkObjectResult)result;
            Assert.AreEqual("uploaded-789.gif", okResult.Value);
            _userProfileDbSetMock.Verify(db => db.Update(It.IsAny<UserProfile>()), Times.Never);
            _contextMock!.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns status 500 when UploadFile throws exception.
        /// Input: Valid file but UploadFile throws exception.
        /// Expected: Returns StatusCode 500 with exception message.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_UploadFileThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            var exceptionMessage = "Failed to upload file";
            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns status 500 when GetUserAsync throws exception.
        /// Input: Valid file but GetUserAsync throws exception.
        /// Expected: Returns StatusCode 500 with exception message.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_GetUserAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            var exceptionMessage = "User manager error";
            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture returns status 500 when SaveChangesAsync throws exception.
        /// Input: Valid file, user, profile but SaveChangesAsync throws exception.
        /// Expected: Returns StatusCode 500 with exception message.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_SaveChangesAsyncThrowsException_ReturnsInternalServerError()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                "upload"))
                .ReturnsAsync("uploaded-file.jpg");

            var user = new ApplicationUser { Id = "user123" };
            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                ApplicationUserId = "user123"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();

            _userProfileDbSetMock!.As<IQueryable<UserProfile>>()
                .Setup(m => m.Provider).Returns(userProfiles.Provider);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.Expression).Returns(userProfiles.Expression);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.ElementType).Returns(userProfiles.ElementType);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.GetEnumerator()).Returns(userProfiles.GetEnumerator());

            var exceptionMessage = "Database save failed";
            _contextMock!.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception(exceptionMessage));

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(ObjectResult));
            var objectResult = (ObjectResult)result;
            Assert.AreEqual(500, objectResult.StatusCode);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture handles file with multiple dots in filename correctly.
        /// Input: File with multiple dots in name (e.g., "image.backup.jpg").
        /// Expected: Extracts correct extension and proceeds with upload.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_FileWithMultipleDots_ExtractsCorrectExtension()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("my.profile.image.backup.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture handles very long filename.
        /// Input: File with very long filename.
        /// Expected: Processes file normally if extension is valid.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_VeryLongFilename_ProcessesNormally()
        {
            // Arrange
            var longName = new string('a', 500);
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns($"{longName}.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture handles filename with special characters.
        /// Input: File with special characters in name.
        /// Expected: Processes file if extension is valid.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_FilenameWithSpecialCharacters_ProcessesNormally()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("file@#$%^&*()_+-=[]{}|;',..jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture handles multiple valid files.
        /// Input: Multiple files with valid extensions and content types.
        /// Expected: All files validated and uploaded, returns Ok.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_MultipleValidFiles_ProcessesAllFiles()
        {
            // Arrange
            var file1Mock = new Mock<IFormFile>();
            file1Mock.Setup(f => f.FileName).Returns("image1.jpg");
            file1Mock.Setup(f => f.ContentType).Returns("image/jpeg");

            var file2Mock = new Mock<IFormFile>();
            file2Mock.Setup(f => f.FileName).Returns("image2.png");
            file2Mock.Setup(f => f.ContentType).Returns("image/png");

            var file3Mock = new Mock<IFormFile>();
            file3Mock.Setup(f => f.FileName).Returns("image3.gif");
            file3Mock.Setup(f => f.ContentType).Returns("image/gif");

            var files = new List<IFormFile> { file1Mock.Object, file2Mock.Object, file3Mock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                It.IsAny<string>()))
                .ReturnsAsync("uploaded-file.jpg");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.IsInstanceOfType(result, typeof(OkObjectResult));
            _functionalServiceMock.Verify(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                "upload"), Times.Once);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture constructs correct profile picture path.
        /// Input: Valid file upload.
        /// Expected: Profile picture path is "/upload/{fileName}".
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_ValidUpload_ConstructsCorrectProfilePicturePath()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            var expectedFileName = "unique-123-file.jpg";
            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                "upload"))
                .ReturnsAsync(expectedFileName);

            var user = new ApplicationUser { Id = "user123" };
            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var userProfile = new UserProfile
            {
                UserProfileId = 1,
                ApplicationUserId = "user123",
                ProfilePicture = "/upload/old.jpg"
            };
            var userProfiles = new List<UserProfile> { userProfile }.AsQueryable();

            _userProfileDbSetMock!.As<IQueryable<UserProfile>>()
                .Setup(m => m.Provider).Returns(userProfiles.Provider);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.Expression).Returns(userProfiles.Expression);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.ElementType).Returns(userProfiles.ElementType);
            _userProfileDbSetMock.As<IQueryable<UserProfile>>()
                .Setup(m => m.GetEnumerator()).Returns(userProfiles.GetEnumerator());

            _contextMock!.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            // Act
            var result = await _controller!.PostUploadProfilePicture(files);

            // Assert
            Assert.AreEqual($"/upload/{expectedFileName}", userProfile.ProfilePicture);
        }

        /// <summary>
        /// Tests that PostUploadProfilePicture passes correct folder name to UploadFile.
        /// Input: Valid file.
        /// Expected: UploadFile called with "upload" as folder parameter.
        /// </summary>
        [TestMethod]
        public async Task PostUploadProfilePicture_ValidFile_PassesCorrectFolderToUploadFile()
        {
            // Arrange
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("profile.jpg");
            fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
            var files = new List<IFormFile> { fileMock.Object };

            _functionalServiceMock!.Setup(f => f.UploadFile(
                It.IsAny<List<IFormFile>>(),
                It.IsAny<IWebHostEnvironment>(),
                "upload"))
                .ReturnsAsync("uploaded-file.jpg");

            _userManagerMock!.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync((ApplicationUser?)null);

            // Act
            await _controller!.PostUploadProfilePicture(files);

            // Assert
            _functionalServiceMock.Verify(f => f.UploadFile(
                It.Is<List<IFormFile>>(l => l == files),
                It.IsAny<IWebHostEnvironment>(),
                "upload"), Times.Once);
        }
    }
}