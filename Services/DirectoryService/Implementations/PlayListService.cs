using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.Diagnostics;
using test.ViewModel.CollectionClass;

namespace test.Services
{
    public class PlayListService: DirectoryService, IPlayListService
    {

        private string PlayListPath(string path)
        {
            if (path == null)
            { Debug.WriteLine("пустоезначение для создания плейлиста"); return ""; }
            else
            {
                PathService pathService = new PathService();
                GetPath getPath = pathService.ParseAll();

                string fullPath = Path.Combine(getPath.PlayListPath, path);

                return fullPath;
            }
         
        }
        public void CreatePlayList(string namePlaylist)
        {
                Directory.CreateDirectory(PlayListPath(namePlaylist));

            Directory.CreateDirectory(Path.Combine(PlayListPath(namePlaylist), "img"));
            Directory.CreateDirectory(Path.Combine(PlayListPath(namePlaylist), "song"));
        }

        public void DelPlayList(string namePlaylist)
        {
            Directory.Delete(PlayListPath(namePlaylist), true);   
        }


        public async void AddTrackToPlayList(PlayList playList, Track track)
        {

            

            string TargerImg = Path.Combine(Path.Combine(playList.Directory, "img"), $"{track.FileName}.jpg");
            string TargerSong = Path.Combine(Path.Combine(playList.Directory, "song"), $"{track.FileName}.mp3");

            await CopyFileToDerictory(track.SongFilePath,  TargerSong);
            await CopyFileToDerictory(track.ImgFilePath, TargerImg);


        }

    }
}
