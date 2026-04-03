using coderush.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace coderush.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public ApplicationDbContext() : this(new DbContextOptionsBuilder<ApplicationDbContext>().Options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder) => base.OnModelCreating(builder);// Customize the ASP.NET Identity model and override the defaults if needed.// For example, you can rename the ASP.NET Identity table names and more.// Add your customizations after calling base.OnModelCreating(builder);

        public virtual DbSet<coderush.Models.ApplicationUser> ApplicationUser { get; set; }

        public virtual DbSet<coderush.Models.Bill> Bill { get; set; }

        public virtual DbSet<coderush.Models.BillType> BillType { get; set; }

        public virtual DbSet<coderush.Models.Branch> Branch { get; set; }

        public virtual DbSet<coderush.Models.CashBank> CashBank { get; set; }

        public virtual DbSet<coderush.Models.Currency> Currency { get; set; }

        public virtual DbSet<coderush.Models.Customer> Customer { get; set; }

        public virtual DbSet<coderush.Models.CustomerType> CustomerType { get; set; }

        public virtual DbSet<coderush.Models.GoodsReceivedNote> GoodsReceivedNote { get; set; }

        public virtual DbSet<coderush.Models.Invoice> Invoice { get; set; }

        public virtual DbSet<coderush.Models.InvoiceType> InvoiceType { get; set; }

        public virtual DbSet<coderush.Models.NumberSequence> NumberSequence { get; set; }

        public virtual DbSet<coderush.Models.PaymentReceive> PaymentReceive { get; set; }

        public virtual DbSet<coderush.Models.PaymentType> PaymentType { get; set; }

        public virtual DbSet<coderush.Models.PaymentVoucher> PaymentVoucher { get; set; }

        public virtual DbSet<coderush.Models.Product> Product { get; set; }

        public virtual DbSet<coderush.Models.ProductType> ProductType { get; set; }

        public virtual DbSet<coderush.Models.PurchaseOrder> PurchaseOrder { get; set; }

        public virtual DbSet<coderush.Models.PurchaseOrderLine> PurchaseOrderLine { get; set; }

        public virtual DbSet<coderush.Models.PurchaseType> PurchaseType { get; set; }

        public virtual DbSet<coderush.Models.SalesOrder> SalesOrder { get; set; }

        public virtual DbSet<coderush.Models.SalesOrderLine> SalesOrderLine { get; set; }

        public virtual DbSet<coderush.Models.SalesType> SalesType { get; set; }

        public virtual DbSet<coderush.Models.Shipment> Shipment { get; set; }

        public virtual DbSet<coderush.Models.ShipmentType> ShipmentType { get; set; }

        public virtual DbSet<coderush.Models.UnitOfMeasure> UnitOfMeasure { get; set; }

        public virtual DbSet<coderush.Models.Vendor> Vendor { get; set; }

        public virtual DbSet<coderush.Models.VendorType> VendorType { get; set; }

        public virtual DbSet<coderush.Models.Warehouse> Warehouse { get; set; }

        public virtual DbSet<coderush.Models.UserProfile> UserProfile { get; set; }

        public virtual DbSet<coderush.Models.ChatConversation> ChatConversation { get; set; }

        public virtual DbSet<coderush.Models.ChatMessage> ChatMessage { get; set; }
    }
}
