using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace MMM
{
    internal class FileHandler
    {
        /// <summary>
        /// Load the config file and parse the paths.
        /// </summary>
        /// <param name="path">Configuration file path</param>
        /// <returns>Empty or valid Config object</returns>
        public static Config ParseConfig(string path)
        {
            if (!System.IO.File.Exists(path))
            {
                return new Config();
            }

            try
            {
                string[] lines = System.IO.File.ReadAllLines(path);
                string modpacksPath = lines[0].Split('=')[1].Trim();
                string modsPath = lines[1].Split('=')[1].Trim();
                string backupPath = lines[2].Split('=')[1].Trim();

                if (string.IsNullOrEmpty(modpacksPath) || string.IsNullOrEmpty(modsPath) || string.IsNullOrEmpty(backupPath))
                {
                    return new Config();
                }
                if (!System.IO.Directory.Exists(modpacksPath) || !System.IO.Directory.Exists(modsPath) || !System.IO.Directory.Exists(backupPath))
                {
                    return new Config();
                }

                return new Config()
                {
                    isValid = true,
                    modpacksPath = modpacksPath,
                    modsPath = modsPath,
                    backupPath = backupPath
                };
            }
            catch (Exception)
            {
                return new Config();
            }
        }

        /// <summary>
        /// Save given paths to the config file. <b>Parameters are not validated!</b>
        /// </summary>
        /// <param name="modpacks">Config.modpacksPath</param>
        /// <param name="mods">Config.modsPath</param>
        /// <param name="backup">Config.backupPath</param>
        /// <returns>Config</returns>
        public static Config SaveConfig(string modpacks, string mods, string backup)
        {
            try
            {
                System.IO.File.WriteAllLines("mmmconfig.txt", new string[]
                {
                    $"modpacksPath={modpacks}",
                    $"modsPath={mods}",
                    $"backupPath={backup}"
                });
                return new Config()
                {
                    isValid = true,
                    modpacksPath = modpacks,
                    modsPath = mods,
                    backupPath = backup
                };
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error saving configuration file: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return new Config();
            }
        }

        public static List<Modpack> LoadModpacks(string path)
        {
            List<Modpack> modpacks = new List<Modpack>();
            if (!System.IO.Directory.Exists(path))
            {
                return modpacks;
            }
            try
            {
                string[] directories = System.IO.Directory.GetDirectories(path);
                foreach (string directory in directories)
                {
                    string name = System.IO.Path.GetFileName(directory);

                    if (string.IsNullOrEmpty(name)) continue;
                    if (name == ".icons") continue;
                    if (name.Length > 13) continue;

                    string imagePath = System.IO.Path.Combine(directory, "icon.png");

                    if (!System.IO.File.Exists(imagePath))
                    {
                        imagePath = System.IO.Path.Combine(directory, "icon.jpg");
                    }
                    if (!System.IO.File.Exists(imagePath))
                    {
                        imagePath = "default.png";
                    }

                    Image image = Image.FromFile(imagePath);
                    if (image.Width != image.Height)
                    {
                        MessageBox.Show($"'{directory}' has an invalid icon: not a square image. Please use a square image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        continue;
                    }

                    List<string> mods = new List<string>();

                    string[] modFiles = System.IO.Directory.GetFiles(directory, "*.jar", System.IO.SearchOption.AllDirectories);
                    foreach (string modFile in modFiles)
                    {
                        string modName = System.IO.Path.GetFileName(modFile);
                        mods.Add(modName);
                    }

                    modpacks.Add(new Modpack(name, imagePath, mods));
                }
            }
            catch (Exception e)
            {
                MessageBox.Show($"Error loading modpacks: {e.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return modpacks;
        }
    }
}
