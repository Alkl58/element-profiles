using element_profiles.Models;
using element_profiles.Views;
using Newtonsoft.Json;
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
        private static readonly string PROFILE_FOLDER = Path.Combine(Directory.GetCurrentDirectory(), "profiles");

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
                MessageDialog md = new()
                {
                    WindowTitle = "Element not found!",
                    Message = "Please make sure Element-Desktop is installed!"
                };
                md.ShowDialog();
                return;
            }

            var versionInfo = FileVersionInfo.GetVersionInfo(elementExec);
            string version = versionInfo.FileVersion ?? "Error";
            ElementVersion = version;
            elementExecutablePath = elementExec;
        }

        public void LoadProfiles()
        {
            ElementProfiles = new();

            // Create Profiles Directory if it doesn't exist
            if (!Directory.Exists(PROFILE_FOLDER)) {
                try
                {
                    Directory.CreateDirectory(PROFILE_FOLDER);
                }
                catch (Exception ex)
                {
                    MessageDialog md = new()
                    {
                        WindowTitle = "Error",
                        Message = ex.Message,
                    };
                    md.ShowDialog();
                    return;
                }
            }

            // Load Profiles from Profile Folder
            foreach(string file in Directory.GetFiles(PROFILE_FOLDER, "*.json"))
            {
                string json = File.ReadAllText(file);
                Profile? deserialized = JsonConvert.DeserializeObject<Profile>(json);
                if (deserialized != null)
                {
                    ElementProfiles.Add(deserialized);
                }
            }

            // TO-DO: Verify profiles all still exist

            // Import Element Profiles (AppData)
            string roamingAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            foreach (string folder in Directory.GetDirectories(roamingAppdata, "*Element*", SearchOption.TopDirectoryOnly))
            {
                string folderName = Path.GetFileName(folder);

                // Default Profile
                if (folderName == "Element")
                {
                    // Skip already imported profiles
                    if (ProfileExists(folderName))
                    {
                        Debug.WriteLine($"Skipped import: {folderName}");
                        continue;
                    }

                    // Create Profile Object
                    Profile defaultProfile = new() { Name = "Default" };

                    // Save Profile Object
                    string defaultJson = JsonConvert.SerializeObject(defaultProfile);
                    File.WriteAllText(Path.Combine(PROFILE_FOLDER, folderName + ".json"), defaultJson);

                    ElementProfiles.Add(defaultProfile);
                    continue;
                }

                string profileName = folderName.Replace("Element-", "");

                // Skip already imported profiles
                if (ProfileExists(profileName))
                {
                    Debug.WriteLine($"Skipped import: {profileName}");
                    continue;
                }

                // Create Profile Object
                Profile profile = new() { Name = profileName };

                // Save Profile Object
                string json = JsonConvert.SerializeObject(profile);
                File.WriteAllText(Path.Combine(PROFILE_FOLDER, folderName + ".json"), json);

                // Add Profile to List
                ElementProfiles.Add(profile);
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