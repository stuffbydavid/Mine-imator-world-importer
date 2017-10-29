using System;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.Web.Script.Serialization;

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
		JavaScriptSerializer serializer = new JavaScriptSerializer();

		public frmFilters(Form callingForm)
		{
			import = callingForm as frmImport;
			InitializeComponent();
		}

		private void frmFilters_Load(object sender, EventArgs e)
		{
			// Set texts
			Text = import.GetText("filterstitle");
			cbxActivate.Text = import.GetText("filtersactivate");
			lblBlocksRemove.Text = import.GetText("filtersblocksremove");
			rbtnRemove.Text = import.GetText("filtersremove");
			rbtnKeep.Text = import.GetText("filterskeep");
			rbtnKeep.Location = new Point(rbtnRemove.Location.X + rbtnRemove.Width + 20, rbtnKeep.Location.Y);
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
			string json = "{\n";
			json += "\t\"active\": " + (cbxActivate.Checked ? "true" : "false") + ",\n";
			json += "\t\"invert\": " + (rbtnKeep.Checked ? "true" : "false") + ",\n";
			json += "\t\"blocks\": [\n";
			for (int i = 0; i < import.filterBlocks.Count; i++)
				json += "\t\t\"" + import.filterBlocks[i] + "\"" + (i < import.filterBlocks.Count - 1 ? "," : "" ) + "\n";
			json += "\t]\n";
			json += "}";

			File.WriteAllText(frmImport.miBlockFilterFile, json);

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
