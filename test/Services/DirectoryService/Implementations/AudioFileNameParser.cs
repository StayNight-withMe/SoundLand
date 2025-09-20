using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


using SI = System.Windows.Shapes;


namespace test.Services
{

    public class AudioFileNameParser : IAudioFileNameParser
    {

        private string[] GetFileParts(string filePart)
        {
            string fileName = Path.GetFileName(filePart);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string[] parts = fileNameWithoutExtension.Split('_');

            return parts;
        }


        public string GetSongName(string[] filePart)
        {
           
            return string.Join("_", filePart.Take(filePart.Length - 3)); 
        }

        public string GetSongArtist(string[] filePart)
        {
            return filePart[filePart.Length - 3];
        }

        public string GetSongDuration(string[] filePart)
        {
            string minutes = filePart[filePart.Length - 2];
            string seconds = filePart[filePart .Length - 1];


            return $"{minutes.PadLeft(2, '0')}:{seconds.PadLeft(2, '0')}";
        }



        
        public FileNameInfo ParseAll(string filePart)
        {
            GetPath getPath = new GetPath();

            var parts = GetFileParts(filePart);

            //string imgFilePath = Path.Combine(getPath.BasePath, Path.GetFileName(filePart));
            

            return new FileNameInfo
            {
                FileName = Path.GetFileName(filePart),
                SongName = GetSongName(parts),
                SongArtist = GetSongArtist(parts),
                SongDuration = GetSongDuration(parts),
                ImgFilePath = filePart,
            };

        }


    }
}
