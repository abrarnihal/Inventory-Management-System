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
    [Route("api/Bill")]
    public class BillController(ApplicationDbContext context,
                    INumberSequence numberSequence) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly INumberSequence _numberSequence = numberSequence;

        // GET: api/Bill
        [HttpGet]
        public async Task<IActionResult> GetBill()
        {
            List<Bill> Items = await _context.Bill.ToListAsync();
            int Count = Items.Count;
            return Ok(new { Items, Count });
        }

        [HttpGet("[action]")]
        public async Task<IActionResult> GetNotPaidYet()
        {
            List<Bill> bills = [];
            List<PaymentVoucher> vouchers = [];
            vouchers = await _context.PaymentVoucher.ToListAsync();
            List<int> ids = [];

            foreach (PaymentVoucher item in vouchers)
            {
                ids.Add(item.BillId);
            }

            bills = await _context.Bill
                .Where(x => !ids.Contains(x.BillId))
                .ToListAsync();
            return Ok(bills);
        }

        [HttpPost("[action]")]
        public IActionResult Insert([FromBody] CrudViewModel<Bill> payload)
        {
            if (payload?.value == null) return BadRequest();
            Bill bill = payload.value;
            bill.BillName = _numberSequence.GetNumberSequence("BILL");
            _context.Bill.Add(bill);
            _context.SaveChanges();
            return Ok(bill);
        }

        [HttpPost("[action]")]
        public IActionResult Update([FromBody] CrudViewModel<Bill> payload)
        {
            Bill bill = payload.value;
            _context.Bill.Update(bill);
            _context.SaveChanges();
            return Ok(bill);
        }

        [HttpPost("[action]")]
        public IActionResult Remove([FromBody] CrudViewModel<Bill> payload)
        {
            Bill bill = _context.Bill
                .Where(x => x.BillId == Convert.ToInt32(payload.key))
                .FirstOrDefault();
            _context.Bill.Remove(bill);
            _context.SaveChanges();
            return Ok(bill);

        }
    }
}