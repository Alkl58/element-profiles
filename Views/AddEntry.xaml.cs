using System.Windows;

namespace element_profiles.Views
{
    public partial class AddEntry : Window
    {
        public string ProfileName { get; private set; }

        public AddEntry()
        {
            InitializeComponent();
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            ProfileName = TextBoxProfileName.Text;
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
