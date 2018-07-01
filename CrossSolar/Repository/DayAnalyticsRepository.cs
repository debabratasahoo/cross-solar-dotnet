using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CrossSolar.Domain;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.EntityFrameworkCore;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace CrossSolar.Repository
{
    public class DayAnalyticsRepository : GenericRepository<OneDayElectricityModel>, IDayAnalyticsRepository
    {
        public DayAnalyticsRepository(CrossSolarDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<List<OneDayElectricityModel>> HistoricalData(string panelId)
        {
            var data = await _dbContext.OneHourElectricitys
                .Where(g => g.PanelId == panelId && g.DateTime.Date < DateTime.Today).ToListAsync();
            var result = new List<OneDayElectricityModel>();
            Parallel.ForEach(data.GroupBy(g => g.DateTime.Date), g =>
            {
                var obj = new OneDayElectricityModel
                {
                    DateTime = g.Key,
                    Average = g.Average(g1 => g1.KiloWatt),
                    Minimum = g.Min(g1 => g1.KiloWatt),
                    Maximum = g.Max(g1 => g1.KiloWatt),
                    Sum = g.Sum(g1 => g1.KiloWatt)
                };
                result.Add(obj);
            });
            return result.OrderBy(g => g.DateTime).ToList();
        }


    }
}