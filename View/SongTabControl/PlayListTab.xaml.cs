using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

using test.Services;

using test.SongTabControl;
using test.ViewModel;
using test.ViewModel.CollectionClass;
using test.ViewModel.TabViewModel;




namespace test.SongTabControl
{
    public partial class PlayListTab : UserControl
    {
        public PlayListTab()
        {

            InitializeComponent();

            DataContext = this;
        }
    }
}