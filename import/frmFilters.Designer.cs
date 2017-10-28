namespace import {
	partial class frmFilters {
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing) {
			if (disposing && (components != null)) {
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent() {
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmFilters));
			this.btnOk = new System.Windows.Forms.Button();
			this.cbxBlocks = new System.Windows.Forms.ComboBox();
			this.btnAdd = new System.Windows.Forms.Button();
			this.lbxFilters = new System.Windows.Forms.ListBox();
			this.btnRemove = new System.Windows.Forms.Button();
			this.lblBlocksRemove = new System.Windows.Forms.Label();
			this.cbxActivate = new System.Windows.Forms.CheckBox();
			this.panFilters = new System.Windows.Forms.Panel();
			this.rbtnKeep = new System.Windows.Forms.RadioButton();
			this.rbtnRemove = new System.Windows.Forms.RadioButton();
			this.panFilters.SuspendLayout();
			this.SuspendLayout();
			// 
			// btnOk
			// 
			this.btnOk.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.btnOk.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnOk.ForeColor = System.Drawing.Color.Black;
			this.btnOk.Location = new System.Drawing.Point(152, 365);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(118, 31);
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "ok";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// cbxBlocks
			// 
			this.cbxBlocks.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cbxBlocks.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
			this.cbxBlocks.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
			this.cbxBlocks.FormattingEnabled = true;
			this.cbxBlocks.Location = new System.Drawing.Point(9, 24);
			this.cbxBlocks.Name = "cbxBlocks";
			this.cbxBlocks.Size = new System.Drawing.Size(216, 21);
			this.cbxBlocks.TabIndex = 5;
			this.cbxBlocks.SelectedIndexChanged += new System.EventHandler(this.cbxBlocks_SelectedIndexChanged);
			// 
			// btnAdd
			// 
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAdd.Enabled = false;
			this.btnAdd.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnAdd.ForeColor = System.Drawing.Color.Black;
			this.btnAdd.Location = new System.Drawing.Point(239, 24);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(74, 22);
			this.btnAdd.TabIndex = 6;
			this.btnAdd.Text = "add";
			this.btnAdd.UseVisualStyleBackColor = true;
			this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
			// 
			// lbxFilters
			// 
			this.lbxFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lbxFilters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.lbxFilters.FormattingEnabled = true;
			this.lbxFilters.Location = new System.Drawing.Point(9, 51);
			this.lbxFilters.Name = "lbxFilters";
			this.lbxFilters.Size = new System.Drawing.Size(384, 238);
			this.lbxFilters.TabIndex = 7;
			this.lbxFilters.SelectedIndexChanged += new System.EventHandler(this.lbxFilters_SelectedIndexChanged);
			// 
			// btnRemove
			// 
			this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRemove.Enabled = false;
			this.btnRemove.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnRemove.ForeColor = System.Drawing.Color.Black;
			this.btnRemove.Location = new System.Drawing.Point(319, 24);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(74, 22);
			this.btnRemove.TabIndex = 8;
			this.btnRemove.Text = "remove";
			this.btnRemove.UseVisualStyleBackColor = true;
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// lblBlocksRemove
			// 
			this.lblBlocksRemove.AutoSize = true;
			this.lblBlocksRemove.BackColor = System.Drawing.Color.Transparent;
			this.lblBlocksRemove.ForeColor = System.Drawing.Color.Black;
			this.lblBlocksRemove.Location = new System.Drawing.Point(9, 8);
			this.lblBlocksRemove.Name = "lblBlocksRemove";
			this.lblBlocksRemove.Size = new System.Drawing.Size(73, 13);
			this.lblBlocksRemove.TabIndex = 9;
			this.lblBlocksRemove.Text = "blocksremove";
			// 
			// cbxActivate
			// 
			this.cbxActivate.AutoSize = true;
			this.cbxActivate.BackColor = System.Drawing.Color.Transparent;
			this.cbxActivate.Checked = true;
			this.cbxActivate.CheckState = System.Windows.Forms.CheckState.Checked;
			this.cbxActivate.ForeColor = System.Drawing.Color.Black;
			this.cbxActivate.Location = new System.Drawing.Point(12, 12);
			this.cbxActivate.Name = "cbxActivate";
			this.cbxActivate.Size = new System.Drawing.Size(64, 17);
			this.cbxActivate.TabIndex = 10;
			this.cbxActivate.Text = "activate";
			this.cbxActivate.UseVisualStyleBackColor = false;
			this.cbxActivate.CheckedChanged += new System.EventHandler(this.cbxActivate_CheckedChanged);
			// 
			// panFilters
			// 
			this.panFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panFilters.Controls.Add(this.rbtnKeep);
			this.panFilters.Controls.Add(this.lblBlocksRemove);
			this.panFilters.Controls.Add(this.rbtnRemove);
			this.panFilters.Controls.Add(this.cbxBlocks);
			this.panFilters.Controls.Add(this.btnRemove);
			this.panFilters.Controls.Add(this.btnAdd);
			this.panFilters.Controls.Add(this.lbxFilters);
			this.panFilters.Location = new System.Drawing.Point(12, 35);
			this.panFilters.Name = "panFilters";
			this.panFilters.Size = new System.Drawing.Size(404, 324);
			this.panFilters.TabIndex = 11;
			// 
			// rbtnKeep
			// 
			this.rbtnKeep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.rbtnKeep.AutoSize = true;
			this.rbtnKeep.ForeColor = System.Drawing.Color.Black;
			this.rbtnKeep.Location = new System.Drawing.Point(117, 295);
			this.rbtnKeep.Name = "rbtnKeep";
			this.rbtnKeep.Size = new System.Drawing.Size(49, 17);
			this.rbtnKeep.TabIndex = 13;
			this.rbtnKeep.Text = "keep";
			this.rbtnKeep.UseVisualStyleBackColor = true;
			this.rbtnKeep.CheckedChanged += new System.EventHandler(this.rbtnKeep_CheckedChanged);
			// 
			// rbtnRemove
			// 
			this.rbtnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.rbtnRemove.AutoSize = true;
			this.rbtnRemove.Checked = true;
			this.rbtnRemove.ForeColor = System.Drawing.Color.Black;
			this.rbtnRemove.Location = new System.Drawing.Point(9, 295);
			this.rbtnRemove.Name = "rbtnRemove";
			this.rbtnRemove.Size = new System.Drawing.Size(60, 17);
			this.rbtnRemove.TabIndex = 12;
			this.rbtnRemove.TabStop = true;
			this.rbtnRemove.Text = "remove";
			this.rbtnRemove.UseVisualStyleBackColor = true;
			this.rbtnRemove.CheckedChanged += new System.EventHandler(this.rbtnRemove_CheckedChanged);
			// 
			// frmFilters
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.ClientSize = new System.Drawing.Size(428, 408);
			this.Controls.Add(this.panFilters);
			this.Controls.Add(this.cbxActivate);
			this.Controls.Add(this.btnOk);
			this.ForeColor = System.Drawing.Color.DimGray;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmFilters";
			this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
			this.Text = "title";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmFilters_FormClosing);
			this.panFilters.ResumeLayout(false);
			this.panFilters.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.ComboBox cbxBlocks;
		private System.Windows.Forms.Button btnAdd;
		private System.Windows.Forms.ListBox lbxFilters;
		private System.Windows.Forms.Button btnRemove;
		private System.Windows.Forms.Label lblBlocksRemove;
		private System.Windows.Forms.CheckBox cbxActivate;
		private System.Windows.Forms.Panel panFilters;
		private System.Windows.Forms.RadioButton rbtnRemove;
		private System.Windows.Forms.RadioButton rbtnKeep;
	}
}