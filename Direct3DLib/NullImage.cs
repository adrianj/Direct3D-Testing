using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;

namespace Direct3DLib
{
	public class NullImage
	{
		private Bitmap image;
		public Image ImageClone
		{
			get
			{
				if (updateRequired) Redraw();
				return image.Clone(new Rectangle(0, 0, image.Width, image.Height), image.PixelFormat);
			}
		}
		public Size Size { get { return image.Size; } }

		private Color backgroundColor;
		public Color BackgroundColor { get { return backgroundColor; } set { backgroundColor = Color.FromArgb(64, value); updateRequired = true; } }

		private Color textColor;
		public Color TextColor { get { return textColor; } set { textColor = Color.FromArgb(64, value); updateRequired = true; } }

		private string text = "Null Image";
		public string Text { get { return text; } set { text = value; updateRequired = true; } }

		private bool updateRequired = true;

		public NullImage() : this(new Size(256,256), Color.LightGreen) { }
		public NullImage(Size size, Color backgroundColor)
		{
			image = new Bitmap(size.Width, size.Height, PixelFormat.Format32bppArgb);
			BackgroundColor = backgroundColor;
			TextColor = Color.DarkBlue;
			updateRequired = true;
		}

		private void Redraw()
		{
			using (Graphics g = Graphics.FromImage(image))
			{
				g.Clear(backgroundColor);
				RectangleF rect = new RectangleF(0.0f, 0.0f, Size.Width, Size.Height);
				//g.FillRectangle(new SolidBrush(backgroundColor), rect);
				StringFormat format = new StringFormat() { Alignment = StringAlignment.Center };
				g.DrawString(text, SystemFonts.DefaultFont, new SolidBrush(textColor), rect, format);
			}
			updateRequired = false;
		}
	}
}
