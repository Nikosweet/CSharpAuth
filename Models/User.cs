using System.Collections.Generic;

namespace RoleBasedFileAccess
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        
        // Архитектура: Информация о ролях и правах хранится в этом словаре
        // Для отчета преподавателю: здесь определены все роли и их права доступа
        private static readonly Dictionary<string, List<string>> RolePermissions = 
            new()
            {
                // Администратор: полный доступ ко всем файлам
                ["Administrator"] = new List<string> 
                { 
                    "finance.txt", 
                    "hr.txt", 
                    "marketing.txt" 
                },
                
                // Пользователь: доступ к HR и маркетингу
                ["User"] = new List<string> 
                { 
                    "hr.txt", 
                    "marketing.txt" 
                },
                
                // Гость: доступ только к маркетингу
                ["Guest"] = new List<string> 
                { 
                    "marketing.txt" 
                }
            };
        
        public bool CanAccess(string fileName)
        {
            return RolePermissions.ContainsKey(Role) && 
                   RolePermissions[Role].Contains(fileName);
        }
        
        public bool IsAdministrator()
        {
            return Role == "Administrator";
        }
        
        public static List<string> GetAvailableRoles()
        {
            return new List<string> { "Administrator", "User", "Guest" };
        }
        
        public static Dictionary<string, List<string>> GetRolePermissions()
        {
            return new Dictionary<string, List<string>>(RolePermissions);
        }
    }
}