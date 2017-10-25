using System;
using System.Windows.Forms;
using System.IO;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace import
{
	public partial class frmFilters : Form
	{
		// Represents a chooseable block variation
		class BlockOption
		{
			public string name, displayName;

			public BlockOption(string name, string displayName)
			{
				this.name = name;
				this.displayName = displayName;
			}

			public override string ToString()
			{
				return displayName;
			}
		}

		frmImport main;

		public frmFilters(Form callingForm)
		{
			main = callingForm as frmImport;
			InitializeComponent();
		}

		private void frmFilters_Load(object sender, EventArgs e)
		{
			/*// Load block variants and store in combobox and listbox
			foreach (KeyValuePair<IdDataPair, Block> pair in main.blockMap)
			{ 
				Block block = pair.Value;
				BlockOption option = new BlockOption(block.name, block.displayName);
				cbxBlocks.Items.Add(option);
				if (main.filterBlocks.Contains(block.name))
					lbxFilters.Items.Add(option);
			}

			panFilters.Enabled = cbxActivate.Checked;*/
		}

		private void cbxActivate_CheckedChanged(object sender, EventArgs e)
		{
			panFilters.Enabled = cbxActivate.Checked;
			main.filterBlocksActive = cbxActivate.Checked;
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			if (cbxBlocks.SelectedIndex == -1 || cbxBlocks.Text == "")
				return;

			// Already added in list
			BlockOption option = (BlockOption)cbxBlocks.SelectedValue;
			if (main.filterBlocks.Contains(option.name))
				return;

			main.filterBlocks.Add(option.name);
			lbxFilters.Items.Add(option.displayName);
		}


		private void btnRemove_Click(object sender, EventArgs e)
		{
			BlockOption option = (BlockOption)lbxFilters.SelectedValue;
			main.filterBlocks.Remove(option.name);
			lbxFilters.Items.Remove(option);
		}


		private void btnOk_Click(object sender, EventArgs e)
		{
			this.Close();
		}

		private void frmFilters_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Save filtered blocks to JSON
			Dictionary<string, dynamic> root = new Dictionary<string, dynamic>();
			root["active"] = cbxActivate.Checked;
			root["invert"] = main.filterBlocksInvert;
			root["blocks"] = main.filterBlocks;
			File.WriteAllText("blocks.mifilter", JsonConvert.SerializeObject(root, Formatting.Indented).Replace("  ", "\t"));
			main.UpdateFilterBlocks();
		}
	}
}
