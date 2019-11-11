using System;
using System.IO;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;
using System.Linq;

namespace PlaytimeTracker
{
    public class PlaytimeTracker : IGameLaunchingPlugin, ICustomField
    {
        string _GameId;
        DateTime _Started;
        DateTime _Finished;
        TimeSpan _SessionTimePlayed;
        TimeSpan _TotalTimePlayed;
        IGame _game;

        public string GameId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }

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
            string LBFolder = AppDomain.CurrentDomain.BaseDirectory;
            string pluginFolder = Path.Combine(LBFolder, "Plugins");
            Directory.CreateDirectory(pluginFolder + "\\PlaytimeTracker");
            string subFolder = Path.Combine(pluginFolder, "PlaytimeTracker");
            string saveFile = Path.Combine(subFolder, _GameId + ".txt");

            _Finished = DateTime.UtcNow;
            _SessionTimePlayed = _Finished.Subtract(_Started);

            var existingField = _game.GetAllCustomFields().FirstOrDefault(f => f.Name.Equals("Playtime", StringComparison.OrdinalIgnoreCase));
            
            if (existingField != null)
            {
                //not null, already exists
                string s = existingField.Value;
                TimeSpan ts = TimeSpan.Parse(s);
                _TotalTimePlayed = _SessionTimePlayed.Add(ts);
                existingField.Value = _TotalTimePlayed.ToString();
                PluginHelper.DataManager.Save();
            }
            else
            {
                //null, doesn't exist, add new
                _TotalTimePlayed = _SessionTimePlayed;
                var field = _game.AddNewCustomField();
                field.Name = "Playtime";
                field.Value = _TotalTimePlayed.ToString();
                PluginHelper.DataManager.Save();
            }

            //if (File.Exists(saveFile))
            //{
            //    string s = File.ReadAllText(saveFile);
            //    TimeSpan ts = TimeSpan.Parse(s);
            //    _TotalTimePlayed = _SessionTimePlayed.Add(ts);
            //    File.WriteAllText(saveFile, _TotalTimePlayed.ToString());
            //}
            //else
            //{
            //    _TotalTimePlayed = _SessionTimePlayed;
            //    File.WriteAllText(saveFile, _TotalTimePlayed.ToString());
            //}
        }
    }
}
