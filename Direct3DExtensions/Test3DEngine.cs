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

	public class Test3DEngine : Sprite3DEngine
	{
		Texturing.SpriteTexture statsSprite;
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
			statsImage = new Bitmap(256, 256, PixelFormat.Format32bppArgb);
			UpdateStats();

			CreateStatsSprite();
		}

		private void CreateStatsSprite()
		{
			statsSprite = new Texturing.SpriteTexture(D3DDevice.Device, 256, 256);
			D3DDevice.Device.PixelShader.SetShaderResource(statsSprite.View, 0);
			AddSprite(statsSprite);
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
				sc.CopyScreenshotToClipboard();
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
			statsSprite.WriteTexture(statsImage);
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
		void DisposeUnmanaged() { }

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
