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
			this.label1 = new System.Windows.Forms.Label();
			this.cbxActivate = new System.Windows.Forms.CheckBox();
			this.panFilters = new System.Windows.Forms.Panel();
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
			this.btnOk.Text = "OK";
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
			// 
			// btnAdd
			// 
			this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnAdd.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnAdd.ForeColor = System.Drawing.Color.Black;
			this.btnAdd.Location = new System.Drawing.Point(239, 24);
			this.btnAdd.Name = "btnAdd";
			this.btnAdd.Size = new System.Drawing.Size(74, 22);
			this.btnAdd.TabIndex = 6;
			this.btnAdd.Text = "Add";
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
			this.lbxFilters.Size = new System.Drawing.Size(384, 264);
			this.lbxFilters.TabIndex = 7;
			// 
			// btnRemove
			// 
			this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.btnRemove.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnRemove.ForeColor = System.Drawing.Color.Black;
			this.btnRemove.Location = new System.Drawing.Point(319, 24);
			this.btnRemove.Name = "btnRemove";
			this.btnRemove.Size = new System.Drawing.Size(74, 22);
			this.btnRemove.TabIndex = 8;
			this.btnRemove.Text = "Remove";
			this.btnRemove.UseVisualStyleBackColor = true;
			this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.BackColor = System.Drawing.Color.Transparent;
			this.label1.ForeColor = System.Drawing.Color.Black;
			this.label1.Location = new System.Drawing.Point(9, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(216, 13);
			this.label1.TabIndex = 9;
			this.label1.Text = "Choose blocks to remove from the selection.";
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
			this.cbxActivate.Size = new System.Drawing.Size(92, 17);
			this.cbxActivate.TabIndex = 10;
			this.cbxActivate.Text = "Activate filters";
			this.cbxActivate.UseVisualStyleBackColor = false;
			this.cbxActivate.CheckedChanged += new System.EventHandler(this.cbxActivate_CheckedChanged);
			// 
			// panFilters
			// 
			this.panFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.panFilters.Controls.Add(this.label1);
			this.panFilters.Controls.Add(this.cbxBlocks);
			this.panFilters.Controls.Add(this.btnRemove);
			this.panFilters.Controls.Add(this.btnAdd);
			this.panFilters.Controls.Add(this.lbxFilters);
			this.panFilters.Location = new System.Drawing.Point(12, 35);
			this.panFilters.Name = "panFilters";
			this.panFilters.Size = new System.Drawing.Size(404, 324);
			this.panFilters.TabIndex = 11;
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
			this.Text = "Filters";
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
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.CheckBox cbxActivate;
		private System.Windows.Forms.Panel panFilters;

	}
}