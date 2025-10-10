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


        private string _basePath;

        private string _tempImgPath;

        private string _tempSongPath;   

        private string _allImgDir;

        private string _allSongDir;

        private bool _popupIsOpen;

        private Track _selectedTrack;

        private PlayList _selectedPlayList;

        private Track _tempChoice;
        //public string inputText; 
        //public string InputText
        //{
        //    get => inputText;
        //    set { inputText = value; OnPropertyChanged(nameof(InputText)); }

        //}



        public Track SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                _selectedTrack = value;
                OnPropertyChanged();

                if (value != null)
                {
                    OnItemSelected(value);
                }
            }
        }

        public bool PopupIsOpen {  get => _popupIsOpen;  set { _popupIsOpen = value; OnPropertyChanged(); } }
        public PlayList SelectedPlayList { get => _selectedPlayList; set { _selectedPlayList = value; OnPropertyChanged(); } }



        public ICommand DellSong { get; set; }
        public ICommand AddToPlaylist { get; set; }
        public ICommand OpenPopup { get; set; }
        public ICommand СancelPopup { get; set; }

        public InitCollection Collections { get; set; }


        private readonly Dispatcher _dispatcher;
        public ALLTrackTabView(Dispatcher uiDispatcher, IAudioFileNameParser audioFileNameParser,
            IPlayListService playListService, IPathService pathService, IDirectoryService directoryService) 
            : base(audioFileNameParser,
             playListService, pathService, directoryService)
        {
            _dispatcher = uiDispatcher;

            GetPath getPath = pathService.ParseAll();

            var fakeEventArgs = new FileSystemEventArgs(WatcherChangeTypes.All, "InitialDirectory", "InitialFile.txt");

            _tempSongPath = getPath.TempSongPath;
            _tempImgPath = getPath.TempImgPath;
            _allImgDir = getPath.AllImgPath;
            _allSongDir = getPath.AllSongPath;

            Collections = new InitCollection();
            _watcher = new FileSystemWatcher(_allSongDir);
  

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

           
            await _directoryService.DellFile(Path.Combine(_allImgDir, $"{track.FileName}.jpg") );

            await _directoryService.DellFile(Path.Combine(_allSongDir, $"{track.FileName}.mp3"));
        }


        private void OnItemSelected(Track selectedItem)
        {
            Debug.WriteLine("Выделение туда сюда ");
            if (selectedItem != null)
            {

                //var index = ALLTracks.IndexOf(selectedItem);
                //var allItems = ALLTracks.ToList();

                Debug.WriteLine(selectedItem.Name);

            }
        }



        public async void UpdateListView(object sender, FileSystemEventArgs e)
        {

            string[] imgFiles = Directory.GetFiles(_allImgDir, "*.jpg");

            await _dispatcher.InvokeAsync(() => { Collections.ALLTracks.Clear(); });

            await _dispatcher.InvokeAsync(() => {

            foreach (var imgFile in imgFiles)
            {

                string fullImgPath = Path.GetFullPath(imgFile);

                FileNameInfo fileInfo = _audioFileNameParser.ParseAll(fullImgPath);

                Debug.WriteLine($"Song: {fileInfo.SongName}, Artist: {fileInfo.SongArtist}, File: {fileInfo.FileName}, Duration: {fileInfo.SongDuration}");

                string imgPath = Path.GetFullPath(imgFile);
                byte[] imageData = File.ReadAllBytes(imgPath);

                    Collections.ALLTracks.Add(new Track
                    {
                        Name = fileInfo.SongName,
                        Artist = fileInfo.SongArtist,
                        FileName = Path.GetFileNameWithoutExtension(fileInfo.FileName),
                        Duration = fileInfo.SongDuration,
                        ImageData = imageData

                    });

              }

         });


        }

        

      


    }

}

