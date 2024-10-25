using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorologicalStationApplication.Models
{
    public class MeteorologicalStation : IMeteorologicalStation
    {
        private Dictionary<DateOnly, MeteorologicalData> data;

        public MeteorologicalStation()
        {
            data = new Dictionary<DateOnly, MeteorologicalData>();
        }

        public void ReplicateData(MeteorologicalStation secondary)
        {
            foreach(var el in data)
            {
                secondary.data[el.Key] = el.Value;
            }
        }

        public string getAllWeatherData()
        {
            string res = "";
            foreach(var el in data)
            {
                res += el.Key.ToString() + " | " + el.Value.ToString() + "\n";
            }
            return res;
        }

        public List<MeteorologicalData> getDataList()
        {
            List<MeteorologicalData> metList = new List<MeteorologicalData>();
            foreach (var el in data)
            {
                metList.Add(el.Value);
            }
            return metList;
        }

        public MeteorologicalData generateWeatherData()
        {
            Random random = new Random();
            DateOnly startDate = new DateOnly(1900, 1, 1);
            DateOnly endDate = DateOnly.FromDateTime(DateTime.Today);
            int range = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
            DateOnly timestamp = startDate.AddDays(random.Next(range));

            Random rand = new Random();

            double temperature = rand.Next(-10, 40); // Temperature in Celsius
            double airPressure = rand.Next(950, 1050); // Air pressure in hPa
            double humidity = rand.Next(0, 100); // Humidity in percentage
            double windSpeed = rand.Next(0, 100); // Wind speed in km/h
            string[] windDirections = { "N", "NE", "E", "SE", "S", "SW", "W", "NW" };
            string windDirection = windDirections[rand.Next(windDirections.Length)];
            double precipitation = rand.NextDouble() * 100; // Precipitation in mm

            MeteorologicalData mData = new MeteorologicalData(temperature, airPressure, humidity, windSpeed, windDirection, precipitation, timestamp);
            data[timestamp] = mData;

            return mData;
        }

        public void setWeatherData(MeteorologicalData weatherData)
        {
            data[weatherData.Time] = weatherData;
        }

        public string getWeatherDataForDate(DateOnly dt)
        {
            if (data.TryGetValue(dt, out var value))
            {
                return value.ToString();
            }
            return "";
        }

        public string getSpecificParam(int param)
        {
            string res = getFirstRow(param);
            foreach(var el in data)
            {
                res += el.Key.ToString() + " | ";
                switch (param)
                {
                    case 1:
                        res += el.Value.Temperature.ToString() + "\n";
                        break;
                    case 2:
                        res += el.Value.AirPressure.ToString() + "\n";
                        break;
                    case 3:
                        res += el.Value.Humidity.ToString() + "\n";
                        break;
                    case 4:
                        res += el.Value.WindSpeed.ToString() + "\n";
                        break;
                    case 5:
                        res += el.Value.WindDirection.ToString() + "\n";
                        break;
                    case 6:
                        res += el.Value.Precipitation.ToString() + "\n";
                        break;
                    default:
                        return "";
                }
            }
            return res;
        }

        public string getSpecificParamForDate(int param, DateOnly dt)
        {

            string res = getFirstRow(param);
            if (data.TryGetValue(dt, out var value))
            {
                res += dt.ToString() + " | ";
                switch (param)
                {
                    case 1:
                        res += value.Temperature.ToString() + "\n";
                        break;
                    case 2:
                        res += value.AirPressure.ToString() + "\n";
                        break;
                    case 3:
                        res += value.Humidity.ToString() + "\n";
                        break;
                    case 4:
                        res += value.WindSpeed.ToString() + "\n";
                        break;
                    case 5:
                        res += value.WindDirection.ToString() + "\n";
                        break;
                    case 6:
                        res += value.Precipitation.ToString() + "\n";
                        break;
                    default:
                        return "";
                }
            }
            return res;
        }

        public string getFirstRow(int param)
        {
            string firstRow = "Date | ";
            switch (param)
            {
                case 1:
                    firstRow += "Temperature\n";
                    break;
                case 2:
                    firstRow += "Air pressure\n";
                    break;
                case 3:
                    firstRow += "Humidity\n";
                    break;
                case 4:
                    firstRow += "Wind speed\n";
                    break;
                case 5:
                    firstRow += "Wind direction\n";
                    break;
                case 6:
                    firstRow += "Precipation\n";
                    break;
                default:
                    return "";
            }
            return firstRow;
        }
    }
}
