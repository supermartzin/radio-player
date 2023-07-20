using Radio.Player.Models;

namespace Radio.Player.Services.Contracts;

public interface IScheduleService
{
    Task<IEnumerable<ScheduleItem>> GetFullScheduleAsync(RadioStation radioStation, CancellationToken cancellationToken = default);

    Task<IEnumerable<ScheduleItem>> GetScheduleForSpecificDayAsync(RadioStation radioStation, DateTime date, CancellationToken cancellationToken = default);
}