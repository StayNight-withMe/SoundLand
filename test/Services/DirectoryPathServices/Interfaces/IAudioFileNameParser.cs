using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace test.Services
{
    public interface IAudioFileNameParser
    {
        public string GetSongName(string[] filePart);
        public string GetSongArtist(string[] filePart);
        public string GetSongDuration(string[] filePart);

        FileNameInfo ParseAll(string filePart);

    }
}
