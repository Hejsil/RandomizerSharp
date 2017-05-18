using System;
using System.Collections.Generic;
using System.Drawing;
using ImageProcessor;
using ImageProcessor.Imaging;

namespace RandomizerSharp
{
    public class GfxFunctions
    {
        public static Bitmap DrawTiledImage(byte[] data, int[] palette, int width, int height, int bpp)
        {
            return DrawTiledImage(data, palette, 0, width, height, 8, 8, bpp);
        }

        public static Bitmap DrawTiledImage(byte[] data, int[] palette, int offset, int width, int height, int bpp)
        {
            return DrawTiledImage(data, palette, offset, width, height, 8, 8, bpp);
        }

        public static Bitmap DrawTiledImage(byte[] data, int[] palette, int offset, int width, int height,
            int tileWidth, int tileHeight, int bpp)
        {
            if (bpp != 1 && bpp != 2 && bpp != 4 && bpp != 8)
                throw new ArgumentException("Bits per pixel must be a multiple of 2.");

            var pixelsPerByte = 8 / bpp;

            if (width * height / pixelsPerByte + offset > data.Length)
                throw new ArgumentException("Invalid input image.");

            var bytesPerTile = tileWidth * tileHeight / pixelsPerByte;
            var numTiles = width * height / (tileWidth * tileHeight);
            var widthInTiles = width / tileWidth;

            var result = new Bitmap(width, height);
            var bim = new FastBitmap(result);

            for (var tile = 0; tile < numTiles; tile++)
            {
                var tileX = tile % widthInTiles;
                var tileY = tile / widthInTiles;
                for (var yT = 0; yT < tileHeight; yT++)
                for (var xT = 0; xT < tileWidth; xT++)
                {
                    var value =
                        data[tile * bytesPerTile + yT * tileWidth / pixelsPerByte + xT / pixelsPerByte + offset] & 0xFF;
                    if (pixelsPerByte != 1)
                        value = (int) ((uint) value >> (xT % pixelsPerByte * bpp)) & ((1 << bpp) - 1);
                    bim.SetPixel(tileX * tileWidth + xT, tileY * tileHeight + yT, Color.FromArgb(palette[value]));
                }
            }

            return bim;
        }

        public static int Conv16BitColorToArgb(int palValue)
        {
            var red = (int) ((palValue & 0x1F) * 8.25);
            var green = (int) (((palValue & 0x3E0) >> 5) * 8.25);
            var blue = (int) (((palValue & 0x7C00) >> 10) * 8.25);
            return unchecked((int) 0xFF000000) | (red << 16) | (green << 8) | blue;
        }

        public static void PseudoTransparency(Bitmap img, int transColor)
        {
            var width = img.Width;
            var height = img.Height;
            var visitPixels = new LinkedList<int>();

            var queued = new bool[width, height];

            for (var x = 0; x < width; x++)
            {
                QueuePixel(x, 0, width, height, visitPixels, queued);
                QueuePixel(x, height - 1, width, height, visitPixels, queued);
            }

            for (var y = 0; y < height; y++)
            {
                QueuePixel(0, y, width, height, visitPixels, queued);
                QueuePixel(width - 1, y, width, height, visitPixels, queued);
            }

            while (visitPixels.Count > 0)
            {
                var nextPixel = visitPixels.First.Value;
                visitPixels.RemoveFirst();

                var x = nextPixel % width;
                var y = nextPixel / width;
                if (img.GetPixel(x, y) == Color.FromArgb(transColor))
                {
                    img.SetPixel(x, y, Color.Empty);
                    QueuePixel(x - 1, y, width, height, visitPixels, queued);
                    QueuePixel(x + 1, y, width, height, visitPixels, queued);
                    QueuePixel(x, y - 1, width, height, visitPixels, queued);
                    QueuePixel(x, y + 1, width, height, visitPixels, queued);
                }
            }
        }

        private static void QueuePixel(int x, int y, int width, int height, LinkedList<int> queue, bool[,] queued)
        {
            if (x >= 0 && x < width && y >= 0 && y < height && !queued[x, y])
            {
                queue.AddLast(y * width + x);
                queued[x, y] = true;
            }
        }
    }
}