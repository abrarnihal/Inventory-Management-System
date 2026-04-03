using coderush.Data;
using coderush.Models;
using coderush.Models.SyncfusionViewModels;
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
    [Route("api/Vendor")]
    public class VendorController(ApplicationDbContext context) : Controller
    {
        private readonly ApplicationDbContext _context = context;

        // GET: api/Vendor
        [HttpGet]
        public async Task<IActionResult> GetVendor()
        {
            var query = _context.Vendor.AsNoTracking();
            int Count = await query.CountAsync();

            if (int.TryParse(Request.Query["$skip"], out int skip))
                query = query.Skip(skip);
            if (int.TryParse(Request.Query["$top"], out int top))
                query = query.Take(top);

            List<Vendor> Items = await query.ToListAsync();
            return Ok(new { Items, Count });
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Insert([FromBody] CrudViewModel<Vendor> payload)
        {
            Vendor vendor = payload.value;
            _context.Vendor.Add(vendor);
            await _context.SaveChangesAsync();
            return Ok(vendor);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Update([FromBody] CrudViewModel<Vendor> payload)
        {
            Vendor vendor = payload.value;
            _context.Vendor.Update(vendor);
            await _context.SaveChangesAsync();
            return Ok(vendor);
        }

        [HttpPost("[action]")]
        public async Task<IActionResult> Remove([FromBody] CrudViewModel<Vendor> payload)
        {
            Vendor vendor = await _context.Vendor.FindAsync(Convert.ToInt32(payload.key));
            _context.Vendor.Remove(vendor);
            await _context.SaveChangesAsync();
            return Ok(vendor);
        }
    }
}