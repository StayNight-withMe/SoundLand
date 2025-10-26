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
    public class PlayListServiceForAllTrack : BasePlayList, IPlayListService
    {


        public PlayListServiceForAllTrack(IPathService pathService) : base(pathService) { }

        public async Task AddTrackToPlayList(PlayList playList, Track track)
        {
            GetPath getPath = _pathService.ParseAll();
            DirectoryService dir = new DirectoryService();


            string SongFile = $"{track.FileName}.mp3";
            string ImgFile = $"{track.FileName}.jpg";

            string TargerImg = Path.Combine(Path.Combine(playList.Directory, "img"), ImgFile);
            string TargerSong = Path.Combine(Path.Combine(playList.Directory, "song"), SongFile);

            string SourceImg = Path.Combine(getPath.AllImgPath, ImgFile);
            string SourceSong = Path.Combine(getPath.AllSongPath, SongFile);

            await dir.CopyFileToDerictory(SourceSong, TargerSong);
            await dir.CopyFileToDerictory(SourceImg, TargerImg);


        }


    }
}
