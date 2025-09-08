using System.Windows;
using System.Windows.Controls;

namespace RemarkableSleepScreenManager.Localization
{
    public partial class LanguageSelector : System.Windows.Controls.UserControl
    {
        public LanguageSelector()
        {
            InitializeComponent();
            
            // Set current language selection
            var currentLanguage = ResourceManager.CurrentCulture.Name;
            foreach (ComboBoxItem item in LanguageComboBox.Items)
            {
                if (item.Tag?.ToString() == currentLanguage)
                {
                    LanguageComboBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void OnLanguageChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is ComboBoxItem selectedItem && 
                selectedItem.Tag is string languageCode)
            {
                ResourceManager.SetLanguage(languageCode);
            }
        }
    }
}
