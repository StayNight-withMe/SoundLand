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
     
            this.DataContext = new PlayListTabView(Application.Current.Dispatcher, new AudioFileNameParser(), new PlayListService(), new PathService());
        }
    }
}