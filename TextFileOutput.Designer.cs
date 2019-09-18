namespace CruiseProcessing
{
    partial class TextFileOutput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextFileOutput));
            this.label1 = new System.Windows.Forms.Label();
            this.finished_Button = new System.Windows.Forms.Button();
            this.go_Button = new System.Windows.Forms.Button();
            this.reportsList = new System.Windows.Forms.RichTextBox();
            this.fileStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(369, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "The following reports were selected for inclusion in the text file.";
            // 
            // finished_Button
            // 
            this.finished_Button.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.finished_Button.Location = new System.Drawing.Point(301, 105);
            this.finished_Button.Name = "finished_Button";
            this.finished_Button.Size = new System.Drawing.Size(84, 23);
            this.finished_Button.TabIndex = 2;
            this.finished_Button.Text = "FINISHED";
            this.finished_Button.UseVisualStyleBackColor = true;
            this.finished_Button.Click += new System.EventHandler(this.click_Finished);
            // 
            // go_Button
            // 
            this.go_Button.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.go_Button.Location = new System.Drawing.Point(12, 105);
            this.go_Button.Name = "go_Button";
            this.go_Button.Size = new System.Drawing.Size(75, 23);
            this.go_Button.TabIndex = 4;
            this.go_Button.Text = "GO";
            this.go_Button.UseVisualStyleBackColor = true;
            this.go_Button.Click += new System.EventHandler(this.click_Go);
            // 
            // reportsList
            // 
            this.reportsList.Font = new System.Drawing.Font("Arial", 9.75F);
            this.reportsList.Location = new System.Drawing.Point(12, 37);
            this.reportsList.Name = "reportsList";
            this.reportsList.ReadOnly = true;
            this.reportsList.Size = new System.Drawing.Size(366, 62);
            this.reportsList.TabIndex = 5;
            this.reportsList.Text = "";
            // 
            // fileStatus
            // 
            this.fileStatus.AutoSize = true;
            this.fileStatus.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.fileStatus.Location = new System.Drawing.Point(96, 108);
            this.fileStatus.Name = "fileStatus";
            this.fileStatus.Size = new System.Drawing.Size(199, 16);
            this.fileStatus.TabIndex = 6;
            this.fileStatus.Text = "Creating text file, please wait.";
            // 
            // TextFileOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(397, 138);
            this.Controls.Add(this.fileStatus);
            this.Controls.Add(this.reportsList);
            this.Controls.Add(this.go_Button);
            this.Controls.Add(this.finished_Button);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TextFileOutput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Creating Text File";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.onClose);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button finished_Button;
        private System.Windows.Forms.Button go_Button;
        private System.Windows.Forms.RichTextBox reportsList;
        public System.Windows.Forms.Label fileStatus;
    }
}