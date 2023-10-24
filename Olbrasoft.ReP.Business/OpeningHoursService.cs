using System.Globalization;
using Altairis.ReP.Data;
using Altairis.ReP.Data.Commands.OpenningHoursChangeCommands;
using Altairis.ReP.Data.Queries.OpeningHoursChangeQueries;
using Altairis.Services.DateProvider;
using Microsoft.Extensions.Options;

namespace Olbrasoft.ReP.Business;

public class OpeningHoursService : BaseService, IOpeningHoursService
{

    private readonly IOptions<AppSettings> _optionsAccessor;
    private readonly IDateProvider _dateProvider;
   

    public OpeningHoursService(IDispatcher dispatcher, IOptions<AppSettings> optionsAccessor, IDateProvider dateProvider) : base(dispatcher)
    {

        _optionsAccessor = optionsAccessor ?? throw new ArgumentNullException(nameof(optionsAccessor));
        _dateProvider = dateProvider ?? throw new ArgumentNullException(nameof(dateProvider));
       
    }

    public async Task<OpeningHoursInfo> GetOpeningHoursAsync(int dayOffset) => await GetOpeningHoursAsync(_dateProvider.Today.AddDays(dayOffset));

    public IEnumerable<OpeningHoursInfo> GetOpeningHours(int dayOffsetFrom, int dayOffsetTo) =>
        GetOpeningHours(_dateProvider.Today.AddDays(dayOffsetFrom), _dateProvider.Today.AddDays(dayOffsetTo));

    public async Task<OpeningHoursInfo> GetOpeningHoursAsync(DateTime date, CancellationToken token = default)
    {
        date = date.Date;

        var ohch = await new OpeningHoursChangeByDateQuery(Dispatcher) { Date = date }.ToResultAsync(token); 
        return ohch == null
            ? GetStandardOpeningHours(date)
            : new OpeningHoursInfo
            {
                Date = date,

                IsException = true,
                OpeningTime = ohch.OpeningTime,
                ClosingTime = ohch.ClosingTime
            };
    }

    private IEnumerable<OpeningHoursInfo> GetOpeningHours(DateTime dateFrom, DateTime dateTo)
    {
        var date = dateFrom.Date;

        var ohchs = GetOpeningHoursChangesBetween(dateFrom, dateTo).GetAwaiter().GetResult();

        while (date <= dateTo.Date)
        {
            var ohch = ohchs.SingleOrDefault(x => x.Date == date);
            yield return ohch == null
               ? GetStandardOpeningHours(date)
               : new OpeningHoursInfo
               {
                   Date = date,
                   IsException = true,
                   OpeningTime = ohch.OpeningTime,
                   ClosingTime = ohch.ClosingTime
               };
            date = date.AddDays(1);
        }
    }

    public IEnumerable<OpeningHoursInfo> GetStandardOpeningHours()
    {
        var dt = DateTime.Today;
        while (dt.DayOfWeek != CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek) dt = dt.AddDays(1);
        for (var i = 0; i < 7; i++)
        {
            yield return GetStandardOpeningHours(dt);
            dt = dt.AddDays(1);
        }
    }

    private OpeningHoursInfo GetStandardOpeningHours(DateTime date)
    {
        var value = _optionsAccessor.Value.OpeningHours.FirstOrDefault(x => x.DayOfWeek == date.DayOfWeek);
        return value == null
            ? new OpeningHoursInfo { Date = date.Date }
            : new OpeningHoursInfo { Date = date.Date, OpeningTime = value.OpeningTime, ClosingTime = value.ClosingTime, IsException = false };
    }

    private async Task<IEnumerable<OpeningHoursChange>> GetOpeningHoursChangesBetween(DateTime dateFrom, DateTime dateTo, CancellationToken token = default)
       => await new OpeningHoursChangesBetweenDatesQuery(Dispatcher) { DateFrom = dateFrom, DateTo = dateTo }.ToResultAsync(token);
   
    public async Task<IEnumerable<OpeningHoursChange>> GetOpeningHoursChangesAsync(CancellationToken token = default)
     => await new OpeningHoursChangesQuery(Dispatcher).ToResultAsync(token);

    public async Task<CommandStatus> SaveOpeningHoursChangeAsync(DateTime date, TimeSpan openingTime, TimeSpan closingTime, CancellationToken token = default)
        => await new SaveOpeningHoursChangeCommand(Dispatcher) { Date = date, OpeningTime = openingTime, ClosingTime = closingTime }.ToResultAsync(token);

    public Task<CommandStatus> DeleteOpeningHoursChangeAsync(int openingHoursChangeId, CancellationToken token = default)
        => new DeleteOpeningHoursChangeCommand(Dispatcher) { OpeningHoursChangeId = openingHoursChangeId }.ToResultAsync(token);

    public Task<IEnumerable<OpeningHoursInfo>> GetOpeningHoursAsync(int dayOffsetFrom, int dayOffsetTo, CancellationToken token = default)
    {
        throw new NotImplementedException();
    }
}
