namespace CruiseProcessing
{
    partial class SelectCSV
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectCSV));
            this.label1 = new System.Windows.Forms.Label();
            this.checkCSV1 = new System.Windows.Forms.CheckBox();
            this.checkCSV2 = new System.Windows.Forms.CheckBox();
            this.checkCSV3 = new System.Windows.Forms.CheckBox();
            this.checkCSV4 = new System.Windows.Forms.CheckBox();
            this.checkCSV5 = new System.Windows.Forms.CheckBox();
            this.checkCSV6 = new System.Windows.Forms.CheckBox();
            this.button1 = new System.Windows.Forms.Button();
            this.create_files = new System.Windows.Forms.Button();
            this.checkCSV7 = new System.Windows.Forms.CheckBox();
            this.checkCSV8 = new System.Windows.Forms.CheckBox();
            this.checkCSV9 = new System.Windows.Forms.CheckBox();
            this.checkCSV10 = new System.Windows.Forms.CheckBox();
            this.checkVSM11 = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(16, 9);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(201, 32);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select CSV files to create by checking the box.";
            // 
            // checkCSV1
            // 
            this.checkCSV1.AutoSize = true;
            this.checkCSV1.Location = new System.Drawing.Point(80, 55);
            this.checkCSV1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkCSV1.Name = "checkCSV1";
            this.checkCSV1.Size = new System.Drawing.Size(441, 24);
            this.checkCSV1.TabIndex = 1;
            this.checkCSV1.Text = "CSV1 --  Comma-delimited text file from the A05 report";
            this.checkCSV1.UseVisualStyleBackColor = true;
            this.checkCSV1.CheckedChanged += new System.EventHandler(this.CSV1_checked);
            // 
            // checkCSV2
            // 
            this.checkCSV2.AutoSize = true;
            this.checkCSV2.Location = new System.Drawing.Point(80, 93);
            this.checkCSV2.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkCSV2.Name = "checkCSV2";
            this.checkCSV2.Size = new System.Drawing.Size(441, 24);
            this.checkCSV2.TabIndex = 2;
            this.checkCSV2.Text = "CSV2 --  Comma-delimited text file from the A06 report";
            this.checkCSV2.UseVisualStyleBackColor = true;
            this.checkCSV2.CheckedChanged += new System.EventHandler(this.CSV2_checked);
            // 
            // checkCSV3
            // 
            this.checkCSV3.AutoSize = true;
            this.checkCSV3.Location = new System.Drawing.Point(80, 132);
            this.checkCSV3.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkCSV3.Name = "checkCSV3";
            this.checkCSV3.Size = new System.Drawing.Size(441, 24);
            this.checkCSV3.TabIndex = 3;
            this.checkCSV3.Text = "CSV3 --  Comma-delimited text file from the A07 report";
            this.checkCSV3.UseVisualStyleBackColor = true;
            this.checkCSV3.CheckedChanged += new System.EventHandler(this.CSV3_checked);
            // 
            // checkCSV4
            // 
            this.checkCSV4.AutoSize = true;
            this.checkCSV4.Location = new System.Drawing.Point(80, 171);
            this.checkCSV4.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkCSV4.Name = "checkCSV4";
            this.checkCSV4.Size = new System.Drawing.Size(441, 24);
            this.checkCSV4.TabIndex = 4;
            this.checkCSV4.Text = "CSV4 --  Comma-delimited text file from the A10 report";
            this.checkCSV4.UseVisualStyleBackColor = true;
            this.checkCSV4.CheckedChanged += new System.EventHandler(this.CSV4_checked);
            // 
            // checkCSV5
            // 
            this.checkCSV5.AutoSize = true;
            this.checkCSV5.Location = new System.Drawing.Point(80, 212);
            this.checkCSV5.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkCSV5.Name = "checkCSV5";
            this.checkCSV5.Size = new System.Drawing.Size(431, 24);
            this.checkCSV5.TabIndex = 5;
            this.checkCSV5.Text = "CSV5 --  Comma-delimited text file from the L1 report";
            this.checkCSV5.UseVisualStyleBackColor = true;
            this.checkCSV5.CheckedChanged += new System.EventHandler(this.CSV5_checked);
            // 
            // checkCSV6
            // 
            this.checkCSV6.AutoSize = true;
            this.checkCSV6.Location = new System.Drawing.Point(80, 253);
            this.checkCSV6.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.checkCSV6.Name = "checkCSV6";
            this.checkCSV6.Size = new System.Drawing.Size(431, 24);
            this.checkCSV6.TabIndex = 6;
            this.checkCSV6.Text = "CSV6 --  Comma-delimited text file from the L2 report";
            this.checkCSV6.UseVisualStyleBackColor = true;
            this.checkCSV6.CheckedChanged += new System.EventHandler(this.CSV6_checked);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F);
            this.button1.Location = new System.Drawing.Point(304, 515);
            this.button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(108, 38);
            this.button1.TabIndex = 7;
            this.button1.Text = "FINISHED";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.onFinished);
            // 
            // create_files
            // 
            this.create_files.Location = new System.Drawing.Point(19, 515);
            this.create_files.Name = "create_files";
            this.create_files.Size = new System.Drawing.Size(118, 38);
            this.create_files.TabIndex = 8;
            this.create_files.Text = "Create Files";
            this.create_files.UseVisualStyleBackColor = true;
            this.create_files.Click += new System.EventHandler(this.onCreateFiles);
            // 
            // checkCSV7
            // 
            this.checkCSV7.AutoSize = true;
            this.checkCSV7.Location = new System.Drawing.Point(80, 295);
            this.checkCSV7.Name = "checkCSV7";
            this.checkCSV7.Size = new System.Drawing.Size(437, 24);
            this.checkCSV7.TabIndex = 9;
            this.checkCSV7.Text = "CSV7 -- Comma-delimited text file from the ST1 report";
            this.checkCSV7.UseVisualStyleBackColor = true;
            this.checkCSV7.CheckedChanged += new System.EventHandler(this.CSV7_checked);
            // 
            // checkCSV8
            // 
            this.checkCSV8.AutoSize = true;
            this.checkCSV8.Location = new System.Drawing.Point(80, 337);
            this.checkCSV8.Name = "checkCSV8";
            this.checkCSV8.Size = new System.Drawing.Size(440, 24);
            this.checkCSV8.TabIndex = 10;
            this.checkCSV8.Text = "CSV8 -- Comma-delimited text file from the UC5 report";
            this.checkCSV8.UseVisualStyleBackColor = true;
            this.checkCSV8.CheckedChanged += new System.EventHandler(this.CSV8_checked);
            // 
            // checkCSV9
            // 
            this.checkCSV9.AutoSize = true;
            this.checkCSV9.Location = new System.Drawing.Point(80, 378);
            this.checkCSV9.Name = "checkCSV9";
            this.checkCSV9.Size = new System.Drawing.Size(481, 24);
            this.checkCSV9.TabIndex = 11;
            this.checkCSV9.Text = "CSV9 -- Comma-delimited text file from Tree Estimate Table";
            this.checkCSV9.UseVisualStyleBackColor = true;
            this.checkCSV9.CheckedChanged += new System.EventHandler(this.CSV9_checked);
            // 
            // checkCSV10
            // 
            this.checkCSV10.AutoSize = true;
            this.checkCSV10.Location = new System.Drawing.Point(80, 424);
            this.checkCSV10.Name = "checkCSV10";
            this.checkCSV10.Size = new System.Drawing.Size(388, 24);
            this.checkCSV10.TabIndex = 12;
            this.checkCSV10.Text = "CSV10 -- Comma-delimited file for Timber Theft";
            this.checkCSV10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkCSV10.UseVisualStyleBackColor = true;
            this.checkCSV10.CheckedChanged += new System.EventHandler(this.CSV10_checked);
            // 
            // checkVSM11
            // 
            this.checkVSM11.AutoSize = true;
            this.checkVSM11.Location = new System.Drawing.Point(80, 463);
            this.checkVSM11.Name = "checkVSM11";
            this.checkVSM11.Size = new System.Drawing.Size(440, 24);
            this.checkVSM11.TabIndex = 13;
            this.checkVSM11.Text = "CSV11 -- Comma-delimited test file from VSM4 Report";
            this.checkVSM11.UseVisualStyleBackColor = true;
            this.checkVSM11.CheckedChanged += new System.EventHandler(this.CSV11_checked);
            // 
            // SelectCSV
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(590, 565);
            this.Controls.Add(this.checkVSM11);
            this.Controls.Add(this.checkCSV10);
            this.Controls.Add(this.checkCSV9);
            this.Controls.Add(this.checkCSV8);
            this.Controls.Add(this.checkCSV7);
            this.Controls.Add(this.create_files);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.checkCSV6);
            this.Controls.Add(this.checkCSV5);
            this.Controls.Add(this.checkCSV4);
            this.Controls.Add(this.checkCSV3);
            this.Controls.Add(this.checkCSV2);
            this.Controls.Add(this.checkCSV1);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "SelectCSV";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select CSV";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkCSV1;
        private System.Windows.Forms.CheckBox checkCSV2;
        private System.Windows.Forms.CheckBox checkCSV3;
        private System.Windows.Forms.CheckBox checkCSV4;
        private System.Windows.Forms.CheckBox checkCSV5;
        private System.Windows.Forms.CheckBox checkCSV6;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button create_files;
        private System.Windows.Forms.CheckBox checkCSV7;
        private System.Windows.Forms.CheckBox checkCSV8;
        private System.Windows.Forms.CheckBox checkCSV9;
        private System.Windows.Forms.CheckBox checkCSV10;
        private System.Windows.Forms.CheckBox checkVSM11;
    }
}