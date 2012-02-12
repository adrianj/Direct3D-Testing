using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImageTiler
{
	/*
	 * Operates over a range of Zoom Levels to produce a mipmapped image.
	 * The output size is 50% longer than images produced using the FlatTiler.
	 */
	public class MipMapTiler : FlatTiler
	{
		public override System.Drawing.Image ConstructTiledImage(BackgroundWorker progressReporter)
		{
			Tiler flat = new FlatTiler();
			flat.ImageFetchFunction = this.ImageFetchFunction;
			flat.NumberOfTiles = this.NumberOfTiles;
			flat.ProgressOffset = 0;
			flat.ProgressRange = 100;
			flat.InvertY = this.InvertY;
			flat.ErrorReplacementImage = this.ErrorReplacementImage;
			flat.PreferredTileSize = this.PreferredTileSize;

			int numImages = 0;
			int minZoomLevel = this.MaxZoomLevel+1;
			for (int z = this.NumberOfTiles; z > 0; z /= 2)
			{
				flat.ProgressRange /= 2;
				numImages++;
				minZoomLevel--;
			}
			Console.WriteLine("minZoom = " + minZoomLevel);

			Size outputSize = CalculateOutputSize(minZoomLevel);
			Image img = new Bitmap(outputSize.Width * 3/2, outputSize.Height);

			using (Graphics g = Graphics.FromImage(img))
			{
				g.FillRectangle(Brushes.Gray, 0, 0, img.Width, img.Height);
				


				int imageNum = 1;
				int xOffset = initialSize.Width * this.NumberOfTiles;
				int yOffset = outputSize.Height - initialSize.Height;
				Image firstImage = null;
				for (int z = 1; z <= this.NumberOfTiles; z *= 2)
				{
					flat.NumberOfTiles = z;
					flat.MaxZoomLevel = this.MaxZoomLevel - numImages + imageNum;
					Size expectedSize = new Size(initialSize.Width * z, initialSize.Height * z);
					try
					{
						firstImage = flat.ConstructTiledImage(progressReporter);
					}
					catch (Exception ex)
					{
						Console.WriteLine("" + ex);
						if(firstImage != null)
							firstImage = new Bitmap(firstImage, firstImage.Width * 2, firstImage.Height * 2);
					}

					if (imageNum == numImages)
					{
						xOffset = 0;
					}
					else
						yOffset -= expectedSize.Height;
					g.DrawImage(firstImage, xOffset, yOffset, expectedSize.Width, expectedSize.Height);


					flat.MaxZoomLevel += 1;
					flat.ProgressOffset += flat.ProgressRange;
					flat.ProgressRange *= 2;
					flat.ErrorReplacementImage = firstImage;
					imageNum++;
				}
			}
			this.ReportProgress(progressReporter, 100);
			return img;
		}

	}
}
