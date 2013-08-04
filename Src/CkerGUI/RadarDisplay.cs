using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Cker.Models;

namespace CkerGUI
{
    /// <summary>
    /// RadarDisplay takes care of drawing vessels on screen.
    /// Uses WPF canvas and shapes; origin is center of canvas, positive up and right.
    /// </summary>
    public class RadarDisplay
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
        public RadarDisplay(Canvas radarCanvas, double range, double canvasRatio)
        {
            canvas = radarCanvas;
            Range = range;
            CanvasRatio = canvasRatio;
            currentVessels = null;

            canvas.SizeChanged += OnCanvasSizeChanged;

            CalculateCanvasProperties();
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
                    if (IsInRange(vessel.X, vessel.Y))
                    {
                        SolidColorBrush colorBrush = new SolidColorBrush();

                        // Make an overlay for the alarm range for testing purposes.
                        Ellipse alarmVisual = new Ellipse();
                        colorBrush = new SolidColorBrush();
                        colorBrush.Color = Color.FromRgb(255, 255, 0);
                        alarmVisual.Fill = colorBrush;
                        alarmVisual.Width = ToPixel(200);
                        alarmVisual.Height = ToPixel(200);
                        alarmVisual.Opacity = 0.4;

                        canvas.Children.Add(alarmVisual);
                        Canvas.SetLeft(alarmVisual, ToPixelX(vessel.X) - alarmVisual.Width / 2);
                        Canvas.SetTop(alarmVisual, ToPixelY(vessel.Y) - alarmVisual.Height / 2);

                        alarmVisual = new Ellipse();
                        colorBrush = new SolidColorBrush();
                        colorBrush.Color = Color.FromRgb(255, 0, 0);
                        alarmVisual.Fill = colorBrush;
                        alarmVisual.Width = ToPixel(50);
                        alarmVisual.Height = ToPixel(50);
                        alarmVisual.Opacity = 0.4;

                        canvas.Children.Add(alarmVisual);
                        Canvas.SetLeft(alarmVisual, ToPixelX(vessel.X) - alarmVisual.Width / 2);
                        Canvas.SetTop(alarmVisual, ToPixelY(vessel.Y) - alarmVisual.Height / 2);

                        colorBrush = new SolidColorBrush();
                        colorBrush.Color = Color.FromRgb(0, 0, 255);

                        // Vessels are assigned different shapes according to their Type. Undefined vessel types are given a default shape.
                        if (vessel.Type.ToString() == "Human")
                        {
                            Polygon vesselVisual = new Polygon();

                            vesselVisual.Fill = colorBrush;
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

                            canvas.Children.Add(vesselVisual);
                            Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                            Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);
                        }
                        else if (vessel.Type.ToString() == "SpeedBoat")
                        {
                            Ellipse vesselVisual = new Ellipse();

                            vesselVisual.Fill = colorBrush;
                            vesselVisual.Width = 10;
                            vesselVisual.Height = 10;

                            canvas.Children.Add(vesselVisual);
                            Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                            Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);
                        }
                        else if (vessel.Type.ToString() == "FishingBoat")
                        {
                            Polygon vesselVisual = new Polygon();

                            vesselVisual.Fill = colorBrush;
                            System.Windows.Point Point1 = new System.Windows.Point(12.5, 8);
                            System.Windows.Point Point2 = new System.Windows.Point(-2.5, 8);
                            System.Windows.Point Point3 = new System.Windows.Point(5, -2);
                            PointCollection myPointCollection = new PointCollection();
                            myPointCollection.Add(Point1);
                            myPointCollection.Add(Point2);
                            myPointCollection.Add(Point3);
                            vesselVisual.Points = myPointCollection;

                            canvas.Children.Add(vesselVisual);
                            Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                            Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);
                        }
                        else if (vessel.Type.ToString() == "CargoVessel")
                        {
                            Polyline vesselVisual = new Polyline();

                            vesselVisual.Fill = colorBrush;
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

                            canvas.Children.Add(vesselVisual);
                            Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                            Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);
                        }
                        else if (vessel.Type.ToString() == "PassengerVessel")
                        {
                            Rectangle vesselVisual = new Rectangle();

                            vesselVisual.Fill = colorBrush;
                            vesselVisual.Width = 10;
                            vesselVisual.Height = 10;

                            canvas.Children.Add(vesselVisual);
                            Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                            Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);
                        }
                        else
                        {
                            Polyline vesselVisual = new Polyline();

                            vesselVisual.Fill = colorBrush;
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

                            canvas.Children.Add(vesselVisual);
                            Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X) - 5);
                            Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y) - 5);
                        }
                    }
                }

                // Keep track of current vessels.
                currentVessels = vessels;
            }
        }

        private bool IsInRange(double posX, double posY)
        {
            bool isInRange = (posX * posX + posY * posY) <= (Range * Range);
            return isInRange;
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
    }
}
