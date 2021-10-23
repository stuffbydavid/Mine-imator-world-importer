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
            this.lblBlocksToRemove = new System.Windows.Forms.Label();
            this.cbxActivate = new System.Windows.Forms.CheckBox();
            this.panFilters = new System.Windows.Forms.Panel();
            this.rbtnKeepFiltered = new System.Windows.Forms.RadioButton();
            this.rbtnRemoveBlocks = new System.Windows.Forms.RadioButton();
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
            this.lbxFilters.Size = new System.Drawing.Size(256, 238);
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
            // lblBlocksToRemove
            // 
            this.lblBlocksToRemove.AutoSize = true;
            this.lblBlocksToRemove.BackColor = System.Drawing.Color.Transparent;
            this.lblBlocksToRemove.ForeColor = System.Drawing.Color.Black;
            this.lblBlocksToRemove.Location = new System.Drawing.Point(9, 8);
            this.lblBlocksToRemove.Name = "lblBlocksToRemove";
            this.lblBlocksToRemove.Size = new System.Drawing.Size(82, 13);
            this.lblBlocksToRemove.TabIndex = 9;
            this.lblBlocksToRemove.Text = "blockstoremove";
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
            this.panFilters.Controls.Add(this.rbtnKeepFiltered);
            this.panFilters.Controls.Add(this.lblBlocksToRemove);
            this.panFilters.Controls.Add(this.rbtnRemoveBlocks);
            this.panFilters.Controls.Add(this.cbxBlocks);
            this.panFilters.Controls.Add(this.btnRemove);
            this.panFilters.Controls.Add(this.btnAdd);
            this.panFilters.Controls.Add(this.lbxFilters);
            this.panFilters.Location = new System.Drawing.Point(12, 35);
            this.panFilters.Name = "panFilters";
            this.panFilters.Size = new System.Drawing.Size(404, 324);
            this.panFilters.TabIndex = 11;
            // 
            // rbtnKeepFiltered
            // 
            this.rbtnKeepFiltered.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbtnKeepFiltered.AutoSize = true;
            this.rbtnKeepFiltered.ForeColor = System.Drawing.Color.Black;
            this.rbtnKeepFiltered.Location = new System.Drawing.Point(117, 295);
            this.rbtnKeepFiltered.Name = "rbtnKeepFiltered";
            this.rbtnKeepFiltered.Size = new System.Drawing.Size(80, 17);
            this.rbtnKeepFiltered.TabIndex = 13;
            this.rbtnKeepFiltered.TabStop = true;
            this.rbtnKeepFiltered.Text = "keepfiltered";
            this.rbtnKeepFiltered.UseVisualStyleBackColor = true;
            this.rbtnKeepFiltered.CheckedChanged += new System.EventHandler(this.rbtnKeepFiltered_CheckedChanged);
            // 
            // rbtnRemoveBlocks
            // 
            this.rbtnRemoveBlocks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.rbtnRemoveBlocks.AutoSize = true;
            this.rbtnRemoveBlocks.Checked = true;
            this.rbtnRemoveBlocks.ForeColor = System.Drawing.Color.Black;
            this.rbtnRemoveBlocks.Location = new System.Drawing.Point(9, 295);
            this.rbtnRemoveBlocks.Name = "rbtnRemoveBlocks";
            this.rbtnRemoveBlocks.Size = new System.Drawing.Size(91, 17);
            this.rbtnRemoveBlocks.TabIndex = 12;
            this.rbtnRemoveBlocks.TabStop = true;
            this.rbtnRemoveBlocks.Text = "removefiltered";
            this.rbtnRemoveBlocks.UseVisualStyleBackColor = true;
            this.rbtnRemoveBlocks.CheckedChanged += new System.EventHandler(this.rbtnRemoveFiltered_CheckedChanged);
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
            this.Load += new System.EventHandler(this.frmFilters_Load);
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
		private System.Windows.Forms.Label lblBlocksToRemove;
		private System.Windows.Forms.CheckBox cbxActivate;
		private System.Windows.Forms.Panel panFilters;
		private System.Windows.Forms.RadioButton rbtnRemoveBlocks;
		private System.Windows.Forms.RadioButton rbtnKeepFiltered;
	}
}