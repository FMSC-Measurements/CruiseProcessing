namespace CruiseProcessing
{
    partial class SpeciesAssociations
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SpeciesAssociations));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.userSpecies = new System.Windows.Forms.ListBox();
            this.availableSpecies = new System.Windows.Forms.ListBox();
            this.associateArrow = new System.Windows.Forms.Button();
            this.removeArrow = new System.Windows.Forms.Button();
            this.associations = new System.Windows.Forms.ListBox();
            this.fractionLeftInWoods = new System.Windows.Forms.ListBox();
            this.damSmallTreesIncluded = new System.Windows.Forms.ListBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(635, 55);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(12, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(635, 36);
            this.label2.TabIndex = 1;
            this.label2.Text = "Additionally, the WT2 report needs the percent of damaged small trees to be inclu" +
                "ded in the report.  Values entered in this column are NOT used in the WT3 report" +
                ".";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label3.Location = new System.Drawing.Point(12, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(635, 43);
            this.label3.TabIndex = 2;
            this.label3.Text = "To make a change to existing associations, click on the association in the Associ" +
                "ations window and then click the left arrow button to remove it.";
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label4.Location = new System.Drawing.Point(12, 120);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(635, 37);
            this.label4.TabIndex = 3;
            this.label4.Text = "The User Species window updates with the user species code and then updates can b" +
                "e made to associations and/or FLIW.";
            // 
            // label5
            // 
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label5.Location = new System.Drawing.Point(10, 157);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(60, 32);
            this.label5.TabIndex = 4;
            this.label5.Text = "User Species";
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label6.Location = new System.Drawing.Point(71, 157);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(62, 39);
            this.label6.TabIndex = 5;
            this.label6.Text = "Available Species";
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label7.Location = new System.Drawing.Point(281, 172);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(83, 17);
            this.label7.TabIndex = 6;
            this.label7.Text = "Associations";
            // 
            // label8
            // 
            this.label8.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label8.Location = new System.Drawing.Point(438, 159);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(92, 34);
            this.label8.TabIndex = 7;
            this.label8.Text = "Fraction Left In Woods";
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label9.Location = new System.Drawing.Point(522, 158);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(123, 35);
            this.label9.TabIndex = 8;
            this.label9.Text = "% Damaged Small Trees Included";
            // 
            // userSpecies
            // 
            this.userSpecies.FormattingEnabled = true;
            this.userSpecies.Location = new System.Drawing.Point(13, 192);
            this.userSpecies.Name = "userSpecies";
            this.userSpecies.Size = new System.Drawing.Size(45, 147);
            this.userSpecies.TabIndex = 9;
            // 
            // availableSpecies
            // 
            this.availableSpecies.FormattingEnabled = true;
            this.availableSpecies.Items.AddRange(new object[] {
            "Grand fir",
            "Subalpine fir",
            "Lodgepole pine",
            "Western larch",
            "Engelmann spruce",
            "Douglas fir",
            "Western hemlock",
            "Western white pine",
            "Western red cedar",
            "Ponderosa pine",
            "White bark pine"});
            this.availableSpecies.Location = new System.Drawing.Point(74, 192);
            this.availableSpecies.Name = "availableSpecies";
            this.availableSpecies.Size = new System.Drawing.Size(120, 147);
            this.availableSpecies.TabIndex = 10;
            // 
            // associateArrow
            // 
            this.associateArrow.Font = new System.Drawing.Font("Arial", 9.75F);
            this.associateArrow.Location = new System.Drawing.Point(211, 234);
            this.associateArrow.Name = "associateArrow";
            this.associateArrow.Size = new System.Drawing.Size(58, 28);
            this.associateArrow.TabIndex = 11;
            this.associateArrow.Text = "-->>>";
            this.associateArrow.UseVisualStyleBackColor = true;
            this.associateArrow.Click += new System.EventHandler(this.onAssociate);
            // 
            // removeArrow
            // 
            this.removeArrow.Font = new System.Drawing.Font("Arial", 9.75F);
            this.removeArrow.Location = new System.Drawing.Point(211, 288);
            this.removeArrow.Name = "removeArrow";
            this.removeArrow.Size = new System.Drawing.Size(58, 28);
            this.removeArrow.TabIndex = 12;
            this.removeArrow.Text = "<<<--";
            this.removeArrow.UseVisualStyleBackColor = true;
            this.removeArrow.Click += new System.EventHandler(this.onRemove);
            // 
            // associations
            // 
            this.associations.FormattingEnabled = true;
            this.associations.Location = new System.Drawing.Point(284, 192);
            this.associations.Name = "associations";
            this.associations.Size = new System.Drawing.Size(140, 147);
            this.associations.TabIndex = 13;
            // 
            // fractionLeftInWoods
            // 
            this.fractionLeftInWoods.FormattingEnabled = true;
            this.fractionLeftInWoods.Location = new System.Drawing.Point(441, 192);
            this.fractionLeftInWoods.Name = "fractionLeftInWoods";
            this.fractionLeftInWoods.Size = new System.Drawing.Size(45, 147);
            this.fractionLeftInWoods.TabIndex = 14;
            // 
            // damSmallTreesIncluded
            // 
            this.damSmallTreesIncluded.FormattingEnabled = true;
            this.damSmallTreesIncluded.Location = new System.Drawing.Point(525, 192);
            this.damSmallTreesIncluded.Name = "damSmallTreesIncluded";
            this.damSmallTreesIncluded.Size = new System.Drawing.Size(45, 147);
            this.damSmallTreesIncluded.TabIndex = 15;
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(13, 350);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(89, 30);
            this.cancelButton.TabIndex = 16;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(541, 350);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(89, 30);
            this.exitButton.TabIndex = 17;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // SpeciesAssociations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(653, 392);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.damSmallTreesIncluded);
            this.Controls.Add(this.fractionLeftInWoods);
            this.Controls.Add(this.associations);
            this.Controls.Add(this.removeArrow);
            this.Controls.Add(this.associateArrow);
            this.Controls.Add(this.availableSpecies);
            this.Controls.Add(this.userSpecies);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SpeciesAssociations";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Species Associations";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListBox userSpecies;
        private System.Windows.Forms.ListBox availableSpecies;
        private System.Windows.Forms.Button associateArrow;
        private System.Windows.Forms.Button removeArrow;
        private System.Windows.Forms.ListBox associations;
        private System.Windows.Forms.ListBox fractionLeftInWoods;
        private System.Windows.Forms.ListBox damSmallTreesIncluded;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button exitButton;
    }
}