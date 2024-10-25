using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeteorologicalStationApplication.Models
{
    public class MeteorologicalData
    {
        public double Temperature { get; set; }
        public double AirPressure { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public string WindDirection { get; set; }
        public double Precipitation { get; set; }
        public DateOnly Time { get; set; }

        public MeteorologicalData(double temperature, double airPressure, double humidity, double windSpeed, string windDirection, double precipitation, DateOnly time)
        {
            Temperature = temperature;
            AirPressure = airPressure;
            Humidity = humidity;
            WindSpeed = windSpeed;
            WindDirection = windDirection;
            Precipitation = precipitation;
            Time = time;
        }

        public override string ToString()
        {
            return $"Temperature: {Temperature}, Air pressure: {AirPressure}, Humidity: {Humidity}, Wind speed: {WindSpeed}, Wind direction: {WindDirection}, Precipitation: {Precipitation}";
        }
    }
}
