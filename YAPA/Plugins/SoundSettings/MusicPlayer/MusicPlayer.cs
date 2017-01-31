﻿using System;
using System.IO;
using YAPA.Contracts;
using YAPA.Plugins;
using YAPA.WPF;

namespace YAPA.Shared
{
    [BuiltInPlugin(Hide = true)]
    public class MusicPlayerPluginMetas : IPluginMeta
    {
        public string Title => "Music player";

        public Type Plugin => typeof(MusicPlayerPlugin);

        public Type Settings => typeof(MusicPlayerPluginSettings);

        public Type SettingEditWindow => typeof(MusicPlayerPluginSettingWindow);
    }

    public class MusicPlayerPlugin : IPlugin
    {
        private readonly IPomodoroEngine _engine;
        private readonly MusicPlayerPluginSettings _settings;
        private readonly IMusicPlayer _musicPlayer;
        private readonly PomodoroEngineSettings _engineSettings;

        public MusicPlayerPlugin(IPomodoroEngine engine, MusicPlayerPluginSettings settings, IMusicPlayer musicPlayer, PomodoroEngineSettings engineSettings)
        {
            _engine = engine;
            _settings = settings;
            _musicPlayer = musicPlayer;
            _engineSettings = engineSettings;

            _engine.PropertyChanged += _engine_PropertyChanged;
        }

        private void _engine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_engine.Phase))
            {
                _musicPlayer.Stop();
                Play();
            }
        }

        private void Play()
        {
            if (_engineSettings.DisableSoundNotifications)
            {
                return;
            }

            var songToPlay = string.Empty;
            var repeat = false;

            switch (_engine.Phase)
            {
                case PomodoroPhase.Work:
                    songToPlay = _settings.WorkSong;
                    repeat = _settings.RepeatWorkSong;
                    break;
                case PomodoroPhase.Break:
                    songToPlay = _settings.BreakSong;
                    repeat = _settings.RepeatBreakSong;
                    break;
                case PomodoroPhase.BreakEnded:
                case PomodoroPhase.WorkEnded:
                    break;
            }

            if (File.Exists(songToPlay))
            {
                _musicPlayer.Load(songToPlay);
                _musicPlayer.Play(repeat);
            }
        }
    }

    public class MusicPlayerPluginSettings : IPluginSettings
    {
        private readonly ISettingsForComponent _settings;

        public string WorkSong
        {
            get { return _settings.Get<string>(nameof(WorkSong), null, true); }
            set { _settings.Update(nameof(WorkSong), value, true); }
        }

        public bool RepeatWorkSong
        {
            get { return _settings.Get(nameof(RepeatWorkSong), false, true); }
            set { _settings.Update(nameof(RepeatWorkSong), value, true); }
        }

        public string BreakSong
        {
            get { return _settings.Get<string>(nameof(BreakSong), null, true); }
            set { _settings.Update(nameof(BreakSong), value, true); }
        }

        public bool RepeatBreakSong
        {
            get { return _settings.Get(nameof(RepeatBreakSong), false, true); }
            set { _settings.Update(nameof(RepeatBreakSong), value, true); }
        }

        public MusicPlayerPluginSettings(ISettings settings)
        {
            _settings = settings.GetSettingsForComponent(nameof(MusicPlayerPlugin));
        }

        public void DeferChanges()
        {
            _settings.DeferChanges();
        }
    }

}