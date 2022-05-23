namespace SeamCarvingAdvanced.GUI
{
    partial class frmMain
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
	        this.groupBox1 = new System.Windows.Forms.GroupBox();
	        this.tbBlockSize = new System.Windows.Forms.TextBox();
	        this.label12 = new System.Windows.Forms.Label();
	        this.tbNeighbourCount = new System.Windows.Forms.TextBox();
	        this.label7 = new System.Windows.Forms.Label();
	        this.label11 = new System.Windows.Forms.Label();
	        this.tbCairAppPath = new System.Windows.Forms.TextBox();
	        this.tbHeight = new System.Windows.Forms.TextBox();
	        this.tbWidth = new System.Windows.Forms.TextBox();
	        this.label10 = new System.Windows.Forms.Label();
	        this.label5 = new System.Windows.Forms.Label();
	        this.cmbSeamCarvingMethod = new System.Windows.Forms.ComboBox();
	        this.cbParallel = new System.Windows.Forms.CheckBox();
	        this.cbForwardEnergy = new System.Windows.Forms.CheckBox();
	        this.cbHd = new System.Windows.Forms.CheckBox();
	        this.label4 = new System.Windows.Forms.Label();
	        this.tbNeighbourCountRatio = new System.Windows.Forms.TextBox();
	        this.label3 = new System.Windows.Forms.Label();
	        this.cmbEnergyFuncType = new System.Windows.Forms.ComboBox();
	        this.label2 = new System.Windows.Forms.Label();
	        this.tbHeightRatio = new System.Windows.Forms.TextBox();
	        this.label1 = new System.Windows.Forms.Label();
	        this.tbWidthRatio = new System.Windows.Forms.TextBox();
	        this.label6 = new System.Windows.Forms.Label();
	        this.tbCalculationTime = new System.Windows.Forms.TextBox();
	        this.splitContainer1 = new System.Windows.Forms.SplitContainer();
	        this.btnOpenImage = new System.Windows.Forms.Button();
	        this.tbInputImagePath = new System.Windows.Forms.TextBox();
	        this.label8 = new System.Windows.Forms.Label();
	        this.pbInput = new System.Windows.Forms.PictureBox();
	        this.pbOutput = new System.Windows.Forms.PictureBox();
	        this.label9 = new System.Windows.Forms.Label();
	        this.openImageDialog = new System.Windows.Forms.OpenFileDialog();
	        this.btnProcess = new System.Windows.Forms.Button();
	        this.groupBox1.SuspendLayout();
	        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
	        this.splitContainer1.Panel1.SuspendLayout();
	        this.splitContainer1.Panel2.SuspendLayout();
	        this.splitContainer1.SuspendLayout();
	        ((System.ComponentModel.ISupportInitialize)(this.pbInput)).BeginInit();
	        ((System.ComponentModel.ISupportInitialize)(this.pbOutput)).BeginInit();
	        this.SuspendLayout();
	        // 
	        // groupBox1
	        // 
	        this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
	        this.groupBox1.Controls.Add(this.tbBlockSize);
	        this.groupBox1.Controls.Add(this.label12);
	        this.groupBox1.Controls.Add(this.tbNeighbourCount);
	        this.groupBox1.Controls.Add(this.label7);
	        this.groupBox1.Controls.Add(this.label11);
	        this.groupBox1.Controls.Add(this.tbCairAppPath);
	        this.groupBox1.Controls.Add(this.tbHeight);
	        this.groupBox1.Controls.Add(this.tbWidth);
	        this.groupBox1.Controls.Add(this.label10);
	        this.groupBox1.Controls.Add(this.label5);
	        this.groupBox1.Controls.Add(this.cmbSeamCarvingMethod);
	        this.groupBox1.Controls.Add(this.cbParallel);
	        this.groupBox1.Controls.Add(this.cbForwardEnergy);
	        this.groupBox1.Controls.Add(this.cbHd);
	        this.groupBox1.Controls.Add(this.label4);
	        this.groupBox1.Controls.Add(this.tbNeighbourCountRatio);
	        this.groupBox1.Controls.Add(this.label3);
	        this.groupBox1.Controls.Add(this.cmbEnergyFuncType);
	        this.groupBox1.Controls.Add(this.label2);
	        this.groupBox1.Controls.Add(this.tbHeightRatio);
	        this.groupBox1.Controls.Add(this.label1);
	        this.groupBox1.Controls.Add(this.tbWidthRatio);
	        this.groupBox1.Location = new System.Drawing.Point(482, 5);
	        this.groupBox1.Name = "groupBox1";
	        this.groupBox1.Size = new System.Drawing.Size(294, 269);
	        this.groupBox1.TabIndex = 41;
	        this.groupBox1.TabStop = false;
	        this.groupBox1.Text = "Settings";
	        // 
	        // tbBlockSize
	        // 
	        this.tbBlockSize.Location = new System.Drawing.Point(143, 230);
	        this.tbBlockSize.Name = "tbBlockSize";
	        this.tbBlockSize.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbBlockSize.Size = new System.Drawing.Size(63, 20);
	        this.tbBlockSize.TabIndex = 64;
	        this.tbBlockSize.Text = "16";
	        this.tbBlockSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
	        // 
	        // label12
	        // 
	        this.label12.AutoSize = true;
	        this.label12.Location = new System.Drawing.Point(15, 233);
	        this.label12.Name = "label12";
	        this.label12.Size = new System.Drawing.Size(57, 13);
	        this.label12.TabIndex = 63;
	        this.label12.Text = "Block Size";
	        // 
	        // tbNeighbourCount
	        // 
	        this.tbNeighbourCount.Location = new System.Drawing.Point(213, 200);
	        this.tbNeighbourCount.Name = "tbNeighbourCount";
	        this.tbNeighbourCount.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbNeighbourCount.Size = new System.Drawing.Size(60, 20);
	        this.tbNeighbourCount.TabIndex = 62;
	        this.tbNeighbourCount.Text = "0";
	        this.tbNeighbourCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
	        this.tbNeighbourCount.TextChanged += new System.EventHandler(this.tbNeighbourCount_TextChanged);
	        // 
	        // label7
	        // 
	        this.label7.AutoSize = true;
	        this.label7.Location = new System.Drawing.Point(96, 77);
	        this.label7.Name = "label7";
	        this.label7.Size = new System.Drawing.Size(32, 13);
	        this.label7.TabIndex = 61;
	        this.label7.Text = "Ratio";
	        // 
	        // label11
	        // 
	        this.label11.AutoSize = true;
	        this.label11.Location = new System.Drawing.Point(15, 177);
	        this.label11.Name = "label11";
	        this.label11.Size = new System.Drawing.Size(79, 13);
	        this.label11.TabIndex = 60;
	        this.label11.Text = "CAIR App Path";
	        // 
	        // tbCairAppPath
	        // 
	        this.tbCairAppPath.Location = new System.Drawing.Point(143, 170);
	        this.tbCairAppPath.Name = "tbCairAppPath";
	        this.tbCairAppPath.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbCairAppPath.Size = new System.Drawing.Size(130, 20);
	        this.tbCairAppPath.TabIndex = 59;
	        this.tbCairAppPath.Text = "..\\..\\..\\Cair\\CAIR.exe";
	        // 
	        // tbHeight
	        // 
	        this.tbHeight.Location = new System.Drawing.Point(213, 72);
	        this.tbHeight.Name = "tbHeight";
	        this.tbHeight.ReadOnly = true;
	        this.tbHeight.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbHeight.Size = new System.Drawing.Size(60, 20);
	        this.tbHeight.TabIndex = 58;
	        this.tbHeight.Text = "0";
	        this.tbHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
	        this.tbHeight.TextChanged += new System.EventHandler(this.tbHeight_TextChanged);
	        // 
	        // tbWidth
	        // 
	        this.tbWidth.Location = new System.Drawing.Point(213, 46);
	        this.tbWidth.Name = "tbWidth";
	        this.tbWidth.ReadOnly = true;
	        this.tbWidth.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbWidth.Size = new System.Drawing.Size(60, 20);
	        this.tbWidth.TabIndex = 57;
	        this.tbWidth.Text = "0";
	        this.tbWidth.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
	        this.tbWidth.TextChanged += new System.EventHandler(this.tbWidth_TextChanged);
	        // 
	        // label10
	        // 
	        this.label10.AutoSize = true;
	        this.label10.Location = new System.Drawing.Point(95, 51);
	        this.label10.Name = "label10";
	        this.label10.Size = new System.Drawing.Size(32, 13);
	        this.label10.TabIndex = 55;
	        this.label10.Text = "Ratio";
	        // 
	        // label5
	        // 
	        this.label5.AutoSize = true;
	        this.label5.Location = new System.Drawing.Point(15, 24);
	        this.label5.Name = "label5";
	        this.label5.Size = new System.Drawing.Size(112, 13);
	        this.label5.TabIndex = 54;
	        this.label5.Text = "Seam Carving Method";
	        // 
	        // cmbSeamCarvingMethod
	        // 
	        this.cmbSeamCarvingMethod.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
	        this.cmbSeamCarvingMethod.FormattingEnabled = true;
	        this.cmbSeamCarvingMethod.Location = new System.Drawing.Point(143, 19);
	        this.cmbSeamCarvingMethod.Name = "cmbSeamCarvingMethod";
	        this.cmbSeamCarvingMethod.Size = new System.Drawing.Size(130, 21);
	        this.cmbSeamCarvingMethod.TabIndex = 53;
	        this.cmbSeamCarvingMethod.SelectedIndexChanged += new System.EventHandler(this.cmbSeamCarvingMethod_SelectedIndexChanged);
	        // 
	        // cbParallel
	        // 
	        this.cbParallel.AutoSize = true;
	        this.cbParallel.Location = new System.Drawing.Point(213, 141);
	        this.cbParallel.Name = "cbParallel";
	        this.cbParallel.Size = new System.Drawing.Size(67, 21);
	        this.cbParallel.TabIndex = 52;
	        this.cbParallel.Text = "Parallel";
	        this.cbParallel.UseVisualStyleBackColor = true;
	        // 
	        // cbForwardEnergy
	        // 
	        this.cbForwardEnergy.AutoSize = true;
	        this.cbForwardEnergy.Location = new System.Drawing.Point(18, 141);
	        this.cbForwardEnergy.Name = "cbForwardEnergy";
	        this.cbForwardEnergy.Size = new System.Drawing.Size(107, 21);
	        this.cbForwardEnergy.TabIndex = 51;
	        this.cbForwardEnergy.Text = "Forward Energy";
	        this.cbForwardEnergy.UseVisualStyleBackColor = true;
	        // 
	        // cbHd
	        // 
	        this.cbHd.AutoSize = true;
	        this.cbHd.Location = new System.Drawing.Point(143, 141);
	        this.cbHd.Name = "cbHd";
	        this.cbHd.Size = new System.Drawing.Size(49, 21);
	        this.cbHd.TabIndex = 49;
	        this.cbHd.Text = "HD";
	        this.cbHd.UseVisualStyleBackColor = true;
	        // 
	        // label4
	        // 
	        this.label4.AutoSize = true;
	        this.label4.Location = new System.Drawing.Point(15, 203);
	        this.label4.Name = "label4";
	        this.label4.Size = new System.Drawing.Size(87, 13);
	        this.label4.TabIndex = 48;
	        this.label4.Text = "Neighbour Count";
	        // 
	        // tbNeighbourCountRatio
	        // 
	        this.tbNeighbourCountRatio.Location = new System.Drawing.Point(143, 200);
	        this.tbNeighbourCountRatio.Name = "tbNeighbourCountRatio";
	        this.tbNeighbourCountRatio.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbNeighbourCountRatio.Size = new System.Drawing.Size(63, 20);
	        this.tbNeighbourCountRatio.TabIndex = 47;
	        this.tbNeighbourCountRatio.Text = "1";
	        this.tbNeighbourCountRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
	        this.tbNeighbourCountRatio.TextChanged += new System.EventHandler(this.tbNeighbourCountRatio_TextChanged);
	        // 
	        // label3
	        // 
	        this.label3.AutoSize = true;
	        this.label3.Location = new System.Drawing.Point(15, 103);
	        this.label3.Name = "label3";
	        this.label3.Size = new System.Drawing.Size(40, 13);
	        this.label3.TabIndex = 46;
	        this.label3.Text = "Energy";
	        // 
	        // cmbEnergyFuncType
	        // 
	        this.cmbEnergyFuncType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
	        this.cmbEnergyFuncType.FormattingEnabled = true;
	        this.cmbEnergyFuncType.Location = new System.Drawing.Point(143, 103);
	        this.cmbEnergyFuncType.Name = "cmbEnergyFuncType";
	        this.cmbEnergyFuncType.Size = new System.Drawing.Size(130, 21);
	        this.cmbEnergyFuncType.TabIndex = 45;
	        // 
	        // label2
	        // 
	        this.label2.AutoSize = true;
	        this.label2.Location = new System.Drawing.Point(15, 77);
	        this.label2.Name = "label2";
	        this.label2.Size = new System.Drawing.Size(38, 13);
	        this.label2.TabIndex = 44;
	        this.label2.Text = "Height";
	        // 
	        // tbHeightRatio
	        // 
	        this.tbHeightRatio.Location = new System.Drawing.Point(143, 72);
	        this.tbHeightRatio.Name = "tbHeightRatio";
	        this.tbHeightRatio.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbHeightRatio.Size = new System.Drawing.Size(64, 20);
	        this.tbHeightRatio.TabIndex = 43;
	        this.tbHeightRatio.Text = "1";
	        this.tbHeightRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
	        this.tbHeightRatio.TextChanged += new System.EventHandler(this.tbHeightRatio_TextChanged);
	        // 
	        // label1
	        // 
	        this.label1.AutoSize = true;
	        this.label1.Location = new System.Drawing.Point(15, 51);
	        this.label1.Name = "label1";
	        this.label1.Size = new System.Drawing.Size(35, 13);
	        this.label1.TabIndex = 42;
	        this.label1.Text = "Width";
	        // 
	        // tbWidthRatio
	        // 
	        this.tbWidthRatio.Location = new System.Drawing.Point(143, 46);
	        this.tbWidthRatio.Name = "tbWidthRatio";
	        this.tbWidthRatio.RightToLeft = System.Windows.Forms.RightToLeft.No;
	        this.tbWidthRatio.Size = new System.Drawing.Size(64, 20);
	        this.tbWidthRatio.TabIndex = 41;
	        this.tbWidthRatio.Text = "0.75";
	        this.tbWidthRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
	        this.tbWidthRatio.TextChanged += new System.EventHandler(this.tbWidthRatio_TextChanged);
	        // 
	        // label6
	        // 
	        this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
	        this.label6.AutoSize = true;
	        this.label6.Location = new System.Drawing.Point(497, 323);
	        this.label6.Name = "label6";
	        this.label6.Size = new System.Drawing.Size(30, 13);
	        this.label6.TabIndex = 50;
	        this.label6.Text = "Time";
	        // 
	        // tbCalculationTime
	        // 
	        this.tbCalculationTime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
	        this.tbCalculationTime.Location = new System.Drawing.Point(625, 320);
	        this.tbCalculationTime.Name = "tbCalculationTime";
	        this.tbCalculationTime.ReadOnly = true;
	        this.tbCalculationTime.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
	        this.tbCalculationTime.Size = new System.Drawing.Size(130, 20);
	        this.tbCalculationTime.TabIndex = 49;
	        this.tbCalculationTime.Text = "0";
	        // 
	        // splitContainer1
	        // 
	        this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.splitContainer1.Location = new System.Drawing.Point(12, 12);
	        this.splitContainer1.Name = "splitContainer1";
	        this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
	        // 
	        // splitContainer1.Panel1
	        // 
	        this.splitContainer1.Panel1.Controls.Add(this.btnOpenImage);
	        this.splitContainer1.Panel1.Controls.Add(this.tbInputImagePath);
	        this.splitContainer1.Panel1.Controls.Add(this.label8);
	        this.splitContainer1.Panel1.Controls.Add(this.pbInput);
	        // 
	        // splitContainer1.Panel2
	        // 
	        this.splitContainer1.Panel2.Controls.Add(this.pbOutput);
	        this.splitContainer1.Panel2.Controls.Add(this.label9);
	        this.splitContainer1.Size = new System.Drawing.Size(464, 739);
	        this.splitContainer1.SplitterDistance = 368;
	        this.splitContainer1.TabIndex = 51;
	        // 
	        // btnOpenImage
	        // 
	        this.btnOpenImage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
	        this.btnOpenImage.Location = new System.Drawing.Point(433, 1);
	        this.btnOpenImage.Name = "btnOpenImage";
	        this.btnOpenImage.Size = new System.Drawing.Size(29, 23);
	        this.btnOpenImage.TabIndex = 4;
	        this.btnOpenImage.Text = "...";
	        this.btnOpenImage.UseVisualStyleBackColor = true;
	        this.btnOpenImage.Click += new System.EventHandler(this.btnOpenImage_Click);
	        // 
	        // tbInputImagePath
	        // 
	        this.tbInputImagePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.tbInputImagePath.Location = new System.Drawing.Point(51, 3);
	        this.tbInputImagePath.Name = "tbInputImagePath";
	        this.tbInputImagePath.ReadOnly = true;
	        this.tbInputImagePath.Size = new System.Drawing.Size(376, 20);
	        this.tbInputImagePath.TabIndex = 3;
	        this.tbInputImagePath.Text = "..\\..\\..\\,,\\Sample Images\\Castle and Man.bmp";
	        // 
	        // label8
	        // 
	        this.label8.AutoSize = true;
	        this.label8.Location = new System.Drawing.Point(3, 6);
	        this.label8.Name = "label8";
	        this.label8.Size = new System.Drawing.Size(42, 13);
	        this.label8.TabIndex = 2;
	        this.label8.Text = "Original";
	        // 
	        // pbInput
	        // 
	        this.pbInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.pbInput.BackColor = System.Drawing.Color.LightBlue;
	        this.pbInput.Location = new System.Drawing.Point(3, 29);
	        this.pbInput.Name = "pbInput";
	        this.pbInput.Size = new System.Drawing.Size(458, 336);
	        this.pbInput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
	        this.pbInput.TabIndex = 1;
	        this.pbInput.TabStop = false;
	        // 
	        // pbOutput
	        // 
	        this.pbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) | System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right)));
	        this.pbOutput.BackColor = System.Drawing.Color.LightBlue;
	        this.pbOutput.Location = new System.Drawing.Point(3, 22);
	        this.pbOutput.Name = "pbOutput";
	        this.pbOutput.Size = new System.Drawing.Size(458, 342);
	        this.pbOutput.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
	        this.pbOutput.TabIndex = 4;
	        this.pbOutput.TabStop = false;
	        // 
	        // label9
	        // 
	        this.label9.AutoSize = true;
	        this.label9.Location = new System.Drawing.Point(3, 6);
	        this.label9.Name = "label9";
	        this.label9.Size = new System.Drawing.Size(39, 13);
	        this.label9.TabIndex = 3;
	        this.label9.Text = "Output";
	        // 
	        // openImageDialog
	        // 
	        this.openImageDialog.FileName = "openImageDialog";
	        this.openImageDialog.Filter = "All supported formats|*.jpeg;*.jpg;*.png;*.bmp;*.gif|JPEG Files (*.jpeg,*.jpg)|*." + "jpeg;*.jpg|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|GIF Files (*.gif)|*.g" + "if";
	        // 
	        // btnProcess
	        // 
	        this.btnProcess.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
	        this.btnProcess.Location = new System.Drawing.Point(625, 291);
	        this.btnProcess.Name = "btnProcess";
	        this.btnProcess.Size = new System.Drawing.Size(130, 23);
	        this.btnProcess.TabIndex = 52;
	        this.btnProcess.Text = "Process";
	        this.btnProcess.UseVisualStyleBackColor = true;
	        this.btnProcess.Click += new System.EventHandler(this.btnProcess_Click);
	        // 
	        // frmMain
	        // 
	        this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
	        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
	        this.ClientSize = new System.Drawing.Size(787, 763);
	        this.Controls.Add(this.btnProcess);
	        this.Controls.Add(this.splitContainer1);
	        this.Controls.Add(this.label6);
	        this.Controls.Add(this.tbCalculationTime);
	        this.Controls.Add(this.groupBox1);
	        this.Name = "frmMain";
	        this.Text = "Seam Carving Advanced";
	        this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.frmMain_FormClosed);
	        this.Load += new System.EventHandler(this.frmMain_Load);
	        this.groupBox1.ResumeLayout(false);
	        this.groupBox1.PerformLayout();
	        this.splitContainer1.Panel1.ResumeLayout(false);
	        this.splitContainer1.Panel1.PerformLayout();
	        this.splitContainer1.Panel2.ResumeLayout(false);
	        this.splitContainer1.Panel2.PerformLayout();
	        ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
	        this.splitContainer1.ResumeLayout(false);
	        ((System.ComponentModel.ISupportInitialize)(this.pbInput)).EndInit();
	        ((System.ComponentModel.ISupportInitialize)(this.pbOutput)).EndInit();
	        this.ResumeLayout(false);
	        this.PerformLayout();
        }

        #endregion

		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.ComboBox cmbSeamCarvingMethod;
		private System.Windows.Forms.CheckBox cbParallel;
		private System.Windows.Forms.CheckBox cbForwardEnergy;
		private System.Windows.Forms.CheckBox cbHd;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox tbNeighbourCountRatio;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox cmbEnergyFuncType;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.TextBox tbHeightRatio;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbWidthRatio;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox tbCalculationTime;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.Button btnOpenImage;
		private System.Windows.Forms.TextBox tbInputImagePath;
		private System.Windows.Forms.Label label8;
		private System.Windows.Forms.PictureBox pbInput;
		private System.Windows.Forms.PictureBox pbOutput;
		private System.Windows.Forms.Label label9;
		private System.Windows.Forms.OpenFileDialog openImageDialog;
		private System.Windows.Forms.Button btnProcess;
		private System.Windows.Forms.TextBox tbHeight;
		private System.Windows.Forms.TextBox tbWidth;
		private System.Windows.Forms.Label label10;
		private System.Windows.Forms.Label label11;
		private System.Windows.Forms.TextBox tbCairAppPath;
		private System.Windows.Forms.TextBox tbNeighbourCount;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox tbBlockSize;
		private System.Windows.Forms.Label label12;
    }
}

