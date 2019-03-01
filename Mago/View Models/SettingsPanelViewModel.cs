using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Mago
{
    public class SettingsPanelViewModel : BaseViewModel
    {
        private bool _lightModeEnabled;
        private bool _darkModeEnabled;

        private int _defaultZoom;
        private bool _autoSaveCurrent;
        private bool _autoDownloadNext;

        private string _mangaPath;
        private bool _autoClear;

        private bool _notiChapterLoaded;
        private bool _notiChapterDownloaded;
        private bool _notiDownloadTaskFinished;

        private bool _isApplyEnabled;
        private string _applyTooltip;

        public ICommand Browse { get; set; }
        public ICommand Reload { get; set; }
        public ICommand Reset { get; set; }
        public ICommand Apply { get; set; }

        private MainViewModel MainView;

        public SettingsPanelViewModel(MainViewModel mainView)
        {
            MainView = mainView;

            Browse = new RelayCommand(BrowserDirectory);
            Reload = new RelayCommand(ReloadSettings);
            Reset = new RelayCommand(ResetSettings);
            Apply = new RelayCommand(ApplySettings);

            ReloadSettings();
        }

        #region public methods
        
        public void ResetSettings()
        {
            MainView.Settings.Reset();
            ReloadSettings();
        }

        public void ApplySettings()
        {
            MainView.Settings.darkModeEnabled = DarkModeEnabled;

            MainView.Settings.ReaderZoomPercent = DefaultZoom;
            MainView.Settings.autoDownloadReadChapters = AutoSaveCurrent;
            MainView.Settings.autoDownloadNextChapter = AutoDownloadNext;

            MainView.Settings.mangaPath = MangaPath;
            MainView.Settings.autoDeleteCompletedDownloads = AutoClear;

            MainView.Settings.chapterReaderLoadNotifications = NotiChapterLoaded;
            MainView.Settings.chapterDownloadNotifications = NotiChapterDownloaded;
            MainView.Settings.downloadTaskNotifications = NotiDownloadTaskFinished;

            MainView.MenuViewModel.DarkThemeEnabled = DarkModeEnabled;
            SaveSystem.SaveSettings(MainView.Settings);
        }

        public void ReloadSettings()
        {
            LightModeEnabled = !MainView.Settings.darkModeEnabled;
            DarkModeEnabled = MainView.Settings.darkModeEnabled;

            DefaultZoom = MainView.Settings.ReaderZoomPercent;
            AutoSaveCurrent = MainView.Settings.autoDownloadReadChapters;
            AutoDownloadNext = MainView.Settings.autoDownloadNextChapter;

            MangaPath = Path.GetFullPath(MainView.Settings.mangaPath);
            AutoClear = MainView.Settings.autoDeleteCompletedDownloads;

            NotiChapterLoaded = MainView.Settings.chapterReaderLoadNotifications;
            NotiChapterDownloaded = MainView.Settings.chapterDownloadNotifications;
            NotiDownloadTaskFinished = MainView.Settings.downloadTaskNotifications;
        }

        public void BrowserDirectory()
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                MangaPath = dialog.FileName;
                IsApplyEnabled = true;
            }
        }

        #endregion

        public bool LightModeEnabled
        {
            get { return _lightModeEnabled; }
            set
            {
                if (_lightModeEnabled == value) return;
                _lightModeEnabled = value;
            }
        }
        public bool DarkModeEnabled
        {
            get { return _darkModeEnabled; }
            set
            {
                if (_darkModeEnabled == value) return;
                _darkModeEnabled = value;
            }
        }

        public int DefaultZoom
        {
            get { return _defaultZoom; }
            set
            {
                if (_defaultZoom == value) return;
                _defaultZoom = value;
            }
        }
        public bool AutoSaveCurrent
        {
            get { return _autoSaveCurrent; }
            set
            {
                if (_autoSaveCurrent == value) return;
                _autoSaveCurrent = value;
            }
        }
        public bool AutoDownloadNext
        {
            get { return _autoDownloadNext; }
            set
            {
                if (_autoDownloadNext == value) return;
                _autoDownloadNext = value;
            }
        }

        public string MangaPath
        {
            get { return _mangaPath; }
            set
            {
                if (_mangaPath == value) return;
                _mangaPath = value;
                IsApplyEnabled = Directory.Exists(_mangaPath);
            }
        }
        public bool AutoClear
        {
            get { return _autoClear; }
            set
            {
                if (_autoClear == value) return;
                _autoClear = value;
            }
        }

        public bool NotiChapterLoaded
        {
            get { return _notiChapterLoaded; }
            set
            {
                if (_notiChapterLoaded == value) return;
                _notiChapterLoaded = value;
            }
        }
        public bool NotiChapterDownloaded
        {
            get { return _notiChapterDownloaded; }
            set
            {
                if (_notiChapterDownloaded == value) return;
                _notiChapterDownloaded = value;
            }
        }
        public bool NotiDownloadTaskFinished
        {
            get { return _notiDownloadTaskFinished; }
            set
            {
                if (_notiDownloadTaskFinished == value) return;
                _notiDownloadTaskFinished = value;
            }
        }

        public bool IsApplyEnabled
        {
            get { return _isApplyEnabled; }
            set
            {
                if (_isApplyEnabled == value) return;
                _isApplyEnabled = value;
                ApplyTooltip = value ? "Apply current settings" : "Path not found";
            }
        }
        public string ApplyTooltip
        {
            get { return _applyTooltip; }
            set
            {
                if (_applyTooltip == value) return;
                _applyTooltip = value;
            }
        }
    }
}
