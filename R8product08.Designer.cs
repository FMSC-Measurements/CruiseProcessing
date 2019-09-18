namespace CruiseProcessing
{
    partial class R8product08
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R8product08));
            this.button1 = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.speciesDIBlist = new System.Windows.Forms.DataGridView();
            this.defaultDIB = new System.Windows.Forms.TextBox();
            this.applyButton = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Species = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.DOB = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.speciesDIBlist)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.button1.Location = new System.Drawing.Point(101, 306);
            this.button1.Margin = new System.Windows.Forms.Padding(4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 0;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.onOK);
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(273, 306);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 28);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(16, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(279, 19);
            this.label1.TabIndex = 2;
            this.label1.Text = "DOB to limit SWT volume calculation.";
            // 
            // speciesDIBlist
            // 
            this.speciesDIBlist.AllowUserToAddRows = false;
            this.speciesDIBlist.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.speciesDIBlist.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Species,
            this.DOB});
            this.speciesDIBlist.Location = new System.Drawing.Point(213, 86);
            this.speciesDIBlist.Margin = new System.Windows.Forms.Padding(4);
            this.speciesDIBlist.Name = "speciesDIBlist";
            this.speciesDIBlist.RowTemplate.Height = 24;
            this.speciesDIBlist.Size = new System.Drawing.Size(208, 198);
            this.speciesDIBlist.TabIndex = 3;
            // 
            // defaultDIB
            // 
            this.defaultDIB.Location = new System.Drawing.Point(37, 23);
            this.defaultDIB.Margin = new System.Windows.Forms.Padding(4);
            this.defaultDIB.MaxLength = 4;
            this.defaultDIB.Name = "defaultDIB";
            this.defaultDIB.Size = new System.Drawing.Size(51, 26);
            this.defaultDIB.TabIndex = 4;
            this.defaultDIB.WordWrap = false;
            // 
            // applyButton
            // 
            this.applyButton.AutoSize = true;
            this.applyButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.applyButton.Location = new System.Drawing.Point(24, 58);
            this.applyButton.Margin = new System.Windows.Forms.Padding(4);
            this.applyButton.Name = "applyButton";
            this.applyButton.Size = new System.Drawing.Size(80, 36);
            this.applyButton.TabIndex = 5;
            this.applyButton.Text = "Apply";
            this.applyButton.UseVisualStyleBackColor = true;
            this.applyButton.Click += new System.EventHandler(this.onApply);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.applyButton);
            this.groupBox1.Controls.Add(this.defaultDIB);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.groupBox1.Location = new System.Drawing.Point(19, 110);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(131, 106);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Default DOB";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(20, 224);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 71);
            this.label2.TabIndex = 7;
            this.label2.Text = "Apply the default DOB to all species in the list by clicking the Apply button.";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 42);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(349, 41);
            this.label3.TabIndex = 8;
            this.label3.Text = "To change DOB for an individual species, type the new DOB in the DOB column for t" +
                "hat species.";
            // 
            // Species
            // 
            this.Species.HeaderText = "SPECIES";
            this.Species.Name = "Species";
            this.Species.ReadOnly = true;
            this.Species.Width = 65;
            // 
            // DOB
            // 
            this.DOB.HeaderText = "DOB";
            this.DOB.Name = "DOB";
            this.DOB.Width = 45;
            // 
            // R8product08
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(464, 352);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.speciesDIBlist);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "R8product08";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " SWT Top DOB by Species";
            this.Click += new System.EventHandler(this.onApply);
            ((System.ComponentModel.ISupportInitialize)(this.speciesDIBlist)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView speciesDIBlist;
        private System.Windows.Forms.TextBox defaultDIB;
        private System.Windows.Forms.Button applyButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridViewTextBoxColumn Species;
        private System.Windows.Forms.DataGridViewTextBoxColumn DOB;
    }
}