using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace coderush.IntegrationTests.PostmanTests;

/// <summary>
/// Runs the Postman collection via Newman CLI and reports pass/fail results
/// as MSTest assertions. This bridges Postman-based API testing with
/// the Visual Studio Test Explorer.
///
/// Prerequisites:
///   1. Install Newman globally:  npm install -g newman
///   2. A working database connection (configured in appsettings.json).
///
/// To skip these tests when the app is not running, set the environment variable
/// SKIP_POSTMAN_TESTS=true.
/// </summary>
[TestClass]
public sealed class PostmanCollectionRunnerTests
{
    private static readonly string CollectionPath = Path.Combine(
        AppContext.BaseDirectory, "PostmanCollections",
        "InventoryManagement_API_Tests.postman_collection.json");

    private static readonly string EnvironmentPath = Path.Combine(
        AppContext.BaseDirectory, "PostmanCollections",
        "InventoryManagement_API_Tests.postman_environment.json");

    [TestMethod]
    public async Task RunPostmanCollection_AllTestsPass()
    {
        if (Environment.GetEnvironmentVariable("SKIP_POSTMAN_TESTS") == "true")
        {
            Assert.Inconclusive("SKIP_POSTMAN_TESTS is set — skipping Postman/Newman tests.");
            return;
        }

        // Check if newman is available before running.
        if (!IsNewmanInstalled())
        {
            Assert.Inconclusive(
                "Newman CLI is not installed or not on PATH. Install via: npm install -g newman");
            return;
        }

        Assert.IsTrue(File.Exists(CollectionPath),
            $"Postman collection not found at: {CollectionPath}");
        Assert.IsTrue(File.Exists(EnvironmentPath),
            $"Postman environment not found at: {EnvironmentPath}");

        // Start the application on the URL the Postman collection expects.
        await using var factory = new PostmanTestServerFactory();
        factory.EnsureStarted();

        // Build the newman command. Use --reporters cli,json so we can parse results.
        string jsonReportPath = Path.Combine(Path.GetTempPath(), $"newman_report_{Guid.NewGuid():N}.json");

        // On Windows, newman is installed as newman.cmd; UseShellExecute=false
        // cannot resolve .cmd files, so we launch through cmd.exe /c.
        string newmanArgs = string.Join(" ",
            "run", $"\"{CollectionPath}\"",
            "-e", $"\"{EnvironmentPath}\"",
            "--reporters", "cli,json",
            "--reporter-json-export", $"\"{jsonReportPath}\"",
            "--insecure");

        bool isWindows = System.Runtime.InteropServices.RuntimeInformation
            .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

        ProcessStartInfo psi = new()
        {
            FileName = isWindows ? "cmd.exe" : "newman",
            Arguments = isWindows ? $"/c newman {newmanArgs}" : newmanArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using Process process = new() { StartInfo = psi };
        process.Start();

        string stdout = await process.StandardOutput.ReadToEndAsync();
        string stderr = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        // Parse the JSON report to extract assertion results.
        if (File.Exists(jsonReportPath))
        {
            string json = await File.ReadAllTextAsync(jsonReportPath);
            ParseAndAssertNewmanResults(json);

            // Cleanup temp file.
            try { File.Delete(jsonReportPath); } catch { /* best effort */ }
        }
        else
        {
            // If no JSON report was produced, fall back to exit code.
            Assert.AreEqual(0, process.ExitCode,
                $"Newman exited with code {process.ExitCode}.\nstdout:\n{stdout}\nstderr:\n{stderr}");
        }
    }

    /// <summary>
    /// Parses a Newman JSON report and asserts that all Postman tests passed.
    /// Collects all failures into a single descriptive assertion message.
    /// </summary>
    private static void ParseAndAssertNewmanResults(string jsonReport)
    {
        using JsonDocument doc = JsonDocument.Parse(jsonReport);

        // Navigate: run.stats.assertions
        if (!doc.RootElement.TryGetProperty("run", out JsonElement run))
        {
            Assert.Fail("Newman report missing 'run' property.");
            return;
        }

        if (run.TryGetProperty("stats", out JsonElement stats) &&
            stats.TryGetProperty("assertions", out JsonElement assertions))
        {
            int total = assertions.GetProperty("total").GetInt32();
            int failed = assertions.GetProperty("failed").GetInt32();

            if (failed > 0)
            {
                // Collect failure details from run.failures array.
                List<string> failureMessages = [];

                if (run.TryGetProperty("failures", out JsonElement failures))
                {
                    foreach (JsonElement failure in failures.EnumerateArray())
                    {
                        string source = failure.TryGetProperty("source", out JsonElement src) &&
                                        src.TryGetProperty("name", out JsonElement srcName)
                            ? srcName.GetString() ?? "?"
                            : "?";

                        string error = failure.TryGetProperty("error", out JsonElement err) &&
                                       err.TryGetProperty("message", out JsonElement errMsg)
                            ? errMsg.GetString() ?? "?"
                            : "?";

                        failureMessages.Add($"  [{source}] {error}");
                    }
                }

                string summary = string.Join(Environment.NewLine, failureMessages);
                Assert.Fail(
                    $"Postman collection: {failed}/{total} assertion(s) failed.\n{summary}");
            }
            else
            {
                // All passed — log for visibility.
                Console.WriteLine($"Postman collection: {total} assertion(s) passed.");
            }
        }
    }

    /// <summary>
    /// Returns true if newman is available on the system PATH.
    /// </summary>
    private static bool IsNewmanInstalled()
    {
        try
        {
            bool isWindows = System.Runtime.InteropServices.RuntimeInformation
                .IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows);

            ProcessStartInfo psi = new()
            {
                FileName = isWindows ? "cmd.exe" : "newman",
                Arguments = isWindows ? "/c newman --version" : "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = new() { StartInfo = psi };
            process.Start();
            process.WaitForExit(10_000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// A <see cref="WebApplicationFactory{TEntryPoint}"/> that starts a real
    /// Kestrel server on <c>https://localhost:5001</c> instead of the in-memory
    /// <c>TestServer</c>, so that external tools like Newman can reach it.
    /// </summary>
    private sealed class PostmanTestServerFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // The base factory adds UseTestServer(). We add Kestrel after it so
            // the real server implementation wins in DI resolution.
            builder.ConfigureWebHost(whb =>
            {
                whb.UseKestrel();
                whb.UseUrls("https://localhost:5001");
            });

            var host = builder.Build();
            host.Start();
            return host;
        }

        /// <summary>
        /// Ensures the Kestrel host is created and started. Must be called before
        /// Newman runs. Because we replaced <c>TestServer</c> with Kestrel, the
        /// factory's internal cast to <c>TestServer</c> throws; the host is already
        /// running at that point, so we catch and ignore the expected exception.
        /// </summary>
        public void EnsureStarted()
        {
            try { _ = Services; }
            catch (InvalidCastException) { }
        }
    }
}
