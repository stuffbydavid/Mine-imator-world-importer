using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.IO;

namespace import
{
	/// <summary>A Minecraft dimension.</summary>
	public enum Dimension
	{
		OVERWORLD = 0,
		NETHER = -1,
		END = 1
	}

	/// <summary>A Minecraft world, represented as a collection or regions containing 32x32 chunks.</summary>
	public class World
	{
		/// <summary>A Minecraft block in the world.</summary>
		public class Block
		{
			public string id = "";
			public byte legacyId, legacyData;
		}

		public string filename = "", name = "";
		public Point3D<int> spawnPos = new Point3D<int>(0, 0, 0);
		public Point3D<double> playerPos = new Point3D<double>(0, 0, 0);
		public List<Region> regions = new List<Region>();

		/// <summary>Loads the regions of the world into memory, returns whether successful.</summary>
		/// <param name="folder">The world folder to load, usually found in the .saves folder in the Minecraft directory.</param>
		/// <param name="dim">Dimension to load.</param>
		public bool Load(string filename, Dimension dim)
		{
			this.filename = filename;
			name = new DirectoryInfo(filename).Name;

			// Clear old regions
			for (int r = 0; r < regions.Count; r++)
				regions[r].Clear();
			regions.Clear();

			// Read level.dat
			NBTReader nbt = new NBTReader();
			NBTCompound nbtRoot = nbt.Open(File.ReadAllBytes(filename + @"\level.dat"), DataFormat.GZIP);
			NBTCompound nbtData = nbtRoot.Get("Data");

			// Player position
			NBTList nbtPlayerPos = nbtData.Get("Player").Get("Pos");
			playerPos.X = nbtPlayerPos.Get(0);
			playerPos.Y = nbtPlayerPos.Get(2);
			playerPos.Z = nbtPlayerPos.Get(1);

			// Spawn position
			spawnPos.X = nbtData.Get("SpawnX");
			spawnPos.Y = nbtData.Get("SpawnZ");
			spawnPos.Z = nbtData.Get("SpawnY");

			// Get regions
			if (dim == Dimension.OVERWORLD)
				filename += @"\region";
			else
				filename += @"\DIM" + (int)dim + @"\region";

			foreach (FileInfo reg in new DirectoryInfo(filename).GetFiles("*.mca"))
			{
				int rx = Convert.ToInt32(reg.Name.Split('.')[1]);
				int ry = Convert.ToInt32(reg.Name.Split('.')[2]);
				regions.Add(new Region(reg.FullName, rx, ry));

				try
				{
					FileStream fs = new FileStream(reg.FullName, FileMode.Open);
					fs.Close();
				}
				catch (IOException)
				{
					MessageBox.Show("This world is used by another process, probably Minecraft or MCEdit. Close them and try again.");
					regions.Clear();
					filename = "";
					return false;
				}
			}

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();

			return true;
		}

		/// <summary> Checks whether the given world and dimension can be loaded.</summary>
		/// <param name="folder">World folder, usually found in the .saves folder in the Minecraft directory.</param>
		/// <param name="dim">Dimension to load.</param>
		public bool CanLoad(string folder, Dimension dim)
		{
			if (!File.Exists(folder + @"\level.dat"))
				return false;

			if (dim == Dimension.OVERWORLD)
				folder += @"\region";
			else
				folder += @"\DIM" + dim + @"\region";

			return Directory.Exists(folder);
		}

		/// <summary>Returns the region that contains block x, y in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		public Region GetRegion(int x, int y)
		{
			int cx = x / 512;
			int cy = y / 512;

			Region reg = null;
			for (int i = 0; i < regions.Count; i++)
			{
				if (regions[i].X == cx && regions[i].Y == cy)
				{
					reg = regions[i];
					if (!reg.isLoaded)
						reg.Load();
					break;
				}
			}
			return reg;
		}

		/// <summary>Returns the chunk that contains block x, y in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		public Chunk GetChunk(int x, int y)
		{
			Region reg = GetRegion(x, y);
			if (reg == null)
				return null;

			return reg.chunks[Util.ModNeg(x, 512) / 16, Util.ModNeg(y, 512) / 16];
		}

		/// <summary>Returns the block ID and data found at x, y, z in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		/// <param name="z">z value to check.</param>
		public Block GetBlock(int x, int y, int z)
		{
			Chunk chunk = GetChunk(x, y);
			if (chunk == null || chunk.sections[z / 16] == null)
				return null;

			return chunk.sections[z / 16].blocks[Util.ModNeg(x, 16), Util.ModNeg(y, 16), z % 16];
		}
	}
}
