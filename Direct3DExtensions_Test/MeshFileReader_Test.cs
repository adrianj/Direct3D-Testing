using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Direct3DExtensions;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class MeshFileReader_Test
	{
		D3DHostForm form;
		Direct3DEngine engine;
		string hgtFile = @"C:\Users\adrianj\Documents\Work\CAD\WebGIS_SRTM3\S37E174.hgt";

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new D3DHostForm();
			engine = new Textured3DEngine();
			form.SetEngine(engine);
		}

		[Test]
		public void TestHGT()
		{
			engine.InitializationComplete += (o, e) =>
				{
					Mesh mesh = MeshFileReader.CreateFromFile(hgtFile);
					mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);
					engine.Geometry.Add(mesh);
					engine.CameraInput.LookAt(new SlimDX.Vector3(5, 10, -2), new SlimDX.Vector3(5, 0, 5));
				};
			Application.Run(form);
		}

	}
}
