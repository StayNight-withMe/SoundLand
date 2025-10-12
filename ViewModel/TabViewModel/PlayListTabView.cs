using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using test.Services;
using ButtonState = test.ViewModel.enamS.ButtonState;
using test.ViewModel.CollectionClass;


namespace test.ViewModel.TabViewModel
{



    public class PlayListTabView : BaseViewModel
    {

        private FileSystemWatcher _watcher;

        private Dispatcher _dispatcher;

        private readonly ITrackCollectionService _collectionService;

        private readonly IPlayListService _playlistService;

        private string _basePath;

        private string _tempImgPath;

        private string _tempSongPath;

        private string _allImgDir;

        private string _playList;

        private bool _popupIsOpen;

        private string _popupTextBox;

        private string _buttonText;

        public PlayList _tempChoice;

        private PlayList _selectedPlayList;

        private Visibility _visiblePlayListView;

        private Visibility _containerVisible;

        private ButtonState _buttonState;

        private readonly IDirectoryService _directoryService;

        private readonly IAudioFileNameParser _audioFileNameParser;

        public ButtonState ButtonStates { get => _buttonState; set { _buttonState = value; OnPropertyChanged(); OnPropertyChanged(nameof(ButtonText)); } }
        public bool PopupIsOpen { get => _popupIsOpen; set { _popupIsOpen = value; OnPropertyChanged(); } }
        public string PopupTextBox { get => _popupTextBox; set { _popupTextBox = value; OnPropertyChanged(); } }
        public PlayList SelectedPlayList { get => _selectedPlayList; set { _selectedPlayList = value; OnPropertyChanged(); if (value != null) _tempChoice = value; } }
        public Visibility VisiblePlayListView { get => _visiblePlayListView; set { _visiblePlayListView = value; OnPropertyChanged(); } }
        public Visibility ContainerVisible { get => _containerVisible; set { _containerVisible = value; OnPropertyChanged(); } }
        public string ButtonText => ButtonStates switch
        {
            ButtonState.Back => "Назад",
            ButtonState.CreatePlayList => "Создать плейлист",
        };

        public ICommand NewPlayList { get; set; }
        public ICommand DellPlayList { get; set; }
        public ICommand Cansel { get; }
        public ICommand OpenPopup { get; }
        public ICommand PlayListChoice { get; set; }

        public InitCollection Collections { get; set; }
        public PlayListTabView(Dispatcher uiDispatcher, IAudioFileNameParser audioFileNameParser,
            IPlayListService playListService, IPathService pathService, IDirectoryService directoryService, ITrackCollectionService collectionService)

        {
            _dispatcher = uiDispatcher;

            _playlistService = playListService;

            _collectionService = collectionService;

            _audioFileNameParser = audioFileNameParser;

            _directoryService = directoryService;

            GetPath getPath = pathService.ParseAll();

            var fakeEventArgs = new FileSystemEventArgs(WatcherChangeTypes.All, "InitialDirectory", "InitialFile.txt");

            _tempSongPath = getPath.TempSongPath;
            _tempImgPath = getPath.TempImgPath;
            _allImgDir = getPath.AllImgPath;
            _playList = getPath.PlayListPath;

            _watcher = new FileSystemWatcher(_playList);
            Collections = new InitCollection();

            _watcher.Created += UpdatePlayList;
            _watcher.Deleted += UpdatePlayList;

            _watcher.EnableRaisingEvents = true;

            UpdatePlayList(null, fakeEventArgs);

            DellPlayList = new RelayCommand<PlayList>(DellPLayListHandler);
            NewPlayList = new RelayCommand<object>(_ => CreatedPlayListHandler());
            OpenPopup = new RelayCommand<object>(_ => OpenPopupHandler());
            Cansel = new RelayCommand<object>(_ => PopupIsOpen = false);
            ButtonStates = ButtonState.CreatePlayList;
            PlayListChoice = new RelayCommand<object>(_ => PlayListChoiceHandler());
            ContainerVisible = Visibility.Collapsed;


        }


        private void PlayListChoiceHandler()
        {
            VisiblePlayListView = Visibility.Collapsed;
            ContainerVisible = Visibility.Visible;
            Debug.WriteLine("Двойно нажатие на плейлист");
            Debug.WriteLine(_tempChoice.Name);
            ButtonStates = ButtonState.Back;
            _collectionService.playList = _tempChoice;

            _dispatcher.InvokeAsync(() => {
                string[] imgFiles = Directory.GetFiles(_tempChoice.Directory, "*.jpg");
                _collectionService.Collection.Clear();


                Debug.WriteLine("Заполнение колекции треков из плейлиста");

                foreach (var imgFile in imgFiles)
                {
                    string fullImgPath = Path.GetFullPath(imgFile);

                    FileNameInfo fileInfo = _audioFileNameParser.ParseAll(fullImgPath);

                    Debug.WriteLine($"Song: {fileInfo.SongName}, Artist: {fileInfo.SongArtist}, File: {fileInfo.FileName}, Duration: {fileInfo.SongDuration}");

                    string imgPath = Path.GetFullPath(imgFile);
                    byte[] imageData = File.ReadAllBytes(imgPath);



                    _collectionService.Collection.Add(new Track
                    {
                        Name = fileInfo.SongName,
                        Artist = fileInfo.SongArtist,
                        FileName = Path.GetFileNameWithoutExtension(fileInfo.FileName),
                        Duration = fileInfo.SongDuration,
                        ImageData = imageData,
                        ImgFilePath = fileInfo.ImgFilePath,
                        SongFilePath = fileInfo.SongFilePath,
                    });


                }



            });

        }


        private void OpenPopupHandler()
        {
            if (ButtonStates == ButtonState.Back)
            {
                VisiblePlayListView = Visibility.Visible;
                ContainerVisible = Visibility.Collapsed;
                ButtonStates = ButtonState.CreatePlayList;
                _collectionService.Collection.Clear();
            }
            else
            {
                Debug.WriteLine("Окрытие попута");
                PopupIsOpen = true;
                Debug.WriteLine(PopupIsOpen);
                string text = "Новый плейлист";
                int count = _directoryService.LenghtDirectory(_playList, text) + 1;
                PopupTextBox = text + count.ToString();
            }

        }


        private async void UpdatePlayList(object sender, FileSystemEventArgs e)
        {
            await _dispatcher.InvokeAsync(() => { Collections.PlayLists.Clear(); });

            string[] folders = Directory.GetDirectories(_playList);

            await _dispatcher.InvokeAsync(() => {

                foreach (string folder in folders)
                {
                    string Name = Path.GetFileName(folder);

                    Collections.PlayLists.Add(new PlayList
                    {
                        Name = Name,
                        Directory = folder,

                    });

                }
            });

        }

        private void DellPLayListHandler(PlayList playList)
        {
            _playlistService.DelPlayList(playList.Directory);
        }



        private void CreatedPlayListHandler()
        {
            _playlistService.CreatePlayList(_popupTextBox);
            PopupIsOpen = false;
        }







    }
}