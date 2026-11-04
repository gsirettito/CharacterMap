using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SiretT.Fonts {

    internal class FontIcon : TextBlock {
        public static readonly DependencyProperty GlypProperty = DependencyProperty.Register(
            "Glyph", typeof(string), typeof(FontIcon), new PropertyMetadata(OnPropertyChange));

        public FontIcon() : base() { }

        public FontIcon(string glyph) : base(new Run(glyph)) {
            Glyph = glyph;
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }

        private static void OnPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var self = d as FontIcon;
            if (e.Property == GlypProperty) {
                self.SetValue(TextBlock.TextProperty, e.NewValue);
            }
        }

        [Category("Common Properties")]
        [Localizability(LocalizationCategory.Text)]
        public string Glyph {
            get {
                return GetValue(GlypProperty).ToString();
            }
            set { SetValue(GlypProperty, value); }
        }
    }
}