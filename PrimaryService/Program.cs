using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimaryService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            string filePath = "first_run.txt";
            string newContent = "This is the first run";
            bool primary = true;
            if (File.Exists(filePath))
            {
                primary = false;
            }

            // Replace the entire content of the file
            File.WriteAllText(filePath, newContent);

            var primaryService = new PrimaryService("Primary", primary, "http://localhost:8081/");
            await primaryService.Start("http://localhost:8080/");
        }
    }
}
