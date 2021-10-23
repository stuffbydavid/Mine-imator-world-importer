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
        public StorageFormat storageFormat;

        /// <summary>Initializes a region, which contains a grid of 32x32 chunks. Use Load() to load the region into memory.</summary>
        /// <param name="filename">Filename of the region, used to load it into memory.</param>
        /// <param name="x">The x value of the region.</param>
        /// <param name="y">The y value of the region.</param>
        public Region(string filename, BlockFormat blockFormat, StorageFormat storageFormat, int x, int y)
		{
			this.filename = filename;
            this.blockFormat = blockFormat;
            this.storageFormat = storageFormat;
            X = x;
			Y = y;
			isLoaded = false;
		}

		/// <summary>Loads the chunks of the region. Returns whether successful.</summary>
		public bool Load()
		{
			if (!File.Exists(filename))
				return false;

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

						Chunk chunk = new Chunk(br.ReadBytes(clen - 6), blockFormat, storageFormat);
						if (!chunk.Load())
							continue;

						chunks[Util.ModNeg(chunk.X, 32), Util.ModNeg(chunk.Y, 32)] = chunk;
					}
				}
				isLoaded = true;
			}
			catch (IOException)
			{
				MessageBox.Show(main.GetText("worldopened"));
				return false;
			}

			load.Close();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			return true;
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