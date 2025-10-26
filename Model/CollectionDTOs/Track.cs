using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace test.ViewModel
{
    public class Track 
    {

        private string _name;
        
        private string _artist;
        
        private string _fileName;
        
        private string _imgFilePath;
        
        private string _songFilePath;
        
        private byte[] _imageData;

        private string _duration;


        public string Name { get => _name; set { _name = value;  } }
        public string Artist { get => _artist;  set { _artist = value;   } }
        public string FileName { get => _fileName; set { _fileName = value;  } }
        public string ImgFilePath {  get => _imgFilePath; set { _imgFilePath = value;  } }
        public string? SongFilePath { get => _songFilePath; set { _songFilePath = value; } }
        public byte[] ImageData { get => _imageData; set { _imageData = value;  } }
        public string Duration { get => _duration; set { _duration = value;  } }


        public override bool Equals(object obj)
        {
            return Equals(obj as Track);
        }

        public bool Equals(Track other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            if (GetType() != other.GetType()) return false;


            return Name == other.Name &&
                   Artist == other.Artist &&
                   FileName == other.FileName;
          
        }


        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Artist, FileName);
       
        }

       
        public static bool operator ==(Track left, Track right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left is null || right is null) return false;
            return left.Equals(right);
        }

        public static bool operator !=(Track left, Track right)
        {
            return !(left == right);
        }



    }
}
