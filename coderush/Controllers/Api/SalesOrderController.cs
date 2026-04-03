using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
using coderush.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace coderush.Controllers.Api
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/SalesOrder")]
    public class SalesOrderController(ApplicationDbContext context,
                    INumberSequence numberSequence) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly INumberSequence _numberSequence = numberSequence;

        // GET: api/SalesOrder
        [HttpGet]
        public async Task<IActionResult> GetSalesOrder()
        {
            List<SalesOrder> Items = await _context.SalesOrder.ToListAsync();
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetNotShippedYet()
        {
            List<SalesOrder> salesOrders = [];
            List<Shipment> shipments = [];
            shipments = await _context.Shipment.ToListAsync();
            List<int> ids = [];

            foreach (Shipment item in shipments)
            {
                ids.Add(item.SalesOrderId);
            }

            salesOrders = await _context.SalesOrder
                .Where(x => !ids.Contains(x.SalesOrderId))
                .ToListAsync();
            return Ok(salesOrders);
        }

        [HttpGet("[action]/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            SalesOrder result = await _context.SalesOrder.FindAsync(id);
            if (result != null)
            {
                await _context.Entry(result).Collection(x => x.SalesOrderLines).LoadAsync();
            }

            return Ok(result);
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

                // update master data by its lines
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
        public IActionResult Insert([FromBody] CrudViewModel<SalesOrder> payload)
        {
            if (payload?.value == null) return BadRequest();
            SalesOrder salesOrder = payload.value;
            salesOrder.SalesOrderName = _numberSequence.GetNumberSequence("SO");
            _context.SalesOrder.Add(salesOrder);
            _context.SaveChanges();
            this.UpdateSalesOrder(salesOrder.SalesOrderId);
            return Ok(salesOrder);
        }

        [HttpPost("[action]")]
        public IActionResult Update([FromBody] CrudViewModel<SalesOrder> payload)
        {
            SalesOrder salesOrder = payload.value;
            _context.SalesOrder.Update(salesOrder);
            _context.SaveChanges();
            return Ok(salesOrder);
        }

        [HttpPost("[action]")]
        public IActionResult Remove([FromBody] CrudViewModel<SalesOrder> payload)
        {
            SalesOrder salesOrder = _context.SalesOrder
                .Where(x => x.SalesOrderId == Convert.ToInt32(payload.key))
                .FirstOrDefault();
            _context.SalesOrder.Remove(salesOrder);
            _context.SaveChanges();
            return Ok(salesOrder);

        }
    }
}