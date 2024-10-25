using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using MeteorologicalStationApplication.Enums;
using MeteorologicalStationApplication.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Replicator
{
    public class Replicator
    {
        private static IMeteorologicalStation station = new MeteorologicalStation();
        private static string primaryServiceUrl = "http://localhost:8080/";
        private static string backupServiceUrl = "http://localhost:8081/";
        //private static string replicatorUrl = "http://localhost:8082/";
        bool fullCopy = false;
        public async Task Start()
        {
            HttpListener listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:8082/");
            listener.Start();
            Console.WriteLine("Replicator running on http://localhost:8082/");

            while (true)
            {
                try
                {
                    HttpListenerContext context;
                    context = listener.GetContext();
                    HttpListenerRequest request = context.Request;
                    HttpListenerResponse response = context.Response;

                    if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/replicate")
                    {
                        var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
                        string destUrl = queryParams["url"];
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string replicatedData = reader.ReadToEnd();
                            ProcessReplicationAsync(replicatedData, destUrl);
                        }
                        response.StatusCode = (int)HttpStatusCode.OK;
                        Console.WriteLine("Data storing to replicator successful.");
                        byte[] responseMessage = Encoding.UTF8.GetBytes("Data storing to replicator successful.");
                        response.ContentLength64 = responseMessage.Length;
                        var output = response.OutputStream;
                        output.Write(responseMessage, 0, responseMessage.Length);
                        output.Close();
                    }
                    else if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/replicateAll")
                    {
                        var queryParams = HttpUtility.ParseQueryString(request.Url.Query);
                        string destUrl = queryParams["url"];
                        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                        {
                            string replicatedData = reader.ReadToEnd();
                            ReplicateAll(replicatedData, destUrl);
                        }
                        response.StatusCode = (int)HttpStatusCode.OK;
                        Console.WriteLine("Data storing to replicator successful.");
                        byte[] responseMessage = Encoding.UTF8.GetBytes("Data storing to replicator successful.");
                        response.ContentLength64 = responseMessage.Length;
                        var output = response.OutputStream;
                        output.Write(responseMessage, 0, responseMessage.Length);
                        output.Close();
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
        public async Task ProcessReplicationAsync(string data, string destUrl)
        {
            Console.WriteLine("Storing data...");
            MeteorologicalData metData = JsonSerializer.Deserialize<MeteorologicalData>(data);
            station.setWeatherData(metData);
            //fullCopy = false;
            await proccess(data, destUrl);
        }

        public async Task ReplicateAll(string data, string destUrl)
        {
            Console.WriteLine("Storing all data...");
            List<MeteorologicalData> metData = JsonSerializer.Deserialize<List<MeteorologicalData>>(data);
            foreach (var el in metData)
            {
                station.setWeatherData(el);
            }
            fullCopy = true;
            await proccess(data, destUrl);
        }

        public async Task proccess(string data, string destUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    string replicate = "replicate";
                    if (fullCopy)
                    {
                        replicate += "All";
                        data = JsonSerializer.Serialize(station.getDataList());
                    }
                        var content = new StringContent(data, Encoding.UTF8, "application/json");
                    var result = await client.PostAsync(destUrl + replicate, content);
                    if (result.IsSuccessStatusCode)
                    {
                        string confirmationMessage = await result.Content.ReadAsStringAsync();
                        Console.WriteLine("Replication to " + destUrl + " Service successful: " + confirmationMessage);
                        fullCopy = false;
                    }
                    else
                    {
                        fullCopy = true;
                        Console.WriteLine("Failed to replicate data to " + destUrl + "Service. Status Code: " + result.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    fullCopy = true;
                    Console.WriteLine($"Failed to replicate data to " + destUrl + " Service: + " + ex.Message);
                }
            }
        }
    }
}
