namespace CruiseProcessing
{
    partial class FLIWdlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FLIWdlg));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.speciesAssoc = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.fractionText = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.DSTinclude = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.exitButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(380, 37);
            this.label1.TabIndex = 0;
            this.label1.Text = "For the species association show, enter the fraction left in the woods and/or per" +
                "cent of damaged small trees included.";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(9, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(309, 71);
            this.label2.TabIndex = 1;
            this.label2.Text = "For the WT2 report, enter the percent of damaged small trees to include in the re" +
                "port.  No entry for the WT3 report is required but if entered, does not affect t" +
                "hat report.";
            // 
            // speciesAssoc
            // 
            this.speciesAssoc.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.speciesAssoc.Location = new System.Drawing.Point(15, 49);
            this.speciesAssoc.Name = "speciesAssoc";
            this.speciesAssoc.ReadOnly = true;
            this.speciesAssoc.Size = new System.Drawing.Size(148, 13);
            this.speciesAssoc.TabIndex = 0;
            this.speciesAssoc.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.fractionText);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.groupBox1.Location = new System.Drawing.Point(79, 77);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(232, 73);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "FRACTION LEFT IN THE WOODS";
            // 
            // fractionText
            // 
            this.fractionText.Location = new System.Drawing.Point(155, 30);
            this.fractionText.Name = "fractionText";
            this.fractionText.Size = new System.Drawing.Size(51, 22);
            this.fractionText.TabIndex = 1;
            this.fractionText.Leave += new System.EventHandler(this.onFractionLeave);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label3.Location = new System.Drawing.Point(9, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 50);
            this.label3.TabIndex = 0;
            this.label3.Text = "Enter as a decimal, i.e. 0.55 for 55% or 1.00 for 100%.";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.DSTinclude);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.groupBox2.Location = new System.Drawing.Point(50, 166);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(333, 143);
            this.groupBox2.TabIndex = 4;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "PERCENT DAMAGED SMALL TREES INCLUDED";
            // 
            // DSTinclude
            // 
            this.DSTinclude.Location = new System.Drawing.Point(184, 102);
            this.DSTinclude.Name = "DSTinclude";
            this.DSTinclude.Size = new System.Drawing.Size(51, 22);
            this.DSTinclude.TabIndex = 2;
            this.DSTinclude.Leave += new System.EventHandler(this.onIncludeLeave);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(9, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(140, 50);
            this.label4.TabIndex = 2;
            this.label4.Text = "Enter as a decimal, i.e. 0.55 for 55% or 1.00 for 100%.";
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(176, 325);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 5;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // FLIWdlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 359);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.speciesAssoc);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FLIWdlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Fraction Left in Woods and % Damaged Small Trees Included";
            this.Leave += new System.EventHandler(this.onFractionLeave);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.TextBox speciesAssoc;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox fractionText;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox DSTinclude;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button exitButton;
    }
}