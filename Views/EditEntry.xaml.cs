using element_profiles.Models;
using System.ComponentModel;
using System.Windows;

namespace element_profiles.Views
{
    public partial class EditEntry : Window, INotifyPropertyChanged
    {
        private Profile _profile;

        public Profile Profile
        {
            get { return _profile; }
            set
            {
                _profile = value;
                OnPropertyChanged(nameof(Profile));
            }
        }

        public EditEntry()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
