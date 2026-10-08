using System;
using System.Security.Cryptography;

namespace InternManagement.Core.Helpers
{
    /// <summary>
    /// Băm mật khẩu bằng PBKDF2 + salt ngẫu nhiên. CSDL chỉ lưu chuỗi băm, không lưu mật khẩu gốc.
    /// Định dạng lưu: "số_vòng_lặp.salt_base64.hash_base64".
    /// </summary>
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;
        private const int Iterations = 10000;

        public static string Hash(string password)
        {
            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = Derive(password, salt, Iterations);
            return Iterations + "." + Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
        }

        public static bool Verify(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            string[] parts = storedHash.Split('.');
            if (parts.Length != 3)
                return false;

            int iterations = int.Parse(parts[0]);
            byte[] salt = Convert.FromBase64String(parts[1]);
            byte[] expected = Convert.FromBase64String(parts[2]);
            byte[] actual = Derive(password, salt, iterations);

            // So sánh từng byte, không dừng sớm (tránh đoán mật khẩu qua thời gian phản hồi)
            int diff = expected.Length ^ actual.Length;
            for (int i = 0; i < expected.Length && i < actual.Length; i++)
                diff |= expected[i] ^ actual[i];
            return diff == 0;
        }

        private static byte[] Derive(string password, byte[] salt, int iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations))
            {
                return pbkdf2.GetBytes(HashSize);
            }
        }
    }
}
