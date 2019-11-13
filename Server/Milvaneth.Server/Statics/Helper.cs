using Microsoft.AspNetCore.Http;
using System;

namespace Milvaneth.Common
{
    public static class Helper
    {
        private const long TicksEpochTime = 621355968000000000L;
        private const long TicksPerSecond = 10000000L;
        private static readonly long TicksShift = (DateTime.Now - DateTime.UtcNow).Ticks;

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp, bool local = true)
        {
            var time = new DateTime(TicksEpochTime + TicksPerSecond * unixTimeStamp, DateTimeKind.Utc);

            if (local)
                return time.ToLocalTime();

            return time;
        }

        public static long DateTimeToUnixTimeStamp(DateTime dateTime)
        {
            return (dateTime.ToUniversalTime().Ticks - TicksEpochTime) / TicksPerSecond;
        }

        public static string GetIp(this IHttpContextAccessor accessor)
        {
            return accessor?.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "NULL";
        }

        public static bool BetweenMinutes(this TimeSpan time, int min, int max)
        {
            return time.Minutes >= min && time.Minutes <= max;
        }

        public static long EstimateNumber(long number, int digit)
        {
            var negative = false;
            if (number < 0)
            {
                if (number == long.MinValue)
                    number++;

                negative = true;
                number = -number;
            }

            var distortion = (long)Math.Pow(Math.E, Math.Floor(Math.Log(number) * 10) / 10);
            var basicTruncate = distortion > 100 ? distortion - (distortion % 100) : distortion - (distortion % 10);

            var totalDigits = Math.Ceiling(Math.Log10(basicTruncate + 0.1));
            var remainLimiter = (long) Math.Pow(10, totalDigits - digit);
            var result = basicTruncate - (basicTruncate % (remainLimiter > 0 ? remainLimiter : 1));
            return negative ? -result : result;
        }

        public static string ToCode(byte[] source)
        {
            var charset = new[]
            {
                '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N',
                'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'
            };

            var length = (source.Length * 8 / 5);

            var buffer = new char[length];

            for (var i = 0; i < length; i++)
            {
                buffer[i] = charset[GetCross(source, i)];
            }

            return new string(buffer);
        }

        private static byte GetCross(byte[] arr, int m)
        {
            // test data: 0b11110_000,0b01_11101_0,0b0010_1110,0b0_00011_11,0b011_00100,0b11010_001,0b01_11001_0,0b0110_1100,0b0_00111_10,0b111_01000,

            var start = m * 5 / 8;
            var end = ((m + 1) * 5 - 1) / 8;

            var a = arr[start];
            var b = arr[end];

            switch (m % 8)
            {
                case 0:
                    return (byte) ((b & 0b1111_1000) >> 3);
                case 1:
                    return (byte) (((a & 0b0000_0111) << 2) | ((b & 0b1100_0000) >> 6));
                case 2:
                    return (byte) ((b & 0b0011_1110) >> 1);
                case 3:
                    return (byte) (((a & 0b0000_0001) << 4) | ((b & 0b1111_0000) >> 4));
                case 4:
                    return (byte) (((a & 0b0000_1111) << 1) | ((b & 0b1000_0000) >> 7));
                case 5:
                    return (byte) ((a & 0b0111_1100) >> 2);
                case 6:
                    return (byte) (((a & 0b0000_0011) << 3) | ((b & 0b1110_0000) >> 5));
                case 7:
                    return (byte) ((b & 0b0001_1111) >> 0);
                default:
                    return (byte) 0;
            }
        }
    }
}
