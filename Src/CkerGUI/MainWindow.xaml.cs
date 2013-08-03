using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using System.Windows.Threading;
using System.ComponentModel;
using CkerGUI.ViewModels;

namespace CkerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        /*~~~~ For LoginOverlay ~~~~*/
        #region Fields

        public LoginViewModel ViewModel;

        #endregion
        /*~~~~ For LoginOverlay ~~~~*/

        private VesselView vesselView;

        public MainWindow()
        {
            /*~~~~ For LoginOverlay ~~~~*/
            // Initialize Login Screen
            this.ViewModel = new LoginViewModel();
            this.DataContext = this.ViewModel;
            /*~~~~ For LoginOverlay ~~~~*/

            InitializeComponent();

            vesselView = new VesselView();
            vesselView.SetupTableWidget(vesselsListView, (DataTemplate)Resources["ColumnHeaderArrowUp"], (DataTemplate)Resources["ColumnHeaderArrowDown"]);
            vesselView.SetupRadarWidget(radarCanvas);
            vesselView.SetupFilteringCheckboxes(toggleAllVesselCheckbox, individualVesselCheckboxes);
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Prevent stuff from resizing themselves.
            // In particular, this is intended to prevent the table from growing when there are lots of items.
            this.SizeToContent = SizeToContent.Manual;
        }

        /*~~~~ For LoginOverlay ~~~~*/
        #region Event handler

        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            this.CKerLoginOverlayControl.Lock();
        }

        #endregion
        /*~~~~ For LoginOverlay ~~~~*/
    }
}
