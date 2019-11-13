using Milvaneth.Common;
using System;
using System.Text.RegularExpressions;

namespace Milvaneth.Communication.Vendor
{
    public class CheckVendor
    {
        public static bool NotValidService(LobbyServiceResult service)
        {
            return service == null ||
                   !service.ServiceProvider.Equals("SHANDA", StringComparison.InvariantCultureIgnoreCase) ||
                   service.ServiceId == 0;
        }

        public static bool NotValidCharacter(LobbyCharacterResult character)
        {
            return character == null || !character.CharacterItems.TrueForAll(x => 
                       x != null &&
                       Regex.IsMatch(x.CurrentWorldName, @"^[a-zA-Z]+$") &&
                       x.DetailJson.StartsWith("{\"content\":[\"") &&
                       x.DetailJson.Contains(x.CharacterName));
        }

        public static bool NotValidTrace(long[] trace)
        {
            return trace == null || trace.Length < 1;
        }

        public static bool NotValidUsername(string username)
        {
            return username.Length < 4 ||
                   username.Length > 16 ||
                   !Regex.IsMatch(username, @"^[a-zA-Z][a-zA-Z0-9_]+$");
        }

        public static bool NotValidDisplayName(string displayName)
        {
            return (!string.IsNullOrEmpty(displayName)) && (
                       displayName.Length < 2 ||
                       displayName.Length > 12 ||
                       !Regex.IsMatch(displayName, @"^[a-zA-Z0-9_\u2E80-\u9FFF]+$"));
        }

        public static bool NotValidEmail(string email)
        {
            return !string.IsNullOrEmpty(email) && !Regex.IsMatch(email,
                @"^[a-zA-Z0-9.+=_~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,17}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9][a-zA-Z0-9-](?:[a-zA-Z0-9-]{0,13}[a-zA-Z0-9])?){1,2}$");
        }

        public static bool NotValidPassword(string password)
        {
            return !Regex.IsMatch(password, @"^(?=.*[\u2E80-\u9FFF]).{4,}$") &&
                   !Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$");
        }

        public static bool NotValidPassword(byte[] password)
        {
            // average entropy of top 100 common passwords
            return Entropy(password) <= 2.4519;
        }

        public static bool NotValidSessionToken(string token)
        {
            return string.IsNullOrWhiteSpace(token) || token.Length < 16;
        }

        public static bool NotValidRenewToken(byte[] token)
        {
            return token == null;
        }

        public static bool NotValidResponse(IMilvanethResponse data)
        {
            return data == null || data.Message / 10000 != 0;
        }

        public static bool NotValidResponseCode(int code)
        {
            return code / 10000 != 0;
        }

        public static bool NotValidCode(string code)
        {
            return !Regex.IsMatch(code, @"^[A-Za-z0-9]{8}$");
        }

        public static bool NotValidData(MilvanethProtocol mp)
        {
            return mp.Context == null || !(mp.Data is PackedResult);
        }

        public static bool NotValidItemId(int id)
        {
            return id <= 1;
        }

        private static double Entropy(byte[] s)
        {
            double len = s.Length;
            var result = 0d;
            var map = new int[256];

            for (var i = 0; i < (int)len; i++)
                map[s[i]]++;

            foreach (var item in map)
            {
                var frequency = item / len;
                if (frequency > 0)
                    result -= frequency * Math.Log(frequency, 2);
            }
            return result;
        }
    }
}
