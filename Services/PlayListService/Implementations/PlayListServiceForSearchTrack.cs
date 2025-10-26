using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.ViewModel.CollectionClass;
using System.IO;
using test.ViewModel;

namespace test.Services
{
    public class PlayListServiceForSearchTrack : BasePlayList, IPlayListService
    {

        private readonly IPathService _pathService;

        public PlayListServiceForSearchTrack(IPathService pathService) : base(pathService) { }


        public async Task AddTrackToPlayList(PlayList playList, Track track)
        {
            DirectoryService dir = new DirectoryService();


            string TargerImg = Path.Combine(Path.Combine(playList.Directory, "img"), $"{track.FileName}.jpg");
            string TargerSong = Path.Combine(Path.Combine(playList.Directory, "song"), $"{track.FileName}.mp3");

            await dir.CopyFileToDerictory(track.SongFilePath, TargerSong);
            await dir.CopyFileToDerictory(track.ImgFilePath, TargerImg);


        }
    }
}
