﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

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
        private IEnumerable<Cker.Server.TargetRecord> currentVessels;

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
        public void DrawVessels(IEnumerable<Cker.Server.TargetRecord> vessels)
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
                        // For now just add them all as circles.
                        Ellipse vesselVisual = new Ellipse();
                        SolidColorBrush colorBrush = new SolidColorBrush();
                        colorBrush.Color = Color.FromRgb(0, 0, 255);
                        vesselVisual.Fill = colorBrush;
                        vesselVisual.Width = 10;
                        vesselVisual.Height = 10;

                        canvas.Children.Add(vesselVisual);
                        Canvas.SetLeft(vesselVisual, ToPixelX(vessel.X));
                        Canvas.SetTop(vesselVisual, ToPixelY(vessel.Y));
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