using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.Services.PlayListService.Interfaces;
using test.ViewModel;
using test.ViewModel.CollectionClass;

namespace test.Services
{
    public interface IPlayListService : ICommonPlayListService
    {
        public Task AddTrackToPlayList(PlayList playList, Track track);
    }



}
