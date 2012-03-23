using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using SlimDX;
using D3D = SlimDX.Direct3D10;

namespace Direct3DExtensions
{
	public class TextureArray : DisposablePattern
	{
		D3D.Texture2D[] texArray;
		D3D.EffectResourceVariable textureResource;
		public D3D.Texture2D tex2DArray;
		public D3D.Texture3D tex3D;
		D3D.ShaderResourceView tex2DArrayView;
		D3D.ShaderResourceView tex3DView;
		D3D.Texture2D tex2D;
		D3D.ShaderResourceView tex2DView;

		D3D.Device device;

		public D3D.Texture2D this[int index] { get { return texArray[index]; } }
		public int Length { get { return texArray.Length; } }

		public TextureArray(D3D.Device device, D3D.Effect effect, string textureName, int arraySize)
		{
			this.device = device;
			
			string folder = @"C:\Users\adrianj\Pictures\CS Patterns\";
			string[] filenames = { 
									 "bslash.bmp",
									 "bsquare2.bmp",
									 "circle.bmp",
									 "cross.bmp"
								 };
			D3D.ImageLoadInformation loadInfo = new D3D.ImageLoadInformation()
			{
				BindFlags = D3D.BindFlags.None,
				CpuAccessFlags = D3D.CpuAccessFlags.Read | D3D.CpuAccessFlags.Write,
				FilterFlags = D3D.FilterFlags.None,
				Format = SlimDX.DXGI.Format.R8_UNorm,
				MipFilterFlags = D3D.FilterFlags.None,
				OptionFlags = D3D.ResourceOptionFlags.None,
				Usage = D3D.ResourceUsage.Staging
			};
			texArray = new D3D.Texture2D[arraySize];
			for (int i = 0; i < arraySize; i++)
			{
				texArray[i] = D3D.Texture2D.FromFile(device, folder + filenames[i], loadInfo);
			}
			
			 

			int w = 64;
			D3D.Texture2DDescription srcDesc = texArray[0].Description;

			D3D.Texture2DDescription td2 = new D3D.Texture2DDescription()
			{
				ArraySize = 1,
				BindFlags = D3D.BindFlags.ShaderResource,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				Format = SlimDX.DXGI.Format.R32_Float,
				Height = w,
				Width = w,
				MipLevels = 1,
				OptionFlags = D3D.ResourceOptionFlags.None,
				Usage = D3D.ResourceUsage.Default,
				SampleDescription = new SlimDX.DXGI.SampleDescription(1,0)
			};

			tex2D = new D3D.Texture2D(device, td2);

			td2.ArraySize = arraySize;

			tex2DArray = new D3D.Texture2D(device, td2);

			D3D.Texture3DDescription tDesc = new D3D.Texture3DDescription()
			{
				BindFlags = D3D.BindFlags.ShaderResource,
				CpuAccessFlags = D3D.CpuAccessFlags.None,
				Depth = arraySize,
				Format =  SlimDX.DXGI.Format.R32_Float,
				Height = w,
				Width = w,
				MipLevels = 1,
				OptionFlags = D3D.ResourceOptionFlags.None,
				Usage = D3D.ResourceUsage.Default
			};
			tex3D = new D3D.Texture3D(device, tDesc);

			float[] data = new float[w*w];
		
			arraySize = 1;
			for (int a = 0; a < arraySize; a++)
			{
				for (int i = 0; i < data.Length; i++)
					data[i] = (float)i * (a + 1) * 2;
				//var map = texArray[a].Map(0, 0, D3D.MapMode.Read, D3D.MapFlags.None);
				DataStream stream = new DataStream(data, true, true);
				stream.Position = 0;
				stream.WriteRange(data);
				//device.UpdateSubresource(box, tex2DArray, a);
				//device.UpdateSubresource(box, tex3D, a);
				stream.Position = 0;
				DataBox box = new DataBox(4 * w, 4 * w * w, stream);
				box.Data.Position = 0;
				device.UpdateSubresource(box, tex2D, 0);
				//texArray[a].Unmap(0);
				/*
				DataBox map = tex2DArray.Map(a, 0, D3D.MapMode.WriteDiscard, D3D.MapFlags.None);
				map.Data.Seek(data.Length * sizeof(float) * a, System.IO.SeekOrigin.Begin);
				map.Data.WriteRange(data);
				
				tex2DArray.Unmap(a);
				 */
			}

			tex2DArrayView = new D3D.ShaderResourceView(device, tex2DArray);
			tex3DView = new D3D.ShaderResourceView(device, tex3D);
			tex2DView = new D3D.ShaderResourceView(device, tex2D);

			BindToEffect(effect, "HiresMap");

		}

		private void BindToEffect(D3D.Effect effect, string textureName)
		{
			textureResource = effect.GetVariableByName(textureName).AsResource();
			textureResource.SetResource(tex2DArrayView);
			//BuildTextureViews();
		}


		public void WriteTexture<T>(T[,] data, int arrayIndex) where T : IConvertible
		{
			//texArray[arrayIndex].WriteTexture(data);

		}

		

		void DisposeManaged() { }
		void DisposeUnmanaged()
		{
			for (int i = 0; i < texArray.Length; i++)
			{
				if (texArray[i] != null) 
					texArray[i].Dispose(); 
				texArray[i] = null;
			}
		}

		bool disposed = false;
		protected override void Dispose(bool disposing)
		{
			if (!disposed)
			{
				if (disposing)
				{
					DisposeManaged();
				}

				DisposeUnmanaged();
				disposed = true;
			}
			base.Dispose(disposing);
		}
    
	}
}
