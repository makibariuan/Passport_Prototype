using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SeniorCitizen.Server.Data;
using System.Linq;

namespace SeniorCitizen.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PSGCController : ControllerBase
    {
        private readonly PsgcDbContext _context;

        public PSGCController(PsgcDbContext context)
        {
            _context = context;
        }

        // GET: api/psgc/regions
        [HttpGet("regions")]
        public async Task<IActionResult> GetRegions()
        {
            var regions = await _context.Regions
                .Include(r => r.Provinces)
                .OrderBy(r => r.RegionCode)
                .ToListAsync();

            return Ok(regions);
        }

        // GET: api/psgc/provinces/{regionCode}
        [HttpGet("provinces/{regionCode}")]
        public async Task<IActionResult> GetProvinces(string regionCode)
        {
            var provinces = await _context.Provinces
                .Where(p => p.RegionCode == regionCode)
                .OrderBy(p => p.ProvinceName)
                .ToListAsync();

            return Ok(provinces);
        }

        // GET: api/psgc/cities/{provinceCode}
        [HttpGet("cities/{provinceCode}")]
        public async Task<IActionResult> GetCities(string provinceCode)
        {
            var cities = await _context.Cities
                .Where(c => c.ProvinceCode == provinceCode)
                .OrderBy(c => c.CityMunName)
                .ToListAsync();

            return Ok(cities);
        }

        // GET: api/psgc/barangays/{cityMunCode}
        [HttpGet("barangays/{cityMunCode}")]
        public async Task<IActionResult> GetBarangays(string cityMunCode)
        {
            var barangays = await _context.Barangays
                .Where(b => b.CityMunCode == cityMunCode)
                .OrderBy(b => b.BarangayName)
                .ToListAsync();

            return Ok(barangays);
        }
    }
}