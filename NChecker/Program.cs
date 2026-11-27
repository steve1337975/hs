using Newtonsoft.Json;
using System.Text;

namespace NChecker
{
    internal class Program
    {
        static Program()
        {
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;
        }
        private static async Task Main()
        {



            await App.Instance.Start();
        }
    }
}