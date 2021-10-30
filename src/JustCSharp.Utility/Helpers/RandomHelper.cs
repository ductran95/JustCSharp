using System;
using System.Text;

namespace JustCSharp.Utility.Helpers
{
    public static class RandomHelper
    {
        private static readonly char[] nonCaseSensitiveChars = "ABCDEFGHIJKLOMNOPQRSTUVWXYZ0123456789".ToCharArray();

        private static readonly char[] caseSensitiveChars =
            "abcdefghijklomnopqrstuvwxyzABCDEFGHIJKLOMNOPQRSTUVWXYZ0123456789".ToCharArray();

        private static readonly Random _random = new Random();

        public static string CreateRandomString(int length, bool caseSensitive = false)
        {
            //return sb.ToString();
            var sb = new StringBuilder();
            var rand = new Random();
            // Characters we will use to generate this random string.

            var allowableChars = caseSensitive ? caseSensitiveChars : nonCaseSensitiveChars;

            // Start generating the random string.
            for (int i = 0; i <= length - 1; i++)
            {
                sb.Append(allowableChars[rand.Next(allowableChars.Length - 1)]);
            }

            return sb.ToString();
        }
    }
}