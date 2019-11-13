using Milvaneth.Common;
using System.Collections.Generic;
using Thaliak.Network.Dispatcher;
using Thaliak.Network.Utilities;

namespace Thaliak.Writer
{
    internal class Net
    {
        internal static void SetMessageId(int dataVersion)
        {
            var msgId = new Dictionary<int, int>
            {
                [(int)MessageIdRetriveKey.NetworkMarketHistory] = 0x012A, // standalone op, 浏览交易板, 典型尺寸1080, 单个, 系列第二个，0140
                [(int)MessageIdRetriveKey.NetworkRetainerHistory] = 0x012B, // standalone op, 浏览雇员交易历史, 典型尺寸1080, 单个，0148
                [(int)MessageIdRetriveKey.NetworkMarketListing] = 0x0126, // standalone op, 浏览交易板, 典型尺寸1560, 多个不连续, 系列第三个，013C
                [(int)MessageIdRetriveKey.NetworkMarketListingCount] = 0x0125, // standalone op, 浏览交易板, 典型尺寸48, 单个, 系列最早，013B
                [(int)MessageIdRetriveKey.NetworkMarketResult] = 0x0139, // standalone op, 浏览交易板总览, 典型尺寸208, 多个不连续，014A
                [(int)MessageIdRetriveKey.NetworkPlayerSpawn] = 0x0175, // standalone op, 等待玩家出生，017F
                [(int)MessageIdRetriveKey.NetworkItemInfo] = 0x0196, // transactional op, 召唤雇员, 典型尺寸96, 极大量，01A1
                [(int)MessageIdRetriveKey.NetworkItemPriceInfo] = 0x0194, // transactional op, 点进雇员, 典型尺寸64, 数量等同在售数量, 在12002容器ItemInfo后传输，019F
                [(int)MessageIdRetriveKey.NetworkRetainerSummary] = 0x0192, // procedure op, 调出雇员列表, 典型尺寸112, 目前固定10个，019D
                [(int)MessageIdRetriveKey.NetworkRetainerSumEnd] = 0x0190, // procedure op, 紧接RetainerSummary后, 典型尺寸56, 单个，019C
                [(int)MessageIdRetriveKey.NetworkItemInfoEnd] = 0x0193, // procedure op, 常见于ItemPriceInfo后, 典型尺寸40（多候选）, 单个，019E
                [(int)MessageIdRetriveKey.NetworkUpdateHpMpTp] = 0x0145, // signal op, 闲时不定时发送, 典型尺寸48, 包含HT MP TP，0145
                [(int)MessageIdRetriveKey.NetworkCharacterName] = 0x018E, // standalone op, 查看物品制作人名时, 典型尺寸72, 按需触发，0199

                // below zero means client packet
                [(int)MessageIdRetriveKey.NetworkLogout] = -0x0074, // signal op, 登出, 单个，0074
                [(int)MessageIdRetriveKey.NetworkLogoutCancel] = -0x0075, // signal op, 取消登出, 单个，0075
                [(int)MessageIdRetriveKey.NetworkClientTrigger] = -0x0138, // transactional op, 许多时机(本处用雇员改价确定时), 典型尺寸64, 包含TriggerUpdateSelling，013A
                [(int)MessageIdRetriveKey.NetworkRequestRetainer] = -0x0161, // procedure op, 召唤雇员, 典型尺寸48, 单个，0151
                [(int)MessageIdRetriveKey.NetworkInventoryModify] = -0x0146, // transactional op, 许多时机(本处用雇员新增/取消出售时), 典型尺寸80, 在ClientTrigger前，0148

                // OPMASK_LOBBY means lobby package
                [(int)MessageIdRetriveKey.NetworkLobbyService] = MessageDispatcher.OPMASK_LOBBY | 0x000C, // standalone op, 不预期有变化, 以包含SHANDA字样为特征
                [(int)MessageIdRetriveKey.NetworkLobbyCharacter] = MessageDispatcher.OPMASK_LOBBY | 0x000D, // standalone op, 不预期有变化, 以包含JSON为特征

                // some non-opcode data are also included
                [(int)MessageIdRetriveKey.NetworkTriggerUpdateSelling] = 400, // THIS IS NOT A OPCODE BUT A COMMANDID, 不预期有变化, 以包含新价格值为特征
            };

            msgId.Add(0, dataVersion); // will trigger exception when duplicate

            var path = Helper.GetMilFilePathRaw("network.pack");
            var ser = new Serializer<Dictionary<int, int>>(path, "");
            ser.Save(msgId);
        }
    }
}
