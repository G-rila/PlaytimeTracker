using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Navigation;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using Xceed.Wpf.Toolkit;


namespace PlaytimeTracker
{
    /// <summary>
    /// Interaction logic for PlaytimeSetter.xaml
    /// </summary>
    public partial class PlaytimeSetter : Window
    {
        public IGame _setGame;
        ICustomField _sExistingField;

        public PlaytimeSetter()
        {
            InitializeComponent();
            Mouse.OverrideCursor = Cursors.Arrow;
        }

        private void SCancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SSaveButton_Click(object sender, RoutedEventArgs e)
        {
            string LBFolder = AppDomain.CurrentDomain.BaseDirectory;
            string pluginFolder = Path.Combine(LBFolder, "Plugins");
            Directory.CreateDirectory(pluginFolder + "\\PlaytimeTracker");
            string subFolder = Path.Combine(pluginFolder, "PlaytimeTracker");
            string oldSaveFile = Path.Combine(subFolder, _setGame.Title + "." + _setGame.Id + ".txt");
            string saveFile = Path.Combine(subFolder, _setGame.Id + ".txt");

            var existingField = _setGame.GetAllCustomFields().FirstOrDefault(f => f.Name.Equals("Playtime", StringComparison.OrdinalIgnoreCase));

            if (File.Exists(oldSaveFile))
            {
                //check if old style txt file exists and update to new style and delete old if found
                string s = File.ReadAllText(oldSaveFile);
                TimeSpan t = TimeSpan.Parse(s);
                File.WriteAllText(saveFile, t.ToString("G"));
                File.Delete(oldSaveFile);
            }

            TimeSpan? ts = sTimesetter.Value;
            TimeSpan timeSpan;

            if (ts.HasValue)
            {
                timeSpan = ts.Value;
                if (existingField != null)
                {
                    existingField.Value = PlaytimeTracker.FriendlyTimeOutput(timeSpan);
                    File.WriteAllText(saveFile, timeSpan.ToString("G"));
                    PluginHelper.DataManager.Save();
                    System.Windows.MessageBox.Show("Saved");
                    this.Close();
                }
                else
                {
                    var newField = _setGame.AddNewCustomField();
                    newField.Name = "Playtime";
                    newField.Value = PlaytimeTracker.FriendlyTimeOutput(timeSpan);
                    File.WriteAllText(saveFile, timeSpan.ToString("G"));
                    PluginHelper.DataManager.Save();
                    System.Windows.MessageBox.Show("Saved");
                    this.Close();
                }
            }
            else
            {
                System.Windows.MessageBox.Show("Input a playtime");
                sTimesetter.Focus();
            }
        }
    }
}
