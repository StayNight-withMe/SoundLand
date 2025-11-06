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
            //
        var dispatcher = Application.Current.Dispatcher;
        var di = new DependencyInjection(dispatcher);

        ViewModelRegistration.RegisterCoreServices(di);
        ViewModelRegistration.RegisterPlayListTab(di);
        ViewModelRegistration.RegisterTrackOfPlayList(di);

        var playListTab = new PlayListTab();
        playListTab.DataContext = di.Resolve<PlayListTabView>();
        PlayListContainer.Children.Add(playListTab);

        var trackOfPlayList = new TrackOfPlayList();
        trackOfPlayList.DataContext = di.Resolve<TrackOfPlayListView>();

        if (playListTab.FindName("TrackOfPlayListContainer") is Panel container)
        {
            container.Children.Add(trackOfPlayList);
        }

  
        var mediaService = (trackOfPlayList.DataContext as TrackOfPlayListView)?.MediaService as MediaService;
        var mediaService1 = (playListTab.DataContext as PlayListTabView)?.MediaService as MediaService;
        
        mediaService?.SetMediaElement(trackOfPlayList.MediaPlayer);
         mediaService1?.SetMediaElement(trackOfPlayList.MediaPlayer);
        }
}

}