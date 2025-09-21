using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using test.Services;

using test.SongTabControl;
using test.ViewModel;
using test.ViewModel.TabViewModel;




namespace test.SongTabControl
{
    public partial class PlayListTab : UserControl
    {
        public PlayListTab()
        {
            InitializeComponent();

            var pathService = new PathService();
            var audioParser = new AudioFileNameParser(pathService);
            var directoryService = new PlayListService();
            var dispatcher = Application.Current.Dispatcher;
            var pythonService = new PythonScriptService();

            this.DataContext = new PlayListTabView(dispatcher,audioParser, directoryService, pathService);         
        }

    }
}