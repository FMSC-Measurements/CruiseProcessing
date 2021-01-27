namespace CruiseProcessing
{
    partial class R9VolEquation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R9VolEquation));
            this.taperEquations = new System.Windows.Forms.CheckBox();
            this.oldEquations = new System.Windows.Forms.CheckBox();
            this.topwoodCalculation = new System.Windows.Forms.Button();
            this.topDIB = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.calcBiomass = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // taperEquations
            // 
            this.taperEquations.AutoSize = true;
            this.taperEquations.Checked = true;
            this.taperEquations.CheckState = System.Windows.Forms.CheckState.Checked;
            this.taperEquations.Font = new System.Drawing.Font("Arial", 9.75F);
            this.taperEquations.Location = new System.Drawing.Point(21, 12);
            this.taperEquations.Name = "taperEquations";
            this.taperEquations.Size = new System.Drawing.Size(149, 20);
            this.taperEquations.TabIndex = 0;
            this.taperEquations.Text = "New Taper Equations";
            this.taperEquations.UseVisualStyleBackColor = true;
            this.taperEquations.Click += new System.EventHandler(this.OnClarkChecked);
            // 
            // oldEquations
            // 
            this.oldEquations.Font = new System.Drawing.Font("Arial", 9.75F);
            this.oldEquations.Location = new System.Drawing.Point(21, 51);
            this.oldEquations.Name = "oldEquations";
            this.oldEquations.Size = new System.Drawing.Size(352, 24);
            this.oldEquations.TabIndex = 1;
            this.oldEquations.Text = "Old Equations (but heights must be in FEET, not logs)";
            this.oldEquations.UseVisualStyleBackColor = true;
            this.oldEquations.Click += new System.EventHandler(this.onGevorkCheck);
            // 
            // topwoodCalculation
            // 
            this.topwoodCalculation.AutoSize = true;
            this.topwoodCalculation.Font = new System.Drawing.Font("Arial", 9.75F);
            this.topwoodCalculation.Location = new System.Drawing.Point(34, 91);
            this.topwoodCalculation.Name = "topwoodCalculation";
            this.topwoodCalculation.Size = new System.Drawing.Size(137, 26);
            this.topwoodCalculation.TabIndex = 2;
            this.topwoodCalculation.Text = "Topwood Calculation";
            this.topwoodCalculation.UseVisualStyleBackColor = true;
            this.topwoodCalculation.Click += new System.EventHandler(this.onTopwoodCalculation);
            // 
            // topDIB
            // 
            this.topDIB.Font = new System.Drawing.Font("Arial", 9.75F);
            this.topDIB.Location = new System.Drawing.Point(212, 91);
            this.topDIB.Name = "topDIB";
            this.topDIB.Size = new System.Drawing.Size(137, 26);
            this.topDIB.TabIndex = 3;
            this.topDIB.Text = "Top DIBs";
            this.topDIB.UseVisualStyleBackColor = true;
            this.topDIB.Click += new System.EventHandler(this.onTopDIB);
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(261, 136);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(88, 23);
            this.exitButton.TabIndex = 4;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // calcBiomass
            // 
            this.calcBiomass.AutoSize = true;
            this.calcBiomass.Font = new System.Drawing.Font("Arial", 9.75F);
            this.calcBiomass.Location = new System.Drawing.Point(35, 136);
            this.calcBiomass.Name = "calcBiomass";
            this.calcBiomass.Size = new System.Drawing.Size(136, 20);
            this.calcBiomass.TabIndex = 5;
            this.calcBiomass.Text = "Calculate Biomass";
            this.calcBiomass.UseVisualStyleBackColor = true;
            // 
            // R9VolEquation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(368, 175);
            this.Controls.Add(this.calcBiomass);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.topDIB);
            this.Controls.Add(this.topwoodCalculation);
            this.Controls.Add(this.oldEquations);
            this.Controls.Add(this.taperEquations);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "R9VolEquation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Volume Equation Information";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox taperEquations;
        private System.Windows.Forms.CheckBox oldEquations;
        private System.Windows.Forms.Button topwoodCalculation;
        private System.Windows.Forms.Button topDIB;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.CheckBox calcBiomass;
    }
}