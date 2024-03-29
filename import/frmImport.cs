﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

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
				public String tint;

				public Preview(Color XYColor, Color XZColor, String tint)
				{
					this.XYColor = XYColor;
					this.XZColor = XZColor;
					this.tint = tint;
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
			public bool waterlogged = false;
			public Block(string name)
			{
				this.name = name;
			}
		}

		public class Biome
		{
			public Color grassColor, foliageColor, waterColor;
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

		/// <summary>A choice in the dimension combobox.</summary>
		public class DimOption
		{
			public string name, displayName, datapackName;
			public bool custom;
			public int dimID;
			public DimOption(string name, string displayName, bool custom, int dimID, string datapackName)
			{
				this.name = name;
				this.displayName = displayName;
				this.custom = custom;
				this.dimID = dimID;
				this.datapackName = datapackName;
			}

			public override string ToString()
			{
				return displayName;
			}

			public void copy(DimOption dim)
            {
				this.name = dim.name;
				this.displayName = dim.displayName;
				this.custom = dim.custom;
				this.dimID = dim.dimID;
				this.datapackName = dim.datapackName;
			}
		}

		// Folders
		public static string mcSaveFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\.minecraft\saves";
#if DEBUG
		public static string currentFolder = @"C:\Dev\Mine-imator\datafiles\Data";
#else
		public static string currentFolder = Application.StartupPath;
#endif
		public static string mcAssetsFile = currentFolder + @"\Minecraft\1.18.midata";
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

		// Biomes
		public Dictionary<string, Biome> biomeMap = new Dictionary<string, Biome>();
		public Dictionary<int, string> biomeIdMap = new Dictionary<int, string>(); // 1.13+ biome ID -> Biome string
		public Dictionary<int, string> legacyBiomeIdMap = new Dictionary<int, string>(); // Legacy(1.12/older) biome ID -> Biome string

		// Filter
		public bool filterBlocksActive = false, filterBlocksInvert = false;
		public List<string> filterBlockNames = new List<string>();

		// Program
		string savefile = "";
		World world = new World();
		SaveRegion selectRegion = new SaveRegion();
		DimOption dimensionSelected = new DimOption("", "", false, 0, "");

		// Region/chunk load queues
		public ConcurrentQueue<Region> regionQueue = new ConcurrentQueue<Region>();
		public Thread regionQueueThread = new Thread(frmImport.LoadRegionQueue);
		public FileStream regionFileStream = null;

		public ConcurrentQueue<Chunk> ChunkImageXYQueue = new ConcurrentQueue<Chunk>();
		public ConcurrentQueue<Chunk> ChunkImageXZQueue = new ConcurrentQueue<Chunk>();
		public Thread ChunkImageThread = new Thread(frmImport.ChunkImageQueue);

		public Thread UIThread = Thread.CurrentThread;

		// Thread for drawing views when needed
		public Thread DrawEventThread = new Thread(frmImport.DrawEventListener);
		public bool updateXYView, updateXZView = false;
		bool updateXYSel, updateXZSel = false;
		int updateXYViewMoveX, updateXYViewMoveY = 0;

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

		private static void LoadRegionQueue()
		{
			frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

			while (true)
			{
				if (!main.UIThread.IsAlive)
					Thread.CurrentThread.Abort();

				for (int i = 0; i < main.regionQueue.Count; i++)
				{
					Region r;
					main.regionQueue.TryPeek(out r);

					if (r != null)
						r.Load();

					main.regionQueue.TryDequeue(out r);

					main.updateXYView = true;
					main.updateXZView = true;
				}
			}
		}

		private static void ChunkImageQueue()
		{
			frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

			while (true)
			{
				if (!main.UIThread.IsAlive)
					Thread.CurrentThread.Abort();

				Parallel.ForEach(main.ChunkImageXYQueue, c =>
				{
					if (!c.XYImageInQueue)
						return;

					if (c != null)
					{
						main.GetChunkXYImage(c);
						c.XYImageInQueue = false;
						main.ChunkImageXYQueue.TryDequeue(out c);
						main.updateXYView = true;
					}
				});

				Parallel.ForEach(main.ChunkImageXZQueue, c =>
				{
					if (!c.XZImageInQueue)
						return;

					if (c != null)
					{
						main.GetChunkXZImage(c);
						c.XZImageInQueue = false;
						main.ChunkImageXZQueue.TryDequeue(out c);
						main.updateXZView = true;
					}
				});
			}
		}

		private static void DrawEventListener()
		{
			frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

			while (true)
			{
				if (!main.UIThread.IsAlive)
					Thread.CurrentThread.Abort();

				if (main.updateXYView)
				{
					main.updateXYView = false;
					main.UpdateXYMap(main.updateXYViewMoveX, main.updateXYViewMoveY);
					main.updateXYViewMoveX = 0;
					main.updateXYViewMoveY = 0;
				}

				if (main.updateXYSel)
				{
					main.updateXYSel = false;
					main.UpdateXYSel();
				}

				if (main.updateXZView)
				{
					main.updateXZView = false;
					main.UpdateXZMap();
				}

				if (main.updateXZSel)
				{
					main.updateXZSel = false;
					main.UpdateXZSel();	
				}
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
			LoadLegacy(miLegacyFile);

			if (File.Exists(miBlockFilterFile))
				LoadFilterBlocks(miBlockFilterFile);

			// Set text
			Text = GetText("title");
			lblInfo.Text = GetText("info");
			lblTopDownView.Text = GetText("topdownview");
			lblCrossSectionView.Text = GetText("crosssectionview");
			lblWorld.Text = GetText("world") + ":";
			lblDimension.Text = GetText("dimension") + ":";
			btnBrowse.Text = GetText("browse");
			btnDone.Text = GetText("done");
			btnFilters.Text = GetText("filters");
			btnCancel.Text = GetText("cancel");
			lblFilterInfo.Text = GetText("filtersalert");
			lblLoadingRegion.Visible = false;

			// Set labels
			UpdateSizeLabel();
			UpdateFilterBlocks();

			// Get worlds
			if (Directory.Exists(mcSaveFolder))
				foreach (DirectoryInfo d in new DirectoryInfo(mcSaveFolder).GetDirectories())
					if (File.Exists(d.FullName + @"\level.dat"))
						cbxSaves.Items.Add(new WorldOption(d.FullName, d.Name));

			LoadDimensions("");
			cbxDimensions.SelectedIndex = 0;

			frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

			if (main != null)
			{
				main.regionQueueThread.Start();
				main.ChunkImageThread.Start();
				main.DrawEventThread.Start();
			}
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
			blockPreviewMap[0] = new Block.Preview(Color.Transparent, Color.Transparent, "");

			string json = File.ReadAllText(filename);
			try
			{
				JsonObject root = (JsonObject)serializer.DeserializeObject(json);
				foreach (JsonNameValuePair key in root)
				{
					string file = key.Key;
					JsonObject obj = (JsonObject)key.Value;

					// Load biomes
					if (file == "biomes")
                    {
						foreach (JsonNameValuePair b in obj)
                        {
							Biome biome = new Biome();

							if (b.Value.ContainsKey("grass"))
								biome.grassColor = Util.HexToColor(b.Value["grass"]);

							if (b.Value.ContainsKey("foliage"))
								biome.foliageColor = Util.HexToColor(b.Value["foliage"]);

							if (b.Value.ContainsKey("water"))
								biome.waterColor = Util.HexToColor(b.Value["water"]);

							biomeMap["minecraft:" + b.Key] = biome;
                        }

						continue;
                    }

					Block.Preview preview = new Block.Preview(Color.Transparent, Color.Transparent, "");

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
					else
						preview.XZColor = preview.XYColor;

					if (!obj.ContainsKey("Y"))
						preview.XYColor = preview.XZColor;

					if (obj.ContainsKey("tint"))
						preview.tint = obj["tint"];

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

					if (curBlock.ContainsKey("waterlogged"))
						block.waterlogged = curBlock["waterlogged"];

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
		private void LoadLegacy(string filename)
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

				JsonObject biomeIds = new JsonObject(root["biome_ids"]);

				foreach (JsonNameValuePair b in biomeIds)
				{
					int id = Int32.Parse(b.Key);
					biomeIdMap[id] = "minecraft:" + b.Value;
				}

				JsonObject legacyBiomeIds = new JsonObject(root["legacy_biome_ids"]);

				foreach (JsonNameValuePair b in legacyBiomeIds)
				{
					int id = Int32.Parse(b.Key);
					legacyBiomeIdMap[id] = "minecraft:" + b.Value;
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

		/// <summary>Loads dimensions from the world filename.</summary>
		private void LoadDimensions(string filename)
        {
			cbxDimensions.Items.Clear();

			// Get dimensions
			cbxDimensions.Items.Add(new DimOption("overworld", GetText("overworld"), false, 0, ""));
			cbxDimensions.Items.Add(new DimOption("nether", GetText("nether"), false, -1, ""));
			cbxDimensions.Items.Add(new DimOption("end", GetText("end"), false, 1, ""));

			// Search for dimensions in world save
			if (Directory.Exists(filename + @"\dimensions")) // Open dimensions folder
				foreach (DirectoryInfo sd in new DirectoryInfo(filename + @"\dimensions").GetDirectories())
					foreach (DirectoryInfo datapackDim in new DirectoryInfo(sd.FullName).GetDirectories()) // Datapack folders
						cbxDimensions.Items.Add(new DimOption(datapackDim.Name, datapackDim.Name + " (" + sd.Name + ")", true, 0, sd.Name));
		}

		/// <summary>Loads the world from the combobox and resets the view.</summary>
		private void LoadWorld(string filename, bool refreshDimensions)
		{
			updateXYView = false;
			updateXZView = false;
			updateXYSel = false;
			updateXZSel = false;

			while (regionQueue.Count > 0)
				regionQueue.TryDequeue(out Region r);

			while (ChunkImageXYQueue.Count > 0)
				ChunkImageXYQueue.TryDequeue(out Chunk c);

			while (ChunkImageXZQueue.Count > 0)
				ChunkImageXZQueue.TryDequeue(out Chunk c);

			if (regionFileStream != null)
            {
				regionFileStream.Close();
				regionFileStream = null;
			}

			if (refreshDimensions)
			{
				int prevDim = cbxDimensions.SelectedIndex;
				LoadDimensions(filename);

				if (prevDim < cbxDimensions.Items.Count)
					cbxDimensions.SelectedIndex = prevDim; // Keep dimension index if we can
				else
					cbxDimensions.SelectedIndex = 0; // Set to overworld
			}

			if (world.Load(filename + @"\level.dat", dimensionSelected))
			{
				XYImageMidPos = new Point((int)world.playerPos.X, (int)world.playerPos.Y);
				XYImageZoom = 8;
				selectRegion.start = new Point3D<int>((int)world.playerPos.X - 10, (int)world.playerPos.Y - 10, (int)world.playerPos.Z - 10);
				selectRegion.end = new Point3D<int>((int)world.playerPos.X + 10, (int)world.playerPos.Y + 10, (int)world.playerPos.Z + 10);
				selectRegion.start.Z = Math.Max(Math.Min(selectRegion.start.Z, World.WORLD_HEIGHT_MAX), World.WORLD_HEIGHT_MIN);
				selectRegion.end.Z = Math.Max(Math.Min(selectRegion.end.Z, World.WORLD_HEIGHT_MAX), World.WORLD_HEIGHT_MIN);
				UpdateSizeLabel();
				btnDone.Enabled = true;
				XZImageMidPos = new Point(selectRegion.start.X + (selectRegion.end.X - selectRegion.start.X) / 2, selectRegion.start.Z + (selectRegion.end.Z - selectRegion.start.Z) / 2);

				updateXYView = true;
				updateXZView = true;
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
			// Add to queue
			if ((chunk.XYImage == null && !chunk.XYImageInQueue) || (chunk.XYImage != null && chunk.XYImageInQueue))
			{
				if (chunk.XYImageInQueue == false)
				{
					frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

					main.ChunkImageXYQueue.Enqueue(chunk);
					chunk.XYImageInQueue = true;
				}

				#region Check regions around chunk in case regions aren't loaded yet
				Region myRegion, edgeRegion;
				bool stopCheck = false;
				myRegion = world.GetRegion(chunk.X * 16, chunk.Y * 16, false);

				// Down
				edgeRegion = world.GetRegion(chunk.X * 16 - 1, chunk.Y * 16, false);
				if (edgeRegion != null && !edgeRegion.isLoaded && (myRegion != edgeRegion))
				{
					edgeRegion.edgeChunks.Add(chunk);
					stopCheck = true;
				}

				// Up
				if (!stopCheck)
				{
					edgeRegion = world.GetRegion(chunk.X * 16 + 1, chunk.Y * 16, false);
					if (edgeRegion != null && !edgeRegion.isLoaded && (myRegion != edgeRegion))
					{
						edgeRegion.edgeChunks.Add(chunk);
						stopCheck = true;
					}
				}

				// Left
				if (!stopCheck)
				{
					edgeRegion = world.GetRegion(chunk.X * 16, chunk.Y * 16 - 1, false);
					if (edgeRegion != null && !edgeRegion.isLoaded && (myRegion != edgeRegion))
					{
						edgeRegion.edgeChunks.Add(chunk);
						stopCheck = true;
					}
				}

				// Right
				if (!stopCheck)
				{
					edgeRegion = world.GetRegion(chunk.X * 16, chunk.Y * 16 + 1, false);
					if (edgeRegion != null && !edgeRegion.isLoaded && (myRegion != edgeRegion))
					{
						edgeRegion.edgeChunks.Add(chunk);
						stopCheck = true;
					}
				}
                #endregion

                return null;
			}

			if (chunk.XYImage != null)
				return chunk.XYImage.Image;

			// *Only draw on chunk drawing thread, do NOT draw on main thread*
			if (Thread.CurrentThread == UIThread || Thread.CurrentThread == regionQueueThread || Thread.CurrentThread == DrawEventThread)
				return null;

			chunk.XYImage = new FastBitmap(16, 16);
			chunk.XYImage.LockImage();

			for (int x = 0; x < 16; x++)
			{
				for (int y = 0; y < 16; y++)
				{
					Color finalColor = Color.Transparent;
					Color transparentColor = Color.Transparent;
					bool foundColor = false;

					for (int s = (World.WORLD_CHUNK_SECTIONS - 1); s >= 0 && !foundColor; s--)
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
							String blockTint = blockPreviewMap[blockPreviewKey].tint;
							if (blockColor != Color.Transparent)
							{
								double light = 0.0;

								// Shade
								if (s * 16 + z < World.WORLD_HEIGHT_MAX)
								{
									blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x - 1, chunk.Y * 16 + y, s * 16 + z + 1);
									if (blockPreviewKey != 0 && blockPreviewMap[blockPreviewKey].XYColor.A == 255)
										light -= .2;

									blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x, chunk.Y * 16 + y - 1, s * 16 + z + 1);
									if (blockPreviewKey != 0 && blockPreviewMap[blockPreviewKey].XYColor.A == 255)
										light -= .2;
								}

								// Highlight
								blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x - 1, chunk.Y * 16 + y, s * 16 + z);
								if (blockPreviewKey == 0)
									light += 0.1;

								if (s * 16 + z > World.WORLD_HEIGHT_MIN)
								{
									blockPreviewKey = world.GetBlockPreviewKey(chunk.X * 16 + x, chunk.Y * 16 + y - 1, s * 16 + z - 1);
									if (blockPreviewKey == 0)
										light += 0.1;
								}

								if (blockColor.A != 255)
									light *= 0.25;

								// Apply biome tint
								String blockBiome = "";
								if (chunk.use2Dbiomes)
									blockBiome = chunk.legacyBiomeId[x, y];
                                else
									blockBiome = section.biomeId[x / 4, y / 4, z / 4];

								if (blockBiome == null || !biomeMap.ContainsKey(blockBiome))
									blockBiome = "minecraft:plains";

								if (blockTint != "")
								{
									if (blockTint == "grass")
										blockColor = Util.ColorMul(blockColor, biomeMap[blockBiome].grassColor);
									else if (blockTint == "foliage")
										blockColor = Util.ColorMul(blockColor, biomeMap[blockBiome].foliageColor);
									else if (blockTint == "water")
										blockColor = Util.ColorMul(blockColor, biomeMap[blockBiome].waterColor);
								}

								blockColor = Util.ColorMul(blockColor, 1.0 + light);

								// Add to final result, cancel if alpha is full
								if (blockColor.A < 255)
								{
									transparentColor = Util.ColorAdd(blockColor, transparentColor);
									finalColor = transparentColor;
								}
								else
								{
									bool underwater = false;

									if (blockPreviewFileKeyMap.ContainsKey("water"))
									{
										underwater = (world.GetBlockPreviewKey(chunk.X * 16 + x, chunk.Y * 16 + y, s * 16 + z + 2) == blockPreviewFileKeyMap["water"]);

										if (underwater)
											transparentColor = Color.FromArgb(Math.Min((int)transparentColor.A, 190), transparentColor.R, transparentColor.G, transparentColor.B);
									}

									if (transparentColor.A == 0)
										finalColor = blockColor;
									else
										finalColor = Util.ColorAdd(blockColor, transparentColor);

									foundColor = true;
									break;
								}
							}
						}
					}

					if (chunk.XYImage == null)
						return null;

					chunk.XYImage.SetPixel(x, y, finalColor);
				}
			}

			if (chunk.XYImage == null)
				return null;

			chunk.XYImage.UnlockImage();

			return chunk.XYImage.Image;
		}

		/// <summary>Gets the XZ bitmap of a chunk. If none have been generated, create it.</summary>
		/// <param name="chunk">The current chunk.</param>
		private Bitmap GetChunkXZImage(Chunk chunk)
		{
			// Add to queue
			if ((chunk.XZImage == null && !chunk.XZImageInQueue) || (chunk.XZImage != null && chunk.XZImageInQueue))
			{
				if (chunk.XZImageInQueue == false)
				{
					frmImport main = ((frmImport)Application.OpenForms["frmImport"]);

					main.ChunkImageXZQueue.Enqueue(chunk);
					chunk.XZImageInQueue = true;
				}

				return null;
			}

			if (chunk.XZImage != null)
				return chunk.XZImage.Image;

			// *Only draw on chunk drawing thread, do NOT draw on main thread*
			if (Thread.CurrentThread == UIThread || Thread.CurrentThread == regionQueueThread || Thread.CurrentThread == DrawEventThread)
				return null;
			
			chunk.XZImage = new FastBitmap(16, World.WORLD_HEIGHT_SIZE);
			chunk.XZImage.LockImage();

			for (int x = 0; x < 16; x++)
			{
				for (int s = (World.WORLD_CHUNK_SECTIONS - 1); s >= 0; s--)
				{
					Chunk.Section section = chunk.sections[s];
					if (section == null)
						continue;

					for (int z = 15; z >= 0; z--)
					{
						Color finalColor = Color.Transparent;
						Color transparentColor = Color.Transparent;
						
						for (int y = 15; y >= 0; y--)
						{
							short blockPreviewKey = section.blockPreviewKey[x, y, z];
							if (blockPreviewKey == 0)
								continue;

							if (!section.IsBlockSaved(x, y, z))
								continue;

							Color blockColor = blockPreviewMap[blockPreviewKey].XZColor;
							String blockTint = blockPreviewMap[blockPreviewKey].tint;

							// Apply biome tint
							String blockBiome = "";
							if (chunk.use2Dbiomes)
								blockBiome = chunk.legacyBiomeId[x, y];
							else
								blockBiome = section.biomeId[x / 4, y / 4, z / 4];

							if (blockBiome == null || !biomeMap.ContainsKey(blockBiome))
								blockBiome = "minecraft:plains";

							if (blockTint != "")
							{
								if (blockTint == "grass")
									blockColor = Util.ColorMul(blockColor, biomeMap[blockBiome].grassColor);
								else if (blockTint == "foliage")
									blockColor = Util.ColorMul(blockColor, biomeMap[blockBiome].foliageColor);
								else if (blockTint == "water")
									blockColor = Util.ColorMul(blockColor, biomeMap[blockBiome].waterColor);
							}

							if (blockColor != Color.Transparent)
							{
								// Add to final result, cancel if alpha is full
								if (blockColor.A < 255)
								{
									transparentColor = Util.ColorAdd(blockColor, transparentColor);
									finalColor = transparentColor;

									if (y == 0 && blockPreviewFileKeyMap.ContainsKey("water"))
									{
										if (world.GetBlockPreviewKey(chunk.X * 16 + x, chunk.Y * 16 + y, s * 16 + z) == blockPreviewFileKeyMap["water"])
											finalColor = Color.FromArgb(Math.Min((int)finalColor.A, 128), finalColor.R, finalColor.G, finalColor.B);
									}
								}
								else
								{
									bool underwater = false;

									if (blockPreviewFileKeyMap.ContainsKey("water"))
									{
										underwater = (world.GetBlockPreviewKey(chunk.X * 16 + x, chunk.Y * 16 + y + 1, s * 16 + z) == blockPreviewFileKeyMap["water"]);

										if (underwater)
											transparentColor = Color.FromArgb(Math.Min((int)transparentColor.A, 190), transparentColor.R, transparentColor.G, transparentColor.B);
									}

									blockColor = Util.ColorBrighter(blockColor, (int)(-60.0f * (1.0f - (float)y / 15.0f)));

									if (transparentColor.A == 0)
										finalColor = blockColor;
									else
										finalColor =  Util.ColorAdd(blockColor, transparentColor);

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
			updateXYView = true;
			updateXZView = true;
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
				for (int dx = 0; dx < XYBlocksWidth + 16; dx += 16)
				{
					for (int dy = 0; dy < XYBlocksHeight + 16; dy += 16)
					{
						Chunk chunk = world.GetChunk(XYStart.X + dx, XYStart.Y + dy);
						if (chunk != null)
						{
							var img = GetChunkXYImage(chunk);

							if (img != null)
								g.DrawImage(img, chunk.X * 16 - XYStart.X, chunk.Y * 16 - XYStart.Y);
						}
					}
				}
			}

			XYMapBitmap.Dispose();
			XYMapBitmap = bmp;
			updateXYSel = true;
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
			Bitmap bmp;
			
			if (XYImageZoom == 1)
				bmp = new Bitmap(XYMapBitmap);
			else
				bmp = Util.ResizeBitmap(XYMapBitmap, (int)(XYBlocksWidth * XYImageZoom), (int)(XYBlocksHeight * XYImageZoom));

			using (Graphics g = Graphics.FromImage(bmp))
			{
				g.DrawImage(spawnImage, (int)((world.spawnPos.X - XYStart.X) * XYImageZoom) - 8, (int)((world.spawnPos.Y - XYStart.Y) * XYImageZoom) - 8);
				g.DrawImage(playerImage, (int)(((int)world.playerPos.X - XYStart.X) * XYImageZoom) - 8, (int)(((int)world.playerPos.Y - XYStart.Y) * XYImageZoom) - 8);
				g.DrawImage(XYSelectBitmap, 0, 0);
			}

			try
			{
				frmImport main = ((frmImport)Application.OpenForms["frmImport"]);
				main.pboxWorldXY.Invoke((MethodInvoker)(() =>
				{
					if (pboxWorldXY.Image != null)
						pboxWorldXY.Image.Dispose();

					pboxWorldXY.Image = bmp;
				}));
			}
			catch (Exception e)
			{
				Thread.CurrentThread.Abort();
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
			Bitmap bmp = new Bitmap(XZBlocksWidth, World.WORLD_HEIGHT_SIZE);

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

							if (img != null)
								g.DrawImage(img, chunk.X * 16 - XZStart.X, World.WORLD_HEIGHT_SIZE - img.Height);
						}
					}
				}
			}

			try
			{
				frmImport main = ((frmImport)Application.OpenForms["frmImport"]);
				main.pboxWorldXY.Invoke((MethodInvoker)(() =>
				{
					XZMapBitmap.Dispose();
					XZMapBitmap = bmp;
				}));
			}
			catch (Exception e)
			{
				Thread.CurrentThread.Abort();
			}

			updateXZSel = true;
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
			Bitmap map = new Bitmap(pboxWorldXZ.Width, pboxWorldXZ.Height);

			using (Graphics g = Graphics.FromImage(map))
			{
				Bitmap chunkMap;
				if (XZImageZoom == 1)
					chunkMap = XZMapBitmap;
				else
					chunkMap = Util.ResizeBitmap(XZMapBitmap, (int)(XZMapBitmap.Width * XZImageZoom), (int)(World.WORLD_HEIGHT_SIZE * XZImageZoom));

				int yoff = XZImageMidPos.Y - 128;
				g.DrawImage(chunkMap, 0, pboxWorldXZ.Height / 2 - chunkMap.Height / 2 + yoff * XZImageZoom);
				g.DrawImage(XZSelectBitmap, 0, 0);
				g.DrawImage(spawnImage, (int)((world.spawnPos.X - XZStart.X) * XZImageZoom) - 8, pboxWorldXZ.Height - (int)((world.spawnPos.Z - XZStart.Y) * XZImageZoom) - 8);
				g.DrawImage(playerImage, (int)((world.playerPos.X - XZStart.X) * XZImageZoom) - 8, pboxWorldXZ.Height - (int)((world.playerPos.Z - XZStart.Y) * XZImageZoom) - 8);
			}

			try
			{
				frmImport main = ((frmImport)Application.OpenForms["frmImport"]);
				main.pboxWorldXY.Invoke((MethodInvoker)(() =>
				{
					if (pboxWorldXZ.Image != null)
						pboxWorldXZ.Image.Dispose();

					pboxWorldXZ.Image = map;
				}));
			}
			catch (Exception e)
            {
				Thread.CurrentThread.Abort();
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
					if (prevpos != newpos)
					{
						updateXYView = true;
						updateXYViewMoveX = (prevpos.X - newpos.X);
						updateXYViewMoveY = (prevpos.Y - newpos.Y);
					}
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

					if (dy != 0 && (Util.IntDiv(endprevpos.Y, 16) != Util.IntDiv(selectRegion.end.Y, 16)))
						updateXZView = true;

					UpdateSizeLabel();
					Point startnewpos = new Point(selectRegion.start.X, selectRegion.start.Y);
					Point endnewpos = new Point(selectRegion.end.X, selectRegion.end.Y);
					if (startprevpos != startnewpos || endprevpos != endnewpos)
						updateXYSel = true;
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

        private void cbxDimensions_SelectedIndexChanged(object sender, EventArgs e)
        {
			if (cbxDimensions.SelectedIndex == -1)
				return;

			if (world.filename == "")
				return;

			dimensionSelected.copy(((DimOption)cbxDimensions.Items[cbxDimensions.SelectedIndex]));
			LoadWorld(Path.GetDirectoryName(world.filename), false);
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
			updateXYSel = true;
			updateXZView = true;
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
				LoadWorld(Path.GetDirectoryName(open.FileName), true);
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

			updateXYView = true;
		}

        private void ResizeXY(object sender, EventArgs e)
		{
			updateXYView = true;
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
						updateXZView = true;
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

					selectRegion.start.Z = Math.Max(Math.Min(selectRegion.start.Z, World.WORLD_HEIGHT_MAX), World.WORLD_HEIGHT_MIN);
					selectRegion.end.Z = Math.Max(Math.Min(selectRegion.end.Z, World.WORLD_HEIGHT_MAX), World.WORLD_HEIGHT_MIN);
					UpdateSizeLabel();
					Point startnewpos = new Point(selectRegion.start.X, selectRegion.start.Z);
					Point endnewpos = new Point(selectRegion.end.X, selectRegion.end.Z);
					if (startprevpos != startnewpos || endprevpos != endnewpos)
						updateXZSel = true;
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
					selectRegion.start.Z = Math.Max(Math.Min(selectRegion.start.Z, World.WORLD_HEIGHT_MAX), World.WORLD_HEIGHT_MIN);
					selectRegion.end.Z = Math.Max(Math.Min(selectRegion.end.Z, World.WORLD_HEIGHT_MAX), World.WORLD_HEIGHT_MIN);
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
			updateXYSel = true;
			updateXZSel = true;
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

			updateXZView = true;
		}

		private void ResizeXZ(object sender, EventArgs e)
		{
			updateXZView = true;
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

		private void cbxSaves_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (cbxSaves.SelectedIndex == -1)
				return;

			LoadWorld(((WorldOption)cbxSaves.Items[cbxSaves.SelectedIndex]).filename, true);
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