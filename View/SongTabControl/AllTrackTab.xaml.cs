using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
using test.ViewModel.TabViewModel;


namespace test.SongTabControl
{
    /// <summary>
    /// Логика взаимодействия для AllTrackTab.xaml
    /// </summary>
    public partial class AllTrackTab : UserControl
    {
        public AllTrackTab()
        {
            InitializeComponent();

            this.DataContext = new ALLTrackTabView(Application.Current.Dispatcher, new AudioFileNameParser(), new DirectoryService(), new PathService());
        }
    }
}
