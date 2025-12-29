using element_profiles.Controls;
using element_profiles.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace element_profiles
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static string? elementExecutablePath;

        private string _elementVersion;
        
        public string ElementVersion
        {
            get { return _elementVersion; }
            set 
            { 
                _elementVersion = value; 
                OnPropertyChanged(nameof(ElementVersion));
            }
        }

        private ObservableCollection<Profile> _elementProfiles;
        public ObservableCollection<Profile> ElementProfiles
        {
            get => _elementProfiles;
            set
            {
                _elementProfiles = value;
                OnPropertyChanged(nameof(ElementProfiles));
            }
        }

        public MainWindow()
        {
            
            InitializeComponent();
            DataContext = this;
            DetectElementInstall();
            LoadProfiles();
        }

        private void DetectElementInstall()
        {
            string localAppdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string elementExec = Path.Combine(localAppdata, "element-desktop", "Element.exe");
            if (!File.Exists(elementExec))
            {
                MessageBox.Show("Element Executable not found!");
                return;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(elementExec);
            string version = versionInfo.FileVersion ?? "Error";
            ElementVersion = version;
            elementExecutablePath = elementExec;

        }

        public void LoadProfiles()
        {
            ElementProfiles = new ObservableCollection<Profile>();

            string roamingAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            foreach(string folder in Directory.GetDirectories(roamingAppdata, "*Element*", SearchOption.TopDirectoryOnly))
            {
                string folderName = Path.GetFileName(folder);

                // Default Profile
                if (folderName == "Element")
                {
                    ElementProfiles.Add(new Profile() { Name = "Default" });
                    continue; 
                }

                string profileName = folderName.Replace("Element-", "");
                ElementProfiles.Add(new Profile() { Name = profileName });
            }
        }

        private bool ProfileExists(string profileName)
        {
            return ElementProfiles.Any(p =>
                string.Equals(p.Name, profileName, StringComparison.OrdinalIgnoreCase));
        }


        #region Buttons
        private void ButtonRemoveProfile_Click(object sender, RoutedEventArgs e)
        {
            var selectedProfile = ListBoxProfiles.SelectedItem as Profile;
            if (selectedProfile == null)
            {
                return;
            }

            if (selectedProfile.Name == "Default") 
            {
                MessageBox.Show("Can't delete default Profile");
                return;
            }

            string roamingAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string profileFolder = Path.Combine(roamingAppdata, "Element-" +  selectedProfile.Name);

            if (Directory.Exists(profileFolder)) 
            {
                try
                {
                    Directory.Delete(profileFolder, true);
                    ElementProfiles.Remove(selectedProfile);
                }
                catch (Exception ex) 
                { 
                    MessageBox.Show(ex.Message, "Error Deleting Profile", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Profile profile)
            {
                string argument = "";
                if (profile.Name != "Default")
                {
                    argument = $"--profile=\"{profile.Name}\"";
                }

                var startInfo = new ProcessStartInfo
                {
                    FileName = elementExecutablePath,
                    Arguments = argument,
                    UseShellExecute = true,
                };
                Process.Start(startInfo);
            }
        }

        private void ButtonAddProfile_Click(object sender, RoutedEventArgs e)
        {
            AddEntry addEntry = new AddEntry();
            bool? result = addEntry.ShowDialog();

            if (result == true)
            {
                string profileName = addEntry.ProfileName;
                if (ProfileExists(profileName))
                {
                    MessageBox.Show("A profile with this name already exists.", "Duplicate profile", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                ElementProfiles.Add(new Profile() { Name = profileName });
            }
        }
        #endregion

        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}