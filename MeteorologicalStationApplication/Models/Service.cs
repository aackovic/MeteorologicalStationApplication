using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;
using System.Web;

namespace MeteorologicalStationApplication.Models
{
    public class Service
    {
        private static IMeteorologicalStation station = new MeteorologicalStation();
        private static string primaryServiceUrl = "http://localhost:8080/";
        private static string backupServiceUrl = "http://localhost:8081/";
        private static string replicatorUrl = "http://localhost:8082/";
        private string serviceName;
        private bool primary = true;
        private bool fullCopy = false;
        private string destUrl;

        public Service(string serviceName, bool primary, string destUrl)
        {
            this.serviceName = serviceName;
            this.primary = primary;
            this.destUrl = destUrl;
        }

        public async Task Start(string url)
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add(url);
            listener.Start();
            Console.WriteLine(serviceName + " Service running on " + url);
            while (true)
            {
                if (primary == true)
                    await primaryS(listener);
                else
                    await secondaryS(listener);
            }
        }

        private async Task primaryS(HttpListener listener)
        {
            _ = Task.Run(async () => await GenerateAndReplicateWeatherData());

            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/weather")
                {
                    string weatherData = "";
                    if (request.Url.Query != "")
                    {
                        var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
                        if (queryParams != null)
                        {
                            if (queryParams["date"] != null)
                            {
                                if (queryParams["type"] != null)
                                {
                                    weatherData = station.getSpecificParamForDate(Convert.ToInt32(queryParams["type"]), DateOnly.Parse(queryParams["date"]));
                                }
                                else
                                {
                                    weatherData = station.getWeatherDataForDate(DateOnly.Parse(queryParams["date"]));
                                }
                            }
                            else
                            {
                                if (queryParams["type"] != null)
                                {
                                    weatherData = station.getSpecificParam(Convert.ToInt32(queryParams["type"]));
                                }
                                else
                                {
                                    weatherData = station.getAllWeatherData();
                                }
                            }
                        }
                    }
                    else
                    {
                        weatherData = station.getAllWeatherData();
                    }
                    if (weatherData == "")
                        weatherData = "There is no available data for your request.";
                    byte[] data = Encoding.UTF8.GetBytes(weatherData);
                    response.ContentLength64 = data.Length;
                    response.ContentType = "application/json";
                    response.StatusCode = (int)HttpStatusCode.OK;

                    using (var output = response.OutputStream)
                    {
                        output.Write(data, 0, data.Length);
                    }
                }
                else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/check")
                {
                    byte[] data = Encoding.UTF8.GetBytes("working");
                    response.ContentLength64 = data.Length;
                    response.ContentType = "application/json";
                    response.StatusCode = (int)HttpStatusCode.OK;

                    using (var output = response.OutputStream)
                    {
                        output.Write(data, 0, data.Length);
                    }
                }
                else
                {
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                }

                response.Close();
            }
        }
        private async Task secondaryS(HttpListener listener)
        {
            while (primary == false)
            {
                try
                {
                    HttpListenerContext context;
                    context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/replicate")
                    {
                        primary = false;
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string replicatedData = reader.ReadToEnd();
                            ProcessReplication(replicatedData);
                        }
                        response.StatusCode = (int)HttpStatusCode.OK;
                        Console.WriteLine("Data replication successful.");
                        byte[] responseMessage = Encoding.UTF8.GetBytes("Data replication successful.");
                        response.ContentLength64 = responseMessage.Length;
                        var output = response.OutputStream;
                        output.Write(responseMessage, 0, responseMessage.Length);
                        output.Close();
                    }
                    else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/replicateAll")
                    {
                        primary = false;
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string replicatedData = reader.ReadToEnd();
                            ReplicateAll(replicatedData);
                        }
                        response.StatusCode = (int)HttpStatusCode.OK;
                        Console.WriteLine("Data replication successful.");
                        byte[] responseMessage = Encoding.UTF8.GetBytes("Data replication successful.");
                        response.ContentLength64 = responseMessage.Length;
                        var output = response.OutputStream;
                        output.Write(responseMessage, 0, responseMessage.Length);
                        output.Close();
                    }
                    else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/weather")
                    {
                        primary = true;
                        string weatherData = "";
                        if (request.Url.Query != "")
                        {
                            var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
                            Console.WriteLine(queryParams["date"]);
                            DateOnly dateOnly = DateOnly.Parse(queryParams["date"]);
                            weatherData = station.getWeatherDataForDate(dateOnly);
                        }
                        else
                        {
                            weatherData = station.getAllWeatherData();
                        }
                        if (weatherData == "")
                            weatherData = "There is no available data for your request.";
                        byte[] data = Encoding.UTF8.GetBytes(weatherData);
                        response.ContentLength64 = data.Length;
                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.OK;

                        using (var output = response.OutputStream)
                        {
                            output.Write(data, 0, data.Length);
                        }
                    }
                    else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/check")
                    {
                        byte[] data = Encoding.UTF8.GetBytes("working");
                        response.ContentLength64 = data.Length;
                        response.ContentType = "application/json";
                        response.StatusCode = (int)HttpStatusCode.OK;

                        using (var output = response.OutputStream)
                        {
                            output.Write(data, 0, data.Length);
                        }
                    }
                    else
                    {
                        response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                catch
                {
                    continue;
                }

            }
        }

        public void ProcessReplication(string data)
        {
            Console.WriteLine(serviceName + " Service: Replicating data...");
            MeteorologicalData metData = JsonSerializer.Deserialize<MeteorologicalData>(data);
            station.setWeatherData(metData);
        }

        public void ReplicateAll(string data)
        {
            //Console.WriteLine(data);
            Console.WriteLine(serviceName + " Service: Replicating all data...");
            List<MeteorologicalData> metData = JsonSerializer.Deserialize<List<MeteorologicalData>>(data);
            foreach (var el in metData)
            {
                station.setWeatherData(el);
            }
        }

        private async Task GenerateAndReplicateWeatherData()
        {
            while (true)
            {
                MeteorologicalData metData = station.generateWeatherData();
                Console.WriteLine(serviceName + " Service: Generated new weather data.");

                await ReplicateToReplicator(metData);

                await Task.Delay(10000);
            }
        }
        private async Task ReplicateToReplicator(MeteorologicalData metData)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string replicate = "replicate";
                    var content = new StringContent(JsonSerializer.Serialize(metData), Encoding.UTF8, "application/json");
                    if (fullCopy)
                    {
                        replicate += "All";
                        fullCopy = false;
                        content = new StringContent(JsonSerializer.Serialize(station.getDataList()), Encoding.UTF8, "application/json");
                    }
                    var result = await client.PostAsync(replicatorUrl + replicate + "?url=" + destUrl, content);
                    if (result.IsSuccessStatusCode)
                    {
                        string confirmationMessage = await result.Content.ReadAsStringAsync();
                        Console.WriteLine("Replication to Replicator Service successful: " + confirmationMessage);
                    }
                    else
                    {
                        Console.WriteLine("Failed to replicate data to Replicator Service. Status Code: " + result.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to replicate data to Replicator Service: {ex.Message}");
                    fullCopy = true;
                }
            }
        }
    }
}
