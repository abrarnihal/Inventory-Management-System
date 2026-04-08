using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace coderush.Services.UnitTests
{
    /// <summary>
    /// Unit tests for the <see cref="FileParserService"/> class.
    /// </summary>
    [TestClass]
    public class FileParserServiceTests
    {
        /// <summary>
        /// Tests that IsSupported returns false when the fileName parameter is null, empty, or whitespace.
        /// </summary>
        /// <param name="fileName">The fileName input to test.</param>
        [TestMethod]
        [DataRow(null, DisplayName = "Null fileName")]
        [DataRow("", DisplayName = "Empty fileName")]
        [DataRow(" ", DisplayName = "Single space")]
        [DataRow("   ", DisplayName = "Multiple spaces")]
        [DataRow("\t", DisplayName = "Tab character")]
        [DataRow("\n", DisplayName = "Newline character")]
        [DataRow("\r\n", DisplayName = "Carriage return and newline")]
        [DataRow("  \t\n  ", DisplayName = "Mixed whitespace")]
        public void IsSupported_NullEmptyOrWhitespaceFileName_ReturnsFalse(string? fileName)
        {
            // Arrange
            var service = new FileParserService();

            // Act
            var result = service.IsSupported(fileName!);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsSupported returns true when the fileName has a supported extension.
        /// </summary>
        /// <param name="fileName">The fileName with a supported extension.</param>
        [TestMethod]
        [DataRow("file.txt", DisplayName = "Text file with .txt extension")]
        [DataRow("document.md", DisplayName = "Markdown file with .md extension")]
        [DataRow("report.docx", DisplayName = "Word document with .docx extension")]
        [DataRow("spreadsheet.xlsx", DisplayName = "Excel file with .xlsx extension")]
        [DataRow("legacy.xls", DisplayName = "Legacy Excel file with .xls extension")]
        [DataRow("README.txt", DisplayName = "README text file")]
        [DataRow("notes.md", DisplayName = "Notes markdown file")]
        [DataRow("contract.docx", DisplayName = "Contract Word document")]
        [DataRow("data.xlsx", DisplayName = "Data Excel file")]
        [DataRow("olddata.xls", DisplayName = "Old data Excel file")]
        public void IsSupported_SupportedExtension_ReturnsTrue(string fileName)
        {
            // Arrange
            var service = new FileParserService();

            // Act
            var result = service.IsSupported(fileName);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that IsSupported is case-insensitive and returns true for supported extensions regardless of case.
        /// </summary>
        /// <param name="fileName">The fileName with varying case in the extension.</param>
        [TestMethod]
        [DataRow("FILE.TXT", DisplayName = "Uppercase .TXT")]
        [DataRow("Document.DOCX", DisplayName = "Uppercase .DOCX")]
        [DataRow("Sheet.XLSX", DisplayName = "Uppercase .XLSX")]
        [DataRow("Old.XLS", DisplayName = "Uppercase .XLS")]
        [DataRow("README.MD", DisplayName = "Uppercase .MD")]
        [DataRow("file.TxT", DisplayName = "Mixed case .TxT")]
        [DataRow("document.DoCx", DisplayName = "Mixed case .DoCx")]
        [DataRow("sheet.XlSx", DisplayName = "Mixed case .XlSx")]
        [DataRow("readme.Md", DisplayName = "Mixed case .Md")]
        [DataRow("FILE.txt", DisplayName = "Mixed case filename FILE.txt")]
        public void IsSupported_CaseInsensitiveSupportedExtension_ReturnsTrue(string fileName)
        {
            // Arrange
            var service = new FileParserService();

            // Act
            var result = service.IsSupported(fileName);

            // Assert
            Assert.IsTrue(result);
        }

        /// <summary>
        /// Tests that IsSupported returns false when the fileName has an unsupported extension.
        /// </summary>
        /// <param name="fileName">The fileName with an unsupported extension.</param>
        [TestMethod]
        [DataRow("document.pdf", DisplayName = "PDF file")]
        [DataRow("image.jpg", DisplayName = "JPEG image")]
        [DataRow("photo.png", DisplayName = "PNG image")]
        [DataRow("app.exe", DisplayName = "Executable file")]
        [DataRow("archive.zip", DisplayName = "ZIP archive")]
        [DataRow("code.cs", DisplayName = "C# source file")]
        [DataRow("script.js", DisplayName = "JavaScript file")]
        [DataRow("style.css", DisplayName = "CSS file")]
        [DataRow("data.json", DisplayName = "JSON file")]
        [DataRow("config.xml", DisplayName = "XML file")]
        [DataRow("file.unknown", DisplayName = "Unknown extension")]
        [DataRow("file.xyz", DisplayName = "Made-up extension")]
        public void IsSupported_UnsupportedExtension_ReturnsFalse(string fileName)
        {
            // Arrange
            var service = new FileParserService();

            // Act
            var result = service.IsSupported(fileName);

            // Assert
            Assert.IsFalse(result);
        }

        /// <summary>
        /// Tests that IsSupported handles edge cases correctly.
        /// </summary>
        /// <param name="fileName">The fileName edge case to test.</param>
        /// <param name="expected">The expected result.</param>
        [TestMethod]
        [DataRow("filename", false, DisplayName = "No extension")]
        [DataRow(".", false, DisplayName = "Just a dot")]
        [DataRow("filename.", false, DisplayName = "Ending with dot")]
        [DataRow(".txt", false, DisplayName = "Hidden file with .txt (no base name)")]
        [DataRow(".gitignore", false, DisplayName = "Hidden file .gitignore")]
        [DataRow("file.name.txt", true, DisplayName = "Multiple dots with supported extension")]
        [DataRow("my.file.name.docx", true, DisplayName = "Multiple dots with .docx")]
        [DataRow("archive.tar.gz", false, DisplayName = "Multiple dots with unsupported extension")]
        [DataRow("C:\\folder\\file.txt", true, DisplayName = "Absolute Windows path with .txt")]
        [DataRow("C:\\Users\\Documents\\report.docx", true, DisplayName = "Absolute Windows path with .docx")]
        [DataRow("/home/user/document.md", true, DisplayName = "Absolute Unix path with .md")]
        [DataRow("folder/subfolder/file.xlsx", true, DisplayName = "Relative path with .xlsx")]
        [DataRow("..\\parent\\file.xls", true, DisplayName = "Relative parent path with .xls")]
        [DataRow("folder\\file.pdf", false, DisplayName = "Path with unsupported extension")]
        [DataRow("file@#$.txt", true, DisplayName = "Special characters in filename with .txt")]
        [DataRow("file name with spaces.docx", true, DisplayName = "Spaces in filename with .docx")]
        [DataRow("very_long_filename_with_many_characters_in_it_to_test_edge_case.txt", true, DisplayName = "Very long filename with .txt")]
        public void IsSupported_EdgeCases_ReturnsExpectedResult(string fileName, bool expected)
        {
            // Arrange
            var service = new FileParserService();

            // Act
            var result = service.IsSupported(fileName);

            // Assert
            Assert.AreEqual(expected, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync returns an empty string when the file parameter is null.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_NullFile_ReturnsEmptyString()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            IFormFile? file = null;

            // Act
            var result = await service.ExtractTextAsync(file!);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync returns an empty string when the file has zero length.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_FileWithZeroLength_ReturnsEmptyString()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(0);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync does not throw when file size is exactly at the 10 MB limit.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_FileAtMaxSize_DoesNotThrowSizeException()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            long maxSize = 10 * 1024 * 1024; // 10 MB
            mockFile.Setup(f => f.Length).Returns(maxSize);
            mockFile.Setup(f => f.FileName).Returns("test.txt");

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync correctly extracts text from a .txt file.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_TxtFile_ReturnsExtractedText()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            var testContent = "This is test content in a text file.";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            mockFile.Setup(f => f.Length).Returns(testContent.Length);
            mockFile.Setup(f => f.FileName).Returns("test.txt");
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.AreEqual(testContent, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync correctly extracts text from a .md file.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_MdFile_ReturnsExtractedText()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            var testContent = "# Markdown Header\n\nThis is markdown content.";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            mockFile.Setup(f => f.Length).Returns(testContent.Length);
            mockFile.Setup(f => f.FileName).Returns("test.md");
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.AreEqual(testContent, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync handles file extensions in a case-insensitive manner.
        /// </summary>
        /// <param name="fileName">The file name with various extension casings.</param>
        [TestMethod]
        [DataRow("test.TXT")]
        [DataRow("test.Txt")]
        [DataRow("test.MD")]
        [DataRow("test.Md")]
        [DataRow("test.DOCX")]
        [DataRow("test.Docx")]
        [DataRow("test.XLSX")]
        [DataRow("test.Xlsx")]
        [DataRow("test.XLS")]
        [DataRow("test.Xls")]
        public async Task ExtractTextAsync_ExtensionCaseInsensitive_RecognizesSupported(string fileName)
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            mockFile.Setup(f => f.Length).Returns(100);
            mockFile.Setup(f => f.FileName).Returns(fileName);

            var extension = Path.GetExtension(fileName).ToLowerInvariant();
            if (extension == ".txt" || extension == ".md")
            {
                var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
                mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);
            }
            else
            {
                var memoryStream = new MemoryStream();
                mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                       .Callback<Stream, CancellationToken>((stream, token) => { })
                       .Returns(Task.CompletedTask);
            }

            // Act & Assert
            // Should not throw unsupported file type exception
            try
            {
                var result = await service.ExtractTextAsync(mockFile.Object);
                // If we get here without exception, the extension was recognized
                Assert.IsNotNull(result);
            }
            catch (InvalidOperationException ex) when (ex.Message.StartsWith("Unsupported file type:"))
            {
                Assert.Fail($"Extension {extension} should be supported but was rejected.");
            }
            catch (Exception)
            {
                // Other exceptions (e.g., FileFormatException) mean the extension
                // was recognized but parsing of mock data failed — acceptable here.
            }
        }

        /// <summary>
        /// Tests that ExtractTextAsync properly handles empty text files.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_EmptyTxtFile_ReturnsEmptyString()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            var memoryStream = new MemoryStream(Array.Empty<byte>());

            mockFile.Setup(f => f.Length).Returns(1); // Non-zero to pass the size check
            mockFile.Setup(f => f.FileName).Returns("empty.txt");
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync handles text files with special characters and Unicode content.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_TxtFileWithSpecialCharacters_ReturnsCorrectContent()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            var testContent = "Special chars: \r\n\t äöü 你好 🎉 €";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            mockFile.Setup(f => f.Length).Returns(testContent.Length);
            mockFile.Setup(f => f.FileName).Returns("special.txt");
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.AreEqual(testContent, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync handles files with very long names properly.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_FileWithLongName_ProcessesCorrectly()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            var longFileName = new string('a', 200) + ".txt";
            var testContent = "content";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            mockFile.Setup(f => f.Length).Returns(testContent.Length);
            mockFile.Setup(f => f.FileName).Returns(longFileName);
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.AreEqual(testContent, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync handles file names with multiple dots correctly.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_FileNameWithMultipleDots_UsesCorrectExtension()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            var testContent = "test content";
            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(testContent));

            mockFile.Setup(f => f.Length).Returns(testContent.Length);
            mockFile.Setup(f => f.FileName).Returns("my.file.name.with.dots.txt");
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.AreEqual(testContent, result);
        }

        /// <summary>
        /// Tests that ExtractTextAsync handles large but valid file sizes just under the maximum limit.
        /// </summary>
        [TestMethod]
        public async Task ExtractTextAsync_FileSizeJustUnderMax_ProcessesSuccessfully()
        {
            // Arrange
            var service = new coderush.Services.FileParserService();
            var mockFile = new Mock<IFormFile>();
            long maxSize = 10 * 1024 * 1024;
            mockFile.Setup(f => f.Length).Returns(maxSize - 1);
            mockFile.Setup(f => f.FileName).Returns("large.txt");

            var memoryStream = new MemoryStream(Encoding.UTF8.GetBytes("content"));
            mockFile.Setup(f => f.OpenReadStream()).Returns(memoryStream);

            // Act
            var result = await service.ExtractTextAsync(mockFile.Object);

            // Assert
            Assert.IsNotNull(result);
        }
    }
}