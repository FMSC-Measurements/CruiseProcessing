namespace CruiseProcessing
{
    partial class ExportDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.logSortCodes = new System.Windows.Forms.DataGridView();
            this.logGradeCodes = new System.Windows.Forms.DataGridView();
            this.exitButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.logSortCodes)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logGradeCodes)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(390, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Modify minimum and maximum values for selected log sort codes.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(11, 170);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(400, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Modify minimum and maximum values for selected log grade codes.";
            // 
            // logSortCodes
            // 
            this.logSortCodes.AllowUserToAddRows = false;
            this.logSortCodes.AllowUserToDeleteRows = false;
            this.logSortCodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.logSortCodes.Location = new System.Drawing.Point(14, 28);
            this.logSortCodes.Name = "logSortCodes";
            this.logSortCodes.Size = new System.Drawing.Size(388, 124);
            this.logSortCodes.TabIndex = 2;
            // 
            // logGradeCodes
            // 
            this.logGradeCodes.AllowUserToAddRows = false;
            this.logGradeCodes.AllowUserToDeleteRows = false;
            this.logGradeCodes.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.logGradeCodes.Location = new System.Drawing.Point(15, 190);
            this.logGradeCodes.Name = "logGradeCodes";
            this.logGradeCodes.Size = new System.Drawing.Size(387, 124);
            this.logGradeCodes.TabIndex = 3;
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(327, 330);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 4;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(236, 330);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // ExportDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 365);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.logGradeCodes);
            this.Controls.Add(this.logSortCodes);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ExportDialog";
            this.Text = " Moidfy Export Sort and Grade Specifications";
            ((System.ComponentModel.ISupportInitialize)(this.logSortCodes)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logGradeCodes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView logSortCodes;
        private System.Windows.Forms.DataGridView logGradeCodes;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button cancelButton;
    }
}