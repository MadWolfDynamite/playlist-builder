using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.WindowsAPICodePack.Dialogs;
using PlaylistBuilder.Client.Api;
using PlaylistBuilder.Client.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Tools.AsyncCommandApi;

namespace PlaylistBuilder.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _windowTitle, _viewModelStatus, _loadedFile;

        public string Title 
        { 
            get { return _windowTitle; } 
            set 
            { 
                if (_windowTitle != value) 
                {
                    _windowTitle = value;
                    RaisePropertyChanged("Title");
                } 
            }
        }
        public ObservableCollection<TrackModel> TrackList { get; set; }

        public string ViewModelStatus
        {
            get { return _viewModelStatus; }
            private set
            {
                if (_viewModelStatus != value)
                {
                    _viewModelStatus = value;
                    RaisePropertyChanged("ViewModelStatus");
                }
            }
        }
        public string LoadedPlaylistFile
        {
            get { return _loadedFile; }
            private set
            {
                if (_loadedFile != value)
                {
                    _loadedFile = value;
                    RaisePropertyChanged("LoadedPlaylistFile");
                }
            }
        }

        public ICommand LoadCommand { get; internal set; }
        public ICommand SaveCommand { get; internal set; }

        public ICommand OpenDirectoryCommand { get; internal set; }

        public MainWindowViewModel()
        {
            TrackList = new ObservableCollection<TrackModel>();

            LoadCommand = new AsyncCommand<string>(async() => LoadedPlaylistFile = await LoadPlaylistDataAsync());
            SaveCommand = new AsyncCommand<string>(async() => LoadedPlaylistFile = await SavePlaylistDataAsync());

            OpenDirectoryCommand = new RelayCommand<string>(param => DirectoryLinkClick(param));

            if (IsInDesignMode)
                Title = "SonicWolf Playlist Builder (Design Mode)";
            else
                Title = "SonicWolf Playlist Builder";
        }

        public async Task<string> LoadPlaylistDataAsync()
        {
            var fileDialog = new CommonOpenFileDialog 
            {
                Title = "Select a playlist to load"
            };

            fileDialog.Filters.Add(new CommonFileDialogFilter("M3U Playlist Files", "*.m3u"));

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                await Task.Delay(1);
                return LoadedPlaylistFile;
            }

            TrackList.Clear();

            ViewModelStatus = "Loading Playlist...";
            LoadedPlaylistFile = "";

            var selectedFile = new FileInfo(fileDialog.FileName);

            var loadedTracks = await PlaylistFileProcessor.LoadPlaylistDataAsync(selectedFile.FullName);
            Environment.CurrentDirectory = selectedFile.DirectoryName;

            foreach (var track in loadedTracks)
            {
                var trackData = await Task.Run(() => MediaFileProcessor.LoadTrackMetadata(track));

                if (trackData != null)
                    TrackList.Add(trackData);
            }

            ViewModelStatus = await Task.Run(() => GetPlaylistSummary());
            //LoadedPlaylistFile = selectedFile.FullName;

            return selectedFile.FullName;
        }

        public async Task<string> SavePlaylistDataAsync()
        {
            var fileDialog = new CommonSaveFileDialog
            {
                Title = "Save Playlist...",

                DefaultExtension = "m3u",
                AlwaysAppendDefaultExtension = true
            };

            fileDialog.Filters.Add(new CommonFileDialogFilter("M3U Playlist Files", "*.m3u"));

            if (fileDialog.ShowDialog() != CommonFileDialogResult.Ok)
            {
                await Task.Delay(1);
                return LoadedPlaylistFile;
            }

            ViewModelStatus = "Loading Playlist...";

            var buffer = new StringBuilder();
            var selectedFile = new FileInfo(fileDialog.FileName);

            foreach (var track in TrackList)
            {
                var filePath = Path.GetRelativePath(selectedFile.DirectoryName, track.ResolvedPath);
                buffer.AppendLine(filePath);
            }

            await PlaylistFileProcessor.SavePlaylistDataAsync(selectedFile.FullName, buffer.ToString());
            ViewModelStatus = await Task.Run(() => GetPlaylistSummary());

            return selectedFile.FullName;
        }

        private string GetPlaylistSummary()
        {
            string result = "";

            double totalLength = 0;
            double trackSeconds, trackMinutes, trackHours;

            Parallel.ForEach(TrackList, track => {
                totalLength += track.Length;
            });

            trackSeconds = totalLength / 1000;
            trackMinutes = Math.Floor(trackSeconds / 60);
            trackHours = Math.Floor(trackMinutes / 60);

            string formattedTime = trackHours > 0 ? $"{trackHours}:{trackMinutes % 60:00}:{trackSeconds % 60:00.000}" : $"{trackMinutes}:{trackSeconds % 60:00.000}";
            result = $"{TrackList.Count} Tracks / {formattedTime}";

            return result;
        }

        private void DirectoryLinkClick(object param)
        {
            var link = param as string;

            var startInfo = new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            };

            Process.Start(startInfo);
        }
    }

    public class TrackLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double @double)
            {
                double trackSeconds, trackMinutes, trackHours;

                trackSeconds = @double / 1000;
                trackMinutes = Math.Floor(trackSeconds / 60);
                trackHours = Math.Floor(trackMinutes / 60);

                return trackHours > 0 ? $"{trackHours}:{(trackMinutes % 60).ToString("00")}:{trackSeconds % 60:00.000}" : $"{trackMinutes}:{trackSeconds % 60:00.000}";
            }

            return "N/A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                double trackSeconds, trackMinutes, trackHours;

                var timeParts = value.ToString().Split(':');
                switch (timeParts.Length)
                {
                    case 2:
                        trackHours = 0;
                        trackMinutes = double.Parse(timeParts[0]);
                        trackSeconds = double.Parse(timeParts[1]);
                        break;
                    case 3:
                        trackHours = double.Parse(timeParts[0]);
                        trackMinutes = double.Parse(timeParts[1]);
                        trackSeconds = double.Parse(timeParts[2]);
                        break;
                    default:
                        trackHours = 0;
                        trackMinutes = 0;
                        trackSeconds = 0;
                        break;
                }

                return ((trackHours * 3600) + (trackMinutes * 60) + trackSeconds) * 1000;
            }

            return 0;
        }
    }

    public class GenreListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is IList<string>)
            {
                var genres = (List<string>)value;
                StringBuilder buffer = new StringBuilder();

                foreach (var genre in genres)
                    buffer.Append($"{genre} / ");

                return buffer.Length - 3 > 0 ? buffer.ToString().Remove(buffer.Length - 3) : buffer.ToString();
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string)
            {
                List<string> genres = new List<string>();

                foreach (var genre in value.ToString().Split('/'))
                    genres.Add(genre.Trim());

                return genres;
            }

            return new List<string>();
        }
    }
}
