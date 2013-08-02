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
using Cker.Models;
using CkerGUI.ViewModels;
using Cker.Presenters;

namespace CkerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List of vessels to link with list view
        private ObservableCollection<Vessel> displayedVessels;

        // Radar.
        private RadarDisplay radarDisplay;

        // Checkboxes allow user to filter list of vessels.
        private List<CheckBox> vesselFilterCheckboxes;

        // Tracks the current header clicked to know where to display the sorting arrow.
        private GridViewColumnHeader currentSortHeader;

        // Vessel presenter to get information from simulation.
        private VesselPresenter vesselPresenter;

        /*~~~~ For LoginOverlay ~~~~*/
        #region Fields

        public LoginViewModel ViewModel;

        #endregion

        #region Constructor
        /*~~~~ For LoginOverlay ~~~~*/

        public MainWindow()
        {
            /*~~~~ For LoginOverlay ~~~~*/
            // Initialize Login Screen
            this.ViewModel = new LoginViewModel();
            this.DataContext = this.ViewModel;
            /*~~~~ For LoginOverlay ~~~~*/

            vesselPresenter = new VesselPresenter();
            vesselPresenter.AddUpdateAction(OnServerUpdate);

            InitializeComponent();
            InitializeFilteringCheckboxes();
            InitializeVesselListDisplay();
            InitializeRadarDisplay();

            // Have a first update now so vessel list doesn't start empty.
            UpdateVessels();
        }

        private void InitializeFilteringCheckboxes()
        {
            vesselFilterCheckboxes = new List<CheckBox>();
            // Add one checkbox for each vessel type.
            foreach (var vesselType in Enum.GetValues(typeof(Cker.Models.Vessel.TargetType)))
            {
                // Make checkbox.
                CheckBox checkbox = new CheckBox();
                checkbox.Name = vesselType.ToString();
                checkbox.Content = vesselType.ToString();
                checkbox.Click += OnCheckboxClick;
                checkbox.IsChecked = true;

                // Add to list.
                vesselFilterCheckboxes.Add(checkbox);

                // Add to panel.
                individualVesselCheckboxes.Children.Add(checkbox);
            }
        }

        private void InitializeVesselListDisplay()
        {
            // Link the list view with the filtered vessels list.
            displayedVessels = new ObservableCollection<Vessel>();
            vesselsListView.ItemsSource = displayedVessels;
        }

        private void InitializeRadarDisplay()
        {
            radarDisplay = new RadarDisplay(radarCanvas, Cker.Simulator.Range, 0.95);
        }

        // Called when a checkbox is clicked
        private void OnCheckboxClick(object sender, RoutedEventArgs e)
        {
            // Cast back to checkbox
            CheckBox checkboxClicked = sender as CheckBox;

            if (checkboxClicked == checkShowAllVesselTypes)
            {
                // Allow user to skip the inderterminate state when toggling the all box.
                if (checkboxClicked.IsChecked == null)
                {
                    checkboxClicked.IsChecked = false;
                }

                // Propagate state to individual checkboxes.
                foreach (var checkbox in vesselFilterCheckboxes)
                {
                    checkbox.IsChecked = checkboxClicked.IsChecked;
                }
            }
            else
            {
                // See if it's everything on or everything off to update the check all box.
                bool isAllChecked = true;
                bool isAllUnchecked = true;
                foreach (var checkbox in vesselFilterCheckboxes)
                {
                    isAllChecked = isAllChecked ? checkbox.IsChecked.Value : isAllChecked;
                    isAllUnchecked = isAllUnchecked ? !checkbox.IsChecked.Value : isAllUnchecked;
                }
                checkShowAllVesselTypes.IsChecked = isAllChecked == isAllUnchecked ? (bool?)null : isAllChecked;
            }

            // See which checkboxes are checked finally to perform filtering.
            List<Vessel.TargetType> wantedTypes = new List<Vessel.TargetType>();
            foreach (var checkbox in vesselFilterCheckboxes)
            {
                if (checkbox.IsChecked == true)
                {
                    wantedTypes.Add( (Vessel.TargetType)Enum.Parse(typeof(Vessel.TargetType), checkbox.Name) );
                }
            }
            vesselPresenter.FilterVessels(wantedTypes);

            // Update vessels now that filtering changed.
            UpdateVessels();
        }

        private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader headerClicked = e.OriginalSource as GridViewColumnHeader;
            Debug.Assert(headerClicked != null, "OnColumnHeaderClick : Handling a null header");

            // Ignore clicks on the grid padding (extra spaces at the far right)
            if (headerClicked.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }

            // Retrieve name of the attritube associated to the column header just clicked.
            var dataBinding = (Binding)headerClicked.Column.DisplayMemberBinding;
            var currentSortAttribute = dataBinding.Path.Path;

            // Apply the sorting.
            vesselPresenter.SortVessels(currentSortAttribute);
            UpdateVessels();

            // Change the look of the current column header to reflect sorting
            // but first reset the look of the previous header
            if (currentSortHeader != null)
            {
                currentSortHeader.Column.HeaderTemplate = null;
            }
            var columnHeaderTemplate = vesselPresenter.CurrentSortDirection == VesselPresenter.SortDirection.Ascending ? Resources["ColumnHeaderArrowUp"] : Resources["ColumnHeaderArrowDown"];
            headerClicked.Column.HeaderTemplate = columnHeaderTemplate as DataTemplate;

            // Save the current sort by attribute for next time.
            currentSortHeader = headerClicked;
        }

        private void OnServerUpdate()
        {
            // Invoke update on the dispatcher thread.
            Application.Current.Dispatcher.Invoke(UpdateVessels);
        }

        private void UpdateVessels()
        {
            // Grab the latest list of vessels to be displayed.
            displayedVessels.Clear();
            foreach (var vessel in vesselPresenter.DisplayedVessels)
            {
                displayedVessels.Add(vessel);
            }

            // Update vessel map display.
            radarDisplay.DrawVessels(displayedVessels);
        }

        /*~~~~ For LoginOverlay ~~~~*/
        #endregion

        #region Event handler

        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            this.CKerLoginOverlayControl.Lock();
        }

        #endregion
        /*~~~~ For LoginOverlay ~~~~*/

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Prevent stuff from resizing themselves.
            this.SizeToContent = SizeToContent.Manual;
        }
    }
}
