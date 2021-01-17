using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface IScheduleService
    {
        Task<IEnumerable<ScheduleItem>> GetFullScheduleAsync(RadioStation radioStation);

        Task<IEnumerable<ScheduleItem>> GetScheduleForSpecificDayAsync(RadioStation radioStation, DateTime date);
    }
}