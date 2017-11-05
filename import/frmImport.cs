using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace import
{
	using JsonObject = Dictionary<string, dynamic>;
	using JsonNameValuePair = KeyValuePair<string, dynamic>;
	using JsonList = List<dynamic>;

	public partial class frmImport : Form
	{
		/// <summary>A type of Minecraft block.</summary>
		public class Block
		{
			/// <summary>Determines how a block is previewed in the top-down or cross-section view.</summary>
			public class Preview
			{
				public Color XYColor, XZColor;

				public Preview(Color XYColor, Color XZColor)
				{
					this.XYColor = XYColor;
					this.XZColor = XZColor;
				}
			}

			/// <summary>A specific block state.</summary>
			public class State
			{
				/// <summary>A possible value of the block state.</summary>
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

			public string name, displayName;
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

		/// <summary>A choise in the world combobox.</summary>
		public class WorldOption
		{
			public string filename, name;
			public WorldOption(string filename, string name)
			{
				this.filename = filename;
				this.name = name;
			}

			public override string ToString()
			{
				return name;
			}
		}

		// Folders
		public static string mcSaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\saves";
		public static string currentFolder = Application.StartupPath; //@"D:\OneDrive\Projects\Minecraft\Mine-imator\Source\datafiles\Data"; //
		public static string mcAssetsFile = currentFolder + @"\Minecraft\1.12.2.midata";
		public static string miLangFile = currentFolder + @"\Languages\english.milanguage";
		public static string miBlockPreviewFile = currentFolder + @"\blockpreview.midata";
		public static string miBlockFilterFile = currentFolder + @"\blockfilter.midata";

		// Language
		public Dictionary<string, string> languageMap = new Dictionary<string, string>();

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

		JavaScriptSerializer serializer = new JavaScriptSerializer();

		public frmImport(string[] args)
		{
			InitializeComponent();

			// Parse arguments
			savefile = "";
			for (int i = 0; i < args.Length; i++)
			{
				if (i > 0)
					savefile += " ";
				savefile += args[i];
			}
		}

		private void frmImport_Load(object sender, EventArgs e)
		{
			if (!File.Exists(miLangFile))
			{
				MessageBox.Show("Could not find translation file, re-install the program.");
				Application.Exit();
				return;
			}

			if (!File.Exists(mcAssetsFile))
			{
				MessageBox.Show("Could not find Minecraft assets, re-install the program.");
				Application.Exit();
				return;
			}

			if (!File.Exists(miBlockPreviewFile))
			{
				MessageBox.Show("Could not find block previews, run Mine-imator first!");
				Application.Exit();
				return;
			}

			LoadLanguage(miLangFile);
			LoadBlockPreviews(miBlockPreviewFile);
			LoadBlocks(mcAssetsFile);

			if (File.Exists(miBlockFilterFile))
				LoadFilterBlocks(miBlockFilterFile);

			// Set text
			Text = GetText("title");
			lblInfo.Text = GetText("info");
			lblTopDownView.Text = GetText("topdownview");
			lblCrossSectionView.Text = GetText("crosssectionview");
			lblWorld.Text = GetText("world") + ":";
			btnBrowse.Text = GetText("browse");
			rbtOver.Text = GetText("overworld");
			rbtNether.Text = GetText("nether");
			rbtEnd.Text = GetText("end");
			btnDone.Text = GetText("done");
			btnFilters.Text = GetText("filters");
			btnCancel.Text = GetText("cancel");
			lblFilterInfo.Text = GetText("filtersalert");

			rbtNether.Location = new Point(rbtOver.Location.X + rbtOver.Width + 5, rbtOver.Location.Y);
			rbtEnd.Location = new Point(rbtNether.Location.X + rbtNether.Width + 5, rbtOver.Location.Y);
			UpdateSizeLabel();
			UpdateFilterBlocks();

			// Get worlds
			if (!Directory.Exists(mcSaveFolder))
				return;
			DirectoryInfo dir = new DirectoryInfo(mcSaveFolder);
			foreach (DirectoryInfo d in dir.GetDirectories())
				if (File.Exists(d.FullName + @"\level.dat"))
					cbxSaves.Items.Add(new WorldOption(d.FullName + @"\level.dat", d.Name));
		}

		/// <summary>Loads the .milanguage file containing the text of the program.</summary>
		private void LoadLanguage(string filename)
		{
			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				LoadLanguageObject("", root);
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to load the language file.");
				Application.Exit();
			}
		}
		private void LoadLanguageObject(string prefix, JsonObject root)
		{
			foreach (JsonNameValuePair key in root)
			{
				if (key.Key.Contains("/"))
					LoadLanguageObject(prefix + key.Key.Replace("/", ""), (JsonObject)key.Value);
				else
					languageMap[prefix + key.Key] = key.Value;
			}
		}

		/// <summary>Gets a specific text from the loaded language file.</summary>
		public string GetText(string name, string prefix = "importfromworld")
		{
			if (languageMap.ContainsKey(prefix + name))
				return languageMap[prefix + name];
			else
				return "<Text not found for \"" + prefix + name + "\">";
		}

		/// <summary>Saves the selection into a .schematic file.</summary>
		private void SaveBlocks(string filename)
		{
			world.SaveReset();

			#region Trim unnecessary space
			int sx, sy, sz, ex, ey, ez;
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
						if (world.IsLegacyBlockNotAir(this, sx, y, z))
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
						if (world.IsLegacyBlockNotAir(this, ex, y, z))
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
						if (world.IsLegacyBlockNotAir(this, x, sy, z))
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
						if (world.IsLegacyBlockNotAir(this, x, ey, z))
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
						if (world.IsLegacyBlockNotAir(this, x, y, sz))
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
						if (world.IsLegacyBlockNotAir(this, x, y, ez))
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
			NBTList tileEntities = new NBTList(TagType.COMPOUND);

			int pos = 0;
			for (int z = sz; z <= ez; z++)
			{
				for (int y = sy; y <= ey; y++)
				{
					for (int x = sx; x <= ex; x++)
					{
						Chunk chunk = world.GetChunk(x, y);

						// Add tile entities of newly iterated chunk
						if (!chunk.tileEntitiesAdded)
						{
							foreach (NBTTag tag in chunk.tileEntities.value)
							{
								NBTCompound comp = (NBTCompound)tag;
								int teX = comp.Get("x").value;
								int teY = comp.Get("z").value;
								int teZ = comp.Get("y").value;
								if (teX < sx || teX > ex || teY < sy || teY > ey || teZ < sz || teZ > ez)
									continue;

								// Subtract by start position in a copy
								NBTCompound newComp = (NBTCompound)comp.Copy();
								newComp.Add(TagType.INT, "x", teX - sx);
								newComp.Add(TagType.INT, "z", teY - sy);
								newComp.Add(TagType.INT, "y", teZ - sz);
								tileEntities.Add(newComp);
							}

							chunk.tileEntitiesAdded = true;
						}

						// Add block
						World.LegacyBlock block = world.GetLegacyBlock(chunk, x, y, z);
						if (block == null || block.id == 0 || IsLegacyBlockFiltered(block))
						{
							blockLegacyIdArray[pos] = 0;
							blockLegacyDataArray[pos] = 0;
						}
						else
						{
							blockLegacyIdArray[pos] = block.id;
							blockLegacyDataArray[pos] = block.data;
						}
						pos++;
					}
				}
			}

			// Create schematic
			NBTCompound schematic = new NBTCompound();
			schematic.Add(TagType.SHORT, "Length", len);
			schematic.Add(TagType.SHORT, "Width", wid);
			schematic.Add(TagType.SHORT, "Height", hei);
			schematic.Add(TagType.STRING, "Materials", "Alpha");
			schematic.Add(TagType.STRING, "FromMap", world.name);
			schematic.Add(TagType.BYTE_ARRAY, "Blocks", blockLegacyIdArray);
			schematic.Add(TagType.BYTE_ARRAY, "Data", blockLegacyDataArray);
			schematic.AddTag("TileEntities", tileEntities);

			NBTWriter nbt = new NBTWriter();
			try
			{
				nbt.Save(filename, "Schematic", schematic);
			}
			catch (Exception e)
			{
				MessageBox.Show(GetText("fileopened"));
			}
		}

		/// <summary>Returns whether a block is filtered by the user.</summary>
		/// <param name="block">The world block to check</param>
		public bool IsLegacyBlockFiltered(World.LegacyBlock block)
		{
			if (filterBlocksActive && blockLegacyIdMap.ContainsKey(block.id) && filterBlocks.Contains(blockLegacyIdMap[block.id].name))
				return !filterBlocksInvert;

			return filterBlocksInvert;
		}

		/// <summary>Updates the label showing the selection size.</summary>
		private void UpdateSizeLabel()
		{
			if (world.filename != "")
			{
				int len = Math.Abs(selectEnd.X - selectStart.X) + 1;
				int wid = Math.Abs(selectEnd.Y - selectStart.Y) + 1;
				int hei = Math.Abs(selectEnd.Z - selectStart.Z) + 1;
				string text = GetText("size");

				text = text.Replace("%1", len.ToString());
				text = text.Replace("%2", wid.ToString());
				text = text.Replace("%3", hei.ToString());
				text = text.Replace("%4", (len * wid * hei).ToString());
				lblSelSize.Text = text;
			}
			else
				lblSelSize.Text = GetText("noworld");
		}

		/// <summary>Loads the blocks from the Minecraft version file.</summary>
		/// <param name="filename">The version file.</param>
		public void LoadBlocks(string filename)
		{
			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				JsonList blocksList = new JsonList(root["blocks"]);

				foreach (JsonObject curBlock in blocksList)
				{
					Block block = new Block(curBlock["name"]);

					// Get preview from name if available
					if (blockPreviewMap.ContainsKey(block.name))
						block.preview = blockPreviewMap[block.name];

					block.displayName = GetText(block.name, "block");

					// Get preview from file if available
					if (curBlock.ContainsKey("file") && blockPreviewMap.ContainsKey(curBlock["file"]))
						block.preview = blockPreviewMap[curBlock["file"]];

					// Get states
					if (curBlock.ContainsKey("states"))
					{
						JsonObject states = (JsonObject)curBlock["states"];
						foreach (JsonNameValuePair pair in states)
						{
							Block.State state = new Block.State(pair.Key);
							List<dynamic> valuesList = new JsonList(pair.Value);

							// Get values of this state
							foreach (dynamic val in valuesList)
							{
								Block.State.Value value;

								if (val.GetType() == typeof(string))
									value = new Block.State.Value(val);
								else
								{
									JsonObject curValue = (JsonObject)val;
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
						LoadBlocksLegacyData(ref block, (JsonObject)curBlock["legacy_data"], 0, 1);

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
		private void LoadBlocksLegacyData(ref Block block, JsonObject obj, byte bitMask, byte bitBase)
		{
			foreach (JsonNameValuePair pair in obj)
			{
				switch (pair.Key)
				{
					// Bitmasks
					case "0x1":			LoadBlocksLegacyData(ref block, (JsonObject)pair.Value, 1, 1); break;
					case "0x2":			LoadBlocksLegacyData(ref block, (JsonObject)pair.Value, 2, 2); break;
					case "0x4":			LoadBlocksLegacyData(ref block, (JsonObject)pair.Value, 4, 4); break;
					case "0x8":			LoadBlocksLegacyData(ref block, (JsonObject)pair.Value, 8, 8); break;
					case "0x1+0x2":		LoadBlocksLegacyData(ref block, (JsonObject)pair.Value, 3, 1); break;
					case "0x1+0x2+0x4": LoadBlocksLegacyData(ref block, (JsonObject)pair.Value, 7, 1); break;
					case "0x4+0x8":		LoadBlocksLegacyData(ref block, (JsonObject)pair.Value, 12, 4); break;

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
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				foreach (JsonNameValuePair key in root)
				{
					string file = key.Key;
					JsonObject obj = (JsonObject)key.Value;
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

		/// <summary>Loads the world from the combobox and resets the view.</summary>
		private void LoadWorld(string filename)
		{
			Dimension dim;
			if (rbtNether.Checked)
				dim = Dimension.NETHER;
			else if (rbtEnd.Checked)
				dim = Dimension.END;
			else
				dim = Dimension.OVERWORLD;

			if (!world.CanLoad(filename, dim))
				return;

			if (world.Load(filename, dim))
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
				MessageBox.Show(GetText("worldopened"));
				pboxWorldXY.Image = new Bitmap(1, 1);
				pboxWorldXZ.Image = new Bitmap(1, 1);
				btnDone.Enabled = false;
			}
		}

		/// <summary>Gets the XY bitmap of the given chunk. If none have been generated, create it.</summary>
		/// <param name="chunk">The wished chunk.</param>
		private Bitmap GetChunkXYImage(Chunk chunk)
		{
			if (chunk.XYImage != null)
				return chunk.XYImage.Image;
			
			chunk.XYImage = new FastBitmap(16, 16);
			chunk.XYImage.LockImage();

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					Color finalColor = Color.Transparent;
					for (int s = 15; s >= 0; s--)
					{
						Chunk.Section section = chunk.sections[s];
						if (section == null)
							continue;

						for (int z = 15; z >= 0; z--)
						{
							byte id = section.blockLegacyIds[x, y, z];
							if (id == 0)
								continue;

							byte data = section.blockLegacyDatas[x, y, z];
							World.LegacyBlock block = new World.LegacyBlock(id, data);

							Color blockColor = GetLegacyBlockXYColor(block);
							if (blockColor != Color.Transparent)
							{
								bool highlight = false, shade = false;
								World.LegacyBlock blockCheck;
								Color blockCheckColor;

								// Shade
								if (s * 16 + z < 255)
								{
									blockCheck = world.GetLegacyBlock(chunk.X * 16 + x - 1, chunk.Y * 16 + y, s * 16 + z + 1);
									if (blockCheck != null)
									{
										blockCheckColor = GetLegacyBlockXYColor(blockCheck);
										if (blockCheckColor.A == 255)
											shade = true;
									}

									if (!shade)
									{
										blockCheck = world.GetLegacyBlock(chunk.X * 16 + x, chunk.Y * 16 + y - 1, s * 16 + z + 1);
										if (blockCheck != null)
										{
											blockCheckColor = GetLegacyBlockXYColor(blockCheck);
											if (blockCheckColor.A == 255)
												shade = true;
										}
									}
								}

								// Highlight
								blockCheck = world.GetLegacyBlock(chunk.X * 16 + x - 1, chunk.Y * 16 + y, s * 16 + z);
								if (blockCheck == null || blockCheck.id == 0)
									highlight = true;

								if (s * 16 + z > 0 && !highlight)
								{
									blockCheck = world.GetLegacyBlock(chunk.X * 16 + x, chunk.Y * 16 + y - 1, s * 16 + z - 1);
									if (blockCheck == null || blockCheck.id == 0)
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
					}

					chunk.XYImage.SetPixel(x, y, finalColor);
				}
			}

			chunk.XYImage.UnlockImage();

			return chunk.XYImage.Image;
		}

		/// <summary>Gets the XZ bitmap of the given chunk. If none have been generated, create it.</summary>
		/// <param name="chunk">The wished chunk.</param>
		private Bitmap GetChunkXZImage(Chunk chunk)
		{
			if (chunk.XZImage != null)
				return chunk.XZImage.Image;

			chunk.XZImage = new FastBitmap(16, 256);
			chunk.XZImage.LockImage();

			for (int x = 0; x < 16; x++)
			{
				for (int s = 15; s >= 0; s--)
				{
					Chunk.Section section = chunk.sections[s];
					if (section == null)
						continue;

					for (int z = 15; z >= 0; z--)
					{
						Color finalColor = Color.Transparent;
						for (int y = 15; y >= 0; y--)
						{
							byte id = section.blockLegacyIds[x, y, z];
							if (id == 0)
								continue;

							byte data = section.blockLegacyDatas[x, y, z];
							World.LegacyBlock block = new World.LegacyBlock(id, data);

							Color blockColor = GetLegacyBlockXZColor(block);
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
							chunk.XZImage.SetPixel(x, chunk.XZImage.Image.Height - 1 - (s * 16 + z), finalColor);
					}
				}
			}

			chunk.XZImage.UnlockImage();

			return chunk.XZImage.Image;
		}

		/// <summary>Gets the top-down drawing color of the block.</summary>
		/// <param name="block">The block to get the color from.</param>
		private Color GetLegacyBlockXYColor(World.LegacyBlock block)
		{
			if (blockLegacyIdMap.ContainsKey(block.id))
			{
				Block.Preview preview = blockLegacyIdMap[block.id].legacyDataPreview[block.data];
				if (preview != null)
					return preview.XYColor;
			}

			return Color.Transparent;
		}

		/// <summary>Gets the cross-section drawing color of the block.</summary>
		/// <param name="block">The block to get the color from.</param>
		private Color GetLegacyBlockXZColor(World.LegacyBlock block)
		{
			if (blockLegacyIdMap.ContainsKey(block.id))
			{
				Block.Preview preview = blockLegacyIdMap[block.id].legacyDataPreview[block.data];
				if (preview != null)
					return preview.XZColor;
			}

			return Color.Transparent;
		}

		/// <summary>Loads which blocks to filter out.</summary>
		/// <param name="filename">The blockfilter.midata file.</param>
		public void LoadFilterBlocks(string filename)
		{
			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				filterBlocksActive = root["active"];
				filterBlocksInvert = root["invert"];
				JsonList blocksList = new JsonList(root["blocks"]);
				foreach (dynamic curBlock in blocksList)
					filterBlocks.Add((string)curBlock);
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to load block filter");
				Application.Exit();
			}

			UpdateFilterBlocks();
		}

		/// <summary>Updates the label of filterblocks.</summary>
		public void UpdateFilterBlocks()
		{
			lblFilterInfo.Visible = (filterBlocksActive && (filterBlocks.Count > 0 || filterBlocksInvert));
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
				pboxWorldXY.Image = Util.ResizeBitmap(XYMapBitmap, (int)(XYBlocksWidth * XYImageZoom), (int)(XYBlocksHeight * XYImageZoom));

			using (Graphics g = Graphics.FromImage(pboxWorldXY.Image))
			{
				g.DrawImage(spawnImage, (int)((world.spawnPos.X - XYStart.X) * XYImageZoom) - 8, (int)((world.spawnPos.Y - XYStart.Y) * XYImageZoom) - 8);
				g.DrawImage(playerImage, (int)(((int)world.playerPos.X - XYStart.X) * XYImageZoom) - 8, (int)(((int)world.playerPos.Y - XYStart.Y) * XYImageZoom) - 8);
				g.DrawImage(XYSelectBitmap, 0, 0);
			}
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
					map = Util.ResizeBitmap(XZMapBitmap, (int)(XZMapBitmap.Width * XZImageZoom), (int)(256 * XZImageZoom));
				g.DrawImage(map, 0, pboxWorldXZ.Height + XZImageZoom - map.Height);
				g.DrawImage(XZSelectBitmap, 0, 0);
				g.DrawImage(spawnImage, (int)((world.spawnPos.X - XZStart.X) * XZImageZoom) - 8, (int)(pboxWorldXZ.Height - world.spawnPos.Z * XZImageZoom) - 8);
				g.DrawImage(playerImage, (int)(((int)world.playerPos.X - XZStart.X) * XZImageZoom) - 8, (int)(pboxWorldXZ.Height - world.playerPos.Z * XZImageZoom) - 8);
			}
		}

		// Picture box events

		private void OverXY(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;
			pboxWorldXY.Focus();

			if (e.Button == MouseButtons.None)
			{
				XYDragSelect = 0;
				if (Util.MouseRectangle(e.Location, XYSelectStartDraw.X - 4, XYSelectStartDraw.Y, XYSelectStartDraw.X + 4, XYSelectEndDraw.Y))
					XYDragSelect = 8; // L

				if (Util.MouseRectangle(e.Location, XYSelectEndDraw.X - 4, XYSelectStartDraw.Y, XYSelectEndDraw.X + 4, XYSelectEndDraw.Y))
					XYDragSelect = 4; // R

				if (Util.MouseRectangle(e.Location, XYSelectStartDraw.X, XYSelectStartDraw.Y - 4, XYSelectEndDraw.X, XYSelectStartDraw.Y + 4))
					XYDragSelect = 2; // U

				if (Util.MouseRectangle(e.Location, XYSelectStartDraw.X, XYSelectEndDraw.Y - 4, XYSelectEndDraw.X, XYSelectEndDraw.Y + 4))
					XYDragSelect = 6; // D

				if (Util.MouseRectangle(e.Location, XYSelectStartDraw.X - 4, XYSelectStartDraw.Y - 4, XYSelectStartDraw.X + 4, XYSelectStartDraw.Y + 4))
					XYDragSelect = 1; // LU

				if (Util.MouseRectangle(e.Location, XYSelectEndDraw.X - 4, XYSelectStartDraw.Y - 4, XYSelectEndDraw.X + 4, XYSelectStartDraw.Y + 4))
					XYDragSelect = 3; // RU

				if (Util.MouseRectangle(e.Location, XYSelectStartDraw.X - 4, XYSelectEndDraw.Y - 4, XYSelectStartDraw.X + 4, XYSelectEndDraw.Y + 4))
					XYDragSelect = 7; // LD

				if (Util.MouseRectangle(e.Location, XYSelectEndDraw.X - 4, XYSelectEndDraw.Y - 4, XYSelectEndDraw.X + 4, XYSelectEndDraw.Y + 4))
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

		private void btnBrowse_Click(object sender, EventArgs e)
		{
			OpenFileDialog open = new OpenFileDialog();
			open.InitialDirectory = mcSaveFolder;
			open.Title = GetText("browsecaption");
			open.Filter = GetText("browseworlds") + "|level.dat";
			DialogResult res = open.ShowDialog();

			if (res == DialogResult.OK && File.Exists(open.FileName))
				LoadWorld(open.FileName);
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

		private void OverXZ(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			pboxWorldXZ.Focus();
			if (e.Button == MouseButtons.None)
			{
				XZDragSelect = 0;
				if (Util.MouseRectangle(e.Location, XZSelectStartDraw.X - 4, XZSelectStartDraw.Y, XZSelectStartDraw.X + 4, XZSelectEndDraw.Y))
					XZDragSelect = 8; // L

				if (Util.MouseRectangle(e.Location, XZSelectEndDraw.X - 4, XZSelectStartDraw.Y, XZSelectEndDraw.X + 4, XZSelectEndDraw.Y))
					XZDragSelect = 4; // R

				if (Util.MouseRectangle(e.Location, XZSelectStartDraw.X, XZSelectStartDraw.Y - 4, XZSelectEndDraw.X, XZSelectStartDraw.Y + 4))
					XZDragSelect = 2; // U

				if (Util.MouseRectangle(e.Location, XZSelectStartDraw.X, XZSelectEndDraw.Y - 4, XZSelectEndDraw.X, XZSelectEndDraw.Y + 4))
					XZDragSelect = 6; // D

				if (Util.MouseRectangle(e.Location, XZSelectStartDraw.X - 4, XZSelectStartDraw.Y - 4, XZSelectStartDraw.X + 4, XZSelectStartDraw.Y + 4))
					XZDragSelect = 1; // LU

				if (Util.MouseRectangle(e.Location, XZSelectEndDraw.X - 4, XZSelectStartDraw.Y - 4, XZSelectEndDraw.X + 4, XZSelectStartDraw.Y + 4))
					XZDragSelect = 3; // RU

				if (Util.MouseRectangle(e.Location, XZSelectStartDraw.X - 4, XZSelectEndDraw.Y - 4, XZSelectStartDraw.X + 4, XZSelectEndDraw.Y + 4))
					XZDragSelect = 7; // LD

				if (Util.MouseRectangle(e.Location, XZSelectEndDraw.X - 4, XZSelectEndDraw.Y - 4, XZSelectEndDraw.X + 4, XZSelectEndDraw.Y + 4))
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

		// Button events

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
				LoadWorld(world.filename);
		}

		private void rbtNether_CheckedChanged(object sender, EventArgs e)
		{
			if (rbtNether.Checked)
				LoadWorld(world.filename);
		}

		private void rbtEnd_CheckedChanged(object sender, EventArgs e)
		{
			if (rbtEnd.Checked)
				LoadWorld(world.filename);
		}

		private void cbxSaves_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cbxSaves.SelectedIndex == -1)
				return;

			LoadWorld(((WorldOption)cbxSaves.Items[cbxSaves.SelectedIndex]).filename);
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
	}
}