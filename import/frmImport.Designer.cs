namespace import
{
	partial class frmImport
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmImport));
			this.lblWorld = new System.Windows.Forms.Label();
			this.cbxSaves = new System.Windows.Forms.ComboBox();
			this.btnDone = new System.Windows.Forms.Button();
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.lblTopDownView = new System.Windows.Forms.Label();
			this.panXY = new System.Windows.Forms.Panel();
			this.pboxWorldXY = new System.Windows.Forms.PictureBox();
			this.lblFilterInfo = new System.Windows.Forms.Label();
			this.lblCrossSectionView = new System.Windows.Forms.Label();
			this.btnFilters = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.lblSelSize = new System.Windows.Forms.Label();
			this.rbtEnd = new System.Windows.Forms.RadioButton();
			this.rbtNether = new System.Windows.Forms.RadioButton();
			this.rbtOver = new System.Windows.Forms.RadioButton();
			this.panYZ = new System.Windows.Forms.Panel();
			this.pboxWorldXZ = new System.Windows.Forms.PictureBox();
			this.lblInfo = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.panXY.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pboxWorldXY)).BeginInit();
			this.panYZ.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pboxWorldXZ)).BeginInit();
			this.SuspendLayout();
			// 
			// lblWorld
			// 
			this.lblWorld.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.lblWorld.AutoSize = true;
			this.lblWorld.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblWorld.Location = new System.Drawing.Point(4, 6);
			this.lblWorld.Name = "lblWorld";
			this.lblWorld.Size = new System.Drawing.Size(33, 13);
			this.lblWorld.TabIndex = 1;
			this.lblWorld.Text = "world";
			// 
			// cbxSaves
			// 
			this.cbxSaves.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.cbxSaves.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbxSaves.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.cbxSaves.FormattingEnabled = true;
			this.cbxSaves.Location = new System.Drawing.Point(48, 3);
			this.cbxSaves.Name = "cbxSaves";
			this.cbxSaves.Size = new System.Drawing.Size(309, 21);
			this.cbxSaves.TabIndex = 2;
			this.cbxSaves.SelectedIndexChanged += new System.EventHandler(this.cbxSaves_SelectedIndexChanged);
			// 
			// btnDone
			// 
			this.btnDone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.btnDone.Enabled = false;
			this.btnDone.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnDone.Location = new System.Drawing.Point(0, 489);
			this.btnDone.Name = "btnDone";
			this.btnDone.Size = new System.Drawing.Size(179, 65);
			this.btnDone.TabIndex = 3;
			this.btnDone.Text = "done";
			this.btnDone.UseVisualStyleBackColor = true;
			this.btnDone.Click += new System.EventHandler(this.btnDone_Click);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.splitContainer1.Location = new System.Drawing.Point(2, 25);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.lblTopDownView);
			this.splitContainer1.Panel1.Controls.Add(this.panXY);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.lblFilterInfo);
			this.splitContainer1.Panel2.Controls.Add(this.lblCrossSectionView);
			this.splitContainer1.Panel2.Controls.Add(this.btnFilters);
			this.splitContainer1.Panel2.Controls.Add(this.btnCancel);
			this.splitContainer1.Panel2.Controls.Add(this.lblSelSize);
			this.splitContainer1.Panel2.Controls.Add(this.rbtEnd);
			this.splitContainer1.Panel2.Controls.Add(this.rbtNether);
			this.splitContainer1.Panel2.Controls.Add(this.rbtOver);
			this.splitContainer1.Panel2.Controls.Add(this.btnDone);
			this.splitContainer1.Panel2.Controls.Add(this.lblWorld);
			this.splitContainer1.Panel2.Controls.Add(this.panYZ);
			this.splitContainer1.Panel2.Controls.Add(this.cbxSaves);
			this.splitContainer1.Size = new System.Drawing.Size(1001, 559);
			this.splitContainer1.SplitterDistance = 635;
			this.splitContainer1.TabIndex = 6;
			// 
			// lblTopDownView
			// 
			this.lblTopDownView.AutoSize = true;
			this.lblTopDownView.Location = new System.Drawing.Point(1, 1);
			this.lblTopDownView.Name = "lblTopDownView";
			this.lblTopDownView.Size = new System.Drawing.Size(70, 13);
			this.lblTopDownView.TabIndex = 1;
			this.lblTopDownView.Text = "topdownview";
			// 
			// panXY
			// 
			this.panXY.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panXY.BackgroundImage = global::import.Properties.Resources.transparentgrid;
			this.panXY.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panXY.Controls.Add(this.pboxWorldXY);
			this.panXY.Location = new System.Drawing.Point(3, 17);
			this.panXY.Name = "panXY";
			this.panXY.Size = new System.Drawing.Size(633, 539);
			this.panXY.TabIndex = 0;
			// 
			// pboxWorldXY
			// 
			this.pboxWorldXY.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pboxWorldXY.BackColor = System.Drawing.Color.Transparent;
			this.pboxWorldXY.Location = new System.Drawing.Point(0, 0);
			this.pboxWorldXY.Name = "pboxWorldXY";
			this.pboxWorldXY.Size = new System.Drawing.Size(629, 535);
			this.pboxWorldXY.TabIndex = 0;
			this.pboxWorldXY.TabStop = false;
			this.pboxWorldXY.SizeChanged += new System.EventHandler(this.ResizeXY);
			this.pboxWorldXY.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveXYStart);
			this.pboxWorldXY.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OverXY);
			this.pboxWorldXY.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MoveXYEnd);
			this.pboxWorldXY.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.ZoomXY);
			// 
			// lblFilterInfo
			// 
			this.lblFilterInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lblFilterInfo.AutoSize = true;
			this.lblFilterInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblFilterInfo.Location = new System.Drawing.Point(183, 513);
			this.lblFilterInfo.Name = "lblFilterInfo";
			this.lblFilterInfo.Size = new System.Drawing.Size(63, 13);
			this.lblFilterInfo.TabIndex = 12;
			this.lblFilterInfo.Text = "filtersalert";
			// 
			// lblCrossSectionView
			// 
			this.lblCrossSectionView.AutoSize = true;
			this.lblCrossSectionView.Location = new System.Drawing.Point(-1, 50);
			this.lblCrossSectionView.Name = "lblCrossSectionView";
			this.lblCrossSectionView.Size = new System.Drawing.Size(88, 13);
			this.lblCrossSectionView.TabIndex = 11;
			this.lblCrossSectionView.Text = "crosssectionview";
			// 
			// btnFilters
			// 
			this.btnFilters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnFilters.Location = new System.Drawing.Point(184, 530);
			this.btnFilters.Name = "btnFilters";
			this.btnFilters.Size = new System.Drawing.Size(84, 23);
			this.btnFilters.TabIndex = 10;
			this.btnFilters.Text = "filters";
			this.btnFilters.UseVisualStyleBackColor = true;
			this.btnFilters.Click += new System.EventHandler(this.btnAdvanced_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.btnCancel.Location = new System.Drawing.Point(274, 530);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(81, 24);
			this.btnCancel.TabIndex = 9;
			this.btnCancel.Text = "cancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			// 
			// lblSelSize
			// 
			this.lblSelSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.lblSelSize.AutoSize = true;
			this.lblSelSize.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblSelSize.Location = new System.Drawing.Point(184, 497);
			this.lblSelSize.Name = "lblSelSize";
			this.lblSelSize.Size = new System.Drawing.Size(45, 13);
			this.lblSelSize.TabIndex = 8;
			this.lblSelSize.Text = "noworld";
			// 
			// rbtEnd
			// 
			this.rbtEnd.AutoSize = true;
			this.rbtEnd.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbtEnd.Location = new System.Drawing.Point(148, 30);
			this.rbtEnd.Name = "rbtEnd";
			this.rbtEnd.Size = new System.Drawing.Size(43, 17);
			this.rbtEnd.TabIndex = 6;
			this.rbtEnd.Text = "end";
			this.rbtEnd.UseVisualStyleBackColor = true;
			this.rbtEnd.CheckedChanged += new System.EventHandler(this.rbtEnd_CheckedChanged);
			// 
			// rbtNether
			// 
			this.rbtNether.AutoSize = true;
			this.rbtNether.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbtNether.Location = new System.Drawing.Point(85, 30);
			this.rbtNether.Name = "rbtNether";
			this.rbtNether.Size = new System.Drawing.Size(57, 17);
			this.rbtNether.TabIndex = 6;
			this.rbtNether.Text = "nether";
			this.rbtNether.UseVisualStyleBackColor = true;
			this.rbtNether.CheckedChanged += new System.EventHandler(this.rbtNether_CheckedChanged);
			// 
			// rbtOver
			// 
			this.rbtOver.AutoSize = true;
			this.rbtOver.Checked = true;
			this.rbtOver.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.rbtOver.Location = new System.Drawing.Point(6, 30);
			this.rbtOver.Name = "rbtOver";
			this.rbtOver.Size = new System.Drawing.Size(73, 17);
			this.rbtOver.TabIndex = 6;
			this.rbtOver.TabStop = true;
			this.rbtOver.Text = "overworld";
			this.rbtOver.UseVisualStyleBackColor = true;
			this.rbtOver.CheckedChanged += new System.EventHandler(this.rbtOver_CheckedChanged);
			// 
			// panYZ
			// 
			this.panYZ.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.panYZ.BackgroundImage = global::import.Properties.Resources.transparentgrid1;
			this.panYZ.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.panYZ.Controls.Add(this.pboxWorldXZ);
			this.panYZ.Location = new System.Drawing.Point(0, 65);
			this.panYZ.Name = "panYZ";
			this.panYZ.Size = new System.Drawing.Size(357, 420);
			this.panYZ.TabIndex = 5;
			// 
			// pboxWorldXZ
			// 
			this.pboxWorldXZ.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.pboxWorldXZ.BackColor = System.Drawing.Color.Transparent;
			this.pboxWorldXZ.Location = new System.Drawing.Point(0, 0);
			this.pboxWorldXZ.Name = "pboxWorldXZ";
			this.pboxWorldXZ.Size = new System.Drawing.Size(353, 416);
			this.pboxWorldXZ.TabIndex = 0;
			this.pboxWorldXZ.TabStop = false;
			this.pboxWorldXZ.SizeChanged += new System.EventHandler(this.ResizeXZ);
			this.pboxWorldXZ.MouseDown += new System.Windows.Forms.MouseEventHandler(this.MoveXZStart);
			this.pboxWorldXZ.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OverXZ);
			this.pboxWorldXZ.MouseUp += new System.Windows.Forms.MouseEventHandler(this.MoveXZEnd);
			this.pboxWorldXZ.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.ZoomXZ);
			// 
			// lblInfo
			// 
			this.lblInfo.AutoSize = true;
			this.lblInfo.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblInfo.Location = new System.Drawing.Point(3, 8);
			this.lblInfo.Name = "lblInfo";
			this.lblInfo.Size = new System.Drawing.Size(25, 13);
			this.lblInfo.TabIndex = 7;
			this.lblInfo.Text = "info";
			// 
			// frmImport
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(200)))), ((int)(((byte)(200)))), ((int)(((byte)(200)))));
			this.ClientSize = new System.Drawing.Size(1005, 584);
			this.Controls.Add(this.lblInfo);
			this.Controls.Add(this.splitContainer1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "frmImport";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "title";
			this.Load += new System.EventHandler(this.frmImport_Load);
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel1.PerformLayout();
			this.splitContainer1.Panel2.ResumeLayout(false);
			this.splitContainer1.Panel2.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.panXY.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pboxWorldXY)).EndInit();
			this.panYZ.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.pboxWorldXZ)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

#endregion

		private System.Windows.Forms.Panel panXY;
		private System.Windows.Forms.Label lblWorld;
		private System.Windows.Forms.ComboBox cbxSaves;
		private System.Windows.Forms.Button btnDone;
		private System.Windows.Forms.Panel panYZ;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.RadioButton rbtEnd;
		private System.Windows.Forms.RadioButton rbtNether;
		private System.Windows.Forms.RadioButton rbtOver;
		private System.Windows.Forms.PictureBox pboxWorldXY;
		private System.Windows.Forms.PictureBox pboxWorldXZ;
		private System.Windows.Forms.Label lblInfo;
		private System.Windows.Forms.Label lblSelSize;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnFilters;
		private System.Windows.Forms.Label lblTopDownView;
		private System.Windows.Forms.Label lblCrossSectionView;
		private System.Windows.Forms.Label lblFilterInfo;

	}
}