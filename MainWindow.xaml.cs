using element_profiles.Models;
using element_profiles.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace element_profiles
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public static readonly string PROFILE_FOLDER = Path.Combine(Directory.GetCurrentDirectory(), "profiles");

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

                // Skip already imported profiles
                if (ProfileExists(folderName))
                {
                    Debug.WriteLine($"Skipped import: {folderName}");
                    continue;
                }

                // Profile Name, if the default folder is used, explicitly set it to "Default"
                string profileName =  folderName == "Element" ? "Default" : folderName.Replace("Element-", "");

                // Create Profile Object
                Profile profile = new() 
                { 
                    Name = profileName,
                    Folder = folderName,
                    IsDefaultProfile = folderName == "Element",
                };

                profile.SaveProfile();

                // Add Profile to List
                ElementProfiles.Add(profile);
            }
        }

        private bool ProfileExists(string folderName)
        {
            return ElementProfiles.Any(p =>
                string.Equals(p.Folder, folderName, StringComparison.OrdinalIgnoreCase));
        }


        #region Buttons
        private void ButtonEditProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxProfiles.SelectedItem is not Profile selectedProfile)
            {
                return;
            }

            // We don't want to edit the original object
            Profile tempProfile = new()
            {
                Name = selectedProfile.Name,
                Folder = selectedProfile.Folder,
            };

            EditEntry editEntry = new()
            {
                Profile = tempProfile
            };

            bool? result = editEntry.ShowDialog();
            if (result == true)
            {
                selectedProfile.Name = tempProfile.Name;
                selectedProfile.SaveProfile();

                // Reload List
                LoadProfiles();
            }
        }

        private void ButtonRemoveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ListBoxProfiles.SelectedItem is not Profile selectedProfile)
            {
                return;
            }

            // Delete Entry from List if successfully deleted
            if (selectedProfile.DeleteProfile())
            {
                ElementProfiles.Remove(selectedProfile);
            }
        }

        private void ButtonLaunch_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Profile profile)
            {
                string argument = "";
                if (profile.Name != "Default")
                {
                    string profileArg = profile.Folder.Replace("Element-", "");
                    argument = $"--profile=\"{profileArg}\"";
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
            AddEntry addEntry = new();
            bool? result = addEntry.ShowDialog();

            if (result == true)
            {
                string folderName = $"Element-{addEntry.ProfileName}";

                if (ProfileExists(folderName))
                {
                    MessageDialog md = new()
                    {
                        WindowTitle = "Duplicate profile!",
                        Message = "A profile with this name already exists."
                    };
                    md.ShowDialog();
                    return;
                }

                Profile profile = new()
                {
                    Name = addEntry.ProfileName,
                    Folder = folderName,
                };

                profile.SaveProfile();

                ElementProfiles.Add(profile);
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