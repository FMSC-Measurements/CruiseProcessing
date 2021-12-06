namespace CruiseProcessing
{
    partial class PrintPreview
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PrintPreview));
            this.browse_Button = new System.Windows.Forms.Button();
            this.previewFileName = new System.Windows.Forms.TextBox();
            this.cancel_Button = new System.Windows.Forms.Button();
            this.reportView = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // browse_Button
            // 
            this.browse_Button.Font = new System.Drawing.Font("Arial", 9.75F);
            this.browse_Button.Location = new System.Drawing.Point(24, 18);
            this.browse_Button.Name = "browse_Button";
            this.browse_Button.Size = new System.Drawing.Size(75, 23);
            this.browse_Button.TabIndex = 0;
            this.browse_Button.Text = "Browse...";
            this.browse_Button.UseVisualStyleBackColor = true;
            this.browse_Button.Click += new System.EventHandler(this.onBrowse);
            // 
            // previewFileName
            // 
            this.previewFileName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.previewFileName.Location = new System.Drawing.Point(248, 23);
            this.previewFileName.Name = "previewFileName";
            this.previewFileName.ReadOnly = true;
            this.previewFileName.Size = new System.Drawing.Size(375, 13);
            this.previewFileName.TabIndex = 1;
            // 
            // cancel_Button
            // 
            this.cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel_Button.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancel_Button.Location = new System.Drawing.Point(793, 404);
            this.cancel_Button.Name = "cancel_Button";
            this.cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.cancel_Button.TabIndex = 2;
            this.cancel_Button.Text = "Cancel";
            this.cancel_Button.UseVisualStyleBackColor = true;
            this.cancel_Button.Click += new System.EventHandler(this.onCancel);
            // 
            // reportView
            // 
            this.reportView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.reportView.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.reportView.Location = new System.Drawing.Point(24, 58);
            this.reportView.Name = "reportView";
            this.reportView.ReadOnly = true;
            this.reportView.Size = new System.Drawing.Size(844, 328);
            this.reportView.TabIndex = 3;
            this.reportView.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(105, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "File currently viewed...";
            // 
            // PrintPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(897, 444);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.reportView);
            this.Controls.Add(this.cancel_Button);
            this.Controls.Add(this.previewFileName);
            this.Controls.Add(this.browse_Button);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PrintPreview";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Print Preview";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button browse_Button;
        private System.Windows.Forms.TextBox previewFileName;
        private System.Windows.Forms.Button cancel_Button;
        private System.Windows.Forms.RichTextBox reportView;
        private System.Windows.Forms.Label label1;
    }
}