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

		frmImport import;

		public frmFilters(Form callingForm)
		{
			import = callingForm as frmImport;

			InitializeComponent();

			// Set texts
			Text = import.GetText("filterstitle");
			cbxActivate.Text = import.GetText("filtersactivate");
			lblBlocksRemove.Text = import.GetText("filtersblocksremove");
			rbtnRemove.Text = import.GetText("filtersremove");
			rbtnKeep.Text = import.GetText("filterskeep");
			btnAdd.Text = import.GetText("filtersadd");
			btnRemove.Text = import.GetText("filtersremove");
			btnOk.Text = import.GetText("filtersok");

			LoadFilters();
		}

		private void LoadFilters()
		{
			// Load block variants and store in combobox and listbox
			foreach (KeyValuePair<string, frmImport.Block> pair in import.blockNameMap)
			{ 
				BlockOption option = new BlockOption(pair.Key, pair.Value.displayName);
				cbxBlocks.Items.Add(option);
				if (import.filterBlocks.Contains(pair.Key))
					lbxFilters.Items.Add(option);
			}

			// Settings
			cbxActivate.Checked = import.filterBlocksActive;
			rbtnRemove.Checked = !import.filterBlocksInvert;
			rbtnKeep.Checked = import.filterBlocksInvert;
			panFilters.Enabled = cbxActivate.Checked;
		}

		private void cbxActivate_CheckedChanged(object sender, EventArgs e)
		{
			panFilters.Enabled = cbxActivate.Checked;
		}

		private void btnAdd_Click(object sender, EventArgs e)
		{
			if (cbxBlocks.SelectedIndex == -1 || cbxBlocks.Text == "")
				return;

			// Already added in list
			BlockOption option = (BlockOption)cbxBlocks.SelectedItem;
			if (import.filterBlocks.Contains(option.name))
				return;

			import.filterBlocks.Add(option.name);
			lbxFilters.Items.Add(option);
		}

		private void btnRemove_Click(object sender, EventArgs e)
		{
			BlockOption option = (BlockOption)lbxFilters.SelectedItem;
			import.filterBlocks.Remove(option.name);
			lbxFilters.Items.Remove(option);
			btnRemove.Enabled = false;
		}


		private void btnOk_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void frmFilters_FormClosing(object sender, FormClosingEventArgs e)
		{
			// Save filtered blocks to JSON
			Dictionary<string, dynamic> root = new Dictionary<string, dynamic>();
			root["active"] = cbxActivate.Checked;
			root["invert"] = rbtnKeep.Checked;
			root["blocks"] = import.filterBlocks;
			File.WriteAllText(frmImport.miBlockFilterFile, JsonConvert.SerializeObject(root, Formatting.Indented).Replace("  ", "\t"));

			import.filterBlocksActive = cbxActivate.Checked;
			import.filterBlocksInvert = rbtnKeep.Checked;
			import.UpdateFilterBlocks();
		}

		private void rbtnKeep_CheckedChanged(object sender, EventArgs e)
		{
			lblBlocksRemove.Text = import.GetText("filtersblockskeep");
		}

		private void rbtnRemove_CheckedChanged(object sender, EventArgs e)
		{
			lblBlocksRemove.Text = import.GetText("filtersblocksremove");
		}

		private void cbxBlocks_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnAdd.Enabled = (cbxBlocks.SelectedItem != null);
		}

		private void lbxFilters_SelectedIndexChanged(object sender, EventArgs e)
		{
			btnRemove.Enabled = (lbxFilters.SelectedItem != null);
		}
	}
}
