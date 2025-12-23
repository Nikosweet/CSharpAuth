using System;

namespace RoleBasedFileAccess
{
    class Program
    {
        static AuthenticationSystem authSystem = new AuthenticationSystem();
        static User currentUser = null;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Создаем файлы бизнес-областей
            CreateBusinessFiles();
            
            // Предварительная регистрация тестовых пользователей
            RegisterDefaultUsers();
            
            // Главное меню
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("=== СИСТЕМА УПРАВЛЕНИЯ ДОСТУПОМ К ФАЙЛАМ ===");
                Console.WriteLine(currentUser != null 
                    ? $"Текущий пользователь: {currentUser.Username} ({currentUser.Role})" 
                    : "Не авторизован");
                Console.WriteLine("===========================================");
                
                if (currentUser == null)
                {
                    ShowLoginMenu();
                }
                else
                {
                    ShowMainMenu();
                }
                
                Console.Write("\nВыберите действие: ");
                var choice = Console.ReadLine();
                
                if (currentUser == null)
                {
                    ProcessLoginChoice(choice, ref exit);
                }
                else
                {
                    ProcessMainChoice(choice, ref exit);
                }
            }
        }

        static void ShowLoginMenu()
        {
            Console.WriteLine("\n1. Войти в систему");
            Console.WriteLine("2. Зарегистрироваться");
            Console.WriteLine("3. Выйти из программы");
            Console.WriteLine("4. Показать информацию о системе");
        }

        static void ShowMainMenu()
        {
            Console.WriteLine("\n1. Просмотреть доступные файлы");
            Console.WriteLine("2. Прочитать файл");
            Console.WriteLine("3. Зарегистрировать нового пользователя");
            Console.WriteLine("4. Выйти из учетной записи");
            Console.WriteLine("5. Выйти из программы");
            Console.WriteLine("6. Показать информацию о системе");
        }

        static void ProcessLoginChoice(string choice, ref bool exit)
        {
            switch (choice)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    Register();
                    break;
                case "3":
                    exit = true;
                    Console.WriteLine("Выход из программы...");
                    break;
                case "4":
                    ShowSystemInfo();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
                default:
                    Console.WriteLine("Неверный выбор!");
                    Console.ReadKey();
                    break;
            }
        }

        static void ProcessMainChoice(string choice, ref bool exit)
        {
            switch (choice)
            {
                case "1":
                    ShowAvailableFiles();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
                case "2":
                    ReadFile();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
                case "3":
                    Register();
                    break;
                case "4":
                    currentUser = null;
                    Console.WriteLine("Вы вышли из учетной записи.");
                    Console.ReadKey();
                    break;
                case "5":
                    exit = true;
                    Console.WriteLine("Выход из программы...");
                    break;
                case "6":
                    ShowSystemInfo();
                    Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                    Console.ReadKey();
                    break;
                default:
                    Console.WriteLine("Неверный выбор!");
                    Console.ReadKey();
                    break;
            }
        }

        static void Login()
        {
            Console.Clear();
            Console.WriteLine("=== ВХОД В СИСТЕМУ ===");
            
            Console.Write("Логин: ");
            string username = Console.ReadLine();
            
            Console.Write("Пароль: ");
            string password = Console.ReadLine();
            
            currentUser = authSystem.Authenticate(username, password);
            
            if (currentUser != null)
            {
                Console.WriteLine($"\n✓ Успешный вход! Добро пожаловать, {currentUser.Username}!");
                Console.WriteLine($"Роль: {currentUser.Role}");
            }
            else
            {
                Console.WriteLine("\n✗ Ошибка входа! Неверный логин или пароль.");
            }
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static void Register()
        {
            Console.Clear();
            Console.WriteLine("=== РЕГИСТРАЦИЯ НОВОГО ПОЛЬЗОВАТЕЛЯ ===");
            
            Console.Write("Введите логин: ");
            string username = Console.ReadLine();
            
            Console.Write("Введите пароль: ");
            string password = Console.ReadLine();
            
            Console.WriteLine("\nДоступные роли:");
            Console.WriteLine("1. Administrator (полный доступ)");
            Console.WriteLine("2. User (доступ к HR и Marketing)");
            Console.WriteLine("3. Guest (только Marketing)");
            
            Console.Write("Выберите роль (1-3): ");
            string roleChoice = Console.ReadLine();
            
            string role = roleChoice switch
            {
                "1" => "Administrator",
                "2" => "User",
                "3" => "Guest",
                _ => "Guest"
            };
            
            authSystem.RegisterUser(username, password, role);
            
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }

        static void ShowAvailableFiles()
        {
            Console.Clear();
            Console.WriteLine("=== ДОСТУПНЫЕ ФАЙЛЫ ===");
            
            if (currentUser == null)
            {
                Console.WriteLine("Не авторизован!");
                return;
            }
            
            string[] files = { "finance.txt", "hr.txt", "marketing.txt" };
            
            foreach (var file in files)
            {
                bool canAccess = currentUser.CanAccess(file);
                Console.WriteLine($"{file}: {(canAccess ? "✓ ДОСТУПЕН" : "✗ ЗАПРЕЩЕН")}");
            }
        }

        static void ReadFile()
        {
            Console.Clear();
            Console.WriteLine("=== ЧТЕНИЕ ФАЙЛА ===");
            
            if (currentUser == null)
            {
                Console.WriteLine("Не авторизован!");
                return;
            }
            
            Console.WriteLine("Доступные файлы:");
            string[] files = { "finance.txt", "hr.txt", "marketing.txt" };
            
            for (int i = 0; i < files.Length; i++)
            {
                bool canAccess = currentUser.CanAccess(files[i]);
                Console.WriteLine($"{i + 1}. {files[i]} {(canAccess ? "✓" : "✗")}");
            }
            
            Console.Write("\nВыберите файл (1-3): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= 3)
            {
                string fileName = files[choice - 1];
                
                if (currentUser.CanAccess(fileName))
                {
                    string content = FileManager.ReadFile(fileName);
                    Console.WriteLine($"\n=== СОДЕРЖИМОЕ {fileName.ToUpper()} ===");
                    Console.WriteLine(content);
                }
                else
                {
                    Console.WriteLine($"\n✗ Доступ к файлу '{fileName}' запрещен для вашей роли '{currentUser.Role}'!");
                }
            }
            else
            {
                Console.WriteLine("Неверный выбор!");
            }
        }

        static void CreateBusinessFiles()
        {
            // Создаем файлы бизнес-областей, если их нет
            var files = new System.Collections.Generic.Dictionary<string, string>
            {
                ["finance.txt"] = "ФИНАНСОВАЯ ОТЧЕТНОСТЬ 2024\n" +
                                 "========================\n" +
                                 "Доход: 10,000,000 руб.\n" +
                                 "Расход: 7,500,000 руб.\n" +
                                 "Прибыль: 2,500,000 руб.\n" +
                                 "\nКлючевые показатели:\n" +
                                 "- Рентабельность: 25%\n" +
                                 "- Бюджет на следующий год: 12,000,000 руб.",

                ["hr.txt"] = "КАДРОВЫЕ ДАННЫЕ\n" +
                            "===============\n" +
                            "Общее количество сотрудников: 150\n" +
                            "Открытые вакансии: 12\n" +
                            "Текучесть кадров: 8%\n" +
                            "\nОтделы:\n" +
                            "- IT: 25 чел.\n" +
                            "- Маркетинг: 18 чел.\n" +
                            "- Финансы: 12 чел.\n" +
                            "- HR: 8 чел.",

                ["marketing.txt"] = "МАРКЕТИНГОВЫЙ ПЛАН 2024\n" +
                                   "=====================\n" +
                                   "Общий бюджет: 2,000,000 руб.\n" +
                                   "Количество кампаний: 5\n" +
                                   "Ожидаемый охват: 1,000,000 чел.\n" +
                                   "\nПланируемые кампании:\n" +
                                   "1. Запуск нового продукта (Январь)\n" +
                                   "2. Летняя распродажа (Июнь)\n" +
                                   "3. Осенняя рекламная кампания (Сентябрь)"
            };
            
            foreach (var file in files)
            {
                if (!System.IO.File.Exists(file.Key))
                {
                    System.IO.File.WriteAllText(file.Key, file.Value);
                }
            }
        }

        static void RegisterDefaultUsers()
        {
            // Предварительная регистрация тестовых пользователей
            authSystem.RegisterUser("admin", "admin123", "Administrator");
            authSystem.RegisterUser("manager", "manager123", "User");
            authSystem.RegisterUser("guest", "guest123", "Guest");
        }

        static void ShowSystemInfo()
        {
            Console.Clear();
            Console.WriteLine("=== ИНФОРМАЦИЯ О СИСТЕМЕ ===");
            Console.WriteLine("\nАРХИТЕКТУРА СИСТЕМЫ:");
            Console.WriteLine("1. Роли и права хранятся в классе User");
            Console.WriteLine("   - Administrator: все файлы");
            Console.WriteLine("   - User: hr.txt, marketing.txt");
            Console.WriteLine("   - Guest: marketing.txt");
            
            Console.WriteLine("\n2. Объекты доступа (бизнес-файлы):");
            Console.WriteLine("   - finance.txt - финансовая информация");
            Console.WriteLine("   - hr.txt - кадровые данные");
            Console.WriteLine("   - marketing.txt - маркетинговые планы");
            
            Console.WriteLine("\n3. Регистрация пользователей:");
            Console.WriteLine("   - Хранятся в users.json (пароли хэшированы)");
            Console.WriteLine("   - Используется SHA256 для хэширования паролей");
            
            Console.WriteLine("\n4. Тестовые пользователи:");
            Console.WriteLine("   - Логин: admin, Пароль: admin123, Роль: Administrator");
            Console.WriteLine("   - Логин: manager, Пароль: manager123, Роль: User");
            Console.WriteLine("   - Логин: guest, Пароль: guest123, Роль: Guest");
        }
    }
}

// dotnet clean - очистка предыдущих сборок
// dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true - создание новой сборки