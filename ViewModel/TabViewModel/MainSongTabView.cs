using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

using System.Windows.Input;
using System.Windows.Threading;

using test.Services;

using test.ViewModel.CollectionClass;


namespace test.ViewModel.TabViewModel
{
    public class MainSongTabView : BaseViewModel
    {
        
        private readonly IPythonScriptService _pythonScriptService;

        private readonly IAudioFileNameParser _audioFileNameParser;

        private readonly IDirectoryService _directoryService;

        private readonly IPlayListService _playListService;

        private readonly IPathService _pathService;

        

        private readonly GetPath _getPath;

        private bool _popupIsOpen;
        public bool PopupIsOpen { get => _popupIsOpen; set { _popupIsOpen = value; OnPropertyChanged(); } }


        private string? _inputText;
        public string InputText {  get => _inputText; set { _inputText = value; OnPropertyChanged(); }  }

        private Track? _selectedTrack;
        public Track SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                _selectedTrack = value;
                OnPropertyChanged(nameof(SelectedTrack));

                if (value != null)
                {
                    OnTrackSelected(value);
                }
            }
        }

        private PlayList _selectedPlayList;
        public PlayList SelectedPlayList
        {
            get => _selectedPlayList;
            set { _selectedPlayList = value; OnPropertyChanged(); }
        }

        private Track _tempChoiceTrack; 


        public ICommand SearchSong { get; set; }
        public ICommand SelectionChanged { get; set; }
        public ICommand ToALLTrack { get; set; }
        public ICommand ToPlayList { get; set; }
        public ICommand AddToPlayList { get; set; }
        public ICommand СancelPopup { get; set; }

        public InitCollection Collections { get; set; }
        public MainSongTabView(IPythonScriptService pythonScriptService, IAudioFileNameParser audioFileNameParser,
            IPlayListService playListService, IPathService pathService, IDirectoryService directoryService)
        {
            _pathService = pathService;

            _playListService = playListService;

            _audioFileNameParser = audioFileNameParser;
            
            _pythonScriptService = pythonScriptService;
            
            _directoryService = directoryService;



            ToALLTrack = new RelayCommand<Track>(ToALLTrackHandler);
            SearchSong = new RelayCommand<object>(_ => SearchSongHandler());
            ToPlayList = new RelayCommand<Track>(ToPlayListHandler);
            СancelPopup = new RelayCommand<Object>(_ => PopupIsOpen = false);
            AddToPlayList = new RelayCommand<Object>(_ => AddToPlayListHandler() );
            Collections = new InitCollection();

            _getPath = _pathService.ParseAll();
        }


        private void ToPlayListHandler(Track track)
        {
            _tempChoiceTrack = track;
            PopupIsOpen = true;
        }

        private async void AddToPlayListHandler()
        {
            
            if (SelectedPlayList != null)
            {
                Debug.WriteLine(_tempChoiceTrack.Name);

                await _pythonScriptService.PythonScript("Untitled-3.py", 1, _inputText, _tempChoiceTrack.FileName, _tempChoiceTrack.Name);

                _playListService.AddTrackToPlayList(SelectedPlayList, _tempChoiceTrack);

            }
            
            PopupIsOpen = false;

        }

      
        private void OnTrackSelected(Track selectedItem)
        {
            Debug.WriteLine("Получение элемента");
            if (selectedItem != null)
            {

                var index = Collections.Tracks.IndexOf(selectedItem);
                var allItems = Collections.Tracks.ToList();

                Debug.WriteLine(selectedItem.Name);

            }
        }








        public async Task SearchSongHandler()
        {

            

            await _directoryService.ClearDirectory(_getPath.TempImgPath);
            await _directoryService.ClearDirectory(_getPath.TempSongPath);

        
            await _pythonScriptService.PythonScript("Untitled-3.py", 2, _inputText, "emp", "emp");


            string[] imgFiles = Directory.GetFiles(_getPath.TempImgPath, "*.jpg");
            Collections.Tracks.Clear();

          


            foreach (var imgFile in imgFiles)
            {
                string fullImgPath = Path.GetFullPath(imgFile);

                FileNameInfo fileInfo = _audioFileNameParser.ParseAll(fullImgPath);

                Debug.WriteLine($"Song: {fileInfo.SongName}, Artist: {fileInfo.SongArtist}, File: {fileInfo.FileName}, Duration: {fileInfo.SongDuration}");

                string imgPath = Path.GetFullPath(imgFile);
                byte[] imageData = File.ReadAllBytes(imgPath);



                Collections.Tracks.Add(new Track
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

        }



        public async void ToALLTrackHandler(Track track)
        {

            await _pythonScriptService.PythonScript("Untitled-3.py", 1, _inputText, track.FileName, track.Name);

            string sourceImagePath = Path.GetFullPath(Path.Combine("temp_img", $"{track.FileName}.jpg"));
            string sourceAudioPath = Path.GetFullPath(Path.Combine("temp_song", $"{track.FileName}.mp3"));

            string targetImgDir = Path.Combine(@"ALL\img", $"{track.FileName}.jpg");
            string targetSongDir = Path.Combine(@"ALL\song", $"{track.FileName}.mp3");

            Debug.WriteLine("SOURCEDIR", sourceImagePath, sourceAudioPath);
            Debug.WriteLine("TARGET DIR", targetSongDir, targetImgDir);

            await _directoryService.CopyFileToDerictory(sourceImagePath, targetImgDir);
            await _directoryService.CopyFileToDerictory(sourceAudioPath, targetSongDir);


        }





    }
}


