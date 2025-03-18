using System;

namespace matritsa.Util {
    public class PluralSet(string one, string other, string? few, string form) {
        /// <summary>
        /// для 1 (и для цифр, кончающихся на 1, кроме 11, на русском)
        /// </summary>
        public string One = one ?? throw new ArgumentNullException(nameof(one));
        /// <summary>
        /// for numbers not covered by the above cases
        /// </summary>
        public string Other = other ?? throw new ArgumentNullException(nameof(other));
        /// <summary>
        /// для маленьких цифр (напр. 2, 3, 4 на русском)
        /// </summary>
        public string? Few = few;
        /// <summary>
        /// конфигурация множественной формы, либо <see cref="PluralFormGeneric"/>, либо <see cref="PluralFormSlavic"/>
        /// </summary>
        public string PluralForm = form ?? throw new ArgumentNullException(nameof(form));

        /// <summary>
        /// 1 - one
        /// <br></br>
        /// все остальные цифры - other
        /// </summary>
        public const string PluralFormGeneric = "Generic";
        /// <summary>
        /// 1 - one
        /// <br></br>
        /// 2, 3, 4 - few
        /// <br></br>
        /// 11 - other
        /// <br></br>
        /// все остальные цифры, кончающиеся на 1 - one
        /// <br></br>
        /// все остальные - other
        /// </summary>
        public const string PluralFormSlavic = "Slavic";
    }
}
