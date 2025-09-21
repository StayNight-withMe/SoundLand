using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.ViewModel.CollectionClass;

namespace test.Services
{
    public interface IPlayListService : IDirectoryService
    {
        public void CreatePlayList(string namePlaylist); 
        public void DelPlayList(string namePlaylist);

        public async void AddTrackToPlayList(PlayList playList, Track track) { }
    }
}
