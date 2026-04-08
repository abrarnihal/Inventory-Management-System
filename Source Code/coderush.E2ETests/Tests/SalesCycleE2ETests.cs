using coderush.E2ETests.Infrastructure;
using coderush.E2ETests.Infrastructure.PageObjects;

namespace coderush.E2ETests.Tests;

/// <summary>
/// End-to-end tests for the full Sales Cycle:
///   Sales Order → Shipment → Invoice → Payment Receive
///
/// Each test method exercises a complete business workflow that spans
/// multiple pages and API calls, verifying data flows correctly from
/// order creation through to payment collection.
/// </summary>
[TestClass]
public sealed class SalesCycleE2ETests : E2ETestBase
{
    private SyncfusionGridPage _salesOrderPage = null!;
    private SyncfusionGridPage _shipmentPage = null!;
    private SyncfusionGridPage _invoicePage = null!;
    private SyncfusionGridPage _paymentReceivePage = null!;

    [TestInitialize]
    public void Setup()
    {
        EnsureLoggedIn();
        _salesOrderPage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/SalesOrder/Index");
        _shipmentPage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/Shipment/Index");
        _invoicePage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/Invoice/Index");
        _paymentReceivePage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/PaymentReceive/Index");
    }

    [TestMethod]
    public void E2E_SalesCycle_CreateOrder_Ship_Invoice_ReceivePayment()
    {
        // ── Step 1: Create a Sales Order via the API ──
        int initialSOCount = GetApiCount("/api/SalesOrder");

        PostApiRecord("/api/SalesOrder/Insert", """
            {"value":{"SalesOrderDate":"2026-03-31","DeliveryDate":"2026-04-07","CustomerId":1,"SalesTypeId":1,"BranchId":1,"CurrencyId":1,"Description":"E2E Test Order","Freight":10.00}}
            """);

        int newSOCount = GetApiCount("/api/SalesOrder");
        Assert.AreEqual(initialSOCount + 1, newSOCount, "Sales Order should be created.");

        // ── Step 2: Verify the grid shows the new order ──
        _salesOrderPage.Navigate();
        Assert.IsTrue(_salesOrderPage.GetRowCount() > 0,
            "Sales Order grid should contain at least one row after creation.");

        // ── Step 3: Create a Shipment linked to the SO ──
        int soId = GetLatestEntityId("/api/SalesOrder", "SalesOrderId");
        int initialShipCount = GetApiCount("/api/Shipment");

        PostApiRecord("/api/Shipment/Insert",
            $$$"""{"value":{"ShipmentDate":"2026-04-01","SalesOrderId":{{{soId}}},"ShipmentTypeId":1,"WarehouseId":1,"Description":"E2E Shipment"}}""");

        int newShipCount = GetApiCount("/api/Shipment");
        Assert.AreEqual(initialShipCount + 1, newShipCount, "Shipment should be created.");

        _shipmentPage.Navigate();
        Assert.IsTrue(_shipmentPage.GetRowCount() > 0,
            "Shipment grid should contain at least one row.");

        // ── Step 4: Create an Invoice linked to the Shipment ──
        int shipId = GetLatestEntityId("/api/Shipment", "ShipmentId");
        int initialInvCount = GetApiCount("/api/Invoice");

        PostApiRecord("/api/Invoice/Insert",
            $$$"""{"value":{"InvoiceDate":"2026-04-02","ShipmentId":{{{shipId}}},"InvoiceTypeId":1,"Description":"E2E Invoice"}}""");

        int newInvCount = GetApiCount("/api/Invoice");
        Assert.AreEqual(initialInvCount + 1, newInvCount, "Invoice should be created.");

        _invoicePage.Navigate();
        Assert.IsTrue(_invoicePage.GetRowCount() > 0,
            "Invoice grid should contain at least one row.");

        // ── Step 5: Record payment against the Invoice ──
        int invId = GetLatestEntityId("/api/Invoice", "InvoiceId");
        int initialPayCount = GetApiCount("/api/PaymentReceive");

        PostApiRecord("/api/PaymentReceive/Insert",
            $$$"""{"value":{"PaymentDate":"2026-04-03","InvoiceId":{{{invId}}},"PaymentTypeId":1,"PaymentAmount":10.00,"CashBankId":1,"Description":"E2E Payment"}}""");

        int newPayCount = GetApiCount("/api/PaymentReceive");
        Assert.AreEqual(initialPayCount + 1, newPayCount, "Payment Receive should be created.");

        _paymentReceivePage.Navigate();
        Assert.IsTrue(_paymentReceivePage.GetRowCount() > 0,
            "Payment Receive grid should contain at least one row.");
    }

    [TestMethod]
    public void E2E_SalesCycle_SONotShippedYet_AppearsInDropdown()
    {
        PostApiRecord("/api/SalesOrder/Insert", """
            {"value":{"SalesOrderDate":"2026-03-31","DeliveryDate":"2026-04-14","CustomerId":2,"SalesTypeId":1,"BranchId":1,"CurrencyId":1,"Remarks":"Unshipped E2E Order","Freight":5.00}}
            """);

        string json = GetApiJson("/api/SalesOrder/GetNotShippedYet");
        Assert.IsTrue(json.Contains("Unshipped E2E Order"),
            "Unshipped SO should appear in the GetNotShippedYet list.");
    }

    [TestMethod]
    public void E2E_SalesCycle_InvoiceNotPaidYet_AppearsInDropdown()
    {
        // SO → Shipment → Invoice (no payment)
        PostApiRecord("/api/SalesOrder/Insert", """
            {"value":{"SalesOrderDate":"2026-03-31","DeliveryDate":"2026-04-14","CustomerId":3,"SalesTypeId":1,"BranchId":1,"CurrencyId":1,"Description":"Unpaid chain E2E","Freight":0}}
            """);
        int soId = GetLatestEntityId("/api/SalesOrder", "SalesOrderId");

        PostApiRecord("/api/Shipment/Insert",
            $$$"""{"value":{"ShipmentDate":"2026-04-01","SalesOrderId":{{{soId}}},"ShipmentTypeId":1,"WarehouseId":1,"Description":"Ship for unpaid"}}""");
        int shipId = GetLatestEntityId("/api/Shipment", "ShipmentId");

        PostApiRecord("/api/Invoice/Insert",
            $$$"""{"value":{"InvoiceDate":"2026-04-02","ShipmentId":{{{shipId}}},"InvoiceTypeId":1,"Description":"Unpaid invoice E2E"}}""");

        string json = GetApiJson("/api/Invoice/GetNotPaidYet");
        Assert.IsTrue(json.Contains("Unpaid invoice E2E"),
            "Unpaid invoice should appear in GetNotPaidYet.");
    }
}
