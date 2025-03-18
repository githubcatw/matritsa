using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Matritsa.LegacyLauncher {

    internal interface IStrings {
        string Error { get; }
        string UnknownDebugMatrigen { get; }
        string NoDotnet { get; }
        string NoMatrigen { get; }
        string UnknownError { get; }
    }

    internal class EnglishStrings: IStrings {
        public string Error => "Error";
        public string UnknownDebugMatrigen => "Unknown matrigen location: should be either debug or release";
        public string NoDotnet => ".NET 6 is required to start Matritsa. Please install .NET 6.";
        public string NoMatrigen => "Matritsa not found. Please reinstall Matritsa.";
        public string UnknownError => "Unknown error:\n\n{0}\n\nPlease reinstall Matritsa.";
    }

    internal class RussianStrings: IStrings {
        public string Error => "Ошибка";
        public string UnknownDebugMatrigen => "Неизвестное место matrigen: разрешено либо debug, либо release";
        public string NoDotnet => "Для запуска Матрицы требуется .NET 6. Пожалуйста, установите .NET 6.";
        public string NoMatrigen => "Матрица не найдена. Пожалуйста, переустановите Матрицу.";
        public string UnknownError => "Неизвестная ошибка:\n\n{0}\n\nПожалуйста, переустановите Матрицу.";
    }
}
