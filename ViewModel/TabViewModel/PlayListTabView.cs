using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Linq;
using test.Model;
using test.Services;

using test.ViewModel.CollectionClass;


namespace test.ViewModel.TabViewModel
{



    public class PlayListTabView : INotifyPropertyChanged
    {
        public ObservableCollection<PlayList> PlayLists { get; set; } = new ObservableCollection<PlayList>();

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



        public bool PopupIsOpen { get { return _popupIsOpen; } set { _popupIsOpen = value; OnPropertyChanged(); } } 
        public string PopupTextBox { get { return _popupTextBox; } set { _popupTextBox = value; OnPropertyChanged(); } }

        public ICommand newPlayList { get; private set; }
        public ICommand dellPlayList { get; private set; }
        public ICommand cansel { get; }
        public ICommand openPopup { get; }

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
            _audioFileNameParser = new AudioFileNameParser();

            _watcher.Created += UpdatePlayList;
            _watcher.Deleted += UpdatePlayList;


            _watcher.EnableRaisingEvents = true;

            UpdatePlayList(null, fakeEventArgs);

            dellPlayList = new RelayCommand<PlayList>(DellPLayList);
            newPlayList = new RelayCommand<object>(_ => CreatedPlayList());
            openPopup = new RelayCommand<object>(_ => PopupOpenWithtext());
            cansel = new RelayCommand<object>(_ => PopupIsOpen = false);
            
        
        }
        




        private void PopupOpenWithtext()
        {
            PopupIsOpen = true;
            string text = "Новый плейлист";
            int count = _directoryService.LenghtDirectory(_playList, text) + 1;
            PopupTextBox = text + count.ToString();
        }


        private async void UpdatePlayList(object sender, FileSystemEventArgs e)
        {
            await _dispatcher.InvokeAsync(() => { PlayLists.Clear(); });

            string[] folders = Directory.GetDirectories( _playList );

            await _dispatcher.InvokeAsync(() => { 
            
                foreach ( string folder in folders )
                {

                    string Name = Path.GetFileName(folder);

                    PlayLists.Add(new PlayList
                    {
                        Name = Name,

                        Directory = folder,

                    });
                    


                }

            
            });


        }

        private void DellPLayList(PlayList playList)
        {
            _directoryService.DelPlayList(playList.Directory);
        }



        private void CreatedPlayList()
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
