using System;
using Milvaneth.Common;

namespace Milvaneth.Overlay
{
    public class ListingData : IMarketData
    {
        public int ItemId { get; set; }
        public bool IsHq { get; set; }
        public int[] Materia { get; set; }
        public int UnitPrice { get; set; }
        public int Quantity { get; set; }
        [Obsolete("Please DO NOT use this to display tax value as it contains unprocessed tax data committed.")]
        public int Tax { get; set; }
        public string Retainer { get; set; }
        public string Artisan { get; set; }
        public bool OnMannequin { get; set; }
        public string PlayerName { get; set; } // When on mannequin, character name is obtainable in client
        public int RetainerLocation { get; set; }
        public int DyeId { get; set; }
        public DateTime ListingTime { get; set; } // this is not impossible to obtain, as one can watch the MB constantly
        public DateTime UpdateTime { get; set; }
        public int Zone { get; set; }
        public int World { get; set; }

        public static ListingData FromResultItem(MarketListingItem item, DateTime time, int zone, int world)
        {
            var materia = new int[item.MateriaCount];

            do
            {
                if (item.MateriaCount == 0) break;
                materia[0] = item.Materia1;

                if (item.MateriaCount == 1) break;
                materia[1] = item.Materia2;

                if (item.MateriaCount == 2) break;
                materia[2] = item.Materia3;

                if (item.MateriaCount == 3) break;
                materia[3] = item.Materia4;

                if (item.MateriaCount == 4) break;
                materia[4] = item.Materia5;
            } while (false);

            return new ListingData
            {
                ItemId = item.ItemId,
                IsHq = item.IsHq != 0,
                Materia = materia,
                UnitPrice = item.UnitPrice,
                Quantity = item.Quantity,
                Tax = item.TotalTax,
                Retainer = item.RetainerName,
                Artisan = item.ArtisanName,
                OnMannequin = item.OnMannequin != 0,
                PlayerName = item.PlayerName,
                RetainerLocation = item.RetainerLocation,
                DyeId = item.DyeId,
                ListingTime = Helper.UnixTimeStampToDateTime(item.UpdateTime),
                UpdateTime = time,
                Zone = zone,
                World = world,
            };
        }
    }
}
