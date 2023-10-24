using Altairis.ReP.Data;
using Altairis.ReP.Data.Entities;

namespace Olbrasoft.ReP.Business.Abstractions;
public interface IOpeningHoursService
{
    Task<CommandStatus> SaveOpeningHoursChangeAsync(DateTime date, TimeSpan openingTime, TimeSpan closingTime, CancellationToken token = default);

    Task<CommandStatus> DeleteOpeningHoursChangeAsync(int openingHoursChangeId, CancellationToken token = default);

    Task<IEnumerable<OpeningHoursChange>> GetOpeningHoursChangesAsync(CancellationToken token = default);

    Task<OpeningHoursInfo> GetOpeningHoursAsync(DateTime date, CancellationToken token = default);

    Task<OpeningHoursInfo> GetOpeningHoursAsync(int dayOffset);

    IEnumerable<OpeningHoursInfo> GetStandardOpeningHours();

    IEnumerable<OpeningHoursInfo> GetOpeningHours(int dayOffsetFrom, int dayOffsetTo);

    Task<IEnumerable<OpeningHoursInfo>> GetOpeningHoursAsync(int dayOffsetFrom, int dayOffsetTo, CancellationToken token = default);
}