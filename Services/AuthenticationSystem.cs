using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace RoleBasedFileAccess
{
    public class AuthenticationSystem
    {
        private List<User> users = new();
        private const string UsersFile = "users.json";
        
        public AuthenticationSystem()
        {
            LoadUsers();
        }
        
        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
        
        public void RegisterUser(string username, string password, string role)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("Логин и пароль не могут быть пустыми!");
                return;
            }
            
            if (users.Exists(u => u.Username == username))
            {
                Console.WriteLine($"Пользователь '{username}' уже существует!");
                return;
            }
            
            var newUser = new User
            {
                Username = username,
                PasswordHash = ComputeHash(password),
                Role = role
            };
            
            users.Add(newUser);
            SaveUsers();
            
            Console.WriteLine($"✓ Пользователь '{username}' успешно зарегистрирован с ролью '{role}'");
        }
        
        public User? Authenticate(string username, string password)
        {
            var passwordHash = ComputeHash(password);
            return users.Find(u => u.Username == username && u.PasswordHash == passwordHash);
        }
        
        private void SaveUsers()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(users, options);
                File.WriteAllText(UsersFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения пользователей: {ex.Message}");
            }
        }
        
        private void LoadUsers()
        {
            try
            {
                if (File.Exists(UsersFile))
                {
                    var json = File.ReadAllText(UsersFile);
                    users = JsonSerializer.Deserialize<List<User>>(json) ?? new List<User>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки пользователей: {ex.Message}");
                users = new List<User>();
            }
        }
        
        public List<string> GetRegisteredUsers()
        {
            var result = new List<string>();
            foreach (var user in users)
            {
                result.Add($"{user.Username} ({user.Role})");
            }
            return result;
        }
    }
}