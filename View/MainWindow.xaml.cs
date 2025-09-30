using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using test.Services;
using test.SongTabControl;
using test.ViewModel;
using test.ViewModel.CollectionClass;
using test.ViewModel.TabViewModel;


namespace test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var pathService = new PathService();
            var audioParser = new AudioFileNameParser(pathService);
            var playListService = new PlayListServiceForAllTrack(pathService);
            var directoryService = new DirectoryService();
            var dispatcher = Application.Current.Dispatcher;
            var pythonService = new PythonScriptService();
            var playListTab = new PlayListTab();
   

            var sharedService = new TrackCollectionService();
            playListTab.DataContext = new PlayListTabView(
                    dispatcher, audioParser, playListService, pathService, directoryService, sharedService
                );

            PlayListContainer.Children.Add(playListTab);


             var trackOfPlayList = new TrackOfPlayList();
            trackOfPlayList.DataContext = new TrackOfPlayListView(sharedService, pathService, audioParser, directoryService, playListService);

            // Добавляем TrackOfPlayList в нужное место
            // (предполагая, что у тебя есть контейнер для него)

            if (playListTab.FindName("TrackOfPlayListContainer") is Panel container)
            {
                container.Children.Add(trackOfPlayList);
            }

            var mediaService = (DataContext as TrackOfPlayListView)?.MediaService as MediaService;
            mediaService?.SetMediaElement(MediaPlayer);


        }
    }
}