using Milvaneth.Common;
using System.Collections.Generic;
using Thaliak.Components;
using Thaliak.Signatures;

namespace Thaliak.Readers
{
    internal partial class DataReader
    {
        private GameService _gs;
        private readonly int _szMps, _szMcm, _szMce, _szMpe;
        internal DataReader(GameService gs)
        {
            _gs = gs;
            _szMps = Signature.PointerLib[PointerType.PlayerStat].Length;
            _szMcm = Signature.PointerLib[PointerType.CharacterMap].Length;
            _szMce = Signature.PointerLib[PointerType.CharacterExtra].Length;
            _szMpe = Signature.PointerLib[PointerType.ArtisanList].Length;
        }

        internal unsafe MemoryPlayerStat GetPlayerStat()
        {
            var ptr = _gs.GetPointer(PointerType.PlayerStat);
            var arr = _gs.Reader.Read(ptr, _szMps);
            MemoryPlayerStat mps;

            fixed (byte* p = &arr[0])
            {
                mps = *(MemoryPlayerStat*) p;
            }

            return mps;
        }

        internal unsafe MemoryCharacterMap GetCharacterMap()
        {
            var ptr = _gs.GetPointer(PointerType.CharacterMap);
            var arr = _gs.Reader.Read(ptr, _szMcm);
            MemoryCharacterMap mcm;

            fixed (byte* p = &arr[0])
            {
                mcm = *(MemoryCharacterMap*)p;
            }

            return mcm;
        }

        internal unsafe MemoryCharacterExtra GetCharacterExtra()
        {
            var ptr = _gs.GetPointer(PointerType.CharacterExtra);
            var arr = _gs.Reader.Read(ptr, _szMce);
            MemoryCharacterExtra mce;

            fixed (byte* p = &arr[0])
            {
                mce = *(MemoryCharacterExtra*)p;
            }

            return mce;
        }

        internal List<MemoryArtisanEntity> GetArtisanList()
        {
            var off = 0;
            var lst = new List<MemoryArtisanEntity>();

            while (GetArtisanEntity(off, out var mpe))
            {
                lst.Add(mpe);
                off += Signature.PointerLib[PointerType.ArtisanList].DtStep;
            }

            return lst;
        }
        private unsafe bool GetArtisanEntity(int offset, out MemoryArtisanEntity mpe)
        {
            mpe = null;

            var ptr = _gs.GetPointer(PointerType.ArtisanList);
            var arr = _gs.Reader.Read(ptr + offset, _szMpe);

            MemoryArtisanEntityRaw mper;
            fixed (byte* p = &arr[0])
            {
                mper = *(MemoryArtisanEntityRaw*)p;
            }

            if (mper.CharacterId == 0) return false;

            var str = Helper.ToUtf8String(arr, 0, 0, 0x40);

            mpe = new MemoryArtisanEntity
            {
                CharacterId = mper.CharacterId,
                CharacterName = str,
            };

            return true;
        }
    }
}
