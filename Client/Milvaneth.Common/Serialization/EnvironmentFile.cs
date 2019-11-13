using System.Collections.Generic;
using System.IO;

namespace Milvaneth.Common
{
    public class EnvironmentFile
    {
        private static bool _ensured; // once is enough
        private static string _pathEnv;

        private Dictionary<DataStore, IUserConfig> envItems = new Dictionary<DataStore, IUserConfig>();
        private readonly Serializer<Dictionary<DataStore, IUserConfig>> _serializer;

        public EnvironmentFile()
        {
            _pathEnv = Helper.GetMilFilePath($"env.pack");
            _serializer = new Serializer<Dictionary<DataStore, IUserConfig>>(_pathEnv, "");
        }

        public bool Load()
        {
            try
            {
                if (!File.Exists(_pathEnv))
                {
                    _serializer.Save(new Dictionary<DataStore, IUserConfig>());
                    _ensured = true;
                    return true;
                }

                envItems = _serializer.Load();
                _ensured = true;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Save()
        {
            _serializer.Save(envItems);
        }

        public IUserConfig ReadEnvFile(DataStore store)
        {
            if (!_ensured)
                Load();

            return envItems.TryGetValue(store, out var ret) ? ret : null;
        }

        public void WriteEnvFile(DataStore store, IUserConfig data, bool saveOnWrite = true)
        {
            if (!_ensured)
                Load();

            envItems[store] = data;

            if(saveOnWrite) Save();
        }
    }

    public enum DataStore
    {
        Milvaneth = 0x0000_0000,
        Communication = 0x0001_0000,
        Internal = 0x0002_0000,
    }
}
