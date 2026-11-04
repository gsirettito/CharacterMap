using MS.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;

namespace SiretT.Fonts {

    public enum FontAwesomePack {
        Solid, Brands, Sharp, Regular, Fluent
    }

    internal class FontAwesomeIcon : TextBlock {
        private FontAwesomeSelector fonts;
        private Uri brandsUri;
        private Uri solidUri;
        private Uri sharpUri;
        private Uri fluentUri;
        private string brandsFamilyName;
        private string solidFamilyName;
        private string sharpFamilyName;
        private string fluentFamilyName;

        public FontAwesomeIcon() : base() {
            fonts = FontAwesomeSelector.Parse(SiretT.Fonts.Resources.css);
            base.TextAlignment = TextAlignment.Center;
            base.VerticalAlignment = VerticalAlignment.Center;
            base.HorizontalAlignment = HorizontalAlignment.Center;
            var assembly = Assembly.GetExecutingAssembly().GetName();
            var @namespace = assembly.Name;
            brandsUri = new Uri($"pack://application:,,,/{@namespace};component/SiretT/Fonts/fa-brands-400.ttf");
            solidUri = new Uri($"pack://application:,,,/{@namespace};component/SiretT/Fonts/fa-solid-900.ttf");
            sharpUri = new Uri($"pack://application:,,,/{@namespace};component/SiretT/Fonts/fa-sharp-solid-900.ttf");
            fluentUri = new Uri($"pack://application:,,,/{@namespace};component/SiretT/Fonts/Segoe Fluent Icons.ttf");
            brandsFamilyName = $"pack://application:,,,/{@namespace};component/SiretT/Fonts/#Font Awesome 6 Brands Regular";
            solidFamilyName = $"pack://application:,,,/{@namespace};component/SiretT/Fonts/#Font Awesome 6 Pro Solid";
            sharpFamilyName = $"pack://application:,,,/{@namespace};component/SiretT/Fonts/#Font Awesome 6 Sharp Solid";
            fluentFamilyName = $"pack://application:,,,/{@namespace};component/SiretT/Fonts/#Segoe Fluent Icons";
        }

        public static readonly DependencyProperty FontPackProperty = DependencyProperty.Register(
                    "FontPack", typeof(FontAwesomePack), typeof(FontAwesomeIcon), new PropertyMetadata(FontAwesomePack.Solid, OnPropertyChange));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            "Icon", typeof(string), typeof(FontAwesomeIcon), new PropertyMetadata(OnPropertyChange));

        private static void OnPropertyChange(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var self = d as FontAwesomeIcon;
            if (e.Property == IconProperty) {
                switch (self.FontPack) {
                    case FontAwesomePack.Solid: self.FontFamily = new System.Windows.Media.FontFamily(self.solidUri, self.solidFamilyName); break;
                    case FontAwesomePack.Sharp: self.FontFamily = new System.Windows.Media.FontFamily(self.sharpUri, self.sharpFamilyName); break;
                    case FontAwesomePack.Brands: self.FontFamily = new System.Windows.Media.FontFamily(self.brandsUri, self.brandsFamilyName); break;
                    case FontAwesomePack.Fluent: self.FontFamily = new System.Windows.Media.FontFamily(self.fluentUri, self.fluentFamilyName); break;
                }
                try {
                    if (self.fonts.Classes.ContainsKey(self.Icon.ToLower())) {
                        var ico = (char)self.fonts.Classes[self.Icon.ToLower()];
                        self.SetValue(TextBlock.TextProperty, ico.ToString());
                        //self.Content = ico.ToString();
                    } else self.SetValue(TextBlock.TextProperty, self.Icon);
                } catch { }
            } else if (e.Property == FontPackProperty) {
                switch ((FontAwesomePack)e.NewValue) {
                    case FontAwesomePack.Solid: self.FontFamily = new System.Windows.Media.FontFamily(self.solidUri, self.solidFamilyName); break;
                    case FontAwesomePack.Sharp: self.FontFamily = new System.Windows.Media.FontFamily(self.sharpUri, self.sharpFamilyName); break;
                    case FontAwesomePack.Brands: self.FontFamily = new System.Windows.Media.FontFamily(self.brandsUri, self.brandsFamilyName); break;
                    case FontAwesomePack.Fluent: self.FontFamily = new System.Windows.Media.FontFamily(self.fluentUri, self.fluentFamilyName); break;
                }
            }
        }

        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }

        [Category("Common Properties")]
        public string Icon {
            get { return GetValue(IconProperty).ToString(); }
            set { SetValue(IconProperty, value); }
        }

        public FontAwesomePack FontPack {
            get { return (FontAwesomePack)GetValue(FontPackProperty); }
            set { SetValue(FontPackProperty, value); }
        }
    }

    internal class FontAwesomeSelector {
        public Dictionary<string, int> Classes { get; private set; }

        public static FontAwesomeSelector Parse(string css) {
            FontAwesomeSelector selector = new FontAwesomeSelector();
            selector.Classes = new Dictionary<string, int>();
            var fa_content = Regex.Matches(css, @"\.fa.+\n.+content:.+\n\}");
            int l1 = "content: \"".Length;
            foreach (Match i in fa_content) {
                var content = Regex.Match(i.Value, @"content: "".+""");
                if (content.Success) {
                    var value = Convert.ToInt32(content.Value.Substring(l1, content.Length - l1 - 1).Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)[0], 16);
                    var l0 = i.Value.Split('\n')[0];
                    var classes = l0.Substring(0, l0.Length - 2).Trim().Split(',');
                    foreach (var c in classes) {
                        var name = c.Split(':')[0].Substring(1);
                        if (!selector.Classes.ContainsKey(name))
                            selector.Classes.Add(name, value);
                    }
                }

            }
            return selector;
        }
    }
}