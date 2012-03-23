using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;

using SlimDX;
using Direct3DExtensions;

namespace Direct3DExtensions_Test
{
	public static class AppControl
	{
		public static bool WindowSet = false;
		public static void SetUpApplication()
		{
			Application.EnableVisualStyles();
			if (!WindowSet)
			{

				Application.SetCompatibleTextRenderingDefault(false);
				WindowSet = true;
			}
		}
	}

	[TestFixture, RequiresSTA]
	public class Basic3DControl_Test
	{
		D3DHostForm form;
		D3DHostControl con;
		Direct3DEngine engine;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			form = new D3DHostForm();
			con = new D3DHostControl();
		}

		[TearDown]
		public void TearDown()
		{
			if (form != null && !form.IsDisposed)
				form.Dispose();
			if (engine != null)
			{
				Console.WriteLine("engine disp");
				engine.Dispose();
			}
			form = null;
			engine = null;
			GC.Collect();
		}

		[Test]
		public void CompletelyEmptyForm()
		{
			engine = new Basic3DEngine();
			Application.Run(form);
		}

		[Test]
		public void RunEmpty()
		{
			engine = new Basic3DEngine();
			form.SetEngine(engine);
			form.Text = "Run Basic";
			con.BackColor = System.Drawing.Color.LightSteelBlue;
			Assert.That(con.IsDesignMode, Is.Not.True);
			Application.Run(form);
			Assert.That(true);
		}

		[Test]
		public void RunTextured()
		{
				engine =  new TestObjectEngine();
				form.SetEngine(engine);
				form.Text = "Run Textured";
				con.BackColor = System.Drawing.Color.LightSteelBlue;
				Assert.That(con.IsDesignMode, Is.Not.True);
				Application.Run(form);
				Assert.That(true);
		}

		[Test]
		public void RunWithError()
		{
			engine = new Basic3DEngine();
			form.SetEngine(engine);
			engine.InitializationComplete += (o, e) => { throw new Exception("Ooops!"); };
			form.Text = "Run with Error";
			Application.Run(form);
		}

		[Test]
		public void TestAddCustomGeometry()
		{
			engine = new Test3DEngine();
			form.SetEngine(engine);
			form.Text = "F8 to change FillMode";
			engine.InitializationComplete += (o, e) => { AddCustomGeometry(); };
			Application.Run(form);
		}


		void AddCustomGeometry()
		{
			engine.Geometry.Clear();	// This clears any meshes created by the Engine
			using (MeshFactory factory = new MeshFactory())
			{

				Mesh mesh = factory.CreateGrid(10, 10, 4, 4, true);
				mesh.Translation = new Vector3(0, -20, 0);
				engine.BindMesh(mesh, 2);

				mesh = factory.CreateBox(0.2f, 0.5f, 1);
				mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);
				engine.Geometry.Add(mesh);
				mesh.Translation = new Vector3(-1, 0, 4);



				mesh = factory.CreateSquare(5, 5);
				mesh.Translation = new Vector3(0,1,0);
				mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);
				engine.Geometry.Add(mesh);

				mesh = factory.CreateSphere(2, 50, 50);
				mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);
				engine.Geometry.Add(mesh);
				mesh.Translation = new Vector3(4, 0, -1);

				//mesh = factory.CreateTeapot();
				//mesh.BindToPass(engine.D3DDevice, engine.Effect, "P2");
				//engine.Geometry.Add(mesh);


				mesh = new ExpandedPlane();
				(mesh as ExpandedPlane).Segments = new System.Drawing.Size(8, 8);
				mesh.Translation = new Vector3(0, -1, 0);
				mesh.Scale = new Vector3(10, 1, 10);
				engine.BindMesh(mesh, 2);
			}
		}

	}
}
