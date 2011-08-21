namespace Direct3DLib_Test
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
			Direct3DLib.Float3 float31 = new Direct3DLib.Float3();
			Direct3DLib.Float3 float32 = new Direct3DLib.Float3();
			Direct3DLib.Float3 float33 = new Direct3DLib.Float3();
			Direct3DLib.Float3 float34 = new Direct3DLib.Float3();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.textBox2 = new System.Windows.Forms.TextBox();
			this.propertyGrid = new System.Windows.Forms.PropertyGrid();
			this.comboBox1 = new System.Windows.Forms.ComboBox();
			this.comboBox2 = new System.Windows.Forms.ComboBox();
			this.comboBox3 = new System.Windows.Forms.ComboBox();
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.earth3DControl = new Direct3DLib.Earth3DControl();
			this.hercules = new Direct3DLib.ComplexShape();
			this.cone1 = new Direct3DLib.Cone();
			this.SuspendLayout();
			// 
			// textBox1
			// 
			this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox1.Location = new System.Drawing.Point(12, 6);
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(774, 157);
			this.textBox1.TabIndex = 1;
			// 
			// textBox2
			// 
			this.textBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.textBox2.Location = new System.Drawing.Point(12, 169);
			this.textBox2.Multiline = true;
			this.textBox2.Name = "textBox2";
			this.textBox2.Size = new System.Drawing.Size(774, 39);
			this.textBox2.TabIndex = 3;
			// 
			// propertyGrid
			// 
			this.propertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.propertyGrid.Location = new System.Drawing.Point(792, 121);
			this.propertyGrid.Name = "propertyGrid";
			this.propertyGrid.Size = new System.Drawing.Size(339, 525);
			this.propertyGrid.TabIndex = 4;
			// 
			// comboBox1
			// 
			this.comboBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox1.FormattingEnabled = true;
			this.comboBox1.Location = new System.Drawing.Point(1006, 6);
			this.comboBox1.Name = "comboBox1";
			this.comboBox1.Size = new System.Drawing.Size(125, 21);
			this.comboBox1.TabIndex = 5;
			this.comboBox1.SelectedValueChanged += new System.EventHandler(this.comboBox1_SelectedValueChanged);
			// 
			// comboBox2
			// 
			this.comboBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox2.FormattingEnabled = true;
			this.comboBox2.Location = new System.Drawing.Point(1006, 32);
			this.comboBox2.Name = "comboBox2";
			this.comboBox2.Size = new System.Drawing.Size(125, 21);
			this.comboBox2.TabIndex = 6;
			this.comboBox2.SelectedValueChanged += new System.EventHandler(this.comboBox1_SelectedValueChanged);
			// 
			// comboBox3
			// 
			this.comboBox3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.comboBox3.FormattingEnabled = true;
			this.comboBox3.Location = new System.Drawing.Point(1006, 59);
			this.comboBox3.Name = "comboBox3";
			this.comboBox3.Size = new System.Drawing.Size(125, 21);
			this.comboBox3.TabIndex = 7;
			this.comboBox3.SelectedValueChanged += new System.EventHandler(this.comboBox1_SelectedValueChanged);
			// 
			// label1
			// 
			this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(897, 9);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(60, 13);
			this.label1.TabIndex = 8;
			this.label1.Text = "Left Mouse";
			// 
			// label2
			// 
			this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(897, 35);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(67, 13);
			this.label2.TabIndex = 9;
			this.label2.Text = "Right Mouse";
			// 
			// label3
			// 
			this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(897, 62);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(64, 13);
			this.label3.TabIndex = 10;
			this.label3.Text = "Both Mouse";
			// 
			// button1
			// 
			this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button1.Location = new System.Drawing.Point(792, 86);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(172, 23);
			this.button1.TabIndex = 12;
			this.button1.Text = "Add Shape";
			this.button1.UseVisualStyleBackColor = true;
			// 
			// button2
			// 
			this.button2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.button2.Location = new System.Drawing.Point(970, 86);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(161, 23);
			this.button2.TabIndex = 14;
			this.button2.Text = "Google Map Test";
			this.button2.UseVisualStyleBackColor = true;
			// 
			// earth3DControl
			// 
			this.earth3DControl.AllowDrop = true;
			this.earth3DControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.earth3DControl.AutomaticallyDownloadMaps = false;
			this.earth3DControl.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
			this.earth3DControl.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.earth3DControl.BothMouseFunction = Direct3DLib.Direct3DControl.MouseOption.CameraTranslateXZ;
			float31.X = 1.74814E+07F;
			float31.Y = 5000F;
			float31.Z = -3682200F;
			this.earth3DControl.CameraLocation = float31;
			this.earth3DControl.CameraPan = 1.653334F;
			this.earth3DControl.CameraTilt = -1.610595F;
			this.earth3DControl.CurrentElevation = 5000D;
			this.earth3DControl.FixTerrain = false;
			this.earth3DControl.FixZoom = false;
			this.earth3DControl.KeyboardMovementSpeed = 5000F;
			this.earth3DControl.LeftMouseFunction = Direct3DLib.Direct3DControl.MouseOption.Select;
			this.earth3DControl.LightAmbientIntensity = 0.7F;
			float32.X = 1F;
			float32.Y = 1F;
			float32.Z = 1F;
			this.earth3DControl.LightDirection = float32;
			this.earth3DControl.LightDirectionalIntensity = 0.3F;
			this.earth3DControl.Location = new System.Drawing.Point(12, 214);
			this.earth3DControl.MouseMovementSpeed = 5000F;
			this.earth3DControl.Name = "earth3DControl";
			this.earth3DControl.RightMouseFunction = Direct3DLib.Direct3DControl.MouseOption.Rotate;
			this.earth3DControl.SelectedObject = null;
			this.earth3DControl.Size = new System.Drawing.Size(774, 433);
			this.earth3DControl.TabIndex = 15;
			this.earth3DControl.TextureImages = new System.Drawing.Image[] {
        ((System.Drawing.Image)(global::Direct3DLib_Test.Properties.Resources.Chrysanthemum)),
        ((System.Drawing.Image)(global::Direct3DLib_Test.Properties.Resources.NoTexture)),
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null,
        null};
			this.earth3DControl.ZClipFar = 1E+07F;
			this.earth3DControl.ZClipNear = 5F;
			this.earth3DControl.Zoom = 5F;
			this.earth3DControl.Load += new System.EventHandler(this.earth3DControl_Load);
			// 
			// hercules
			// 
			this.hercules.CanPick = true;
			this.hercules.Location = new SlimDX.Vector3(1.748E+07F, 8200F, -3680000F);
			this.hercules.Rotation = new SlimDX.Vector3(4.783185F, 0F, 0F);
			this.hercules.Scale = new SlimDX.Vector3(100F, 100F, 100F);
			this.hercules.SolidColor = System.Drawing.Color.Silver;
			this.hercules.SourceFile = "Resources.hercules.stl";
			this.hercules.TextureIndex = -1;
			this.hercules.Topology = SlimDX.Direct3D10.PrimitiveTopology.TriangleList;
			this.hercules.Transparency = ((byte)(255));
			// 
			// cone1
			// 
			float33.X = 1.748E+07F;
			float33.Y = 0F;
			float33.Z = -3688000F;
			this.cone1.BroadLocation = float33;
			this.cone1.CanPick = true;
			this.cone1.Corners = 6;
			this.cone1.diff = new SlimDX.Vector3(0F, -8000F, -8000F);
			this.cone1.distance = 11313.708984375D;
			this.cone1.Location = new SlimDX.Vector3(1.748E+07F, 8000F, -3680000F);
			float34.X = 1.748E+07F;
			float34.Y = 8000F;
			float34.Z = -3680000F;
			this.cone1.NarrowLocation = float34;
			this.cone1.Rotation = new SlimDX.Vector3(0F, 0F, 0F);
			this.cone1.rotX = 2.356194F;
			this.cone1.rotY = 3.141593F;
			this.cone1.Scale = new SlimDX.Vector3(500F, 11313.71F, 500F);
			this.cone1.SolidColor = System.Drawing.Color.Lime;
			this.cone1.TextureIndex = -1;
			this.cone1.Topology = SlimDX.Direct3D10.PrimitiveTopology.TriangleList;
			this.cone1.Transparency = ((byte)(255));
			this.cone1.Width = 500F;
			// 
			// TestForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1143, 658);
			this.Controls.Add(this.earth3DControl);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.comboBox3);
			this.Controls.Add(this.comboBox2);
			this.Controls.Add(this.comboBox1);
			this.Controls.Add(this.propertyGrid);
			this.Controls.Add(this.textBox2);
			this.Controls.Add(this.textBox1);
			this.Name = "TestForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "TestForm";
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.PropertyGrid propertyGrid;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ComboBox comboBox2;
        private System.Windows.Forms.ComboBox comboBox3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private Direct3DLib.Earth3DControl earth3DControl;
		private Direct3DLib.ComplexShape hercules;
		private Direct3DLib.Cone cone1;
    }
}