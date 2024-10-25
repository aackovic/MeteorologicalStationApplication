using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;
using MeteorologicalStationApplication.Enums;
using MeteorologicalStationApplication.Models;

namespace SecondaryService
{
    public class SecondaryService : Service
    {
        public SecondaryService(string serviceName, bool primary, string destUrl) : base(serviceName, primary, destUrl)
        {
        }
    }
}
