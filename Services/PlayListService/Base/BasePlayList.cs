using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.ViewModel.CollectionClass;
using test.ViewModel;

namespace test.Services
{
    abstract public class BasePlayList : IPlayListService
    {
        protected readonly IPathService _pathService;

        public BasePlayList(IPathService pathService) 
        {
            _pathService = pathService;
        }
         


        private string PlayListPath(string path)
        {
            if (path == null)
            { Debug.WriteLine("пустое значение для создания плейлиста"); return ""; }
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


        public virtual void AddTrackToPlayList(PlayList playList, Track track) { }
        
        

        


    }
}
