using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using MessageBox = System.Windows.MessageBox;
using System.Windows.Threading;
using Button = System.Windows.Controls.Button;

namespace MMM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public const string CONFIG_FILE = "mmmconfig.txt";
        private Config config = new Config();
        public ObservableCollection<Modpack> Modpacks { get; set; } = new ObservableCollection<Modpack>();

        private DispatcherTimer timer;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += autoReloadModpacks;
            timer.Start();
        }

        private void updateConfigurationFields()
        {
            pathModpacks.Text = config.modpacksPath;
            pathMods.Text = config.modsPath;
            pathBackup.Text = config.backupPath;
            btnConfCancel.IsEnabled = false;
            btnConfSave.IsEnabled = false;
        }

        private void onTabChange(object sender, SelectionChangedEventArgs e)
        {
            if (((TabItem)((System.Windows.Controls.TabControl)sender).SelectedItem).Header.ToString() == "Configuration")
            {
                updateConfigurationFields();
            }
        }

        private void onWindowLoaded(object sender, RoutedEventArgs e)
        {
            #region Load & Validate Configuration
            if (!System.IO.File.Exists(CONFIG_FILE))
            {
                foreach (TabItem tab in tabControl.Items)
                {
                    if (tab.Header.ToString() == "Configuration")
                    {
                        tabControl.SelectedItem = tab;
                    }
                    else
                    {
                        tab.IsEnabled = false;
                    }
                }
                MessageBox.Show("Configuration file not found. Please create a new configuration file.", "Configuration File Not Found", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            config = FileHandler.ParseConfig(CONFIG_FILE);

            if (!config.isValid)
            {
                foreach (TabItem tab in tabControl.Items)
                {
                    if (tab.Header.ToString() == "Configuration")
                    {
                        tabControl.SelectedItem = tab;
                    }
                    else
                    {
                        tab.IsEnabled = false;
                    }
                }
                MessageBox.Show("Configuration file is invalid. Please create a new configuration file.", "Invalid Configuration File", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            #endregion
        }

        private void loadModpacks()
        {
            if (config.modpacksPath == null || config.modsPath == null || config.backupPath == null)
            {
                return;
            }
            List<Modpack> modpacks = FileHandler.LoadModpacks(config.modpacksPath);
            Modpacks.Clear();
            foreach (Modpack modpack in modpacks)
            {
                Modpacks.Add(modpack);
            }
        }

        private void autoReloadModpacks(object? sender, EventArgs e)
        {
            loadModpacks();
            timer.Interval = TimeSpan.FromSeconds(5);
        }

        #region Configuration
        private void previewConfInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (e.Text == "?" || e.Text == ">" || e.Text == "<" || e.Text == "*" || e.Text == "?" || e.Text == "'" || e.Text == "\"" || e.Text == "\'" || e.Text == "|") e.Handled = true;
            else e.Handled = false;
        }

        private void checkConfiguration(object sender, System.Windows.Input.KeyEventArgs? e)
        {
            if (pathModpacks.Text.Length > 4 &&
                pathMods.Text.Length > 4 &&
                pathBackup.Text.Length > 4)
            {
                btnConfSave.IsEnabled = true;
                if (config.isValid) btnConfCancel.IsEnabled = true;
                else btnConfCancel.IsEnabled = false;
            }
            else
            {
                btnConfSave.IsEnabled = false;
                btnConfCancel.IsEnabled = false;
            }
        }

        private void onClick_modsdirhelp(object sender, RoutedEventArgs e)
        {
            string defaultModsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft\\mods";
            MessageBoxResult result = MessageBox.Show("Default mods directory is:\n" + defaultModsPath + "\n\nWould you like to use this path?", "Default Mods Directory", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                pathMods.Text = defaultModsPath;
            }
        }

        private void onClick_createmodpackfolder(object sender, RoutedEventArgs e)
        {
            if (pathModpacks.Text.Length < 4)
            {
                MessageBox.Show("Please enter a valid modpack folder path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (System.IO.Directory.Exists(pathModpacks.Text))
            {
                MessageBox.Show("Modpack folder already exists at:\n" + pathModpacks.Text, "Modpack Folder Already Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                System.IO.Directory.CreateDirectory(pathModpacks.Text);
                System.IO.Directory.CreateDirectory(
                    System.IO.Path.Combine(pathModpacks.Text, ".icons")
                );
                MessageBox.Show("New modpack folder created at:\n" + pathModpacks.Text, "New Modpack Folder Created", MessageBoxButton.OK, MessageBoxImage.Information);
                checkConfiguration(sender, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create modpack folder:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void onClick_createbackupfolder(object sender, RoutedEventArgs e)
        {
            if (pathBackup.Text.Length < 4)
            {
                MessageBox.Show("Please enter a valid backup folder path.", "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (System.IO.Directory.Exists(pathBackup.Text))
            {
                MessageBox.Show("Backup folder already exists at:\n" + pathBackup.Text, "Backup Folder Already Exists", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                System.IO.Directory.CreateDirectory(pathBackup.Text);
                MessageBox.Show("New backup folder created at:\n" + pathBackup.Text, "New Backup Folder Created", MessageBoxButton.OK, MessageBoxImage.Information);
                checkConfiguration(sender, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to create backup folder:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void onClick_browse(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            folderBrowser.Description = "Select a directory";
            folderBrowser.ShowNewFolderButton = true;
            folderBrowser.RootFolder = Environment.SpecialFolder.Desktop;
            if (folderBrowser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                ((System.Windows.Controls.TextBox)(this.FindName(((System.Windows.Controls.Button)sender).Uid))).Text = folderBrowser.SelectedPath;
                checkConfiguration(sender, null);
            }
        }

        private void onClick_confSave(object sender, RoutedEventArgs e)
        {
            #region Validate Configuration
            if (!System.IO.Directory.Exists(pathModpacks.Text))
            {
                MessageBox.Show("Invalid modpack folder path:\n" + pathModpacks.Text, "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!System.IO.Directory.Exists(pathMods.Text))
            {
                MessageBox.Show("Invalid mods folder path:\n" + pathMods.Text, "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!System.IO.Directory.Exists(pathBackup.Text))
            {
                MessageBox.Show("Invalid backup folder path:\n" + pathBackup.Text, "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            #endregion
            
            config = FileHandler.SaveConfig(pathModpacks.Text, pathMods.Text, pathBackup.Text);

            foreach (TabItem tab in tabControl.Items)
            {
                tab.IsEnabled = true;
            }

            updateConfigurationFields();
        }

        private void onClick_confCancel(object sender, RoutedEventArgs e)
        {
            updateConfigurationFields();
        }
        #endregion

        private void onClick_modpackLoad(object sender, RoutedEventArgs e)
        {
            if (config.modpacksPath == null)
            {
                MessageBox.Show("Modpacks path is not configured.", "Configuration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string modpackName = (string)((Button)sender).Tag;
            string modpackPath = System.IO.Path.Combine(config.modpacksPath, modpackName);

            if (!System.IO.Directory.Exists(modpackPath))
            {
                MessageBox.Show("Modpack folder does not exist:\n" + modpackPath, "Invalid Path", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int modAmount = System.IO.Directory.GetFiles(modpackPath, "*.jar").Length;

            if (modAmount == 0)
            {
                MessageBox.Show("Modpack does not contain any mods:\n" + modpackPath, "Empty Modpack", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult messageBoxResult = MessageBox.Show($"Are you sure you want to load the modpack '{modpackName}'?\nThe modpack contains {modAmount} mods.", "Load Modpack", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.No) return;

            try
            {
                foreach (string file in System.IO.Directory.GetFiles(config.backupPath, "*.jar"))
                {
                    System.IO.File.Delete(file);
                }

                foreach (string file in System.IO.Directory.GetFiles(config.modsPath, "*.jar"))
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    System.IO.File.Move(file, System.IO.Path.Combine(config.backupPath, fileName), true);
                }
                foreach (string file in System.IO.Directory.GetFiles(modpackPath, "*.jar"))
                {
                    string fileName = System.IO.Path.GetFileName(file);
                    System.IO.File.Copy(file, System.IO.Path.Combine(config.modsPath, fileName), true);
                }
                MessageBox.Show($"Modpack {modpackName} loaded successfully.", "Load Modpack", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load modpack:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void onClick_modpackEdit(object sender, RoutedEventArgs e)
        {
            Modpack modpack = (Modpack)((Button)sender).DataContext;
            Edit editWindow = new Edit(config, modpack);

            editWindow.Show();
        }

        private void createModpack(object sender, RoutedEventArgs e)
        {
            string modpackName = Microsoft.VisualBasic.Interaction.InputBox("Enter modpack name:", "Create Modpack", "New Modpack", -1, -1);
            if (string.IsNullOrWhiteSpace(modpackName))
            {
                MessageBox.Show("Modpack name cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (modpackName.Any(c => "\\/:?<>*|\"".Contains(c)))
            {
                MessageBox.Show("Modpack name contains invalid characters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (Modpacks.Any(m => m.name.Equals(modpackName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show("Modpack with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            try
            {
                string modpackPath = System.IO.Path.Combine(config.modpacksPath, modpackName);
                System.IO.Directory.CreateDirectory(modpackPath);

                MessageBox.Show($"Modpack '{modpackName}' created successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

                System.Diagnostics.Process.Start("explorer.exe", modpackPath);

                loadModpacks();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create modpack:\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}