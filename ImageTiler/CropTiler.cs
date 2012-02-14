using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;

namespace ImageTiler
{
	public class CropTiler : FlatTiler
	{
		public Point OffsetIntoLargerImage { get; set; }

		public override System.Drawing.Image ConstructTiledImage(BackgroundWorker progressReporter)
		{
			Image image = FetchImage(this.MaxZoomLevel, 0, 0);
			Size outputSize = new Size(image.Width / this.NumberOfTiles, image.Height / this.NumberOfTiles);
			Image output = new Bitmap(outputSize.Width, outputSize.Height);
			using (Graphics g = Graphics.FromImage(output))
			{
				Rectangle destRect = new Rectangle(new Point(0, 0), outputSize);
				Point offsetInPixels = new Point(OffsetIntoLargerImage.X * outputSize.Width, OffsetIntoLargerImage.Y * outputSize.Height);
				Rectangle srcRect = new Rectangle(offsetInPixels, outputSize);
				g.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);
			}
			return output;
		}
	}
}
