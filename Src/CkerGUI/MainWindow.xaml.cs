using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using Cker.Authentication;
using Cker.Models;

namespace CkerGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private VesselView vesselView;

        public MainWindow()
        {
            InitializeComponent();

            vesselView = new VesselView();
            vesselView.SetupTableWidget(vesselsListView, (DataTemplate)Resources["ColumnHeaderArrowUp"], (DataTemplate)Resources["ColumnHeaderArrowDown"]);
            vesselView.SetupRadarWidget(radarCanvas);
            vesselView.SetupFilteringCheckboxes(toggleAllVesselCheckbox, individualVesselCheckboxes);

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
                paddingColumn.Width = radarGrid.Margin.Left + 4;
                paddingColumn.DisplayMemberBinding = new Binding() { Source = null };
                vesselsGridView.Columns.Insert(0, paddingColumn);
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

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // Prevent stuff from resizing themselves.
            // In particular, this is intended to prevent the table from growing when there are lots of items.
            this.SizeToContent = SizeToContent.Manual;
        }
    }
}
