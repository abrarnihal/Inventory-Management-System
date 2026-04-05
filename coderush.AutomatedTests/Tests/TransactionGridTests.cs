using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.AutomatedTests.Tests;

/// <summary>
/// Verifies that all transaction-related pages (GRN, Shipment, Bill,
/// Invoice, PaymentVoucher, PaymentReceive) render their grids.
/// </summary>
[TestClass]
public sealed class TransactionGridTests : SeleniumTestBase
{
    [TestInitialize]
    public void EnsureAuth() => EnsureLoggedIn();

    [TestMethod] public void GoodsReceivedNoteGrid_Loads() => AssertGridLoads("/GoodsReceivedNote/Index");
    [TestMethod] public void ShipmentGrid_Loads() => AssertGridLoads("/Shipment/Index");
    [TestMethod] public void BillGrid_Loads() => AssertGridLoads("/Bill/Index");
    [TestMethod] public void InvoiceGrid_Loads() => AssertGridLoads("/Invoice/Index");
    [TestMethod] public void PaymentVoucherGrid_Loads() => AssertGridLoads("/PaymentVoucher/Index");
    [TestMethod] public void PaymentReceiveGrid_Loads() => AssertGridLoads("/PaymentReceive/Index");

    private void AssertGridLoads(string path)
    {
        NavigateTo(path);
        WaitForGrid();

        var grid = Driver.FindElements(By.CssSelector(".e-grid"));
        Assert.IsTrue(grid.Count > 0,
            $"Syncfusion grid not found on {path}.");
    }
}
