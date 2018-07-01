using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using CrossSolar.Controllers;
using CrossSolar.Domain;
using CrossSolar.Models;
using CrossSolar.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Moq;
using Xunit;

namespace CrossSolar.Tests.Controller
{
    public class AnalyticsControllerTests
    {
        public AnalyticsControllerTests()
        {
            _analyticsController = new AnalyticsController(_analyticsRepositoryMock.Object, _panelRepositoryMock.Object, _dayAnalyticsRepositoryMock.Object);
        }

        private readonly AnalyticsController _analyticsController;

        private readonly Mock<IAnalyticsRepository> _analyticsRepositoryMock = new Mock<IAnalyticsRepository>();

        private readonly Mock<IDayAnalyticsRepository> _dayAnalyticsRepositoryMock = new Mock<IDayAnalyticsRepository>();

        private readonly Mock<IPanelRepository> _panelRepositoryMock = new Mock<IPanelRepository>();
        private readonly Mock<IGenericRepository<PanelModel>> _genericRepositoryMock = new Mock<IGenericRepository<PanelModel>>();

        [Fact]
        public async Task DayResults_HistroricalData()
        {
            _dayAnalyticsRepositoryMock.Setup(x => x.HistoricalData("1")).Returns(TestDataOneDayElectricityModel());
            var result = await _analyticsController.DayResults("1");

            // Assert
            Assert.NotNull(result);
        }

        private async Task<List<OneDayElectricityModel>> TestDataOneDayElectricityModel()
        {
            List<OneDayElectricityModel> obj = new List<OneDayElectricityModel>
            {
                new OneDayElectricityModel
                {
                    Sum = 46,
                    Average = 5.1111111111111107,
                    DateTime = DateTime.Now.AddDays(-1),
                    Maximum = 10,
                    Minimum = 1
                },
                new OneDayElectricityModel
                {
                    Sum = 35,
                    Average = 5,
                    DateTime = DateTime.Now.AddDays(-2),
                    Maximum = 9,
                    Minimum = 2
                },
            };
            List<OneDayElectricityModel> list = await Task.Run(() => obj);
            return list;
        }

        private IQueryable<OneHourElectricity> TestDataOneHourElectricity()
        {
            List<OneHourElectricity> obj = new List<OneHourElectricity>
            {
                new OneHourElectricity
                {
                    Id= 1,
                    KiloWatt= 5,
                    DateTime = DateTime.Now
                },
                new OneHourElectricity
                {
                    Id= 2,
                    KiloWatt= 6,
                    DateTime = DateTime.Now
                },
            };
            return obj.AsQueryable();
        }

        private IQueryable<CrossSolar.Domain.Panel> PanelModelData()
        {
            List<CrossSolar.Domain.Panel> panellist = new List<CrossSolar.Domain.Panel>
            {
                new CrossSolar.Domain.Panel
                {
                    Id = 1,
                    Brand = "Areva",
                    Latitude = 12.345678,
                    Longitude = 98.7655432,
                    Serial = "AAAA1111BBBB2222"
                }
            };
         
            return panellist.AsQueryable();
        }

        [Fact]
        public async Task PostTest()
        {
            var panel = new PanelModel
            {
                Brand = "Areva",
                Latitude = 12.345678,
                Longitude = 98.7655432,
                Serial = "AAAA1111BBBB2222"
            };
            await _genericRepositoryMock.Object.InsertAsync(panel);
            var data = new OneHourElectricityModel() { DateTime = DateTime.Now.Date.AddDays(-1), KiloWatt = 5 };

            // Arrange
            // Act
            var result = await _analyticsController.Post("1", data);

            // Assert
            // Assert.NotNull(result);

            var createdResult = result as CreatedResult;
            Assert.NotNull(createdResult);
            Assert.Equal(201, createdResult.StatusCode);
        }

        [Fact]
        public async Task GetTest()
        {
            var panels = new List<Panel>
            {
                new Panel{
                    Id = 1,
                    Brand = "Areva",
                    Latitude = 12.345678,
                    Longitude = 98.7655432,
                    Serial = "AAAA1111BBBB2222"
                }
            }.AsQueryable();

           // var panels = Enumerable.Empty<Panel>().AsQueryable();
            var onehourelectricity = Enumerable.Empty<OneHourElectricity>().AsQueryable();

            var panelmockset = new Mock<DbSet<Panel>>();
            var onehourelectricitymockset = new Mock<DbSet<OneHourElectricity>>();

            panelmockset.As<IAsyncEnumerable<Panel>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new TestAsyncEnumerator<Panel>(panels.GetEnumerator()));

            panelmockset.As<IQueryable<Panel>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Panel>(panels.Provider));

            onehourelectricitymockset.As<IAsyncEnumerable<OneHourElectricity>>()
                .Setup(m => m.GetEnumerator())
                .Returns(new TestAsyncEnumerator<OneHourElectricity>(onehourelectricity.GetEnumerator()));

            onehourelectricitymockset.As<IQueryable<OneHourElectricity>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<OneHourElectricity>(onehourelectricity.Provider));



            panelmockset.As<IQueryable<Panel>>().Setup(m => m.Expression).Returns(panels.Expression);
            panelmockset.As<IQueryable<Panel>>().Setup(m => m.ElementType).Returns(panels.ElementType);
            panelmockset.As<IQueryable<Panel>>().Setup(m => m.GetEnumerator()).Returns(() => panels.GetEnumerator());



            onehourelectricitymockset.As<IQueryable<OneHourElectricity>>().Setup(m => m.Expression).Returns(onehourelectricity.Expression);
            onehourelectricitymockset.As<IQueryable<OneHourElectricity>>().Setup(m => m.ElementType).Returns(onehourelectricity.ElementType);
            onehourelectricitymockset.As<IQueryable<OneHourElectricity>>().Setup(m => m.GetEnumerator())
                .Returns(() => onehourelectricity.GetEnumerator());


            var contextOptions = new DbContextOptions<CrossSolarDbContext>();
            var mockPanelContext = new Mock<CrossSolarDbContext>(contextOptions);
            mockPanelContext.Setup(c => c.Set<Panel>()).Returns(panelmockset.Object);
           
            var panelRepository = new PanelRepository(mockPanelContext.Object);

            var mockHourElectricityContext = new Mock<CrossSolarDbContext>(contextOptions);
            mockHourElectricityContext.Setup(c => c.Set<OneHourElectricity>()).Returns(onehourelectricitymockset.Object);

            var panelService = new PanelRepository(mockPanelContext.Object);
            var elecService = new AnalyticsRepository(mockHourElectricityContext.Object);            
            _panelRepositoryMock.Setup(x => x.Query()).Returns(panelService.Query());
            _analyticsRepositoryMock.Setup(x => x.Query()).Returns(elecService.Query());
          


            var result = await _analyticsController.Get("1");

            // Assert
            Assert.NotNull(result);
           
        }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public IAsyncEnumerable<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            return new TestAsyncEnumerable<TResult>(expression);
        }

        public Task<TResult> ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken)
        {
            return Task.FromResult(Execute<TResult>(expression));
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetEnumerator()
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public void Dispose()
        {
            _inner.Dispose();
        }

        public T Current
        {
            get
            {
                return _inner.Current;
            }
            
        } 

        public Task<bool> MoveNext(CancellationToken cancellationToken)
        {
            return Task.FromResult(_inner.MoveNext());
        }
    }
}
