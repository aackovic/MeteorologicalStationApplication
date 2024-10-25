using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MeteorologicalStationApplication.Models
{
    public interface IMeteorologicalStation
    {
        string getSpecificParam(int param);
        string getSpecificParamForDate(int param, DateOnly dt);

        string getAllWeatherData();

        MeteorologicalData generateWeatherData();

        void setWeatherData(MeteorologicalData weatherData);

        List<MeteorologicalData> getDataList();

        string getWeatherDataForDate(DateOnly dt);

    }
}
