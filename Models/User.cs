using System.Collections.Generic;

namespace RoleBasedFileAccess
{
    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        
        // Архитектура: информация о ролях и правах хранится здесь
        private static readonly Dictionary<string, List<string>> RolePermissions = 
            new()
            {
                ["Administrator"] = new List<string> { "finance.txt", "hr.txt", "marketing.txt" },
                ["User"] = new List<string> { "hr.txt", "marketing.txt" },
                ["Guest"] = new List<string> { "marketing.txt" }
            };
        
        public bool CanAccess(string fileName)
        {
            return RolePermissions.ContainsKey(Role) && 
                   RolePermissions[Role].Contains(fileName);
        }
    }
}