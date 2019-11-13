using System;
using System.IO;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using System.Linq;

namespace PlaytimeTracker
{
    public class PlaytimeTracker : IGameLaunchingPlugin
    {
        IGame _game;
        string _GameId;
        DateTime _Started;
        DateTime _Finished;
        TimeSpan _SessionTimePlayed;
        TimeSpan _TotalTimePlayed;
        
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
            string saveFile = Path.Combine(subFolder, _GameId + ".txt");

            var existingField = _game.GetAllCustomFields().FirstOrDefault(f => f.Name.Equals("Playtime", StringComparison.OrdinalIgnoreCase));

            if (File.Exists(saveFile))
            {
                //file exists, update
                string s = File.ReadAllText(saveFile);
                TimeSpan ts = TimeSpan.Parse(s);
                _TotalTimePlayed = _SessionTimePlayed.Add(ts);
                File.WriteAllText(saveFile, _TotalTimePlayed.ToString());
                
                existingField.Value = FriendlyTimeOutput(_TotalTimePlayed);
            }
            else
            {
                //file doesn't exist, create
                File.WriteAllText(saveFile, _SessionTimePlayed.ToString());

                var newField = _game.AddNewCustomField();
                newField.Name = "Playtime";
                newField.Value = FriendlyTimeOutput(_SessionTimePlayed);
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
    }
}
