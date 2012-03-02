﻿namespace Direct3DExtensions_Test
{
	partial class TestForm
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
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.button1 = new System.Windows.Forms.Button();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.independentMeshEngine1 = new Direct3DExtensions.TestObjectEngine();
			this.d3DHostControl1 = new Direct3DExtensions.D3DHostControl();
			this.SuspendLayout();
			// 
			// richTextBox1
			// 
			this.richTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.richTextBox1.Location = new System.Drawing.Point(632, 370);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(292, 217);
			this.richTextBox1.TabIndex = 2;
			this.richTextBox1.Text = "";
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(632, 593);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(292, 23);
			this.button1.TabIndex = 3;
			this.button1.Text = "Start Direct3D";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.propertyGrid1.Location = new System.Drawing.Point(632, 12);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.SelectedObject = this.independentMeshEngine1;
			this.propertyGrid1.Size = new System.Drawing.Size(292, 352);
			this.propertyGrid1.TabIndex = 1;
			// 
			// independentMeshEngine1
			// 
			this.independentMeshEngine1.CameraInput = null;
			this.independentMeshEngine1.D3DDevice = null;
			this.independentMeshEngine1.Effect = null;
			this.independentMeshEngine1.Geometry = null;
			this.independentMeshEngine1.HostControl = this.d3DHostControl1;
			this.independentMeshEngine1.CameraChanged += new Direct3DExtensions.CameraChangedEventHandler(this.independentMeshEngine1_CameraChanged);
			// 
			// d3DHostControl1
			// 
			this.d3DHostControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.d3DHostControl1.BackColor = System.Drawing.Color.LightBlue;
			this.d3DHostControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.d3DHostControl1.Location = new System.Drawing.Point(12, 12);
			this.d3DHostControl1.Name = "d3DHostControl1";
			this.d3DHostControl1.Size = new System.Drawing.Size(614, 604);
			this.d3DHostControl1.TabIndex = 0;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(936, 628);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.richTextBox1);
			this.Controls.Add(this.propertyGrid1);
			this.Controls.Add(this.d3DHostControl1);
			this.Name = "TestForm";
			this.Text = "TestForm";
			this.ResumeLayout(false);

		}

		#endregion

		private Direct3DExtensions.D3DHostControl d3DHostControl1;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private Direct3DExtensions.TestObjectEngine independentMeshEngine1;
		private System.Windows.Forms.Button button1;
	}
}