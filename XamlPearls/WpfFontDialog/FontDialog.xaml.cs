using System;
using System.Collections;
using System.Windows;
using System.Windows.Media;

namespace XamlPearls.XamlFont
{
    /// <summary>
    /// Interaction logic for FontDialog.xaml
    /// </summary>
    public partial class FontDialog : Window
    {
        private int[] _defaultFontSizes;
        private int[] _fontSizes = null;
        private FontInfo _selectedFont;

        public FontDialog(bool previewFontInFontList = true, bool allowArbitraryFontSizes = true, bool showColorPicker = true)
        {
            InitializeComponent();
            I18NUtil.SetLanguage(Resources);
            this.colorFontChooser.PreviewFontInFontList = previewFontInFontList;
            this.colorFontChooser.AllowArbitraryFontSizes = allowArbitraryFontSizes;
            this.colorFontChooser.ShowColorPicker = showColorPicker;
            _fontSizes = new int[500];
            for (int i = 0; i < 500; i++)
            {
                _fontSizes[i] = i + 1;
            }
        }

        public FontInfo Font
        {
            get
            {
                return _selectedFont;
            }
            set
            {
                _selectedFont = value;
            }
        }
        public int[] FontSizes
        {
            get
            {
                return _fontSizes ?? _defaultFontSizes;
            }
            set
            {
                _fontSizes = value;
            }
        }
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.Font = this.colorFontChooser.SelectedFont;
            base.DialogResult = new bool?(true);
        }

        private void SyncFontColor()
        {
            this.colorFontChooser.colorPicker.SelectedColor = this.Font.BrushColor.Color;
            this.colorFontChooser.txtSampleText.Foreground = this.Font.BrushColor;
        }

        private void SyncFontName()
        {
            string fontFamilyName = this._selectedFont.Family.Source;
            bool foundMatch = false;
            int idx = 0;
            foreach (object item in (IEnumerable)this.colorFontChooser.lstFamily.Items)
            {
                if (fontFamilyName == item.ToString())
                {
                    foundMatch = true;
                    break;
                }
                idx++;
            }
            if (!foundMatch)
            {
                idx = 0;
            }
            this.colorFontChooser.lstFamily.SelectedIndex = idx;
            this.colorFontChooser.lstFamily.ScrollIntoView(this.colorFontChooser.lstFamily.Items[idx]);
        }

        private void SyncFontSize()
        {
            double fontSize = this._selectedFont.Size;
            this.colorFontChooser.lstFontSizes.ItemsSource = FontSizes;
            this.colorFontChooser.tbFontSize.Text = fontSize.ToString();
        }

        private void SyncFontTypeface()
        {
            string fontTypeFaceSb = FontInfo.TypefaceToString(this._selectedFont.Typeface);
            int idx = 0;
            foreach (object item in (IEnumerable)this.colorFontChooser.lstTypefaces.Items)
            {
                if (fontTypeFaceSb == FontInfo.TypefaceToString(item as FamilyTypeface))
                {
                    break;
                }
                idx++;
            }
            this.colorFontChooser.lstTypefaces.SelectedIndex = idx;
            this.colorFontChooser.lstTypefaces.ScrollIntoView(this.colorFontChooser.lstTypefaces.SelectedItem);
        }

        private void Window_Loaded_1(object sender, RoutedEventArgs e)
        {
            this.SyncFontColor();
            this.SyncFontName();
            this.SyncFontSize();
            this.SyncFontTypeface();
        }
    }
}
