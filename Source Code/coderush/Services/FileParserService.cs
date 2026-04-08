using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coderush.Services
{
    public class FileParserService : IFileParserService
    {
        private static readonly string[] SupportedExtensions = [".txt", ".md", ".docx", ".xlsx", ".xls"];

        private const long MaxFileSize = 10 * 1024 * 1024; // 10 MB

        public bool IsSupported(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return false;

            var name = Path.GetFileName(fileName);
            var ext = Path.GetExtension(name).ToLowerInvariant();

            if (string.IsNullOrEmpty(Path.GetFileNameWithoutExtension(name)))
                return false;

            return SupportedExtensions.Contains(ext);
        }

        public async Task<string> ExtractTextAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return string.Empty;

            if (file.Length > MaxFileSize)
                throw new InvalidOperationException("File size exceeds the 10 MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            return ext switch
            {
                ".txt" or ".md" => await ReadPlainTextAsync(file),
                ".docx" => await ReadDocxAsync(file),
                ".xlsx" or ".xls" => await ReadExcelAsync(file),
                _ => throw new InvalidOperationException($"Unsupported file type: {ext}")
            };
        }

        private static async Task<string> ReadPlainTextAsync(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return await reader.ReadToEndAsync();
        }

        private static async Task<string> ReadDocxAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var wordDoc = WordprocessingDocument.Open(memoryStream, false);
            var body = wordDoc.MainDocumentPart?.Document?.Body;
            if (body == null)
                return string.Empty;

            var sb = new StringBuilder();
            foreach (var paragraph in body.Descendants<Paragraph>())
            {
                var text = paragraph.InnerText;
                if (!string.IsNullOrWhiteSpace(text))
                    sb.AppendLine(text);
            }

            return sb.ToString();
        }

        private static async Task<string> ReadExcelAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var workbook = new XLWorkbook(memoryStream);
            var sb = new StringBuilder();

            foreach (var worksheet in workbook.Worksheets)
            {
                sb.AppendLine($"## Sheet: {worksheet.Name}");
                sb.AppendLine();

                var rangeUsed = worksheet.RangeUsed();
                if (rangeUsed == null)
                    continue;

                var rows = rangeUsed.RowsUsed().ToList();
                if (rows.Count == 0)
                    continue;

                // Build a markdown table from the worksheet
                var firstRow = rows[0];
                var colCount = firstRow.CellCount();

                // Header row
                var headers = new string[colCount];
                for (int c = 0; c < colCount; c++)
                {
                    headers[c] = firstRow.Cell(c + 1).GetFormattedString().Trim();
                    if (string.IsNullOrEmpty(headers[c]))
                        headers[c] = $"Column{c + 1}";
                }

                sb.AppendLine("| " + string.Join(" | ", headers) + " |");
                sb.AppendLine("| " + string.Join(" | ", headers.Select(_ => "---")) + " |");

                // Data rows
                for (int r = 1; r < rows.Count; r++)
                {
                    var row = rows[r];
                    var cells = new string[colCount];
                    for (int c = 0; c < colCount; c++)
                    {
                        cells[c] = row.Cell(c + 1).GetFormattedString().Trim();
                    }
                    sb.AppendLine("| " + string.Join(" | ", cells) + " |");
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
