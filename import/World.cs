using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace import
{
	/// <summary>A Minecraft dimension.</summary>
	public enum Dimension
	{
		OVERWORLD = 0,
		NETHER = -1,
		END = 1
	}

	/// <summary>Minecraft block format.</summary>
	public enum BlockFormat
	{
		LEGACY = 0,
		MODERN = 1
	}

	/// <summary>A Minecraft world, represented as a collection or regions containing 32x32 chunks.</summary>
	public class World
	{
		public const short BLOCKFORMAT_MODERN_VERSION = 1451;

		public string filename = "", name = "";
		public BlockFormat blockFormat;
		public Point3D<int> spawnPos = new Point3D<int>(0, 0, 0);
		public Point3D<double> playerPos = new Point3D<double>(0, 0, 0);
		public List<Region> regions = new List<Region>();

		/// <summary>Loads the regions of the world into memory, returns whether successful.</summary>
		/// <param name="filename">The level.dat file of the world to load.</param>
		/// <param name="dim">Dimension to load.</param>
		public bool Load(string filename, Dimension dim)
		{
			if (!File.Exists(filename))
				return false;

			// Look if level.dat is valid NBT and determine block format
			NBTReader nbt = new NBTReader();
			int versionId = 0;
			try
			{
				NBTCompound nbtLevelRoot = nbt.Open(File.ReadAllBytes(filename), DataFormat.GZIP);
				NBTCompound nbtLevelData = nbtLevelRoot.Get("Data");
				NBTCompound nbtLevelVersion = nbtLevelData.Get("Version");
				if (nbtLevelVersion != null)
					versionId = nbtLevelVersion.Get("Id").value;
			}
			catch (Exception e)
			{
				MessageBox.Show(((frmImport)Application.OpenForms["frmImport"]).GetText("worldunsupported"));
				return false;
			}

			// Look if dimension exists
			string regionFolder = new FileInfo(filename).DirectoryName;
			if (dim == Dimension.OVERWORLD)
				regionFolder += @"\region";
			else
				regionFolder += @"\DIM" + (int)dim + @"\region";
		
			// Determine block format
			if (versionId >= BLOCKFORMAT_MODERN_VERSION)
				blockFormat = BlockFormat.MODERN;
			else
				blockFormat = BlockFormat.LEGACY;

			// Set name
			FileInfo info = new FileInfo(filename);
			this.filename = filename;
			name = info.Directory.Name;

			// Clear old regions
			for (int r = 0; r < regions.Count; r++)
				regions[r].Clear();
			regions.Clear();

			if (!Directory.Exists(regionFolder))
				return true;

			// Get player and spawn position from level.dat
			NBTCompound nbtRoot = nbt.Open(File.ReadAllBytes(filename), DataFormat.GZIP);
			NBTCompound nbtData = (NBTCompound)nbtRoot.Get("Data");

			// Player position
			if (nbtData.Get("Player") != null)
			{ 
				NBTList nbtPlayerPos = (NBTList)nbtData.Get("Player").Get("Pos");
				playerPos.X = nbtPlayerPos.Get(0).value;
				playerPos.Y = nbtPlayerPos.Get(2).value;
				playerPos.Z = nbtPlayerPos.Get(1).value;
			}

			// Spawn position
			spawnPos.X = nbtData.Get("SpawnX").value;
			spawnPos.Y = nbtData.Get("SpawnZ").value;
			spawnPos.Z = nbtData.Get("SpawnY").value;

			// Player position from player.dat
			if (blockFormat == BlockFormat.MODERN)
			{
				string playerFolder = new FileInfo(filename).DirectoryName + @"\playerdata";
				FileInfo[] playerFiles = new DirectoryInfo(playerFolder).GetFiles("*.dat");
				if (playerFiles.Length > 0)
				{
					// Get player and spawn position from latest changed player.dat file
					FileInfo playerDat = playerFiles.OrderByDescending(f => f.LastWriteTime).First();
					NBTCompound playerDatNbtRoot = nbt.Open(File.ReadAllBytes(playerDat.FullName), DataFormat.GZIP);

					// Player position
					NBTList playerDatNbtPos = (NBTList)playerDatNbtRoot.Get("Pos");
					playerPos.X = playerDatNbtPos.Get(0).value;
					playerPos.Y = playerDatNbtPos.Get(2).value;
					playerPos.Z = playerDatNbtPos.Get(1).value;

					// Overwrite pawn position
					if (playerDatNbtRoot.Get("SpawnX") != null)
					{
						spawnPos.X = playerDatNbtRoot.Get("SpawnX").value;
						spawnPos.Y = playerDatNbtRoot.Get("SpawnZ").value;
						spawnPos.Z = playerDatNbtRoot.Get("SpawnY").value;
					}
				}
			}

			// Go through regions
			foreach (FileInfo reg in new DirectoryInfo(regionFolder).GetFiles("*.mca"))
			{
				// Check if not locked by another process
				try
				{
					FileStream fs = new FileStream(reg.FullName, FileMode.Open);
					fs.Close();
				}
				catch (IOException)
				{
					MessageBox.Show(((frmImport)Application.OpenForms["frmImport"]).GetText("worldopened"));
					regions.Clear();
					filename = "";
					return false;
				}

				int rx = Convert.ToInt32(reg.Name.Split('.')[1]);
				int ry = Convert.ToInt32(reg.Name.Split('.')[2]);
				regions.Add(new Region(reg.FullName, blockFormat, rx, ry));
			}

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();

			return true;
		}

		public void SaveBlocks(string filename, int xs, int ys, int zs, int xe, int ye, int ze)
		{

		}

		/// <summary>Returns the region that contains block x, y in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		public Region GetRegion(int x, int y)
		{
			int cx = Util.IntDiv(x, 512);
			int cy = Util.IntDiv(y, 512);

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

			return reg.chunks[Util.ModNeg(Util.IntDiv(x, 16), 32), Util.ModNeg(Util.IntDiv(y, 16), 32)];
		}

		/// <summary>Returns the block preview key found at x, y, z in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		/// <param name="z">z value to check.</param>
		public short GetBlockPreviewKey(int x, int y, int z)
		{
			Chunk chunk = GetChunk(x, y);
			if (chunk == null || chunk.sections[z / 16] == null)
				return 0;

			Chunk.Section sec = chunk.sections[z / 16];
			int sx = Util.ModNeg(x, 16);
			int sy = Util.ModNeg(y, 16);
			int sz = z % 16;
			return sec.blockPreviewKey[sx, sy, sz];
		}

		/// <summary>Runs before saving a schematic.</summary>
		public void SaveReset()
		{
			foreach (Region reg in regions)
				reg.SaveReset();
		}
	}
}
