using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Direct3DExtensions
{

	public class TestObjectEngine : Textured3DEngine
	{
		
		protected override void InitGeometry()
		{
			base.InitGeometry();
			CreateShapes(this);
		}

		private static void CreateShapes(Direct3DEngine engine)
		{
			Mesh mesh = CreateSimpleMesh();
			engine.Geometry.Add(mesh);
			mesh.BindToPass(engine.D3DDevice, engine.Effect, 1);
			using (MeshFactory factory = new MeshFactory())
			{
				mesh = factory.CreateSphere(0.5f, 12, 12);
				engine.Geometry.Add(mesh);
				mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);

				mesh = factory.CreateTorus(0.5f, 2, 12, 20);
				engine.Geometry.Add(mesh);
				mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);

				mesh = factory.CreateBox(1, 0, 1);
				engine.Geometry.Add(mesh);
				mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);
			}
		}
		
	}
}
