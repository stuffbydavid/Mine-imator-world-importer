﻿using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Forms;

namespace import
{
	/// <summary>Represents a grid of 16x16x256 blocks.</summary>
	public class Chunk
	{
		/// <summary>A section of 16x16x16 blocks.</summary>
		public class Section
		{
			public short[,,] blockPreviewKey = new short[16, 16, 16];
			public short[,,] blockPalettePos;
			public string[] blockPaletteMcId;
			public NBTCompound[] blockPaletteProperties;
			public byte[,,] blockLegacyId;
			public byte[,,] blockLegacyData;
			public BlockFormat blockFormat;

			/// <summary>Parses the blocks of the section from the given NBT structure and stores the data.</summary>
			/// <param name="nbtSection">The NBT data of the section.</param>
			/// <param name="blockFormat">The format of the blocks in the section.</param>
			public void Load(NBTCompound nbtSection, BlockFormat blockFormat)
			{
				this.blockFormat = blockFormat;
				frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

				// 1.13 world format
				if (blockFormat == BlockFormat.MODERN)
				{
					blockPalettePos = new short[16, 16, 16];

					NBTList nbtBlockPalette = nbtSection.Get("Palette");
					if (nbtBlockPalette == null)
						return;

					// Generate bit array
					long[] sectionStateArray = nbtSection.Get("BlockStates").value;
					List<byte> bytes = new List<byte>();
					bool[] blockStateBits = new bool[sectionStateArray.Length * 64];
					foreach (long l in sectionStateArray)
						bytes.AddRange(System.BitConverter.GetBytes(l));
					new BitArray(bytes.ToArray()).CopyTo(blockStateBits, 0);

					// Create palette
					blockPaletteMcId = new string[nbtBlockPalette.Length()];
					blockPaletteProperties = new NBTCompound[nbtBlockPalette.Length()];
					int p = 0;
					foreach (NBTCompound nbtBlockCompound in nbtBlockPalette.value)
					{
						blockPaletteMcId[p] = nbtBlockCompound.Get("Name").value;
						blockPaletteProperties[p] = nbtBlockCompound.Get("Properties");
						p++;
					}

					// Parse blocks
					int bitsPerBlock = blockStateBits.Length / (16 * 16 * 16);
					int longBitPos = 0;
					for (int z = 0; z < 16; z++)
					{
						for (int y = 0; y < 16; y++)
						{
							for (int x = 0; x < 16; x++)
							{
								int palettePos = 0;
								for (int b = 0; b < bitsPerBlock; b++)
									if (blockStateBits[longBitPos++])
										palettePos |= 1 << b;

								if (palettePos > 0)
									blockPreviewKey[x, y, z] = main.GetBlockPreviewKey(blockPaletteMcId[palettePos], blockPaletteProperties[palettePos]);

								blockPalettePos[x, y, z] = (short)palettePos;
							}
						}
					}
				}

				// Legacy IDs and data
				else
				{
					blockLegacyId = new byte[16, 16, 16];
					blockLegacyData = new byte[16, 16, 16];

					byte[] sectionLegacyIdArray = nbtSection.Get("Blocks").value;
					byte[] sectionLegacyDataArray = nbtSection.Get("Data").value;

					// Process 16x16x16 block grid
					int pos = 0;
					for (int z = 0; z < 16; z++)
					{
						for (int y = 0; y < 16; y++)
						{
							for (int x = 0; x < 16; x++)
							{
								byte legacyId = sectionLegacyIdArray[pos];
								byte legacyData = 0;

								if (legacyId > 0)
									legacyData = (Util.Nibble4(sectionLegacyDataArray[pos / 2], pos % 2 == 0));

								blockLegacyId[x, y, z] = legacyId;
								blockLegacyData[x, y, z] = legacyData;
								blockPreviewKey[x, y, z] = main.blockLegacyPreviewKey[legacyId, legacyData];
								pos++;
							}
						}
					}
				}
			}

			/// <summary>Returns whether the block at x, y, z in the section should be saved.</summary>
			/// <param name="x">x value to check.</param>
			/// <param name="y">y value to check.</param>
			/// <param name="z">z value to check.</param>
			public bool IsBlockSaved(int x, int y, int z)
			{
				frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

				string mcId;

				if (blockFormat == BlockFormat.MODERN)
				{
					short palettePos = blockPalettePos[x, y, z];
					if (palettePos == 0) // Air
						return false;

					mcId = blockPaletteMcId[palettePos];
				}
				else
				{
					byte legacyId = blockLegacyId[x, y, z];
					if (legacyId == 0) // Air
						return false;

					byte legacyData = blockLegacyData[x, y, z];
					mcId = main.blockLegacyMcId[legacyId, legacyData];
				}

				// Check user filter settings
				return !main.IsBlockFiltered(mcId);
			}
		}

		public byte[] data;
		public int X, Y;
		public FastBitmap XYImage, XZImage;
		public Section[] sections;
		public NBTList tileEntities;
		public bool tileEntitiesSaved;
		public BlockFormat blockFormat;

		/// <summary>Initializes a new chunk at the given position and with the given amount of sections (slices with 16x16x16 blocks).</summary>
		/// <param name="data">The uncompressed NBT Data of the chunk</param>
		public Chunk(byte[] data, BlockFormat blockFormat)
		{
			this.data = data;
			this.blockFormat = blockFormat;
			XYImage = null;
			XZImage = null;

			sections = new Section[16];
			for (int s = 0; s < 16; s++)
				sections[s] = null;
		}

		/// <summary>Loads the blocks of the chunk. Returns whether successful.</summary>
		public bool Load()
		{
			NBTReader nbt = new NBTReader();
			NBTCompound nbtChunk = nbt.Open(data, DataFormat.ZLIB);
			NBTCompound nbtLevel = (NBTCompound)nbtChunk.Get("Level");
			NBTList nbtSections = (NBTList)nbtLevel.Get("Sections");

			// No blocks in this chunk
			if (nbtSections == null || nbtSections.Length() == 0)
				return false;

			X = nbtLevel.Get("xPos").value;
			Y = nbtLevel.Get("zPos").value;
			tileEntities = nbtLevel.Get("TileEntities");

			// Process sections
			for (int s = 0; s < nbtSections.Length(); s++)
			{
				NBTCompound nbtSection = (NBTCompound)nbtSections.Get(s);
				Section section = new Section();
				section.Load(nbtSection, blockFormat);

				// Add section to chunk
				int sectionZ = nbtSection.Get("Y").value;
				sections[sectionZ] = section;
			}

			data = null;
			return true;
		}
	}
}
