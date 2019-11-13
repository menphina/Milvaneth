using System;
using System.Runtime.Caching;

namespace Milvaneth.Communication.Data
{
    public static class DataHolder
    {
        internal static string Username = null;
        internal static byte[] RenewToken = null;
        internal static MemoryCache DataCache = MemoryCache.Default;

        public static void AddCache(int world, int item, int container, object value, int life)
        {
            var policy = new CacheItemPolicy();
            policy.AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(life);
            DataCache.Set($"w{world}i{item}c{container}", value, policy);
        }

        public static object GetCache(int world, int item, int container)
        {
            return DataCache.Get($"w{world}i{item}c{container}");
        }

        public static object RemoveCache(int world, int item, int container)
        {
            return DataCache.Remove($"w{world}i{item}c{container}");
        }
    }
}
