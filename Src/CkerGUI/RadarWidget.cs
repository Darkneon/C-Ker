using System;
using System.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using Cker.Models;
using Cker.Presenters;
using System.Windows.Media.Animation;

namespace CkerGUI
{
    /// <summary>
    /// RadarDisplay takes care of drawing vessels on screen.
    /// Uses WPF canvas and shapes; origin is center of canvas, positive up and right.
    /// </summary>
    public class RadarWidget
    {
        // Range of the radar in vessel units.
        public double Range { get; private set; }

        // Range of the radar in screen pixels.
        public double PixelRange { get; private set; }

        // Ratio of the min dimension to use as the pixel range.
        public double CanvasRatio { get; private set; }

        // Center position of the radar in screen pixels, relative to the canvas origin.
        // This will be the radar's origin.
        public double CenterX { get; private set; }
        public double CenterY { get; private set; }

        // Canvas to draw on.
        private Canvas canvas;

        // Current vessels being drawn.
        private IEnumerable<Vessel> currentVessels;

        // Vessel Presenter object to retreive alarm info
        private VesselPresenter vesselPresenter;

        /// <summary>
        /// Must specify the canvas to draw to on instantiation.
        /// 
        /// The range and canvas ratio determine the units scaling and size of the display.
        /// Range is the max range of the radar in whatever unit the vessels are using.
        /// Canvas ratio is the percentage of the lowest dimension of the ratio to use as the radar pixel radius.
        /// That pixel radius will then be matched with the desired range in vessel units.
        /// 
        /// </summary>
        /// <param name="radarCanvas"></param>
        /// <param name="range"></param>
        /// <param name="canvasRatio"></param>
        public RadarWidget(VesselPresenter presenter, Canvas radarCanvas, double range, double canvasRatio)
        {
            canvas = radarCanvas;
            Range = range;
            CanvasRatio = canvasRatio;
            currentVessels = null;
            vesselPresenter = presenter;
            vesselPresenter.AddUpdateAction(OnSimulationUpdate);

            canvas.SizeChanged += OnCanvasSizeChanged;

            CalculateCanvasProperties();
        }

        /// <summary>
        /// Refreshes the vessel drawing with the latest information.
        /// </summary>
        public void UpdateVessels()
        {
            // Refresh the canvas
            DrawVessels(vesselPresenter.DisplayedVessels);
        }

        /// <summary>
        /// Refreshes the alarm drawing with the latest information.
        /// </summary>
        public void UpdateAlarms()
        {
            foreach (var alarm in vesselPresenter.CurrentAlarms)
            {
                DrawAlarm(alarm.type, alarm.first);
                DrawAlarm(alarm.type, alarm.second);
            }
        }

        /// <summary>
        /// Draw specified alarm type for specified vessel.
        /// </summary>
        public void DrawAlarm(Cker.Simulator.AlarmType type, Vessel vessel)
        {
            if (!vesselPresenter.DisplayedVessels.Contains(vessel))
            {
                return;
            }

            // Animation used to scale.
            var animation = new DoubleAnimation(50.0, new Duration(TimeSpan.FromSeconds(0.5)), FillBehavior.Stop);
            var alarmColor = type == Cker.Simulator.AlarmType.Low ? Colors.Yellow : Colors.Red;

            Ellipse alarmVisual = new Ellipse();
            alarmVisual.Fill = new SolidColorBrush(alarmColor);
            alarmVisual.Opacity = type == Cker.Simulator.AlarmType.Low ? 0.3 : 0.5;
            alarmVisual.Width = 1;
            alarmVisual.Height = 1;
            alarmVisual.RenderTransformOrigin = new Point(0.5, 0.5);
            alarmVisual.RenderTransform = new ScaleTransform(1.0, 1.0);
            alarmVisual.RenderTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
            alarmVisual.RenderTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
            canvas.Children.Add(alarmVisual);
            Canvas.SetLeft(alarmVisual, ToPixelX(vessel.X));
            Canvas.SetTop(alarmVisual, ToPixelY(vessel.Y));
        }

        /// <summary>
        /// Draws the specified vessels on the canvas.
        /// Will only draw vessels that are within range.
        /// Each call will wipe the canvas and update with the new specified vessels.
        /// </summary>
        /// <param name="vessels"></param>
        public void DrawVessels(IEnumerable<Vessel> vessels)
        {
            if (vessels != null)
            {
                // Clear canvas
                canvas.Children.Clear();

                // Add shape for each vessel in range.
                foreach (var vessel in vessels)
                {
                    #region Vessel Tooltip

                    // Displays a tooltip with vessel information when the mouse is over the corresponding vessel
                    ToolTip vesselToolTip = new ToolTip();
                    vesselToolTip.Placement = PlacementMode.Right;
                    vesselToolTip.PlacementRectangle = new Rect(50, 0, 0, 0);
                    vesselToolTip.HorizontalOffset = 10;
                    vesselToolTip.VerticalOffset = 20;

                    //Create BulletDecorator and set it 
                    //as the tooltip content.
                    BulletDecorator bdec = new BulletDecorator();
                    TextBlock vesselDesc = new TextBlock();
                    vesselDesc.Text = "Vessel ID: " + vessel.ID + "\nType: " + vessel.Type.ToString() + "\nX-Pos: " + Math.Truncate(vessel.X) + ", Y-Pos: " + Math.Truncate(vessel.Y);
                    bdec.Child = vesselDesc;
                    vesselToolTip.Content = bdec;

                    #endregion

                    SolidColorBrush vesselColor = new SolidColorBrush();
                    vesselColor.Color = Color.FromRgb(0, 0, 255); // Orange Scheme: (255, 170, 0), Green Scheme: (0, 255, 1), Blue Scheme: (0, 0, 255)

                    // Vessels are assigned different shapes according to their Type. Undefined vessel types are given a default shape.
                    // Vessel Type: Human
                    if (vessel.Type.ToString() == "Human")
                    {
                        Polygon vesselVisual = new Polygon();

                        vesselVisual.Fill = vesselColor;

                        #region Draws Polygon shape for Human types

                        System.Windows.Point Point1 = new System.Windows.Point(10, 13);
                        System.Windows.Point Point2 = new System.Windows.Point(5, -2);
                        System.Windows.Point Point3 = new System.Windows.Point(0, 13);
                        System.Windows.Point Point4 = new System.Windows.Point(10, -2);
                        System.Windows.Point Point5 = new System.Windows.Point(0, -2);
                        PointCollection myPointCollection = new PointCollection();
                        myPointCollection.Add(Point1);
                        myPointCollection.Add(Point2);
                        myPointCollection.Add(Point3);
                        myPointCollection.Add(Point4);
                        myPointCollection.Add(Point5);
                        vesselVisual.Points = myPointCollection;

                        #endregion

                        canvas.Children.Add(vesselVisual);
                        Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                        Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);

                        vesselVisual.ToolTip = vesselToolTip;
                    }
                    // Vessel Type: SpeedBoat
                    else if (vessel.Type.ToString() == "SpeedBoat")
                    {
                        Ellipse vesselVisual = new Ellipse();

                        vesselVisual.Fill = vesselColor;

                        vesselVisual.Width = 10;
                        vesselVisual.Height = 10;

                        canvas.Children.Add(vesselVisual);
                        Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                        Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);

                        vesselVisual.ToolTip = vesselToolTip;
                    }
                    // Vessel Type: FishingBoat
                    else if (vessel.Type.ToString() == "FishingBoat")
                    {
                        Polygon vesselVisual = new Polygon();

                        vesselVisual.Fill = vesselColor;

                        #region Draws Polygon shape for FishingBoat types (triangle)

                        System.Windows.Point Point1 = new System.Windows.Point(12.5, 8);
                        System.Windows.Point Point2 = new System.Windows.Point(-2.5, 8);
                        System.Windows.Point Point3 = new System.Windows.Point(5, -2);
                        PointCollection myPointCollection = new PointCollection();
                        myPointCollection.Add(Point1);
                        myPointCollection.Add(Point2);
                        myPointCollection.Add(Point3);
                        vesselVisual.Points = myPointCollection;

                        #endregion

                        canvas.Children.Add(vesselVisual);
                        Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                        Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);

                        vesselVisual.ToolTip = vesselToolTip;
                    }
                    // Vessel Type: CargoVessel
                    else if (vessel.Type.ToString() == "CargoVessel")
                    {
                        Polyline vesselVisual = new Polyline();

                        vesselVisual.Fill = vesselColor;

                        #region Draws Polyline shape for CargoVessel types

                        System.Windows.Point Point1 = new System.Windows.Point(12.5, 0);
                        System.Windows.Point Point2 = new System.Windows.Point(-2.5, 0);
                        System.Windows.Point Point3 = new System.Windows.Point(12.5, 10);
                        System.Windows.Point Point4 = new System.Windows.Point(-2.5, 10);
                        PointCollection myPointCollection = new PointCollection();
                        myPointCollection.Add(Point1);
                        myPointCollection.Add(Point2);
                        myPointCollection.Add(Point3);
                        myPointCollection.Add(Point4);
                        vesselVisual.Points = myPointCollection;

                        #endregion

                        canvas.Children.Add(vesselVisual);
                        Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                        Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);

                        vesselVisual.ToolTip = vesselToolTip;
                    }
                    // Vessel Type: PassengerVessel
                    else if (vessel.Type.ToString() == "PassengerVessel")
                    {
                        Rectangle vesselVisual = new Rectangle();

                        vesselVisual.Fill = vesselColor;
                        vesselVisual.Width = 10;
                        vesselVisual.Height = 10;

                        canvas.Children.Add(vesselVisual);
                        Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                        Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);

                        vesselVisual.ToolTip = vesselToolTip;
                    }
                    // Vessel Type: Undefined/User-defined
                    else
                    {
                        Polyline vesselVisual = new Polyline();

                        vesselVisual.Fill = vesselColor;

                        #region Draws Polyline shape for Undefined types

                        System.Windows.Point Point1 = new System.Windows.Point(0, 12.5);
                        System.Windows.Point Point2 = new System.Windows.Point(0, -2.5);
                        System.Windows.Point Point3 = new System.Windows.Point(10, 12.5);
                        System.Windows.Point Point4 = new System.Windows.Point(10, -2.5);
                        PointCollection myPointCollection = new PointCollection();
                        myPointCollection.Add(Point1);
                        myPointCollection.Add(Point2);
                        myPointCollection.Add(Point3);
                        myPointCollection.Add(Point4);
                        vesselVisual.Points = myPointCollection;

                        #endregion

                        canvas.Children.Add(vesselVisual);
                        Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                        Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);

                        vesselVisual.ToolTip = vesselToolTip;
                    }

                    // Label vessel ID over
                    TextBlock vesselIdText = new TextBlock();
                    vesselIdText.Text = "" + vessel.ID;
                    vesselIdText.Foreground = new SolidColorBrush(Colors.Wheat);
                    canvas.Children.Add(vesselIdText);
                    Canvas.SetLeft(vesselIdText, ToPixelX(vessel.X));
                    Canvas.SetTop(vesselIdText, ToPixelY(vessel.Y));
                }

                // Keep track of current vessels.
                currentVessels = vessels;
            }
        }

        private double ToPixel(double vesselUnit)
        {
            double pixelUnit = (vesselUnit / Range) * PixelRange;
            return pixelUnit;
        }

        private double ToPixelX(double vesselUnit)
        {
            double pixelUnitX = ToPixel(vesselUnit) + CenterX;
            return pixelUnitX;
        }

        private double ToPixelY(double vesselUnit)
        {
            double pixelUnitY = -ToPixel(vesselUnit) + CenterY;
            return pixelUnitY;
        }

        private void OnCanvasSizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            // Recalculate properties
            CalculateCanvasProperties();

            // Redraw current vessels
            DrawVessels(currentVessels);
        }

        private void CalculateCanvasProperties()
        {
            // Calculate pixel range based on wanted ratio.
            double minDimension = Math.Min(canvas.ActualWidth, canvas.ActualHeight);
            PixelRange = CanvasRatio * minDimension / 2;

            // Calculate center
            CenterX = canvas.ActualWidth / 2;
            CenterY = canvas.ActualHeight / 2;
        }

        private void OnSimulationUpdate()
        {
            // Invoke update on the dispatcher thread to allow GUI operations.
            Application.Current.Dispatcher.Invoke(UpdateVessels);
            Application.Current.Dispatcher.Invoke(UpdateAlarms);
        }
    }
}
