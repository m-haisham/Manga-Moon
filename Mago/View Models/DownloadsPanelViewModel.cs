using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;
using System.Windows;

namespace Mago
{
    public class DownloadsPanelViewModel : BaseViewModel
    {
        #region Private Members

        private ObservableCollection<DownloadProgressViewModel> _DownloadsPanel;
        private int _animationIndex = 0;
        private PackIconKind _playPackIcon = PackIconKind.Play;
        private bool _allItemsSelected;
        private Visibility _visible = Visibility.Collapsed;

        #endregion

        #region Public Commands

        public ICommand ClearCommand { get; set; }
        public ICommand CleanCommand { get; set; }
        public ICommand TogglePlayCommand { get; set; }

        public ICommand UpQueueCommand { get; set; }
        public ICommand DownQueueCommand { get; set; }

        public ICommand DownloadsPanelAnimation { get; set; }

        #endregion

        #region Constructor
        public DownloadsPanelViewModel()
        {
            _DownloadsPanel = Setup();
            ClearCommand = new RelayCommand(ClearSelected);
            CleanCommand = new RelayCommand(Clean);
            TogglePlayCommand = new RelayCommand(TogglePlay);


            DownloadsPanelAnimation = new RelayCommand(() => IncrementDownloadsPanelIndex());
            
        }
        #endregion

        #region Private Methods
        private void ClearSelected()
        {
            List<DownloadProgressViewModel> Markers = GetSelected();

            foreach (var marker in Markers)
                _DownloadsPanel.Remove(marker);
        }

        private void Clean()
        {
            List<DownloadProgressViewModel> Markers = new List<DownloadProgressViewModel>();
            for (int i = 0; i < _DownloadsPanel.Count; i++)
            {
                if (_DownloadsPanel[i].DownloadState == DownloadState.Completed)
                    Markers.Add(_DownloadsPanel[i]);
            }

            foreach (var marker in Markers)
                _DownloadsPanel.Remove(marker);
        }

        private void TogglePlay()
        {
            if (PlayPackIcon == PackIconKind.Pause)
                PlayPackIcon = PackIconKind.Play;
            else if (PlayPackIcon == PackIconKind.Play)
                PlayPackIcon = PackIconKind.Pause;
        }

        private void QueueUp()
        {
            List<int> Markers = GetSelectedIndex();
            for (int i = 0; i < Markers.Count; i++)
            {
                if (_DownloadsPanel[i].DownloadState != DownloadState.InProgress)
                    Swap(_DownloadsPanel[Markers[i]], _DownloadsPanel[Markers[i] + 1]);
            }
        }

        private void QueueDown()
        {
            List<int> Markers = GetSelectedIndex();
            for (int i = Markers.Count; i >= 0; i--)
            {
                if (_DownloadsPanel[i].DownloadState != DownloadState.InProgress || i == _DownloadsPanel.Count - 1)
                    Swap(_DownloadsPanel[Markers[i]], _DownloadsPanel[Markers[i] + 1]);
            }
        }
        
        private async Task IncrementDownloadsPanelIndex()
        {
            AnimationIndex = AnimationIndex == 0 ? 1 : 0;
            switch (AnimationIndex)
            {
                case 0:
                    await Task.Delay(1000);
                    Visible = Visibility.Collapsed;
                    return;

                case 1:
                    Visible = Visibility.Visible;
                    return;
            }
        }

        #endregion

        #region Helper Methods

        private List<DownloadProgressViewModel> GetSelected()
        {
            List<DownloadProgressViewModel> Selected = new List<DownloadProgressViewModel>();
            for (int i = 0; i < _DownloadsPanel.Count; i++)
            {
                if (_DownloadsPanel[i].IsSelected)
                    Selected.Add(_DownloadsPanel[i]);
            }

            return Selected;
        }

        private List<int> GetSelectedIndex()
        {
            List<int> Selected = new List<int>();
            for (int i = 0; i < _DownloadsPanel.Count; i++)
            {
                if (_DownloadsPanel[i].IsSelected)
                    Selected.Add(i);
            }

            return Selected;
        }

        private void Swap<T>(T element1,T element2)
        {
            var backup = element1;
            element1 = element2;
            element2 = backup;
        }

        #endregion
        
        private ObservableCollection<DownloadProgressViewModel> Setup()
        {
            var obj = new ObservableCollection<DownloadProgressViewModel>();
            for (int i = 0; i < 10; i++)
            {
                obj.Add(
                    new DownloadProgressViewModel
                    {
                        Header = "Name " + i.ToString(),
                        Description = "desciption",
                        Maximum = 10,
                        DownloadState = DownloadState.Queued
                    }
                );
            }

            return obj;
        }

        private void AddDownload(string _header, string _description, int Maximum)
        {
            _DownloadsPanel.Add(
                new DownloadProgressViewModel
                {
                    Header = _header,
                    Description = _description,
                    Progress = 0,
                    Maximum = Maximum,
                    DownloadState = DownloadState.Queued
                });

            if(_DownloadsPanel.Count == 1 && _playPackIcon == PackIconKind.Pause)
            {
                //StartDownload
            }
        }

        public ObservableCollection<DownloadProgressViewModel> DownloadsPanel => _DownloadsPanel;

        public bool AllItemsSelected
        {
            get { return _allItemsSelected; }
            set
            {
                if (_allItemsSelected == value) return;

                _allItemsSelected = value;

                foreach (var panel in _DownloadsPanel)
                {
                    panel.IsSelected = value;
                }
            }
        }

        public Visibility Visible
        {
            get { return _visible; }
            set
            {
                if (_visible == value) return;
                _visible = value;
            }
        }
        
        public int AnimationIndex
        {
            get { return _animationIndex; }
            set
            {
                if (_animationIndex == value) return;
                _animationIndex = value;
            }
        }

        public PackIconKind PlayPackIcon
        {
            get { return _playPackIcon; }
            set
            {
                if (_playPackIcon == value) return;
                _playPackIcon = value;
            }
        }
        
    }
}
