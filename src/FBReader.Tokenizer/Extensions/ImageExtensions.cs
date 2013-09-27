/*
 * Author: CactusSoft (http://cactussoft.biz/), 2013
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA
 * 02110-1301, USA.
 */

using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using FBReader.Tokenizer.Data;

namespace FBReader.Tokenizer.Extensions
{
    public static class BitmapImageExtension
    {
        public static Size GetImageSize(this Stream imageStream)
        {
            var image = new BitmapImage();
            image.SetSource(imageStream);
            var size = new Size(image.PixelWidth, image.PixelHeight);
            image.ClearValue(BitmapImage.UriSourceProperty);
            return size;
        }

        public static void SaveJpeg(this BitmapSource bitmap, Stream output, int width, int height, bool saveRatio)
        {
            int pixelWidth;
            int pixelHeight;
            double actualFactor = bitmap.PixelWidth/((double) bitmap.PixelHeight);
            double desiredFactor = width/((double) height);
            if (saveRatio)
            {
                if (actualFactor < desiredFactor)
                {
                    pixelWidth = bitmap.PixelWidth;
                    pixelHeight = (int) (pixelWidth/desiredFactor);
                }
                else
                {
                    pixelHeight = bitmap.PixelHeight;
                    pixelWidth = (int) (pixelHeight*desiredFactor);
                }
            }
            else
            {
                pixelWidth = bitmap.PixelWidth;
                pixelHeight = bitmap.PixelHeight;
            }
            var bitmap2 = new WriteableBitmap(bitmap);
            bitmap2.Invalidate();
            var bitmap3 = new WriteableBitmap(pixelWidth, pixelHeight);
            if (pixelWidth == bitmap.PixelWidth)
            {
                Buffer.BlockCopy(bitmap2.Pixels, 0, bitmap3.Pixels, 0, (pixelWidth*pixelHeight)*4);
            }
            else
            {
                int srcOffset = 0;
                int dstOffset = 0;
                for (int i = 0; i < pixelHeight; i++)
                {
                    Buffer.BlockCopy(bitmap2.Pixels, srcOffset, bitmap3.Pixels, dstOffset, pixelWidth*4);
                    srcOffset += bitmap.PixelWidth*4;
                    dstOffset += pixelWidth*4;
                }
            }
            bitmap3.SaveJpeg(output, width, height, 0, 100);
        }

        public static Size FitToSize(this BookImage @this, Size pageSize)
        {
            double width = @this.Width;
            double height = @this.Height;
            if (height > pageSize.Height)
            {
                double num = height / pageSize.Height;
                height /= num;
                width /= num;
            }
            if (width > pageSize.Width)
            {
                double num = width / pageSize.Width;
                height /= num;
                width /= num;
            }
            return new Size((int)width, (int)height);
        }
    }
}