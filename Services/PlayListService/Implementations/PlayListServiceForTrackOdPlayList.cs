using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using test.Services.PlayListService.Interfaces;
using test.ViewModel;

namespace test.Services.PlayListService.Implementations
{
    public class PlayListServiceForTrackOdPlayList : BasePlayList, IPlayListServiceInside
    {

        private readonly IDirectoryService _directoryService;
        public PlayListServiceForTrackOdPlayList(IPathService pathService, IDirectoryService directoryService) : base(pathService) 
        {
            _directoryService = directoryService;


        }

        public async Task AddTrackToPlayList(PlayList sourcePlayList, PlayList targetPlayList, Track track)
        {

            IDirectoryService directoryService = new DirectoryService();

            string SourceImg = Path.Combine(Path.Combine(sourcePlayList.Directory, "img"), $"{track.FileName}.jpg");
            string SourceSong = Path.Combine(Path.Combine(sourcePlayList.Directory, "song"), $"{track.FileName}.mp3");


            string TargetImg = Path.Combine(Path.Combine(sourcePlayList.Directory, "img"), $"{track.FileName}.jpg");
            string TargerSong = Path.Combine(Path.Combine(sourcePlayList.Directory, "song"), $"{track.FileName}.mp3");


            await directoryService.CopyFileToDerictory(SourceSong, TargerSong);
            await directoryService.CopyFileToDerictory(SourceImg, TargerSong);

        }

        public async Task DeleteTrackFromPlayList(Track track, PlayList sourcePlaylist)
        {
            var path = _pathService.ParseAll();

            var playListPath = Path.Combine(path.PlayListPath, sourcePlaylist.Name);

            var songPath = Path.Combine(playListPath, "song");

            var imgPath = Path.Combine(playListPath, "img");

            await _directoryService.DellFile(Path.Combine(imgPath, track.FileName+".jpg"));
            await _directoryService.DellFile(Path.Combine(songPath, track.FileName+".mp3"));

        }

   
    }
}
