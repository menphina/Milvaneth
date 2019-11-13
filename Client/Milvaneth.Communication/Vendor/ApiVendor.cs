using Milvaneth.Communication.Api;
using Milvaneth.Communication.Data;
using Milvaneth.Communication.Foundation;
using Milvaneth.Communication.Procedure;
using System;
using System.Net.Http;

namespace Milvaneth.Communication.Vendor
{
    public class ApiVendor
    {
        private static string _tokenStore;

        public static void SetPreferredEndpoint(string endpoint)
        {
            EndpointSelector.SetPreferredEndpoint(endpoint);
        }

        public static bool TryPrepareRestApi()
        {
            try
            {
                PrepareRestApi();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void PrepareRestApi()
        {
            PrepareRestApi(RestApi.Instance.Get, RestApi.Instance.Post, RestApi.Instance.Put, RestApi.Instance.Delete);
        }

        public static void PrepareRestApi(VerbMethod get, VerbMethod post, VerbMethod put, VerbMethod delete)
        {
            RestApi.Initialize();
            ApiCall.Get = get;
            ApiCall.Post = post;
            ApiCall.Put = put;
            ApiCall.Delete = delete;
        }

        public static void SetToken(string token)
        {
            _tokenStore = token;
        }

        public static string GetToken()
        {
            return _tokenStore;
        }

        public static bool HasToken()
        {
            return !string.IsNullOrEmpty(_tokenStore);
        }

        public static int RenewToken(string username, ref byte[] renewToken)
        {
            var rp = new RenewProcedure();
            try
            {
                var ret = rp.Step1(username, renewToken);

                if (CheckVendor.NotValidResponseCode(ret))
                    return ret;

                return rp.Step2(out renewToken);
            }
            catch (HttpRequestException e)
            {
                return 02_0000 + (int)e.Data["StatusCode"];
            }
        }

        public static void SetRenew(byte[] renewToken)
        {
            if (renewToken != null)
            {
                var tmp = new byte[renewToken.Length];
                Buffer.BlockCopy(renewToken, 0, tmp, 0, tmp.Length);
                DataHolder.RenewToken = tmp;
            }
            else
            {
                DataHolder.RenewToken = null;
            }
        }

        public static byte[] GetRenew()
        {
            if (DataHolder.RenewToken != null)
            {
                var tmp = new byte[DataHolder.RenewToken.Length];
                Buffer.BlockCopy(DataHolder.RenewToken, 0, tmp, 0, tmp.Length);
                return tmp;
            }

            return null;
        }

        public static void SetUsername(string username)
        {
            DataHolder.Username = username;
        }

        public static string GetUsername()
        {
            return DataHolder.Username;
        }

        public static bool ValidateAndRenewToken()
        {
            if (DataHolder.RenewToken == null || CheckVendor.NotValidUsername(DataHolder.Username))
            {
                return false;
            }

            int ret;
            var renew = new RenewProcedure();

            try
            {
                ret = renew.Step1(DataHolder.Username, DataHolder.RenewToken);

                byte[] token = null;

                ret = CheckVendor.NotValidResponseCode(ret) ? ret : renew.Step2(out token);

                if (CheckVendor.NotValidResponseCode(ret))
                {
                    return false;
                }

                SetRenew(token);
            }
            catch (HttpRequestException ex)
            {
                ret = 02_0000 + (int)(ex.Data["StatusCode"]);
            }
            catch (Exception)
            {
                ret = 02_0000;
            }

            return !CheckVendor.NotValidResponseCode(ret) && HasToken();
        }
    }
}
