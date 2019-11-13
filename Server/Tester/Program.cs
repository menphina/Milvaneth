using System;
using System.IO;
using System.Net;
using MessagePack;

namespace Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            var str = new[] {"test1", "test2"};
            var dat = MessagePackSerializer.Serialize(str);
            var p = int.Parse(Console.ReadLine());
            var httpWebRequest = (HttpWebRequest)WebRequest.Create($"https://localhost:{p}/api/values");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = httpWebRequest.GetRequestStream())
            {
                streamWriter.Write(dat, 0 ,dat.Length);
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }
    }
}
