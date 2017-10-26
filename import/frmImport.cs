using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace import
{
	using JsonObject = Dictionary<string, dynamic>;
	using JsonNameValuePair = KeyValuePair<string, dynamic>;
	using JsonList = List<Dictionary<string, dynamic>>;

	public partial class frmImport : Form
	{
		/// <summary>A type of Minecraft block</summary>
		public class Block
		{
			/// <summary>Determines how a block is previewed in the top-down or cross-section view</summary>
			public class Preview
			{
				public Color XYColor, XZColor;

				public Preview(Color XYColor, Color XZColor)
				{
					this.XYColor = XYColor;
					this.XZColor = XZColor;
				}
			}

			/// <summary>A specific block state</summary>
			public class State
			{
				/// <summary>A possible value of the block state</summary>
				public class Value
				{
					public string name;
					public Block.Preview preview = null;

					public Value(string name)
					{
						this.name = name;
					}
				}

				public string name;
				public Dictionary<string, Block.State.Value> values = new Dictionary<string, Block.State.Value>();

				public State(string name)
				{
					this.name = name;
				}
			}

			public string name;
			public Block.Preview preview = null;
			public Dictionary<string, Block.State> states = new Dictionary<string, Block.State>();
			public Block.Preview[] legacyDataPreview = new Block.Preview[16];

			public Block(string name)
			{
				this.name = name;
				for (var i = 0; i < 16; i++)
					legacyDataPreview[i] = null;
			}
		}

		// Folders
		static string mcSaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\saves";
		static string dataFolder = @"D:\OneDrive\Projects\Minecraft\Mine-imator\Source\datafiles\Data";
		static string mcAssetsFile = dataFolder + @"\Minecraft\1.12.midata";
		static string miLangFile = dataFolder + @"\Languages\english.milanguage";
		static string miBlockPreviewFile = dataFolder + @"\blockpreview.midata";

		// Blocks
		public Dictionary<string, Block> blockNameMap = new Dictionary<string, Block>();
		public Dictionary<byte, Block> blockLegacyIdMap = new Dictionary<byte, Block>();
		public Dictionary<string, Block.Preview> blockPreviewMap = new Dictionary<string, Block.Preview>();

		// Filter
		public bool filterBlocksActive = false, filterBlocksInvert = false;
		public List<string> filterBlocks = new List<string>();

		// Program
		string savefile = "";
		World world = new World();

		Point moveStartMPos, moveStartPos, XYImageMidPos, XZImageMidPos;
		float XYImageZoom = 8, XZImageZoom = 4;
		byte XYDragView = 0, XYDragSelect = 0;
		byte XZDragView = 0, XZDragSelect = 0;

		Image spawnImage = import.Properties.Resources.spawn;
		Image playerImage = import.Properties.Resources.player;
		Point3D<int> selectStart, selectEnd;
		Point XYSelectStartDraw, XYSelectEndDraw, XYStart;
		Point XZSelectStartDraw, XZSelectEndDraw, XZStart;
		int XYBlocksWidth, XYBlocksHeight;
		int XZBlocksWidth, XZBlocksHeight;
		Bitmap XYMapBitmap = new Bitmap(1, 1);
		Bitmap XYSelectBitmap = new Bitmap(1, 1);
		Bitmap XZMapBitmap = new Bitmap(1, 1);
		Bitmap XZSelectBitmap = new Bitmap(1, 1);

		public frmImport(string[] args)
		{
			if (!File.Exists(mcAssetsFile))
			{
				MessageBox.Show("Could not find Minecraft assets, re-install the program.");
				Application.Exit();
			}

			if (!File.Exists(miLangFile))
			{
				MessageBox.Show("Could not find translation file, re-install the program.");
				Application.Exit();
			}

			if (!File.Exists(miBlockPreviewFile))
			{
				MessageBox.Show("Could not find block previews, run Mine-imator first!");
				Application.Exit();
			}

			InitializeComponent();
			LoadBlockPreviews(miBlockPreviewFile);
			LoadBlocks(mcAssetsFile);
			LoadFilterBlocks();

			savefile = "";
			for (int i = 0; i < args.Length; i++)
			{
				if (i > 0)
					savefile += " ";
				savefile += args[i];
			}
		}

		/// <summary>Loads the world from the combobox and resets the view.</summary>
		private void LoadWorld()
		{
			if (cbxSaves.SelectedIndex == -1)
				return;

			string worldfolder = mcSaveFolder + @"\" + cbxSaves.Items[cbxSaves.SelectedIndex];

			Dimension dim;
			if (rbtNether.Checked)
				dim = Dimension.NETHER;
			else if (rbtEnd.Checked)
				dim = Dimension.END;
			else
				dim = Dimension.OVERWORLD;

			if (!world.CanLoad(worldfolder, dim))
				return;

			if (world.Load(worldfolder, dim))
			{
				XYImageMidPos = new Point((int)world.playerPos.X, (int)world.playerPos.Y);
				XYImageZoom = 8;
				selectStart = new Point3D<int>((int)world.playerPos.X - 10, (int)world.playerPos.Y - 10, (int)world.playerPos.Z - 10);
				selectEnd = new Point3D<int>((int)world.playerPos.X + 10, (int)world.playerPos.Y + 10, (int)world.playerPos.Z + 10);
				selectStart.Z = Math.Max(Math.Min(selectStart.Z, 255), 0);
				selectEnd.Z = Math.Max(Math.Min(selectEnd.Z, 255), 0);
				UpdateSizeLabel();
				btnDone.Enabled = true;
				XZImageMidPos = new Point(selectStart.X + (selectEnd.X - selectStart.X) / 2, selectStart.Z + (selectEnd.Z - selectStart.Z) / 2);

				UpdateXYMap(0, 0);
				UpdateXZMap();
			}
			else
			{
				pboxWorldXY.Image = new Bitmap(1, 1);
				pboxWorldXZ.Image = new Bitmap(1, 1);
				btnDone.Enabled = false;
			}
		}

		/// <summary>Saves the selection into a .schematic file</summary>
		private void SaveBlocks(string filename)
		{
			int sx, sy, sz, ex, ey, ez;

			#region Trim unnecessary space
			sx = selectStart.X;
			sy = selectStart.Y;
			sz = selectStart.Z;
			ex = selectEnd.X;
			ey = selectEnd.Y;
			ez = selectEnd.Z;

			#region X+
			for (sx = selectStart.X; sx < selectEnd.X; sx++)
			{
				bool foundblock = false;
				for (int y = selectStart.Y; y <= selectEnd.Y; y++)
				{
					for (int z = selectStart.Z; z <= selectEnd.Z; z++)
					{
						World.Block block = world.GetBlock(sx, y, z);
						if (block != null && block.legacyId > 0 && !isFiltered(block))
						{
							foundblock = true;
							break;
						}
					}
					if (foundblock) break;
				}
				if (foundblock) break;
			}
			#endregion
			#region X-
			for (ex = selectEnd.X; ex > sx; ex--)
			{
				bool foundblock = false;
				for (int y = selectStart.Y; y <= selectEnd.Y; y++)
				{
					for (int z = selectStart.Z; z <= selectEnd.Z; z++)
					{
						World.Block block = world.GetBlock(ex, y, z);
						if (block != null && block.legacyId > 0 && !isFiltered(block))
						{
							foundblock = true;
							break;
						}
					}
					if (foundblock) break;
				}
				if (foundblock) break;
			}
			#endregion
			#region Y+
			for (sy = selectStart.Y; sy < selectEnd.Y; sy++)
			{
				bool foundblock = false;
				for (int x = selectStart.X; x <= selectEnd.X; x++)
				{
					for (int z = selectStart.Z; z <= selectEnd.Z; z++)
					{
						World.Block block = world.GetBlock(x, sy, z);
						if (block != null && block.legacyId > 0 && !isFiltered(block))
						{
							foundblock = true;
							break;
						}
					}
					if (foundblock) break;
				}
				if (foundblock) break;
			}
			#endregion
			#region Y-
			for (ey = selectEnd.Y; ey > sy; ey--)
			{
				bool foundblock = false;
				for (int x = selectStart.X; x <= selectEnd.X; x++)
				{
					for (int z = selectStart.Z; z <= selectEnd.Z; z++)
					{
						World.Block block = world.GetBlock(x, ey, z);
						if (block != null && block.legacyId > 0 && !isFiltered(block))
						{
							foundblock = true;
							break;
						}
					}
					if (foundblock) break;
				}
				if (foundblock) break;
			}
			#endregion
			#region Z+
			for (sz = selectStart.Z; sz < selectEnd.Z; sz++)
			{
				bool foundblock = false;
				for (int x = selectStart.X; x <= selectEnd.X; x++)
				{
					for (int y = selectStart.Y; y <= selectEnd.Y; y++)
					{
						World.Block block = world.GetBlock(x, y, sz);
						if (block != null && block.legacyId > 0 && !isFiltered(block))
						{
							foundblock = true;
							break;
						}
					}
					if (foundblock) break;
				}
				if (foundblock) break;
			}
			#endregion
			#region Z-
			for (ez = selectEnd.Z; ez > sz; ez--)
			{
				bool foundblock = false;
				for (int x = selectStart.X; x <= selectEnd.X; x++)
				{
					for (int y = selectStart.Y; y <= selectEnd.Y; y++)
					{
						World.Block block = world.GetBlock(x, y, ez);
						if (block != null && block.legacyId > 0 && !isFiltered(block))
						{
							foundblock = true;
							break;
						}
					}
					if (foundblock) break;
				}
				if (foundblock) break;
			}
			#endregion
			#endregion

			int len = (ey - sy) + 1, wid = (ex - sx) + 1, hei = (ez - sz) + 1;
			byte[] blockLegacyIdArray = new byte[len * wid * hei];
			byte[] blockLegacyDataArray = new byte[len * wid * hei];
			int pos = 0;

			for (int z = sz; z <= ez; z++)
			{
				for (int y = sy; y <= ey; y++)
				{
					for (int x = sx; x <= ex; x++)
					{
						World.Block block = world.GetBlock(x, y, z);
						if (block == null || isFiltered(block))
						{
							blockLegacyIdArray[pos] = 0;
							blockLegacyDataArray[pos] = 0;
						}
						else
						{
							blockLegacyIdArray[pos] = block.legacyId;
							blockLegacyDataArray[pos] = block.legacyData;
						}

						pos++;
					}
				}
			}

			NBTCompound schematic = new NBTCompound();
			schematic.Add(TagType.SHORT, "Length", len);
			schematic.Add(TagType.SHORT, "Width", wid);
			schematic.Add(TagType.SHORT, "Height", hei);
			schematic.Add(TagType.STRING, "Materials", "Alpha");
			schematic.Add(TagType.STRING, "FromMap", world.name);
			schematic.Add(TagType.BYTE_ARRAY, "Blocks", blockLegacyIdArray);
			schematic.Add(TagType.BYTE_ARRAY, "Data", blockLegacyDataArray);

			NBTWriter nbt = new NBTWriter();
			try
			{
				nbt.Save(filename, "Schematic", schematic);
			}
			catch (Exception e)
			{
				MessageBox.Show("Could not write to file. Check that it's not used by another process.");
			}
		}

		private bool isFiltered(World.Block block)
		{
			/*if (filterBlocksActive && blockMap.ContainsKey(idData.id) && filterBlocks.Contains(blockMap[idData.id].name[idData.data]))
				return !filterBlocksInvert;

			return filterBlocksInvert;*/
			return false;
		}

		/// <summary>Updates the label showing the selection size.</summary>
		private void UpdateSizeLabel()
		{
			int len = Math.Abs(selectEnd.X - selectStart.X) + 1;
			int wid = Math.Abs(selectEnd.Y - selectStart.Y) + 1;
			int hei = Math.Abs(selectEnd.Z - selectStart.Z) + 1;
			lblSelSize.Text = len + "x" + wid + "x" + hei + " (" + len * wid * hei + " blocks)";
		}

		/// <summary>Loads the blocks from the Minecraft version file.</summary>
		public void LoadBlocks(string filename)
		{
			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = JsonConvert.DeserializeObject<JsonObject>(json);
				JsonList blocksList = root["blocks"].ToObject<JsonList>();

				foreach (JsonObject curBlock in blocksList)
				{
					Block block = new Block(curBlock["name"]);

					// Get preview from name if available
					if (blockPreviewMap.ContainsKey(curBlock["name"]))
						block.preview = blockPreviewMap[curBlock["name"]];

					// Get preview from file if available
					if (curBlock.ContainsKey("file") && blockPreviewMap.ContainsKey(curBlock["file"]))
						block.preview = blockPreviewMap[curBlock["file"]];

					// Get states
					if (curBlock.ContainsKey("states"))
					{
						JsonObject states = curBlock["states"].ToObject<JsonObject>();
						foreach (JsonNameValuePair pair in states)
						{
							Block.State state = new Block.State(pair.Key);
							List<dynamic> valuesList = pair.Value.ToObject<List<dynamic>>();

							// Get values of this state
							foreach (dynamic val in valuesList)
							{
								Block.State.Value value;

								if (val.GetType() == typeof(string))
									value = new Block.State.Value(val);
								else
								{
									JsonObject curValue = val.ToObject<JsonObject>();
									value = new Block.State.Value(curValue["value"]);

									if (curValue.ContainsKey("file") && blockPreviewMap.ContainsKey(curValue["file"]))
										value.preview = blockPreviewMap[curValue["file"]];
								}

								state.values[value.name] = value;
							}

							block.states[pair.Key] = state;
						}
					}

					// Add to ID Map
					if (curBlock.ContainsKey("legacy_id"))
						blockLegacyIdMap[(byte)curBlock["legacy_id"]] = block;

					// Create data states
					if (block.preview != null)
						for (var i = 0; i < 16; i++)
							block.legacyDataPreview[i] = block.preview;
					if (curBlock.ContainsKey("legacy_data"))
						LoadBlockLegacyData(ref block, curBlock["legacy_data"].ToObject<JsonObject>(), 0, 1);

					blockNameMap[block.name] = block;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to load Minecraft assets");
				Application.Exit();
			}

			// Water/Lava
			blockNameMap["flowing_water"] = blockNameMap["water"];
			blockNameMap["flowing_lava"] = blockNameMap["lava"];
			blockLegacyIdMap[8] = blockLegacyIdMap[9];
			blockLegacyIdMap[10] = blockLegacyIdMap[11];
		}

		private void LoadBlockLegacyData(ref Block block, JsonObject obj, byte bitMask, byte bitBase)
		{
			foreach (JsonNameValuePair pair in obj)
			{
				switch (pair.Key)
				{
					// Bitmasks
					case "0x1":	LoadBlockLegacyData(ref block, pair.Value.ToObject<JsonObject>(), 1, 1); break;
					case "0x2": LoadBlockLegacyData(ref block, pair.Value.ToObject<JsonObject>(), 2, 2); break;
					case "0x4": LoadBlockLegacyData(ref block, pair.Value.ToObject<JsonObject>(), 4, 4); break;
					case "0x8": LoadBlockLegacyData(ref block, pair.Value.ToObject<JsonObject>(), 8, 8); break;
					case "0x1+0x2": LoadBlockLegacyData(ref block, pair.Value.ToObject<JsonObject>(), 3, 1); break;
					case "0x1+0x2+0x4": LoadBlockLegacyData(ref block, pair.Value.ToObject<JsonObject>(), 7, 1); break;
					case "0x4+0x8": LoadBlockLegacyData(ref block, pair.Value.ToObject<JsonObject>(), 12, 4); break;

					// Number (apply previous bitmask)
					default:
					{
						byte value = Convert.ToByte(pair.Key);
						string[] stateString = pair.Value.Split(new char[] { ',' });
						Block.Preview preview = null;

						// Find preview of state
						for (int s = 0; s < stateString.Length; s++)
						{
							string[] curStateString = stateString[s].Split(new char[] { '=' });
							string stateName = curStateString[0];
							string stateVal = curStateString[1];
							if (block.states.ContainsKey(stateName))
							{
								Block.State blockState = block.states[stateName];
								if (blockState.values.ContainsKey(stateVal))
									preview = blockState.values[stateVal].preview;
							}
						}

						if (preview != null)
						{
							// Insert into array
							if (bitMask > 0)
							{
								for (var d = 0; d < 16; d++)
									if ((d & bitMask) / bitBase == value) // Check data value with bitmask
										block.legacyDataPreview[d] = preview;
							}
							else
								block.legacyDataPreview[value] = preview;
						}

						break;
					}
				}
			}
		}

		/// <summary>Loads the top-down/cross-section colors of the blocks, as generated by Mine-imator.</summary>
		public void LoadBlockPreviews(string filename)
		{
			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = JsonConvert.DeserializeObject<JsonObject>(json);
				foreach (JsonNameValuePair key in root)
				{
					string file = key.Key;
					JsonObject obj = key.Value.ToObject<JsonObject>();
					Block.Preview preview = new Block.Preview(Color.Transparent, Color.Transparent);

					// Top-down color
					if (obj.ContainsKey("Y"))
					{
						if (obj.ContainsKey("Y_alpha"))
							preview.XYColor = Util.HexToColor(obj["Y"], (int)((float)obj["Y_alpha"] * 255.0f));
						else
							preview.XYColor = Util.HexToColor(obj["Y"]);
					}

					// Top-down color
					if (obj.ContainsKey("Z"))
					{
						if (obj.ContainsKey("Z_alpha"))
							preview.XZColor = Util.HexToColor(obj["Z"], (int)((float)obj["Z_alpha"] * 255.0f));
						else
							preview.XZColor = Util.HexToColor(obj["Z"]);
					}

					blockPreviewMap[file] = preview;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to load block previews");
				Application.Exit();
			}
		}

		/// <summary>Gets the XY bitmap of the given chunk. If none have been generated, create it.</summary>
		/// <param name="chunk">The wished chunk.</param>
		private Bitmap GetChunkXYImage(Chunk chunk)
		{
			if (chunk.hasXYImage)
				return chunk.XYImage.Image;
			
			chunk.XYImage = new FastBitmap(16, 16);
			chunk.XYImage.LockImage();

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					Color finalColor = Color.Transparent;
					for (int z = 255; z >= 0; z--)
					{
						Chunk.Section section = chunk.sections[z / 16];
						if (section == null)
							continue;

						byte id = section.blocks[x, y, z % 16].legacyId;
						if (id == 0)
							continue;

						Color blockColor = GetBlockXYColor(section.blocks[x, y, z % 16]);
						if (blockColor != Color.Transparent)
						{
							bool highlight = false, shade = false;
							World.Block blockCheck;
							Color blockCheckColor;

							// Shade
							if (z < 255)
							{
								blockCheck = world.GetBlock(chunk.X * 16 + x - 1, chunk.Y * 16 + y, z + 1);
								if (blockCheck != null)
								{
									blockCheckColor = GetBlockXYColor(blockCheck);
									if (blockCheckColor.A == 255)
										shade = true;
								}

								if (!shade)
								{
									blockCheck = world.GetBlock(chunk.X * 16 + x, chunk.Y * 16 + y - 1, z + 1);
									if (blockCheck != null)
									{
										blockCheckColor = GetBlockXYColor(blockCheck);
										if (blockCheckColor.A == 255)
											shade = true;
									}
								}
							}

							// Highlight
							blockCheck = world.GetBlock(chunk.X * 16 + x - 1, chunk.Y * 16 + y, z);
							if (blockCheck == null || blockCheck.legacyId == 0)
								highlight = true;

							if (!highlight)
							{
								blockCheck = world.GetBlock(chunk.X * 16 + x, chunk.Y * 16 + y - 1, z - 1);
								if (blockCheck == null || blockCheck.legacyId == 0)
									highlight = true;
							}

							// Apply
							if (highlight)
								blockColor = Util.ColorBrighter(blockColor, 15);
							else if (shade)
								blockColor = Util.ColorBrighter(blockColor, -15);

							// Add to final result, cancel if alpha is full
							finalColor = Util.ColorAdd(blockColor, finalColor);
							if (finalColor.A == 255)
								break;
						}
					}

					chunk.XYImage.SetPixel(x, y, finalColor);
				}
			}

			chunk.XYImage.UnlockImage();
			chunk.hasXYImage = true;

			return chunk.XYImage.Image;
		}

		/// <summary>Gets the XZ bitmap of the given chunk. If none have been generated, create it.</summary>
		/// <param name="chunk">The wished chunk.</param>
		private Bitmap GetChunkXZImage(Chunk chunk)
		{
			if (chunk.hasXZImage)
				return chunk.XZImage.Image;

			chunk.XZImage = new FastBitmap(16, 256);
			chunk.XZImage.LockImage();

			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 256; z++)
				{
					Chunk.Section section = chunk.sections[z / 16];
					if (section == null)
						continue;

					Color finalColor = Color.Transparent;
					for (int y = 15; y >= 0; y--)
					{
						Color blockColor = GetBlockXZColor(section.blocks[x, y, z % 16]);
						if (blockColor != Color.Transparent)
						{
							// Add to final result, cancel if alpha is full
							finalColor = Util.ColorAdd(blockColor, finalColor);
							if (finalColor.A == 255)
							{
								finalColor = Util.ColorBrighter(finalColor, (int)(-60.0f * (1.0f - (float)y / 15.0f)));
								break;
							}
						}
					}

					if (finalColor != Color.Transparent)
						chunk.XZImage.SetPixel(x, chunk.XZImage.Image.Height - 1 - z, finalColor); 
				}
			}

			chunk.XZImage.UnlockImage();
			chunk.hasXZImage = true;

			return chunk.XZImage.Image;
		}

		/// <summary>Gets the top-down drawing color of the block.</summary>
		/// <param name="block">The block to get the color from.</param>
		private Color GetBlockXYColor(World.Block worldBlock)
		{
			if (blockLegacyIdMap.ContainsKey(worldBlock.legacyId))
			{
				Block.Preview preview = blockLegacyIdMap[worldBlock.legacyId].legacyDataPreview[worldBlock.legacyData];
				if (preview != null)
					return preview.XYColor;
			}

			return Color.Transparent;
		}

		/// <summary>Gets the cross-section drawing color of the block.</summary>
		/// <param name="block">The block to get the color from.</param>
		private Color GetBlockXZColor(World.Block worldBlock)
		{
			if (blockLegacyIdMap.ContainsKey(worldBlock.legacyId))
			{
				Block.Preview preview = blockLegacyIdMap[worldBlock.legacyId].legacyDataPreview[worldBlock.legacyData];
				if (preview != null)
					return preview.XZColor;
			}

			return Color.Transparent;
		}

		/// <summary>Loads which blocks to filter out.</summary>
		public void LoadFilterBlocks()
		{
			if (File.Exists("blocks.mifilter"))
			{
				string json = File.ReadAllText("blocks.mifilter");
				try
				{
					Dictionary<string, dynamic> root = (Dictionary<string, dynamic>)JsonConvert.DeserializeObject(json);
					filterBlocksActive = root["active"];
					filterBlocksInvert = root["invert"];
					filterBlocks = root["blocks"];
				}
				catch (Exception e)
				{
					// Silently ignore filtered blocks
				}
			}

			UpdateFilterBlocks();
		}

		/// <summary>Updates the label of filterblocks.</summary>
		public void UpdateFilterBlocks()
		{
			lblFilterInfo.Visible = filterBlocksActive;
		}

		/// <summary>Updates the bitmap of the XY map.</summary>
		/// <param name="x">View movement along the x axis.</param>
		/// <param name="y">View movement along the y axis.</param>
		private void UpdateXYMap(int x, int y)
		{
			if (world.filename == "")
				return;

			bool move = (x != 0 || y != 0);
			int screenwid = pboxWorldXY.Width, screenhei = pboxWorldXY.Height;
			XYBlocksWidth = (int)(screenwid / XYImageZoom) + 1;
			XYBlocksHeight = (int)(screenhei / XYImageZoom) + 1;
			XYStart = new Point((int)(XYImageMidPos.X - (screenwid / XYImageZoom) / 2), (int)(XYImageMidPos.Y - (screenhei / XYImageZoom) / 2));
			Bitmap bmp = new Bitmap(XYBlocksWidth, XYBlocksHeight);

			// Find chunks and draw them
			using (Graphics g = Graphics.FromImage(bmp))
			{
				if (!move)
				{
					for (int dx = 0; dx < XYBlocksWidth + 16; dx += 16)
					{
						for (int dy = 0; dy < XYBlocksHeight + 16; dy += 16)
						{
							Chunk chunk = world.GetChunk(XYStart.X + dx, XYStart.Y + dy);
							if (chunk != null)
								g.DrawImage(GetChunkXYImage(chunk), chunk.X * 16 - XYStart.X, chunk.Y * 16 - XYStart.Y);
						}
					}
				}
				else
				{
					g.DrawImage(XYMapBitmap, x, y);
					int sx, ex, sy, ey;
					if (x < 0)
					{
						sx = XYBlocksWidth + x;
						ex = XYBlocksWidth;
					}
					else
					{
						sx = 0;
						ex = x + 16;
					}

					if (y < 0)
					{
						sy = XYBlocksHeight + y;
						ey = XYBlocksHeight;
					}
					else
					{
						sy = 0;
						ey = y + 16;
					}

					if (x != 0)
					{
						for (int dx = sx; dx < ex; dx += 16)
						{
							for (int dy = 0; dy < XYBlocksHeight + 16; dy += 16)
							{
								Chunk chunk = world.GetChunk(XYStart.X + dx, XYStart.Y + dy);
								if (chunk != null)
									g.DrawImage(GetChunkXYImage(chunk), chunk.X * 16 - XYStart.X, chunk.Y * 16 - XYStart.Y);
							}
						}
					}
					if (y != 0)
					{
						for (int dx = 0; dx < XYBlocksWidth + 16; dx += 16)
						{
							for (int dy = sy; dy < ey; dy += 16)
							{
								Chunk chunk = world.GetChunk(XYStart.X + dx, XYStart.Y + dy);
								if (chunk != null)
									g.DrawImage(GetChunkXYImage(chunk), chunk.X * 16 - XYStart.X, chunk.Y * 16 - XYStart.Y);
							}
						}
					}
				}
			}

			XYMapBitmap.Dispose();
			XYMapBitmap = bmp;
			UpdateXYSel();
		}

		/// <summary>Updates the bitmap for the XY selection box.</summary>
		private void UpdateXYSel()
		{
			int sx = Math.Min(selectStart.X, selectEnd.X), sy = Math.Min(selectStart.Y, selectEnd.Y);
			int ex = Math.Max(selectStart.X, selectEnd.X), ey = Math.Max(selectStart.Y, selectEnd.Y);
			XYSelectStartDraw = new Point((int)((sx - XYStart.X) * XYImageZoom - XYImageZoom / 2), (int)(((sy - XYStart.Y)) * XYImageZoom - XYImageZoom / 2));
			XYSelectEndDraw = new Point((int)((ex - XYStart.X) * XYImageZoom + XYImageZoom / 2), (int)((ey - XYStart.Y) * XYImageZoom + XYImageZoom / 2));

			XYSelectBitmap.Dispose();
			XYSelectBitmap = new Bitmap(pboxWorldXY.Width, pboxWorldXY.Height);
			using (Graphics g = Graphics.FromImage(XYSelectBitmap))
			{
				using (Pen p = new Pen(Color.White, 2))
					g.DrawRectangle(p, XYSelectStartDraw.X, XYSelectStartDraw.Y, XYSelectEndDraw.X - XYSelectStartDraw.X, XYSelectEndDraw.Y - XYSelectStartDraw.Y);
			}
			UpdateXYPicBox();
		}

		/// <summary>Draws the map, selection box, player and spawn location to the XY picture box.</summary>
		private void UpdateXYPicBox()
		{
			if (pboxWorldXY.Image != null)
				pboxWorldXY.Image.Dispose();

			if (XYImageZoom == 1)
				pboxWorldXY.Image = new Bitmap(XYMapBitmap);
			else
				pboxWorldXY.Image = ResizeBitmap(XYMapBitmap, (int)(XYBlocksWidth * XYImageZoom), (int)(XYBlocksHeight * XYImageZoom));

			using (Graphics g = Graphics.FromImage(pboxWorldXY.Image))
			{
				g.DrawImage(spawnImage, (int)((world.spawnPos.X - XYStart.X) * XYImageZoom) - 8, (int)((world.spawnPos.Y - XYStart.Y) * XYImageZoom) - 8);
				g.DrawImage(playerImage, (int)(((int)world.playerPos.X - XYStart.X) * XYImageZoom) - 8, (int)(((int)world.playerPos.Y - XYStart.Y) * XYImageZoom) - 8);
				g.DrawImage(XYSelectBitmap, 0, 0);
			}
		}

		private void OverXY(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;
			pboxWorldXY.Focus();

			if (e.Button == MouseButtons.None)
			{
				XYDragSelect = 0;
				if (mouse_rectangle(e.Location, XYSelectStartDraw.X - 4, XYSelectStartDraw.Y, XYSelectStartDraw.X + 4, XYSelectEndDraw.Y))
					XYDragSelect = 8; // L

				if (mouse_rectangle(e.Location, XYSelectEndDraw.X - 4, XYSelectStartDraw.Y, XYSelectEndDraw.X + 4, XYSelectEndDraw.Y))
					XYDragSelect = 4; // R

				if (mouse_rectangle(e.Location, XYSelectStartDraw.X, XYSelectStartDraw.Y - 4, XYSelectEndDraw.X, XYSelectStartDraw.Y + 4))
					XYDragSelect = 2; // U

				if (mouse_rectangle(e.Location, XYSelectStartDraw.X, XYSelectEndDraw.Y - 4, XYSelectEndDraw.X, XYSelectEndDraw.Y + 4))
					XYDragSelect = 6; // D

				if (mouse_rectangle(e.Location, XYSelectStartDraw.X - 4, XYSelectStartDraw.Y - 4, XYSelectStartDraw.X + 4, XYSelectStartDraw.Y + 4))
					XYDragSelect = 1; // LU

				if (mouse_rectangle(e.Location, XYSelectEndDraw.X - 4, XYSelectStartDraw.Y - 4, XYSelectEndDraw.X + 4, XYSelectStartDraw.Y + 4))
					XYDragSelect = 3; // RU

				if (mouse_rectangle(e.Location, XYSelectStartDraw.X - 4, XYSelectEndDraw.Y - 4, XYSelectStartDraw.X + 4, XYSelectEndDraw.Y + 4))
					XYDragSelect = 7; // LD

				if (mouse_rectangle(e.Location, XYSelectEndDraw.X - 4, XYSelectEndDraw.Y - 4, XYSelectEndDraw.X + 4, XYSelectEndDraw.Y + 4))
					XYDragSelect = 5; // RD
			}
			else
			{
				int dx = (int)((e.Location.X - moveStartMPos.X) / XYImageZoom);
				int dy = (int)((e.Location.Y - moveStartMPos.Y) / XYImageZoom);

				if (XYDragView == 1) // Move XY view
				{
					Point prevpos = new Point(XYImageMidPos.X, XYImageMidPos.Y);
					XYImageMidPos = new Point(moveStartPos.X - dx, moveStartPos.Y - dy);
					Point newpos = new Point(XYImageMidPos.X, XYImageMidPos.Y);
					if (prevpos != newpos) UpdateXYMap(prevpos.X - newpos.X, prevpos.Y - newpos.Y);
				}
				else // Change rectangle
				{
					Point startprevpos = new Point(selectStart.X, selectStart.Y);
					Point endprevpos = new Point(selectEnd.X, selectEnd.Y);
					if (XYDragSelect == 1) // LU
					{
						selectStart.X = moveStartPos.X + dx;
						selectStart.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 2) // U
						selectStart.Y = moveStartPos.Y + dy;

					if (XYDragSelect == 3) // RU
					{
						selectEnd.X = moveStartPos.X + dx;
						selectStart.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 4) // R
						selectEnd.X = moveStartPos.X + dx;

					if (XYDragSelect == 5) // RD
					{
						selectEnd.X = moveStartPos.X + dx;
						selectEnd.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 6) // D
						selectEnd.Y = moveStartPos.Y + dy;

					if (XYDragSelect == 7) // LD
					{
						selectStart.X = moveStartPos.X + dx;
						selectEnd.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 8) // L
						selectStart.X = moveStartPos.X + dx;

					UpdateSizeLabel();
					Point startnewpos = new Point(selectStart.X, selectStart.Y);
					Point endnewpos = new Point(selectEnd.X, selectEnd.Y);
					if (startprevpos != startnewpos || endprevpos != endnewpos)
						UpdateXYSel();
				}
			} //#YOLO

			// Set cursor image
			if (XYDragSelect == 4 || XYDragSelect == 8)
				Cursor.Current = Cursors.SizeWE;

			if (XYDragSelect == 2 || XYDragSelect == 6)
				Cursor.Current = Cursors.SizeNS;

			if (XYDragSelect == 1 || XYDragSelect == 5)
				Cursor.Current = Cursors.SizeNWSE;

			if (XYDragSelect == 3 || XYDragSelect == 7)
				Cursor.Current = Cursors.SizeNESW;
		}

		private void MoveXYStart(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			if (XYDragSelect == 0)
			{
				if (e.Button == MouseButtons.Left) //Create rectangle at position
				{
					XYDragSelect = 5;
					selectStart = new Point3D<int>((int)(e.Location.X / XYImageZoom + XYStart.X), (int)(e.Location.Y / XYImageZoom + XYStart.Y + 1), selectStart.Z);
					selectEnd = new Point3D<int>((int)(e.Location.X / XYImageZoom + XYStart.X), (int)(e.Location.Y / XYImageZoom + XYStart.Y + 1), selectEnd.Z);
					UpdateSizeLabel();
				}
				else //Move view
				{
					XYDragView = 1;
					moveStartPos = XYImageMidPos;
				}
			}

			if (XYDragSelect == 1 || XYDragSelect == 2)
				moveStartPos = new Point(selectStart.X, selectStart.Y);
			else if (XYDragSelect == 3 || XYDragSelect == 4)
				moveStartPos = new Point(selectEnd.X, selectStart.Y);
			else if (XYDragSelect == 5 || XYDragSelect == 6)
				moveStartPos = new Point(selectEnd.X, selectEnd.Y);
			else if (XYDragSelect == 7 || XYDragSelect == 8)
				moveStartPos = new Point(selectStart.X, selectEnd.Y);
			moveStartMPos = e.Location;
		}

		private void MoveXYEnd(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			XYDragView = 0;
			if (selectStart.X > selectEnd.X)
			{
				int tmp = selectStart.X;
				selectStart.X = selectEnd.X;
				selectEnd.X = tmp;
			}

			if (selectStart.Y > selectEnd.Y)
			{
				int tmp = selectStart.Y;
				selectStart.Y = selectEnd.Y;
				selectEnd.Y = tmp;
			}

			XYDragSelect = 0;
			XZImageMidPos = new Point(selectStart.X + (selectEnd.X - selectStart.X) / 2, selectStart.Z + (selectEnd.Z - selectStart.Z) / 2);
			UpdateXYSel();
			UpdateXZMap();
			UpdateSizeLabel();
		}

		private void ZoomXY(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			if (XYDragSelect > 0)
				return;

			if (e.Delta > 0 && XYImageZoom < 32)
				XYImageZoom *= 2;

			if (e.Delta < 0 && XYImageZoom > 0.25)
				XYImageZoom /= 2;
			UpdateXYMap(0, 0);
		}

		private void ResizeXY(object sender, EventArgs e)
		{
			UpdateXYMap(0, 0);
		}

		/// <summary>Updates the bitmap of the XZ map.</summary>
		private void UpdateXZMap()
		{
			if (world.filename == "")
				return;

			int screenwid = pboxWorldXZ.Width, screenhei = pboxWorldXZ.Height;
			XZBlocksWidth = (int)(screenwid / XZImageZoom) + 1;
			XZBlocksHeight = (int)(screenhei / XZImageZoom) + 1;
			XZStart = new Point((int)Math.Floor(XZImageMidPos.X - (screenwid / XZImageZoom) / 2), 0);
			Bitmap bmp = new Bitmap(XZBlocksWidth, 256);

			//Find chunks and draw them
			using (Graphics g = Graphics.FromImage(bmp))
			{
				for (int dy = selectEnd.Y - 48; dy <= selectEnd.Y; dy += 16)
				{
					for (int dx = 0; dx < XZBlocksWidth + 16; dx += 16)
					{
						int cx = XZStart.X + dx;
						Chunk chunk = world.GetChunk(cx, dy);
						if (chunk != null)
						{
							Bitmap img = GetChunkXZImage(chunk);
							g.DrawImage(img, chunk.X * 16 - XZStart.X, 256 - img.Height);
						}
					}
				}
			}

			XZMapBitmap.Dispose();
			XZMapBitmap = bmp;
			UpdateXZSel();
		}

		/// <summary>Updates the bitmap for the XZ selection box.</summary>
		private void UpdateXZSel()
		{
			int sx = Math.Min(selectStart.X, selectEnd.X), sz = Math.Min(selectStart.Z, selectEnd.Z);
			int ex = Math.Max(selectStart.X, selectEnd.X), ez = Math.Max(selectStart.Z, selectEnd.Z);
			XZSelectStartDraw = new Point((int)((sx - XZStart.X) * XZImageZoom - XZImageZoom / 2), pboxWorldXZ.Size.Height - Math.Max(1, (int)(ez * XZImageZoom + XZImageZoom / 2)));
			XZSelectEndDraw = new Point((int)((ex - XZStart.X) * XZImageZoom + XZImageZoom / 2), pboxWorldXZ.Size.Height - Math.Max(1, (int)(sz * XZImageZoom - XZImageZoom / 2)));
			XZSelectBitmap.Dispose();
			XZSelectBitmap = new Bitmap(pboxWorldXZ.Width, pboxWorldXZ.Height);
			using (Graphics g = Graphics.FromImage(XZSelectBitmap))
			using (Pen p = new Pen(Color.White, 2))
				g.DrawRectangle(p, XZSelectStartDraw.X, XZSelectStartDraw.Y, XZSelectEndDraw.X - XZSelectStartDraw.X, XZSelectEndDraw.Y - XZSelectStartDraw.Y);
			UpdateXZPicBox();
		}

		/// <summary>Draws the map, selection box, player and spawn location to the XZ picture box.</summary>
		private void UpdateXZPicBox()
		{
			if (pboxWorldXZ.Image != null)
				pboxWorldXZ.Image.Dispose();

			pboxWorldXZ.Image = new Bitmap(pboxWorldXZ.Width, pboxWorldXZ.Height);

			using (Graphics g = Graphics.FromImage(pboxWorldXZ.Image))
			{
				Bitmap map;
				if (XZImageZoom == 1)
					map = XZMapBitmap;
				else
					map = ResizeBitmap(XZMapBitmap, (int)(XZMapBitmap.Width * XZImageZoom), (int)(256 * XZImageZoom));
				g.DrawImage(map, 0, pboxWorldXZ.Height + XZImageZoom - map.Height);
				g.DrawImage(XZSelectBitmap, 0, 0);
				g.DrawImage(spawnImage, (int)((world.spawnPos.X - XZStart.X) * XZImageZoom) - 8, (int)(pboxWorldXZ.Height - world.spawnPos.Z * XZImageZoom) - 8);
				g.DrawImage(playerImage, (int)(((int)world.playerPos.X - XZStart.X) * XZImageZoom) - 8, (int)(pboxWorldXZ.Height - world.playerPos.Z * XZImageZoom) - 8);
			}
		}

		private void OverXZ(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			pboxWorldXZ.Focus();
			if (e.Button == MouseButtons.None)
			{
				XZDragSelect = 0;
				if (mouse_rectangle(e.Location, XZSelectStartDraw.X - 4, XZSelectStartDraw.Y, XZSelectStartDraw.X + 4, XZSelectEndDraw.Y))
					XZDragSelect = 8; // L

				if (mouse_rectangle(e.Location, XZSelectEndDraw.X - 4, XZSelectStartDraw.Y, XZSelectEndDraw.X + 4, XZSelectEndDraw.Y))
					XZDragSelect = 4; // R

				if (mouse_rectangle(e.Location, XZSelectStartDraw.X, XZSelectStartDraw.Y - 4, XZSelectEndDraw.X, XZSelectStartDraw.Y + 4))
					XZDragSelect = 2; // U

				if (mouse_rectangle(e.Location, XZSelectStartDraw.X, XZSelectEndDraw.Y - 4, XZSelectEndDraw.X, XZSelectEndDraw.Y + 4))
					XZDragSelect = 6; // D

				if (mouse_rectangle(e.Location, XZSelectStartDraw.X - 4, XZSelectStartDraw.Y - 4, XZSelectStartDraw.X + 4, XZSelectStartDraw.Y + 4))
					XZDragSelect = 1; // LU

				if (mouse_rectangle(e.Location, XZSelectEndDraw.X - 4, XZSelectStartDraw.Y - 4, XZSelectEndDraw.X + 4, XZSelectStartDraw.Y + 4))
					XZDragSelect = 3; // RU

				if (mouse_rectangle(e.Location, XZSelectStartDraw.X - 4, XZSelectEndDraw.Y - 4, XZSelectStartDraw.X + 4, XZSelectEndDraw.Y + 4))
					XZDragSelect = 7; // LD

				if (mouse_rectangle(e.Location, XZSelectEndDraw.X - 4, XZSelectEndDraw.Y - 4, XZSelectEndDraw.X + 4, XZSelectEndDraw.Y + 4))
					XZDragSelect = 5; // RD
			}
			else
			{
				int dx = (int)((e.Location.X - moveStartMPos.X) / XZImageZoom);
				int dy = -(int)((e.Location.Y - moveStartMPos.Y) / XZImageZoom);

				if (XZDragView == 1) // Move XZ view
				{
					Point prevpos = new Point(XZImageMidPos.X, XZImageMidPos.Y);
					XZImageMidPos = new Point(moveStartPos.X - dx, moveStartPos.Y - dy);
					Point newpos = new Point(XZImageMidPos.X, XZImageMidPos.Y);
					if (prevpos != newpos)
						UpdateXZMap();
				}
				else // Change rectangle
				{
					Point startprevpos = new Point(selectStart.X, selectStart.Z);
					Point endprevpos = new Point(selectEnd.X, selectEnd.Z);
					if (XZDragSelect == 1) // LU
					{
						selectStart.X = moveStartPos.X + dx;
						selectEnd.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 2) // U
						selectEnd.Z = moveStartPos.Y + dy;

					if (XZDragSelect == 3) //RU
					{
						selectEnd.X = moveStartPos.X + dx;
						selectEnd.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 4) // R
						selectEnd.X = moveStartPos.X + dx;

					if (XZDragSelect == 5) // RD
					{
						selectEnd.X = moveStartPos.X + dx;
						selectStart.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 6) // D
						selectStart.Z = moveStartPos.Y + dy;

					if (XZDragSelect == 7) // LD
					{
						selectStart.X = moveStartPos.X + dx;
						selectStart.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 8) // L
						selectStart.X = moveStartPos.X + dx;

					selectStart.Z = Math.Max(Math.Min(selectStart.Z, 255), 0);
					selectEnd.Z = Math.Max(Math.Min(selectEnd.Z, 255), 0);
					UpdateSizeLabel();
					Point startnewpos = new Point(selectStart.X, selectStart.Z);
					Point endnewpos = new Point(selectEnd.X, selectEnd.Z);
					if (startprevpos != startnewpos || endprevpos != endnewpos)
						UpdateXZSel();
				}
			}

			// Set cursor image
			if (XZDragSelect == 4 || XZDragSelect == 8)
				Cursor.Current = Cursors.SizeWE;

			if (XZDragSelect == 2 || XZDragSelect == 6)
				Cursor.Current = Cursors.SizeNS;

			if (XZDragSelect == 1 || XZDragSelect == 5)
				Cursor.Current = Cursors.SizeNWSE;

			if (XZDragSelect == 3 || XZDragSelect == 7)
				Cursor.Current = Cursors.SizeNESW;
		}

		private void MoveXZStart(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			if (XZDragSelect == 0)
			{
				if (e.Button == MouseButtons.Left) // Create rectangle at position
				{
					XZDragSelect = 5;
					selectStart = new Point3D<int>((int)(e.Location.X / XZImageZoom + XZStart.X), selectStart.Y, (int)((pboxWorldXZ.Size.Height - e.Location.Y) / XZImageZoom + 1));
					selectEnd = new Point3D<int>((int)(e.Location.X / XZImageZoom + XZStart.X), selectEnd.Y, (int)((pboxWorldXZ.Size.Height - e.Location.Y) / XZImageZoom + 1));
					selectStart.Z = Math.Max(Math.Min(selectStart.Z, 255), 0);
					selectEnd.Z = Math.Max(Math.Min(selectEnd.Z, 255), 0);
					UpdateSizeLabel();
				}
				else //Move view
				{
					XZDragView = 1;
					moveStartPos = XZImageMidPos;
				}
			}

			if (XZDragSelect == 1 || XZDragSelect == 2)
				moveStartPos = new Point(selectStart.X, selectEnd.Z);

			else if (XZDragSelect == 3 || XZDragSelect == 4)
				moveStartPos = new Point(selectEnd.X, selectEnd.Z);

			else if (XZDragSelect == 5 || XZDragSelect == 6)
				moveStartPos = new Point(selectEnd.X, selectStart.Z);

			else if (XZDragSelect == 7 || XZDragSelect == 8)
				moveStartPos = new Point(selectStart.X, selectStart.Z);

			moveStartMPos = e.Location;
		}

		private void MoveXZEnd(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			XZDragView = 0;
			if (selectStart.X > selectEnd.X)
			{
				int tmp = selectStart.X;
				selectStart.X = selectEnd.X;
				selectEnd.X = tmp;
			}

			if (selectStart.Z > selectEnd.Z)
			{
				int tmp = selectStart.Z;
				selectStart.Z = selectEnd.Z;
				selectEnd.Z = tmp;
			}

			XZDragSelect = 0;
			UpdateXYSel();
			UpdateXZSel();
			UpdateSizeLabel();
		}

		private void ZoomXZ(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			if (XZDragSelect > 0)
				return;

			if (e.Delta > 0 && XZImageZoom < 32)
				XZImageZoom *= 2;

			if (e.Delta < 0 && XZImageZoom > 0.25)
				XZImageZoom /= 2;

			UpdateXZMap();
		}

		private void ResizeXZ(object sender, EventArgs e)
		{
			UpdateXZMap();
		}

		private void frmImport_Load(object sender, EventArgs e)
		{
			// Get worlds
			if (!Directory.Exists(mcSaveFolder))
				return;
			DirectoryInfo dir = new DirectoryInfo(mcSaveFolder);
			foreach (DirectoryInfo d in dir.GetDirectories())
			{
				if (File.Exists(d.FullName + @"\level.dat")) cbxSaves.Items.Add(d.Name);
			}
		}

		private void btnDone_Click(object sender, EventArgs e)
		{
			if (savefile != "")
			{
				SaveBlocks(savefile);
				Application.Exit();
			}
			else
			{
				SaveFileDialog sfd = new SaveFileDialog();
				sfd.Filter = "Minecraft Schematics (*.schematic)|*.schematic";
				sfd.Title = "Save Schematic";
				sfd.ShowDialog();
				if (sfd.FileName != "")
					SaveBlocks(sfd.FileName);
			}
		}

		private void rbtOver_CheckedChanged(object sender, EventArgs e)
		{
			if (rbtOver.Checked)
				LoadWorld();
		}

		private void rbtNether_CheckedChanged(object sender, EventArgs e)
		{
			if (rbtNether.Checked)
				LoadWorld();
		}

		private void rbtEnd_CheckedChanged(object sender, EventArgs e)
		{
			if (rbtEnd.Checked)
				LoadWorld();
		}

		private void cbxSaves_SelectedIndexChanged(object sender, EventArgs e)
		{
			LoadWorld();
		}

		private void btnAdvanced_Click(object sender, EventArgs e)
		{
			frmFilters frm = new frmFilters(this);
			frm.Location = new Point(this.Location.X + this.Size.Width / 2 - frm.Size.Width / 2, this.Location.Y + this.Size.Height / 2 - frm.Size.Height / 2);
			frm.ShowDialog();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		private Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
		{
			Bitmap result = new Bitmap(width, height);
			using (Graphics g = Graphics.FromImage(result))
			{
				g.InterpolationMode = InterpolationMode.NearestNeighbor;
				g.DrawImage(sourceBMP, 0, 0, width, height);
			}
			return result;
		}

		private bool mouse_rectangle(Point m, int x1, int y1, int x2, int y2)
		{
			return (m.X >= x1 && m.Y >= y1 && m.X < x2 && m.Y < y2);
		}
	}
}