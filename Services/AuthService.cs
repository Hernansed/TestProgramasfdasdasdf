using Microsoft.Data.Sqlite;
using System;
using curs.Models;

namespace curs.Services
{
    public static class AuthService
    {
        public static string HashPassword(string pwd)
        {
            using var sha = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(pwd);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }

        public static User? Authenticate(string username, string password)
        {
            var hash = HashPassword(password);
            using var conn = new SqliteConnection($"Data Source={Data.Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Username, Role, Email, Name FROM Users WHERE Username=$u AND PasswordHash=$p";
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$p", hash);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                var user = new User { Id = reader.GetInt32(0), Username = reader.GetString(1), Role = reader.GetString(2) };
                if (!reader.IsDBNull(3)) user.Email = reader.GetString(3);
                if (!reader.IsDBNull(4)) user.Name = reader.GetString(4);
                return user;
            }
            return null;
        }

        public static bool EmailExists(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            using var conn = new SqliteConnection($"Data Source={Data.Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Email=$e";
            cmd.Parameters.AddWithValue("$e", email);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public static bool UsernameExists(string username)
        {
            if (string.IsNullOrWhiteSpace(username)) return false;
            using var conn = new SqliteConnection($"Data Source={Data.Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(1) FROM Users WHERE Username=$u";
            cmd.Parameters.AddWithValue("$u", username);
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            return count > 0;
        }

        public static bool Register(string username, string password, string? email = null, string? name = null, string role = "user")
        {
            var hash = HashPassword(password);
            using var conn = new SqliteConnection($"Data Source={Data.Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            if (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(name))
            {
                // register with email and name
                cmd.CommandText = "INSERT INTO Users (Username,PasswordHash,Role,Email,Name) VALUES ($u,$p,$r,$e,$n);";
                cmd.Parameters.AddWithValue("$e", email ?? string.Empty);
                cmd.Parameters.AddWithValue("$n", name ?? string.Empty);
            }
            else
            {
                cmd.CommandText = "INSERT INTO Users (Username,PasswordHash,Role) VALUES ($u,$p,$r);";
            }
            cmd.Parameters.AddWithValue("$u", username);
            cmd.Parameters.AddWithValue("$p", hash);
            cmd.Parameters.AddWithValue("$r", role);
            try
            {
                cmd.ExecuteNonQuery();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string? ResetPassword(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return null;
            using var conn = new SqliteConnection($"Data Source={Data.Database.DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id FROM Users WHERE Email=$e";
            cmd.Parameters.AddWithValue("$e", email);
            var obj = cmd.ExecuteScalar();
            if (obj == null || obj == DBNull.Value) return null;
            var id = Convert.ToInt32(obj);

            // generate new password
            var newPass = GenerateRandomPassword(10);
            var hash = HashPassword(newPass);
            using var update = conn.CreateCommand();
            update.CommandText = "UPDATE Users SET PasswordHash=$p WHERE Id=$id";
            update.Parameters.AddWithValue("$p", hash);
            update.Parameters.AddWithValue("$id", id);
            update.ExecuteNonQuery();
            return newPass;
        }

        private static string GenerateRandomPassword(int length)
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var sb = new System.Text.StringBuilder();
            using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
            var buf = new byte[4];
            for (int i = 0; i < length; i++)
            {
                rng.GetBytes(buf);
                var val = BitConverter.ToUInt32(buf, 0);
                sb.Append(chars[(int)(val % (uint)chars.Length)]);
            }
            return sb.ToString();
        }
    }
}
