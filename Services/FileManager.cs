using System;
using System.IO;

namespace RoleBasedFileAccess
{
    public static class FileManager
    {
        public static string ReadFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    return File.ReadAllText(fileName);
                }
                else
                {
                    return $"Файл '{fileName}' не найден на диске.";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка при чтении файла: {ex.Message}";
            }
        }
        
        public static void CreateRoleConfigurationFile()
        {
            try
            {
                if (!File.Exists("roles.json"))
                {
                    string roleConfig = @"{
  ""role_configuration"": {
    ""description"": ""Конфигурация ролей и прав доступа системы"",
    ""roles"": [
      {
        ""name"": ""Administrator"",
        ""permissions"": [""finance.txt"", ""hr.txt"", ""marketing.txt""],
        ""registration_rights"": ""Может регистрировать любых пользователей"",
        ""description"": ""Полный доступ ко всем функциям""
      },
      {
        ""name"": ""User"",
        ""permissions"": [""hr.txt"", ""marketing.txt""],
        ""registration_rights"": ""Может регистрировать только User/Guest"",
        ""description"": ""Стандартный пользователь""
      },
      {
        ""name"": ""Guest"",
        ""permissions"": [""marketing.txt""],
        ""registration_rights"": ""Не может регистрировать других"",
        ""description"": ""Гость с минимальными правами""
      }
    ]
  }
}";
                    
                    File.WriteAllText("roles.json", roleConfig);
                    Console.WriteLine("✓ Создан файл конфигурации ролей: roles.json");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Ошибка создания файла ролей: {ex.Message}");
            }
        }
    }
}