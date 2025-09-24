using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace test.ViewModel
{
    public class Track : INotifyPropertyChanged
    {

        private const int MaxLength = 15;

        private string _name;
        
        private string _artist;
        
        private string _fileName;
        
        private string _imgFilePath;
        
        private string _songFilePath;
        
        private byte[] _imageData;

        private string _duration;


        public string Name { get => _name;
             set
            {
                if (value == _name) return;


                string processed = value?.Substring(0, Math.Min(value.Length, MaxLength))
                                      .PadRight(MaxLength);

                _name = processed ?? new string(' ', MaxLength);

                OnPropertyChanged();
            }
        }
        public string Artist { get => _artist; 
            set
            {
                if (value == _artist) return;


                string processed = value?.Substring(0, Math.Min(value.Length, MaxLength))
                                      .PadRight(MaxLength);

                _artist = processed ?? new string(' ', MaxLength);

                OnPropertyChanged();
            }
        }
        public string FileName { get => _fileName; set { _fileName = value; OnPropertyChanged(); } }
        public string ImgFilePath {  get => _imgFilePath; set { _imgFilePath = value; OnPropertyChanged(); } }
        public string? SongFilePath { get => _songFilePath; set { _songFilePath = value; OnPropertyChanged(); } }
        public byte[] ImageData { get => _imageData; set { _imageData = value; OnPropertyChanged(); } } // Изображение в памяти ибо по иному хуево + нужны абсолютные пути, без них тоже поебота
        public string Duration { get => _duration; set { _duration = value; OnPropertyChanged(); } }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
