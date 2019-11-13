using Flurl;
using Milvaneth.Common;
using Milvaneth.Communication.Vendor;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Milvaneth.Communication.Foundation
{
    internal class RestApi : IDisposable
    {
        public bool Initialized { get; private set; }
        public const string FrameworkVersion = "1.0";
        public const string ApiVersion = "APIv1";

        private static RestApi _instance;

        private HttpClient _client;

        public static RestApi Instance
        {
            get
            {
                if(_instance == null || _instance.Initialized == false)
                    Initialize();
                return _instance;
            }
        }

        public static void Initialize()
        {
            _instance = new RestApi();
        }

        private RestApi()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            ServicePointManager.DefaultConnectionLimit = 8;

            var endpoint = EndpointSelector.PickEndpoint(MilvanethConfig.Store.Api.Endpoints, MilvanethConfig.Store.Api.Prefix);

            _client = new HttpClient
            {
                BaseAddress = new Uri(endpoint)
            };

            _client.DefaultRequestHeaders.Accept.Clear();
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MilvanethConfig.Store.Api.Mime));
            _client.DefaultRequestHeaders.UserAgent.Clear();
            _client.DefaultRequestHeaders.UserAgent.ParseAdd(
                $@"HttpClient/{Environment.Version} MilCom/{FrameworkVersion} ({ApiVersion}) " +
                $@"Milvaneth/{MilvanethConfig.Store.Global.MilVersion} (D{MilvanethConfig.Store.Global.DataVersion}, G{MilvanethConfig.Store.Global.GameVersion})");

            Initialized = true;
        }

        ~RestApi()
        {
            try { _client?.Dispose(); } catch { /* ignored */ }
            Initialized = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _client.Dispose();
            Initialized = false;
        }

        public byte[] Post(string url, byte[] input)
        {
            var response = Synchronize(() => _client.PostAsync(FormatRoute(url), new ByteArrayContent(input)));
            response.EnsureSuccess();

            var data = Synchronize(() => response.Content.ReadAsByteArrayAsync());
            return data;
        }

        public byte[] Get(string url, byte[] input)
        {
            var response = Synchronize(() => _client.GetAsync(FormatRoute(url)));
            response.EnsureSuccess();

            var data = Synchronize(() => response.Content.ReadAsByteArrayAsync());
            return data;
        }

        public byte[] Put(string url, byte[] input)
        {
            var response = Synchronize(() => _client.PutAsync(FormatRoute(url), new ByteArrayContent(input)));
            response.EnsureSuccess();

            var data = Synchronize(() => response.Content.ReadAsByteArrayAsync());
            return data;
        }

        public byte[] Delete(string url, byte[] input)
        {
            var response = Synchronize(() => _client.DeleteAsync(FormatRoute(url)));
            response.EnsureSuccess();

            var data = Synchronize(() => response.Content.ReadAsByteArrayAsync());
            return data;
        }

        private string FormatRoute(string url)
        {
            return MilvanethConfig.Store.Api.Prefix
                .AppendPathSegment(url)
                // token can be ignored so it's safe to leave it here
                .SetQueryParam("token", ApiVendor.GetToken())
                // Environment.TickCount is faster than DateTime.Now.Ticks
                .SetQueryParam("t", Environment.TickCount);
        }

        // https://stackoverflow.com/questions/53529061/whats-the-right-way-to-use-httpclient-synchronously
        public static T Synchronize<T>(Func<Task<T>> action) where T : class
        {
            var task = Task.Run(action);
            task.Wait(10000);
            return task.IsCompleted ? task.Result : null;
        }
    }

    public static class ResponseThrowHelper
    {
        public static HttpResponseMessage EnsureSuccess(this HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                if (response.Content != null)
                    response.Content.Dispose();
                var ex = new HttpRequestException($"Response status code does not indicate success: {(int)response.StatusCode} ({response.StatusCode}).");
                ex.Data["StatusCode"] = response.StatusCode;
                throw ex;
            }

            return response;
        }
    }
}
