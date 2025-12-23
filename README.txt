Для запуска программы используйте в терминале:
dotnet clean - очистка предыдущих сборок
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
exe-файл хранится в: release/net8.0/win-x64/publish