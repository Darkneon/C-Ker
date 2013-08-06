using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Cker.Models;
using Cker.Presenters;

namespace CkerGUI
{
    /// <summary>
    /// Provides the GUI display for the vessels.
    /// This includes the table and the radar widgets, as well as the filtering options.
    /// </summary>
    public class VesselView
    {
        // Checkboxes allow user to filter list of vessels.
        private List<CheckBox> vesselFilterCheckboxes;
        private CheckBox vesselToggleAllCheckbox;

        // Table widget for the list of vessels display.
        private TableWidget tableWidget;

        // Radar. **to be replaced with new radar**
        private RadarWidget radarDisplay;

        // Vessel presenter to get information from simulation.
        private VesselPresenter vesselPresenter;

        public VesselView()
        {
            var selectedScenarioFile = Application.Current.Properties["SelectedScenarioFile"] as string;
            vesselPresenter = new VesselPresenter(selectedScenarioFile);
            vesselPresenter.AddUpdateAction(OnServerUpdate);
        }

        /// <summary>
        /// Initialize table widget in the specified container.
        /// </summary>
        /// <param name="tableContainer"></param>
        /// <param name="sortAscendingDisplay"></param>
        /// <param name="sortDescendingDisplay"></param>
        public void SetupTableWidget(ListView tableContainer, DataTemplate sortAscendingDisplay, DataTemplate sortDescendingDisplay)
        {
            tableWidget = new TableWidget(vesselPresenter, tableContainer, sortAscendingDisplay, sortDescendingDisplay);
        }

        /// <summary>
        /// Initialize radar widget in the specified container.
        /// </summary>
        /// <param name="radarContainer"></param>
        public void SetupRadarWidget(Canvas radarContainer)
        {
            radarDisplay = new RadarWidget(vesselPresenter, radarContainer, Cker.Simulator.Range, 0.95);
        }

        /// <summary>
        /// Initialize filterting checkboxes in the specified container.
        /// </summary>
        /// <param name="toggleAllCheckbox"></param>
        /// <param name="checkboxContainer"></param>
        public void SetupFilteringCheckboxes(CheckBox toggleAllCheckbox, StackPanel checkboxContainer)
        {
            Debug.Assert(toggleAllCheckbox != null && checkboxContainer != null, "SetupFilteringCheckboxes : null arguments");

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
                checkboxContainer.Children.Add(checkbox);
            }
            vesselToggleAllCheckbox = toggleAllCheckbox;
            vesselToggleAllCheckbox.Click += OnCheckboxClick;
        }

        private void OnCheckboxClick(object sender, RoutedEventArgs e)
        {
            // Cast back to checkbox
            CheckBox checkboxClicked = sender as CheckBox;

            if (checkboxClicked == vesselToggleAllCheckbox)
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
                vesselToggleAllCheckbox.IsChecked = isAllChecked == isAllUnchecked ? (bool?)null : isAllChecked;
            }

            // See which checkboxes are checked finally to perform filtering.
            List<Vessel.TargetType> wantedTypes = new List<Vessel.TargetType>();
            foreach (var checkbox in vesselFilterCheckboxes)
            {
                if (checkbox.IsChecked == true)
                {
                    wantedTypes.Add((Vessel.TargetType)Enum.Parse(typeof(Vessel.TargetType), checkbox.Name));
                }
            }
            vesselPresenter.FilterVessels(wantedTypes);

            // Update vessels now that filtering changed.
            UpdateVessels();
        }

        private void OnServerUpdate()
        {
            // Invoke update on the dispatcher thread to allow GUI operations.
            Application.Current.Dispatcher.Invoke(UpdateVessels);
        }

        private void UpdateVessels()
        {
            // Update the table widget.
            tableWidget.UpdateVessels();

            // Update vessel map display.
            radarDisplay.DrawVessels( vesselPresenter.DisplayedVessels );
        }
    }
}
