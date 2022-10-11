using System;
using System.Drawing;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace raw_ws.Helpers
{
    public static class F
    {
        public static DateTime ConvertJavaMiliSecondToDateTime(string javaMS)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Add(new TimeSpan(Convert.ToInt64(javaMS) * TimeSpan.TicksPerMillisecond)).ToLocalTime();
        }

        public static string ConvertDateTimeToJavaMiliSecond(DateTime date)
        {
            return "" + (date - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static Bitmap CreateThumbnail(Bitmap PassedImage, int MaxSide, int MinSize)
        {
            if (PassedImage.Width < PassedImage.Height)
            {
                int tempSize;
                tempSize = MinSize;
                MinSize = MaxSide;
                MaxSide = tempSize;
            }
            return new Bitmap(PassedImage, new Size(MaxSide, MinSize));
        }

        public static string Normalize(string input)
        {
            return new Regex("[^a-zA-Z0-9_ ().-]").Replace(input.Normalize(NormalizationForm.FormD), "");
        }

        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }

        public static double CurrentTimeMillis()
        {
            return (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds;
        }

        public static class Security
        {
            public static string Encrypt(string plainText, string key)
            {
                return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(plainText), GetRijndaelManaged(key)));
            }

            public static string Decrypt(string encryptedText, string key)
            {
                return Encoding.UTF8.GetString(Decrypt(Convert.FromBase64String(encryptedText), GetRijndaelManaged(key)));
            }

            private static RijndaelManaged GetRijndaelManaged(string secretKey)
            {
                var keyBytes = new byte[16];
                var secretKeyBytes = Encoding.UTF8.GetBytes(secretKey);
                Array.Copy(secretKeyBytes, keyBytes, Math.Min(keyBytes.Length, secretKeyBytes.Length));
                return new RijndaelManaged
                {
                    Mode = CipherMode.CBC,
                    Padding = PaddingMode.PKCS7,
                    KeySize = 128,
                    BlockSize = 128,
                    Key = keyBytes,
                    IV = keyBytes
                };
            }

            private static byte[] Encrypt(byte[] plainBytes, RijndaelManaged rijndaelManaged)
            {
                return rijndaelManaged.CreateEncryptor().TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            }

            private static byte[] Decrypt(byte[] encryptedData, RijndaelManaged rijndaelManaged)
            {
                return rijndaelManaged.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);
            }

            private static string GetMd5Hash(string input)
            {
                MD5 md5Hash = MD5.Create();

                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder sBuilder = new StringBuilder();

                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                return sBuilder.ToString();
            }

            private static int getControlDigit()
            {
                return (int)(F.CurrentTimeMillis() / 1000000) % 1000;
            }

            public static string getKey()
            {
                return GetMd5Hash("" + getControlDigit());
            }

        }

    }
}
