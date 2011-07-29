using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Direct3DLib;
using SlimDX;

namespace Direct3DLib
{
	public partial class Earth3DControl : UserControl
	{
		#region Private Fields
		public const int TEXTURE_OFFSET = ShaderHelper.MAX_TEXTURES / 2;
		private List<string> localTextureFilenames = new List<string>();
		private List<string> externalTextureFilenames = new List<string>();
		private EarthTiles earthTiles = new EarthTiles();
		private Float3 previousCameraLocation = new Float3();
		private bool isInitialized { get { return engine.Engine.IsInitialized; } }
		#endregion

		public string debugString = "";

		#region Direct3DControl Wrapped Properties, Events and Methods
		#region Properties
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float CameraTilt { get { return engine.Tilt; } set { engine.Tilt = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float CameraPan { get { return engine.Pan; } set { engine.Pan = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public Float3 CameraLocation { get { return new Float3(engine.CameraLocation); } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float ZClipNear { get { return engine.ZClipNear; } set { engine.ZClipNear = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float ZClipFar { get { return engine.ZClipFar; } set { engine.ZClipFar = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public Float3 LightDirection { get { return new Float3(engine.LightDirection); } set { engine.LightDirection = value.AsVector3(); } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float LightDirectionalIntensity { get { return engine.LightDirectionalIntensity; } set { engine.LightDirectionalIntensity = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public float LightAmbientIntensity { get { return engine.LightAmbientIntensity; } set { engine.LightAmbientIntensity = value; } }
		[CategoryAttribute("Camera, Lighting and Textures")]
		public string[] TextureImageFilenames { get { return GetTextureFilenames(); } set { SetTextureFilenames(value); } }
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public Direct3DControl.MouseOption LeftMouseFunction { get { return engine.LeftMouseFunction; } set { engine.LeftMouseFunction = value; } }
		/// <summary>
		/// Function to pertorm when the right mouse button is clicked.
		/// </summary>
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public Direct3DControl.MouseOption RightMouseFunction { get { return engine.RightMouseFunction; } set { engine.RightMouseFunction = value; } }
		/// <summary>
		/// Function to perform when both mouse buttons are held down. NOTE: Select function does not work correctly
		/// for both mouse buttons.
		/// </summary>
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public Direct3DControl.MouseOption BothMouseFunction { get { return engine.BothMouseFunction; } set { engine.BothMouseFunction = value; } }
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public float MouseMovementSpeed { get { return engine.MouseMovementSpeed; } set { engine.MouseMovementSpeed = value; } }
		[CategoryAttribute("Mouse and Keyboard Functions")]
		public float KeyboardMovementSpeed { get { return engine.KeyboardMovementSpeed; } set { engine.KeyboardMovementSpeed = value; } }

		public object SelectedObject { get { return engine.SelectedObject; } set { engine.SelectedObject = value; } }
		public List<IRenderable> ShapeList { get { return engine.Engine.ShapeList; } }
		public double RefreshRate { get { return engine.Engine.RefreshRate; } }

		#endregion



		#region Events
		public event EventHandler CameraChanged
		{
			add { engine.CameraChanged += value; }
			remove { engine.CameraChanged -= value; }
		}
		public event PropertyChangedEventHandler SelectedObjectChanged
		{
			add { engine.SelectedObjectChanged += value; }
			remove { engine.SelectedObjectChanged -= value; }
		}
		#endregion

		#region Methods
		public void UpdateShapes() { engine.Engine.UpdateShapes(); }
		public void Render() { engine.Render(); }
		#endregion
		#endregion

		[CategoryAttribute("Camera, Lighting and Textures")]
		public LatLong CurrentLatLong
		{
			get { return earthTiles.ConvertCameraLocationToLatLong(CameraLocation); }
			set { SetCameraLocation(earthTiles.ConvertLatLongElevationToCameraLocation(value, CurrentElevation)); }
		}
		[CategoryAttribute("Camera, Lighting and Textures")]
		public double CurrentElevation
		{
			get { return CameraLocation.Y; }
			set { SetCameraLocation(earthTiles.ConvertLatLongElevationToCameraLocation(CurrentLatLong, value)); }
		}
		

		private string[] GetTextureFilenames()
		{
			return externalTextureFilenames.ToArray();
		}

		private void SetTextureFilenames(string[] filesNotIncludingLocalTextures)
		{
			externalTextureFilenames.Clear();
			externalTextureFilenames.AddRange(filesNotIncludingLocalTextures);
			CombineTextureFilenameListAndUpdate();
		}

		private void CombineTextureFilenameListAndUpdate()
		{
			string[] newFiles = new string[ShaderHelper.MAX_TEXTURES];
			Array.Copy(engine.TextureImageFilenames, newFiles, Math.Min(engine.TextureImageFilenames.Length, ShaderHelper.MAX_TEXTURES));
			for (int i = 0; i < Math.Min(externalTextureFilenames.Count, TEXTURE_OFFSET); i++)
			{
				newFiles[i] = externalTextureFilenames[i];
			}
			for (int i = 0; i < Math.Min(localTextureFilenames.Count, TEXTURE_OFFSET); i++)
			{
				newFiles[i + TEXTURE_OFFSET] = localTextureFilenames[i];
			}
			engine.TextureImageFilenames = newFiles;
		}


		public Earth3DControl()
		{
			InitializeComponent();
			engine.Engine.Camera.IsFirstPerson = true;
			earthTiles.EngineShapeList = engine.Engine.ShapeList;
		}

		private void engine_CameraChanged(object sender, EventArgs e)
		{
			if (this.isInitialized)
			{
				RestrictCameraElevation();
				if (previousCameraLocation.X != CameraLocation.X || previousCameraLocation.Z != CameraLocation.Z || previousCameraLocation.Y != CameraLocation.Y)
				{
					LatLong latLong = earthTiles.ConvertCameraLocationToLatLong(CameraLocation);
					latLong = earthTiles.CalculateNearestLatLongAtCurrentDelta(latLong);
					earthTiles.InitializeAtCameraLocation(CameraLocation);
					debugString = "" + latLong + "," + CameraLocation.Y;
					earthTiles.CameraLocationChanged(CameraLocation);
				}
			}
			previousCameraLocation = CameraLocation;
		}

		private void RestrictCameraElevation()
		{
			Float3 loc = CameraLocation;
			if (loc.Y < -100)
				loc.Y = -100;
			if (loc.Y > 12000)
				loc.Y = 12000;
			engine.CameraLocation = loc.AsVector3();
		}

		private void SetCameraLocation(Float3 camLoc)
		{
			engine.CameraLocation = camLoc.AsVector3();
		}

		private void Earth3DControl_Load(object sender, EventArgs e)
		{
			if (this.isInitialized)
			{
				this.CurrentLatLong = new LatLong(-36.827, 174.812);
				earthTiles.InitializeAtGivenLatLongElevation(CurrentLatLong, CurrentElevation);
				engine.Engine.UpdateShapes();
				earthTiles.MapChanged += (o, ev) => { engine.Engine.UpdateShapes(); };
			}
		}
	}
}
