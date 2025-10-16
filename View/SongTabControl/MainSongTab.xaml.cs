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



            var dispatcher = Application.Current.Dispatcher;

            DependencyInjection di = new DependencyInjection(dispatcher);

            ViewModelRegistration.RegisterMainSongTab(di);
            ViewModelRegistration.RegisterCoreServices(di);

            DataContext = di.Resolve<MainSongTabView>();

            var mediaService = (DataContext as MainSongTabView)?.MediaService as MediaService;
            mediaService?.SetMediaElement(MediaPlayer);

        }




    }
}