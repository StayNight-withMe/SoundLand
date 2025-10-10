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

        protected readonly IPythonScriptService _pythonScriptService;

        protected readonly IAudioFileNameParser _audioFileNameParser;

        protected readonly IPlayListService _playlistService;

        protected readonly IPathService _pathService;

        protected readonly IDirectoryService _directoryService;

        public event PropertyChangedEventHandler PropertyChanged;

        public BaseViewModel(IAudioFileNameParser audioFileNameParser, IPlayListService playlistService, IPathService pathService, IDirectoryService directoryService)
        {
        
            _audioFileNameParser = audioFileNameParser;
            _playlistService = playlistService;
            _pathService = pathService;
            _directoryService = directoryService;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }



    }
}
