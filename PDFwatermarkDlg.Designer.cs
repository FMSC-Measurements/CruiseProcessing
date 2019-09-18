namespace CruiseProcessing
{
    partial class PDFwatermarkDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PDFwatermarkDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.WatermarkNone = new System.Windows.Forms.RadioButton();
            this.WatermarkDraft = new System.Windows.Forms.RadioButton();
            this.WatermarkCOR = new System.Windows.Forms.RadioButton();
            this.watermarkCruise = new System.Windows.Forms.RadioButton();
            this.include_Date = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.5F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(168, 44);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select the desired watermark for the PDF file.";
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Arial", 9.5F);
            this.button1.Location = new System.Drawing.Point(164, 226);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "FINISHED";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.onFinished);
            // 
            // WatermarkNone
            // 
            this.WatermarkNone.AutoSize = true;
            this.WatermarkNone.Font = new System.Drawing.Font("Arial", 9.5F);
            this.WatermarkNone.Location = new System.Drawing.Point(36, 56);
            this.WatermarkNone.Name = "WatermarkNone";
            this.WatermarkNone.Size = new System.Drawing.Size(106, 20);
            this.WatermarkNone.TabIndex = 2;
            this.WatermarkNone.TabStop = true;
            this.WatermarkNone.Text = "No watermark";
            this.WatermarkNone.UseVisualStyleBackColor = true;
            this.WatermarkNone.Click += new System.EventHandler(this.onNoWatermark);
            // 
            // WatermarkDraft
            // 
            this.WatermarkDraft.AutoSize = true;
            this.WatermarkDraft.Font = new System.Drawing.Font("Arial", 9.5F);
            this.WatermarkDraft.Location = new System.Drawing.Point(36, 95);
            this.WatermarkDraft.Name = "WatermarkDraft";
            this.WatermarkDraft.Size = new System.Drawing.Size(68, 20);
            this.WatermarkDraft.TabIndex = 3;
            this.WatermarkDraft.TabStop = true;
            this.WatermarkDraft.Text = "DRAFT";
            this.WatermarkDraft.UseVisualStyleBackColor = true;
            this.WatermarkDraft.Click += new System.EventHandler(this.onDraft);
            // 
            // WatermarkCOR
            // 
            this.WatermarkCOR.AutoSize = true;
            this.WatermarkCOR.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
            this.WatermarkCOR.Font = new System.Drawing.Font("Arial", 9.5F);
            this.WatermarkCOR.Location = new System.Drawing.Point(36, 137);
            this.WatermarkCOR.Name = "WatermarkCOR";
            this.WatermarkCOR.Size = new System.Drawing.Size(176, 20);
            this.WatermarkCOR.TabIndex = 4;
            this.WatermarkCOR.TabStop = true;
            this.WatermarkCOR.Text = "CONTRACT OF RECORD";
            this.WatermarkCOR.UseVisualStyleBackColor = true;
            this.WatermarkCOR.Click += new System.EventHandler(this.onContractOfRecord);
            // 
            // watermarkCruise
            // 
            this.watermarkCruise.AutoSize = true;
            this.watermarkCruise.Font = new System.Drawing.Font("Arial", 9F);
            this.watermarkCruise.Location = new System.Drawing.Point(36, 176);
            this.watermarkCruise.Name = "watermarkCruise";
            this.watermarkCruise.Size = new System.Drawing.Size(146, 19);
            this.watermarkCruise.TabIndex = 5;
            this.watermarkCruise.TabStop = true;
            this.watermarkCruise.Text = "CRUISE OF RECORD";
            this.watermarkCruise.UseVisualStyleBackColor = true;
            this.watermarkCruise.Click += new System.EventHandler(this.onCruiseOfRecord);
            // 
            // include_Date
            // 
            this.include_Date.Font = new System.Drawing.Font("Arial", 9F);
            this.include_Date.Location = new System.Drawing.Point(12, 214);
            this.include_Date.Name = "include_Date";
            this.include_Date.Size = new System.Drawing.Size(143, 48);
            this.include_Date.TabIndex = 6;
            this.include_Date.Text = "Include today\'s date in watermark?";
            this.include_Date.UseVisualStyleBackColor = true;
            this.include_Date.Click += new System.EventHandler(this.onIncludeDate);
            // 
            // PDFwatermarkDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(251, 266);
            this.Controls.Add(this.include_Date);
            this.Controls.Add(this.watermarkCruise);
            this.Controls.Add(this.WatermarkCOR);
            this.Controls.Add(this.WatermarkDraft);
            this.Controls.Add(this.WatermarkNone);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PDFwatermarkDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add Watermark to PDF";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RadioButton WatermarkNone;
        private System.Windows.Forms.RadioButton WatermarkDraft;
        private System.Windows.Forms.RadioButton WatermarkCOR;
        private System.Windows.Forms.RadioButton watermarkCruise;
        private System.Windows.Forms.CheckBox include_Date;
    }
}