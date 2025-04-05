using Avalonia;
using Avalonia.Media.Fonts;
using System;

namespace matritsa.Desktop {
    public sealed class OfficeCodeProFontCollection : EmbeddedFontCollection {
        public OfficeCodeProFontCollection() : base(
            new Uri("fonts:OfficeCodePro", UriKind.Absolute),
            new Uri("avares://matritsa/Assets/Fonts", UriKind.Absolute)) {
        }
    }
}
