using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace ChallongeManager
{
    class Program
    {
        static async Task Main()
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos/1");
                    response.EnsureSuccessStatusCode();
                    string body = await response.Content.ReadAsStringAsync();

                    // string alternatebody = await client.GetStringAsync("https://jsonplaceholder.typicode.com/todos/1");
                    Console.WriteLine(body);
                }
                catch (HttpRequestException ex)
                {
                    Console.WriteLine($"Oops, {ex.Message}");
                }
            }

            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }
    }
}
