namespace CruiseProcessing
{
    partial class CapturePercentRemoved
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CapturePercentRemoved));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.finished_Button = new System.Windows.Forms.Button();
            this.PercentRemovedGrid = new System.Windows.Forms.DataGridView();
            this.Species = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Product = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.percentRemoved = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.PercentRemovedGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(345, 64);
            this.label1.TabIndex = 0;
            this.label1.Text = "Biomass has been selected for all or some volume equations.  Percent removed must" +
                " be included for each species/product combination on each equation.";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(12, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(345, 41);
            this.label2.TabIndex = 1;
            this.label2.Text = "Please enter the percent as a whole number.  The default value shown here is 95%." +
                "";
            // 
            // finished_Button
            // 
            this.finished_Button.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.finished_Button.Location = new System.Drawing.Point(260, 265);
            this.finished_Button.Name = "finished_Button";
            this.finished_Button.Size = new System.Drawing.Size(87, 23);
            this.finished_Button.TabIndex = 7;
            this.finished_Button.Text = "FINISHED";
            this.finished_Button.UseVisualStyleBackColor = true;
            this.finished_Button.Click += new System.EventHandler(this.onFinished);
            // 
            // PercentRemovedGrid
            // 
            this.PercentRemovedGrid.AllowUserToAddRows = false;
            this.PercentRemovedGrid.AllowUserToDeleteRows = false;
            this.PercentRemovedGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.PercentRemovedGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Species,
            this.Product,
            this.percentRemoved});
            this.PercentRemovedGrid.Location = new System.Drawing.Point(54, 105);
            this.PercentRemovedGrid.Name = "PercentRemovedGrid";
            this.PercentRemovedGrid.Size = new System.Drawing.Size(236, 150);
            this.PercentRemovedGrid.TabIndex = 8;
            // 
            // Species
            // 
            this.Species.HeaderText = "Species";
            this.Species.Name = "Species";
            this.Species.ReadOnly = true;
            this.Species.Width = 65;
            // 
            // Product
            // 
            this.Product.HeaderText = "Product";
            this.Product.Name = "Product";
            this.Product.ReadOnly = true;
            this.Product.Width = 50;
            // 
            // percentRemoved
            // 
            this.percentRemoved.HeaderText = "Percent removed";
            this.percentRemoved.Name = "percentRemoved";
            this.percentRemoved.Width = 60;
            // 
            // CapturePercentRemoved
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(359, 300);
            this.Controls.Add(this.PercentRemovedGrid);
            this.Controls.Add(this.finished_Button);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "CapturePercentRemoved";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Enter Percent Removed";
            ((System.ComponentModel.ISupportInitialize)(this.PercentRemovedGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button finished_Button;
        private System.Windows.Forms.DataGridView PercentRemovedGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn Species;
        private System.Windows.Forms.DataGridViewTextBoxColumn Product;
        private System.Windows.Forms.DataGridViewTextBoxColumn percentRemoved;

    }
}