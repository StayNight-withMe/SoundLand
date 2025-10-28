using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using ButtonState = test.ViewModel.enamS.PlayPauseButtonStates;
using test.Services;

using test.ViewModel.CollectionClass;
using test.SongTabControl;


namespace test.ViewModel.TabViewModel
{
    public class MainSongTabView : BaseViewModel
    {

        private readonly IPythonScriptService _pythonScriptService;

        private readonly IPlayListService _playListService;

        private IMediaService _mediaService;

        private Track _tempChoiceTrack;

        private bool _popupIsOpen;
        public bool PopupIsOpen { get => _popupIsOpen; set { _popupIsOpen = value; OnPropertyChanged(); } }

        private string? _inputText;
        public string InputText { get => _inputText; set { _inputText = value; OnPropertyChanged(); } }

        private Track? _selectedTrack;
        public Track SelectedTrack { get => _selectedTrack; set { _selectedTrack = value; OnPropertyChanged(); if (value != null) { OnTrackSelected(value); } } }

        private List<PlayList> _selectedPlayList;

        private PlayList _playList;

        public PlayList SelectedPlayList { get => _playList; set { _selectedPlayList.Add(value); OnPropertyChanged(); OnPropertyChanged(nameof(SelectedPlayLists)); } }
        public List<PlayList> SelectedPlayLists { get => _selectedPlayList; set { _selectedPlayList = value; OnPropertyChanged(); OnPropertyChanged(nameof(SelectedPlayList)); } }
        private string? _sourceForMediaElement;
        public string SourceForMediaElement { get => _sourceForMediaElement; set { _sourceForMediaElement = value; OnPropertyChanged(); } }
        public IMediaService MediaService { get => _mediaService; set { _mediaService = value; } }

        private string? _image;
        public string Image { get => _image; set { _image = value; OnPropertyChanged(); } }

        private double _songSliderValue;
        public double SongSliderValue { get => _songSliderValue;
            set { _songSliderValue = value; OnPropertyChanged();
                if (TotalSeconds > 0)
                {
                    double seconds = (value / 100) * TotalSeconds;
                    _mediaService.Seek(seconds);
                    OnPropertyChanged(nameof(SecondsProcess));

                }
            }
        }

        public string SecondsProcess { get { TimeSpan time = TimeSpan.FromSeconds(SongSliderValue); return time.ToString(@"mm\:ss"); } }
        private ButtonState _states;
        public ButtonState State { get => _states; set { _states = value; OnPropertyChanged(); OnPropertyChanged(nameof(PlayPauseButtonText)); } }
        public string PlayPauseButtonText => State switch
        {
            ButtonState.Play => "Play",
            ButtonState.Pause => "Pause",


        };

        private double _totalSeconds;
        public double TotalSeconds
        {
            get => _totalSeconds;
            set
            {
                _totalSeconds = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SecondForView));
            }
        }

        public string SecondForView { get { TimeSpan time = TimeSpan.FromSeconds(_totalSeconds); ; return time.ToString(@"mm\:ss"); } }
        public ObservableCollection<Track> Tracks { get => _trackCollectionService.Collection; set { _trackCollectionService.Collection = value; } }
        public ICommand SearchSong { get; set; }
        public ICommand SelectionChanged { get; set; }
        public ICommand ToALLTrack { get; set; }
        public ICommand ToPlayList { get; set; }
        public ICommand AddToPlayList { get; set; }
        public ICommand СancelPopup { get; set; }
        public ICommand PlayPause { get; set; }
        public InitCollection Collections { get; set; }
        public MainSongTabView(IPythonScriptService pythonScriptService, IAudioFileNameParser audioFileNameParser,
            IPlayListService playListService, IPathService pathService, IDirectoryService directoryService, ITrackCollectionService trackCollectionService)
        {
            _playListService = playListService;

            _pythonScriptService = pythonScriptService;

            _directoryService = directoryService;

            _audioFileNameParser = audioFileNameParser;

            _trackCollectionService = trackCollectionService;

            _mediaService = new MediaService();

            _mediaService.PositionChanged += OnPositionChanged;

            _mediaService.DurationChanged += OnDurationChanged;

            ToALLTrack = new RelayCommand<Track>(ToALLTrackHandler);
            SearchSong = new RelayCommand<object>(_ => SearchSongHandler());
            ToPlayList = new RelayCommand<Track>(ToPlayListHandler);
            СancelPopup = new RelayCommand<Object>(_ => PopupIsOpen = false);
            AddToPlayList = new RelayCommand<Object>(_ => AddToPlayListHandler(), _ => SelectedPlayLists.Count > 0);
            Collections = new InitCollection();
            PlayPause = new RelayCommand<object>(_ => PlayPauseHandler());

            _getPath = pathService.ParseAll();
            State = ButtonState.Pause;

            SelectedPlayLists = new List<PlayList>();
        }

        private void OnDurationChanged(double duration)
        {
            TotalSeconds = duration;
        }


        private void PlayPauseHandler()
        {
            if (State == ButtonState.Pause)
            {
                MediaService.Stop();
                State = ButtonState.Play;

            }
            else if (State == ButtonState.Play)
            {
                MediaService.Start();
                State = ButtonState.Pause;

            }
        }


        private void OnPositionChanged(double position)
        {
            if (_mediaService.TotalSeconds > 0)
            {

                SongSliderValue = (position / _mediaService.TotalSeconds) * 100;
            }
        }

        private void ToPlayListHandler(Track track)
        {
            _tempChoiceTrack = track;
            PopupIsOpen = true;
        }

        private async void AddToPlayListHandler()
        {

            if (!Path.Exists(_tempChoiceTrack.SongFilePath))
            {
                Debug.WriteLine("Ждем скачивания");
                await _pythonScriptService.PythonScript("Untitled-3.py", 1, _inputText, _tempChoiceTrack.FileName, _tempChoiceTrack.Name);
                Debug.WriteLine("дождались");
            }

            if (SelectedPlayLists != null && SelectedPlayLists.Count != 0)
            {
                foreach (var item in SelectedPlayLists)
                {
                    Debug.WriteLine(_tempChoiceTrack.Name);

                  
                    await _playListService.AddTrackToPlayList(item, _tempChoiceTrack);
                }
              

            }
            else
            {
                MessageBox.Show("выберите хотя бы один плейлист");
            }
            
            PopupIsOpen = false;

        }

      
        private async Task OnTrackSelected(Track selectedItem)
        {
            Debug.WriteLine("Получение элемента");
            if (selectedItem != null)
            {

                var index = Collections.Tracks.IndexOf(selectedItem);
                var allItems = Collections.Tracks.ToList();
                Debug.WriteLine(selectedItem.Name);

                await _pythonScriptService.PythonScript("Untitled-3.py", 1, _inputText, selectedItem.FileName, selectedItem.Name);

                SourceForMediaElement = Path.GetFullPath(selectedItem.SongFilePath);

                Debug.WriteLine(selectedItem.SongFilePath);

                if (MediaService?.MediaElement == null)
                {
                    MessageBox.Show("медиав-серисс = 0");
                    return;
                }

                Image = Path.GetFullPath(selectedItem.ImgFilePath);
                State = ButtonState.Pause;
                MediaService.Seek(0);
                MediaService.Start();    
                
            }
        }

        public async Task SearchSongHandler()
        {
            MediaService.Stop();
            _trackCollectionService.Collection.Clear();
        
        

            SourceForMediaElement = null;
          

            if(Image != null)
            {

                try
                {
                  
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    Thread.Sleep(200);  

                    // 2. Пытаемся удалить:
                    File.Delete(Image);

                }
                catch (IOException)
                {
                   

                    try
                    {
                        File.Delete(Image);

                    }
                    catch
                    {
                        
                    }
                }
                finally
                {
                   Image = Constants.defaultImagePath;

                }


            }


            await _directoryService.ClearDirectory(_getPath.TempSongPath);
            await _directoryService.ClearDirectory(_getPath.TempImgPath);
            
      

            await _pythonScriptService.PythonScript("Untitled-3.py", 2, _inputText, "emp", "emp");

            string imgFiles = Path.Combine(_getPath.TempImgPath, "*.jpg");
           

            _trackCollectionService.GetTracks(_getPath.TempImgPath, _audioFileNameParser);
        }
        //

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


