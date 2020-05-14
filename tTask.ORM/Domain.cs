using System;
using System.Security.Cryptography;
using System.Text;

namespace tTask.ORM
{
    public static class Domain
    {
        public static byte[] GetHash(string input)
        {
            using (HashAlgorithm alg = SHA256.Create())
                return alg.ComputeHash(Encoding.UTF8.GetBytes(input));
        }

        public static string GetHashString(string input, string salt = "")
        {
            string saltAndIn = String.Concat(input, salt);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(saltAndIn))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }

        public static string GetDomainHash(string input)
        {
            string salt = GetHashString(input);
            string hash = GetHashString(input, salt);
            return hash;
        }
    }
}
