using Milvaneth.Common;
using Milvaneth.Common.Communication.Auth;
using Milvaneth.Common.Communication.Data;
using Milvaneth.Common.Communication.Login;
using Milvaneth.Common.Communication.Recovery;
using Milvaneth.Common.Communication.Register;
using Milvaneth.Server.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace Milvaneth.Server.Statics
{
    public static class DataChecker
    {
        #region PacketCheck

        public static bool Check(this ClientChallenge data)
        {
            return data.Trace != null &&
                   data.Trace.Length >= 1 &&
                   IsValidReportTime(data.ReportTime) &&
                   IsValidUsername(data.Username) &&
                   IsValidPow(data.ProofOfWork);
        }

        public static bool Check(this ClientResponse data)
        {
            return data.ClientEvidence != null &&
                   data.ClientToken != null &&
                   IsValidReportTime(data.ReportTime);
        }

        public static bool Check(this RecoveryRequest data)
        {
            return GlobalConfig.SRP6_ALLOWED_PARAM_GROUP.Contains(data.GroupParam) &&
                   data.OperationToken != null &&
                   data.OperationToken.Length == GlobalConfig.TOKEN_LENGTH &&
                   data.Salt != null &&
                   data.Salt.Length >= GlobalConfig.SALT_MIN_LENGTH &&
                   data.Salt.Length <= GlobalConfig.SALT_MAX_LENGTH &&
                   data.Verifier != null &&
                   data.Verifier.Length >= 256 &&
                   data.Verifier.Length <= 1024 &&
                   IsValidReportTime(data.ReportTime);
        }

        public static bool Check(this RegisterForm data)
        {
            return data.Trace != null &&
                   data.Trace.Length >= 1 &&
                   GlobalConfig.SRP6_ALLOWED_PARAM_GROUP.Contains(data.GroupParam) &&
                   data.Salt != null &&
                   data.Salt.Length >= GlobalConfig.SALT_MIN_LENGTH &&
                   data.Salt.Length <= GlobalConfig.SALT_MAX_LENGTH &&
                   data.Verifier != null &&
                   data.Verifier.Length >= 256 &&
                   data.Verifier.Length <= 1024 &&
                   IsValidReportTime(data.ReportTime) &&
                   IsValidUsername(data.Username) &&
                   IsValidDisplayName(data.DisplayName) &&
                   IsValidEmail(data.Email) &&
                   IsValidPow(data.ProofOfWork) &&
                   CheckLobbyService(data.Service) &&
                   CheckLobbyCharacter(data.Character) &&
                   CheckOnlineCharacterBinding(data.Service.ServiceId, data.Character.CharacterItems);
        }

        public static bool Check(this AccountUpdate data)
        {
            return data.Trace != null &&
                   data.Trace.Length >= 1 &&
                   IsValidReportTime(data.ReportTime) &&
                   IsValidDisplayName(data.DisplayName);
        }

        public static bool Check(this RecoveryEmail data)
        {
            return data.Trace != null &&
                   data.Trace.Length >= 1 &&
                   IsValidReportTime(data.ReportTime) &&
                   IsValidUsername(data.Username) &&
                   IsValidEmail(data.Email);
        }

        public static bool Check(this RecoveryGame data)
        {
            return data.Trace != null &&
                   data.Trace.Length >= 1 &&
                   IsValidUsername(data.Username) &&
                   IsValidReportTime(data.ReportTime) &&
                   CheckLobbyService(data.Service) &&
                   CheckLobbyCharacter(data.Character) &&
                   CheckOnlineCharacterBinding(data.Service.ServiceId, data.Character.CharacterItems);
        }

        public static bool Check(this AuthRequest data)
        {
            return data.AuthToken != null &&
                   data.AuthToken.Length == GlobalConfig.TOKEN_LENGTH &&
                   IsValidReportTime(data.ReportTime) &&
                   IsValidUsername(data.Username);
        }

        public static bool Check(this AuthRenew data)
        {
            return data.RenewToken != null &&
                   data.RenewToken.Length == GlobalConfig.TOKEN_LENGTH &&
                   IsValidReportTime(data.ReportTime) &&
                   IsValidUsername(data.Username);
        }

        public static bool Check(this OverviewRequest data)
        {
            return data.QueryItems != null &&
                   data.QueryItems.All(x => x > 0) &&
                   IsValidReportTime(data.ReportTime);
        }

        public static bool Check(this MilvanethContext data)
        {
            return data != null &&
                   data.CharacterId != 0 &&
                   GlobalConfig.CHARA_SERVER_AVAILABLE.Contains(data.World);
        }

        public static bool Check(this PackedResult data)
        {
            return data != null &&
                   data.Result != null &&
                   IsValidReportTime(data.ReportTime) &&
                   Enum.IsDefined(typeof(PackedResultType), data.Type) &&
                   CheckResult(data.Result, data.Type);
        }

        #endregion

        #region StructureCheck

        private static bool CheckResult(IResult result, PackedResultType type)
        {
            switch (type)
            {
                case PackedResultType.Artisan:
                    if (!(result is ArtisanResult artisanResult))
                        return false;

                    return artisanResult.ArtisanList != null &&
                           artisanResult.ArtisanList.Count >= 0 &&
                           artisanResult.ArtisanList.TrueForAll(CheckArtisan);

                case PackedResultType.Chatlog:
                    // not upload
                    return result is ChatlogResult;

                case PackedResultType.Inventory:
                    if (!(result is InventoryResult inventoryResult))
                        return false;

                    return inventoryResult.Context > 0 &&
                           inventoryResult.InventoryContainers != null &&
                           inventoryResult.InventoryContainers.Count > 0;

                case PackedResultType.InventoryNetwork:
                    if (!(result is InventoryResult inventoryNetworkResult))
                        return false;

                    return inventoryNetworkResult.Context > 0 &&
                           inventoryNetworkResult.InventoryContainers != null &&
                           inventoryNetworkResult.InventoryContainers.Count > 0;

                case PackedResultType.MarketHistory:
                    if (!(result is MarketHistoryResult marketHistoryResult))
                        return false;

                    return marketHistoryResult.HistoryItems != null &&
                           marketHistoryResult.HistoryItems.Count >= 0 &&
                           marketHistoryResult.HistoryItems.TrueForAll(CheckHistory);

                case PackedResultType.MarketListing:
                    if (!(result is MarketListingResult marketListingResult))
                        return false;

                    return marketListingResult.ListingItems != null &&
                           marketListingResult.ListingItems.Count >= 0 &&
                           marketListingResult.ListingItems.TrueForAll(CheckListing);

                case PackedResultType.MarketOverview:
                    if (!(result is MarketOverviewResult marketOverviewResult))
                        return false;

                    return marketOverviewResult.ResultItems != null &&
                           marketOverviewResult.ResultItems.Count >= 0 &&
                           marketOverviewResult.ResultItems.TrueForAll(CheckOverview);

                case PackedResultType.MarketRequest:
                    // not upload
                    return result is MarketRequestResult;

                case PackedResultType.RetainerHistory:
                    if (!(result is RetainerHistoryResult retainerHistoryResult))
                        return false;

                    return retainerHistoryResult.RetainerId != 0 &&
                           retainerHistoryResult.HistoryItems != null &&
                           retainerHistoryResult.HistoryItems.Count >= 0 &&
                           retainerHistoryResult.HistoryItems.TrueForAll(CheckRetainerHistory);

                case PackedResultType.RetainerList:
                    if (!(result is RetainerInfoResult retainerInfoResult))
                        return false;

                    return retainerInfoResult.RetainerInfo != null &&
                           retainerInfoResult.RetainerInfo.Count > 0 &&
                           retainerInfoResult.RetainerInfo.TrueForAll(CheckRetainerInfo);

                case PackedResultType.RetainerUpdate:
                    if (!(result is RetainerUpdateResult retainerUpdateResult))
                        return false;

                    return retainerUpdateResult.UpdateItems != null &&
                           retainerUpdateResult.UpdateItems.Count > 0 &&
                           retainerUpdateResult.UpdateItems.TrueForAll(CheckRetainerUpdate);

                case PackedResultType.Status:
                    if (!(result is StatusResult statusResult))
                        return false;

                    return statusResult.CharacterId != 0 &&
                           GlobalConfig.CHARA_SERVER_AVAILABLE.Contains(statusResult.CharacterHomeWorld) &&
                           GlobalConfig.CHARA_SERVER_AVAILABLE.Contains(statusResult.CharacterCurrentWorld) &&
                           statusResult.LevelInfo != null &&
                           statusResult.CharaInfo != null &&
                           statusResult.SessionTime > 0 &&
                           IsValidInGameName(statusResult.CharacterName, false) &&
                           IsValidReportTime(statusResult.ServerTime);

                case PackedResultType.CurrentWorld:
                    // not upload
                    return result is CurrentWorldResult;

                case PackedResultType.LobbyService:
                    if (!(result is LobbyServiceResult lobbyServiceResult))
                        return false;

                    return CheckLobbyService(lobbyServiceResult);

                case PackedResultType.LobbyCharacter:
                    if (!(result is LobbyCharacterResult lobbyCharacterResult))
                        return false;

                    return CheckLobbyCharacter(lobbyCharacterResult);

                default:
                    return false;
            }
        }

        private static bool CheckArtisan(ArtisanInfo info)
        {
            return info != null &&
                   info.CharacterId != 0 &&
                   IsValidInGameName(info.CharacterName, !info.IsValid);
        }

        private static bool CheckRetainerInfo(RetainerInfoItem info)
        {
            return info != null &&
                   info.RetainerId != 0 &&
                   info.ListingDueDate >= 0 &&
                   info.RetainerOrder <= 12 &&
                   info.ItemsInSell <= 20 &&
                   GlobalConfig.MARKET_RETAINER_LOCALTION.Contains(info.RetainerLocation) &&
                   IsValidInGameName(info.RetainerName);
        }

        private static bool CheckRetainerUpdate(RetainerUpdateItem info)
        {
            return info != null &&
                   info.RetainerId != 0 &&
                   info.ContainerId == GlobalConfig.MARKET_RETAINER_CONTAINER &&
                   info.ContainerSlot >= 0 &&
                   info.ContainerSlot <= 20 &&
                   (info.IsRemove ||
                    info.ItemInfo != null &&
                    info.ItemInfo.Amount > 0 &&
                    info.ItemInfo.Amount < GlobalConfig.MARKET_COUNT_THRESHOLD &&
                    info.OldPrice > 0 &&
                    info.OldPrice <= GlobalConfig.MARKET_PRICE_THRESHOLD / info.ItemInfo.Amount &&
                    info.NewPrice > 0 &&
                    info.NewPrice <= GlobalConfig.MARKET_PRICE_THRESHOLD / info.ItemInfo.Amount);
        }

        private static bool CheckLobbyService(LobbyServiceResult info)
        {
            return info != null &&
                   info.ServiceProvider == GlobalConfig.SERVICE_PROVIDER_ID &&
                   info.ServiceId != 0;
        }

        private static bool CheckLobbyCharacter(LobbyCharacterResult info)
        {
            return info != null &&
                   info.CharacterItems != null &&
                   info.CharacterItems.Count > 0 &&
                   info.CharacterItems.Count <= Math.Max(GlobalConfig.CHARA_SERVER_GROUP1.Length, GlobalConfig.CHARA_SERVER_GROUP2.Length) * 8 &&
                   info.CharacterItems.TrueForAll(CheckLobbyCharacterItem);
        }

        private static bool CheckLobbyCharacterItem(LobbyCharacterItem info)
        {
            return info != null &&
                   info.CharacterId != 0 &&
                   GlobalConfig.CHARA_SERVER_AVAILABLE.Contains(info.CurrentWorldId) &&
                   GlobalConfig.CHARA_SERVER_AVAILABLE.Contains(info.HomeWorldId) &&
                   !string.IsNullOrWhiteSpace(info.CurrentWorldName) &&
                   !string.IsNullOrWhiteSpace(info.HomeWorldName) &&
                   IsValidInGameName(info.CharacterName, false);
        }

        private static bool CheckRetainerHistory(RetainerHistoryItem info)
        {
            return info != null &&
                   info.IsHq < 2 &&
                   info.ItemId > 0 &&
                   info.OnMannequin < 2 &&
                   info.Quantity > 0 &&
                   info.Quantity <= GlobalConfig.MARKET_COUNT_THRESHOLD &&
                   info.TotalPrice > 0 &&
                   info.TotalPrice <= GlobalConfig.MARKET_PRICE_THRESHOLD &&
                   info.PurchaseTime > GlobalConfig.CHARA_SERVER_GENESIS &&
                   info.PurchaseTime < Helper.DateTimeToUnixTimeStamp(CachedTimeService.Utc) &&
                   IsValidInGameName(info.BuyerName, false);
        }

        private static bool CheckOverview(MarketOverviewItem item)
        {
            return item.ItemId > 0 &&
                   item.Demand >= 0 &&
                   item.OpenListing >= 0;
        }

        private static bool CheckListing(MarketListingItem item)
        {
            return item != null &&
                   item.ItemId > 0 &&
                   item.RetainerId != 0 &&
                   item.IsHq < 2 &&
                   item.OnMannequin < 2 &&
                   item.ListingId != 0 &&
                   item.OwnerId != 0 &&
                   item.UnitPrice > 0 &&
                   item.Quantity > 0 &&
                   item.Quantity <= GlobalConfig.MARKET_COUNT_THRESHOLD &&
                   item.UnitPrice <= GlobalConfig.MARKET_PRICE_THRESHOLD / item.Quantity && // no overflow
                   item.TotalTax <= item.UnitPrice * item.Quantity / 10 &&
                   IsValidInGameName(item.PlayerName) &&
                   IsValidInGameName(item.RetainerName, false);
        }

        private static bool CheckHistory(MarketHistoryItem item)
        {
            return item != null &&
                   item.ItemId > 0 &&
                   item.IsHq < 2 &&
                   item.OnMannequin < 2 &&
                   item.UnitPrice > 0 &&
                   item.Quantity > 0 &&
                   item.Quantity <= GlobalConfig.MARKET_COUNT_THRESHOLD &&
                   item.UnitPrice <= GlobalConfig.MARKET_PRICE_THRESHOLD / item.Quantity && // no overflow
                   IsValidInGameName(item.BuyerName, false);
        }

        #endregion

        #region BasicValidation

        private static readonly Regex NameCheck = new Regex(@"^[a-zA-Z0-9_\u2E80-\u9FFF]+$");
        private static readonly Regex EmailCheck = new Regex(@"^[a-zA-Z0-9.+=_~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,17}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9][a-zA-Z0-9-](?:[a-zA-Z0-9-]{0,13}[a-zA-Z0-9])?){1,2}$");
        private static readonly Regex UsernameCheck = new Regex(@"^[a-zA-Z][a-zA-Z0-9_]+$");

        private static bool IsValidUsername(string username)
        {
            return username.Length >= 4 ||
                   username.Length <= 16 ||
                   UsernameCheck.IsMatch(username);
        }

        private static bool IsValidPow(byte[] pow)
        {
            return pow == null || pow.Length == GlobalConfig.POW_LENGTH;
        }

        private static bool IsValidReportTime(DateTime report)
        {
            return (CachedTimeService.Utc - report).BetweenMinutes(0, GlobalConfig.INVOKE_DELAY_TOLERANCE);
        }

        private static bool IsValidDisplayName(string displayName)
        {
            return (string.IsNullOrEmpty(displayName)) || (
                       displayName.Length >= 2 ||
                       displayName.Length <= 12 ||
                       NameCheck.IsMatch(displayName));
        }

        private static bool IsValidInGameName(string name, bool allowEmpty = true)
        {
            return (allowEmpty && string.IsNullOrEmpty(name)) || (
                       name.Length >= 1 ||
                       name.Length <= 8 ||
                       NameCheck.IsMatch(name));
        }

        private static bool IsValidEmail(string email)
        {
            return string.IsNullOrEmpty(email) || EmailCheck.IsMatch(email);
        }

        #endregion

        #region OnlineChecking

        public static bool CheckOnlineCharacterBinding(uint serviceId, List<LobbyCharacterItem> characters)
        {
            // throw new NotImplementedException("Check service not available");
            return true;
        }

        #endregion

        #region Utilities

        public static bool WeightedSuccess(int success, int total, int referenceTotal)
        {
            if (referenceTotal != -1)
            {
                if (referenceTotal <= 3 && referenceTotal != total)
                {
                    return false;
                }

                if (total < (referenceTotal - (1 + referenceTotal / 5)))
                {
                    return false;
                }
            }

            if (total <= 3)
                return success >= total;

            if (total <= 6)
                return success >= total - 1;

            if (total <= 15)
                return success >= total - 2;

            return success >= total - 3;
        }

        #endregion
    }
}
