namespace CruiseProcessing
{
    partial class selectLength
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(selectLength));
            this.label1 = new System.Windows.Forms.Label();
            this.sixteenLength = new System.Windows.Forms.RadioButton();
            this.thirtyTwoLength = new System.Windows.Forms.RadioButton();
            this.finishedButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(293, 90);
            this.label1.TabIndex = 0;
            this.label1.Text = "GR05 graph can be generated with 16-foot or 32-foot logs.  Click the desired log " +
                "length for this graph.  Default is 16-foot logs.";
            // 
            // sixteenLength
            // 
            this.sixteenLength.AutoSize = true;
            this.sixteenLength.Location = new System.Drawing.Point(45, 102);
            this.sixteenLength.Name = "sixteenLength";
            this.sixteenLength.Size = new System.Drawing.Size(155, 23);
            this.sixteenLength.TabIndex = 1;
            this.sixteenLength.TabStop = true;
            this.sixteenLength.Text = "16-foot log length";
            this.sixteenLength.UseVisualStyleBackColor = true;
            this.sixteenLength.Click += new System.EventHandler(this.onSixteen);
            // 
            // thirtyTwoLength
            // 
            this.thirtyTwoLength.AutoSize = true;
            this.thirtyTwoLength.Location = new System.Drawing.Point(45, 147);
            this.thirtyTwoLength.Name = "thirtyTwoLength";
            this.thirtyTwoLength.Size = new System.Drawing.Size(155, 23);
            this.thirtyTwoLength.TabIndex = 2;
            this.thirtyTwoLength.TabStop = true;
            this.thirtyTwoLength.Text = "32-foot log length";
            this.thirtyTwoLength.UseVisualStyleBackColor = true;
            this.thirtyTwoLength.Click += new System.EventHandler(this.onThirtyTwo);
            // 
            // finishedButton
            // 
            this.finishedButton.Location = new System.Drawing.Point(166, 196);
            this.finishedButton.Name = "finishedButton";
            this.finishedButton.Size = new System.Drawing.Size(110, 35);
            this.finishedButton.TabIndex = 3;
            this.finishedButton.Text = "FINISHED";
            this.finishedButton.UseVisualStyleBackColor = true;
            this.finishedButton.Click += new System.EventHandler(this.onFinished);
            // 
            // selectLength
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(296, 247);
            this.Controls.Add(this.finishedButton);
            this.Controls.Add(this.thirtyTwoLength);
            this.Controls.Add(this.sixteenLength);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 9.75F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "selectLength";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Select log length";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton sixteenLength;
        private System.Windows.Forms.RadioButton thirtyTwoLength;
        private System.Windows.Forms.Button finishedButton;
    }
}