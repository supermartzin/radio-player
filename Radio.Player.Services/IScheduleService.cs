using System.Collections.Generic;
using System.Threading.Tasks;

using Rtvs.iRadio.Models;

namespace Rtvs.iRadio.Services
{
    public interface IScheduleService
    {
        Task<IEnumerable<ScheduleItem>> GetSchedule(RadioStationId stationId);
    }
}