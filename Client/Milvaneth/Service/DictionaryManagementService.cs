using Milvaneth.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Milvaneth.Service
{
    public static class DictionaryManagementService
    {
        public static Dictionary<int, string> Map { get; private set; }
        public static Dictionary<int, string> World { get; private set; }
        public static Dictionary<int, string> Materia { get; private set; }
        public static Dictionary<int, string> Retainer { get; private set; }
        public static Dictionary<int, string> Item { get; private set; }
        public static Dictionary<int, string> Dye { get; private set; }
        public static Dictionary<int, string> RetainerAbbr { get; private set; }
        public static Dictionary<int, string> LocalizedWorld { get; private set; }
        public static Dictionary<int, string> MateriaAbbr { get; private set; }
        public static int DataVersion { get; private set; }

        public static void Initialize()
        {
            var path = Helper.GetMilFilePathWithVenv("dictionary.pack", -1);
            var ser = new Serializer<Dictionary<int, Dictionary<int, string>>>(path, "");
            var tmp = ser.Load();

            DataVersion = tmp[DictionaryRetrieveKey.DAT_VER].First().Key;
            World = tmp[DictionaryRetrieveKey.WORLD];
            Materia = tmp[DictionaryRetrieveKey.MATERIA];
            Retainer = tmp[DictionaryRetrieveKey.RET_LOC];
            Item = tmp[DictionaryRetrieveKey.ITEM];
            Dye = tmp[DictionaryRetrieveKey.DYE];
            Map = tmp[DictionaryRetrieveKey.MAP];
            RetainerAbbr = tmp[DictionaryRetrieveKey.RET_LOCL_NICK];
            LocalizedWorld = tmp[DictionaryRetrieveKey.WORLD_NICK];
            MateriaAbbr = tmp[DictionaryRetrieveKey.MATERIA_NICK];

            LoggingManagementService.WriteLine($"{nameof(DictionaryManagementService)} initialized", "DicMgmt");
        }

        public static void Dispose()
        {
            World = null;
            Materia = null;
            Retainer = null;
            Item = null;
            Dye = null;
            Map = null;
            RetainerAbbr = null;
            LocalizedWorld = null;
            MateriaAbbr = null;

            LoggingManagementService.WriteLine($"{nameof(DictionaryManagementService)} uninitialized", "DicMgmt");
        }

        public static Dictionary<int, string> SearchItem(string key, bool inclNotSell = false)
        {
            var regex = new Regex(key);

            if(inclNotSell)
            {
                return Item.Where(x => regex.IsMatch(x.Value)).ToDictionary(x => x.Key, x => x.Value);
            }

            return Item.Where(x => x.Key > 0 && regex.IsMatch(x.Value)).ToDictionary(x => x.Key, x => x.Value);
        }

        public static string GetName(int key)
        {
            return Item.TryGetValue(Math.Abs(key), out var name) || Item.TryGetValue(-Math.Abs(key), out name) ? name : "无法获取名称";
        }
    }
}
