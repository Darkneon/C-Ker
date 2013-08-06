using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Cker.Authentication;
using Cker.Models;
using Microsoft.Win32;

namespace CkerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VesselView vesselView;
        private LoginView loginView;

        public MainWindow()
        {
            InitializeComponent();
            InitializeScenarioFileList();

            loginView = new LoginView();
            loginView.AddLoginCompleteAction(OnLoginComplete);
            DataContext = loginView;
        }

        private void InitializeScenarioFileList()
        {
            // Get the list of files in the asset directory.
            string[] scenarioFiles = Directory.GetFiles("Assets/", "*.vsf");

            // Add each one to the list box to display.
            foreach (var file in scenarioFiles)
            {
                // Strip the directory part and keep only name and extension
                var fileName = file.Replace("Assets/", "");

                fileSelectionListBox.Items.Add(fileName);
            }
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Prevent stuff from resizing themselves.
            // In particular, this is intended to prevent the table from growing when there are lots of items.
            this.SizeToContent = SizeToContent.Manual;
        }

        private void OnLoginComplete()
        {
            // Hide login information
            loginInformationLabel.Visibility = System.Windows.Visibility.Hidden;

            // Get the current user to check it's priviledges.
            User currentUser = Authenticator.CurrentUser;
            if (currentUser.Type == UserType.Operator)
            {
                // Remove filtering and sorting abilities.
                filteringOptionsPanel.Visibility = Visibility.Hidden;
                vesselsListView.AddHandler(GridViewColumnHeader.PreviewMouseLeftButtonDownEvent, new RoutedEventHandler(OnOperatorMouseDown));
                vesselsGrid.Margin = new Thickness(-Application.Current.MainWindow.Width / 6.0, 0.0, 0.0, 0.0);
                // Center the table correctly.
                GridViewColumn paddingColumn = new GridViewColumn();
                paddingColumn.Width = 104.0;
                paddingColumn.DisplayMemberBinding = new Binding() { Source = null };
                vesselsGridView.Columns.Insert(0, paddingColumn);
            }
        }

        private void OnListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fileList = sender as ListBox;
            var selectedFile = fileList.SelectedItem as string;

            // Only handle if valid
            if (selectedFile != null && selectedFile != "")
            {
                OnScenarioFileSelected(selectedFile);
            }
        }

        private void OnNewSimulationButtonClick(object sender, RoutedEventArgs e)
        {
            // Make the file selection visible.
            fileSelectionListBox.Visibility = System.Windows.Visibility.Visible;
            fileSelectionLabel.Visibility = System.Windows.Visibility.Visible;
            fileBrowseButton.Visibility = System.Windows.Visibility.Visible;
            fileCancelButton.Visibility = System.Windows.Visibility.Visible;
        }

        private void OnFileBrowseButtonClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog browseDialog = new OpenFileDialog();
            browseDialog.DefaultExt = ".vsf";
            browseDialog.Filter = "scenario files (*.vsf)|*vsf";
            browseDialog.Multiselect = false;
            browseDialog.Title = "Select scenario file.";
            
            bool? result = browseDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                string selectedFile = Path.GetFileName(browseDialog.FileName);
                string fileExtension = Path.GetExtension(selectedFile);
                if (fileExtension == ".vsf")
                {
                    OnScenarioFileSelected(selectedFile);
                }
                else
                {
                    fileBrowseErrorLabel.Visibility = System.Windows.Visibility.Visible;
                }
            }
        }

        private void OnFileCancelButtonClick(object sender, RoutedEventArgs e)
        {
            // Hide stuff not needed.
            HideFileSelectionOptions();
        }

        private void OnScenarioFileSelected(string file)
        {
            // Setup simulation view
            vesselView = new VesselView(file);
            vesselView.SetupTableWidget(vesselsListView, (DataTemplate)Resources["ColumnHeaderArrowUp"], (DataTemplate)Resources["ColumnHeaderArrowDown"]);
            vesselView.SetupRadarWidget(radarCanvas);
            vesselView.SetupFilteringCheckboxes(toggleAllVesselCheckbox, individualVesselCheckboxes);
            vesselView.UpdateVessels();

            // Show filtering options if admin
            User currentUser = Authenticator.CurrentUser;
            if (currentUser.Type == UserType.Administrator)
            {
                filteringOptionsPanel.Visibility = System.Windows.Visibility.Visible;
            }

            // Hide stuff not needed.
            HideFileSelectionOptions();
            foreach (var column in vesselsGridView.Columns)
            {
                column.HeaderTemplate = null;
            }
        }

        private void OnOperatorMouseDown(object sender, RoutedEventArgs e)
        {
            // Skip the mouse down on the column to prevent sorting from operator.
            // But don't skip the scroll bar to still allow scrolling.
            var sourceElement = e.OriginalSource as DependencyObject;
            var sourceElementType = sourceElement.GetType();
            if (sourceElementType.Name != "ScrollChrome")
            {
                e.Handled = true;
            }
        }

        private void HideFileSelectionOptions()
        {
            // Hide file selection options
            fileSelectionListBox.Visibility = System.Windows.Visibility.Hidden;
            fileSelectionLabel.Visibility = System.Windows.Visibility.Hidden;
            fileBrowseButton.Visibility = System.Windows.Visibility.Hidden;
            fileBrowseErrorLabel.Visibility = System.Windows.Visibility.Hidden;
            fileCancelButton.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
