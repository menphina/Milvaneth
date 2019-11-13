using Milvaneth.Common;
using System;
using System.Collections.Generic;
using Thaliak.Signatures;

namespace Thaliak.Writer
{
    internal class Sig
    {
        private const string encKey = "";

        // here we disabled Market signature as we have network monitor.
        private static readonly Dictionary<SignatureType, SigRecord> signatureLib =
            new Dictionary<SignatureType, SigRecord>
            {
                [SignatureType.ChatLog] = new SigRecord("e8********85c0740e488b0d********33D2E8********488b0d", SignatureType.ChatLog),
                [SignatureType.ServerTime] = new SigRecord("0fb7c0894710488b0d", SignatureType.ServerTime),
                [SignatureType.MapInfo] = new SigRecord("f30f108d080400004c8d85580600000fb705", SignatureType.MapInfo),
                [SignatureType.PlayerStat] = new SigRecord("83f9ff7412448b048e8bd3488d0d", SignatureType.PlayerStat),
                [SignatureType.Inventory] = new SigRecord("48895C2408488974241048897C2418488B3D", SignatureType.Inventory),
                //[SignatureType.CurrentGil] = new SigRecord("418d8118fcffff85c075584c8b05", SignatureType.CurrentGil),
                [SignatureType.ArtisanList] = new SigRecord("488BD990803B0075**8B5340488D0D", SignatureType.ArtisanList),
                [SignatureType.Combatant] = new SigRecord("488b420848c1e8033da701000077248bc0488d0d", SignatureType.Combatant),

                [SignatureType.ThreadStack] = new SigRecord("4d5a", SignatureType.ThreadStack), // MZ
                [SignatureType.Invalid] = new SigRecord("4d5a", SignatureType.Invalid),
            };

        private static readonly Dictionary<PointerType, PtrRecord> pointerLib =
            new Dictionary<PointerType, PtrRecord>
            {
                [PointerType.ChatLogEntry] = new PtrRecord(new[] { 0, 176, 1144 }, 952, 48),
                [PointerType.ServerTime] = new PtrRecord(new[] { 0, 0x48, 0x8 }, 0x844, 4),
                [PointerType.MapInfo] = new PtrRecord(new int[] { }, 0, 4),
                [PointerType.SessionUpTime] = new PtrRecord(new int[] { }, 0x24, 4), // Game up time
                [PointerType.PlayerStat] = new PtrRecord(new int[] { }, 0, 616),
                [PointerType.CharacterMap] = new PtrRecord(new[] {0}, 0, 400),
                [PointerType.CharacterExtra] = new PtrRecord(new[] {0}, 6164, 245),
                [PointerType.Inventory] = new PtrRecord(new[] {0}, 0, 56, 56), // Pointer Array
                [PointerType.CurrentGil] = new PtrRecord(new[] {0, 0x78}, 0xC, 4),
                //[PointerType.LocalWorldName] = new PtrRecord(new[] {-0x3E8, 0x8}, 0x102, 32, 0, ThreadNr.Thread0), // TS pointer demo, may not stable
                [PointerType.ArtisanList] = new PtrRecord(new int[] { }, 0x3848, 0x48, 0x48),
            };

        internal static void SaveSearchPack(int dataVersion)
        {
            var data =
                new Tuple<int, Dictionary<SignatureType, SigRecord>, Dictionary<PointerType, PtrRecord>>(
                    dataVersion, signatureLib, pointerLib);
            var path = Helper.GetMilFilePathRaw("search.pack");
            var ser =
                new Serializer<Tuple<int, Dictionary<SignatureType, SigRecord>, Dictionary<PointerType, PtrRecord>>>(
                    path, encKey);

            ser.Save(data);
        }
    }

}
