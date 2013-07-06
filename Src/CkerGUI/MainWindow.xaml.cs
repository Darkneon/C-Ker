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

        public MainWindow()
        {
            InitializeComponent();
            InitializeFilteringOptions();
            InitializeVesselListDisplay();
            InitializeRadarDisplay();
            Cker.Server.Start();
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
                checkbox.Click += OnCheckboxToggle;
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
            allVessels = Cker.Server.Parse("Assets/", "comp354_vessel.vsf");

            // Start by displaying them all.
            filteredVessels = new ObservableCollection<Vessel>();
            foreach (var vessel in allVessels)
            {
                filteredVessels.Add(vessel);
            }
            vesselsListView.ItemsSource = filteredVessels;
        }

        private void InitializeRadarDisplay()
        {
            Debug.Assert(allVessels != null, "Vessels not initialized yet.");

            radarDisplay = new RadarDisplay(radarCanvas, Cker.Server.Simulator.Range, 0.95);
            radarDisplay.DrawVessels(allVessels);
        }

        // Called when a checkbox is toggled
        private void OnCheckboxToggle(object sender, RoutedEventArgs e)
        {
            // Cast back to checkbox
            CheckBox checkboxToggled = sender as CheckBox;

            if (checkboxToggled == checkShowAllVesselTypes)
            {
                // Allow user to skip the inderterminate state when toggling the all box.
                if (checkboxToggled.IsChecked == null)
                {
                    checkboxToggled.IsChecked = false;
                }

                // Propagate state to individual checkboxes.
                foreach (var checkbox in vesselFilterCheckboxes)
                {
                    checkbox.IsChecked = checkboxToggled.IsChecked;
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


            // TODO: Update filtering according to current updated checkboxes.
        }
    }
}
