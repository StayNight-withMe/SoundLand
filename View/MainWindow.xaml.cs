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

            var dispatcher = Application.Current.Dispatcher;

            // ✅ Создаём DI контейнер:
            var di = new DependencyInjection(dispatcher);

            // ✅ Регистрируем ВСЕ сервисы ОДИН РАЗ:
            ViewModelRegistration.RegisterCoreServices(di);      // ✅ Основные сервисы
            ViewModelRegistration.RegisterPlayListTab(di);       // ✅ Для PlayListTab
            ViewModelRegistration.RegisterTrackOfPlayList(di);   // ✅ Для TrackOfPlayList

            // ✅ Создаём PlayListTab:
            var playListTab = new PlayListTab();
            playListTab.DataContext = di.Resolve<PlayListTabView>();
            PlayListContainer.Children.Add(playListTab);

            // ✅ Создаём TrackOfPlayList:
            var trackOfPlayList = new TrackOfPlayList();
            trackOfPlayList.DataContext = di.Resolve<TrackOfPlayListView>();

            // ✅ Добавляем TrackOfPlayList в контейнер:
            if (playListTab.FindName("TrackOfPlayListContainer") is Panel container)
            {
                container.Children.Add(trackOfPlayList);
            }

            // ❌ Было:
            // var mediaService = (DataContext as TrackOfPlayListView)?.MediaService as MediaService;

            // ✅ Стало:
            var mediaService = (trackOfPlayList.DataContext as TrackOfPlayListView)?.MediaService as MediaService;
            mediaService?.SetMediaElement(MediaPlayer);
        }
    }
}