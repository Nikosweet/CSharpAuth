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
                    return $"Файл '{fileName}' не найден!";
                }
            }
            catch (Exception ex)
            {
                return $"Ошибка чтения файла '{fileName}': {ex.Message}";
            }
        }
        
        public static void ShowFileInfo()
        {
            string[] files = { "finance.txt", "hr.txt", "marketing.txt" };
            
            Console.WriteLine("\nИНФОРМАЦИЯ О ФАЙЛАХ:");
            foreach (var file in files)
            {
                bool exists = File.Exists(file);
                Console.WriteLine($"{file}: {(exists ? "существует" : "отсутствует")}");
            }
        }
    }
}