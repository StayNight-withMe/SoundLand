using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using test.ViewModel;

namespace test.Services
{
    public interface ITrackCollectionService
    {
        public PlayList playList { get; set; }
        public ObservableCollection<Track> Collection { get; set; }
        public ObservableCollection<Track> GetTracks(string path, IAudioFileNameParser audioFileNameParser);


    }
}
