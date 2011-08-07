namespace Direct3DLib
{
	partial class EarthControlOptionsForm
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
			this.okButton = new System.Windows.Forms.Button();
			this.applyButton = new System.Windows.Forms.Button();
			this.cancelButton = new System.Windows.Forms.Button();
			this.gotoBox = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.fixTerrainCheck = new System.Windows.Forms.CheckBox();
			this.keySpeedBox = new System.Windows.Forms.NumericUpDown();
			this.label7 = new System.Windows.Forms.Label();
			this.elevationBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.textureBox = new DTALib.FileBrowserBox();
			this.label2 = new System.Windows.Forms.Label();
			this.terrainBox = new DTALib.FileBrowserBox();
			this.label5 = new System.Windows.Forms.Label();
			this.maxZoomBox = new System.Windows.Forms.NumericUpDown();
			this.label6 = new System.Windows.Forms.Label();
			this.logDeltaBox = new System.Windows.Forms.NumericUpDown();
			this.fixZoomCheck = new System.Windows.Forms.CheckBox();
			this.useGisCheck = new System.Windows.Forms.CheckBox();
			this.autoDownloadCheck = new System.Windows.Forms.CheckBox();
			this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.keySpeedBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.maxZoomBox)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.logDeltaBox)).BeginInit();
			this.tableLayoutPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// okButton
			// 
			this.okButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.okButton.Location = new System.Drawing.Point(40, 6);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(75, 23);
			this.okButton.TabIndex = 0;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.buttonClick);
			// 
			// applyButton
			// 
			this.applyButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.applyButton.Location = new System.Drawing.Point(196, 6);
			this.applyButton.Name = "applyButton";
			this.applyButton.Size = new System.Drawing.Size(75, 23);
			this.applyButton.TabIndex = 1;
			this.applyButton.Text = "Apply";
			this.applyButton.UseVisualStyleBackColor = true;
			this.applyButton.Click += new System.EventHandler(this.buttonClick);
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.cancelButton.Location = new System.Drawing.Point(353, 6);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(75, 23);
			this.cancelButton.TabIndex = 2;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.buttonClick);
			// 
			// gotoBox
			// 
			this.gotoBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.gotoBox, 2);
			this.gotoBox.Location = new System.Drawing.Point(159, 5);
			this.gotoBox.Name = "gotoBox";
			this.gotoBox.Size = new System.Drawing.Size(307, 20);
			this.gotoBox.TabIndex = 3;
			// 
			// label1
			// 
			this.label1.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(3, 9);
			this.label1.Margin = new System.Windows.Forms.Padding(3);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(84, 13);
			this.label1.TabIndex = 4;
			this.label1.Text = "Go To Lat/Long";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.ColumnCount = 3;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel1.Controls.Add(this.fixTerrainCheck, 0, 9);
			this.tableLayoutPanel1.Controls.Add(this.keySpeedBox, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.label7, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.elevationBox, 1, 1);
			this.tableLayoutPanel1.Controls.Add(this.label4, 0, 1);
			this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.gotoBox, 1, 0);
			this.tableLayoutPanel1.Controls.Add(this.label3, 0, 3);
			this.tableLayoutPanel1.Controls.Add(this.textureBox, 1, 3);
			this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.terrainBox, 1, 2);
			this.tableLayoutPanel1.Controls.Add(this.label5, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.maxZoomBox, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.label6, 0, 5);
			this.tableLayoutPanel1.Controls.Add(this.logDeltaBox, 1, 5);
			this.tableLayoutPanel1.Controls.Add(this.fixZoomCheck, 0, 9);
			this.tableLayoutPanel1.Controls.Add(this.useGisCheck, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.autoDownloadCheck, 1, 8);
			this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 12);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 10;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(469, 317);
			this.tableLayoutPanel1.TabIndex = 5;
			// 
			// fixTerrainCheck
			// 
			this.fixTerrainCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.fixTerrainCheck.AutoSize = true;
			this.fixTerrainCheck.Location = new System.Drawing.Point(3, 289);
			this.fixTerrainCheck.Name = "fixTerrainCheck";
			this.fixTerrainCheck.Size = new System.Drawing.Size(75, 17);
			this.fixTerrainCheck.TabIndex = 21;
			this.fixTerrainCheck.Text = "Fix Terrain";
			this.fixTerrainCheck.UseVisualStyleBackColor = true;
			// 
			// keySpeedBox
			// 
			this.keySpeedBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.keySpeedBox, 2);
			this.keySpeedBox.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.keySpeedBox.Location = new System.Drawing.Point(159, 191);
			this.keySpeedBox.Maximum = new decimal(new int[] {
            50000,
            0,
            0,
            0});
			this.keySpeedBox.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
			this.keySpeedBox.Name = "keySpeedBox";
			this.keySpeedBox.Size = new System.Drawing.Size(307, 20);
			this.keySpeedBox.TabIndex = 19;
			this.keySpeedBox.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
			// 
			// label7
			// 
			this.label7.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(3, 195);
			this.label7.Margin = new System.Windows.Forms.Padding(3);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(86, 13);
			this.label7.TabIndex = 18;
			this.label7.Text = "Keyboard Speed";
			// 
			// elevationBox
			// 
			this.elevationBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.elevationBox, 2);
			this.elevationBox.Location = new System.Drawing.Point(159, 36);
			this.elevationBox.Name = "elevationBox";
			this.elevationBox.Size = new System.Drawing.Size(307, 20);
			this.elevationBox.TabIndex = 11;
			// 
			// label4
			// 
			this.label4.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(3, 40);
			this.label4.Margin = new System.Windows.Forms.Padding(3);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(84, 13);
			this.label4.TabIndex = 10;
			this.label4.Text = "Go To Elevation";
			// 
			// label3
			// 
			this.label3.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(3, 102);
			this.label3.Margin = new System.Windows.Forms.Padding(3);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(99, 13);
			this.label3.TabIndex = 7;
			this.label3.Text = "Map Texture Folder";
			// 
			// textureBox
			// 
			this.textureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.textureBox, 2);
			this.textureBox.Directory = true;
			this.textureBox.Filename = "";
			this.textureBox.Filter = "All Files (*.*)|*.*";
			this.textureBox.InitialDirectory = "";
			this.textureBox.Label = "";
			this.textureBox.Location = new System.Drawing.Point(159, 98);
			this.textureBox.MaxPreviousFiles = 5;
			this.textureBox.Name = "textureBox";
			this.textureBox.SaveFile = false;
			this.textureBox.Size = new System.Drawing.Size(307, 21);
			this.textureBox.TabIndex = 9;
			this.textureBox.TextBoxReadOnly = true;
			// 
			// label2
			// 
			this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(3, 71);
			this.label2.Margin = new System.Windows.Forms.Padding(3);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(93, 13);
			this.label2.TabIndex = 5;
			this.label2.Text = "GIS Terrain Folder";
			// 
			// terrainBox
			// 
			this.terrainBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.terrainBox, 2);
			this.terrainBox.Directory = true;
			this.terrainBox.Filename = "";
			this.terrainBox.Filter = "All Files (*.*)|*.*";
			this.terrainBox.InitialDirectory = "";
			this.terrainBox.Label = "";
			this.terrainBox.Location = new System.Drawing.Point(159, 67);
			this.terrainBox.MaxPreviousFiles = 5;
			this.terrainBox.Name = "terrainBox";
			this.terrainBox.SaveFile = false;
			this.terrainBox.Size = new System.Drawing.Size(307, 21);
			this.terrainBox.TabIndex = 8;
			this.terrainBox.TextBoxReadOnly = true;
			// 
			// label5
			// 
			this.label5.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(3, 133);
			this.label5.Margin = new System.Windows.Forms.Padding(3);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(110, 13);
			this.label5.TabIndex = 12;
			this.label5.Text = "Maximum Zoom Level";
			// 
			// maxZoomBox
			// 
			this.maxZoomBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.maxZoomBox, 2);
			this.maxZoomBox.Location = new System.Drawing.Point(159, 129);
			this.maxZoomBox.Maximum = new decimal(new int[] {
            21,
            0,
            0,
            0});
			this.maxZoomBox.Name = "maxZoomBox";
			this.maxZoomBox.Size = new System.Drawing.Size(307, 20);
			this.maxZoomBox.TabIndex = 13;
			// 
			// label6
			// 
			this.label6.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(3, 164);
			this.label6.Margin = new System.Windows.Forms.Padding(3);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(111, 13);
			this.label6.TabIndex = 14;
			this.label6.Text = "Tile Size [ log2(delta) ]";
			// 
			// logDeltaBox
			// 
			this.logDeltaBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel1.SetColumnSpan(this.logDeltaBox, 2);
			this.logDeltaBox.Location = new System.Drawing.Point(159, 160);
			this.logDeltaBox.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
			this.logDeltaBox.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            -2147483648});
			this.logDeltaBox.Name = "logDeltaBox";
			this.logDeltaBox.Size = new System.Drawing.Size(307, 20);
			this.logDeltaBox.TabIndex = 15;
			// 
			// fixZoomCheck
			// 
			this.fixZoomCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.fixZoomCheck.AutoSize = true;
			this.fixZoomCheck.Location = new System.Drawing.Point(159, 289);
			this.fixZoomCheck.Name = "fixZoomCheck";
			this.fixZoomCheck.Size = new System.Drawing.Size(69, 17);
			this.fixZoomCheck.TabIndex = 16;
			this.fixZoomCheck.Text = "Fix Zoom";
			this.fixZoomCheck.UseVisualStyleBackColor = true;
			// 
			// useGisCheck
			// 
			this.useGisCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.useGisCheck.AutoSize = true;
			this.useGisCheck.Location = new System.Drawing.Point(3, 255);
			this.useGisCheck.Name = "useGisCheck";
			this.useGisCheck.Size = new System.Drawing.Size(102, 17);
			this.useGisCheck.TabIndex = 17;
			this.useGisCheck.Text = "Use GIS Terrain";
			this.useGisCheck.UseVisualStyleBackColor = true;
			// 
			// autoDownloadCheck
			// 
			this.autoDownloadCheck.Anchor = System.Windows.Forms.AnchorStyles.Left;
			this.autoDownloadCheck.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.autoDownloadCheck, 2);
			this.autoDownloadCheck.Location = new System.Drawing.Point(159, 255);
			this.autoDownloadCheck.Name = "autoDownloadCheck";
			this.autoDownloadCheck.Size = new System.Drawing.Size(176, 17);
			this.autoDownloadCheck.TabIndex = 20;
			this.autoDownloadCheck.Text = "Automatically Download Images";
			this.autoDownloadCheck.UseVisualStyleBackColor = true;
			// 
			// tableLayoutPanel2
			// 
			this.tableLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.tableLayoutPanel2.ColumnCount = 3;
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
			this.tableLayoutPanel2.Controls.Add(this.okButton, 0, 0);
			this.tableLayoutPanel2.Controls.Add(this.applyButton, 1, 0);
			this.tableLayoutPanel2.Controls.Add(this.cancelButton, 2, 0);
			this.tableLayoutPanel2.Location = new System.Drawing.Point(12, 335);
			this.tableLayoutPanel2.Name = "tableLayoutPanel2";
			this.tableLayoutPanel2.RowCount = 1;
			this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.tableLayoutPanel2.Size = new System.Drawing.Size(469, 32);
			this.tableLayoutPanel2.TabIndex = 6;
			// 
			// EarthControlOptionsForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(493, 370);
			this.Controls.Add(this.tableLayoutPanel2);
			this.Controls.Add(this.tableLayoutPanel1);
			this.Name = "EarthControlOptionsForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "EarthControlOptionsForm";
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.keySpeedBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.maxZoomBox)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.logDeltaBox)).EndInit();
			this.tableLayoutPanel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.Button applyButton;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.TextBox gotoBox;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private DTALib.FileBrowserBox terrainBox;
		private DTALib.FileBrowserBox textureBox;
		private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
		private System.Windows.Forms.TextBox elevationBox;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.NumericUpDown maxZoomBox;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.NumericUpDown logDeltaBox;
		private System.Windows.Forms.CheckBox fixZoomCheck;
		private System.Windows.Forms.CheckBox useGisCheck;
		private System.Windows.Forms.NumericUpDown keySpeedBox;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.CheckBox fixTerrainCheck;
		private System.Windows.Forms.CheckBox autoDownloadCheck;
	}
}