using System.Windows.Controls;
using test.ViewModel.TabViewModel;
using System.Windows;
using System.Windows.Threading;

using test.Services;

namespace test.SongTabControl
{
    public partial class MainSongTab : UserControl
    {
        public MainSongTab()
        {
            InitializeComponent();

            var pathService = new PathService();
            var audioParser = new AudioFileNameParser(pathService);
            var playListService = new PlayListServiceForSearchTrack(pathService);
            var directoryService = new DirectoryService();
            var dispatcher = Application.Current.Dispatcher;
            var pythonService = new PythonScriptService();
         
            this.DataContext = new MainSongTabView(pythonService, audioParser, playListService, pathService, directoryService);

        }
    }
}