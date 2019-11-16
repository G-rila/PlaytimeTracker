using System;
using System.IO;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using System.Linq;

namespace PlaytimeTracker
{
    public class PlaytimeTracker : IGameLaunchingPlugin, IGameMenuItemPlugin
    {
        IGame _game;
        string _GameId;
        DateTime _Started;
        DateTime _Finished;
        TimeSpan _SessionTimePlayed;
        TimeSpan _TotalTimePlayed;

        public bool SupportsMultipleGames => false;

        public string Caption => "Reset Playtime";

        public System.Drawing.Image IconImage => Properties.Resources.clock;

        public bool ShowInLaunchBox => true;

        public bool ShowInBigBox => true;

        public void OnAfterGameLaunched(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            _game = game;
            _GameId = game.Id;
            _Started = DateTime.UtcNow;
        }

        public void OnBeforeGameLaunching(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            throw new NotImplementedException();
        }

        public void OnGameExited()
        {
            _Finished = DateTime.UtcNow;
            _SessionTimePlayed = _Finished.Subtract(_Started);

            string LBFolder = AppDomain.CurrentDomain.BaseDirectory;
            string pluginFolder = Path.Combine(LBFolder, "Plugins");
            Directory.CreateDirectory(pluginFolder + "\\PlaytimeTracker");
            string subFolder = Path.Combine(pluginFolder, "PlaytimeTracker");
            string saveFile = Path.Combine(subFolder, _game.Title + "." + _GameId + ".txt");
            string oldSaveFile = Path.Combine(subFolder, _GameId + ".txt");

            //check to see if the Playtime custom field already exists
            var existingField = _game.GetAllCustomFields().FirstOrDefault(f => f.Name.Equals("Playtime", StringComparison.OrdinalIgnoreCase));

            if (File.Exists(oldSaveFile))
            {
                //check if old style txt file exists and update to new style and delete old if found
                string s = File.ReadAllText(oldSaveFile);
                TimeSpan ts = TimeSpan.Parse(s);
                File.WriteAllText(saveFile, ts.ToString("G"));
                File.Delete(oldSaveFile);
            }
            
            if (File.Exists(saveFile))
            {
                //file exists, update
                string s = File.ReadAllText(saveFile);
                TimeSpan ts = TimeSpan.Parse(s);

                _TotalTimePlayed = _SessionTimePlayed.Add(ts);
                File.WriteAllText(saveFile, _TotalTimePlayed.ToString("G"));

                if (existingField != null)
                {
                    existingField.Value = FriendlyTimeOutput(_TotalTimePlayed);
                }
                else
                {
                    var newField = _game.AddNewCustomField();
                    newField.Name = "Playtime";
                    newField.Value = FriendlyTimeOutput(_TotalTimePlayed);
                }
            }
            else
            {
                //file doesn't exist, create
                File.WriteAllText(saveFile, _SessionTimePlayed.ToString("G"));

                if (existingField != null)
                {
                    existingField.Value = FriendlyTimeOutput(_SessionTimePlayed);
                }
                else
                {
                    var newField = _game.AddNewCustomField();
                    newField.Name = "Playtime";
                    newField.Value = FriendlyTimeOutput(_SessionTimePlayed);
                }
            }
            PluginHelper.DataManager.Save();
        }
        public static string FriendlyTimeOutput(TimeSpan span)
        {
            string output;
            if (span.TotalMinutes < 1.0)
            {
                output = String.Format("{0} seconds", span.Seconds);
            }
            else if (span.TotalHours < 1.0)
            {
                output = String.Format("{0} minutes, {1} seconds", span.Minutes, span.Seconds);
            }
            else // more than 1 hour
            {
                output = String.Format("{0} hours, {1} minutes, {2} seconds", (int)span.TotalHours, span.Minutes, span.Seconds);
            }
            return output;
        }

        public bool GetIsValidForGame(IGame selectedGame)
        {
            var existingField = selectedGame.GetAllCustomFields().FirstOrDefault(f => f.Name.Equals("Playtime", StringComparison.OrdinalIgnoreCase));
            if (existingField != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool GetIsValidForGames(IGame[] selectedGames)
        {
            return false;
        }

        public void OnSelected(IGame selectedGame)
        {
            var existingField = selectedGame.GetAllCustomFields().FirstOrDefault(f => f.Name.Equals("Playtime", StringComparison.OrdinalIgnoreCase));
            existingField.Value = "";

            string LBFolder = AppDomain.CurrentDomain.BaseDirectory;
            string pluginFolder = Path.Combine(LBFolder, "Plugins");
            Directory.CreateDirectory(pluginFolder + "\\PlaytimeTracker");
            string subFolder = Path.Combine(pluginFolder, "PlaytimeTracker");
            string saveFile = Path.Combine(subFolder, selectedGame.Title + "." + selectedGame.Id + ".txt");
            if (File.Exists(saveFile))
            {
                File.WriteAllText(saveFile, "0:00:00:00");
            }
        }

        public void OnSelected(IGame[] selectedGames)
        {
            return;
        }
    }
}
