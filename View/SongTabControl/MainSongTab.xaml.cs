using System.Windows.Controls;
using test.ViewModel.TabViewModel;
using System.Windows;
using System.Windows.Threading;
using test.Model;
using test.Services;

namespace test.SongTabControl
{
    public partial class MainSongTab : UserControl
    {
        public MainSongTab()
        {
            InitializeComponent();

            this.DataContext = new MainSongTabView(new PythonScriptService(), new AudioFileNameParser(), new DirectoryService(), new PathService());
        }
    }
}