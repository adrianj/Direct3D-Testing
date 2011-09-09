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
			this.Compass = new Direct3DLib.Compass();
			this.grid = new Direct3DLib.shapes.Grid();
			this.SuspendLayout();
			// 
			// Compass
			// 
			this.Compass.CanPick = true;
			this.Compass.Location = new SlimDX.Vector3(60F, 60F, 0F);
			this.Compass.Name = "Compass";
			this.Compass.Rotation = new SlimDX.Vector3(0F, 0F, 0F);
			this.Compass.Scale = new SlimDX.Vector3(30F, 30F, 1F);
			this.Compass.SolidColor = System.Drawing.Color.Empty;
			this.Compass.TextureIndex = -1;
			this.Compass.Topology = SlimDX.Direct3D10.PrimitiveTopology.TriangleList;
			this.Compass.Transparency = ((byte)(160));
			// 
			// grid
			// 
			this.grid.CanPick = true;
			this.grid.Location = new SlimDX.Vector3(0F, 0F, 0F);
			this.grid.Name = "grid";
			this.grid.Rotation = new SlimDX.Vector3(0F, 0F, 0F);
			this.grid.Scale = new SlimDX.Vector3(1F, 1F, 1F);
			this.grid.SolidColor = System.Drawing.Color.Black;
			this.grid.TextureIndex = -1;
			this.grid.Topology = SlimDX.Direct3D10.PrimitiveTopology.LineList;
			this.grid.Transparency = ((byte)(64));
			this.grid.XCount = 24;
			this.grid.YCount = 4;
			this.grid.ZCount = 24;
			// 
			// Earth3DControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.KeyboardMovementSpeed = 5F;
			this.MouseMovementSpeed = 5F;
			this.Name = "Earth3DControl";
			this.ShapeList.Add(this.Compass);
			this.ShapeList.Add(this.grid);
			this.Size = new System.Drawing.Size(667, 432);
			this.ZClipFar = 10000F;
			this.ZClipNear = 0.05F;
			this.Load += new System.EventHandler(this.Earth3DControl_Load);
			this.ResumeLayout(false);

		}

		#endregion

		private Compass Compass;
		private shapes.Grid grid;


	}
}
