using Microsoft.Data.Sqlite;
using System;

namespace curs.Data
{
    public static class Database
    {
        public static string DbPath = "app.db";

        public static void Initialize()
        {
            using var conn = new SqliteConnection($"Data Source={DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Users (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Username TEXT UNIQUE,
    PasswordHash TEXT,
    Role TEXT
);

CREATE TABLE IF NOT EXISTS Tests (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Title TEXT,
    Content TEXT
);
 
CREATE TABLE IF NOT EXISTS TestAttempts (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    TestId INTEGER,
    UserId INTEGER,
    Answers TEXT,
    CreatedAt TEXT
);

";
            cmd.ExecuteNonQuery();

            // Ensure Email and Name columns exist and Email is unique
            cmd.CommandText = "PRAGMA table_info(Users);";
            using var reader = cmd.ExecuteReader();
            var hasEmail = false;
            var hasName = false;
            while (reader.Read())
            {
                var col = reader.GetString(1);
                if (col == "Email") hasEmail = true;
                if (col == "Name") hasName = true;
            }
            reader.Close();

            if (!hasEmail)
            {
                cmd.CommandText = "ALTER TABLE Users ADD COLUMN Email TEXT;";
                cmd.ExecuteNonQuery();
            }
            if (!hasName)
            {
                cmd.CommandText = "ALTER TABLE Users ADD COLUMN Name TEXT;";
                cmd.ExecuteNonQuery();
            }

            // Ensure unique index on Email
            cmd.CommandText = "CREATE UNIQUE INDEX IF NOT EXISTS idx_users_email ON Users(Email);";
            try { cmd.ExecuteNonQuery(); } catch { /* ignore if cannot create */ }
        }

        public static void CreateDefaultAdmin()
        {
            using var conn = new SqliteConnection($"Data Source={DbPath}");
            conn.Open();
            using var cmd = conn.CreateCommand();
            // Ensure admin user exists and set its password to 'admin'
            cmd.CommandText = @"INSERT INTO Users (Username, PasswordHash, Role, Email, Name)
VALUES ($u, $p, $r, $e, $n)
ON CONFLICT(Username) DO UPDATE SET PasswordHash = $p, Role = $r, Email = $e, Name = $n;";
            cmd.Parameters.AddWithValue("$u", "admin");
            cmd.Parameters.AddWithValue("$p", Services.AuthService.HashPassword("admin"));
            cmd.Parameters.AddWithValue("$r", "admin");
            cmd.Parameters.AddWithValue("$e", "admin@local");
            cmd.Parameters.AddWithValue("$n", "Administrator");
            cmd.ExecuteNonQuery();
        }
    }
}
