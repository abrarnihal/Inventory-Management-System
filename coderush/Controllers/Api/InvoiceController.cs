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
    [Route("api/Invoice")]
    public class InvoiceController(ApplicationDbContext context,
                    INumberSequence numberSequence) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly INumberSequence _numberSequence = numberSequence;

        // GET: api/Invoice
        [HttpGet]
        public async Task<IActionResult> GetInvoice()
        {
            List<Invoice> Items = await _context.Invoice.ToListAsync();
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetNotPaidYet()
        {
            List<Invoice> invoices = [];
            List<PaymentReceive> receives = [];
            receives = await _context.PaymentReceive.ToListAsync();
            List<int> ids = [];

            foreach (PaymentReceive item in receives)
            {
                ids.Add(item.InvoiceId);
            }

            invoices = await _context.Invoice
                .Where(x => !ids.Contains(x.InvoiceId))
                .ToListAsync();
            return Ok(invoices);
        }

        [HttpPost("[action]")]
        public IActionResult Insert([FromBody] CrudViewModel<Invoice> payload)
        {
            if (payload?.value == null) return BadRequest();
            Invoice invoice = payload.value;
            invoice.InvoiceName = _numberSequence.GetNumberSequence("INV");
            _context.Invoice.Add(invoice);
            _context.SaveChanges();
            return Ok(invoice);
        }

        [HttpPost("[action]")]
        public IActionResult Update([FromBody] CrudViewModel<Invoice> payload)
        {
            Invoice invoice = payload.value;
            _context.Invoice.Update(invoice);
            _context.SaveChanges();
            return Ok(invoice);
        }

        [HttpPost("[action]")]
        public IActionResult Remove([FromBody] CrudViewModel<Invoice> payload)
        {
            Invoice invoice = _context.Invoice
                .Where(x => x.InvoiceId == Convert.ToInt32(payload.key))
                .FirstOrDefault();
            _context.Invoice.Remove(invoice);
            _context.SaveChanges();
            return Ok(invoice);

        }
    }
}