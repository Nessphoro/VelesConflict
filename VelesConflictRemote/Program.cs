using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VelesConflictRemote
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Veles Conflict Remote Tool";
            RemoteWorker worker = new RemoteWorker(args);
            worker.Run();
        }
    }
}
