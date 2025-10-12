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
    public class TrackCollectionService: ITrackCollectionService, INotifyPropertyChanged
    {

        private PlayList _playList;
        private ObservableCollection<Track> _collection;
        public PlayList playList { get => _playList; set { _playList = value; Debug.WriteLine($"{value.Name} в свойстве присвоение значения"); 
                OnPropertyChanged(); } }
        public ObservableCollection<Track> Collection { get { return _collection; Debug.WriteLine("Колекция внутри сервиса изменена"); } set { _collection = value;
                OnPropertyChanged(); Debug.WriteLine("Колекция внутри сервиса изменена");
            } }


        public TrackCollectionService()
        {
            _collection = new ObservableCollection<Track>();
        }
        public ObservableCollection<Track> GetTracks(string path, IAudioFileNameParser audioFileNameParser)
        {


            string[] imgFiles = Directory.GetFiles(path, "*.jpg");
           

            Collection.Clear();

            foreach (var imgFile in imgFiles)
            {
                try
                {
                    FileNameInfo fileInfo = audioFileNameParser.ParseAll(imgFile);
                    if (fileInfo == null) continue;

                    byte[] imageData = File.ReadAllBytes(imgFile);

                    Collection.Add(new Track
                    {
                        Name = fileInfo.SongName,
                        Artist = fileInfo.SongArtist,
                        FileName = Path.GetFileNameWithoutExtension(fileInfo.FileName),
                        ImageData = imageData,
                        Duration = fileInfo.SongDuration,
                        ImgFilePath = fileInfo.ImgFilePath,
                        SongFilePath = fileInfo.SongFilePath,
                    });
                }
                //я хз пока что и мне лень слои catch добавить, но нужно
                catch (Exception ex)
                {
                    Debug.WriteLine($"Ошибка обработки файла {imgFile}: {ex.Message}");
                    continue;
                }
            }
         
            return Collection;
           
        }

        public event PropertyChangedEventHandler PropertyChanged;

       
        

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}