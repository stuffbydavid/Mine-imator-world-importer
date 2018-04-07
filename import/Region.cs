using System;
using System.Collections.Generic;
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
		public BlockFormat blockFormat;

		/// <summary>Initializes a region, which contains a grid of 32x32 chunks. Use Load() to load the region into memory.</summary>
		/// <param name="filename">Filename of the region, used to load it into memory.</param>
		/// <param name="x">The x value of the region.</param>
		/// <param name="y">The y value of the region.</param>
		public Region(string filename, BlockFormat blockFormat, int x, int y)
		{
			this.filename = filename;
			this.blockFormat = blockFormat;
			X = x;
			Y = y;
			isLoaded = false;
		}

		/// <summary>Loads the chunks of the region and processes the IDs/State IDs of the blocks.</summary>
		public void Load()
		{
			if (!File.Exists(filename))
				return;

			NBTReader nbt = new NBTReader();
			frmLoadingRegion load = new frmLoadingRegion();
			frmImport main = ((frmImport)Application.OpenForms["frmImport"]);
			load.Show();
			load.Text = main.GetText("loadingregion");

			// Process region file
			// http://minecraft.gamepedia.com/Region_file_format
			try
			{
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

						// Decompress and read NBT structure
						br.ReadByte();
						br.ReadByte();

						NBTCompound nbtChunk = nbt.Open(br.ReadBytes(clen - 6), DataFormat.ZLIB);
						NBTCompound nbtLevel = (NBTCompound)nbtChunk.Get("Level");
						NBTList nbtSections = (NBTList)nbtLevel.Get("Sections");

						if (nbtSections == null || nbtSections.Length() == 0)
							continue;

						int chunkX = nbtLevel.Get("xPos").value;
						int chunkY = nbtLevel.Get("zPos").value;
						Chunk chunk = new Chunk(chunkX , chunkY);
						chunk.tileEntities = nbtLevel.Get("TileEntities");

						// Process sections
						for (int s = 0; s < nbtSections.Length(); s++)
						{
							NBTCompound nbtSection = (NBTCompound)nbtSections.Get(s);
							Chunk.Section section = new Chunk.Section();
							section.Load(nbtSection, blockFormat);

							// Add section to chunk
							int sectionZ = nbtSection.Get("Y").value;
							chunk.sections[sectionZ] = section;
						}

						chunks[Util.ModNeg(chunkX, 32), Util.ModNeg(chunkY, 32)] = chunk;
					}
				}
				isLoaded = true;
			}
			catch (IOException)
			{
				MessageBox.Show(main.GetText("worldopened"));
			}

			load.Close();
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

		/// <summary>Runs before saving a schematic.</summary>
		public void SaveReset()
		{
			for (int i = 0; i < 32; i++)
				for (int j = 0; j < 32; j++)
					if (chunks[i, j] != null)
						chunks[i, j].tileEntitiesAdded = false;
		}
	}
}