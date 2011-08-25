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
			this.SuspendLayout();
			// 
			// Earth3DControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.ActiveBorder;
			this.KeyboardMovementSpeed = 5F;
			this.MouseMovementSpeed = 5F;
			this.Name = "Earth3DControl";
			this.Size = new System.Drawing.Size(667, 432);
			this.ZClipFar = 10000F;
			this.ZClipNear = 0.05F;
			this.Load += new System.EventHandler(this.Earth3DControl_Load);
			this.ResumeLayout(false);

		}

		#endregion

	}
}
