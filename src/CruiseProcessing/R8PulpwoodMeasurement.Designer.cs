namespace CruiseProcessing
{
    partial class R8PulpwoodMeasurement
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R8PulpwoodMeasurement));
            this.label1 = new System.Windows.Forms.Label();
            this.finishButton = new System.Windows.Forms.Button();
            this.pulpwoodHeight = new System.Windows.Forms.ListBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(30, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(266, 46);
            this.label1.TabIndex = 0;
            this.label1.Text = "Click one pulpwood height measurement for these equations.";
            // 
            // finishButton
            // 
            this.finishButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.finishButton.Location = new System.Drawing.Point(237, 148);
            this.finishButton.Name = "finishButton";
            this.finishButton.Size = new System.Drawing.Size(123, 35);
            this.finishButton.TabIndex = 1;
            this.finishButton.Text = "FINISHED";
            this.finishButton.UseVisualStyleBackColor = true;
            this.finishButton.Click += new System.EventHandler(this.onFinished);
            // 
            // pulpwoodHeight
            // 
            this.pulpwoodHeight.Font = new System.Drawing.Font("Arial", 9.75F);
            this.pulpwoodHeight.FormattingEnabled = true;
            this.pulpwoodHeight.ItemHeight = 18;
            this.pulpwoodHeight.Items.AddRange(new object[] {
            "4\" DOB All Species",
            "Total Height All Species",
            "Pine Total Height - Hardwood 4\""});
            this.pulpwoodHeight.Location = new System.Drawing.Point(34, 69);
            this.pulpwoodHeight.Name = "pulpwoodHeight";
            this.pulpwoodHeight.Size = new System.Drawing.Size(253, 58);
            this.pulpwoodHeight.TabIndex = 2;
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(34, 149);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(107, 34);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // R8PulpwoodMeasurement
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 196);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.pulpwoodHeight);
            this.Controls.Add(this.finishButton);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "R8PulpwoodMeasurement";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pulpwood Height Measurement";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button finishButton;
        private System.Windows.Forms.ListBox pulpwoodHeight;
        private System.Windows.Forms.Button cancelButton;
    }
}