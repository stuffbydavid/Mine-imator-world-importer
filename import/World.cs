using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using static import.frmImport;

namespace import
{
    /// <summary>Minecraft block format.</summary>
    public enum BlockFormat
    {
        LEGACY = 0,
        AQUATIC = 1,
		NETHER = 2,
		CAVES_CLIFFS = 3
    }

    /// <summary>A Minecraft world, represented as a collection or regions containing 32x32 chunks.</summary>
    public class World
	{
		public const short BLOCKFORMAT_AQUATIC_VERSION = 1451;
		public const short BLOCKFORMAT_NETHER_VERSION = 2529;
		public const short BLOCKFORMAT_CAVESCLIFFS_VERSION = 2838;

        public const int SCHEMATIC_VERSION = 1;
		public const int SCHEMATIC_CONTENT_VERSION = 0;

		public const int WORLD_HEIGHT_MAX = 319;
		public const int WORLD_HEIGHT_MIN = -64;
		public const int WORLD_HEIGHT_SIZE = 384;
		public const int WORLD_CHUNK_SECTIONS = 64;

		public string filename = "", name = "";
		public BlockFormat blockFormat;
        public Point3D<int> spawnPos = new Point3D<int>(0, 0, 0);
		public Point3D<double> playerPos = new Point3D<double>(0, 0, 0);
		List<Region> regionList = new List<Region>();
		public Region[,] regions;
		int minRegionX, minRegionY, maxRegionX, maxRegionY;

		/// <summary>Loads the regions of the world into memory, returns whether successful.</summary>
		/// <param name="filename">The level.dat file of the world to load.</param>
		/// <param name="dim">Dimension to load.</param>
		public bool Load(string filename, DimOption dim)
		{
			if (!File.Exists(filename))
				return false;

			frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

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
				MessageBox.Show(main.GetText("worldunsupported"));
				return false;
			}

			// Look if dimension exists
			string regionFolder = new FileInfo(filename).DirectoryName;

			if (!dim.custom)
            {
				if (dim.dimID == 0) // Overworld
					regionFolder += @"\region";
				else
					regionFolder += @"\DIM" + dim.dimID + @"\region";
			}
            else
				regionFolder += @"\dimensions\" + dim.datapackName + "\\" + dim.name + @"\region";
		
			// Determine block format
			if (versionId >= BLOCKFORMAT_CAVESCLIFFS_VERSION)
				blockFormat = BlockFormat.CAVES_CLIFFS;
			else if (versionId >= BLOCKFORMAT_NETHER_VERSION)
				blockFormat = BlockFormat.NETHER;
			else if (versionId >= BLOCKFORMAT_AQUATIC_VERSION)
				blockFormat = BlockFormat.AQUATIC;
			else
				blockFormat = BlockFormat.LEGACY;

            // Set name
            FileInfo info = new FileInfo(filename);
			this.filename = filename;
			name = info.Directory.Name;

			// Clear old regions
			foreach (Region reg in regionList)
				reg.Clear();
			regionList.Clear();

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
			if (blockFormat >= BlockFormat.AQUATIC)
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
			minRegionX = minRegionY = maxRegionX = maxRegionY = 0;
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
					MessageBox.Show(main.GetText("worldopened"));
					filename = "";
					return false;
				}

				int rx = Convert.ToInt32(reg.Name.Split('.')[1]);
				int ry = Convert.ToInt32(reg.Name.Split('.')[2]);

				// Calculate world dimensions
				if (regionList.Count > 0)
				{
					minRegionX = Math.Min(rx, minRegionX); minRegionY = Math.Min(ry, minRegionY);
					maxRegionX = Math.Max(rx, maxRegionX); maxRegionY = Math.Max(ry, maxRegionY);
				}
				else
				{
					minRegionX = rx; minRegionY = ry;
					maxRegionX = rx; maxRegionY = ry;
				}
				regionList.Add(new Region(reg.FullName, blockFormat, rx, ry));
			}

			// Construct array of regions for lookup
			regions = new Region[((maxRegionX - minRegionX) + 1), ((maxRegionY - minRegionY) + 1)];
			foreach (Region reg in regionList)
				regions[reg.X - minRegionX, reg.Y - minRegionY] = reg;

			// Force garbage collection
			GC.Collect();
			GC.WaitForPendingFinalizers();

			return true;
		}

		/// <summary>Generates a schematic NBT structure of the selected region.</summary>
		/// <param name="region">The selected region of blocks to save.</param>
		public NBTCompound SaveBlocks(SaveRegion region)
		{
			SaveRegion tRegion = region.Copy();

			// Reset save
			foreach (Region reg in regionList)
				for (int x = 0; x < 32; x++)
					for (int y = 0; y < 32; y++)
						if (reg.chunks[x, y] != null)
							reg.chunks[x, y].tileEntitiesSaved = false;

			// Trim selection
			Action TrimXp = delegate
			{
				for (; tRegion.start.X < tRegion.end.X; tRegion.start.X++)
					for (int y = tRegion.start.Y; y <= tRegion.end.Y; y++)
						for (int z = tRegion.start.Z; z <= tRegion.end.Z; z++)
							if (IsBlockSaved(tRegion.start.X, y, z))
								return;
			};
			Action TrimXn = delegate
			{
				for (; tRegion.start.X < tRegion.end.X; tRegion.end.X--)
					for (int y = tRegion.start.Y; y <= tRegion.end.Y; y++)
						for (int z = tRegion.start.Z; z <= tRegion.end.Z; z++)
							if (IsBlockSaved(tRegion.end.X, y, z))
								return;
			};
			Action TrimYp = delegate
			{
				for (; tRegion.start.Y < tRegion.end.Y; tRegion.start.Y++)
					for (int x = tRegion.start.X; x <= tRegion.end.X; x++)
						for (int z = tRegion.start.Z; z <= tRegion.end.Z; z++)
							if (IsBlockSaved(x, tRegion.start.Y, z))
								return;
			};
			Action TrimYn = delegate
			{
				for (; tRegion.start.Y < tRegion.end.Y; tRegion.end.Y--)
					for (int x = tRegion.start.X; x <= tRegion.end.X; x++)
						for (int z = tRegion.start.Z; z <= tRegion.end.Z; z++)
							if (IsBlockSaved(x, tRegion.end.Y, z))
								return;
			};
			Action TrimZp = delegate
			{
				for (; tRegion.start.Z < tRegion.end.Z; tRegion.start.Z++)
					for (int x = tRegion.start.X; x <= tRegion.end.X; x++)
						for (int y = tRegion.start.Y; y <= tRegion.end.Y; y++)
							if (IsBlockSaved(x, y, tRegion.start.Z))
								return;
			};
			Action TrimZn = delegate
			{
				for (; tRegion.start.Z < tRegion.end.Z; tRegion.end.Z--)
					for (int x = tRegion.start.X; x <= tRegion.end.X; x++)
						for (int y = tRegion.start.Y; y <= tRegion.end.Y; y++)
							if (IsBlockSaved(x, y, tRegion.end.Z))
								return;
			};

			TrimXp();
			TrimXn();
			TrimYp();
			TrimYn();
			TrimZp();
			TrimZn();

			int len = (tRegion.end.Y - tRegion.start.Y) + 1;
			int wid = (tRegion.end.X - tRegion.start.X) + 1;
			int hei = (tRegion.end.Z - tRegion.start.Z) + 1;

			// Create schematic
			NBTCompound schematic = new NBTCompound();
			NBTList tileEntities = new NBTList(TagType.COMPOUND);

			schematic.Add(TagType.SHORT, "Length", len);
			schematic.Add(TagType.SHORT, "Width", wid);
			schematic.Add(TagType.SHORT, "Height", hei);

			NBTCompound palette = null;
			int[] blockDataArray = null;
			byte[] blockLegacyIdArray = null;
			byte[] blockLegacyDataArray = null;

			frmImport main = ((frmImport)Application.OpenForms["frmImport"]);
			bool filterWater = main.IsBlockFiltered("minecraft:water");

			if (blockFormat >= BlockFormat.AQUATIC)
			{
				// Use Sponge Schematic format
				// https://github.com/SpongePowered/Schematic-Specification/blob/master/versions/schematic-1.md

				schematic.Add(TagType.INT, "Version", SCHEMATIC_VERSION);

				NBTCompound metaData = new NBTCompound();
				metaData.Add(TagType.STRING, "FromMap", name);
				schematic.AddTag("Metadata", metaData);

				palette = new NBTCompound();
				palette.Add(TagType.INT, "minecraft:air", 0);
				blockDataArray = new int[len * wid * hei];
			}
			else
			{
				// Use legacy Schematic format
				// https://minecraft.gamepedia.com/Schematic_file_format

				schematic.Add(TagType.STRING, "FromMap", name);
				schematic.Add(TagType.STRING, "Materials", "Alpha");

				blockLegacyIdArray = new byte[len * wid * hei];
				blockLegacyDataArray = new byte[len * wid * hei];
			}

			int pos = 0;
			for (int z = tRegion.start.Z; z <= tRegion.end.Z; z++)
			{
				for (int y = tRegion.start.Y; y <= tRegion.end.Y; y++)
				{
					for (int x = tRegion.start.X; x <= tRegion.end.X; x++, pos++)
					{
						Chunk chunk = GetChunk(x, y);
						if (chunk == null)
							continue;

						// Add tile entities of newly iterated chunk
						if (!chunk.tileEntitiesSaved)
						{
							foreach (NBTTag tag in chunk.tileEntities.value)
							{
								NBTCompound comp = (NBTCompound)tag;
								string teId = comp.Get("id").value;
								int teX = comp.Get("x").value;
								int teY = comp.Get("z").value;
								int teZ = comp.Get("y").value;

								// Check bounds
								if (teX < tRegion.start.X || teX > tRegion.end.X ||
									teY < tRegion.start.Y || teY > tRegion.end.Y ||
									teZ < tRegion.start.Z || teZ > tRegion.end.Z)
									continue;

								// Subtract by start position in a copy
								NBTCompound newComp = (NBTCompound)comp.Copy();
								int[] posArr = { teX - tRegion.start.X, teZ - tRegion.start.Z, teY - tRegion.start.Y };

								if (blockFormat >= BlockFormat.AQUATIC)
								{
									newComp.Remove("id");
									newComp.Remove("x");
									newComp.Remove("y");
									newComp.Remove("z");

									newComp.Add(TagType.INT, "ContentVersion", SCHEMATIC_CONTENT_VERSION);
									newComp.Add(TagType.STRING, "Id", teId);
									newComp.Add(TagType.INT_ARRAY, "Pos", posArr);
								}
								else
								{
									newComp.Add(TagType.INT, "x", posArr[0]);
									newComp.Add(TagType.INT, "y", posArr[1]);
									newComp.Add(TagType.INT, "z", posArr[2]);
								}

								tileEntities.Add(newComp);
							}

							chunk.tileEntitiesSaved = true;
						}

						Chunk.Section section = chunk.sections[(z + 64) / 16];
						if (section == null)
							continue;

						// Position within current chunk section
						int sx = Util.ModNeg(x, 16);
						int sy = Util.ModNeg(y, 16);
						int sz = Util.ModNeg(z, 16);

						if (blockFormat >= BlockFormat.AQUATIC)
						{
							// Check filter
							if (!section.IsBlockSaved(sx, sy, sz))
							{
								blockDataArray[pos] = 0;
								continue;
							}

							short sectionPalettePos = section.blockPalettePos[sx, sy, sz];
							string sectionMcId = section.blockPaletteMcId[sectionPalettePos];
							NBTCompound sectionPaletteProperties = section.blockPaletteProperties[sectionPalettePos];

							// Remove waterlogged state if water is filtered
							if (filterWater && main.blockNameMap[sectionMcId].waterlogged)
							{
								bool hasWaterlogged = false;

								if (sectionPaletteProperties != null)
								{
									foreach (KeyValuePair<string, NBTTag> nbtPair in sectionPaletteProperties.value)
									{
										if (nbtPair.Key == "waterlogged" && nbtPair.Value.value == "true")
										{
											nbtPair.Value.value = "false";
											hasWaterlogged = true;
										}
									}
								}

								if (!hasWaterlogged)
								{
									if (sectionPaletteProperties == null)
										sectionPaletteProperties = new NBTCompound();

									sectionPaletteProperties.Add(TagType.STRING, "waterlogged", "false");
								}
							}

							// Construct ID:[properties] key
							string key = sectionMcId;
							if (sectionPaletteProperties != null)
								key += sectionPaletteProperties;

							// Find or create palette index
							NBTTag indexTag = palette.Get(key);
							int index;
							if (indexTag == null)
							{
								index = palette.Count();
								palette.Add(TagType.INT, key, index);
							}
							else
								index = indexTag.value;

							blockDataArray[pos] = index;
						}
						else
						{
							// Check filter
							if (!section.IsBlockSaved(sx, sy, sz))
							{
								blockLegacyIdArray[pos] = 0;
								continue;
							}

							byte legacyId = section.blockLegacyId[sx, sy, sz];
							blockLegacyIdArray[pos] = legacyId;

							// Not air
							if (legacyId > 0)
								blockLegacyDataArray[pos] = section.blockLegacyData[sx, sy, sz];
						}
					}
				}
			}

			schematic.AddTag("TileEntities", tileEntities);

			if (blockFormat >= BlockFormat.AQUATIC)
			{
				schematic.AddTag("Palette", palette);
				if (palette.Count() > byte.MaxValue)
				{
					// Int array
					schematic.Add(TagType.INT, "PaletteMax", palette.Count() * 4);
					schematic.Add(TagType.INT_ARRAY, "BlockData", blockDataArray);
				}
				else
				{
					// Byte array
					byte[] byteArr = new byte[blockDataArray.Count()];
					for (int i = 0; i < blockDataArray.Count(); i++)
						byteArr[i] = (byte)blockDataArray[i];

					schematic.Add(TagType.INT, "PaletteMax", palette.Count());
					schematic.Add(TagType.BYTE_ARRAY, "BlockData", byteArr);
				}
			}
			else
			{
				schematic.Add(TagType.BYTE_ARRAY, "Blocks", blockLegacyIdArray);
				schematic.Add(TagType.BYTE_ARRAY, "Data", blockLegacyDataArray);
			}

			return schematic;
		}

		/// <summary>Returns whether the block at x, y, z in the world should be saved.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		/// <param name="z">z value to check.</param>
		public bool IsBlockSaved(int x, int y, int z)
		{
			// Find section
			Chunk.Section sec = GetChunkSection(x, y, z);
			if (sec == null)
				return false;

			int sx = Util.ModNeg(x, 16);
			int sy = Util.ModNeg(y, 16);
			int sz = Util.ModNeg(z, 16);
			return sec.IsBlockSaved(sx, sy, sz);
		}

		/// <summary>Returns the region that contains all the blocks at x, y in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		/// <param name="returnLoaded">Only return fully-loaded regions.</param>
		public Region GetRegion(int x, int y, bool returnLoaded)
		{
			int cx = Util.IntDiv(x, 512);
			int cy = Util.IntDiv(y, 512);

			if (cx < minRegionX || cy < minRegionY || cx > maxRegionX || cy > maxRegionY)
				return null;

			Region reg = regions[cx - minRegionX, cy - minRegionY];
			if (reg != null && !reg.isLoaded && !reg.inQueue)
			{
				// Add to region queue
				frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

				main.regionQueue.Enqueue(reg);
				reg.inQueue = true;

				if (returnLoaded)
					return null;
				else
					return reg;
			}

			return reg;
		}

		/// <summary>Returns the chunk that contains all the blocks at x, y in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		public Chunk GetChunk(int x, int y)
		{
			Region reg = GetRegion(x, y, true);
			if (reg == null)
				return null;

			return reg.chunks[Util.ModNeg(Util.IntDiv(x, 16), 32), Util.ModNeg(Util.IntDiv(y, 16), 32)];
		}

		/// <summary>Returns the chunk section that contains all the blocks at x, y in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		/// <param name="z">z value to check.</param>
		public Chunk.Section GetChunkSection(int x, int y, int z)
		{
			Chunk chunk = GetChunk(x, y);
			if (chunk == null)
				return null;

			return chunk.sections[z / 16];
		}

		/// <summary>Returns the preview key of the block at x, y, z in the world.</summary>
		/// <param name="x">x value to check.</param>
		/// <param name="y">y value to check.</param>
		/// <param name="z">z value to check.</param>
		public short GetBlockPreviewKey(int x, int y, int z)
		{
			Chunk.Section sec = GetChunkSection(x, y, z);
			if (sec == null)
				return 0;

			int sx = Util.ModNeg(x, 16);
			int sy = Util.ModNeg(y, 16);
			int sz = Util.ModNeg(z, 16);

			// Check if filtered
			if (!sec.IsBlockSaved(sx, sy, sz))
				return 0;

			return sec.blockPreviewKey[sx, sy, sz];
		}

		/// <summary>Clears the images of all chunks.</summary>
		public void ClearChunkImages()
		{
			foreach (Region reg in regionList)
				for (int x = 0; x < 32; x++)
					for (int y = 0; y < 32; y++)
						if (reg.chunks[x, y] != null)
							reg.chunks[x, y].XYImage = reg.chunks[x, y].XZImage = null;
		}
	}
}
