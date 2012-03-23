using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

using SlimDX;
using SlimDX.Direct3D10;

namespace Direct3DExtensions
{
	public class Test3DEngine : Basic3DEngine
	{
		Sprite spriteGroup;
		List<SpriteInstance> allSprites = new List<SpriteInstance>();
		SpriteInstance statsSprite;
		ShaderResourceView statsView;
		Texture2D statsTexture;
		Bitmap statsImage;
		int framesSinceFpsUpdate = 1;
		float timeSinceFpsUpdate = 1;
		float framesPerSecond = 1;

		public string additionalStatistics { get; set; }

		public bool ShowStatistics { get { return allSprites.Contains(statsSprite); } set { SetShowStatistics(value); } }

		

		protected override void InitCameraInput()
		{
			base.InitCameraInput();
			this.HostControl.KeyDown += new System.Windows.Forms.KeyEventHandler(HostControl_KeyDown);
		}

		protected override void InitDevice()
		{
			base.InitDevice();

			spriteGroup = new SlimDX.Direct3D10.Sprite(D3DDevice.Device, 1);


			statsImage = new Bitmap(256, 256, PixelFormat.Format32bppArgb);

			statsTexture = CreateEmptyTexture(D3DDevice.Device, 256, 256);

			UpdateStats();

			statsView = new ShaderResourceView(D3DDevice.Device, statsTexture);
			D3DDevice.Device.PixelShader.SetShaderResource(statsView, 0);
			statsSprite = new SlimDX.Direct3D10.SpriteInstance(statsView, new Vector2(0, 0), new Vector2(1, 1));

			statsSprite.Transform = Matrix.Scaling(new Vector3(1,1,0)) * Matrix.Translation(new Vector3(-0.5f, 0.5f, 0));

			AddSprite(statsSprite);
		}


		private static void WriteToTexture(Texture2D tex, Bitmap bmp)
		{
			BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			int numBytes = data.Stride * bmp.Height;
			byte[] bytes = new byte[numBytes];

			Marshal.Copy(data.Scan0, bytes, 0, numBytes);
			bmp.UnlockBits(data);

			for (int i = 0; i < bytes.Length; i += 4)
			{
				byte temp = bytes[i];
				bytes[i] = bytes[i + 2];
				bytes[i + 2] = temp;
			}
			DataRectangle rect = tex.Map(0, MapMode.WriteDiscard, MapFlags.None);


			using (DataStream stream = rect.Data)
			{
				numBytes = Math.Min((int)stream.Length, numBytes);
				stream.Write(bytes, 0, numBytes);
			}

			tex.Unmap(0);
		}

		private static Texture2D CreateEmptyTexture(Device device, int width, int height)
		{
			Texture2DDescription texDesc = new Texture2DDescription()
			{
				ArraySize = 1,
				MipLevels = 1,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1, 0),
				Format = SlimDX.DXGI.Format.R8G8B8A8_UNorm,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None,
				BindFlags = BindFlags.ShaderResource,
				Usage = ResourceUsage.Dynamic,
				Height = height,
				Width = width
			};
			Texture2D tex = new Texture2D(device, texDesc);
			return tex;
		}

		public void AddSprite(SpriteInstance sprite)
		{
			allSprites.Add(sprite);
		}

		public void RemoveSprite(SpriteInstance sprite)
		{
			if (!allSprites.Contains(sprite)) return;
			allSprites.Remove(sprite);
		}

		protected override void Render()
		{
			base.Render();
			timeSinceFpsUpdate += CameraInput.TimeDelta;
			framesSinceFpsUpdate++;
			if (timeSinceFpsUpdate >= 3)
			{
				framesPerSecond = (float)framesSinceFpsUpdate / timeSinceFpsUpdate;
				UpdateStats();
				framesSinceFpsUpdate = 0;
				timeSinceFpsUpdate = 0;
			}
		}

		

		private void ScreenshotOnPrintScreen()
		{
			if (this.CameraInput.Input.IsKeyPressed(Keys.F12))
			{
				Direct3DExtensions.Texturing.ScreenCapture sc = new Direct3DExtensions.Texturing.ScreenCapture(this);
				sc.TakeScreenshotToClipboard();
			}
		}

		private void UpdateStats()
		{
			if (!ShowStatistics) return;
			using (Graphics g = Graphics.FromImage(statsImage))
			{
				g.Clear(Color.FromArgb(0, 0, 0, 0));
				Vector3 pos = CameraInput.Camera.Position;
				Vector3 ypr = CameraInput.Camera.YawPitchRoll;
				System.Drawing.Font font = new System.Drawing.Font("Arial", 10);
				g.DrawString("FPS: " + framesPerSecond.ToString("G3")+
				"\nCamPos: "+pos.X.ToString("G3")+","+pos.Y.ToString("G3")+","+pos.Z.ToString("G3")+
				"\nCamYaw: "+ypr.X.ToString("G3")+
				"\nCamPitch: "+ypr.Y.ToString("G3")+
				"\nIndices Drawn: "+D3DDevice.IndicesDrawn+
				"\n"+additionalStatistics
				, font, Brushes.Orange, new PointF(10, 10));
			}

			WriteToTexture(statsTexture, statsImage);
		}

		protected override void DrawGeometry()
		{
			base.DrawGeometry();
			
			D3DDevice.SetRasterizer(FillMode.Solid);
			spriteGroup.Begin(SpriteFlags.SortBackToFront);
			spriteGroup.DrawImmediate(allSprites.ToArray());
			spriteGroup.End();
			 
		}

		void HostControl_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F8)
				ToggleFillMode();
			if (e.KeyCode == Keys.F9)
				ToggleStatsDisplay();
			if (e.KeyCode == Keys.F12)
				ScreenshotOnPrintScreen();
		}

		public void ToggleFillMode()
		{
			if (D3DDevice.FillMode == SlimDX.Direct3D10.FillMode.Solid)
				D3DDevice.FillMode = SlimDX.Direct3D10.FillMode.Wireframe;
			else
				D3DDevice.FillMode = SlimDX.Direct3D10.FillMode.Solid;
		}

		void SetShowStatistics(bool showStats)
		{
			if (showStats == ShowStatistics) return;
			if (showStats)
				AddSprite(statsSprite);
			else
				RemoveSprite(statsSprite);
		}

		public void ToggleStatsDisplay()
		{
			ShowStatistics = !ShowStatistics;
		}


		void DisposeManaged() { if (statsImage != null) statsImage.Dispose(); statsImage = null; }
		void DisposeUnmanaged() {
			if (spriteGroup != null) spriteGroup.Dispose(); spriteGroup = null;
			if (statsTexture != null) statsTexture.Dispose(); statsTexture = null;
			if (statsView != null) statsView.Dispose(); statsView = null;
		}

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
    
	}
}
