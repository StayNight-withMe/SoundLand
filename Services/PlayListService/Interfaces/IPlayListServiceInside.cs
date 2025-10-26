using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.ViewModel;

namespace test.Services.PlayListService.Interfaces
{
    public interface IPlayListServiceInside : ICommonPlayListService
    {

        public Task AddTrackToPlayList(PlayList sourcePlayList, PlayList targetPlayList, Track track);
        public Task DeleteTrackFromPlayList(Track track, PlayList sourcePlaylist);

    }
}
