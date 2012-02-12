using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Direct3DLib
{
	public partial class EarthControlOptionsForm : Form
	{
		private LatLong prevGoto;
		public LatLong GotoLatLong
		{
			get
			{
				try { return LatLong.Parse(gotoBox.Text); }
				catch (ArgumentException aex) { HandlePropertyError(aex); return prevGoto; }
			}
			set { prevGoto = value; gotoBox.Text = prevGoto.ToString(); }
		}

		private double prevEle;
		public double GotoElevation
		{
			get
			{
				try { return double.Parse(elevationBox.Text); }
				catch (ArgumentException aex) { HandlePropertyError(aex); return prevEle; }
			}
			set { prevEle = value; elevationBox.Text = prevEle.ToString(); }
		}

		private string prevTerrainFolder;
		public string TerrainFolder
		{
			get
			{
				try { return GetFolderFromString(terrainBox.Filename); }
				catch (FileNotFoundException aex) { HandlePropertyError(aex); return prevTerrainFolder; }
			}
			set { prevTerrainFolder = value; terrainBox.Filename = prevTerrainFolder; }
		}

		private string prevTextureFolder;
		public string TextureFolder
		{
			get
			{
				try { return GetFolderFromString(textureBox.Filename); }
				catch (FileNotFoundException aex) { HandlePropertyError(aex); return prevTextureFolder; }
			}
			set { prevTextureFolder = value; textureBox.Filename = prevTextureFolder; }
		}

		private string GetFolderFromString(string path)
		{
			if (!Directory.Exists(path)) throw new FileNotFoundException("Could not file Folder", path);
			return path;
		}

		public int MaxGoogleZoom
		{
			get { return (int)maxZoomBox.Value; }
			set {  maxZoomBox.Value = value; }
		}

		public double Delta
		{
			get { return Math.Pow(2.0, (double)logDeltaBox.Value); }
			set { logDeltaBox.Value = (decimal)Math.Floor(Math.Log(value, 2.0)); }
		}

		public bool FixZoom
		{
			get { return fixZoomCheck.Checked; }
			set { fixZoomCheck.Checked = value; }
		}

		public bool FixTerrain
		{
			get { return fixTerrainCheck.Checked; }
			set { fixTerrainCheck.Checked = value; }
		}

		public int KeyboardSpeed
		{
			get { return (int)keySpeedBox.Value; }
			set { keySpeedBox.Value = value; }
		}

		public bool UseGIS
		{
			get { return useGisCheck.Checked; }
			set { useGisCheck.Checked = value; }
		}

		public bool AutomaticallyDownloadMaps
		{
			get { return autoDownloadCheck.Checked; }
			set { autoDownloadCheck.Checked = value; }
		}

		public bool ShowGrid
		{
			get { return showGridCheck.Checked; }
			set { showGridCheck.Checked = value; }
		}

		public event EventHandler OptionsChanged;
		private void FireOptionsChangedEvent()
		{
			if (OptionsChanged != null)
				OptionsChanged(this, new EventArgs());
		}

		public EarthControlOptionsForm()
		{
			InitializeComponent();
		}

		private void buttonClick(object sender, EventArgs e)
		{
			if (sender == okButton)
			{
				FireOptionsChangedEvent();
				this.DialogResult = System.Windows.Forms.DialogResult.OK;
				this.Close();
			}
			else if (sender == applyButton)
			{
				FireOptionsChangedEvent();
			}
			else if (sender == cancelButton)
			{
				this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
				this.Close();
			}
		}

		private void HandlePropertyError(Exception ex)
		{
			MessageBox.Show("Error setting property\n" + ex);
		}

		public static void CheckGlobalSettings()
		{
			if (!Directory.Exists(Properties.Settings.Default.MapTerrainFolder))
			{
				FolderBrowserDialog fbd = new FolderBrowserDialog();
				fbd.Description = "Select the folder with GIS terrain data (*.hgt files)";
				fbd.ShowDialog();
				Properties.Settings.Default.MapTerrainFolder = fbd.SelectedPath;
				Properties.Settings.Default.Save();
			}
			if (!Directory.Exists(Properties.Settings.Default.MapTextureFolder))
			{
				FolderBrowserDialog fbd = new FolderBrowserDialog();
				fbd.Description = "Select the folder with downloaded Google Maps.";
				fbd.ShowDialog();
				Properties.Settings.Default.MapTextureFolder = fbd.SelectedPath;
				Properties.Settings.Default.Save();
			}
		}

		public static void UseGisCheckChanged()
		{
			MessageBox.Show("Currently loaded terrain will not change until Application restarts",
				"GIS Terrain Data Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
		}

	}
}
