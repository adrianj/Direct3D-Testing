using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImageTiler
{
	public delegate Point CropOffsetFunction(int largeZoomLevel, int thisZoomLevel);
	/*
	 * Operates over a range of Zoom Levels to produce a mipmapped image.
	 * The output size is 50% longer than images produced using the FlatTiler.
	 */
	public class MipMapTiler : FlatTiler
	{
		public int MinZoomLevel { get; set; }
		public CropOffsetFunction CropOffsetFunction { get; set; }

		Tiler flat = new FlatTiler();
		CropTiler crop = new CropTiler();
		Size outputSize;
		int minFlatZoomLevel;

		public override System.Drawing.Image ConstructTiledImage(BackgroundWorker progressReporter)
		{
			flat.ImageFetchFunction = this.ImageFetchFunction;
			flat.NumberOfTiles = this.NumberOfTiles;
			flat.ProgressOffset = 0;
			flat.ProgressRange = 100;
			flat.InvertY = this.InvertY;
			flat.ErrorReplacementImage = this.ErrorReplacementImage;
			flat.PreferredTileSize = this.PreferredTileSize;
			crop.ImageFetchFunction = this.ImageFetchFunction;

			int minZoomLevel = this.MinZoomLevel;
			if (minZoomLevel == 0 || minZoomLevel > MaxZoomLevel)
			{
				minZoomLevel = this.MaxZoomLevel + 1;
				for (int z = this.NumberOfTiles; z > 0; z /= 2)
				{
					flat.ProgressRange /= 2;
					minZoomLevel--;
				}
			}
			minFlatZoomLevel = MaxZoomLevel;
			int numTiles = this.NumberOfTiles;
			for (int i = MaxZoomLevel; i > minZoomLevel; i--)
			{
				numTiles /= 2;
				if (numTiles == 0)
					break;
				minFlatZoomLevel--;
			}

			Console.WriteLine("minZoom: " + minZoomLevel + ", minFlatZoom: " + minFlatZoomLevel + ", maxZoom: " + MaxZoomLevel);

			outputSize = CalculateOutputSize(minFlatZoomLevel);
			Image outputImage = new Bitmap(outputSize.Width * 3/2, outputSize.Height);

			DrawTiledImages(progressReporter, minFlatZoomLevel, this.MaxZoomLevel, outputImage);
			DrawCroppedImages(progressReporter, minZoomLevel, minFlatZoomLevel - 1, outputImage);
			this.ReportProgress(progressReporter, 100);
			return outputImage;
		}

		private void DrawTiledImages(BackgroundWorker progressReporter, int minZoomLevel, int maxZoomLevel, Image outputImage)
		{
			using (Graphics g = Graphics.FromImage(outputImage))
			{
				g.FillRectangle(Brushes.Gray, 0, 0, outputImage.Width, outputImage.Height);

				Image image = null;
				for(int zoomLevel = minZoomLevel; zoomLevel <= maxZoomLevel; zoomLevel++)
				//for (int numTiles = 1; numTiles <= this.NumberOfTiles; numTiles *= 2)
				{
					int zoomDiff = maxZoomLevel - zoomLevel;
					int numTiles = this.NumberOfTiles >> zoomDiff;

					Size expectedSize = new Size(initialSize.Width * numTiles, initialSize.Height * numTiles);
					flat.MaxZoomLevel = zoomLevel;
					flat.NumberOfTiles = numTiles;
					flat.ErrorReplacementImage = image;
					Point offsets = GetMipMapOffsets(zoomLevel, numTiles);
					image = FetchFlatImage(progressReporter, flat);


					g.DrawImage(image, offsets.X, offsets.Y, expectedSize.Width, expectedSize.Height);


					flat.MaxZoomLevel += 1;
					flat.ProgressOffset += flat.ProgressRange;
					flat.ProgressRange *= 2;
					this.ErrorReplacementImage = image;
				}
			}
			
		}


		private void DrawCroppedImages(BackgroundWorker progressReporter, int minZoomLevel, int maxZoomLevel, Image outputImage)
		{
			using (Graphics g = Graphics.FromImage(outputImage))
			{
				for (int zoomLevel = maxZoomLevel; zoomLevel >= minZoomLevel; zoomLevel--)
				{
					int zoomDiff = minFlatZoomLevel - zoomLevel;
					int numTiles = (1 << zoomDiff);
					Point offsets = GetMipMapOffsets(zoomLevel, -(numTiles>>1));
					Size expectedSize = new Size(initialSize.Width / numTiles, initialSize.Height / numTiles);
					crop.NumberOfTiles = numTiles;
					crop.MaxZoomLevel = zoomLevel;
					if (this.CropOffsetFunction != null)
					{
						Point o =  this.CropOffsetFunction(zoomLevel,minFlatZoomLevel);
						if (this.InvertY)
							o.Y = numTiles - o.Y-1;
						crop.OffsetIntoLargerImage = o;
						Console.WriteLine("Crop Offset: " + crop.OffsetIntoLargerImage);
					}
					Console.WriteLine("numTiles: " + numTiles + ", zoomLevel: " + zoomLevel + ", offsets: " + offsets);
					Image image = FetchCroppedImage(progressReporter, crop);
					this.ErrorReplacementImage = image;
					g.DrawImage(image, offsets.X, offsets.Y, expectedSize.Width, expectedSize.Height);
				}
			}
		}

		private Point GetMipMapOffsets(int zoomLevel, int numTiles)
		{
			Point ret = new Point();
			if (zoomLevel != this.MaxZoomLevel)
			{
				ret.X = this.NumberOfTiles * initialSize.Width;
				if (numTiles >= 0)
					ret.Y = (this.NumberOfTiles - (numTiles << 1)) * initialSize.Height;
				else
				{
					double yoff = (double)this.NumberOfTiles + 1.0 / (double)numTiles;
					ret.Y = (int)(yoff * (double)initialSize.Height);
				}
			}
			return ret;
		}

		private Image FetchCroppedImage(BackgroundWorker progressReporter, CropTiler tiler)
		{
			try
			{
				return tiler.ConstructTiledImage(progressReporter);
			}
			catch (Exception ex)
			{
				Console.WriteLine("" + ex);
				if (this.ErrorReplacementImage != null)
				{
					return new Bitmap(ErrorReplacementImage, ErrorReplacementImage.Width / 2, ErrorReplacementImage.Height / 2);
				}
				else
					throw new Exception("Previous image was null.", ex);
			}
		}

		private Image FetchFlatImage(BackgroundWorker progressReporter, Tiler tiler)
		{
			try
			{
				return tiler.ConstructTiledImage(progressReporter);
			}
			catch (Exception ex)
			{
				Console.WriteLine("" + ex);
				if (this.ErrorReplacementImage != null)
				{
					return new Bitmap(ErrorReplacementImage, ErrorReplacementImage.Width * 2, ErrorReplacementImage.Height * 2);
				}
				else
					throw new Exception("Previous image was null.", ex);
			}
		}


	}
}
