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

namespace CkerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // List of vessels
        private List<Vessel> allVessels;
        private ObservableCollection<Vessel> filteredVessels;

        // Radar.
        private RadarDisplay radarDisplay;

        // Checkboxes allow user to filter list of vessels.
        private List<CheckBox> vesselFilterCheckboxes;

        // Tracks the current attribute to sort list by, and the sorting direction.
        private GridViewColumnHeader currentSortHeader;
        private ListSortDirection currentSortDirection;

        public MainWindow()
        {
            Cker.Server.Start("Assets/", "comp354_vessel.vsf");
            Cker.Server.AfterUpdate += OnServerUpdate;

            InitializeComponent();
            InitializeFilteringOptions();
            InitializeVesselListDisplay();
            InitializeRadarDisplay();

            // Have a first update now so vessel list doesn't start empty.
            UpdateVessels();
        }

        private void InitializeFilteringOptions()
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
            // Populate vessels
            allVessels = Cker.Server.Vessels;

            // Link the list view with the filtered vessels list.
            filteredVessels = new ObservableCollection<Vessel>();
            vesselsListView.ItemsSource = filteredVessels;
        }

        private void InitializeRadarDisplay()
        {
            Debug.Assert(allVessels != null, "Vessels not initialized yet.");

            radarDisplay = new RadarDisplay(radarCanvas, Cker.Server.Simulator.Range, 0.95);
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

            // Determine sort direction.
            // If the clicked attribute is the same as the current one, toggle direction. Otherwise just assume one.
            if (headerClicked == currentSortHeader)
            {
                currentSortDirection = GetToggledSortDirection(currentSortDirection);
            }
            else
            {
                currentSortDirection = ListSortDirection.Descending;
            }

            // Retrieve name of the attritube associated to the column header just clicked.
            var dataBinding = (Binding)headerClicked.Column.DisplayMemberBinding;
            var currentSortAttribute = dataBinding.Path.Path;

            // Apply the sorting on the list.
            vesselsListView.Items.SortDescriptions.Clear();
            vesselsListView.Items.SortDescriptions.Add(new SortDescription(currentSortAttribute, currentSortDirection));
            UpdateVessels();

            // Change the look of the current column header to reflect sorting
            // but first reset the look of the previous header
            if (currentSortHeader != null)
            {
                currentSortHeader.Column.HeaderTemplate = null;
            }
            var columnHeaderTemplate = currentSortDirection == ListSortDirection.Ascending ? Resources["ColumnHeaderArrowUp"] : Resources["ColumnHeaderArrowDown"];
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
            // Update list of vessels according to current filters
            filteredVessels.Clear();
            foreach (var checkbox in vesselFilterCheckboxes)
            {
                if (checkbox.IsChecked == true)
                {
                    var matchingVessels = allVessels.FindAll(vessel => vessel.Type.ToString() == checkbox.Name);
                    foreach (var match in matchingVessels)
                    {
                        filteredVessels.Add(match);
                    }
                }
            }

            // Update vessel map display.
            radarDisplay.DrawVessels(filteredVessels);
        }

        // Returns the toggled direction from the specified direction.
        private ListSortDirection GetToggledSortDirection(ListSortDirection direction)
        {
            ListSortDirection toggledDirection = ListSortDirection.Ascending;
            if (direction == ListSortDirection.Ascending)
            {
                toggledDirection = ListSortDirection.Descending;
            }
            return toggledDirection;
        }
    }
}
