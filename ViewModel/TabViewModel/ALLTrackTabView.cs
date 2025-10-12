using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
//using test.Model;
using test.Services;
//using test.Services.ScriptService.Interfaces;
using test.ViewModel.CollectionClass;



namespace test.ViewModel.TabViewModel
{
    public class ALLTrackTabView : BaseViewModel
    {

        private FileSystemWatcher _watcher;

        private bool _popupIsOpen;

        private Track _selectedTrack;

        private PlayList _selectedPlayList;

        private Track _tempChoice;

        public Track SelectedTrack { get => _selectedTrack; set { _selectedTrack = value; OnPropertyChanged(); if (value != null) { OnItemSelected(value); } } }
        public bool PopupIsOpen {  get => _popupIsOpen;  set { _popupIsOpen = value; OnPropertyChanged(); } }
        public PlayList SelectedPlayList { get => _selectedPlayList; set { _selectedPlayList = value; OnPropertyChanged(); } }
        public ObservableCollection<Track> Tracks { get { return _trackCollectionService.Collection; } set { _trackCollectionService.Collection = value;  } }
        public ICommand DellSong { get; set; }
        public ICommand AddToPlaylist { get; set; }
        public ICommand OpenPopup { get; set; }
        public ICommand СancelPopup { get; set; }

        private readonly Dispatcher _dispatcher;
        public ALLTrackTabView(Dispatcher uiDispatcher, IAudioFileNameParser audioFileNameParser,
            IPlayListService playListService, IPathService pathService, IDirectoryService directoryService, ITrackCollectionService trackCollection) 
        {

            _directoryService = directoryService;

            _audioFileNameParser = audioFileNameParser;

            _dispatcher = uiDispatcher;

            _playlistService = playListService;

            _trackCollectionService = trackCollection;

            _getPath = pathService.ParseAll();

            var fakeEventArgs = new FileSystemEventArgs(WatcherChangeTypes.All, "InitialDirectory", "InitialFile.txt");

            _audioFileNameParser = audioFileNameParser;
            _watcher = new FileSystemWatcher(_getPath.AllSongPath);
  

            _watcher.Created += UpdateListView;
            _watcher.Deleted += UpdateListView;

            _watcher.EnableRaisingEvents = true;

            UpdateListView(null, fakeEventArgs);


            СancelPopup = new RelayCommand<object>(_ => PopupIsOpen = false);
            OpenPopup = new RelayCommand<Track>(OpenPopupHandler);
            DellSong = new RelayCommand<Track>(DellSongHandler);
            AddToPlaylist = new RelayCommand<object>(_ => AddToPlayListHandler());
        }

        private void OpenPopupHandler(Track track)
        {
            _tempChoice = track;
            PopupIsOpen = true;
        }

        private void AddToPlayListHandler()
        {
            Debug.WriteLine("AddToPlayListHandler запустилась");
            if(SelectedPlayList != null)
            {
                _playlistService.AddTrackToPlayList(SelectedPlayList, _tempChoice);
            }
            PopupIsOpen = false;
        }

        async void DellSongHandler(Track track)
        {

            Debug.WriteLine("Функция Удаления запустилась");

            await _directoryService.DellFile(Path.Combine(_getPath.AllImgPath, $"{track.FileName}.jpg") );

            await _directoryService.DellFile(Path.Combine(_getPath.AllSongPath, $"{track.FileName}.mp3"));
        }

        private void OnItemSelected(Track selectedItem)
        {
            Debug.WriteLine("Выделение туда сюда ");
            if (selectedItem != null)
            {
                Debug.WriteLine(selectedItem.Name);

            }
        }

        public async void UpdateListView(object sender, FileSystemEventArgs e)
        {
            await _dispatcher.InvokeAsync(() => { _trackCollectionService.Collection.Clear(); });
            await _dispatcher.InvokeAsync(() => { _trackCollectionService.GetTracks(_getPath.AllImgPath, _audioFileNameParser); }); 

        }

    }

}

