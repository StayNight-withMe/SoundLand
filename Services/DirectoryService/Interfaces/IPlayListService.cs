using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace test.Services
{
    public interface IPlayListService : IDirectoryService
    {
        public void CreatePlayList(string namePlaylist); 
        public void DelPlayList(string namePlaylist);
    }
}
