namespace Direct3DLib
{
	partial class BasicForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.direct3DControl1 = new Direct3DLib.Direct3DControl();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// direct3DControl1
			// 
			this.direct3DControl1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
			this.direct3DControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.direct3DControl1.BothMouseFunction = Direct3DLib.Direct3DControl.MouseOption.CameraTranslateXZ;
			this.direct3DControl1.KeyboardMovementSpeed = 50F;
			this.direct3DControl1.LeftMouseFunction = Direct3DLib.Direct3DControl.MouseOption.Select;
			this.direct3DControl1.LightAmbientIntensity = 0.3F;
			this.direct3DControl1.LightDirection = new SlimDX.Vector3(1F, 1F, 1F);
			this.direct3DControl1.LightDirectionalIntensity = 0.7F;
			this.direct3DControl1.Location = new System.Drawing.Point(35, 142);
			this.direct3DControl1.MouseMovementSpeed = 50F;
			this.direct3DControl1.Name = "direct3DControl1";
			this.direct3DControl1.Pan = 0.7F;
			this.direct3DControl1.RightMouseFunction = Direct3DLib.Direct3DControl.MouseOption.Rotate;
			this.direct3DControl1.SelectedObject = null;
			this.direct3DControl1.Size = new System.Drawing.Size(611, 470);
			this.direct3DControl1.TabIndex = 0;
			this.direct3DControl1.CameraLocation = new SlimDX.Vector3(-2F, 2F, -2F);
			this.direct3DControl1.TextureImageFilenames = new string[] {
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png",
        "textures\\NoTexture.png"};
			this.direct3DControl1.Tilt = -0.7F;
			this.direct3DControl1.ZClipFar = 100F;
			this.direct3DControl1.ZClipNear = 0.1F;
			this.direct3DControl1.Zoom = 5F;
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(35, 13);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(611, 107);
			this.textBox1.TabIndex = 1;
			// 
			// BasicForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(725, 656);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.direct3DControl1);
			this.Name = "BasicForm";
			this.Text = "BasicForm";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Direct3DControl direct3DControl1;
		private System.Windows.Forms.TextBox textBox1;
	}
}