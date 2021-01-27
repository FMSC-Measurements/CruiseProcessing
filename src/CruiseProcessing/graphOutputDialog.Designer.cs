namespace CruiseProcessing
{
    partial class graphOutputDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(graphOutputDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.OK_button = new System.Windows.Forms.Button();
            this.Cancel_button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(260, 59);
            this.label1.TabIndex = 0;
            this.label1.Text = "Some graph reports have been selected and need to be created here since they cann" +
                "ot be placed in the text output file.";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(12, 68);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(259, 88);
            this.label2.TabIndex = 1;
            this.label2.Text = "Each graph report is placed in a JPG file in the same directory as the output fil" +
                "e.  Once this JPG file is created, the PDF file must be created so the graphs ca" +
                "n be printed if desired.";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label3.Location = new System.Drawing.Point(15, 160);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(257, 84);
            this.label3.TabIndex = 2;
            this.label3.Text = "To create the graph reports, click the CONTINUE button.  Each graph will be displ" +
                "ayed for review.  Close the graph display by clicking the X button in the upper " +
                "right corner.";
            // 
            // OK_button
            // 
            this.OK_button.AutoSize = true;
            this.OK_button.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.OK_button.Location = new System.Drawing.Point(15, 256);
            this.OK_button.Name = "OK_button";
            this.OK_button.Size = new System.Drawing.Size(84, 26);
            this.OK_button.TabIndex = 3;
            this.OK_button.Text = "CONTINUE";
            this.OK_button.UseVisualStyleBackColor = true;
            this.OK_button.Click += new System.EventHandler(this.onContinue);
            // 
            // Cancel_button
            // 
            this.Cancel_button.AutoSize = true;
            this.Cancel_button.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.Cancel_button.Location = new System.Drawing.Point(167, 256);
            this.Cancel_button.Name = "Cancel_button";
            this.Cancel_button.Size = new System.Drawing.Size(88, 26);
            this.Cancel_button.TabIndex = 4;
            this.Cancel_button.Text = "No, Thanks";
            this.Cancel_button.UseVisualStyleBackColor = true;
            this.Cancel_button.Click += new System.EventHandler(this.onCancel);
            // 
            // graphOutputDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 295);
            this.Controls.Add(this.Cancel_button);
            this.Controls.Add(this.OK_button);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "graphOutputDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Graph Output";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button OK_button;
        private System.Windows.Forms.Button Cancel_button;
    }
}