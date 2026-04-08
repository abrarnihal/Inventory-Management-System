using coderush.E2ETests.Infrastructure;
using coderush.E2ETests.Infrastructure.PageObjects;

namespace coderush.E2ETests.Tests;

/// <summary>
/// End-to-end tests for the full Purchase Cycle:
///   Purchase Order → Goods Received Note → Bill → Payment Voucher
/// </summary>
[TestClass]
public sealed class PurchaseCycleE2ETests : E2ETestBase
{
    private SyncfusionGridPage _poPage = null!;
    private SyncfusionGridPage _grnPage = null!;
    private SyncfusionGridPage _billPage = null!;
    private SyncfusionGridPage _pvPage = null!;

    [TestInitialize]
    public void Setup()
    {
        EnsureLoggedIn();
        _poPage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/PurchaseOrder/Index");
        _grnPage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/GoodsReceivedNote/Index");
        _billPage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/Bill/Index");
        _pvPage = new SyncfusionGridPage(Driver, Wait, BaseUrl, "/PaymentVoucher/Index");
    }

    [TestMethod]
    public void E2E_PurchaseCycle_CreatePO_ReceiveGoods_Bill_PayVoucher()
    {
        // ── Step 1: Create a Purchase Order ──
        int initialPOCount = GetApiCount("/api/PurchaseOrder");

        PostApiRecord("/api/PurchaseOrder/Insert", """
            {"value":{"PurchaseOrderDate":"2026-03-31","DeliveryDate":"2026-04-07","VendorId":1,"PurchaseTypeId":1,"BranchId":1,"CurrencyId":1,"Description":"E2E Purchase Order","Freight":15.00}}
            """);

        Assert.AreEqual(initialPOCount + 1, GetApiCount("/api/PurchaseOrder"),
            "Purchase Order should be created.");

        _poPage.Navigate();
        Assert.IsTrue(_poPage.GetRowCount() > 0,
            "Purchase Order grid should contain at least one row.");

        // ── Step 2: Receive goods (GRN) against the PO ──
        int poId = GetLatestEntityId("/api/PurchaseOrder", "PurchaseOrderId");
        int initialGRNCount = GetApiCount("/api/GoodsReceivedNote");

        PostApiRecord("/api/GoodsReceivedNote/Insert",
            $$$"""{"value":{"GRNDate":"2026-04-01","PurchaseOrderId":{{{poId}}},"WarehouseId":1,"Description":"E2E GRN"}}""");

        Assert.AreEqual(initialGRNCount + 1, GetApiCount("/api/GoodsReceivedNote"),
            "GRN should be created.");

        _grnPage.Navigate();
        Assert.IsTrue(_grnPage.GetRowCount() > 0,
            "GRN grid should contain at least one row.");

        // ── Step 3: Create a Bill linked to the GRN ──
        int grnId = GetLatestEntityId("/api/GoodsReceivedNote", "GoodsReceivedNoteId");
        int initialBillCount = GetApiCount("/api/Bill");

        PostApiRecord("/api/Bill/Insert",
            $$$"""{"value":{"BillDate":"2026-04-02","GoodsReceivedNoteId":{{{grnId}}},"BillTypeId":1,"Description":"E2E Bill"}}""");

        Assert.AreEqual(initialBillCount + 1, GetApiCount("/api/Bill"),
            "Bill should be created.");

        _billPage.Navigate();
        Assert.IsTrue(_billPage.GetRowCount() > 0,
            "Bill grid should contain at least one row.");

        // ── Step 4: Pay the bill via Payment Voucher ──
        int billId = GetLatestEntityId("/api/Bill", "BillId");
        int initialPVCount = GetApiCount("/api/PaymentVoucher");

        PostApiRecord("/api/PaymentVoucher/Insert",
            $$$"""{"value":{"PaymentDate":"2026-04-03","BillId":{{{billId}}},"PaymentTypeId":1,"PaymentAmount":15.00,"CashBankId":1,"Description":"E2E Voucher"}}""");

        Assert.AreEqual(initialPVCount + 1, GetApiCount("/api/PaymentVoucher"),
            "Payment Voucher should be created.");

        _pvPage.Navigate();
        Assert.IsTrue(_pvPage.GetRowCount() > 0,
            "Payment Voucher grid should contain at least one row.");
    }

    [TestMethod]
    public void E2E_PurchaseCycle_PONotReceivedYet_AvailableForGRN()
    {
        PostApiRecord("/api/PurchaseOrder/Insert", """
            {"value":{"PurchaseOrderDate":"2026-03-31","DeliveryDate":"2026-04-14","VendorId":2,"PurchaseTypeId":1,"BranchId":1,"CurrencyId":1,"Description":"Unreceived E2E PO","Freight":0}}
            """);

        string json = GetApiJson("/api/PurchaseOrder/GetNotReceivedYet");
        Assert.IsTrue(json.Contains("Unreceived E2E PO"),
            "Unreceived PO should appear in GetNotReceivedYet.");
    }

    [TestMethod]
    public void E2E_PurchaseCycle_BillNotPaidYet_AvailableForVoucher()
    {
        // PO → GRN → Bill (no payment)
        PostApiRecord("/api/PurchaseOrder/Insert", """
            {"value":{"PurchaseOrderDate":"2026-03-31","DeliveryDate":"2026-04-14","VendorId":3,"PurchaseTypeId":1,"BranchId":1,"CurrencyId":1,"Description":"Unpaid bill E2E PO","Freight":0}}
            """);
        int poId = GetLatestEntityId("/api/PurchaseOrder", "PurchaseOrderId");

        PostApiRecord("/api/GoodsReceivedNote/Insert",
            $$$"""{"value":{"GRNDate":"2026-04-01","PurchaseOrderId":{{{poId}}},"WarehouseId":1,"Description":"GRN for unpaid bill"}}""");
        int grnId = GetLatestEntityId("/api/GoodsReceivedNote", "GoodsReceivedNoteId");

        PostApiRecord("/api/Bill/Insert",
            $$$"""{"value":{"BillDate":"2026-04-02","GoodsReceivedNoteId":{{{grnId}}},"BillTypeId":1,"Description":"Unpaid bill E2E"}}""");

        string json = GetApiJson("/api/Bill/GetNotPaidYet");
        Assert.IsTrue(json.Contains("Unpaid bill E2E"),
            "Unpaid bill should appear in GetNotPaidYet.");
    }
}
