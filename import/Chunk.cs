using System;
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
			public bool[] blockStateBits;
			public NBTList blockPalette;
			public byte[,,] blockLegacyId;
			public byte[,,] blockLegacyData;

			/// <summary>Parses the blocks of the section from the given NBT structure and stores the data.</summary>
			/// <param name="nbtSection">The NBT data of the section.</param>
			/// <param name="blockFormat">The format of the blocks in the section.</param>
			public void Load(NBTCompound nbtSection, BlockFormat blockFormat)
			{
				frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

				// 1.13 world format
				if (blockFormat == BlockFormat.MODERN)
				{
					blockPalette = nbtSection.Get("Palette");
					if (blockPalette == null)
						return;

					long[] sectionStateArray = nbtSection.Get("BlockStates").value;

					// Generate bit array
					List<byte> bytes = new List<byte>();
					blockStateBits = new bool[sectionStateArray.Length * 64];
					foreach (long l in sectionStateArray)
						bytes.AddRange(System.BitConverter.GetBytes(l));
					new BitArray(bytes.ToArray()).CopyTo(blockStateBits, 0);

					// Create lists
					List<string> blockMcIds = new List<string>();
					List<NBTCompound> blockProperties = new List<NBTCompound>();
					foreach (NBTCompound nbtBlockCompound in blockPalette.value)
					{
						blockMcIds.Add(nbtBlockCompound.Get("Name").value);
						blockProperties.Add(nbtBlockCompound.Get("Properties"));
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
									blockPreviewKey[x, y, z] = main.GetBlockPreviewKey(blockMcIds[palettePos], blockProperties[palettePos]);
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
		}

		public int X, Y;
		public FastBitmap XYImage, XZImage;
		public Section[] sections;
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

			sections = new Section[16];
			for (int s = 0; s < 16; s++)
				sections[s] = null;
		}
	}
}
