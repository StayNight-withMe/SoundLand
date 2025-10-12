using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using test.Services;

namespace test.ViewModel
{



    abstract public class BaseViewModel : INotifyPropertyChanged
    {
        protected  IAudioFileNameParser _audioFileNameParser;

        protected IPlayListService _playlistService;

        protected IDirectoryService _directoryService;

        protected ITrackCollectionService _trackCollectionService;

        protected GetPath _getPath;

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
