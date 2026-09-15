namespace TengaiXMLApp
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.btnSelectFile = new System.Windows.Forms.Button();
            this.lblFilePath = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtOffsetLeft = new System.Windows.Forms.TextBox();
            this.txtOffsetRight = new System.Windows.Forms.TextBox();
            this.picPreview = new System.Windows.Forms.PictureBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.chkMultiLayer = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.txtLayerInterval = new System.Windows.Forms.TextBox();
            this.txtLayerCount = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // btnSelectFile
            // 
            this.btnSelectFile.Location = new System.Drawing.Point(12, 12);
            this.btnSelectFile.Name = "btnSelectFile";
            this.btnSelectFile.Size = new System.Drawing.Size(113, 23);
            this.btnSelectFile.TabIndex = 0;
            this.btnSelectFile.Text = "XMLファイル選択";
            this.btnSelectFile.UseVisualStyleBackColor = true;
            this.btnSelectFile.Click += new System.EventHandler(this.btnSelectFile_Click);
            // 
            // lblFilePath
            // 
            this.lblFilePath.AutoSize = true;
            this.lblFilePath.Location = new System.Drawing.Point(143, 17);
            this.lblFilePath.Name = "lblFilePath";
            this.lblFilePath.Size = new System.Drawing.Size(75, 12);
            this.lblFilePath.TabIndex = 3;
            this.lblFilePath.Text = "ファイル未選択";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "左オフセット距離 (m)";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(202, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "右オフセット距離 (m)";
            // 
            // txtOffsetLeft
            // 
            this.txtOffsetLeft.Location = new System.Drawing.Point(131, 50);
            this.txtOffsetLeft.Name = "txtOffsetLeft";
            this.txtOffsetLeft.Size = new System.Drawing.Size(40, 19);
            this.txtOffsetLeft.TabIndex = 1;
            this.txtOffsetLeft.Text = "30";
            this.txtOffsetLeft.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtOffsetRight
            // 
            this.txtOffsetRight.Location = new System.Drawing.Point(312, 50);
            this.txtOffsetRight.Name = "txtOffsetRight";
            this.txtOffsetRight.Size = new System.Drawing.Size(40, 19);
            this.txtOffsetRight.TabIndex = 2;
            this.txtOffsetRight.Text = "30";
            this.txtOffsetRight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // picPreview
            // 
            this.picPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.picPreview.BackColor = System.Drawing.Color.White;
            this.picPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picPreview.Location = new System.Drawing.Point(12, 97);
            this.picPreview.Name = "picPreview";
            this.picPreview.Size = new System.Drawing.Size(776, 646);
            this.picPreview.TabIndex = 6;
            this.picPreview.TabStop = false;
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("MS UI Gothic", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.lblStatus.Location = new System.Drawing.Point(12, 81);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(161, 13);
            this.lblStatus.TabIndex = 3;
            this.lblStatus.Text = "XMLファイルを選択してください";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(694, 749);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(94, 23);
            this.btnGenerate.TabIndex = 8;
            this.btnGenerate.Text = "拡張XML作成";
            this.btnGenerate.UseVisualStyleBackColor = true;
            // 
            // btnReset
            // 
            this.btnReset.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReset.Location = new System.Drawing.Point(12, 749);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(116, 23);
            this.btnReset.TabIndex = 7;
            this.btnReset.Text = "起終点選択 リセット";
            this.btnReset.UseVisualStyleBackColor = true;
            // 
            // chkMultiLayer
            // 
            this.chkMultiLayer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkMultiLayer.AutoSize = true;
            this.chkMultiLayer.Location = new System.Drawing.Point(637, 21);
            this.chkMultiLayer.Name = "chkMultiLayer";
            this.chkMultiLayer.Size = new System.Drawing.Size(122, 16);
            this.chkMultiLayer.TabIndex = 4;
            this.chkMultiLayer.Text = "上下の層も作成する";
            this.chkMultiLayer.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(651, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "z値変化量(m)";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(651, 72);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(72, 12);
            this.label4.TabIndex = 12;
            this.label4.Text = "作成する層数";
            // 
            // txtLayerInterval
            // 
            this.txtLayerInterval.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLayerInterval.Location = new System.Drawing.Point(733, 43);
            this.txtLayerInterval.Name = "txtLayerInterval";
            this.txtLayerInterval.Size = new System.Drawing.Size(45, 19);
            this.txtLayerInterval.TabIndex = 5;
            this.txtLayerInterval.Text = "-0.3";
            this.txtLayerInterval.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // txtLayerCount
            // 
            this.txtLayerCount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLayerCount.Location = new System.Drawing.Point(733, 69);
            this.txtLayerCount.Name = "txtLayerCount";
            this.txtLayerCount.Size = new System.Drawing.Size(45, 19);
            this.txtLayerCount.TabIndex = 6;
            this.txtLayerCount.Text = "10";
            this.txtLayerCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.ClientSize = new System.Drawing.Size(800, 784);
            this.Controls.Add(this.txtLayerCount);
            this.Controls.Add(this.txtLayerInterval);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.chkMultiLayer);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.picPreview);
            this.Controls.Add(this.txtOffsetRight);
            this.Controls.Add(this.txtOffsetLeft);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblFilePath);
            this.Controls.Add(this.btnSelectFile);
            this.MinimumSize = new System.Drawing.Size(600, 300);
            this.Name = "Form1";
            this.Text = "いきなり 転圧層拡張増殖";
            ((System.ComponentModel.ISupportInitialize)(this.picPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelectFile;
        private System.Windows.Forms.Label lblFilePath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtOffsetLeft;
        private System.Windows.Forms.TextBox txtOffsetRight;
        private System.Windows.Forms.PictureBox picPreview;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.CheckBox chkMultiLayer;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox txtLayerInterval;
        private System.Windows.Forms.TextBox txtLayerCount;
    }
}

