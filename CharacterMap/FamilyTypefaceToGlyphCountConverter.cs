using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace CharacterMap {
    public sealed class FamilyTypefaceToGlyphCountConverter : IMultiValueConverter {

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture) {
            if (values?[0] is FamilyTypeface familyTypeface && values?[1] is FontFamilyItem familyItem)
                return GetGlyphCount(familyItem.FontFamily, familyTypeface.Style, familyTypeface.Weight, familyTypeface.Stretch).ToString();
            return -1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        private int GetGlyphCount(FontFamily f, FontStyle fontStyle, FontWeight fontWeight, FontStretch fontStretch) {
            Typeface typeface = new Typeface(f, fontStyle, fontWeight, fontStretch);
            if (!typeface.TryGetGlyphTypeface(out GlyphTypeface glyphTypeface))
                return 0;

            return glyphTypeface.GlyphCount;
        }
    }
}