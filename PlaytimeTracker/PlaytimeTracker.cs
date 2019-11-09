using System;
using System.IO;
using Unbroken.LaunchBox.Plugins;
using Unbroken.LaunchBox.Plugins.Data;

namespace PlaytimeTracker
{
    public class PlaytimeTracker : IGameLaunchingPlugin
    {
        string _GameId;
        DateTime _Started;
        DateTime _Finished;
        TimeSpan _SessionTimePlayed;
        TimeSpan _TotalTimePlayed;
        public void OnAfterGameLaunched(IGame game, IAdditionalApplication app, IEmulator emulator)
        {
            if (game != null)
            {
                _GameId = game.Id;
                _Started = DateTime.UtcNow;
            }
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

            if (File.Exists(saveFile))
            {
                string s = File.ReadAllText(saveFile);
                TimeSpan ts = TimeSpan.Parse(s);
                _TotalTimePlayed = _SessionTimePlayed.Add(ts);
                File.WriteAllText(saveFile, _TotalTimePlayed.ToString());
            }
            else
            {
                _TotalTimePlayed = _SessionTimePlayed;
                File.WriteAllText(saveFile, _TotalTimePlayed.ToString());
            }
        }
    }
}
