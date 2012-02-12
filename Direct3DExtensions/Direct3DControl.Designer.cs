namespace Direct3DExtensions
{
	partial class Direct3DControl
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
			this.errLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// errLabel
			// 
			this.errLabel.Anchor = System.Windows.Forms.AnchorStyles.None;
			this.errLabel.AutoSize = true;
			this.errLabel.Location = new System.Drawing.Point(89, 223);
			this.errLabel.MaximumSize = new System.Drawing.Size(200, 200);
			this.errLabel.Name = "errLabel";
			this.errLabel.Size = new System.Drawing.Size(195, 26);
			this.errLabel.TabIndex = 0;
			this.errLabel.Text = "If you can see this Text, then something has gone wrong!";
			this.errLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Direct3DControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.Controls.Add(this.errLabel);
			this.Name = "Direct3DControl";
			this.Size = new System.Drawing.Size(418, 447);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label errLabel;
	}
}
