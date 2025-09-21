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
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;

using test.Services;

using test.ViewModel.CollectionClass;


namespace test.ViewModel.TabViewModel
{



    public class PlayListTabView : INotifyPropertyChanged
    {

        private FileSystemWatcher _watcher;

        private Dispatcher _dispatcher;


        private readonly IPythonScriptService _pythonScriptService;

        private readonly IAudioFileNameParser _audioFileNameParser;

        private readonly IPlayListService _directoryService;

        private readonly IPathService _pathService;


        private string _basePath;

        private string _tempImgPath;

        private string _tempSongPath;

        private string _allImgDir;

        private string _playList;


        private bool _popupIsOpen;
        private string _popupTextBox;
        private PlayList _selectedPlayList;

        public bool PopupIsOpen { get { return _popupIsOpen; } set { _popupIsOpen = value; OnPropertyChanged(); } }
        public string PopupTextBox { get { return _popupTextBox; } set { _popupTextBox = value; OnPropertyChanged(); } }
        public PlayList SelectedPlayList { get => _selectedPlayList; set { _selectedPlayList = value; OnPropertyChanged(); } }

        public ICommand NewPlayList { get; private set; }
        public ICommand DellPlayList { get; private set; }
        public ICommand Cansel { get; }
        public ICommand OpenPopup { get; }

       
        public InitCollection Collections { get; set; }
        public PlayListTabView(Dispatcher uiDispatcher, IAudioFileNameParser audioFileNameParser,
            IPlayListService playListService, IPathService pathService )
        {
            _dispatcher = uiDispatcher;

            _pathService = pathService;

            _directoryService = playListService;

            _audioFileNameParser = audioFileNameParser;

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
            
        
        }
        
        private void OpenPopupHandler()
        {
            PopupIsOpen = true;
            string text = "Новый плейлист";
            int count = _directoryService.LenghtDirectory(_playList, text) + 1;
            PopupTextBox = text + count.ToString();
        }


        private async void UpdatePlayList(object sender, FileSystemEventArgs e)
        {
            await _dispatcher.InvokeAsync(() => { Collections.PlayLists.Clear(); });

            string[] folders = Directory.GetDirectories( _playList );

            await _dispatcher.InvokeAsync(() => { 
            
                foreach ( string folder in folders )
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
            _directoryService.DelPlayList(playList.Directory);
        }



        private void CreatedPlayListHandler()
        {
            _directoryService.CreatePlayList(_popupTextBox);
            PopupIsOpen = false;
        }





        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        }

    }
}
