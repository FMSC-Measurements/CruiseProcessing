namespace CruiseProcessing
{
    partial class R8VolEquation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R8VolEquation));
            this.label1 = new System.Windows.Forms.Label();
            this.topwoodCalculation = new System.Windows.Forms.Button();
            this.topDIBS = new System.Windows.Forms.Button();
            this.OK_button = new System.Windows.Forms.Button();
            this.cancel_button = new System.Windows.Forms.Button();
            this.calcBiomass = new System.Windows.Forms.CheckBox();
            this.newClarkCheckBox = new System.Windows.Forms.CheckBox();
            this.oldClarkCheckBox = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(52, 11);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(264, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "REGION 8 VOLUME EQUATIONS";
            // 
            // topwoodCalculation
            // 
            this.topwoodCalculation.AutoSize = true;
            this.topwoodCalculation.Font = new System.Drawing.Font("Arial", 9.75F);
            this.topwoodCalculation.Location = new System.Drawing.Point(111, 136);
            this.topwoodCalculation.Margin = new System.Windows.Forms.Padding(4);
            this.topwoodCalculation.Name = "topwoodCalculation";
            this.topwoodCalculation.Size = new System.Drawing.Size(183, 32);
            this.topwoodCalculation.TabIndex = 3;
            this.topwoodCalculation.Text = "Topwood Calculation";
            this.topwoodCalculation.UseVisualStyleBackColor = true;
            this.topwoodCalculation.Click += new System.EventHandler(this.onTopwoodClick);
            // 
            // topDIBS
            // 
            this.topDIBS.AutoSize = true;
            this.topDIBS.Font = new System.Drawing.Font("Arial", 9.75F);
            this.topDIBS.Location = new System.Drawing.Point(111, 176);
            this.topDIBS.Margin = new System.Windows.Forms.Padding(4);
            this.topDIBS.Name = "topDIBS";
            this.topDIBS.Size = new System.Drawing.Size(183, 32);
            this.topDIBS.TabIndex = 4;
            this.topDIBS.Text = "Top DOBs";
            this.topDIBS.UseVisualStyleBackColor = true;
            this.topDIBS.Click += new System.EventHandler(this.onTopDIBSclick);
            // 
            // OK_button
            // 
            this.OK_button.Font = new System.Drawing.Font("Arial", 9.75F);
            this.OK_button.Location = new System.Drawing.Point(295, 264);
            this.OK_button.Margin = new System.Windows.Forms.Padding(4);
            this.OK_button.Name = "OK_button";
            this.OK_button.Size = new System.Drawing.Size(100, 28);
            this.OK_button.TabIndex = 5;
            this.OK_button.Text = "FINISHED";
            this.OK_button.UseVisualStyleBackColor = true;
            this.OK_button.Click += new System.EventHandler(this.onOK);
            // 
            // cancel_button
            // 
            this.cancel_button.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancel_button.Location = new System.Drawing.Point(19, 264);
            this.cancel_button.Margin = new System.Windows.Forms.Padding(4);
            this.cancel_button.Name = "cancel_button";
            this.cancel_button.Size = new System.Drawing.Size(100, 28);
            this.cancel_button.TabIndex = 6;
            this.cancel_button.Text = "Cancel";
            this.cancel_button.UseVisualStyleBackColor = true;
            this.cancel_button.Click += new System.EventHandler(this.onCancel);
            // 
            // calcBiomass
            // 
            this.calcBiomass.AutoSize = true;
            this.calcBiomass.Font = new System.Drawing.Font("Arial", 9.75F);
            this.calcBiomass.Location = new System.Drawing.Point(112, 227);
            this.calcBiomass.Margin = new System.Windows.Forms.Padding(4);
            this.calcBiomass.Name = "calcBiomass";
            this.calcBiomass.Size = new System.Drawing.Size(165, 23);
            this.calcBiomass.TabIndex = 7;
            this.calcBiomass.Text = "Calculate Biomass";
            this.calcBiomass.UseVisualStyleBackColor = true;
            // 
            // newClarkCheckBox
            // 
            this.newClarkCheckBox.AutoSize = true;
            this.newClarkCheckBox.Checked = true;
            this.newClarkCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.newClarkCheckBox.Font = new System.Drawing.Font("Arial", 9.75F);
            this.newClarkCheckBox.Location = new System.Drawing.Point(112, 51);
            this.newClarkCheckBox.Name = "newClarkCheckBox";
            this.newClarkCheckBox.Size = new System.Drawing.Size(182, 23);
            this.newClarkCheckBox.TabIndex = 8;
            this.newClarkCheckBox.Text = "New Clark Equations";
            this.newClarkCheckBox.UseVisualStyleBackColor = true;
            this.newClarkCheckBox.Click += new System.EventHandler(this.onNewClark);
            // 
            // oldClarkCheckBox
            // 
            this.oldClarkCheckBox.AutoSize = true;
            this.oldClarkCheckBox.Font = new System.Drawing.Font("Arial", 9.75F);
            this.oldClarkCheckBox.Location = new System.Drawing.Point(112, 94);
            this.oldClarkCheckBox.Name = "oldClarkCheckBox";
            this.oldClarkCheckBox.Size = new System.Drawing.Size(175, 23);
            this.oldClarkCheckBox.TabIndex = 9;
            this.oldClarkCheckBox.Text = "Old Clark Equations";
            this.oldClarkCheckBox.UseVisualStyleBackColor = true;
            this.oldClarkCheckBox.Click += new System.EventHandler(this.onOldClark);
            // 
            // R8VolEquation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(408, 308);
            this.Controls.Add(this.oldClarkCheckBox);
            this.Controls.Add(this.newClarkCheckBox);
            this.Controls.Add(this.calcBiomass);
            this.Controls.Add(this.cancel_button);
            this.Controls.Add(this.OK_button);
            this.Controls.Add(this.topDIBS);
            this.Controls.Add(this.topwoodCalculation);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "R8VolEquation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Select Pulpwood Height Measurement";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button topwoodCalculation;
        private System.Windows.Forms.Button topDIBS;
        private System.Windows.Forms.Button OK_button;
        private System.Windows.Forms.Button cancel_button;
        private System.Windows.Forms.CheckBox calcBiomass;
        private System.Windows.Forms.CheckBox newClarkCheckBox;
        private System.Windows.Forms.CheckBox oldClarkCheckBox;
    }
}