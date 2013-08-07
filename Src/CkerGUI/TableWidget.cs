using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Media;
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

        // GUI element which contains the list of vessels to be displayed.
        private ListView vesselListContainer;

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

            // Store arguments
            sortAscendingArrowDisplay = sortAscendingDisplay;
            sortDescendingArrowDisplay = sortDescendingDisplay;
            vesselPresenter = presenter;
            vesselListContainer = tableContainer;

            // Link the list view with the filtered vessels list.
            displayedVessels = new ObservableCollection<Vessel>();
            vesselListContainer.ItemsSource = displayedVessels;

            // Register callback when a column header is clicked to do sorting.
            vesselListContainer.AddHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnColumnHeaderClick));

            // Register callback when the simulation updates to refresh our list.
            vesselPresenter.AddUpdateAction(OnSimulationUpdate);

            // Register callback when item container status changes to be able to change style colors for alarms.
            vesselListContainer.ItemContainerGenerator.StatusChanged += new EventHandler(OnItemContainerStatusChanged);
        }

        /// <summary>
        /// Reset the widget to be like at start.
        /// </summary>
        public void Reset()
        {
            vesselListContainer.RemoveHandler(GridViewColumnHeader.ClickEvent, new RoutedEventHandler(OnColumnHeaderClick));
            vesselListContainer.ItemContainerGenerator.StatusChanged -= new EventHandler(OnItemContainerStatusChanged);
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

            // Ignore clicks on the grid padding (extra spaces at the far right)
            if (headerClicked == null || headerClicked.Role == GridViewColumnHeaderRole.Padding)
            {
                return;
            }

            // Retrieve name of the attritube associated to the column header just clicked.
            // This corresponds to the binding; however, this is not the case when there are multibinds; we just take the header name instead.
            var dataBinding = (Binding)headerClicked.Column.DisplayMemberBinding;
            var currentSortAttribute = dataBinding != null ? dataBinding.Path.Path : headerClicked.Content as string;

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

        private void OnItemContainerStatusChanged(object sender, EventArgs e)
        {
            // Item containers (here, it is ListViewItem) are created asynchronously only when needed.
            // So in order to access container, we need this callback to know when it is created.
            if (vesselListContainer.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                // Loop through each item and see if the container is available and modify container styles.
                int i = 0;
                foreach (var vessel in vesselListContainer.Items)
                {
                    // Try to get container.
                    var item = vesselListContainer.ItemContainerGenerator.ContainerFromItem(vessel) as ListViewItem;
                    if (item != null)
                    {
                        // Alternate row colours.
                        item.Background = (i & 1) == 0 ? Brushes.White : Brushes.AliceBlue;

                        // Change colour if there is associated alarm with this vessel.
                        var alarmMatch = vesselPresenter.CurrentAlarms.Find(alarm => alarm.first == vessel || alarm.second == vessel);
                        if (alarmMatch.first != null && alarmMatch.second != null)
                        {
                            // Red if high risk, yellow if low risk
                            if (alarmMatch.type == Cker.Simulator.AlarmType.Low)
                            {
                                item.Background = Brushes.Yellow;
                            }
                            else if (alarmMatch.type == Cker.Simulator.AlarmType.High)
                            {
                                item.Background = Brushes.Red;
                            }
                        }
                    }
                    ++i;
                }
            }
        }

        private void OnSimulationUpdate()
        {
            // Invoke update on the dispatcher thread to allow GUI operations.
            Application.Current.Dispatcher.Invoke(UpdateVessels);
        }
    }
}
