using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using test.ViewModel.CollectionClass;
using test.ViewModel;

namespace test.Services
{
    public class TrackCollectionService: BaseCollectionService<Track>, INotifyPropertyChanged
    {

        private PlayList _playList;
        private ObservableCollection<Track> _collection;
        public PlayList playList { get => _playList; set { _playList = value; Debug.WriteLine($"{value.Name} в свойстве присвоение значения"); OnPropertyChanged(); } }
        public new ObservableCollection<Track> Collection { get => _collection; set { _collection = value; OnPropertyChanged(); } }

        public IEnumerable<Track> GetTracks(string path, IAudioFileNameParser audioFileNameParser)
        {
            if (!Directory.Exists(path))
                return Enumerable.Empty<Track>();

            string[] imgFiles = Directory.GetFiles(path, "*.jpg");
            if (imgFiles.Length == 0)
                return Enumerable.Empty<Track>();

            var collection = new ObservableCollection<Track>(); 

            foreach (var imgFile in imgFiles)
            {
                try
                {
                    FileNameInfo fileInfo = audioFileNameParser.ParseAll(imgFile);
                    if (fileInfo == null) continue;

                    byte[] imageData = File.ReadAllBytes(imgFile);

                    collection.Add(new Track
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
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка обработки файла {imgFile}: {ex.Message}");
                    continue;
                }
            }
            Collection = collection;
            return collection;
           
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}