using Milvaneth.Common;
using Thaliak.Readers;

namespace Thaliak.Converters
{
    internal class StatusCharaInfoConverter
    {
        internal static StatusCharaInfo FromStruct(MemoryCharacterMap cm, MemoryCharacterExtra ce, int gil)
        {
            return new StatusCharaInfo
            {
                Id = cm.ID,
                IsGm = cm.IsGM,
                Title = ce.Title,
                Job = ce.Job,
                Level = ce.Level,
                Icon = ce.Icon,
                Status = ce.Status,
                GilHold = gil,
            };
        }
    }
}
