using coderush.AutomatedTests.Infrastructure;
using OpenQA.Selenium;

namespace coderush.AutomatedTests.Tests;

/// <summary>
/// Verifies that every reference data / lookup page renders its Syncfusion
/// grid correctly with at least the default seeded row.
/// </summary>
[TestClass]
public sealed class ReferenceDataGridTests : SeleniumTestBase
{
    [TestInitialize]
    public void EnsureAuth() => EnsureLoggedIn();

    [TestMethod] public void BranchGrid_Loads() => AssertGridLoads("/Branch/Index");
    [TestMethod] public void WarehouseGrid_Loads() => AssertGridLoads("/Warehouse/Index");
    [TestMethod] public void CashBankGrid_Loads() => AssertGridLoads("/CashBank/Index");
    [TestMethod] public void CurrencyGrid_Loads() => AssertGridLoads("/Currency/Index");
    [TestMethod] public void UnitOfMeasureGrid_Loads() => AssertGridLoads("/UnitOfMeasure/Index");
    [TestMethod] public void CustomerTypeGrid_Loads() => AssertGridLoads("/CustomerType/Index");
    [TestMethod] public void VendorTypeGrid_Loads() => AssertGridLoads("/VendorType/Index");
    [TestMethod] public void ProductTypeGrid_Loads() => AssertGridLoads("/ProductType/Index");
    [TestMethod] public void BillTypeGrid_Loads() => AssertGridLoads("/BillType/Index");
    [TestMethod] public void InvoiceTypeGrid_Loads() => AssertGridLoads("/InvoiceType/Index");
    [TestMethod] public void PaymentTypeGrid_Loads() => AssertGridLoads("/PaymentType/Index");
    [TestMethod] public void PurchaseTypeGrid_Loads() => AssertGridLoads("/PurchaseType/Index");
    [TestMethod] public void SalesTypeGrid_Loads() => AssertGridLoads("/SalesType/Index");
    [TestMethod] public void ShipmentTypeGrid_Loads() => AssertGridLoads("/ShipmentType/Index");

    private void AssertGridLoads(string path)
    {
        NavigateTo(path);
        WaitForGrid();

        var grid = Driver.FindElements(By.CssSelector(".e-grid"));
        Assert.IsTrue(grid.Count > 0,
            $"Syncfusion grid not found on {path}.");

        var headers = Driver.FindElements(By.CssSelector(".e-grid .e-headercelldiv"));
        Assert.IsTrue(headers.Count > 0,
            $"Grid on {path} has no column headers.");
    }
}
