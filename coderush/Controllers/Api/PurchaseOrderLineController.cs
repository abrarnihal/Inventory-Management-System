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
    [Route("api/PurchaseOrderLine")]
    public class PurchaseOrderLineController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/PurchaseOrderLine
        [HttpGet]
        public async Task<IActionResult> GetPurchaseOrderLine()
        {
            StringValues headers = Request.Headers["PurchaseOrderId"];
            int purchaseOrderId = Convert.ToInt32(headers);
            List<PurchaseOrderLine> Items = await _context.PurchaseOrderLine
                .Where(x => x.PurchaseOrderId.Equals(purchaseOrderId))
                .ToListAsync();
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        private static PurchaseOrderLine Recalculate(PurchaseOrderLine purchaseOrderLine)
        {
            purchaseOrderLine.Amount = purchaseOrderLine.Quantity * purchaseOrderLine.Price;
            purchaseOrderLine.DiscountAmount = (purchaseOrderLine.DiscountPercentage * purchaseOrderLine.Amount) / 100.0;
            purchaseOrderLine.SubTotal = purchaseOrderLine.Amount - purchaseOrderLine.DiscountAmount;
            purchaseOrderLine.TaxAmount = (purchaseOrderLine.TaxPercentage * purchaseOrderLine.SubTotal) / 100.0;
            purchaseOrderLine.Total = purchaseOrderLine.SubTotal + purchaseOrderLine.TaxAmount;

            return purchaseOrderLine;
        }

        private void UpdatePurchaseOrder(int purchaseOrderId)
        {
            PurchaseOrder purchaseOrder = new();
            purchaseOrder = _context.PurchaseOrder
                .Where(x => x.PurchaseOrderId.Equals(purchaseOrderId))
                .FirstOrDefault();

            if (purchaseOrder != null)
            {
                List<PurchaseOrderLine> lines = [];
                lines = [.. _context.PurchaseOrderLine.Where(x => x.PurchaseOrderId.Equals(purchaseOrderId))];

                //update master data by its lines
                purchaseOrder.Amount = lines.Sum(x => x.Amount);
                purchaseOrder.SubTotal = lines.Sum(x => x.SubTotal);

                purchaseOrder.Discount = lines.Sum(x => x.DiscountAmount);
                purchaseOrder.Tax = lines.Sum(x => x.TaxAmount);

                purchaseOrder.Total = purchaseOrder.Freight + lines.Sum(x => x.Total);

                _context.Update(purchaseOrder);

                _context.SaveChanges();
            }
        }


        [HttpPost("[action]")]
        public IActionResult Insert([FromBody] CrudViewModel<PurchaseOrderLine> payload)
        {
            PurchaseOrderLine purchaseOrderLine = payload.value;
            purchaseOrderLine = Recalculate(purchaseOrderLine);
            _context.PurchaseOrderLine.Add(purchaseOrderLine);
            _context.SaveChanges();
            this.UpdatePurchaseOrder(purchaseOrderLine.PurchaseOrderId);
            return Ok(purchaseOrderLine);
        }

        [HttpPost("[action]")]
        public IActionResult Update([FromBody] CrudViewModel<PurchaseOrderLine> payload)
        {
            PurchaseOrderLine purchaseOrderLine = payload.value;
            purchaseOrderLine = Recalculate(purchaseOrderLine);
            _context.PurchaseOrderLine.Update(purchaseOrderLine);
            _context.SaveChanges();
            this.UpdatePurchaseOrder(purchaseOrderLine.PurchaseOrderId);
            return Ok(purchaseOrderLine);
        }

        [HttpPost("[action]")]
        public IActionResult Remove([FromBody] CrudViewModel<PurchaseOrderLine> payload)
        {
            PurchaseOrderLine purchaseOrderLine = _context.PurchaseOrderLine
                .Where(x => x.PurchaseOrderLineId == Convert.ToInt32(payload.key))
                .FirstOrDefault();
            _context.PurchaseOrderLine.Remove(purchaseOrderLine);
            _context.SaveChanges();
            this.UpdatePurchaseOrder(purchaseOrderLine.PurchaseOrderId);
            return Ok(purchaseOrderLine);

        }
    }
}