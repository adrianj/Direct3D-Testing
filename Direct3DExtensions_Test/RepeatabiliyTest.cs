using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using System.Diagnostics;

using SlimDX;
using Direct3DExtensions;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class RepeatabiliyTest
	{

		D3DHostForm form;
		Direct3DEngine engine;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
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
		public void RunRepeatedly()
		{
			for (int i = 0; i < 5; i++)
			{
				System.Threading.Thread t = new System.Threading.Thread(new System.Threading.ThreadStart(ThreadProc));
				t.Start();
				for (int k = 0; k < 10; k++)
				{
					System.Threading.Thread.Sleep(200);
					if (form != null && form.IsDisposed) break;
				}
				Console.WriteLine("Requesting close");
				for (int k = 10; k >= 0; k--)
				{
					form.CloseByExternalThread();
					t.Join(200);
					if (t.ThreadState == System.Threading.ThreadState.Stopped)
						break;
					Console.WriteLine(""+t.ThreadState);
					if (k == 0)
						throw new Exception("Form did not close when requested");
				}
				Console.WriteLine("Close complete");
			}
		}

		Stopwatch watch = new Stopwatch();
		Random rand = new Random();

		[Test]
		public void KeepAddingShapes()
		{
			form = new D3DHostForm();
			engine = new TestObjectEngine();
			watch.Start();
			form.SetEngine(engine);
			engine.PostRendering += (o, e) => { AddObject(); };
			Application.Run(form);
		}

		void AddObject()
		{
			if (watch.ElapsedMilliseconds > 2000)
			{
				watch.Restart();
				Console.WriteLine("Add a shape");
				double x = 2 * rand.NextDouble() - 1;
				double y = 2 * rand.NextDouble() - 1;
				double z = 2 * rand.NextDouble() - 1;
				using (MeshFactory fact = new MeshFactory())
				{
					Mesh mesh = fact.CreateSphere(0.1f, 150, 150);
					mesh.Translation = new Vector3((float)x, (float)y, (float)z);
					mesh.BindToPass(engine.D3DDevice, engine.Effect, 2);
					engine.Geometry.Add(mesh);
				}
			}
		}

		private void ThreadProc()
		{
			form = new D3DHostForm();
			engine = new TestObjectEngine();
			form.SetEngine(engine);
			Application.Run(form);
		}
		
	}
}
