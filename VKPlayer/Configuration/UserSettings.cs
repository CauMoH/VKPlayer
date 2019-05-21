using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using VKPlayer.AppCommon;
using VKPlayer.Common;
using VKPlayer.Configuration.XmlCommon;
using VKPlayer.Enums;
using VKPlayer.EventArgs;

namespace VKPlayer.Configuration
{
    public sealed class UserSettings
    {
        private readonly XmlConfig _xmlConfig;

        #region Props

        public WindowDimensions MainWindowSettings { get; set; }
        public string UserName { get; set; }

        private readonly Password _password = new Password();
        public SecureString Password
        {
            get => _password.GetSecure();
            set => _password.SetSecure(value);
        }
        public string AccessToken { get; set; }
        public uint UserId { get; set; }
        public string DownloadFolder { get; set; }
        public AudioGenreExt PopularAudioGenre { get; set; }
        public bool PopularOnlyEng { get; set; }
        public bool RecommendationsIsShuffle { get; set; }

        #endregion

        public UserSettings()
        {
            _snapshot = new Snapshot<UserSettings>(this);

            Reset();

            _xmlConfig = XmlConfigBuilder.Root("App")
                .Setting(nameof(UserName), () => UserName)
                .Setting(nameof(Password), _password.GetProtected, _password.SetProtected)
                .Block("General", bi => bi
                .Setting(nameof(AccessToken), () => AccessToken)
                .Setting(nameof(UserId), () => UserId)
                .Setting(nameof(AudioGenreExt), () => PopularAudioGenre)
                .Setting(nameof(PopularOnlyEng), () => PopularOnlyEng)
                .Setting(nameof(RecommendationsIsShuffle), () => RecommendationsIsShuffle)
                .Setting(nameof(DownloadFolder), () => DownloadFolder))
                .Block("VKPlayer_MainWindow", bi => bi
                    .Setting(nameof(MainWindowSettings.Top), () => MainWindowSettings.Top)
                    .Setting(nameof(MainWindowSettings.Left), () => MainWindowSettings.Left)
                    .Setting(nameof(MainWindowSettings.Width), () => MainWindowSettings.Width)
                    .Setting(nameof(MainWindowSettings.Height), () => MainWindowSettings.Height)
                    .Setting(nameof(MainWindowSettings.IsMaximized), () => MainWindowSettings.IsMaximized))
                .Build();
        }

        #region Change tracking

        private readonly Snapshot<UserSettings> _snapshot;

        private IEnumerable<ChangeProp> GetChangedProps()
        {
            return _snapshot.GetChangedPropNames();
        }

        private void FixPropChanges()
        {
            _snapshot.Save();
        }

        #endregion

        public void Load()
        {
            Reset();

            try
            {
                var settingsPath = PathHelper.ConfigFilePath;
                if (File.Exists(settingsPath))
                {
                    _xmlConfig.Load(settingsPath);
                }

                FixPropChanges();
            }
            catch (Exception e)
            {
                Reset();
            }
        }

        public void Save(bool isSilent = true)
        {
            _xmlConfig.Save(PathHelper.ConfigFilePath);

            var names = GetChangedProps();
            FixPropChanges();

            if (!isSilent)
            {
                RaiseSaved(names);
            }
        }

        private void Reset()
        {
            UserName = string.Empty;
            _password.Reset();
            AccessToken = string.Empty;
            UserId = 0;
            PopularAudioGenre = AudioGenreExt.All;
            PopularOnlyEng = false;
            RecommendationsIsShuffle = false;
            DownloadFolder = string.Empty;

            MainWindowSettings = new WindowDimensions();

            FixPropChanges();
        }

        #region Saved

        public event EventHandler<SettingsSavedEventArgs> Saved;

        private void RaiseSaved(IEnumerable<ChangeProp> changedPropNames)
        {
            Saved?.Invoke(this, new SettingsSavedEventArgs(changedPropNames));
        }

        #endregion
    }
}
