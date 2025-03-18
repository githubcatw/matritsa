using Avalonia.Data.Converters;
using matritsa.Properties;
using System.IO;

namespace matritsa.Util {
    public static class Converters {
        public static readonly IValueConverter FileStringConverter =
           new FuncValueConverter<string?, string>(x => {
               if (string.IsNullOrEmpty(x)) {
                   return Resources.codeListNone;
               }
               return Path.GetFileName(x);
           });
    }
}