namespace CruiseProcessing
{
    partial class PDFfileOutput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PDFfileOutput));
            this.label1 = new System.Windows.Forms.Label();
            this.outputFileToConvert = new System.Windows.Forms.TextBox();
            this.browse_Button = new System.Windows.Forms.Button();
            this.convert_Button = new System.Windows.Forms.Button();
            this.PDFoutputFile = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.finished_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.5F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Output file to convert";
            // 
            // outputFileToConvert
            // 
            this.outputFileToConvert.Location = new System.Drawing.Point(12, 28);
            this.outputFileToConvert.Name = "outputFileToConvert";
            this.outputFileToConvert.Size = new System.Drawing.Size(298, 20);
            this.outputFileToConvert.TabIndex = 1;
            // 
            // browse_Button
            // 
            this.browse_Button.Font = new System.Drawing.Font("Arial", 9.5F);
            this.browse_Button.Location = new System.Drawing.Point(325, 26);
            this.browse_Button.Name = "browse_Button";
            this.browse_Button.Size = new System.Drawing.Size(75, 23);
            this.browse_Button.TabIndex = 2;
            this.browse_Button.Text = "Browse...";
            this.browse_Button.UseVisualStyleBackColor = true;
            this.browse_Button.Click += new System.EventHandler(this.onBrowse);
            // 
            // convert_Button
            // 
            this.convert_Button.Font = new System.Drawing.Font("Arial", 9.5F);
            this.convert_Button.Location = new System.Drawing.Point(127, 63);
            this.convert_Button.Name = "convert_Button";
            this.convert_Button.Size = new System.Drawing.Size(116, 23);
            this.convert_Button.TabIndex = 5;
            this.convert_Button.Text = "CONVERT";
            this.convert_Button.UseVisualStyleBackColor = true;
            this.convert_Button.Click += new System.EventHandler(this.onConvert);
            // 
            // PDFoutputFile
            // 
            this.PDFoutputFile.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PDFoutputFile.Location = new System.Drawing.Point(12, 111);
            this.PDFoutputFile.Name = "PDFoutputFile";
            this.PDFoutputFile.ReadOnly = true;
            this.PDFoutputFile.Size = new System.Drawing.Size(385, 13);
            this.PDFoutputFile.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.5F);
            this.label2.Location = new System.Drawing.Point(12, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 16);
            this.label2.TabIndex = 7;
            this.label2.Text = "PDF File";
            // 
            // finished_Button
            // 
            this.finished_Button.Font = new System.Drawing.Font("Arial", 9.5F);
            this.finished_Button.Location = new System.Drawing.Point(325, 140);
            this.finished_Button.Name = "finished_Button";
            this.finished_Button.Size = new System.Drawing.Size(75, 23);
            this.finished_Button.TabIndex = 3;
            this.finished_Button.Text = "FINISHED";
            this.finished_Button.UseVisualStyleBackColor = true;
            this.finished_Button.Click += new System.EventHandler(this.onFinished);
            // 
            // PDFfileOutput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(416, 175);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.PDFoutputFile);
            this.Controls.Add(this.convert_Button);
            this.Controls.Add(this.finished_Button);
            this.Controls.Add(this.browse_Button);
            this.Controls.Add(this.outputFileToConvert);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PDFfileOutput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Create PDF File";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox outputFileToConvert;
        private System.Windows.Forms.Button browse_Button;
        private System.Windows.Forms.Button convert_Button;
        private System.Windows.Forms.TextBox PDFoutputFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button finished_Button;
    }
}