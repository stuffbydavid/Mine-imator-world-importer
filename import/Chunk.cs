using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace import
{
	/// <summary>Represents a 16x16x256 collection of blocks and their additional data.</summary>
	public class Chunk
	{
		public class Section
		{
			public World.Block[,,] blocks = new World.Block[16, 16, 16];
		}

		public int X, Y;
		public FastBitmap XYImage, XZImage;
		public bool hasXYImage, hasXZImage;
		public Section[] sections = new Section[16];

		/// <summary>Initializes a new chunk at the given position and with the given amount of sections (slices with 16x16x16 blocks).</summary>
		/// <param name="x">x value to add the chunk.</param>
		/// <param name="y">y value to add the chunk.</param>
		public Chunk(int x, int y)
		{
			X = x; Y = y;

			hasXYImage = false;
			hasXZImage = false;

			for (int s = 0; s < 16; s++)
				sections[s] = null;
		}
	}
}
