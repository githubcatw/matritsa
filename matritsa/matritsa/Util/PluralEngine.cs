using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace matritsa.Util {
    public class PluralEngine {
        public static string GetPluralForm(float number, PluralSet set) {
            if (number == 1) {
                return set.One;
            }
            // правила для славянских языков
            else if (set.PluralForm == "Slavic") {
                // на русском числительные 2-5 требуют отдельной формы 
                if (number >= 2 && number < 5) {
                    return set.Few ?? set.Other;
                }
                // оставляем форму 1 для числительных, которые заканчиваются на 1, кроме 11
                else if (number.ToString().EndsWith("1") && number != 11) {
                    return set.One;
                }
                else {
                    return set.Other;
                }
            }
            else {
                return set.Other;
            }
        }
    }
}
