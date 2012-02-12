using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace ImageTiler
{
	public delegate Image ImageFetchFunction(int zoomLevel, int tileX, int tileY);

	public interface Tiler : PartialProgressReporter
	{
		/* Invert Image along Y Axis */
		bool InvertY { get; set; }
		/* Zoom level of largest, most detailed part of the image */
		int MaxZoomLevel { get; set; }
		/* Number of original tiles to fit in each row/column of output image.*/
		int NumberOfTiles { get; set; }
		/* Size of input image tiles. Leave at (0,0) to use actual size of first input image */
		Size PreferredTileSize { get; set; }
		/* A function that takes zoom level, x offset and y offset and expects an Image in return. (eg, use z,x,y to calculate filename) */
		ImageFetchFunction ImageFetchFunction { get; set; }
		/* Image to index in to as a substitute if an error occurs. (eg, use an image from 1 zoom level lower) */
		Image ErrorReplacementImage { get; set; }
		/* Instruct Tiler to do it's thing. */
		Image ConstructTiledImage();
		/* Instruct Tiler to do it's thing, with background worker reporting progress if running async. */
		Image ConstructTiledImage(BackgroundWorker progressReporter);
	}

	/*
	 * Use to construct a simple Flat Tiled Image, that is where output width/height is equal to input width/height * MaxTileWidth.
	 */
	public class FlatTiler : SimpleProgressReporter, Tiler
	{
		public virtual bool InvertY { get; set; }
		public int MaxZoomLevel { get; set; }
		public int NumberOfTiles { get; set; }
		public ImageFetchFunction ImageFetchFunction { get; set; }
		public Image ErrorReplacementImage { get; set; }
		public Size PreferredTileSize { get; set; }

		protected Size initialSize;

		public virtual Image ConstructTiledImage(BackgroundWorker progressReporter)
		{
			if (ImageFetchFunction == null) throw new ArgumentNullException("Must specify ImageFetchFunction.");
			Size outputSize = CalculateOutputSize(MaxZoomLevel);
			Image img = new Bitmap(outputSize.Width, outputSize.Height);
			//Image img = new Bitmap(firstImage, outputSize);
			this.ProgressIncrement = (double)100 / (double)(NumberOfTiles * NumberOfTiles);
			ReportProgress(progressReporter, 0);
			using(Graphics g = Graphics.FromImage(img))
			{
			for(int x = 0; x < NumberOfTiles; x++)
				for (int y = 0; y < NumberOfTiles; y++)
				{
					Image temp = FetchImage(MaxZoomLevel, x, y);
					//if (temp.Size != initialSize)
					//	throw new ArgumentException("Image tile is not consistent size: "+MaxZoomLevel+", "+x+", "+y);
					int yOffset = y * initialSize.Height;
					if (this.InvertY)
					{
						yOffset = initialSize.Height * (NumberOfTiles - y - 1);
					}
					g.DrawImage(temp, x * initialSize.Width, yOffset,initialSize.Width, initialSize.Height);
					temp.Dispose();
					ReportIncrementalProgress(progressReporter);
				}
			}
			return img;
		}

		protected virtual Size CalculateOutputSize(int zoomLevel)
		{
			initialSize = PreferredTileSize;
			if (initialSize.Width < 1 || initialSize.Height < 1)
			{
				Image firstImage = FetchImage(zoomLevel, 0, 0);
				initialSize = firstImage.Size;
			}
			Size outputSize = new Size(initialSize.Width * NumberOfTiles, initialSize.Height * NumberOfTiles);
			return outputSize;
		}

		protected virtual Image FetchImage(int zoomLevel, int x, int y)
		{
			string err = "";
			try
			{
				Image firstImage = ImageFetchFunction(zoomLevel, x, y);
				return firstImage;
			}
			catch (Exception ex)
			{
				err = ""+ex.GetType()+", "+ex.Message;
				if (this.ErrorReplacementImage == null)
				{
					throw new Exception("ImageFetchError, No replacement: " + err, ex);
				}
				if (initialSize.Width < 1 || initialSize.Height < 1)
				{
					throw new Exception("ImageFetchError, PreferredSize Not Specified: " + err, ex);
				}
			}
			Console.WriteLine("Using error image: "+err+", "+initialSize);
			Image ret = new Bitmap(initialSize.Width, initialSize.Height);
			using (Graphics g = Graphics.FromImage(ret))
			{
				Rectangle destRect = new Rectangle(0, 0, ret.Width, ret.Height);
				int n = this.NumberOfTiles;
				int srcLeft = initialSize.Height * x / 2;
				int srcTop = initialSize.Width * y / 2;
				int srcWidth = initialSize.Width / 2;
				int srcHeight = initialSize.Height / 2;
				Rectangle srcRect = new Rectangle(srcLeft,srcTop, srcWidth, srcHeight);
				if (this.InvertY)
				{
					Console.WriteLine("SrcRect (pre invert): " + srcRect+",x="+x+",y="+y+"     "+ErrorReplacementImage.Size);
					//srcRect.Location = new Point(srcRect.Left, srcRect.Bottom - initialSize.Height * y / 2);
					srcTop = (initialSize.Height*n - initialSize.Height * (y+1)) / 2;
					srcRect = new Rectangle(srcLeft, srcTop, srcWidth, srcHeight);
				}
				Console.WriteLine("SrcRect: " + srcRect);
				g.DrawImage(ErrorReplacementImage, destRect, srcRect, GraphicsUnit.Pixel);
			}
			return ret;
		}

		public virtual Image ConstructTiledImage()
		{
			BackgroundWorker nonReporter = new BackgroundWorker() { WorkerReportsProgress = false };
			return ConstructTiledImage(nonReporter);
		}
	}
}
