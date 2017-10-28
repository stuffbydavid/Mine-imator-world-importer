using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace import
{
	/// <summary>Represents a grid of 16x16x256 blocks.</summary>
	public class Chunk
	{
		/// <summary>A section </summary>
		public class Section
		{
			public byte[,,] blockLegacyIds = new byte[16, 16, 16];
			public byte[,,] blockLegacyDatas = new byte[16, 16, 16];
		}

		public int X, Y;
		public FastBitmap XYImage, XZImage;
		public Section[] sections = new Section[16];
		public NBTList tileEntities;
		public bool tileEntitiesAdded = false;

		/// <summary>Initializes a new chunk at the given position and with the given amount of sections (slices with 16x16x16 blocks).</summary>
		/// <param name="x">x value to add the chunk.</param>
		/// <param name="y">y value to add the chunk.</param>
		public Chunk(int x, int y)
		{
			X = x; Y = y;

			XYImage = null;
			XZImage = null;

			for (int s = 0; s < 16; s++)
				sections[s] = null;
		}
	}
}
