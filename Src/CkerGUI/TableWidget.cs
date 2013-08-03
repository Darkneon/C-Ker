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
using Cker.Models;
using Cker.Presenters;

namespace CkerGUI
{
    /// <summary>
    /// Display the table list view for the vessels.
    /// </summary>
    public class TableWidget
    {
        // List of vessels to link with table list view
        private ObservableCollection<Vessel> displayedVessels;

        // Tracks the current header clicked to know where to display the sorting arrow.
        private GridViewColumnHeader currentSortHeader;
        private DataTemplate sortAscendingArrowDisplay;
        private DataTemplate sortDescendingArrowDisplay;

        // Reference to the vessel presenter to get simulation info.
        private VesselPresenter vesselPresenter;

        /// <summary>
        /// Instantiates the table widget.
        /// </summary>
        /// <param name="presenter">reference to the vessel presenter</param>
        /// <param name="tableContainer">container GUI element of the table</param>
        /// <param name="sortAscendingDisplay">icon to display in column header when sorting ascending</param>
        /// <param name="sortDescendingDisplay">icon to display in column header when sorting descending</param>
        public TableWidget(VesselPresenter presenter, ListView tableContainer, DataTemplate sortAscendingDisplay, DataTemplate sortDescendingDisplay)
        {
            Debug.Assert(presenter != null && tableContainer != null && sortAscendingDisplay != null && sortDescendingDisplay != null, "TableWidget : null arguments");

            // Store template resources
            sortAscendingArrowDisplay = sortAscendingDisplay;
            sortDescendingArrowDisplay = sortDescendingDisplay;

            // Store presenter
            vesselPresenter = presenter;

            // Link the list view with the filtered vessels list.
            displayedVessels = new ObservableCollection<Vessel>();
            tableContainer.ItemsSource = displayedVessels;

            // Register callback when a column header is clicked to do sorting.
            tableContainer.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnColumnHeaderClick));

            // Register callback when the simulation updates.
            vesselPresenter.AddUpdateAction(OnSimulationUpdate);
        }

        /// <summary>
        /// Refreshes the vessel list with the latest information.
        /// </summary>
        public void UpdateVessels()
        {
            // Refresh and grab the latest list of vessels to be displayed.
            displayedVessels.Clear();
            foreach (var vessel in vesselPresenter.DisplayedVessels)
            {
                displayedVessels.Add(vessel);
            }
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

            // Apply the sorting and update.
            vesselPresenter.SortVessels(currentSortAttribute);
            UpdateVessels();

            // Change the look of the current column header to reflect sorting
            // but first reset the look of the previous header
            if (currentSortHeader != null)
            {
                currentSortHeader.Column.HeaderTemplate = null;
            }
            var columnHeaderTemplate = vesselPresenter.CurrentSortDirection == VesselPresenter.SortDirection.Ascending ? sortAscendingArrowDisplay : sortDescendingArrowDisplay;
            headerClicked.Column.HeaderTemplate = columnHeaderTemplate;

            // Save the current sort by attribute for next time.
            currentSortHeader = headerClicked;
        }

        private void OnSimulationUpdate()
        {
            // Invoke update on the dispatcher thread to allow GUI operations.
            Application.Current.Dispatcher.Invoke(UpdateVessels);
        }
    }
}
