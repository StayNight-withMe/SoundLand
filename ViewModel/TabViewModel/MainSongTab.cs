using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Threading;
using test.Model;
using test.Services;

using test.ViewModel.CollectionClass;


namespace test.ViewModel.TabViewModel
{
    public class MainSongTabView 
    {
        public ObservableCollection<Track> Tracks { get; set; } = new ObservableCollection<Track>();


        private readonly IPythonScriptService _pythonScriptService;

        private readonly IAudioFileNameParser _audioFileNameParser;

        private readonly IDirectoryService _directoryService;

        private readonly IPathService _pathService;


        private string? _inputText;
        private Track? _selectedTrack; 

        public ICommand ButtonSearchClick { get; set; }
        public ICommand SelectionChanged { get; set; }
        public ICommand ToALLTrack { get; set; }
        

        public string InputText
        {
            get => _inputText;
            set { _inputText = value; OnPropertyChanged(nameof(InputText)); }
        }

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


        public MainSongTabView(IPythonScriptService pythonScriptService, IAudioFileNameParser audioFileNameParser,
            IDirectoryService directoryService, IPathService pathService)
        {
            _pathService = pathService;

            _directoryService = directoryService;

            _audioFileNameParser = audioFileNameParser;

            _pythonScriptService = pythonScriptService;

         

            ToALLTrack = new RelayCommand<Track>(SaveTrack);
            ButtonSearchClick = new RelayCommand<object>(_ => ViewSearch());
        }



        private void OnItemSelected(Track selectedItem)
        {
            Debug.WriteLine("Получение элемента");
            if (selectedItem != null)
            {

                var index = Tracks.IndexOf(selectedItem);
                var allItems = Tracks.ToList();

                Debug.WriteLine(selectedItem.Name);

            }
        }








        public async Task ViewSearch()
        {

            GetPath getPath = _pathService.ParseAll();

            await _directoryService.ClearDirectory(getPath.TempImgPath);
            await _directoryService.ClearDirectory(getPath.TempSongPath);

        
            await _pythonScriptService.PythonScript("Untitled-3.py", 2, _inputText, "emp", "emp");


            string[] imgFiles = Directory.GetFiles(getPath.TempImgPath, "*.jpg");
            Tracks.Clear();

            var parser = new AudioFileNameParser();


            foreach (var imgFile in imgFiles)
            {
                string fullImgPath = Path.GetFullPath(imgFile);

                FileNameInfo fileInfo = _audioFileNameParser.ParseAll(fullImgPath);

                Debug.WriteLine($"Song: {fileInfo.SongName}, Artist: {fileInfo.SongArtist}, File: {fileInfo.FileName}, Duration: {fileInfo.SongDuration}");

                string imgPath = Path.GetFullPath(imgFile);
                byte[] imageData = File.ReadAllBytes(imgPath);

               

                Tracks.Add(new Track
                {
                    Name = fileInfo.SongName,
                    Artist = fileInfo.SongArtist,
                    FileName = Path.GetFileNameWithoutExtension(fileInfo.FileName),
                    Duration = fileInfo.SongDuration,
                    ImageData = imageData,
                    ImgFilePath = fileInfo.ImgFilePath,

                });


            }

        }



        public async void SaveTrack(Track track)
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



        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string name = "")
        {
            if (PropertyChanged != null) { PropertyChanged(this, new PropertyChangedEventArgs(name)); }
        }


    }
}


