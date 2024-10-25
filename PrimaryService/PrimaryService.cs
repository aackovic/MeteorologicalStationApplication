using System;
using System.Net;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Web;
using MeteorologicalStationApplication.Enums;
using MeteorologicalStationApplication.Models;

namespace PrimaryService
{
    public class PrimaryService : Service
    {
        public PrimaryService(string serviceName, bool primary, string destUrl) : base(serviceName, primary, destUrl)
        {
        }
    }
}
