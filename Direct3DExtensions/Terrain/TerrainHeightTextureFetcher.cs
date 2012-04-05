using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;

namespace Direct3DExtensions.Terrain
{
	public interface TerrainHeightTextureFetcher
	{
		/// <summary>
		/// Fetches terrain height information by specifying a region in Latitude/Longitude notation.  The actual output size in pixels is kind of variable
		/// and depends a lot on the values of PixelsPerLatitude, PixelsPerLongitude and some rounding which is hard to control.
		/// </summary>
		/// <param name="regionInLongLatNotation"></param>
		/// <returns></returns>
		short[,] FetchTerrain(RectangleF regionInLongLatNotation);
		/// <summary>
		/// Fetches terrain height information by specifying the central Latitude/Longitude and then select the region as offsets from this point in pixels.
		/// E.g, staringLatLong = (-36,174) would have Auckland, NZ close to the centre of the region if the top/left of the rectangle were (0,0).
		/// This has the advantage that the output size is guaranteed to be the size of the rectangle, and adjacent regions can be easily calculated by
		/// adding an offset to the rectangle position instead of calculating new lat/long coordinates.
		/// </summary>
		/// <param name="startingLongLat"></param>
		/// <param name="regionInPixels"></param>
		/// <returns></returns>
		short[,] FetchTerrain(PointF startingLongLat, Rectangle regionInPixels);
		int PixelsPerLatitude { get; }
		int PixelsPerLongitude { get; }
		string SrtmDataFolder { get; set; }
	}

	public class TerrainDescriptor
		{
			public PointF longLat { get; set; }
			public Rectangle regionInPixels { get; set; }

			// override object.Equals
			public override bool Equals(object obj)
			{
				if (obj == null || GetType() != obj.GetType())
				{
					return false;
				}
				TerrainDescriptor other = obj as TerrainDescriptor;
				if (this.longLat != other.longLat)
					return false;
				if (this.regionInPixels != other.regionInPixels)
					return false;
				return true;
			}

			// override object.GetHashCode
			public override int GetHashCode()
			{
				return longLat.GetHashCode() ^ regionInPixels.GetHashCode();
			}
	}

	public class MemorySet<T1,T2> where T2 : IConvertible
	{
		class TimestampedData
		{
		
			T2[,] tag;
			public int LastAccessTime { get; private set; }
			public TimestampedData(T2[,] data)
			{
				this.tag = data;
				Tap();
			}

			public T2[,] GetData()
			{
				Tap();
				return tag;
			}

			void Tap()
			{
				LastAccessTime = Environment.TickCount;
			}

			// override object.Equals
			public override bool Equals(object obj)
			{
				if (obj == null || GetType() != obj.GetType())
				{
					return false;
				}
				return this.tag.Equals((obj as TimestampedData).tag);
			}

			// override object.GetHashCode
			public override int GetHashCode()
			{
				return tag.GetHashCode();
			}
		}

		Dictionary<T1, TimestampedData> memory = new Dictionary<T1, TimestampedData>();

		/// <summary>
		/// Maximum total size, in pixels, that the stack will store.
		/// </summary>
		public long MaxSize { get; set; }

		public MemorySet()
		{
			MaxSize = 64;
		}

		public bool Contains(T1 value)
		{
			return memory.ContainsKey(value);
		}

		public void Add(T1 value, T2[,] array)
		{
			if (memory.Count * array.Length > MaxSize)
				RemoveOldestElement();

			TimestampedData data = new TimestampedData(array);
			memory[value] = data;
		}

		public T2[,] Get(T1 value)
		{
			if (!Contains(value))
				return null;
			TimestampedData data = memory[value];
			return data.GetData();
		}

		void RemoveOldestElement()
		{
			if (memory.Count < 1) return;
			int min = int.MaxValue;
			T1 minKey = default(T1);
			foreach(KeyValuePair<T1,TimestampedData> pair in memory)
				if (pair.Value.LastAccessTime < min)
				{
					min = pair.Value.LastAccessTime;
					minKey = pair.Key;
				}
			memory.Remove(minKey);
		}
	}
}
