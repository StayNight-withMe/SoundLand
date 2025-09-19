using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.IO;
using System.Diagnostics;

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
            
           
        }

        public void DelPlayList(string namePlaylist)
        {
            Directory.Delete(PlayListPath(namePlaylist), true);   
        }

    }
}
