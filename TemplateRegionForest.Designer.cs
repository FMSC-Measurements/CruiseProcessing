namespace CruiseProcessing
{
    partial class TemplateRegionForest
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TemplateRegionForest));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.templateRegion = new System.Windows.Forms.ComboBox();
            this.templateForest = new System.Windows.Forms.TextBox();
            this.DoneButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(17, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(223, 43);
            this.label1.TabIndex = 0;
            this.label1.Text = "Please enter Region and Forest so biomass equations can be updated.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(17, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(58, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "REGION";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(117, 60);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(151, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Enter the Forest Number";
            // 
            // templateRegion
            // 
            this.templateRegion.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.templateRegion.FormattingEnabled = true;
            this.templateRegion.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "06"});
            this.templateRegion.Location = new System.Drawing.Point(20, 79);
            this.templateRegion.Name = "templateRegion";
            this.templateRegion.Size = new System.Drawing.Size(56, 24);
            this.templateRegion.TabIndex = 3;
            this.templateRegion.SelectedIndexChanged += new System.EventHandler(this.onSelectedRegion);
            // 
            // templateForest
            // 
            this.templateForest.Location = new System.Drawing.Point(120, 79);
            this.templateForest.Name = "templateForest";
            this.templateForest.Size = new System.Drawing.Size(76, 20);
            this.templateForest.TabIndex = 4;
            this.templateForest.TextChanged += new System.EventHandler(this.onEnteredForest);
            // 
            // DoneButton
            // 
            this.DoneButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.DoneButton.Location = new System.Drawing.Point(182, 116);
            this.DoneButton.Name = "DoneButton";
            this.DoneButton.Size = new System.Drawing.Size(75, 23);
            this.DoneButton.TabIndex = 6;
            this.DoneButton.Text = "DONE";
            this.DoneButton.UseVisualStyleBackColor = true;
            this.DoneButton.Click += new System.EventHandler(this.onDone);
            // 
            // TemplateRegionForest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(278, 158);
            this.Controls.Add(this.DoneButton);
            this.Controls.Add(this.templateForest);
            this.Controls.Add(this.templateRegion);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "TemplateRegionForest";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Region and Forest";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox templateRegion;
        private System.Windows.Forms.TextBox templateForest;
        private System.Windows.Forms.Button DoneButton;
    }
}