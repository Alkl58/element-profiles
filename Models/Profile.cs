using element_profiles.Views;
using Newtonsoft.Json;
using System.IO;

namespace element_profiles.Models
{
    public class Profile
    {
        public required string Folder { get; set; }
        public required string Name { get; set; }
        public bool IsDefaultProfile { get; set; } = false;

        public void SaveProfile()
        {
            string fileName = $"{Folder}.json";
            if (!IsDefaultProfile)
            {
                fileName = fileName.Replace("Element-", "");
            }

            string json = JsonConvert.SerializeObject(this);
            File.WriteAllText(Path.Combine(MainWindow.PROFILE_FOLDER, fileName), json);
        }

        public bool DeleteProfile()
        {
            string roamingAppdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string profileFolder = Path.Combine(roamingAppdata, Folder);

            if (Directory.Exists(profileFolder))
            {
                try
                {
                    // Delete Element Profile Folder
                    Directory.Delete(profileFolder, true);
                    
                }
                catch
                {
                    MessageDialog md = new()
                    {
                        WindowTitle = "Error Deleting Profile!",
                        Message = "Make sure the Element-Desktop instance is not running!"
                    };
                    md.ShowDialog();
                    return false;
                }
            }

            // Find json file and delete
            foreach (string file in Directory.GetFiles(MainWindow.PROFILE_FOLDER, "*.json"))
            {
                string json = File.ReadAllText(file);
                Profile? deserialized = JsonConvert.DeserializeObject<Profile>(json);
                if (deserialized != null)
                {
                    if (deserialized.Folder != Folder)
                    {
                        continue;
                    }

                    try
                    {
                        File.Delete(file);
                    }
                    catch
                    {
                        MessageDialog md = new()
                        {
                            WindowTitle = "Error Deleting Profile!",
                            Message = "Make sure the Element-Desktop instance is not running!"
                        };
                        md.ShowDialog();
                        return false;
                    }
                }
            }

            return true;
        }
    }
}
