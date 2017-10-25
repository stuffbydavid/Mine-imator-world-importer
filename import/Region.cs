using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace import
{
	public class Region
	{
		public int X, Y;
		public bool isLoaded;
		public string filename;
		public Chunk[,] chunks = new Chunk[32, 32];

		/// <summary>Initializes a region, which contains a grid of 32x32 chunks. Use Load() to load the region into memory.</summary>
		/// <param name="filename">Filename of the region, used to load it into memory.</param>
		/// <param name="x">The x value of the region.</param>
		/// <param name="y">The y value of the region.</param>
		public Region(string filename, int x, int y)
		{
			this.filename = filename;
			X = x;
			Y = y;
			isLoaded = false;
		}

		/// <summary>Loads the chunks of the region and their blocks into memory.</summary>
		public void Load()
		{
			if (!File.Exists(filename))
				return;

			NBTReader nbt = new NBTReader();
			frmLoadingRegion load = new frmLoadingRegion();
			load.Show();
			load.Text = "Loading region " + X + "," + Y + "...";

			// Process region file
			// http://minecraft.gamepedia.com/Region_file_format
			using (FileStream fs = new FileStream(filename, FileMode.Open))
			{
				BinaryReader br = new BinaryReader(fs);
				List<int> chunkoff = new List<int>();
				for (int c = 0; c < 32 * 32; c++)
				{
					int off = Util.ReadInt24(br); // Offset (4KiB sectors)
					if (off > 0)
						chunkoff.Add(off);
					br.ReadByte(); // Sector count
				}

				// Process the chunks
				// http://minecraft.gamepedia.com/Chunk_format
				for (int c = 0; c < chunkoff.Count; c++)
				{
					fs.Seek(chunkoff[c] * 4096, 0);
					int clen = Util.ReadInt32(br) - 1;
					br.ReadByte(); //Always 2

					// Decompress and read
					br.ReadByte();
					br.ReadByte();
					NBTCompound nbtChunk = nbt.Open(br.ReadBytes(clen - 6), DataFormat.ZLIB);
					NBTCompound nbtLevel = nbtChunk.Get("Level");
					NBTList nbtSections = nbtLevel.Get("Sections");

					if (nbtSections == null || nbtSections.Length() == 0)
						continue;

					int chunkX = nbtLevel.Get("xPos");
					int chunkY = nbtLevel.Get("zPos");
					Chunk chunk = new Chunk(chunkX, chunkY);

					bool legacy = true;

					// Process sections
					for (int s = 0; s < nbtSections.Length(); s++)
					{
						NBTCompound nbtSection = nbtSections.Get(s);
						int sectionZ = nbtSection.Get("Y");
						int sectionIdArrayPos = nbtSection.Get("Blocks");
						int sectionDataArrayPos = nbtSection.Get("Data");

						Chunk.Section section = new Chunk.Section();

						// Process 16x16x16 block grid
						int pos = 0;
						for (int z = 0; z < 16; z++)
						{
							for (int y = 0; y < 16; y++)
							{
								for (int x = 0; x < 16; x++)
								{
									World.Block block = new World.Block();

									if (legacy)
									{
										block.legacyId = nbt.data[sectionIdArrayPos + pos];
										block.legacyData = (byte)(Util.Nibble4(nbt.data[sectionDataArrayPos + pos / 2], pos % 2 == 0));
									}
									else
									{
										//TODO
									}

									section.blocks[x, y, z] = block;
									pos++;
								}
							}
						}

						chunk.sections[sectionZ] = section;
					}

					chunks[Util.ModNeg(chunkX, 32), Util.ModNeg(chunkY, 32)] = chunk;
				}
			}

			load.Close();
			isLoaded = true;

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		/// <summary>Clears the region of all chunks.</summary>
		public void Clear()
		{
			for (int i = 0; i < 32; i++)
				for (int j = 0; j < 32; j++)
					chunks[i, j] = null;
		}
	}
}