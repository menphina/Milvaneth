using Milvaneth.Common;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Definitions.Communication.Data;
using Milvaneth.Server.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Milvaneth.Server.Statics
{
    public static class DataMapper
    {
        public static CharacterData ToDb(this LobbyCharacterItem item, long serviceId)
        {
            var detail = JsonConvert.DeserializeObject<LobbyJson>(item.DetailJson);
            var hasJobLevels = detail?.ClassName == "ClientSelectData";

            var result = new CharacterData();
            {
                result.CharacterId = item.CharacterId;
                result.CharacterName = item.CharacterName;
                result.ServiceId = serviceId;
                result.HomeWorld = item.HomeWorldId;
                result.JobLevels = hasJobLevels ? LobbyJsonCharacter.MapFrom(detail.Content).CharacterLevels : null;
            }

            return result;
        }

        public static RetainerData ToDb(this RetainerInfoItem item, long characterId, int world)
        {
            var result = new RetainerData();
            {
                result.RetainerId = item.RetainerId;
                result.RetainerName = item.RetainerName;
                result.Character = characterId;
                result.Location = item.RetainerLocation;
                result.World = world;
            }

            return result;
        }

        public static ListingData ToDb(this MarketListingItem item, DateTime reportTime, int world)
        {
            var result = new ListingData();
            {
                result.ReportTime = reportTime;
                result.World = world;
                result.ListingId = item.ListingId;
                result.RetainerId = item.RetainerId;
                result.OwnerId = item.OwnerId;
                result.ArtisanId = item.ArtisanId;
                result.UnitPrice = item.UnitPrice;
                result.TotalTax = item.TotalTax;
                result.Quantity = item.Quantity;
                result.ItemId = item.ItemId;
                result.UpdateTime = item.UpdateTime;
                result.ContainerId = item.ContainerId;
                result.SlotId = item.SlotId;
                result.Condition = item.Condition;
                result.SpiritBond = item.SpiritBond;
                result.Materia1 = item.Materia1;
                result.Materia2 = item.Materia2;
                result.Materia3 = item.Materia3;
                result.Materia4 = item.Materia4;
                result.Materia5 = item.Materia5;
                result.RetainerName = item.RetainerName;
                result.PlayerName = item.PlayerName;
                result.IsHq = item.IsHq == 1;
                result.MateriaCount = item.MateriaCount;
                result.OnMannequin = item.OnMannequin == 1;
                result.RetainerLoc = item.RetainerLocation;
                result.DyeId = item.DyeId;
            }

            return result;
        }

        public static ListingResponseItem FromDb(this ListingData item, string artisanName)
        {
            var result = new MarketListingItem();
            {
                result.ListingId = 0;
                result.RetainerId = 0;
                result.OwnerId = 0;
                result.ArtisanId = 0;
                result.ArtisanName = artisanName;
                result.UnitPrice = item.UnitPrice;
                result.TotalTax = item.TotalTax;
                result.Quantity = item.Quantity;
                result.ItemId = item.ItemId;
                result.UpdateTime = 0;
                result.ContainerId = 0;
                result.SlotId = 0;
                result.Condition = item.Condition;
                result.SpiritBond = item.SpiritBond;
                result.Materia1 = item.Materia1;
                result.Materia2 = item.Materia2;
                result.Materia3 = item.Materia3;
                result.Materia4 = item.Materia4;
                result.Materia5 = item.Materia5;
                result.RetainerName = item.RetainerName;
                result.PlayerName = item.PlayerName;
                result.IsHq = item.IsHq ? (byte)1 : (byte)0;
                result.MateriaCount = (byte)item.MateriaCount;
                result.OnMannequin = item.OnMannequin ? (byte)1 : (byte)0;
                result.RetainerLocation = (byte)item.RetainerLoc;
                result.DyeId = item.DyeId;
            }

            return new ListingResponseItem
            {
                ReportTime = item.ReportTime,
                WorldId = item.World,
                RawItem = result
            };
        }

        public static HistoryData ToDb(this MarketHistoryItem item, DateTime reportTime, int world)
        {
            var result = new HistoryData();
            {
                result.ReportTime = reportTime;
                result.World = world;
                result.ItemId = item.ItemId;
                result.UnitPrice = item.UnitPrice;
                result.PurchaseTime = item.PurchaseTime;
                result.Quantity = item.Quantity;
                result.IsHq = item.IsHq == 1;
                result.OnMannequin = item.OnMannequin == 1;
                result.BuyerName = item.BuyerName;
            }

            return result;
        }

        public static HistoryResponseItem FromDb(this HistoryData item)
        {
            var result = new MarketHistoryItem();
            {
                result.ItemId = item.ItemId;
                result.UnitPrice = item.UnitPrice;
                result.PurchaseTime = (int)item.PurchaseTime;
                result.Quantity = item.Quantity;
                result.IsHq = item.IsHq ? (byte)1 : (byte)0;
                result.OnMannequin = item.OnMannequin ? (byte)1 : (byte)0;
                result.BuyerName = item.BuyerName;
            }

            return new HistoryResponseItem
            {
                ReportTime = item.ReportTime,
                WorldId = item.World,
                RawItem = result
            };
        }

        public static short[] ToDb(this StatusLevelInfo info)
        {
            return new []
            {
                info.PGL, info.GLA, info.MRD, info.ARC, info.LNC, info.THM, info.CNJ, // Before The Calamity
                info.CRP, info.BSM, info.ARM, info.GSM, info.LTW, info.WVR, info.ALC, info.CUL, // Disciple of the Hand
                info.MIN, info.BTN, info.FSH, // Disciple of the Land
                info.ACN, info.ROG, // ARR New
                info.MCH, info.DRK, info.AST, // HW New
                info.SAM, info.RDM, info.BLU, // StB New
                info.GNB, info.DNC // ShB New
            };
        }

        public static OverviewData ToDb(this MarketOverviewItem item, DateTime reportTime, int world)
        {
            var result = new OverviewData();
            {
                result.ReportTime = reportTime;
                result.World = world;
                result.ItemId = item.ItemId;
                result.OpenListing = item.OpenListing;
                result.Demand = item.Demand;
            }

            return result;
        }

        public static OverviewResponseItem FromDb(this OverviewData item)
        {
            var result = new OverviewResponseItem();
            {
                result.ItemId = item.ItemId;
                result.OpenListing = item.OpenListing;
                result.Demand = item.Demand;
                result.World = item.World;
                result.ReportTime = item.ReportTime;
            }

            return result;
        }

        #region LobbyJsonParseHelper

        public class LobbyJsonCharacter
        {
            public static LobbyJsonCharacter MapFrom(JArray content)
            {
                return new LobbyJsonCharacter
                {
                    CharacterName = (string)content[0],
                    CharacterLevels = ((JArray)content[1]).Select(x => short.Parse((string)x)).ToArray(),
                    UNK1 = (string)content[2],
                    UNK2 = (string)content[3],
                    UNK3 = (string)content[4],
                    BirthMonth = byte.Parse((string)content[5]),
                    BirthDay = byte.Parse((string)content[6]),
                    GuardianDeity = byte.Parse((string)content[7]),
                    ClassId = byte.Parse((string)content[8]),
                    UNK4 = (string)content[9],
                    ZoneId = int.Parse((string)content[10]),
                    UNK5 = (string)content[11],
                    Appearances = ((JArray)content[12]).Select(x => byte.Parse((string)x)).ToArray(),
                    MainHandModel = long.Parse((string)content[13]),
                    OffHandModel = long.Parse((string)content[14]),
                    Models = ((JArray)content[15]).Select(x => int.Parse((string)x)).ToArray(),
                    UNK6 = (string)content[16],
                    UNK7 = (string)content[17],
                    UNK8 = (string)content[18],
                    UNK9 = (string)content[19],
                    EquipDisplayFlags = short.Parse((string)content[20]),
                    UNK10 = (string)content[21],
                    UNK11 = (string)content[22],
                    UNK12 = (string)content[23],
                    UNK13 = (string)content[24],
                };
            }

            public string CharacterName;
            public short[] CharacterLevels;
            public string UNK1;
            public string UNK2;
            public string UNK3;
            public byte BirthMonth;
            public byte BirthDay;
            public byte GuardianDeity;
            public byte ClassId;
            public string UNK4;
            public int ZoneId;
            public string UNK5;
            public byte[] Appearances;
            public long MainHandModel;
            public long OffHandModel;
            public int[] Models;
            public string UNK6;
            public string UNK7;
            public string UNK8;
            public string UNK9;
            public short EquipDisplayFlags;
            public string UNK10;
            public string UNK11;
            public string UNK12;
            public string UNK13;
        }

        public class LobbyJson
        {
            public JArray Content;
            public string ClassName;
            public int ClassId;
        }

        #endregion
    }
}
