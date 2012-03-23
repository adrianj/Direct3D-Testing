using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Direct3DExtensions;
using Direct3DExtensions.Terrain;
using System.Drawing;

namespace Direct3DExtensions_Test
{
	[TestFixture]
	public class TestTerrainFetcher
	{
		// I want the image centre around about Devonport, with width/height a power of two.
		float w = 1.6f;
		float h = 1.6f;
		float lat = -36.8f;
		float lng = 174.8f;
		RectangleF latLongRegion;
		short[,] terrain;

		[SetUp]
		public void SetUp()
		{
		}

		[Test]
		public void TestImageCreation()
		{
			short[,] terrain = new short[32, 64];
			for (int y = 0; y < terrain.GetLength(0); y++)
				for (int x = 0; x < terrain.GetLength(1); x++)
					terrain[y, x] = (short)(y * terrain.GetLength(1) + x);
			Image img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(6));
			ImagingFunctions.SaveAndDisplayImage(img);
		}

		[Test]
		public void TestStrm3()
		{
			latLongRegion = new RectangleF(lng - w * 0.5f, lat - h * 0.5f, w, h);
			long t0 = System.Environment.TickCount;
			long t1 = System.Environment.TickCount;
			TerrainHeightTextureFetcher fetcher = new Srtm3TextureFetcher();
			
			terrain = fetcher.FetchTerrain(latLongRegion);
			
			t0 = t1; t1 = System.Environment.TickCount;
			Console.WriteLine("Load: "+((t1-t0)/10));
			Image img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(256));
			t0 = t1; t1 = System.Environment.TickCount;
			Console.WriteLine("Copy Image: " + ((t1 - t0) / 10));
			ImagingFunctions.SaveAndDisplayImage(img, "terrainFetch.png");
			t0 = t1; t1 = System.Environment.TickCount;
			Console.WriteLine("Save and show: " + ((t1 - t0) / 10));
			
		}



		[Test]
		public void TestStrm3PixelRegion()
		{
			PointF longLat = new PointF(lng, lat);
			TerrainHeightTextureFetcher fetcher = new Srtm3TextureFetcher();
			Rectangle region = new Rectangle(0, 0, 1000, 1500);

			terrain = fetcher.FetchTerrain(longLat, region);

			Image img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(256));
			ImagingFunctions.SaveAndDisplayImage(img, "terrainFetchPixel030.png");
			region = new Rectangle(0, 1500, 1000, 1500);

			terrain = fetcher.FetchTerrain(longLat, region);

			img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(256));
			ImagingFunctions.SaveAndDisplayImage(img, "terrainFetchPixel032.png");
		}



		[Test]
		public void TestStrm30PixelRegion()
		{
			PointF longLat = new PointF(lng, lat);
			TerrainHeightTextureFetcher fetcher = new Srtm30TextureFetcher();
			Rectangle region = new Rectangle(0, 0, 2000, 1500);

			terrain = fetcher.FetchTerrain(longLat, region);

			Image img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(256));
			ImagingFunctions.SaveAndDisplayImage(img, "terrainFetchPixel300.png");
			region = new Rectangle(0, 1500, 2000, 1500);

			terrain = fetcher.FetchTerrain(longLat, region);

			img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(256));
			ImagingFunctions.SaveAndDisplayImage(img, "terrainFetchPixel302.png");
		}



		[Test]
		public void TestStrm30PixelMap()
		{
			PointF longLat = new PointF(lng, lat);
			TerrainHeightTextureFetcher fetcher = new Srtm30TextureFetcher();
			int w = 2048;
			int h = 2048;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 2; j++)
				{

					Rectangle region = new Rectangle(700 + i * w, 1800 - j * h, w, h);

					terrain = fetcher.FetchTerrain(longLat, region);

					Image img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(256));
					ImagingFunctions.SaveAndDisplayImage(img, "terrainFetchPixel04" + i + j + ".png");
				}
			}

		}

		[Test]
		public void TestStrm30()
		{
			w = 50;
			h = 20;
			latLongRegion = new RectangleF(lng - w * 0.5f, lat - h * 0.5f, w, h);
			long t0 = System.Environment.TickCount;
			long t1 = System.Environment.TickCount;
			TerrainHeightTextureFetcher fetcher = new Srtm30TextureFetcher();
			latLongRegion = new RectangleF(lng - w * 0.5f, lat - h * 0.5f, w, h);
			terrain = fetcher.FetchTerrain(latLongRegion);

			t0 = t1; t1 = System.Environment.TickCount;
			Console.WriteLine("Load: " + ((t1 - t0) / 10));
			DisplayTerrain(terrain, "terrainFetch30.png");
			t0 = t1; t1 = System.Environment.TickCount;
			Console.WriteLine("Save and show: " + ((t1 - t0) / 10));
		}

		public static void DisplayTerrain(short [,] terrain, string filename)
		{
			Image img = ImagingFunctions.CreateImageFromArray(terrain, ImagingFunctions.HsvColourMap(256));
			ImagingFunctions.SaveAndDisplayImage(img, filename);
		}
	}
}
