namespace Direct3DLib
{
	partial class Earth3DControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.engine = new Direct3DLib.Direct3DControl();
			this.SuspendLayout();
			// 
			// engine
			// 
			this.engine.AllowDrop = true;
			this.engine.BothMouseFunction = Direct3DLib.Direct3DControl.MouseOption.CameraTranslateXZ;
			this.engine.CameraLocation = new SlimDX.Vector3(0F, 12000F, 0F);
			this.engine.Dock = System.Windows.Forms.DockStyle.Fill;
			this.engine.KeyboardMovementSpeed = 5000F;
			this.engine.LeftMouseFunction = Direct3DLib.Direct3DControl.MouseOption.Select;
			this.engine.LightAmbientIntensity = 0.1F;
			this.engine.LightDirection = new SlimDX.Vector3(0.1F, 1F, 0F);
			this.engine.LightDirectionalIntensity = 0.9F;
			this.engine.Location = new System.Drawing.Point(0, 0);
			this.engine.MouseMovementSpeed = 5000F;
			this.engine.Name = "engine";
			this.engine.Pan = 0.7853982F;
			this.engine.RightMouseFunction = Direct3DLib.Direct3DControl.MouseOption.Rotate;
			this.engine.SelectedObject = null;
			this.engine.Size = new System.Drawing.Size(667, 432);
			this.engine.TabIndex = 0;
			this.engine.TextureImageFilenames = new string[0];
			this.engine.Tilt = -1.570796F;
			this.engine.ZClipFar = 1000000F;
			this.engine.ZClipNear = 5F;
			this.engine.Zoom = 5F;
			this.engine.CameraChanged += new System.EventHandler(this.engine_CameraChanged);
			this.engine.DragDrop += new System.Windows.Forms.DragEventHandler(this.engine_DragDrop);
			// 
			// Earth3DControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.Controls.Add(this.engine);
			this.Name = "Earth3DControl";
			this.Size = new System.Drawing.Size(667, 432);
			this.Load += new System.EventHandler(this.Earth3DControl_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private Direct3DLib.Direct3DControl engine;
	}
}
