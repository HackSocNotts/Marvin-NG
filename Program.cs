using System;
using System.Threading.Tasks;

namespace MarvinNG
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Bot b = new Bot();
            await b.Run();
        }
    }
}
