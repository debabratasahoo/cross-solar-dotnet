using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrossSolar.Controllers
{
    [Route("panel")]
    public class AnalyticsController : Controller
    {
        private readonly IAnalyticsRepository _analyticsRepository;
        private readonly IDayAnalyticsRepository _dayAnalyticsRepository;

        private readonly IPanelRepository _panelRepository;

        public AnalyticsController(IAnalyticsRepository analyticsRepository, IPanelRepository panelRepository, IDayAnalyticsRepository dayAnalyticsRepository)
        {
            _analyticsRepository = analyticsRepository;
            _panelRepository = panelRepository;
            _dayAnalyticsRepository = dayAnalyticsRepository;
        }

        // GET panel/XXXX1111YYYY2222/analytics
        [HttpGet("{panelId}/[controller]")]
        public async Task<IActionResult> Get([FromRoute] string panelId)
        {    
                var panel = await _panelRepository.Query()
                    .FirstOrDefaultAsync(x => x.Id == Convert.ToInt32(panelId));

                if (panel == null) return NotFound();

                var analytics = await _analyticsRepository.Query()
                    .Where(x => x.PanelId.Equals(panelId, StringComparison.CurrentCultureIgnoreCase)).ToListAsync();

                var result = new OneHourElectricityListModel
                {
                    OneHourElectricitys = analytics.Select(c => new OneHourElectricityModel
                    {
                        Id = c.Id,
                        KiloWatt = c.KiloWatt,
                        DateTime = c.DateTime
                    })
                };
                return Ok(result);           
        }

        // GET panel/XXXX1111YYYY2222/analytics/day
        [HttpGet("{panelId}/[controller]/day")]
        public async Task<IActionResult> DayResults([FromRoute] string panelId)
        {
            if (panelId == String.Empty) return BadRequest(ModelState);
            
                var result = await _dayAnalyticsRepository.HistoricalData(panelId);
                return Ok(result);
        }

        // POST panel/XXXX1111YYYY2222/analytics
        [HttpPost("{panelId}/[controller]")]
        public async Task<IActionResult> Post([FromRoute] string panelId, [FromBody] OneHourElectricityModel value)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var oneHourElectricityContent = new OneHourElectricity
            {
                PanelId = panelId,
                KiloWatt = value.KiloWatt,
                DateTime = DateTime.UtcNow
            };

            await _analyticsRepository.InsertAsync(oneHourElectricityContent);

            var result = new OneHourElectricityModel
            {
                Id = oneHourElectricityContent.Id,
                KiloWatt = oneHourElectricityContent.KiloWatt,
                DateTime = oneHourElectricityContent.DateTime
            };

            return Created($"panel/{panelId}/analytics/{result.Id}", result);
        }
    }
}