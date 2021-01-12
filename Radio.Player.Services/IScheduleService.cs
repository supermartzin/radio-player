using System.Collections.Generic;
using System.Threading.Tasks;

using Radio.Player.Models;

namespace Radio.Player.Services
{
    public interface IScheduleService
    {
        Task<IEnumerable<ScheduleItem>> GetSchedule(RadioStation radioStation);
    }
}