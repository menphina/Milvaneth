using Flurl;
using Milvaneth.Common;
using Milvaneth.Communication.Vendor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace Milvaneth.Communication.Foundation
{
    internal static class EndpointSelector
    {
        private static string _preferred;
        private static string _selected;
        public static void SetPreferredEndpoint(string url)
        {
            if (string.IsNullOrWhiteSpace(url) ||
                !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                !Url.IsValid(url) ||
                !uri.Scheme.Equals("https", StringComparison.InvariantCultureIgnoreCase))
            {
                _preferred = null;
                return;
            }

            foreach (var i in MilvanethConfig.Store.Api.Endpoints)
            {
                if (i.Equals(Uri.UriSchemeHttps + Uri.SchemeDelimiter + uri.Authority,
                    StringComparison.InvariantCultureIgnoreCase))
                {
                    _preferred = i;
                    _selected = null;
                }
            }
        }

        internal static string PickEndpoint(string[] endpoints, string prefix)
        {
            if (_selected != null)
            {
                return _selected;
            }

            if (_preferred != null && endpoints.Contains(_preferred) &&
                CheckAvailability(_preferred, prefix).Item1 >= 0)
            {
                _selected = _preferred;
                return _selected;
            }

            var tokenSource = new CancellationTokenSource();
            var token = tokenSource.Token;
            var results = new Task<Tuple<int, string>>[endpoints.Length];
            var i = 0;

            foreach (var endpoint in endpoints)
            {
                results[i] = Task.Run(() => CheckAvailability(endpoint, prefix), token);
                i++;
            }

            var result = RestApi.Synchronize(() => WhenAny(results, x => !CheckVendor.NotValidResponseCode(x.Item1)));

            _selected = CheckVendor.NotValidResponseCode(result.Item1) ? null : result.Item2;
            return _selected;
        }

        // https://stackoverflow.com/questions/24550932/tpl-wait-for-task-to-complete-with-a-specific-return-value
        private static async Task<T> WhenAny<T>(IEnumerable<Task<T>> tasks, Func<T, bool> predicate)
        {
            var taskList = tasks.ToList();
            Task<T> completedTask;
            do
            {
                completedTask = await Task.WhenAny(taskList);
                taskList.Remove(completedTask);
            } while (!predicate(await completedTask) && taskList.Any());

            return await completedTask;
        }

        internal static Tuple<int, string> CheckAvailability(string url, string prefix)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(url) ||
                    !Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
                    !Url.IsValid(url))
                {
                    return new Tuple<int, string>(02_0005, url); // Invalid url
                }

                if (!PingHost(uri.Host, out var time))
                    return new Tuple<int, string>(02_0006, url); // cannot ping

                using (var client = new TimedWebClient())
                {
                    try
                    {
                        var s = client.DownloadString(Url.Combine(url, prefix, ""));
                        if (!s.StartsWith("Milvaneth Api Service"))
                            return new Tuple<int, string>(02_0004, url); // not valid service
                    }
                    catch
                    {
                        return new Tuple<int, string>(02_0004, url);
                    }

                }

                return new Tuple<int, string>(time, url);
            }
            catch
            {
                return new Tuple<int, string>(02_0001, url); // other exception
            }
        }

        private static bool PingHost(string host, out int delay)
        {
            delay = 0;
            var address = Dns.GetHostEntry(host).AddressList.First();
            var pingOptions = new PingOptions(128, true);
            var ping = new Ping();
            var buffer = new byte[32];
            var succ = 0;

            for (var i = 0; i < 4; i++)
            {
                try
                {
                    var pingReply = ping.Send(address, 300, buffer, pingOptions);

                    if (pingReply != null && pingReply.Status == IPStatus.Success)
                    {
                        succ++;
                        delay += (int) pingReply.RoundtripTime;
                    }
                }
                catch
                {
                    delay = -1;
                    return false;
                }

            }

            delay = succ > 0 ? delay / succ : -1;
            return succ > 2;
        }

        // https://stackoverflow.com/questions/12878857/how-to-limit-the-time-downloadstringurl-allowed-by-500-milliseconds
        private class TimedWebClient : WebClient
        {
            public int Timeout { get; set; }

            public TimedWebClient()
            {
                this.Timeout = 3800;
            }

            protected override WebRequest GetWebRequest(Uri address)
            {
                var objWebRequest = base.GetWebRequest(address);
                objWebRequest.Timeout = this.Timeout;
                return objWebRequest;
            }
        }
    }
}
