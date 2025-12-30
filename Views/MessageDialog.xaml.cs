using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;

namespace element_profiles.Views
{
    public partial class MessageDialog : Window, INotifyPropertyChanged
    {
        private string _windowTitle;
        public string WindowTitle {
            get { return _windowTitle; }
            set
            {
                _windowTitle = value;
                OnPropertyChanged(nameof(WindowTitle));
            }
        }

        private string _message;
        public string Message { 
            get { return _message; } 
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            } 
        }

        public MessageDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
