using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using NUnit.Framework;
using Direct3DExtensions;
using Direct3DExtensions.Texturing;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestVirtualTexture
	{
		D3DHostForm form;
		MultipleEffect3DEngine engine;
		Mesh mesh;
		int w = 32;
		int r = 32;
		float[,] data;
		Effect effect;
		ShaderTexture sTex;

		[SetUp]
		public void SetUp()
		{
			AppControl.SetUpApplication();
			engine = new MultipleEffect3DEngine();
			using (MeshFactory fact = new MeshFactory())
				mesh = fact.CreateGrid(r, r, r, r, true);
			form = new D3DHostForm();
			engine.Effect.ShaderFilename = @"Effects\VirtualTextureTest.fx";
			effect = engine.Effect;
			form.SetEngine(engine);
			data = new float[w, w];
			FillWithIncrementalData(data);
		}

		private void FillWithIncrementalData(float [,] data)
		{
			for (int y = 0; y < data.GetLength(0); y++)
				for (int x = 0; x < data.GetLength(1); x++)
					data[y, x] = (y * data.GetLength(1) + x) / (float)(data.Length);
		}

		private void FillWithValue(float[,] data, float value)
		{
			for (int y = 0; y < data.GetLength(0); y++)
				for (int x = 0; x < data.GetLength(1); x++)
					data[y, x] = value;
		}

		[Test]
		public void TestStagingTexture()
		{
			engine.InitializeDirect3D();
			StagingTexture st = CreateFilledStagingTexture(data);
			Assert.That(st.Description.Width, Is.EqualTo(w));
			Assert.That(st.Description.Height, Is.EqualTo(w));
			float[,] readData = st.ReadTexture();
			Assert.That(readData, Is.EquivalentTo(data));

		}

		[Test]
		public void TestTexture()
		{
			engine.InitializeDirect3D();
			sTex = CreateShaderTexture();
			Assert.Throws<InvalidOperationException>(() => sTex.WriteTexture(data));
			Assert.Throws<InvalidOperationException>(() => sTex.ReadTexture());
			StagingTexture staging = CreateFilledStagingTexture(data);
			sTex.WriteTexture(staging, 0, 0);
		}

		StagingTexture partFill;
		StagingTexture completeFill;
		bool fillWithGrey = false;
		double elapsed = 0;
		long fillTime = 50;
		long notFillTime = 50;

		[Test]
		public void TestTextureWithEffect()
		{
			
			engine.InitializationComplete += (o, e) => InitComplete();
			engine.PostRendering += (o, e) => PostRender(e);
			Application.Run(form);
		}

		void PostRender(RenderEventArgs e)
		{
			elapsed += e.TimeDelta;
			if (elapsed > 10)
			{
				elapsed = 0;
				if (fillWithGrey)
				{
					long start = System.Diagnostics.Stopwatch.GetTimestamp();

					sTex.WriteTexture(partFill, w*r/2, 0);
					long end = System.Diagnostics.Stopwatch.GetTimestamp();
					fillTime = (fillTime + end - start) / 2;
					Console.WriteLine("Filling: " + fillTime);
				}
				else
				{
					long start = System.Diagnostics.Stopwatch.GetTimestamp();

					sTex.WriteTexture(completeFill, 0, 0);
					long end = System.Diagnostics.Stopwatch.GetTimestamp();
					notFillTime = (notFillTime + end - start) / 2;
					Console.WriteLine("Not filling: " + notFillTime);
				}
				fillWithGrey = !fillWithGrey;
			}
		}

		void InitComplete()
		{
			float[,] newData = new float[w * r, w];
			FillWithValue(newData, 0.3f);
			partFill = CreateFilledStagingTexture(newData);
			newData = new float[w * r, w * r];
			FillWithValue(newData, 0.8f);
			completeFill = CreateFilledStagingTexture(newData);
			sTex = CreateShaderTexture();
			newData = new float[w * r, w];
			FillWithValue(newData, 0.5f);
			
			StagingTexture staging = CreateFilledStagingTexture(newData);
			sTex.WriteTexture(staging, 0, 0);

			for (int i = 0; i < r; i++)
			{
				staging = CreateFilledStagingTexture(data);
				sTex.WriteTexture(staging, i*w, i*w);
			}
			sTex.BindToEffect(effect, "tex");
			mesh.BindToPass(engine.D3DDevice, effect, "P1");
			engine.Geometry.Add(mesh);
		}

		private ShaderTexture CreateShaderTexture()
		{
			ShaderTexture st = new ShaderTexture(engine.D3DDevice.Device, w * r, w * r, SlimDX.DXGI.Format.R32_Float);
			return st;
		}

		private StagingTexture CreateFilledStagingTexture(float[,] data)
		{
			StagingTexture staging = new StagingTexture(engine.D3DDevice.Device, w, w, SlimDX.DXGI.Format.R32_Float);
			staging.WriteTexture(data);
			return staging;
		}

	}
}
