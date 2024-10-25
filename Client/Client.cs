using MeteorologicalStationApplication.Enums;
using MeteorologicalStationApplication.Models;
using System.Text;

namespace Client
{
    public class Client
    {
        private static string primaryServiceUrl = "http://localhost:8080/weather";
        private static string backupServiceUrl = "http://localhost:8081/weather";
        private static string checkPrimaryUrl = "http://localhost:8080/check";
        private static string checkBackupUrl = "http://localhost:8081/check";
        bool current = true;

        public async Task Start()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(10));
            int bothOfflineCounter = 0;
            while (true)
            {
                if (current)
                {
                    try
                    {
                        string data = await FetchWeatherData(primaryServiceUrl, checkPrimaryUrl);
                        Console.WriteLine("Data from Primary Service:");
                        Console.WriteLine(data);
                    }
                    catch
                    {
                        Console.WriteLine("Primary Service is offline. Fetching data from Backup Service...");
                        current = false;
                        try
                        {
                            string data = await FetchWeatherData(backupServiceUrl, checkBackupUrl);
                            Console.WriteLine("Data from Backup Service:");
                            Console.WriteLine(data);
                        }
                        catch
                        {
                            Console.WriteLine("Both services are offline. Try again.");
                            current = true;
                            bothOfflineCounter++;
                            if (bothOfflineCounter == 5)
                            {
                                Console.WriteLine("Shutting down");
                                return;
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        string data = await FetchWeatherData(backupServiceUrl,checkBackupUrl);
                        Console.WriteLine("Data from Backup Service:");
                        Console.WriteLine(data);
                    }
                    catch
                    {
                        Console.WriteLine("Both services are offline.");
                        current = true;
                        bothOfflineCounter++;
                        if (bothOfflineCounter == 5)
                        {
                            Console.WriteLine("Shutting down");
                            return;
                        }
                    }
                }

                //await Task.Delay(5000);
            }
        }

        private static async Task<string> FetchWeatherData(string serviceUrl, string checkUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetStringAsync(checkUrl);
                if (response != "working")
                    return "";
            }
            Console.WriteLine("Do you want all data or specific data?");
            Console.WriteLine("1) all");
            Console.WriteLine("2) specific");
            string q = Console.ReadLine();
            if (q == "all")
            {
                Console.WriteLine("All dates or specific date?");
                Console.WriteLine("1) all");
                Console.WriteLine("2) specific");
                string w = Console.ReadLine();
                if (w == "all")
                {
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetStringAsync(serviceUrl);
                        return response;
                    }
                }
                else
                {
                    Console.WriteLine("Enter date");
                    string queryParam = Console.ReadLine();
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetStringAsync(serviceUrl + "?date=" + queryParam);
                        return response;
                    }
                }
            }
            else if (q == "specific")
            {
                Console.WriteLine("All dates or specific date?");
                Console.WriteLine("1) all");
                Console.WriteLine("2) specific");
                string w = Console.ReadLine();
                if (w == "all")
                {
                    string query = getQueryInput();
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetStringAsync(serviceUrl + "?type=" + query);
                        return response;
                    }
                }
                else
                {
                    Console.WriteLine("Enter date");
                    string queryParam = Console.ReadLine();
                    string query = getQueryInput();
                    using (HttpClient client = new HttpClient())
                    {
                        var response = await client.GetStringAsync(serviceUrl + "?date=" + queryParam + "&" + "type=" + query);
                        return response;
                    }
                }
            }
            else
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetStringAsync(serviceUrl);
                    return response;
                }
            }
        }

        static string getQueryInput()
        {
            Console.WriteLine("Choose parameter to request (enter a number):");
            Console.WriteLine("1) temperature");
            Console.WriteLine("2) air pressure");
            Console.WriteLine("3) humidity");
            Console.WriteLine("4) wind speed");
            Console.WriteLine("5) wind direction");
            Console.WriteLine("6) precipation");
            string query = Console.ReadLine();
            return query;
        }
    }
}
