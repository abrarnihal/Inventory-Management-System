using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Controllers.Api
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/SalesOrderLine")]
    public class SalesOrderLineController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/SalesOrderLine
        [HttpGet]
        public async Task<IActionResult> GetSalesOrderLine()
        {
            StringValues headers = Request.Headers["SalesOrderId"];
            int salesOrderId = Convert.ToInt32(headers);
            List<SalesOrderLine> Items = await _context.SalesOrderLine
                .Where(x => x.SalesOrderId.Equals(salesOrderId))
                .ToListAsync();
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetSalesOrderLineByShipmentId()
        {
            StringValues headers = Request.Headers["ShipmentId"];
            int shipmentId = Convert.ToInt32(headers);
            Shipment shipment = await _context.Shipment.SingleOrDefaultAsync(x => x.ShipmentId.Equals(shipmentId));
            List<SalesOrderLine> Items = [];
            if (shipment != null)
            {
                int salesOrderId = shipment.SalesOrderId;
                Items = await _context.SalesOrderLine
                    .Where(x => x.SalesOrderId.Equals(salesOrderId))
                    .ToListAsync();
            }
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetSalesOrderLineByInvoiceId()
        {
            StringValues headers = Request.Headers["InvoiceId"];
            int invoiceId = Convert.ToInt32(headers);
            Invoice invoice = await _context.Invoice.SingleOrDefaultAsync(x => x.InvoiceId.Equals(invoiceId));
            List<SalesOrderLine> Items = [];
            if (invoice != null)
            {
                int shipmentId = invoice.ShipmentId;
                Shipment shipment = await _context.Shipment.SingleOrDefaultAsync(x => x.ShipmentId.Equals(shipmentId));
                if (shipment != null)
                {
                    int salesOrderId = shipment.SalesOrderId;
                    Items = await _context.SalesOrderLine
                        .Where(x => x.SalesOrderId.Equals(salesOrderId))
                        .ToListAsync();
                }
            }
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        private static SalesOrderLine Recalculate(SalesOrderLine salesOrderLine)
        {
            salesOrderLine.Amount = salesOrderLine.Quantity * salesOrderLine.Price;
            salesOrderLine.DiscountAmount = (salesOrderLine.DiscountPercentage * salesOrderLine.Amount) / 100.0;
            salesOrderLine.SubTotal = salesOrderLine.Amount - salesOrderLine.DiscountAmount;
            salesOrderLine.TaxAmount = (salesOrderLine.TaxPercentage * salesOrderLine.SubTotal) / 100.0;
            salesOrderLine.Total = salesOrderLine.SubTotal + salesOrderLine.TaxAmount;

            return salesOrderLine;
        }

        private void UpdateSalesOrder(int salesOrderId)
        {
            SalesOrder salesOrder = new();
            salesOrder = _context.SalesOrder
                .Where(x => x.SalesOrderId.Equals(salesOrderId))
                .FirstOrDefault();

            if (salesOrder != null)
            {
                List<SalesOrderLine> lines = [];
                lines = [.. _context.SalesOrderLine.Where(x => x.SalesOrderId.Equals(salesOrderId))];

                //update master data by its lines
                salesOrder.Amount = lines.Sum(x => x.Amount);
                salesOrder.SubTotal = lines.Sum(x => x.SubTotal);

                salesOrder.Discount = lines.Sum(x => x.DiscountAmount);
                salesOrder.Tax = lines.Sum(x => x.TaxAmount);

                salesOrder.Total = salesOrder.Freight + lines.Sum(x => x.Total);

                _context.Update(salesOrder);

                _context.SaveChanges();
            }
        }

        [HttpPost("[action]")]
        public IActionResult Insert([FromBody] CrudViewModel<SalesOrderLine> payload)
        {
            SalesOrderLine salesOrderLine = payload.value;
            salesOrderLine = Recalculate(salesOrderLine);
            _context.SalesOrderLine.Add(salesOrderLine);
            _context.SaveChanges();
            this.UpdateSalesOrder(salesOrderLine.SalesOrderId);
            return Ok(salesOrderLine);
        }

        [HttpPost("[action]")]
        public IActionResult Update([FromBody] CrudViewModel<SalesOrderLine> payload)
        {
            SalesOrderLine salesOrderLine = payload.value;
            salesOrderLine = Recalculate(salesOrderLine);
            _context.SalesOrderLine.Update(salesOrderLine);
            _context.SaveChanges();
            this.UpdateSalesOrder(salesOrderLine.SalesOrderId);
            return Ok(salesOrderLine);
        }

        [HttpPost("[action]")]
        public IActionResult Remove([FromBody] CrudViewModel<SalesOrderLine> payload)
        {
            SalesOrderLine salesOrderLine = _context.SalesOrderLine
                .Where(x => x.SalesOrderLineId == Convert.ToInt32(payload.key))
                .FirstOrDefault();
            _context.SalesOrderLine.Remove(salesOrderLine);
            _context.SaveChanges();
            this.UpdateSalesOrder(salesOrderLine.SalesOrderId);
            return Ok(salesOrderLine);

        }
    }
}