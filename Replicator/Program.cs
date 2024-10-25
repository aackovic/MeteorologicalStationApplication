using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replicator
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var replicator = new Replicator();
            await replicator.Start();
        }
    }
}
