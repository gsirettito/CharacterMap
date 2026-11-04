//<Run Text = "{Binding UnicodeValue, StringFormat=\&amp;#x\{0:X4\};, Mode=OneWay}" />

using System;
using System.Windows.Media;

namespace CharacterMap {
    internal class FontFamilyItem {
        public string Name { get; set; }
        public bool IsSymbol { get; set; }
        public FontFamily FontFamily { get; set; }
        public int GlyphCount { get; internal set; }
    }
}