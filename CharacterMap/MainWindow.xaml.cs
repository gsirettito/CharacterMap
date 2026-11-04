//<Run Text = "{Binding UnicodeValue, StringFormat=\&amp;#x\{0:X4\};, Mode=OneWay}" />

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using VirtualCollection.VirtualCollection;

namespace CharacterMap {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static readonly HashSet<string> KnownSymbolFonts = new HashSet<string> {
            "Wingdings",
            "Wingdings 2",
            "Wingdings 3",
            "Webdings",
            "Symbol",
            "Segoe MDL2 Assets",
            "Segoe UI Symbol",
            "Segoe Fluent Icons",
            "Bookshelf Symbol 7",
            "MS Reference Specialty",
            "FontAwesome",
            "Material Icons",
        };
        private ItemListViewModel<FontFamilyItem> fontListViewModelItems;
        private ItemListViewModel<FontCharacter> charListViewModelItems;

        public MainWindow() {
            InitializeComponent();

            fontListViewModelItems = new ItemListViewModel<FontFamilyItem>();
            fontListViewModelItems.PropertyChanged += fontListViewItems_PropertyChanged;

            charListViewModelItems = new ItemListViewModel<FontCharacter>();
            charListViewModelItems.PropertyChanged += charListViewItems_PropertyChanged;
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            var fonts = Fonts.SystemFontFamilies
                .OrderBy(f => f.Source)
                .Select(f => new FontFamilyItem {
                    Name = f.Source,
                    IsSymbol = IsSymbolFont(f),
                    FontFamily = f,
                })
                .ToList();

            fontListViewModelItems.SetItems(fonts);
            if (fontListBox.ItemsSource != null)
                CollectionViewSource.GetDefaultView(fontListBox.ItemsSource).Filter = fontNameFilter;

            fontListViewModelItems.SelectIndex(0);
        }

        private void fontListViewItems_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            fontListBox.DataContext = null;
            fontListBox.DataContext = fontListViewModelItems;
            if (fontListBox.ItemsSource != null)
                CollectionViewSource.GetDefaultView(fontListBox.ItemsSource).Filter = fontNameFilter;
        }

        private void charListViewItems_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            charListView.DataContext = null;
            charListView.DataContext = charListViewModelItems;
            if (charListView.ItemsSource != null)
                CollectionViewSource.GetDefaultView(charListView.ItemsSource).Filter = charNameFilter;
        }

        private bool IsSymbolFont(FontFamily fontFamily) {

            if (IsKnownSymbolFont(fontFamily.Source))
                return true;

            Typeface typeface = new Typeface(fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

            string testChars = " Aa0";

            foreach (char c in testChars) {
                if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                    return true;

                if (glyphTypeface.CharacterToGlyphMap.ContainsKey(c) && glyphTypeface.Symbol && glyphTypeface.SampleTexts.Count == 0)
                    return true;

                if (!glyphTypeface.CharacterToGlyphMap.ContainsKey(c))
                    return true;
            }
            return false;
        }

        private bool IsKnownSymbolFont(string fontName) {
            return KnownSymbolFonts.Contains(fontName);
        }

        private void fontListBox_Selected(object sender, SelectionChangedEventArgs e) {
            if (fontListBox.SelectedItem != null) {
                var lastSelectedIndex = charListView.SelectedIndex == -1 ? 1 : charListView.SelectedIndex;
                dynamic font = fontListBox.SelectedItem;
                typefacesCombo.ItemsSource = (font?.FontFamily as FontFamily).FamilyTypefaces;
                typefacesCombo.SelectedIndex = 0;
                charListViewModelItems.SetItems(GetCharactersForFont(font?.FontFamily as FontFamily, typefacesCombo.SelectedValue as FamilyTypeface));
                ScrollToCharItemVirtualized(charListViewModelItems.Items.ElementAt(lastSelectedIndex));
                charListViewModelItems.SelectIndex(lastSelectedIndex);
                charListView.SelectedIndex = lastSelectedIndex;
                ListViewItem lvi = FindAllChild<ListViewItem>(charListView).Where(i => i.DataContext == charListView.SelectedItem).FirstOrDefault();
                lvi?.BringIntoView();
                charListView.ScrollIntoView(charListView.SelectedItem);
                charListView.Focus();
                if (charListView.ItemContainerGenerator.ContainerFromIndex(0) is ListViewItem listViewItem) {
                    listViewItem.IsSelected = true;
                }
            }
        }

        private static T[] FindAllChild<T>(DependencyObject reference) where T : class {
            // Do a breadth first search.
            var queue = new Queue<DependencyObject>();
            List<T> result = new List<T>();
            queue.Enqueue(reference);
            while (queue.Count > 0) {
                DependencyObject child = queue.Dequeue();
                T obj = child as T;
                if (obj != null) {
                    result.Add(obj);
                }

                // Add the children to the queue to search through later.
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(child); i++) {
                    queue.Enqueue(VisualTreeHelper.GetChild(child, i));
                }
            }
            return result.ToArray(); // Not found.
        }

        private IEnumerable<string> GetTypefacesForFont(FontFamily fontFamily) {
            List<string> typefaces = new List<string>();
            foreach (var i in fontFamily.FamilyTypefaces) {
                var typeface = i.AdjustedFaceNames?.First().Value;
                if (typeface != null) {
                    typefaces.Add(typeface);
                    continue;
                }

                typeface = $"{(i.Stretch == FontStretches.Normal ? "" : $"{i.Stretch}")} {(i.Weight == FontWeights.Normal ? "" : $"{i.Weight}")} {(i.Style == FontStyles.Normal ? "" : $"{i.Style}")}".Trim();
                typeface = typeface == "" ? "Regular" : typeface;
                typefaces.Add(typeface);
            }
            return typefaces;
        }

        private List<FontCharacter> GetCharactersForFont(FontFamily fontFamily, FamilyTypeface familyTypeface = null) {
            var characters = new List<FontCharacter>();

            if (fontFamily == null) return characters;

            Typeface typeface = new Typeface(
                fontFamily,
                familyTypeface?.Style ?? FontStyles.Normal,
                familyTypeface?.Weight ?? FontWeights.Normal,
                familyTypeface?.Stretch ?? FontStretches.Normal);

            if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                return characters;

            foreach (var kvp in glyphTypeface.CharacterToGlyphMap) {
                int unicodeValue = kvp.Key;
                ushort glyphIndex = kvp.Value;
                var width = glyphTypeface.AdvanceWidths[glyphIndex];
                var height = glyphTypeface.AdvanceHeights[glyphIndex];
                char character = (char)unicodeValue;
                var geometry = glyphTypeface.GetGlyphOutline(glyphIndex, 100.0, 100.0);
                //geometry.Transform = new TranslateTransform(0, 100.0);

                if (!char.IsControl(character)) {
                    characters.Add(new FontCharacter {
                        Character = character,
                        FontName = fontFamily.Source,
                        DesignerWidth = width,
                        DesignerHeight = height,
                        Geometry = geometry
                    });
                }
            }
            return characters.OrderBy(c => c.UnicodeValue).ToList();
        }

        private string ConvertToSvg(FontCharacter fontCharacter, double fontSize = 256.0) {
            Typeface typeface = new Typeface(new FontFamily(fontCharacter?.FontName), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                return null;

            ushort glyphIndex = glyphTypeface.CharacterToGlyphMap[fontCharacter.Character];
            Geometry geometry = glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, fontSize);
            //geometry.Transform = new TranslateTransform(0, fontSize);

            Rect bounds = geometry.Bounds;

            double scale = Math.Min(256.0 / bounds.Width, 256.0 / bounds.Height) * 0.9;
            double translateX = -bounds.Left + (256.0 - bounds.Width * scale) / 2.0 / scale;
            double translateY = -bounds.Top + (256.0 - bounds.Height * scale) / 2.0 / scale;

            TransformGroup transform = new TransformGroup();
            transform.Children.Add(new TranslateTransform(translateX, translateY));
            transform.Children.Add(new ScaleTransform(scale, scale));
            geometry.Transform = transform;


            StringBuilder svg = new StringBuilder();
            svg.AppendLine($"<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            svg.AppendLine($"<svg viewBox=\"0 0 {fontSize} {fontSize}\" xmlns=\"http://www.w3.org/2000/svg\">");
            svg.AppendLine("<path d=\"");

            PathGeometry pathGeometry = geometry.GetFlattenedPathGeometry();
            foreach (PathFigure figure in pathGeometry.Figures) {
                svg.Append($"M {figure.StartPoint.X},{figure.StartPoint.Y} ");

                foreach (PathSegment segment in figure.Segments) {
                    if (segment is LineSegment line) {
                        svg.Append($"L {line.Point.X},{line.Point.Y} ");
                    } else if (segment is PolyLineSegment polyLine) {
                        foreach (Point point in polyLine.Points) {
                            svg.Append($"L {point.X},{point.Y} ");
                        }
                    }
                    // Agregar más tipos de segmentos según necesidad
                }

                if (figure.IsClosed) svg.Append("Z ");
            }

            svg.Append("\" fill=\"black\"/>");
            svg.AppendLine("</svg>");

            return svg.ToString();
        }

        private void charItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            if (e.ClickCount == 2) {
                textBox.Text += (charListView.SelectedItem as FontCharacter)?.Character;
            }
        }

        private bool fontNameFilter(object item) {
            if (String.IsNullOrEmpty(filterBox.Text))
                return true;

            dynamic device = item;

            return device?.Name.IndexOf(filterBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private bool charNameFilter(object item) {
            if (string.IsNullOrEmpty(charFilterBox.Text))
                return true;

            dynamic device = item;

            return device?.UnicodeHex.IndexOf(charFilterBox.Text, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private void filterBox_TextChanged(object sender, TextChangedEventArgs e) {
            CollectionViewSource.GetDefaultView(fontListBox.ItemsSource).Refresh();
        }

        private void charFilterBox_TextChanged(object sender, TextChangedEventArgs e) {
            CollectionViewSource.GetDefaultView(charListView.ItemsSource).Refresh();
        }

        private void Hyperlink_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(((sender as Hyperlink).Parent as TextBlock).Text);
        }

        private void hypCopyGlyph_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(((sender as Hyperlink).DataContext as FontCharacter).UnicodeHex);
        }

        private void hypCopyUTF16_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(char.ConvertFromUtf32(((sender as Hyperlink).DataContext as FontCharacter).UnicodeValue));
        }

        private void hypCopySVG_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(ConvertToSvg(((sender as Hyperlink).DataContext as FontCharacter)));
        }

        private void hypCopyData_Click(object sender, RoutedEventArgs e) {
            Clipboard.SetText(((sender as Hyperlink).DataContext as FontCharacter).Data);
        }

        private void typefacesCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (fontListBox.SelectedItem != null) {
                dynamic font = fontListBox.SelectedItem;
                charListView.ItemsSource = GetCharactersForFont(font?.FontFamily as FontFamily, typefacesCombo.SelectedValue as FamilyTypeface);
            }
        }

        private void TextBlock_MouseDown(object sender, MouseButtonEventArgs e) {

        }

        private void charListView_PreviewKeyDown(object sender, KeyEventArgs e) {
            if (charListView.SelectedItem == null) return;
            var index = charListView.SelectedIndex;
            var container = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromIndex(index);
            if (container == null) return;
            var location = container.TranslatePoint(new Point(), charListView);
            var elementsLeft = (int)(location.X / container.ActualWidth);
            var elementsRight = (int)((charListView.ActualWidth - (location.X + container.ActualWidth)) / (container.ActualWidth + 3));
            var elementsUp = (int)(location.Y / container.ActualHeight);
            var elementsBottom = (int)((charListView.ActualHeight - (location.Y + container.ActualHeight)) / (container.ActualHeight + 3));
            var scroll = GetElementChild<ScrollViewer>(charListView);
            if (scroll == null) return;
            var offset = scroll.VerticalOffset;
            e.Handled = true;

            switch (e.Key) {
                case Key.Down:
                case Key.Right: {
                        var newIndex = e.Key == Key.Down ? index + elementsRight + elementsLeft + 1 : index + 1;
                        if (newIndex >= charListView.Items.Count) return;
                        if (elementsBottom == 0) {
                            var element = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromIndex(newIndex);
                            int auto = 0;
                            while (element == null) {
                                scroll.ScrollToVerticalOffset(offset + 120);
                                scroll.UpdateLayout();
                                element = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromIndex(newIndex);
                                auto += 120;
                            }
                            var newLocation = element.TranslatePoint(new Point(), charListView);
                            var newOffset = (newLocation - location).Y + auto;
                            var fixOffset = (location.Y + (container.ActualHeight + 6)) - charListView.ActualHeight;
                            scroll.ScrollToVerticalOffset(offset + newOffset + fixOffset);
                        }
                        charListView.SelectedIndex = newIndex;
                        return;
                    }
                case Key.Up:
                case Key.Left: {
                        var newIndex = e.Key == Key.Up ? index - (elementsRight + elementsLeft + 1) : index - 1;
                        if (newIndex < 0) return;
                        if (elementsUp == 0) {
                            var element = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromIndex(newIndex);
                            int auto = 0;
                            while (element == null) {
                                scroll.ScrollToVerticalOffset(offset - 120);
                                scroll.UpdateLayout();
                                element = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromIndex(newIndex);
                                auto += 120;
                            }
                            var newLocation = element.TranslatePoint(new Point(), charListView);
                            var newOffset = (location - newLocation).Y + auto;
                            var fixOffset = 6 - location.Y;
                            scroll.ScrollToVerticalOffset(offset - (newOffset + fixOffset));
                        }
                        charListView.SelectedIndex = newIndex;
                        return;
                    }
            }
        }

        private bool ItsItemVisible(object item) {
            var container = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromItem(item);
            if (container == null) return false;

            var scrollViewer = GetElementChild<ScrollViewer>(charListView);
            if (scrollViewer == null) return false;

            var transform = container.TransformToVisual(scrollViewer);
            var position = transform.Transform(new Point());

            return position.Y >= 0 && position.Y <= scrollViewer.ViewportHeight;
        }

        private void ScrollToCharItemVirtualized(object item) {
            if (item == null) return;

            var container = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromItem(item);
            if (container == null) {
                charListView.ScrollIntoView(item);
                charListView.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() => {
                    ScrollToCharItemVirtualized(item);
                }));
                return;
            }

            var scrollViewer = GetElementChild<ScrollViewer>(charListView);
            if (scrollViewer == null) return;

            var transform = container.TransformToVisual(scrollViewer);
            var position = transform.Transform(new Point());

            var targetOffset = position.Y - (scrollViewer.ViewportHeight / 2) + (container.ActualHeight / 2);
            scrollViewer.ScrollToVerticalOffset(targetOffset);
        }

        private void ScrollToCharIndex(int index) {
            if (charListView.Items.Count == 0) return;
            index = Math.Max(Math.Min(index, charListView.Items.Count - 1), 0);
            var scroll = GetElementChild<ScrollViewer>(charListView);
            if (scroll == null) return;
            var offset = 0;
            var element = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromIndex(index);
            int auto = 0;
            while (element == null) {
                if (offset + auto > scroll.ExtentHeight) return;
                scroll.ScrollToVerticalOffset(offset + 120);
                scroll.UpdateLayout();
                element = (FrameworkElement)charListView.ItemContainerGenerator.ContainerFromIndex(index);
                auto += 120;
            }
            var newLocation = element.TranslatePoint(new Point(), charListView);
            var newOffset = newLocation.Y + auto;
            var fixOffset = 0;
            scroll.ScrollToVerticalOffset(offset + newOffset + fixOffset);
        }

        private T GetElementChild<T>(DependencyObject depObj) {
            if (depObj is T obj)
                return obj;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++) {
                var child = VisualTreeHelper.GetChild(depObj, i);
                var result = GetElementChild<T>(child);
                if (result != null) return result;
            }
            return default(T);
        }
    }
}
