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
	using Vars = Dictionary<string, string>;
	using VarNameValuePair = KeyValuePair<string, string>;
	
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
					public ushort id;
					public short previewKey = 0;

					public Value(string name, ushort id)
					{
						this.name = name;
						this.id = id;
					}
				}

				public string name;
				public ushort id;
				public Dictionary<string, Block.State.Value> valueNameMap = new Dictionary<string, Block.State.Value>();

				public State(string name, ushort id)
				{
					this.name = name;
					this.id = id;
				}
			}

			public string name, displayName;
			public Block.Preview preview = null;
			public Dictionary<string, Block.State> stateNameMap = new Dictionary<string, Block.State>();
			public Dictionary<string, Vars> mcIdVarsMap = new Dictionary<string, Vars>();
			public short[] stateIdPreviewKey;
			public Vars defaultVars = null;

			public Block(string name)
			{
				this.name = name;
			}
		}

		/// <summary>A choice in the world combobox.</summary>
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
#if DEBUG
		public static string currentFolder = @"C:\Dev\Mine-imator\datafiles\Data";
#else
		public static string currentFolder = Application.StartupPath;
#endif
		public static string mcAssetsFile = currentFolder + @"\Minecraft\1.17.1.midata";
		public static string miLangFile = currentFolder + @"\Languages\english.milanguage";
		public static string miBlockPreviewFile = currentFolder + @"\blockpreview.midata";
		public static string miBlockFilterFile = currentFolder + @"\blockfilter.midata";
		public static string miSettingsFile = currentFolder + @"\settings.midata";
		public static string miLegacyFile = currentFolder + @"\legacy.midata";

		// Language
		public Dictionary<string, string> languageMap = new Dictionary<string, string>();

		// Blocks
		public Dictionary<string, Block> blockNameMap = new Dictionary<string, Block>(); // Name -> Block
		public Dictionary<string, Block> blockMcIdMap = new Dictionary<string, Block>(); // Minecraft ID -> Block
		public Dictionary<short, Block.Preview> blockPreviewMap = new Dictionary<short, Block.Preview>(); // Preview key -> preview
		public Dictionary<string, int> blockPreviewFileKeyMap = new Dictionary<string, int>(); // JSON filename -> preview key
		public string[,] blockLegacyMcId = new string[256, 16]; // Legacy ID + data -> Minecraft ID
		public short[,] blockLegacyPreviewKey = new short[256, 16]; // Legacy ID + data -> Preview key

		// Filter
		public bool filterBlocksActive = false, filterBlocksInvert = false;
		public List<string> filterBlockNames = new List<string>();

		// Program
		string savefile = "";
		World world = new World();
		SaveRegion selectRegion = new SaveRegion();

		// 3D selector
		Point moveStartMPos, moveStartPos, XYImageMidPos, XZImageMidPos;
		float XYImageZoom = 8, XZImageZoom = 4;
		byte XYDragView = 0, XYDragSelect = 0;
		byte XZDragView = 0, XZDragSelect = 0;

		Image spawnImage = import.Properties.Resources.spawn;
		Image playerImage = import.Properties.Resources.player;
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
            LoadSettings(miSettingsFile);

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

			if (!File.Exists(miLegacyFile))
			{
				MessageBox.Show("Could not find legacy.midata, re-install the program.");
				Application.Exit();
				return;
			}

			LoadLanguage(miLangFile);
			LoadBlockPreviews(miBlockPreviewFile);
			LoadBlocks(mcAssetsFile);
			LoadLegacyBlocks(miLegacyFile);

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

			// Set labels
			rbtNether.Location = new Point(rbtOver.Location.X + rbtOver.Width + 5, rbtOver.Location.Y);
			rbtEnd.Location = new Point(rbtNether.Location.X + rbtNether.Width + 5, rbtOver.Location.Y);
			UpdateSizeLabel();
			UpdateFilterBlocks();

			// Get worlds
			if (Directory.Exists(mcSaveFolder))
				foreach (DirectoryInfo d in new DirectoryInfo(mcSaveFolder).GetDirectories())
					if (File.Exists(d.FullName + @"\level.dat"))
						cbxSaves.Items.Add(new WorldOption(d.FullName + @"\level.dat", d.Name));
		}

		/// <summary>Loads the chosen translation file from the settings (if available).</summary>
		private void LoadSettings(string filename)
		{
			if (!File.Exists(filename))
				return;

			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				JsonObject settingsAssets = root["assets"];
				JsonObject settingsInterface = root["interface"];
				mcAssetsFile = currentFolder + @"\Minecraft\" + settingsAssets["version"]+ ".midata";
				miLangFile = settingsInterface["language_filename"];
			}
			catch (Exception e)
			{
				return;
			}
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
		
		/// <summary>Loads the top-down/cross-section colors of the blocks, as generated by Mine-imator.</summary>
		public void LoadBlockPreviews(string filename)
		{
			// No preview
			blockPreviewMap[0] = new Block.Preview(Color.Transparent, Color.Transparent);

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

					blockPreviewFileKeyMap[file] = blockPreviewMap.Count;
					blockPreviewMap[(short)blockPreviewMap.Count] = preview;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to load block previews");
				Application.Exit();
			}
		}

		/// <summary>Loads the blocks from the Minecraft version file.</summary>
		/// <param name="filename">The version file.</param>
		private void LoadBlocks(string filename)
		{
			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				JsonList blocksList = new JsonList(root["blocks"]);

				foreach (JsonObject curBlock in blocksList)
				{
					// Create block and set name
					Block block = new Block(curBlock["name"]);
					block.displayName = GetText(block.name, "block");

					// Default state vars
					if (curBlock.ContainsKey("default_state"))
						block.defaultVars = StringToVars(curBlock["default_state"]);

					// Get states (each possible combination of values for each variant is represented as an unique state ID for the block)
					ushort stateIdAmount = 1;
					if (curBlock.ContainsKey("states"))
					{
						JsonObject states = (JsonObject)curBlock["states"];

						foreach (JsonNameValuePair pair in states)
						{
							Block.State state = new Block.State(pair.Key, stateIdAmount);
							List<dynamic> valuesList = new JsonList(pair.Value);
							stateIdAmount *= (ushort)valuesList.Count;
							ushort valId = 0;

							// Get values of this state
							foreach (dynamic val in valuesList)
							{
								Block.State.Value value;

								if (val.GetType() == typeof(string))
									value = new Block.State.Value(val, valId++);
								else
								{
									JsonObject curValue = (JsonObject)val;
									value = new Block.State.Value(curValue["value"], valId++);

									if (curValue.ContainsKey("file") && blockPreviewFileKeyMap.ContainsKey(curValue["file"]))
										value.previewKey = (short)blockPreviewFileKeyMap[curValue["file"]];
								}

								state.valueNameMap[value.name] = value;
							}

							block.stateNameMap[pair.Key] = state;
						}
					}

					// Connect to Minecraft ID(s)
					dynamic mcId = curBlock["id"];
					if (mcId.GetType() == typeof(string))
					{ 
						blockMcIdMap[mcId] = block;
						block.mcIdVarsMap[mcId] = null;
					}
					else
					{
						// Store the variables for each Minecraft ID
						foreach (JsonNameValuePair pair in mcId)
						{
							blockMcIdMap[pair.Key] = block;
							if (pair.Value != "")
								block.mcIdVarsMap[pair.Key] = StringToVars(pair.Value);
							else
								block.mcIdVarsMap[pair.Key] = null;
						}
					}

					// Get preview key from name if available (water/lava only)
					short previewKey = 0;
					if (blockPreviewFileKeyMap.ContainsKey(block.name))
						previewKey = (short)blockPreviewFileKeyMap[block.name];

					// Get preview key from file if available
					if (curBlock.ContainsKey("file") && blockPreviewFileKeyMap.ContainsKey(curBlock["file"]))
						previewKey = (short)blockPreviewFileKeyMap[curBlock["file"]];

					// Generate preview keys for each possible state (numerical ID) of the block
					block.stateIdPreviewKey = new short[stateIdAmount];
					for (var i = 0; i < stateIdAmount; i++)
						block.stateIdPreviewKey[i] = previewKey;

					foreach (dynamic nameState in block.stateNameMap)
					{
						Block.State state = (Block.State)nameState.Value;
						foreach (dynamic nameValue in state.valueNameMap)
						{
							Block.State.Value value = (Block.State.Value)nameValue.Value;
							if (value.previewKey == 0)
								continue;

							// Apply value preview colors to affected state IDs
							for (var i = 0; i < stateIdAmount; i++)
							{
								var val = Util.IntDiv(i, state.id);

								// Fill in states that may not have a preview key
								val = (val % state.valueNameMap.Count);

								if (val == value.id)
									block.stateIdPreviewKey[i] = value.previewKey;
							}
						}
					}

					blockNameMap[block.name] = block;
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to load Minecraft assets");
				Application.Exit();
			}
		}

		/// <summary> Load legacy block IDs/data and translate to block names and preview keys.</summary>
		private void LoadLegacyBlocks(string filename)
		{
			// Clear data
			for (int i = 0; i < 256; i++)
			{
				for (int d = 0; d < 16; d++)
				{
					blockLegacyMcId[i, d] = "";
					blockLegacyPreviewKey[i, d] = 0;
				}

			}

			// Parse file
			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				JsonObject legacyBlockIds = new JsonObject(root["legacy_block_id"]);

				foreach (JsonNameValuePair pair in legacyBlockIds)
				{
					byte legacyId = Convert.ToByte(pair.Key);
					JsonObject curBlock = (JsonObject)pair.Value;

					// Look for Minecraft ID
					string mcId = "";
					if (curBlock.ContainsKey("id"))
						mcId = curBlock["id"];

					// Minecraft ID and variables for each data value
					Vars[] dataVars = new Vars[16];
					for (int d = 0; d < 16; d++)
						dataVars[d] = new Vars();

					// Process data
					if (curBlock.ContainsKey("data"))
						LoadLegacyBlocksData((JsonObject)curBlock["data"], 0, 1, ref dataVars);

					// Get preview keys for each data value
					for (int d = 0; d < 16; d++)
					{
						blockLegacyPreviewKey[legacyId, d] = 0;

						string curMcId = mcId;
						if (dataVars[d].ContainsKey("id"))
						{
							curMcId = dataVars[d]["id"];
							dataVars[d].Remove("id");
						}

						blockLegacyMcId[legacyId, d] = curMcId;
						if (!blockMcIdMap.ContainsKey(curMcId))
							continue;

						blockLegacyPreviewKey[legacyId, d] = GetBlockPreviewKey(curMcId, dataVars[d]);
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show("Failed to load Legacy file");
				Application.Exit();
			}
		}
		private void LoadLegacyBlocksData(JsonObject obj, byte bitMask, byte bitBase, ref Vars[] dataVars)
		{
			foreach (JsonNameValuePair pair in obj)
			{
				switch (pair.Key)
				{
					// Bitmasks
					case "0x1":			LoadLegacyBlocksData((JsonObject)pair.Value, 1, 1, ref dataVars); break;
					case "0x2":			LoadLegacyBlocksData((JsonObject)pair.Value, 2, 2, ref dataVars); break;
					case "0x4":			LoadLegacyBlocksData((JsonObject)pair.Value, 4, 4, ref dataVars); break;
					case "0x8":			LoadLegacyBlocksData((JsonObject)pair.Value, 8, 8, ref dataVars); break;
					case "0x1+0x2":		LoadLegacyBlocksData((JsonObject)pair.Value, 3, 1, ref dataVars); break;
					case "0x1+0x2+0x4": LoadLegacyBlocksData((JsonObject)pair.Value, 7, 1, ref dataVars); break;
					case "0x4+0x8":		LoadLegacyBlocksData((JsonObject)pair.Value, 12, 4, ref dataVars); break;

					// Number (apply previous bitmask)
					default:
					{
						byte value = Convert.ToByte(pair.Key);
						Vars vars = StringToVars(pair.Value);

						// Insert into array
						if (bitMask > 0)
						{
							for (var d = 0; d < 16; d++)
								if ((d & bitMask) / bitBase == value) // Check data value with bitmask
									VarsAdd(ref dataVars[d], vars);
						}
						else
							VarsAdd(ref dataVars[value], vars);

						break;
					}
				}
			}
		}

		/// <summary>Converts a string of variables (like "foo=10,bar=hello") into a new Dictionary of name/value pairs.</summary>
		/// <param name="str">The string to convert.</param>
		Vars StringToVars(string str)
		{
			Vars vars = new Vars();
			string[] varsSplit = str.Split(new char[] { ',' });

			foreach (string s in varsSplit)
			{
				string[] nameValSplit = s.Split(new char[] { '=' });
				vars[nameValSplit[0]] = nameValSplit[1];
			}

			return vars;
		}

		/// <summary>Adds two sets of variables together and stores the result in the first set.</summary>
		/// <param name="dest">The target set of variables to be modified.</param>
		/// <param name="source">A second set of variables that will overwrite the first.</param>
		private void VarsAdd(ref Vars dest, Vars source)
		{
			foreach (VarNameValuePair pair in source)
				dest[pair.Key] = pair.Value;
		}

		/// <summary>Returns the preview colors of the block with the given Minecraft ID.</summary>
		/// <param name="mcId">The Minecraft ID to check.</param>
		/// <param name="nbtVars">The variables of the block.</param>
		public short GetBlockPreviewKey(string mcId, NBTCompound nbtVars)
		{
			if (nbtVars == null)
				return GetBlockPreviewKey(mcId);
			
			// Convert dictionary of string->NBTTag of TAG_String to string->string
			Vars vars = new Vars();
			foreach (KeyValuePair<string, NBTTag> nbtPair in nbtVars.value)
				vars[nbtPair.Key] = (nbtPair.Value).value;

			return GetBlockPreviewKey(mcId, vars);
		}
		public short GetBlockPreviewKey(string mcId, Vars vars = null)
		{
			// Check if the Minecraft ID has no definition, and thus shouldn't be displayed
			if (!blockMcIdMap.ContainsKey(mcId))
				return 0;

			// Find the current Mine-imator block from the Minecraft ID
			Block block = blockMcIdMap[mcId];

			// Combine with the variables of the block's Minecraft ID
			Vars finalVars = new Vars();

			if (block.defaultVars != null)
				VarsAdd(ref finalVars, block.defaultVars);

			if (block.mcIdVarsMap[mcId] != null)
				VarsAdd(ref finalVars, block.mcIdVarsMap[mcId]);

			if (vars != null)
				VarsAdd(ref finalVars, vars);

			// Calculate state ID of the block and get final preview key
			ushort stateId = 0;
			foreach (VarNameValuePair pair in finalVars)
			{
				string stateName = pair.Key;
				string stateVal = pair.Value;

				if (stateName != "id" && block.stateNameMap.ContainsKey(stateName))
				{
					Block.State blockState = block.stateNameMap[stateName];
					if (blockState.valueNameMap.ContainsKey(stateVal))
						stateId += (ushort)(blockState.id * blockState.valueNameMap[stateVal].id);
				}
			}

			return block.stateIdPreviewKey[stateId];
		}

		/// <summary>Returns whether a block is filtered by the user.</summary>
		/// <param name="mcId">The Minecraft ID of the block to check.</param>
		public bool IsBlockFiltered(string mcId)
		{
			if (!filterBlocksActive || !blockMcIdMap.ContainsKey(mcId))
				return false;

			if (filterBlockNames.Contains(blockMcIdMap[mcId].name))
				return !filterBlocksInvert;

			return filterBlocksInvert;
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

			if (world.Load(filename, dim))
			{
				XYImageMidPos = new Point((int)world.playerPos.X, (int)world.playerPos.Y);
				XYImageZoom = 8;
				selectRegion.start = new Point3D<int>((int)world.playerPos.X - 10, (int)world.playerPos.Y - 10, (int)world.playerPos.Z - 10);
				selectRegion.end = new Point3D<int>((int)world.playerPos.X + 10, (int)world.playerPos.Y + 10, (int)world.playerPos.Z + 10);
				selectRegion.start.Z = Math.Max(Math.Min(selectRegion.start.Z, 319), -64);
				selectRegion.end.Z = Math.Max(Math.Min(selectRegion.end.Z, 319), -64);
				UpdateSizeLabel();
				btnDone.Enabled = true;
				XZImageMidPos = new Point(selectRegion.start.X + (selectRegion.end.X - selectRegion.start.X) / 2, selectRegion.start.Z + (selectRegion.end.Z - selectRegion.start.Z) / 2);

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

		/// <summary>Saves the selection into a .schematic file.</summary>
		private void SaveBlocks(string filename)
		{
			try
			{
				NBTWriter nbt = new NBTWriter();
				nbt.Save(filename, "Schematic", world.SaveBlocks(selectRegion));
			}
			catch (IOException e)
			{
				MessageBox.Show(GetText("fileopened"));
			}
		}

		/// <summary>Updates the label showing the selection size.</summary>
		private void UpdateSizeLabel()
		{
			if (world.filename != "")
			{
				int len = Math.Abs(selectRegion.end.X - selectRegion.start.X) + 1;
				int wid = Math.Abs(selectRegion.end.Y - selectRegion.start.Y) + 1;
				int hei = Math.Abs(selectRegion.end.Z - selectRegion.start.Z) + 1;
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

		/// <summary>Gets the XY bitmap of a chunk. If none have been generated, create it.</summary>
		/// <param name="chunk">The current chunk.</param>
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

					for (int s = 24; s >= 0; s--)
					{
						Chunk.Section section = chunk.sections[s];
						if (section == null)
							continue;

						for (int z = 15; z >= 0; z--)
						{
							short blockPreviewKey = section.blockPreviewKey[x, y, z];
							if (blockPreviewKey == 0)
								continue;

							if (!section.IsBlockSaved(x, y, z))
								continue;

							Color blockColor = blockPreviewMap[blockPreviewKey].XYColor;
							if (blockColor != Color.Transparent)
							{
								bool highlight = false, shade = false;

								// Shade
								if (s * 16 + z < 255)
								{
									blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x - 1, chunk.Y * 16 + y, s * 16 + z + 1);
									if (blockPreviewKey != 0 && blockPreviewMap[blockPreviewKey].XYColor.A == 255)
										shade = true;
									else
									{
										blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x, chunk.Y * 16 + y - 1, s * 16 + z + 1);
										if (blockPreviewKey != 0 && blockPreviewMap[blockPreviewKey].XYColor.A == 255)
											shade = true;
									}
								}

								// Highlight
								blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x - 1, chunk.Y * 16 + y, s * 16 + z);
								if (blockPreviewKey == 0)
									highlight = true;
								else if (s * 16 + z > 0)
								{
									blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x, chunk.Y * 16 + y - 1, s * 16 + z - 1);
									if (blockPreviewKey == 0)
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

		/// <summary>Gets the XZ bitmap of a chunk. If none have been generated, create it.</summary>
		/// <param name="chunk">The current chunk.</param>
		private Bitmap GetChunkXZImage(Chunk chunk)
		{
			if (chunk.XZImage != null)
				return chunk.XZImage.Image;

			chunk.XZImage = new FastBitmap(16, 384);
			chunk.XZImage.LockImage();

			for (int x = 0; x < 16; x++)
			{
				for (int s = 24; s >= 0; s--)
				{
					Chunk.Section section = chunk.sections[s];
					if (section == null)
						continue;

					for (int z = 15; z >= 0; z--)
					{
						Color finalColor = Color.Transparent;
						for (int y = 15; y >= 0; y--)
						{
							short blockPreviewKey = section.blockPreviewKey[x, y, z];
							if (blockPreviewKey == 0)
								continue;

							if (!section.IsBlockSaved(x, y, z))
								continue;

							Color blockColor = blockPreviewMap[blockPreviewKey].XZColor;
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
					filterBlockNames.Add((string)curBlock);
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
			lblFilterInfo.Visible = (filterBlocksActive && (filterBlockNames.Count > 0 || filterBlocksInvert));

			// Clear old images
			world.ClearChunkImages();
			GC.Collect();
			GC.WaitForPendingFinalizers();

			// Update
			UpdateXYMap(0, 0);
			UpdateXZMap();
		}

		/// <summary>Updates the bitmap of the XY map.</summary>
		/// <param name="x">View movement along the x axis.</param>
		/// <param name="y">View movement along the y axis.</param>
		/// <param name="clear">Whether to clear all the old chunk images.</param>
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
			int sx = Math.Min(selectRegion.start.X, selectRegion.end.X), sy = Math.Min(selectRegion.start.Y, selectRegion.end.Y);
			int ex = Math.Max(selectRegion.start.X, selectRegion.end.X), ey = Math.Max(selectRegion.start.Y, selectRegion.end.Y);
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
			XZStart = new Point((int)Math.Floor(XZImageMidPos.X - (screenwid / XZImageZoom) / 2), (int)Math.Floor(XZImageMidPos.Y - (screenhei / XZImageZoom) / 2));
			Bitmap bmp = new Bitmap(XZBlocksWidth, 384);

			// Find chunks and draw them
			using (Graphics g = Graphics.FromImage(bmp))
			{
				for (int dy = selectRegion.end.Y - 48; dy <= selectRegion.end.Y; dy += 16)
				{
					for (int dx = 0; dx < XZBlocksWidth + 16; dx += 16)
					{
						int cx = XZStart.X + dx;
						Chunk chunk = world.GetChunk(cx, dy);
						if (chunk != null)
						{
							Bitmap img = GetChunkXZImage(chunk);
							g.DrawImage(img, chunk.X * 16 - XZStart.X, 384 - img.Height);
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
			int sx = Math.Min(selectRegion.start.X, selectRegion.end.X), sz = Math.Min(selectRegion.start.Z, selectRegion.end.Z);
			int ex = Math.Max(selectRegion.start.X, selectRegion.end.X), ez = Math.Max(selectRegion.start.Z, selectRegion.end.Z);
			XZSelectStartDraw = new Point((int)((sx - XZStart.X) * XZImageZoom - XZImageZoom / 2), pboxWorldXZ.Size.Height - Math.Max(1, (int)((ez - XZStart.Y) * XZImageZoom + XZImageZoom / 2)));
			XZSelectEndDraw = new Point((int)((ex - XZStart.X) * XZImageZoom + XZImageZoom / 2), pboxWorldXZ.Size.Height - Math.Max(1, (int)((sz - XZStart.Y)* XZImageZoom - XZImageZoom / 2)));
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
					map = Util.ResizeBitmap(XZMapBitmap, (int)(XZMapBitmap.Width * XZImageZoom), (int)(384 * XZImageZoom));

				int yoff = XZImageMidPos.Y - 128;
				g.DrawImage(map, 0, pboxWorldXZ.Height / 2 - map.Height / 2 + yoff * XZImageZoom);
				g.DrawImage(XZSelectBitmap, 0, 0);
				g.DrawImage(spawnImage, (int)((world.spawnPos.X - XZStart.X) * XZImageZoom) - 8, pboxWorldXZ.Height - (int)((world.spawnPos.Z - XZStart.Y) * XZImageZoom) - 8);
				g.DrawImage(playerImage, (int)((world.playerPos.X - XZStart.X) * XZImageZoom) - 8, pboxWorldXZ.Height - (int)((world.playerPos.Z - XZStart.Y) * XZImageZoom) - 8);
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
					Point startprevpos = new Point(selectRegion.start.X, selectRegion.start.Y);
					Point endprevpos = new Point(selectRegion.end.X, selectRegion.end.Y);
					if (XYDragSelect == 1) // LU
					{
						selectRegion.start.X = moveStartPos.X + dx;
						selectRegion.start.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 2) // U
						selectRegion.start.Y = moveStartPos.Y + dy;

					if (XYDragSelect == 3) // RU
					{
						selectRegion.end.X = moveStartPos.X + dx;
						selectRegion.start.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 4) // R
						selectRegion.end.X = moveStartPos.X + dx;

					if (XYDragSelect == 5) // RD
					{
						selectRegion.end.X = moveStartPos.X + dx;
						selectRegion.end.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 6) // D
						selectRegion.end.Y = moveStartPos.Y + dy;

					if (XYDragSelect == 7) // LD
					{
						selectRegion.start.X = moveStartPos.X + dx;
						selectRegion.end.Y = moveStartPos.Y + dy;
					}

					if (XYDragSelect == 8) // L
						selectRegion.start.X = moveStartPos.X + dx;

					UpdateSizeLabel();
					Point startnewpos = new Point(selectRegion.start.X, selectRegion.start.Y);
					Point endnewpos = new Point(selectRegion.end.X, selectRegion.end.Y);
					if (startprevpos != startnewpos || endprevpos != endnewpos)
						UpdateXYSel();
				}
			}

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
					selectRegion.start = new Point3D<int>((int)(e.Location.X / XYImageZoom + XYStart.X), (int)(e.Location.Y / XYImageZoom + XYStart.Y + 1), selectRegion.start.Z);
					selectRegion.end = new Point3D<int>((int)(e.Location.X / XYImageZoom + XYStart.X), (int)(e.Location.Y / XYImageZoom + XYStart.Y + 1), selectRegion.end.Z);
					UpdateSizeLabel();
				}
				else //Move view
				{
					XYDragView = 1;
					moveStartPos = XYImageMidPos;
				}
			}

			if (XYDragSelect == 1 || XYDragSelect == 2)
				moveStartPos = new Point(selectRegion.start.X, selectRegion.start.Y);
			else if (XYDragSelect == 3 || XYDragSelect == 4)
				moveStartPos = new Point(selectRegion.end.X, selectRegion.start.Y);
			else if (XYDragSelect == 5 || XYDragSelect == 6)
				moveStartPos = new Point(selectRegion.end.X, selectRegion.end.Y);
			else if (XYDragSelect == 7 || XYDragSelect == 8)
				moveStartPos = new Point(selectRegion.start.X, selectRegion.end.Y);
			moveStartMPos = e.Location;
		}

		private void MoveXYEnd(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			XYDragView = 0;
			if (selectRegion.start.X > selectRegion.end.X)
			{
				int tmp = selectRegion.start.X;
				selectRegion.start.X = selectRegion.end.X;
				selectRegion.end.X = tmp;
			}

			if (selectRegion.start.Y > selectRegion.end.Y)
			{
				int tmp = selectRegion.start.Y;
				selectRegion.start.Y = selectRegion.end.Y;
				selectRegion.end.Y = tmp;
			}

			XYDragSelect = 0;
			XZImageMidPos = new Point(selectRegion.start.X + (selectRegion.end.X - selectRegion.start.X) / 2, selectRegion.start.Z + (selectRegion.end.Z - selectRegion.start.Z) / 2);
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
					XZImageMidPos = new Point(moveStartPos.X - dx, (moveStartPos.Y - dy));
					Point newpos = new Point(XZImageMidPos.X, XZImageMidPos.Y);
					if (prevpos != newpos)
						UpdateXZMap();
				}
				else // Change rectangle
				{
					Point startprevpos = new Point(selectRegion.start.X, selectRegion.start.Z);
					Point endprevpos = new Point(selectRegion.end.X, selectRegion.end.Z);
					if (XZDragSelect == 1) // LU
					{
						selectRegion.start.X = moveStartPos.X + dx;
						selectRegion.end.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 2) // U
						selectRegion.end.Z = moveStartPos.Y + dy;

					if (XZDragSelect == 3) //RU
					{
						selectRegion.end.X = moveStartPos.X + dx;
						selectRegion.end.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 4) // R
						selectRegion.end.X = moveStartPos.X + dx;

					if (XZDragSelect == 5) // RD
					{
						selectRegion.end.X = moveStartPos.X + dx;
						selectRegion.start.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 6) // D
						selectRegion.start.Z = moveStartPos.Y + dy;

					if (XZDragSelect == 7) // LD
					{
						selectRegion.start.X = moveStartPos.X + dx;
						selectRegion.start.Z = moveStartPos.Y + dy;
					}

					if (XZDragSelect == 8) // L
						selectRegion.start.X = moveStartPos.X + dx;

					selectRegion.start.Z = Math.Max(Math.Min(selectRegion.start.Z, 319), -64);
					selectRegion.end.Z = Math.Max(Math.Min(selectRegion.end.Z, 319), -64);
					UpdateSizeLabel();
					Point startnewpos = new Point(selectRegion.start.X, selectRegion.start.Z);
					Point endnewpos = new Point(selectRegion.end.X, selectRegion.end.Z);
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
					selectRegion.start = new Point3D<int>((int)(e.Location.X / XZImageZoom + XZStart.X), selectRegion.start.Y, (int)((pboxWorldXZ.Size.Height - e.Location.Y) / XZImageZoom + XZStart.Y + 1));
					selectRegion.end = new Point3D<int>((int)(e.Location.X / XZImageZoom + XZStart.X), selectRegion.end.Y, (int)((pboxWorldXZ.Size.Height - e.Location.Y) / XZImageZoom + XZStart.Y  + 1));
					selectRegion.start.Z = Math.Max(Math.Min(selectRegion.start.Z, 319), -64);
					selectRegion.end.Z = Math.Max(Math.Min(selectRegion.end.Z, 319), -64);
					UpdateSizeLabel();
				}
				else //Move view
				{
					XZDragView = 1;
					moveStartPos = XZImageMidPos;
				}
			}

			if (XZDragSelect == 1 || XZDragSelect == 2)
				moveStartPos = new Point(selectRegion.start.X, selectRegion.end.Z);

			else if (XZDragSelect == 3 || XZDragSelect == 4)
				moveStartPos = new Point(selectRegion.end.X, selectRegion.end.Z);

			else if (XZDragSelect == 5 || XZDragSelect == 6)
				moveStartPos = new Point(selectRegion.end.X, selectRegion.start.Z);

			else if (XZDragSelect == 7 || XZDragSelect == 8)
				moveStartPos = new Point(selectRegion.start.X, selectRegion.start.Z);

			moveStartMPos = e.Location;
		}

		private void MoveXZEnd(object sender, MouseEventArgs e)
		{
			if (world.filename == "")
				return;

			XZDragView = 0;
			if (selectRegion.start.X > selectRegion.end.X)
			{
				int tmp = selectRegion.start.X;
				selectRegion.start.X = selectRegion.end.X;
				selectRegion.end.X = tmp;
			}

			if (selectRegion.start.Z > selectRegion.end.Z)
			{
				int tmp = selectRegion.start.Z;
				selectRegion.start.Z = selectRegion.end.Z;
				selectRegion.end.Z = tmp;
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