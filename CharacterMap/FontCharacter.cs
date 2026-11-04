//<Run Text = "{Binding UnicodeValue, StringFormat=\&amp;#x\{0:X4\};, Mode=OneWay}" />

using System.Windows.Media;

namespace CharacterMap {
    public class FontCharacter {
        public char Character { get; set; }
        public int UnicodeValue => (int)Character;
        public string UnicodeHex => $"U+{UnicodeValue:X4}";
        public string Description => GetCharacterDescription(Character);

        public string FontName { get; set; }
        public double DesignerWidth { get; internal set; }
        public double DesignerHeight { get; internal set; }
        public Geometry Geometry { get; internal set; }
        public string Data => Geometry.ToString();

        private string GetCharacterDescription(char c) {
            if (char.IsLetterOrDigit(c)) return "Letra o digito";
            if (char.IsPunctuation(c)) return "Signo de Puntuación";
            if (char.IsSymbol(c)) return "Símbolo";
            if (char.IsWhiteSpace(c)) return "Espacio en blanco";
            if (char.IsControl(c)) return "Carácter de control";
            return "Otro";
        }
    }
}
