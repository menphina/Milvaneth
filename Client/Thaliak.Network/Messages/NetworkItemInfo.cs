using System.Runtime.InteropServices;
using Milvaneth.Common;
using Thaliak.Network.Utilities;

namespace Thaliak.Network.Messages
{
    public class NetworkItemInfo : NetworkMessageProcessor, IResult
    {
        public int ContainerSequence;
        public int Unknown1;
        public short ContainerId;
        public short ContainerSlot;
        public int Quantity;
        public int ItemId;
        public int ReservedFlag;
        public long ArtisanId;
        public byte IsHq;
        public byte Unknown2;
        public short Condition;
        public short SpiritBond;
        public byte DyeId;
        public byte Unknown3;
        public int GlamourItemId;
        public short Materia1;
        public short Materia2;
        public short Materia3;
        public short Materia4;
        public short Materia5;
        public byte Buffer1;
        public byte Buffer2;
        public byte Buffer3;
        public byte Buffer4;
        public byte Buffer5;
        public byte Padding;
        public int Unknown4;

        public new static int GetMessageId()
        {
            return MessageIdRetriver.Instance.GetMessageId(MessageIdRetriveKey.NetworkItemInfo);
        }

        public new static unsafe NetworkItemInfo Consume(byte[] data, int offset)
        {
            fixed (byte* raw = &data[offset])
            {
                return (*(NetworkItemInfoRaw*)raw).Spawn(data, offset);
            }
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct NetworkItemInfoRaw : INetworkMessageBase<NetworkItemInfo>
    {
        [FieldOffset(0)]
        public int ContainerSequence;

        [FieldOffset(4)]
        public int Unknown1;

        [FieldOffset(8)]
        public short ContainerId;

        [FieldOffset(10)]
        public short ContainerSlot;

        [FieldOffset(12)]
        public int Quantity;

        [FieldOffset(16)]
        public int ItemId;

        [FieldOffset(20)]
        public int ReservedFlag;

        [FieldOffset(24)]
        public long ArtisanId;

        [FieldOffset(32)]
        public byte IsHq;

        [FieldOffset(33)]
        public byte Unknown2;

        [FieldOffset(34)]
        public short Condition;

        [FieldOffset(36)]
        public short SpiritBond;

        [FieldOffset(38)]
        public byte DyeId;

        [FieldOffset(39)]
        public byte Unknown3;

        [FieldOffset(40)]
        public int GlamourItemId;

        [FieldOffset(44)]
        public short Materia1;

        [FieldOffset(46)]
        public short Materia2;

        [FieldOffset(48)]
        public short Materia3;

        [FieldOffset(50)]
        public short Materia4;

        [FieldOffset(52)]
        public short Materia5;

        [FieldOffset(54)]
        public byte Buffer1;

        [FieldOffset(55)]
        public byte Buffer2;

        [FieldOffset(56)]
        public byte Buffer3;

        [FieldOffset(57)]
        public byte Buffer4;

        [FieldOffset(58)]
        public byte Buffer5;

        [FieldOffset(59)]
        public byte Padding;

        [FieldOffset(60)]
        public int Unknown4;

        public NetworkItemInfo Spawn(byte[] data, int offset)
        {
            return new NetworkItemInfo
            {
                ContainerSequence = this.ContainerSequence,
                Unknown1 = this.Unknown1,
                ContainerId = this.ContainerId,
                ContainerSlot = this.ContainerSlot,
                Quantity = this.Quantity,
                ItemId = this.ItemId,
                ReservedFlag = this.ReservedFlag,
                ArtisanId = this.ArtisanId,
                IsHq = this.IsHq,
                Unknown2 = this.Unknown2,
                Condition = this.Condition,
                SpiritBond = this.SpiritBond,
                DyeId = this.DyeId,
                Unknown3 = this.Unknown3,
                GlamourItemId = this.GlamourItemId,
                Materia1 = this.Materia1,
                Materia2 = this.Materia2,
                Materia3 = this.Materia3,
                Materia4 = this.Materia4,
                Materia5 = this.Materia5,
                Buffer1 = this.Buffer1,
                Buffer2 = this.Buffer2,
                Buffer3 = this.Buffer3,
                Buffer4 = this.Buffer4,
                Buffer5 = this.Buffer5,
                Padding = this.Padding,
                Unknown4 = this.Unknown4,
            };
        }
    }
}
