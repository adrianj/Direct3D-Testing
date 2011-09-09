using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using SlimDX;
using System.Drawing;

namespace Direct3DLib
{
	public partial class Earth3DControl : Direct3DControl
	{
		#region Private Fields
		public const int TEXTURE_OFFSET = ShaderHelper.MAX_TEXTURES - (EarthTiles.TILE_COUNT * EarthTiles.TILE_COUNT);
		private const int TEXTURES_USED = (EarthTiles.TILE_COUNT * EarthTiles.TILE_COUNT) + 1;
		private const int TEXTURES_REMAINING = ShaderHelper.MAX_TEXTURES - TEXTURES_USED;
		private List<string> localTextureFilenames = new List<string>();
		private List<string> externalTextureFilenames = new List<string>();
		private EarthTiles earthTiles = new EarthTiles();
		private Float3 previousCameraLocation = new Float3();
		private bool isInitialized { get { return Engine.IsInitialized; } }
		private EarthControlOptionsForm optionsForm = new EarthControlOptionsForm();
		#endregion

		public bool ShowGrid { get; set; }
		public string debugString = "";
		private double currentLat;
		private double currentLong;
		private double currentEle;
		[CategoryAttribute("Camera, Lighting and Textures")]
		[Description("The latitude and longitude of the camera")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public LatLong CurrentLatLong
		{
			get
			{
				LatLong ll = EarthProjection.ConvertCameraLocationToLatLong(new Float3(CameraLocation));
				currentLat = ll.Latitude; currentLong = ll.Longitude;
				return new LatLong(currentLat, currentLong);
			}
			set
			{
				currentLat = value.Latitude; currentLong = value.Longitude;
				this.CameraLocation = EarthProjection.ConvertLatLongElevationToCameraLocation(value, CurrentElevation);
			}
		}
		[CategoryAttribute("Camera, Lighting and Textures")]
		[Description("The elevation of the camera in metres")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public double CurrentElevation
		{
			get {
				currentEle = EarthProjection.ConvertCameraLocationToElevation(CameraLocation); return currentEle;
			}
			set
			{
				currentEle = value;
				this.CameraLocation = EarthProjection.ConvertLatLongElevationToCameraLocation(CurrentLatLong, value);
			}
		}
		public int MaxTextureZoom { get { return EarthTiles.MaxGoogleZoom; } set { if(value <= 20 && value >= 0) EarthTiles.MaxGoogleZoom = value; } }
		public bool AutomaticallyDownloadMaps
		{
			get { return earthTiles.AutomaticallyDownloadMaps; }
			set { earthTiles.AutomaticallyDownloadMaps = value; }
		}
		public bool FixTerrain { get { return earthTiles.FixTerrain; } set { earthTiles.FixTerrain = value; } }
		public bool FixZoom { get { return earthTiles.FixZoom; } set { earthTiles.FixZoom = value; } }
		private bool UseTerrainData
		{
			get { return earthTiles.UseTerrainData; }
			set
			{
				Properties.Settings.Default.UseGISData = value;
				Properties.Settings.Default.Save(); 
				earthTiles.UseTerrainData = value;
			}
		}
		[Editor(typeof(System.Windows.Forms.Design.FolderNameEditor),typeof(System.Drawing.Design.UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string TerrainFolder
		{
			get { return Properties.Settings.Default.MapTerrainFolder; }
			set { Properties.Settings.Default.MapTerrainFolder = value; if (!this.DesignMode)Properties.Settings.Default.Save(); }
		}
		[Editor(typeof(System.Windows.Forms.Design.FolderNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string TextureFolder
		{
			get { return Properties.Settings.Default.MapTextureFolder; }
			set { Properties.Settings.Default.MapTextureFolder = value; if(!this.DesignMode)Properties.Settings.Default.Save(); }
		}

		public override Image[] TextureImages
		{
			get
			{
				Image [] value = new Image[TEXTURES_REMAINING];
				Array.Copy(base.TextureImages, 0, value, 0, value.Length);
				return value;
			}
			set
			{
				Image[] val = new Image[ShaderHelper.MAX_TEXTURES];
				Array.Copy(val, 0, value, 0, Math.Min(val.Length, TEXTURES_REMAINING));
				Array.Copy(base.TextureImages, TEXTURES_REMAINING, val, TEXTURES_REMAINING, TEXTURES_USED);
				base.TextureImages = val;
			}
		}


		public Earth3DControl()
		{
			InitializeComponent();
			InitializeOthers();
			Engine.Camera.IsFirstPerson = true;
		}

		private void InitializeOthers()
		{
			DoubleClick += Earth3DControl_DoubleClick;
			CameraChanged +=new EventHandler(engine_CameraChanged);
			optionsForm.FormClosing += (o, ev) =>
			{
				optionsForm.Hide();
				ev.Cancel = true;
			};
			optionsForm.OptionsChanged += (o, ev) => { UpdateControlFromOptions(); };
		}

		private void engine_CameraChanged(object sender, EventArgs e)
		{
			if (this.isInitialized)
			{
				RestrictCameraElevation();
				if (previousCameraLocation.X != CameraLocation.X || previousCameraLocation.Z != CameraLocation.Z || previousCameraLocation.Y != CameraLocation.Y)
				{
					earthTiles.CameraLocationChanged(new Float3(CameraLocation));
				}
				UpdateDebugString();
			}
			previousCameraLocation = new Float3(CameraLocation);
		}

		private void UpdateDebugString()
		{
			if (SelectedObject != null)
			{
				Shape shape = SelectedObject as Shape;
				Matrix wvp = shape.World * CameraView * CameraProjection;
				BoundingBox newBB = shape.MaxBoundingBox;
				newBB = Direct3DEngine.BoundingBoxMultiplyMatrix(newBB, wvp);
				BoundingBox bbCam = new BoundingBox(new Vector3(-1.0f, -1.0f, 0), new Vector3(1.0f, 1.0f, 1.0f));
				bool onScreen = BoundingBox.Intersects(newBB, bbCam);
				debugString = printCorners(newBB)
					+ Environment.NewLine + BoundingBox.Intersects(newBB, bbCam);
			}
		}



		private string printCorners(Vector3[] corners)
		{
			string ret = printVector3(corners[0]);
			for (int i = 1; i < corners.Length; i ++)
			{
				ret += Environment.NewLine + printVector3(corners[i]);
			}
			return ret;
		}

		private string printCorners(BoundingBox bb)
		{
			Vector3[] corners = bb.GetCorners();
			return printCorners(corners);
		}

		private string printVector3(Vector3 vec)
		{
			string ret = "";
			ret += "" + vec.X.ToString("G8") + " ";
			ret += "" + vec.Y.ToString("G8") + " ";
			ret += "" + vec.Z.ToString("G8");
			return ret;
		}

		private void RestrictCameraElevation()
		{
			Float3 loc = new Float3(CameraLocation);
			if (loc.Y < -100)
			{
				loc.Y = -100;
				CameraLocation = loc;
			}
			if (loc.Y > (float)EarthTiles.MAX_ELEVATION)
			{
				loc.Y = (float)EarthTiles.MAX_ELEVATION;
				CameraLocation = loc;
			}
		}

		public void Reset()
		{
			if (this.isInitialized)
			{
				earthTiles.Clear();
				earthTiles.InitializeAtCameraLocation(CameraLocation);
				SetGrid();
			}
		}

		private void Earth3DControl_Load(object sender, EventArgs e)
		{
			if (this.isInitialized)
			{
				base.TextureImages[TEXTURE_OFFSET-1] = Direct3DLib.Properties.Resources.compass;
				this.Engine.UpdateTextures();
				EarthControlOptionsForm.CheckGlobalSettings();
				UseTerrainData = Properties.Settings.Default.UseGISData;
				earthTiles.MapChanged += new ShapeChangeEventHandler(earthTiles_MapChanged); 
			}
			this.Reset();
		}

		void earthTiles_MapChanged(object sender, ShapeChangeEventArgs e)
		{
			if (e.Action == ShapeChangeEventArgs.ChangeAction.Add)
			{
				if (!Engine.ShapeList.Contains(e.ChangedShape))
				{
					this.InsertShape(0, e.ChangedShape);
					SetGrid();
				}
			}
			if (e.Action == ShapeChangeEventArgs.ChangeAction.Remove)
			{
				this.RemoveShape(e.ChangedShape);
			}
		}

		private void Earth3DControl_DoubleClick(object sender, EventArgs e)
		{
			UpdateOptionsFromControl();
			optionsForm.Show();
		}

		public override float CameraPan
		{
			get { return base.CameraPan; }
			set
			{
				base.CameraPan = value;
				UpdateCompass();
			}
		}

		public void UpdateCompass()
		{
			Compass.Rotation = new Vector3(0, 0, -CameraPan+(float)Math.PI/2);
		}

		private void UpdateControlFromOptions()
		{
			this.MaxTextureZoom = optionsForm.MaxGoogleZoom;
			FixTerrain = optionsForm.FixTerrain;
			KeyboardMovementSpeed = optionsForm.KeyboardSpeed;
			if (UseTerrainData != optionsForm.UseGIS)
			{
				UseTerrainData = optionsForm.UseGIS;
				//EarthControlOptionsForm.UseGisCheckChanged();
			}
			FixZoom = optionsForm.FixZoom;
			AutomaticallyDownloadMaps = optionsForm.AutomaticallyDownloadMaps;
			this.TerrainFolder = optionsForm.TerrainFolder;
			this.TextureFolder = optionsForm.TextureFolder;
			CurrentLatLong = optionsForm.GotoLatLong;
			CurrentElevation = optionsForm.GotoElevation;
			ShowGrid = optionsForm.ShowGrid;
			this.Reset();
			UpdateOptionsFromControl();
		}

		private void UpdateOptionsFromControl()
		{
			optionsForm.GotoLatLong = CurrentLatLong;
			optionsForm.AutomaticallyDownloadMaps = AutomaticallyDownloadMaps;
			optionsForm.UseGIS = UseTerrainData;
			optionsForm.FixZoom = FixZoom;
			optionsForm.KeyboardSpeed = (int)KeyboardMovementSpeed;
			optionsForm.GotoElevation = CurrentElevation;
			optionsForm.MaxGoogleZoom = this.MaxTextureZoom;
			optionsForm.FixTerrain = FixTerrain;
			optionsForm.TerrainFolder = this.TerrainFolder;
			optionsForm.TextureFolder = this.TextureFolder;
			optionsForm.ShowGrid = this.ShowGrid;
		}

		void SetGrid()
		{
			if (ShowGrid)
			{
				float scale = (float)(EarthProjection.UnitsPerDegreeLatitude * earthTiles.CurrentDelta) / 16;
				Vector3 loc = new Vector3(RoundToNearestInt(CameraLocation.X, scale), 0, RoundToNearestInt(CameraLocation.Z, scale));
				loc.X -= scale * grid.XCount / 2;
				loc.Z -= scale * grid.ZCount / 2;
				grid.Location = loc;
				grid.Scale = new Vector3(scale, scale, scale);
				grid.Topology = SlimDX.Direct3D10.PrimitiveTopology.LineList;
			}
			else
				grid.Topology = SlimDX.Direct3D10.PrimitiveTopology.Undefined;
		}

		float RoundToNearestInt(float value, float scale)
		{
			double val = Math.Round(value/scale);
			val *= scale;
			return (float)val;
		}
	}
}
