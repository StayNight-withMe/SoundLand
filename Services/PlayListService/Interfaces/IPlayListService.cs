using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.ViewModel.CollectionClass;
using test.ViewModel;

namespace test.Services
{
    public interface IPlayListService 
    {
        
        public void CreatePlayList(string namePlaylist); 
        public void DelPlayList(string namePlaylist);
        public virtual async void AddTrackToPlayList(PlayList playList, Track track) { }
    }
}
