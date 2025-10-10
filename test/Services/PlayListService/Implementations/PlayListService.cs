using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.Diagnostics;
using test.ViewModel;
using System.Configuration;
using System.Windows.Controls;
using System.Runtime.CompilerServices;
using test.Services;



namespace test.Services
{
  
    }


    public class PlayListServiceForPlayListDirectory : BasePlayList
    {
    
        private IPathService _pathService;



    public PlayListServiceForPlayListDirectory(IPathService pathService) : base(pathService) { }


        public  async void AddTrackToPlayList(PlayList sourcePlayList, PlayList targetPlayList,Track track)
        {

            IDirectoryService directoryService = new DirectoryService();

            string SourceImg = Path.Combine(Path.Combine(sourcePlayList.Directory, "img"), $"{track.FileName}.jpg");
            string SourceSong = Path.Combine(Path.Combine(sourcePlayList.Directory, "song"), $"{track.FileName}.mp3");


            string TargetImg = Path.Combine(Path.Combine(sourcePlayList.Directory, "img"), $"{track.FileName}.jpg");
            string TargerSong = Path.Combine(Path.Combine(sourcePlayList.Directory, "song"), $"{track.FileName}.mp3");


            await directoryService.CopyFileToDerictory(SourceSong, TargerSong);
            await directoryService.CopyFileToDerictory(SourceImg, TargerSong);

    }



}




