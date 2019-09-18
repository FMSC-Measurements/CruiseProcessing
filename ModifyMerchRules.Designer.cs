namespace CruiseProcessing
{
    partial class ModifyMerchRules
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyMerchRules));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.nonsawProducts = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.previousButton = new System.Windows.Forms.Button();
            this.nextButton = new System.Windows.Forms.Button();
            this.finishedButton = new System.Windows.Forms.Button();
            this.segmentLogic = new System.Windows.Forms.ComboBox();
            this.trimText = new System.Windows.Forms.TextBox();
            this.minLogLength = new System.Windows.Forms.NumericUpDown();
            this.maxLogLength = new System.Windows.Forms.NumericUpDown();
            this.minMerchLength = new System.Windows.Forms.NumericUpDown();
            this.stumpHgt = new System.Windows.Forms.TextBox();
            this.topDIB = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.oddSegment = new System.Windows.Forms.RadioButton();
            this.evenSegment = new System.Windows.Forms.RadioButton();
            this.label12 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.currVolEq = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.minLogLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxLogLength)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minMerchLength)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(423, 41);
            this.label1.TabIndex = 0;
            this.label1.Text = "To view merchandizing rules for a non-sawtimber product, select a product from th" +
                "e list below.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(13, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(150, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Non-sawtimber Products";
            // 
            // nonsawProducts
            // 
            this.nonsawProducts.FormattingEnabled = true;
            this.nonsawProducts.Location = new System.Drawing.Point(169, 53);
            this.nonsawProducts.Name = "nonsawProducts";
            this.nonsawProducts.Size = new System.Drawing.Size(56, 17);
            this.nonsawProducts.TabIndex = 2;
            this.nonsawProducts.SelectedIndexChanged += new System.EventHandler(this.onItemSelected);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label3.Location = new System.Drawing.Point(13, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(431, 107);
            this.label3.TabIndex = 3;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label5.Location = new System.Drawing.Point(23, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 16);
            this.label5.TabIndex = 6;
            this.label5.Text = "Trim (<1)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label6.Location = new System.Drawing.Point(23, 45);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 16);
            this.label6.TabIndex = 7;
            this.label6.Text = "Minimum log length";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label7.Location = new System.Drawing.Point(23, 77);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(125, 16);
            this.label7.TabIndex = 8;
            this.label7.Text = "Maximum log length";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label8.Location = new System.Drawing.Point(23, 109);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(119, 16);
            this.label8.TabIndex = 9;
            this.label8.Text = "Segmentation logic";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label9.Location = new System.Drawing.Point(23, 227);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(85, 16);
            this.label9.TabIndex = 10;
            this.label9.Text = "Stump height";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label10.Location = new System.Drawing.Point(23, 259);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 16);
            this.label10.TabIndex = 11;
            this.label10.Text = "Top DIB ";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label11.Location = new System.Drawing.Point(23, 141);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(140, 16);
            this.label11.TabIndex = 12;
            this.label11.Text = "Minimum merch length";
            // 
            // previousButton
            // 
            this.previousButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.previousButton.Location = new System.Drawing.Point(126, 509);
            this.previousButton.Name = "previousButton";
            this.previousButton.Size = new System.Drawing.Size(75, 23);
            this.previousButton.TabIndex = 14;
            this.previousButton.Text = "Previous";
            this.previousButton.UseVisualStyleBackColor = true;
            this.previousButton.Click += new System.EventHandler(this.onPrevious);
            // 
            // nextButton
            // 
            this.nextButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.nextButton.Location = new System.Drawing.Point(243, 509);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(75, 23);
            this.nextButton.TabIndex = 15;
            this.nextButton.Text = "Next";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.onNext);
            // 
            // finishedButton
            // 
            this.finishedButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.finishedButton.Location = new System.Drawing.Point(369, 538);
            this.finishedButton.Name = "finishedButton";
            this.finishedButton.Size = new System.Drawing.Size(75, 23);
            this.finishedButton.TabIndex = 16;
            this.finishedButton.Text = "FINISHED";
            this.finishedButton.UseVisualStyleBackColor = true;
            this.finishedButton.Click += new System.EventHandler(this.onFinished);
            // 
            // segmentLogic
            // 
            this.segmentLogic.Enabled = false;
            this.segmentLogic.FormattingEnabled = true;
            this.segmentLogic.Items.AddRange(new object[] {
            "21 -- If top seg < 1/2 nom log len, combine with next lowest log",
            "22 -- Top placed with next lowest log and segmented",
            "23 -- Top segment stands on its own",
            "24 -- If top seg < 1/4 log len drop the top.  If top >= 1/4 and <= 3/4 nom length" +
                ", top is 1/2 of nom log length, else top is nom log len."});
            this.segmentLogic.Location = new System.Drawing.Point(174, 104);
            this.segmentLogic.Name = "segmentLogic";
            this.segmentLogic.Size = new System.Drawing.Size(203, 21);
            this.segmentLogic.TabIndex = 17;
            // 
            // trimText
            // 
            this.trimText.Enabled = false;
            this.trimText.Location = new System.Drawing.Point(174, 13);
            this.trimText.Name = "trimText";
            this.trimText.Size = new System.Drawing.Size(29, 20);
            this.trimText.TabIndex = 18;
            // 
            // minLogLength
            // 
            this.minLogLength.Enabled = false;
            this.minLogLength.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.minLogLength.Location = new System.Drawing.Point(174, 45);
            this.minLogLength.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.minLogLength.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.minLogLength.Name = "minLogLength";
            this.minLogLength.Size = new System.Drawing.Size(63, 20);
            this.minLogLength.TabIndex = 19;
            this.minLogLength.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // maxLogLength
            // 
            this.maxLogLength.Enabled = false;
            this.maxLogLength.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.maxLogLength.Location = new System.Drawing.Point(174, 77);
            this.maxLogLength.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.maxLogLength.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.maxLogLength.Name = "maxLogLength";
            this.maxLogLength.Size = new System.Drawing.Size(63, 20);
            this.maxLogLength.TabIndex = 20;
            this.maxLogLength.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // minMerchLength
            // 
            this.minMerchLength.Enabled = false;
            this.minMerchLength.Increment = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.minMerchLength.Location = new System.Drawing.Point(174, 137);
            this.minMerchLength.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.minMerchLength.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.minMerchLength.Name = "minMerchLength";
            this.minMerchLength.Size = new System.Drawing.Size(63, 20);
            this.minMerchLength.TabIndex = 21;
            this.minMerchLength.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // stumpHgt
            // 
            this.stumpHgt.Enabled = false;
            this.stumpHgt.Location = new System.Drawing.Point(174, 227);
            this.stumpHgt.Name = "stumpHgt";
            this.stumpHgt.Size = new System.Drawing.Size(29, 20);
            this.stumpHgt.TabIndex = 22;
            // 
            // topDIB
            // 
            this.topDIB.Enabled = false;
            this.topDIB.Location = new System.Drawing.Point(174, 255);
            this.topDIB.Name = "topDIB";
            this.topDIB.Size = new System.Drawing.Size(29, 20);
            this.topDIB.TabIndex = 23;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.oddSegment);
            this.groupBox1.Controls.Add(this.evenSegment);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.topDIB);
            this.groupBox1.Controls.Add(this.stumpHgt);
            this.groupBox1.Controls.Add(this.minMerchLength);
            this.groupBox1.Controls.Add(this.maxLogLength);
            this.groupBox1.Controls.Add(this.minLogLength);
            this.groupBox1.Controls.Add(this.trimText);
            this.groupBox1.Controls.Add(this.segmentLogic);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Location = new System.Drawing.Point(43, 210);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 293);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            // 
            // oddSegment
            // 
            this.oddSegment.AutoSize = true;
            this.oddSegment.Location = new System.Drawing.Point(174, 196);
            this.oddSegment.Name = "oddSegment";
            this.oddSegment.Size = new System.Drawing.Size(116, 17);
            this.oddSegment.TabIndex = 26;
            this.oddSegment.TabStop = true;
            this.oddSegment.Text = "Odd length allowed";
            this.oddSegment.UseVisualStyleBackColor = true;
            this.oddSegment.CheckedChanged += new System.EventHandler(this.oddSelected);
            // 
            // evenSegment
            // 
            this.evenSegment.AutoSize = true;
            this.evenSegment.Location = new System.Drawing.Point(174, 173);
            this.evenSegment.Name = "evenSegment";
            this.evenSegment.Size = new System.Drawing.Size(145, 17);
            this.evenSegment.TabIndex = 25;
            this.evenSegment.TabStop = true;
            this.evenSegment.Text = "Even length only (default)";
            this.evenSegment.UseVisualStyleBackColor = true;
            this.evenSegment.CheckedChanged += new System.EventHandler(this.evenSelected);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label12.Location = new System.Drawing.Point(23, 173);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(103, 16);
            this.label12.TabIndex = 24;
            this.label12.Text = "Segment Length";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(28, 188);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(167, 16);
            this.label4.TabIndex = 25;
            this.label4.Text = "Current Volume Equation";
            // 
            // currVolEq
            // 
            this.currVolEq.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.currVolEq.Location = new System.Drawing.Point(201, 191);
            this.currVolEq.Name = "currVolEq";
            this.currVolEq.ReadOnly = true;
            this.currVolEq.Size = new System.Drawing.Size(100, 13);
            this.currVolEq.TabIndex = 26;
            // 
            // ModifyMerchRules
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(456, 573);
            this.Controls.Add(this.currVolEq);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.finishedButton);
            this.Controls.Add(this.nextButton);
            this.Controls.Add(this.previousButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.nonsawProducts);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModifyMerchRules";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Modify Merchandizing Rules";
            ((System.ComponentModel.ISupportInitialize)(this.minLogLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxLogLength)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minMerchLength)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox nonsawProducts;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button previousButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button finishedButton;
        private System.Windows.Forms.ComboBox segmentLogic;
        private System.Windows.Forms.TextBox trimText;
        private System.Windows.Forms.NumericUpDown minLogLength;
        private System.Windows.Forms.NumericUpDown maxLogLength;
        private System.Windows.Forms.NumericUpDown minMerchLength;
        private System.Windows.Forms.TextBox stumpHgt;
        private System.Windows.Forms.TextBox topDIB;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox currVolEq;
        private System.Windows.Forms.RadioButton oddSegment;
        private System.Windows.Forms.RadioButton evenSegment;
        private System.Windows.Forms.Label label12;
    }
}