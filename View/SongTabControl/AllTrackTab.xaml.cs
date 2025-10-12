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
    public partial class AllTrackTab : UserControl
    {
        public AllTrackTab()
        {
            InitializeComponent();


            var dispatcher = Application.Current.Dispatcher;

            DependencyInjection DI = new DependencyInjection(dispatcher);
            
            ViewModelRegistration.RegisterAllTrackTab(DI);
            ViewModelRegistration.RegisterCoreServices(DI);

     
          DataContext = DI.Resolve<ALLTrackTabView>();
        }
    }
}
