// для сборки в терминал:
// dotnet clean
// dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
// exe-файл хранится в: release/net8.0/win-x64/publish
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
            
            // Создаем файл конфигурации ролей
            FileManager.CreateRoleConfigurationFile();
            
            // Главное меню
            bool exit = false;
            while (!exit)
            {
                Console.Clear();
                ShowHeader();
                
                if (currentUser == null)
                {
                    ShowUnauthorizedMenu();
                }
                else
                {
                    ShowAuthorizedMenu();
                }
                
                Console.Write("\nВыберите действие: ");
                var choice = Console.ReadLine();
                
                if (currentUser == null)
                {
                    ProcessUnauthorizedChoice(choice, ref exit);
                }
                else
                {
                    ProcessAuthorizedChoice(choice, ref exit);
                }
            }
        }

        static void ShowHeader()
        {
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║   СИСТЕМА УПРАВЛЕНИЯ ДОСТУПОМ К ФАЙЛАМ       ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            Console.WriteLine($"Текущий пользователь: {(currentUser != null ? $"{currentUser.Username} ({currentUser.Role})" : "Не авторизован")}");
            Console.WriteLine(new string('─', 50));
        }

        static void ShowUnauthorizedMenu()
        {
            Console.WriteLine("\nГЛАВНОЕ МЕНЮ:");
            Console.WriteLine("1. Войти в систему");
            Console.WriteLine("2. Зарегистрироваться (стать пользователем)");
            Console.WriteLine("3. Показать информацию о системе");
            Console.WriteLine("4. Выйти из программы");
            Console.WriteLine("\n📝 Регистрация доступна для всех (роли: User или Guest)");
        }

        static void ShowAuthorizedMenu()
        {
            Console.WriteLine("\nОСНОВНОЕ МЕНЮ:");
            Console.WriteLine("1. Просмотреть доступные файлы");
            Console.WriteLine("2. Прочитать файл");
            Console.WriteLine("3. Показать информацию о системе");
            
            if (currentUser.Role == "Administrator")
            {
                Console.WriteLine("4. Зарегистрировать нового пользователя");
                Console.WriteLine("5. Просмотреть список пользователей");
            }
            
            Console.WriteLine("9. Выйти из учетной записи");
            Console.WriteLine("0. Выйти из программы");
            
            if (currentUser.Role != "Administrator")
            {
                Console.WriteLine("\n⚠  Регистрация новых пользователей доступна только администраторам");
            }
        }

        static void ProcessUnauthorizedChoice(string choice, ref bool exit)
        {
            switch (choice)
            {
                case "1":
                    Login();
                    break;
                case "2":
                    RegisterNewUser();
                    break;
                case "3":
                    ShowSystemInfo();
                    WaitForKey();
                    break;
                case "4":
                    exit = true;
                    Console.WriteLine("\nЗавершение работы программы...");
                    WaitForKey();
                    break;
                default:
                    Console.WriteLine("\nНеверный выбор!");
                    WaitForKey();
                    break;
            }
        }

        static void ProcessAuthorizedChoice(string choice, ref bool exit)
        {
            switch (choice)
            {
                case "1":
                    ShowAvailableFiles();
                    WaitForKey();
                    break;
                case "2":
                    ReadFile();
                    WaitForKey();
                    break;
                case "3":
                    ShowSystemInfo();
                    WaitForKey();
                    break;
                case "4" when currentUser.Role == "Administrator":
                    RegisterNewUserAsAdmin();
                    break;
                case "5" when currentUser.Role == "Administrator":
                    ShowUserList();
                    WaitForKey();
                    break;
                case "9":
                    currentUser = null;
                    Console.WriteLine("\n✓ Вы вышли из учетной записи.");
                    WaitForKey();
                    break;
                case "0":
                    exit = true;
                    Console.WriteLine("\nЗавершение работы программы...");
                    WaitForKey();
                    break;
                default:
                    Console.WriteLine("\nНеверный выбор!");
                    WaitForKey();
                    break;
            }
        }

        static void Login()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║                   ВХОД В СИСТЕМУ              ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            
            Console.Write("Логин: ");
            string username = Console.ReadLine();
            
            Console.Write("Пароль: ");
            string password = GetHiddenPassword();
            
            currentUser = authSystem.Authenticate(username, password);
            
            if (currentUser != null)
            {
                Console.WriteLine($"\n✓ Успешный вход! Добро пожаловать, {currentUser.Username}!");
                Console.WriteLine($"  Роль: {currentUser.Role}");
                
                if (currentUser.Role == "Administrator")
                {
                    Console.WriteLine($"\n⭐ Вы администратор! Вы можете регистрировать новых пользователей.");
                }
            }
            else
            {
                Console.WriteLine("\n✗ Ошибка входа! Неверный логин или пароль.");
            }
            
            WaitForKey();
        }

        static void RegisterNewUser()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║          РЕГИСТРАЦИЯ ПОЛЬЗОВАТЕЛЯ            ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            
            Console.WriteLine("Регистрация новой учетной записи пользователя\n");
            
            Console.Write("Введите логин: ");
            string username = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("\n✗ Логин не может быть пустым!");
                WaitForKey();
                return;
            }
            
            Console.Write("Введите пароль: ");
            string password = GetHiddenPassword();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("\n✗ Пароль не может быть пустым!");
                WaitForKey();
                return;
            }
            
            Console.WriteLine("\n╔════════════════════════════════════════════════╗");
            Console.WriteLine("║              ВЫБОР РОЛИ                       ║");
            Console.WriteLine("╠════════════════════════════════════════════════╣");
            Console.WriteLine("║ 1. User (пользователь)                        ║");
            Console.WriteLine("║    • Доступ к hr.txt и marketing.txt          ║");
            Console.WriteLine("║    • Может читать файлы по правам доступа     ║");
            Console.WriteLine("║                                                ║");
            Console.WriteLine("║ 2. Guest (гость)                              ║");
            Console.WriteLine("║    • Доступ только к marketing.txt            ║");
            Console.WriteLine("║    • Минимальные права доступа                ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            
            Console.WriteLine("\n⚠  Администраторов может создавать только администратор!");
            
            Console.Write("\nВыберите роль (1-2): ");
            string roleChoice = Console.ReadLine();
            
            string role = roleChoice switch
            {
                "1" => "User",
                "2" => "Guest",
                _ => "User"
            };
            
            // Регистрируем как обычный пользователь (не администратор)
            authSystem.RegisterUser(username, password, role, "self-registration");
            
            Console.WriteLine($"\n✓ Вы успешно зарегистрированы как {role}!");
            Console.WriteLine("  Теперь вы можете войти в систему со своими учетными данными.");
            
            WaitForKey();
        }

        static void RegisterNewUserAsAdmin()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║    РЕГИСТРАЦИЯ ПОЛЬЗОВАТЕЛЯ (АДМИНИСТРАТОР)  ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            
            Console.WriteLine($"Администратор: {currentUser.Username}\n");
            
            Console.Write("Введите логин нового пользователя: ");
            string username = Console.ReadLine();
            
            if (string.IsNullOrWhiteSpace(username))
            {
                Console.WriteLine("\n✗ Логин не может быть пустым!");
                WaitForKey();
                return;
            }
            
            Console.Write("Введите пароль: ");
            string password = GetHiddenPassword();
            
            if (string.IsNullOrWhiteSpace(password))
            {
                Console.WriteLine("\n✗ Пароль не может быть пустым!");
                WaitForKey();
                return;
            }
            
            Console.WriteLine("\n╔════════════════════════════════════════════════╗");
            Console.WriteLine("║              ВЫБОР РОЛИ                       ║");
            Console.WriteLine("╠════════════════════════════════════════════════╣");
            Console.WriteLine("║ 1. Administrator (администратор)              ║");
            Console.WriteLine("║    • Полный доступ ко всем файлам             ║");
            Console.WriteLine("║    • Может регистрировать новых пользователей ║");
            Console.WriteLine("║                                                ║");
            Console.WriteLine("║ 2. User (пользователь)                        ║");
            Console.WriteLine("║    • Доступ к hr.txt и marketing.txt          ║");
            Console.WriteLine("║    • Не может регистрировать пользователей    ║");
            Console.WriteLine("║                                                ║");
            Console.WriteLine("║ 3. Guest (гость)                              ║");
            Console.WriteLine("║    • Доступ только к marketing.txt            ║");
            Console.WriteLine("║    • Не может регистрировать пользователей    ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝");
            
            Console.Write("\nВыберите роль (1-3): ");
            string roleChoice = Console.ReadLine();
            
            string role = roleChoice switch
            {
                "1" => "Administrator",
                "2" => "User",
                "3" => "Guest",
                _ => "User"
            };
            
            if (role == "Administrator")
            {
                Console.Write("\n⚠  ВНИМАНИЕ: Вы регистрируете нового администратора! ");
                Console.Write("Подтвердите (y/n): ");
                if (Console.ReadLine()?.ToLower() != "y")
                {
                    Console.WriteLine("\nРегистрация отменена.");
                    WaitForKey();
                    return;
                }
            }
            
            authSystem.RegisterUser(username, password, role, currentUser.Username);
            
            WaitForKey();
        }

        static void ShowAvailableFiles()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║            ДОСТУПНЫЕ ФАЙЛЫ                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            
            Console.WriteLine($"Пользователь: {currentUser.Username}");
            Console.WriteLine($"Роль: {currentUser.Role}\n");
            
            string[] files = { "finance.txt", "hr.txt", "marketing.txt" };
            
            foreach (var file in files)
            {
                bool canAccess = currentUser.CanAccess(file);
                Console.Write($"{file,-15} ");
                Console.ForegroundColor = canAccess ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(canAccess ? "✓ ДОСТУПЕН" : "✗ ЗАПРЕЩЕН");
                Console.ResetColor();
            }
        }

        static void ReadFile()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║                ЧТЕНИЕ ФАЙЛА                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            
            Console.WriteLine($"Текущий пользователь: {currentUser.Username} ({currentUser.Role})\n");
            
            string[] files = { "finance.txt", "hr.txt", "marketing.txt" };
            
            for (int i = 0; i < files.Length; i++)
            {
                bool canAccess = currentUser.CanAccess(files[i]);
                Console.Write($"{i + 1}. {files[i],-15} ");
                Console.ForegroundColor = canAccess ? ConsoleColor.Green : ConsoleColor.Red;
                Console.WriteLine(canAccess ? "✓" : "✗");
                Console.ResetColor();
            }
            
            Console.Write("\nВыберите файл (1-3) или 0 для отмены: ");
            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice == 0) return;
                
                if (choice >= 1 && choice <= 3)
                {
                    string fileName = files[choice - 1];
                    
                    if (currentUser.CanAccess(fileName))
                    {
                        string content = FileManager.ReadFile(fileName);
                        Console.WriteLine($"\n╔{'═', 48}╗");
                        Console.WriteLine($"║ ФАЙЛ: {fileName.ToUpper(),-39} ║");
                        Console.WriteLine($"╚{'═', 48}╝\n");
                        Console.WriteLine(content);
                    }
                    else
                    {
                        Console.WriteLine($"\n✗ Доступ запрещен!");
                        Console.WriteLine($"  Роль '{currentUser.Role}' не имеет прав на чтение файла '{fileName}'");
                    }
                }
                else
                {
                    Console.WriteLine("\nНеверный выбор!");
                }
            }
        }

        static void ShowUserList()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║        СПИСОК ПОЛЬЗОВАТЕЛЕЙ                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            
            var users = authSystem.GetRegisteredUsers();
            
            if (users.Count == 0)
            {
                Console.WriteLine("Пользователи не найдены.");
                return;
            }
            
            Console.WriteLine($"Всего пользователей: {users.Count}\n");
            Console.WriteLine(new string('─', 40));
            
            foreach (var user in users)
            {
                Console.WriteLine($"  • {user}");
            }
            
            Console.WriteLine(new string('─', 40));
            Console.WriteLine($"\nФайл с пользователями: users.json");
        }

        static void ShowSystemInfo()
        {
            Console.Clear();
            Console.WriteLine("╔════════════════════════════════════════════════╗");
            Console.WriteLine("║        ИНФОРМАЦИЯ О СИСТЕМЕ                  ║");
            Console.WriteLine("╚════════════════════════════════════════════════╝\n");
            
            // 1. Архитектура: где хранится информация о ролях и правах
            Console.WriteLine("1. АРХИТЕКТУРА СИСТЕМЫ:");
            Console.WriteLine("   └─ Роли и права хранятся в классе User.cs");
            Console.WriteLine("      ├─ Administrator: finance.txt, hr.txt, marketing.txt");
            Console.WriteLine("      ├─ User: hr.txt, marketing.txt");
            Console.WriteLine("      └─ Guest: marketing.txt\n");
            
            // 2. Объекты доступа
            Console.WriteLine("2. ОБЪЕКТЫ ДОСТУПА (бизнес-файлы):");
            Console.WriteLine("   ├─ finance.txt - Финансовая отчетность");
            Console.WriteLine("   ├─ hr.txt - Кадровые данные");
            Console.WriteLine("   └─ marketing.txt - Маркетинговые планы\n");
            
            // 3. Регистрация пользователей
            Console.WriteLine("3. СИСТЕМА РЕГИСТРАЦИИ:");
            Console.WriteLine("   ├─ Любой пользователь может зарегистрироваться");
            Console.WriteLine("   │  └─ Доступные роли: User или Guest");
            Console.WriteLine("   └─ Только администраторы могут регистрировать:");
            Console.WriteLine("      ├─ Новых User и Guest");
            Console.WriteLine("      └─ Новых Administrator (других администраторов)\n");
            
            // 4. Текущая сессия
            Console.WriteLine("4. ТЕКУЩАЯ СЕССИЯ:");
            Console.WriteLine($"   ├─ Пользователь: {currentUser?.Username ?? "Не авторизован"}");
            Console.WriteLine($"   ├─ Роль: {currentUser?.Role ?? "Нет"}");
            Console.WriteLine($"   ├─ Может регистрировать: {(currentUser?.Role == "Administrator" ? "Да" : "Нет")}");
            Console.WriteLine($"   └─ Всего файлов: 3\n");
            
            // 5. Файлы конфигурации
            Console.WriteLine("5. ФАЙЛЫ КОНФИГУРАЦИИ:");
            Console.WriteLine("   ├─ roles.json - права доступа ролей");
            Console.WriteLine("   └─ users.json - зарегистрированные пользователи");
        }

        static void CreateBusinessFiles()
        {
            var files = new System.Collections.Generic.Dictionary<string, string>
            {
                ["finance.txt"] = "ФИНАНСОВАЯ ОТЧЕТНОСТЬ 2024\n" +
                                 new string('=', 40) + "\n" +
                                 "ОБЩИЕ ПОКАЗАТЕЛИ:\n" +
                                 "• Общий доход: 10,000,000 руб.\n" +
                                 "• Операционные расходы: 7,500,000 руб.\n" +
                                 "• Чистая прибыль: 2,500,000 руб.\n" +
                                 "• Рентабельность: 25%\n\n" +
                                 "КВАРТАЛЬНАЯ ДЕТАЛИЗАЦИЯ:\n" +
                                 "Q1: Доход 2,200,000, Прибыль 550,000\n" +
                                 "Q2: Доход 2,800,000, Прибыль 700,000\n" +
                                 "Q3: Доход 2,500,000, Прибыль 625,000\n" +
                                 "Q4: Доход 2,500,000, Прибыль 625,000",

                ["hr.txt"] = "КАДРОВЫЕ ДАННЫЕ И ОТЧЕТ\n" +
                            new string('=', 40) + "\n" +
                            "СТАТИСТИКА ПО СОТРУДНИКАМ:\n" +
                            "• Общая численность: 150 человек\n" +
                            "• Средний возраст: 34 года\n" +
                            "• Текучесть кадров: 8% в год\n" +
                            "• Открытые вакансии: 12 позиций\n\n" +
                            "РАСПРЕДЕЛЕНИЕ ПО ОТДЕЛАМ:\n" +
                            "• IT и разработка: 32 чел. (21%)\n" +
                            "• Маркетинг: 18 чел. (12%)\n" +
                            "• Финансы: 15 чел. (10%)\n" +
                            "• Продажи: 45 чел. (30%)\n" +
                            "• Поддержка: 22 чел. (15%)\n" +
                            "• HR и администрирование: 18 чел. (12%)",

                ["marketing.txt"] = "МАРКЕТИНГОВЫЙ ПЛАН И СТРАТЕГИЯ 2024\n" +
                                   new string('=', 40) + "\n" +
                                   "ОБЩИЙ БЮДЖЕТ: 2,000,000 руб.\n\n" +
                                   "РАСПРЕДЕЛЕНИЕ БЮДЖЕТА:\n" +
                                   "1. Цифровая реклама: 800,000 руб. (40%)\n" +
                                   "   • Google Ads: 400,000 руб.\n" +
                                   "   • Социальные сети: 300,000 руб.\n" +
                                   "   • Email-рассылки: 100,000 руб.\n\n" +
                                   "2. Офлайн-мероприятия: 400,000 руб. (20%)\n" +
                                   "   • Конференции: 200,000 руб.\n" +
                                   "   • Выставки: 150,000 руб.\n" +
                                   "   • Тренинги: 50,000 руб.\n\n" +
                                   "3. Контент-маркетинг: 300,000 руб. (15%)\n" +
                                   "   • Блог и статьи: 150,000 руб.\n" +
                                   "   • Видео-контент: 100,000 руб.\n" +
                                   "   • Инфографика: 50,000 руб.\n\n" +
                                   "4. Партнерские программы: 500,000 руб. (25%)"
            };
            
            foreach (var file in files)
            {
                if (!System.IO.File.Exists(file.Key))
                {
                    System.IO.File.WriteAllText(file.Key, file.Value);
                }
            }
        }

        static string GetHiddenPassword()
        {
            string password = "";
            ConsoleKeyInfo key;
            
            do
            {
                key = Console.ReadKey(true);
                
                if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password.Substring(0, (password.Length - 1));
                    Console.Write("\b \b");
                }
            }
            while (key.Key != ConsoleKey.Enter);
            
            Console.WriteLine();
            return password;
        }

        static void WaitForKey()
        {
            Console.WriteLine("\nНажмите любую клавишу для продолжения...");
            Console.ReadKey();
        }
    }
}