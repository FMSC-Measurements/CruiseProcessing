namespace CruiseProcessing
{
    partial class EnterNewFilename
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterNewFilename));
            this.label1 = new System.Windows.Forms.Label();
            this.newFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.DoneButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(14, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(289, 64);
            this.label1.TabIndex = 0;
            this.label1.Text = "Enter the new template filename.  NOTE:  This will be placed in the same director" +
                "y as the original template file.";
            // 
            // newFileName
            // 
            this.newFileName.Location = new System.Drawing.Point(56, 122);
            this.newFileName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.newFileName.Name = "newFileName";
            this.newFileName.Size = new System.Drawing.Size(210, 22);
            this.newFileName.TabIndex = 1;
            this.newFileName.TextChanged += new System.EventHandler(this.onNewFilename);
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(14, 64);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(305, 54);
            this.label2.TabIndex = 2;
            this.label2.Text = "IF NO FILENAME IS ENTERED, THE ORIGINAL FILENAME IS USED AND OVERWRITES THE ORIGI" +
                "NAL TEMPLATE FILE.";
            // 
            // DoneButton
            // 
            this.DoneButton.Location = new System.Drawing.Point(228, 163);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 23);
            this.DoneButton.TabIndex = 3;
            this.DoneButton.Text = "DONE";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.onDone);
            // 
            // EnterNewFilename
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(331, 199);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.newFileName);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EnterNewFilename";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter New Filename";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox newFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button DoneButton;
    }
}