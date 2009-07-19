using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Rensoft.Drawing
{
    public class ImageCropper
    {
        private const int defaultDpi = 72;

        private bool enableBoundsOverflow;
        private Color background;
        private int dpi = defaultDpi;

        public int Dpi
        {
            get { return dpi; }
            set { dpi = value; }
        }

        public Color Background
        {
            get { return background; }
            set { background = value; }
        }

        protected Brush BackgroundBrush
        {
            get { return new Pen(background).Brush; }
        }

        /// <summary>
        /// Gets or sets wether or not the image can expand beyond the width or height bounds.
        /// </summary>
        public bool EnableBoundsOverflow
        {
            get { return enableBoundsOverflow; }
            set { enableBoundsOverflow = value; }
        }

        public ImageCropper() { }

        public ImageCropper(bool fillBounds, Color background)
        {
            this.enableBoundsOverflow = fillBounds;
            this.background = background;
        }

        public void Crop(
            Stream sourceStream,
            Size targetSize,
            Stream targetStream,
            ImageFormat targetFormat)
        {
            Image source = Image.FromStream(sourceStream);

            int sourceWidth = source.Width;
            int sourceHeight = source.Height;
            int destX = 0;
            int destY = 0;

            // Calculate relational width/height percentages.
            float aspectWidth = (float)targetSize.Width / (float)sourceWidth;
            float aspectHeight = (float)targetSize.Height / (float)sourceHeight;

            float percent = 0;
            
            // Is the height smaller than the height?
            if (aspectHeight < aspectWidth)
            {
                if (enableBoundsOverflow)
                {
                    // Zoom to fit the entire height.
                    percent = aspectHeight;

                    // Move vertically to left edge.
                    destX += (int)((targetSize.Width - (sourceWidth * percent)) / 2);
                }
                else
                {
                    // Zoom so that image is only as big as the width.
                    percent = aspectWidth;

                    // Move horizontally to top edge.
                    destY += (int)((targetSize.Height - (sourceHeight * percent)) / 2);
                }
            }
            else
            {
                if (enableBoundsOverflow)
                {
                    // Zoom to the full width of the image.
                    percent = aspectWidth;

                    // Move horizontally to top edge.
                    destY += (int)((targetSize.Height - (sourceHeight * percent)) / 2);
                }
                else
                {
                    // Zoom so that image is only as big as the height.
                    percent = aspectHeight;

                    // Move vertically to left edge.
                    destX += (int)((targetSize.Width - (sourceWidth * percent)) / 2);
                }
            }

            // Resize width and height proportionally.
            int destWidth = (int)(sourceWidth * percent);
            int destHeight = (int)(sourceHeight * percent);

            if (source.Size.Equals(new Size(destWidth, destWidth)))
            {
                // If source and target sizes equal, no action needed.
                ((Bitmap)source).Save(targetStream, targetFormat);
                return;
            }

            Rectangle destBounds = new Rectangle(destX, destY, destWidth, destHeight);
            Crop(source, targetStream, targetFormat, destBounds, targetSize);
        }

        public void Crop(
            Image source, 
            Stream targetStream, 
            ImageFormat targetFormat,
            Rectangle targetBounds, 
            Size targetSize)
        {
            // Create a base to draw image onto.
            Bitmap result = new Bitmap(
                targetSize.Width, targetSize.Height,
                PixelFormat.Format32bppArgb);

            result.SetResolution(dpi, dpi);

            // Get graphics handler from base image.
            Graphics graphics = Graphics.FromImage(result);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.CompositingQuality = CompositingQuality.HighQuality;

            if (enableBoundsOverflow)
            {
                // Fill the entire bitmap with specified background colour where overflow enabled.
                graphics.FillRectangle(
                    BackgroundBrush, 
                    0, 0,
                    targetSize.Width,
                    targetSize.Height);
            }

            // Apply padding to hide blended edges.
            targetBounds.X -= 2;
            targetBounds.Y -= 2;
            targetBounds.Width += 4;
            targetBounds.Height += 4;

            Rectangle sourceBounds = new Rectangle(Point.Empty, source.Size);

            // Draw source onto base using graphics handler.
            graphics.DrawImage(source, targetBounds, sourceBounds, GraphicsUnit.Pixel);

            result.Save(targetStream, targetFormat);
            result.Dispose();
            graphics.Dispose();
        }

        public Rectangle GetBounds(Point startPoint, float zoom, Size sourceSize, Size targetSize)
        {
            float percent = 0;
            int destX = startPoint.X;
            int destY = startPoint.Y;

            // Calculate relational width/height percentages.
            float percentWidth = (float)targetSize.Width / (float)sourceSize.Width;
            float percentHeight = (float)targetSize.Height / (float)sourceSize.Height;

            if (percentHeight < percentWidth)
            {
                // Move horizontally to top edge.
                percent = percentWidth;
                //destY += (int)((cropSize.Height - (scaledImage.Height * percent)) / 2);
            }
            else
            {
                // Move vertically to left edge.
                percent = percentHeight;
                //destX += (int)((cropSize.Width - (scaledImage.Width * percent)) / 2);
            }

            percent *= zoom;

            // Resize width and height proportionally.
            int destWidth = (int)(sourceSize.Width * percent);
            int destHeight = (int)(sourceSize.Height * percent);

            Rectangle rectangle = new Rectangle();

            rectangle.Width = destWidth;
            rectangle.Height = destHeight;

            // Limit top left position to below 0.
            if (destX < 0) rectangle.X = destX; 
            if (destY < 0) rectangle.Y = destY;

            // Once X has changed, check Right is not out of bounds.
            while (rectangle.Right < targetSize.Width)
            {
                rectangle.X++;
            }

            // Once Y has changed, check Bottom is not out of bounds.
            while (rectangle.Bottom < targetSize.Height)
            {
                rectangle.Y++;
            }

            return rectangle;
        }
    }
}
