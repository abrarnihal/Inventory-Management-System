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
    [Route("api/Shipment")]
    public class ShipmentController(ApplicationDbContext context,
                    INumberSequence numberSequence) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly INumberSequence _numberSequence = numberSequence;

        // GET: api/Shipment
        [HttpGet]
        public async Task<IActionResult> GetShipment()
        {
            List<Shipment> Items = await _context.Shipment.ToListAsync();
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetNotInvoicedYet()
        {
            List<Shipment> shipments = [];
            List<Invoice> invoices = [];
            invoices = await _context.Invoice.ToListAsync();
            List<int> ids = [];

            foreach (Invoice item in invoices)
            {
                ids.Add(item.ShipmentId);
            }

            shipments = await _context.Shipment
                .Where(x => !ids.Contains(x.ShipmentId))
                .ToListAsync();
            return Ok(shipments);
        }

        [HttpPost("[action]")]
        public IActionResult Insert([FromBody] CrudViewModel<Shipment> payload)
        {
            if (payload?.value == null) return BadRequest();
            Shipment shipment = payload.value;
            shipment.ShipmentName = _numberSequence.GetNumberSequence("DO");
            _context.Shipment.Add(shipment);
            _context.SaveChanges();
            return Ok(shipment);
        }

        [HttpPost("[action]")]
        public IActionResult Update([FromBody] CrudViewModel<Shipment> payload)
        {
            Shipment shipment = payload.value;
            _context.Shipment.Update(shipment);
            _context.SaveChanges();
            return Ok(shipment);
        }

        [HttpPost("[action]")]
        public IActionResult Remove([FromBody] CrudViewModel<Shipment> payload)
        {
            Shipment shipment = _context.Shipment
                .Where(x => x.ShipmentId == Convert.ToInt32(payload.key))
                .FirstOrDefault();
            _context.Shipment.Remove(shipment);
            _context.SaveChanges();
            return Ok(shipment);

        }
    }
}