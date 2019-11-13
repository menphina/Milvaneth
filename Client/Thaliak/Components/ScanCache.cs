using System.Collections.Generic;

namespace Milvaneth.Common
{
    public class ScanCache
    {
        public long LastCacheTime;
        public string CacheGameVersion;
        public int SignatureVersion;
        public IEnumerable<KeyValuePair<int, long>> CacheValue;
    }
}