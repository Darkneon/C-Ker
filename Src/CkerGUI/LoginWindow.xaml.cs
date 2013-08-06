using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CkerGUI
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        // Login overlay
        public LoginView ViewModel;

        public LoginWindow()
        {
            InitializeComponent();

            // Initialize Login Screen
            this.ViewModel = new LoginView();
            this.DataContext = this.ViewModel;

            InitializeScenarioFileList();
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

        private void OnListDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var fileList = sender as ListBox;
            var selectedFile = fileList.SelectedItem as string;

            // Store it for later.
            Application.Current.Properties["SelectedScenarioFile"] = selectedFile;

            // Switch to the main window now.
            MainWindow mainWindow = new MainWindow();
            App.Current.MainWindow = mainWindow;
            mainWindow.Show();
            this.Close();
        }

        private void btnLock_Click(object sender, RoutedEventArgs e)
        {
            this.CKerLoginOverlayControl.Lock();
        }
    }
}
