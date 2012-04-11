using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Windows.Forms;
using SlimDX;
using Direct3DExtensions;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestMeshOptimiser
	{
		D3DHostForm form;
		Direct3DEngine engine;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new D3DHostForm();
		}

		[TearDown]
		public void TearDown()
		{
			if (form != null && !form.IsDisposed)
				form.Dispose();
			form = null;
			GC.Collect();
		}


		[Test]
		public void TestMeshAddition()
		{
			engine = new Test3DEngine();
			form.SetEngine(engine);
			form.Text = "F8 to change FillMode";
			engine.InitializationComplete += (o, e) => { AddCustomGeometry(); };
			Application.Run(form);
		}


		[Test]
		public void TestExTerrainOptimise()
		{
			engine = new Test3DEngine();
			form.SetEngine(engine);
			form.Text = "F8 to change FillMode";
			engine.InitializationComplete += (o, e) => { AddExTerrain(); };
			Application.Run(form);
		}

		void AddCustomGeometry()
		{
			engine.Geometry.Clear();	// This clears any meshes created by the Engine
			using (MeshFactory factory = new MeshFactory())
			{
				Mesh mesh1 = factory.CreateGrid(4, 4, 4, 4,false);
				Mesh mesh2 = factory.CreateGrid(4, 4, 4, 4, false);
				mesh2.Translation = new Vector3(2, 0, 2);
				MeshOptimiser.AddMesh(mesh1, mesh2);
				engine.BindMesh(mesh1, 2);
			}
		}

		void AddExTerrain()
		{
			engine.Geometry.Clear();	// This clears any meshes created by the Engine
			Mesh mesh = new Direct3DExtensions.Terrain.TerrainMeshSet();
			engine.BindMesh(mesh, "PosOnly");

		}
	}
}
