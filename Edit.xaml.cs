using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.MessageBox;

namespace MMM
{
    /// <summary>
    /// Interaction logic for Edit.xaml
    /// </summary>
    public partial class Edit : Window
    {
        private Modpack modpack;
        private Config config = new Config();
        public Edit(Config config, Modpack modpack)
        {
            InitializeComponent();
            this.Title = $"View modpack '{modpack.name}'";
            this.modpack = modpack;
            this.config = config;

            img.Source = new BitmapImage(new Uri(modpack.imagePath, UriKind.RelativeOrAbsolute));
            modpackname.Text = modpack.name;
            modslist.Text = string.Join(Environment.NewLine, modpack.mods.Select(mod => mod.Replace("<LineBreak/>", Environment.NewLine)));
        }

        private void savebtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(modpackname.Text) || string.IsNullOrWhiteSpace(modslist.Text))
            {
                MessageBox.Show("Modpack name and mods list cannot be empty.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (modpackname.Text.Any(c => "\\/:?<>*|\"".Contains(c)))
            {
                MessageBox.Show("Modpack name contains invalid characters.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (modpackname.Text != modpack.name)
            {
                if (config.modpacksPath.Contains(modpackname.Text))
                {
                    MessageBox.Show("Modpack with this name already exists.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                modpack.name = modpackname.Text;

                MessageBox.Show($"Modpack '{modpack.name}' renamed successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", System.IO.Path.Combine(config.modpacksPath, modpack.name));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open modpack directory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void deleteclick(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show($"Are you sure you want to delete the modpack '{modpack.name}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                try
                {
                    System.IO.Directory.Delete(System.IO.Path.Combine(config.modpacksPath, modpack.name), true);
                    MessageBox.Show($"Modpack '{modpack.name}' deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete modpack: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
