using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;

namespace Rensoft.Drawing
{
    public class ImageShrinker
    {
        public void AutoShrink(
           Stream sourceStream,
           Size maximumSize,
           Stream targetStream,
           ImageFormat targetFormat)
        {
            AutoShrink(Image.FromStream(sourceStream), maximumSize, targetStream, targetFormat);
        }

        public void AutoShrink(
           Image source,
           Size maximumSize,
           Stream targetStream,
           ImageFormat targetFormat)
        {
            Size targetSize = ShrinkSize(source.Size, maximumSize);

            if (source.Size == targetSize)
            {
                // If source and target sizes equal, no action needed.
                ((Bitmap)source).Save(targetStream, targetFormat);
                return;
            }

            targetSize = ManualShrink(source, targetSize, targetStream, targetFormat);
        }

        public Size ManualShrink(
            Image source, 
            Size targetSize, 
            Stream targetStream,
            ImageFormat targetFormat)
        {
            // Create a base to draw image onto.
            Bitmap result = new Bitmap(
                targetSize.Width, targetSize.Height,
                PixelFormat.Format32bppArgb);

            // Use web-optimised standard.
            result.SetResolution(72, 72);

            // Get graphics handler from base image.
            Graphics graphics = Graphics.FromImage(result);
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            graphics.DrawImage(source,
                new Rectangle(Point.Empty, targetSize),
                new Rectangle(Point.Empty, source.Size),
                GraphicsUnit.Pixel);

            result.Save(targetStream, targetFormat);
            result.Dispose();
            graphics.Dispose();
            return targetSize;
        }

        public Size ShrinkSize(Size original, Size limit)
        {
            float scaleFactor;
            int targetWidth = 0;
            int targetHeight = 0;

            if (original.Width > original.Height)
            {
                // Shrink to fit height.
                scaleFactor = (float)original.Height / (float)limit.Height;
                targetHeight = limit.Height;
                targetWidth = (int)(original.Width / scaleFactor);

                if (targetWidth > limit.Width)
                {
                    // Shrink to fit width.
                    scaleFactor = (float)original.Width / (float)limit.Width;
                    targetWidth = limit.Width;
                    targetHeight = (int)(original.Height / scaleFactor);
                }
            }
            else
            {
                // Shrink to fit width.
                scaleFactor = (float)original.Width / (float)limit.Width;
                targetWidth = limit.Width;
                targetHeight = (int)(original.Height / scaleFactor);

                if (targetHeight > limit.Height)
                {
                    // Shrink to fit width.
                    scaleFactor = (float)original.Height / (float)limit.Height;
                    targetHeight = limit.Height;
                    targetWidth = (int)(original.Width / scaleFactor);
                }
            }

            return new Size(targetWidth, targetHeight);
        }
    }
}
