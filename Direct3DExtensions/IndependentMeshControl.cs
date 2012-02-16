using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Direct3DExtensions
{
	public class IndependentMeshControl : Textured3DControl
	{
		protected override void InitGeometry()
		{
			base.InitGeometry();
			Mesh mesh = CreateSimpleMesh();
			Geometry.Meshes.Add(mesh);
			mesh.BindToPass(D3DDevice, Effect, 1);
			using (MeshFactory factory = new MeshFactory())
			{
				mesh = factory.CreateSphere(0.5f, 12, 12);
				Geometry.Meshes.Add(mesh);
				mesh.BindToPass(D3DDevice, Effect, 2);

				mesh = factory.CreateTorus(0.5f, 2, 12, 20);
				Geometry.Meshes.Add(mesh);
				mesh.BindToPass(D3DDevice, Effect, 2);

				mesh = factory.CreateBox(1, 0, 1);
				Geometry.Meshes.Add(mesh);
				mesh.BindToPass(D3DDevice, Effect, 2);
			}
		}

		protected override void Render()
		{
			CameraInput.OnRender();
			D3DDevice.Clear();
			//Effect.ApplyAll(CameraInput.Camera);
			Effect.ApplyPerFrameConstants(CameraInput.Camera);
			foreach (Mesh m in Geometry.Meshes)
			{
				Effect[m.EffectPassIndex].Apply();
				m.Draw();
			}
			//Geometry.Draw();
			D3DDevice.Present();
		}
	}
}
