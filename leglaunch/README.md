# Программа запуска Матрицы для старых компьютеров (leglaunch)
Эта программа предназначена для запуска Матрицы на некоторых компьютерах с ОС Windows, на которых не запускается основной exe-файл Матрицы (например 32-разрядные компьютеры). Она запускает DLL-файл Матрицы с помощью программы dotnet.exe, являющейся частью дистрибутива .NET 6.

Программа запуска Матрицы для старых компьютеров устанавливается автоматически на 32-разрядных компьютерах, но может быть установлена и отдельно.

Программа собрана для .NET Framework 4.0. Подразумевается, что на большинстве компьютеров, на которых будет запускаться Матрица, уже будет установлена эта версия .NET.

## Параметры для отладки
Программу запуска Матрицы для старых компьютеров можно использовать с версиями Матрицы, собранными для отладки в Visual Studio (т.е. если dll-файл Матрицы находится в папке Debug или Release ее проекта, а не в той же папке, что leglaunch.exe). Для этого надо запустить leglaunch.exe с аргументом `--matrigen-location`:
```cmd
> leglaunch --matrigen-location debug
:: запускает Матрицу из папки Debug ее проекта
> leglaunch --matrigen-location release
:: запускает Матрицу из папки Release ее проекта
```
**Внимание:** ожидается, что структура проекта такая же, как в этом репозитории. Для улучшения безопасности возможности указать другую папку **нет**.
