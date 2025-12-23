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
            
            // Если нет пользователей, создаем первого администратора
            if (users.Count == 0)
            {
                CreateFirstAdministrator();
            }
        }
        
        private void CreateFirstAdministrator()
        {
            string defaultAdmin = "admin";
            string defaultPassword = "admin123";
            
            var adminUser = new User
            {
                Username = defaultAdmin,
                PasswordHash = ComputeHash(defaultPassword),
                Role = "Administrator"
            };
            
            users.Add(adminUser);
            SaveUsers();
            
            Console.WriteLine("\n╔════════════════════════════════════════════════╗");
            Console.WriteLine("║   ПЕРВЫЙ ЗАПУСК СИСТЕМЫ                      ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            Console.WriteLine("Создан первый администратор по умолчанию:");
            Console.WriteLine($"  Логин: {defaultAdmin}");
            Console.WriteLine($"  Пароль: {defaultPassword}");
            Console.WriteLine("\n⚠  Запомните эти данные для первого входа!");
            Console.WriteLine("   Вы сможете изменить пароль после входа.\n");
            
            WaitForAnyKey();
        }
        
        private static string ComputeHash(string input)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToBase64String(bytes);
        }
        
        public void RegisterUser(string username, string password, string role, string registeredBy)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("\n✗ Логин и пароль не могут быть пустыми!");
                return;
            }
            
            if (users.Exists(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                Console.WriteLine($"\n✗ Пользователь '{username}' уже существует!");
                return;
            }
            
            // Проверяем, можно ли регистрировать эту роль
            if (role == "Administrator" && registeredBy != "admin" && !IsAdministrator(registeredBy))
            {
                Console.WriteLine("\n✗ Ошибка: только администраторы могут создавать других администраторов!");
                Console.WriteLine("  Зарегистрируйтесь как User или Guest, либо войдите как администратор.");
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
            
            Console.WriteLine($"\n✓ Пользователь успешно зарегистрирован!");
            Console.WriteLine($"  Логин: {username}");
            Console.WriteLine($"  Роль: {role}");
            
            if (registeredBy != "self-registration")
            {
                Console.WriteLine($"  Зарегистрирован администратором: {registeredBy}");
            }
            else
            {
                Console.WriteLine($"  Зарегистрирован самостоятельно");
            }
            
            if (role == "Administrator")
            {
                Console.WriteLine("\n⚠  ВНИМАНИЕ: Зарегистрирован новый администратор!");
                Console.WriteLine("   Теперь он может регистрировать других пользователей.");
            }
        }
        
        private bool IsAdministrator(string username)
        {
            var user = users.Find(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
            return user != null && user.Role == "Administrator";
        }
        
        public User? Authenticate(string username, string password)
        {
            var passwordHash = ComputeHash(password);
            return users.Find(u => 
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) && 
                u.PasswordHash == passwordHash);
        }
        
        private void SaveUsers()
        {
            try
            {
                var options = new JsonSerializerOptions 
                { 
                    WriteIndented = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                };
                
                var usersData = new
                {
                    system_info = "Role-Based File Access System - Users Database",
                    generated_at = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    total_users = users.Count,
                    administrators = users.Count(u => u.Role == "Administrator"),
                    regular_users = users.Count(u => u.Role == "User"),
                    guests = users.Count(u => u.Role == "Guest"),
                    users_list = users
                };
                
                var json = JsonSerializer.Serialize(usersData, options);
                File.WriteAllText(UsersFile, json);
                
                // Для отчета: показываем содержимое файла
                Console.WriteLine("\n📄 Файл users.json обновлен:");
                Console.WriteLine(new string('─', 50));
                Console.WriteLine(json);
                Console.WriteLine(new string('─', 50));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Ошибка сохранения: {ex.Message}");
            }
        }
        
        private void LoadUsers()
        {
            try
            {
                if (File.Exists(UsersFile))
                {
                    var json = File.ReadAllText(UsersFile);
                    var data = JsonSerializer.Deserialize<JsonElement>(json);
                    
                    if (data.TryGetProperty("users_list", out var usersList))
                    {
                        users = JsonSerializer.Deserialize<List<User>>(usersList.ToString()) ?? new List<User>();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Ошибка загрузки: {ex.Message}");
                users = new List<User>();
            }
        }
        
        public List<string> GetRegisteredUsers()
        {
            var result = new List<string>();
            
            foreach (var user in users)
            {
                string roleIcon = user.Role == "Administrator" ? "👑" : 
                                 user.Role == "User" ? "👤" : "👣";
                result.Add($"{roleIcon} {user.Username,-15} ({user.Role})");
            }
            
            return result;
        }
        
        public int GetUserCount()
        {
            return users.Count;
        }
        
        public int GetAdministratorCount()
        {
            return users.Count(u => u.Role == "Administrator");
        }
        
        private void WaitForAnyKey()
        {
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}