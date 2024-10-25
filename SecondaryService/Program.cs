using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecondaryService
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var backupService = new SecondaryService("Secondary", false, "http://localhost:8080/");
            await backupService.Start("http://localhost:8081/");
        }
    }
}
