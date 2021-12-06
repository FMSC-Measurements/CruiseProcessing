namespace CruiseProcessing
{
    partial class StewardshipProductCosts
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StewardshipProductCosts));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.StewardCosts = new System.Windows.Forms.DataGridView();
            this.costUnitDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.costSpeciesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.costProductDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.costPoundsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.costCostDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scalePCDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.includeInReportDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.stewProductCostsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.StewardCosts)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.stewProductCostsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(420, 39);
            this.label1.TabIndex = 0;
            this.label1.Text = "In the list below, enter each cutting unit, species and product group with values" +
                " as appropriate.  Scale Defect % is for Sawtimber Only.";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(361, 23);
            this.label2.TabIndex = 1;
            this.label2.Text = "Be sure to check the box to include this group in the report.";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(259, 284);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(357, 284);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 3;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // StewardCosts
            // 
            this.StewardCosts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.StewardCosts.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.StewardCosts.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.StewardCosts.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.costUnitDataGridViewTextBoxColumn,
            this.costSpeciesDataGridViewTextBoxColumn,
            this.costProductDataGridViewTextBoxColumn,
            this.costPoundsDataGridViewTextBoxColumn,
            this.costCostDataGridViewTextBoxColumn,
            this.scalePCDataGridViewTextBoxColumn,
            this.includeInReportDataGridViewTextBoxColumn});
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.StewardCosts.DefaultCellStyle = dataGridViewCellStyle2;
            this.StewardCosts.Location = new System.Drawing.Point(12, 74);
            this.StewardCosts.Name = "StewardCosts";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Control;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.StewardCosts.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.StewardCosts.Size = new System.Drawing.Size(433, 192);
            this.StewardCosts.TabIndex = 4;
            // 
            // costUnitDataGridViewTextBoxColumn
            // 
            this.costUnitDataGridViewTextBoxColumn.DataPropertyName = "costUnit";
            this.costUnitDataGridViewTextBoxColumn.HeaderText = "Cutting Unit";
            this.costUnitDataGridViewTextBoxColumn.Name = "costUnitDataGridViewTextBoxColumn";
            this.costUnitDataGridViewTextBoxColumn.Width = 50;
            // 
            // costSpeciesDataGridViewTextBoxColumn
            // 
            this.costSpeciesDataGridViewTextBoxColumn.DataPropertyName = "costSpecies";
            this.costSpeciesDataGridViewTextBoxColumn.HeaderText = "Species";
            this.costSpeciesDataGridViewTextBoxColumn.Name = "costSpeciesDataGridViewTextBoxColumn";
            this.costSpeciesDataGridViewTextBoxColumn.Width = 50;
            // 
            // costProductDataGridViewTextBoxColumn
            // 
            this.costProductDataGridViewTextBoxColumn.DataPropertyName = "costProduct";
            this.costProductDataGridViewTextBoxColumn.HeaderText = "Product";
            this.costProductDataGridViewTextBoxColumn.Name = "costProductDataGridViewTextBoxColumn";
            this.costProductDataGridViewTextBoxColumn.Width = 50;
            // 
            // costPoundsDataGridViewTextBoxColumn
            // 
            this.costPoundsDataGridViewTextBoxColumn.DataPropertyName = "costPounds";
            this.costPoundsDataGridViewTextBoxColumn.HeaderText = "Pounds per Cubic Foot";
            this.costPoundsDataGridViewTextBoxColumn.Name = "costPoundsDataGridViewTextBoxColumn";
            this.costPoundsDataGridViewTextBoxColumn.Width = 65;
            // 
            // costCostDataGridViewTextBoxColumn
            // 
            this.costCostDataGridViewTextBoxColumn.DataPropertyName = "costCost";
            this.costCostDataGridViewTextBoxColumn.HeaderText = "Cost";
            this.costCostDataGridViewTextBoxColumn.Name = "costCostDataGridViewTextBoxColumn";
            this.costCostDataGridViewTextBoxColumn.Width = 65;
            // 
            // scalePCDataGridViewTextBoxColumn
            // 
            this.scalePCDataGridViewTextBoxColumn.DataPropertyName = "scalePC";
            this.scalePCDataGridViewTextBoxColumn.HeaderText = "Scale Defect %";
            this.scalePCDataGridViewTextBoxColumn.Name = "scalePCDataGridViewTextBoxColumn";
            this.scalePCDataGridViewTextBoxColumn.Width = 50;
            // 
            // includeInReportDataGridViewTextBoxColumn
            // 
            this.includeInReportDataGridViewTextBoxColumn.DataPropertyName = "includeInReport";
            this.includeInReportDataGridViewTextBoxColumn.HeaderText = "Include";
            this.includeInReportDataGridViewTextBoxColumn.Name = "includeInReportDataGridViewTextBoxColumn";
            this.includeInReportDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.includeInReportDataGridViewTextBoxColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.includeInReportDataGridViewTextBoxColumn.Width = 50;
            // 
            // stewProductCostsBindingSource
            // 
            this.stewProductCostsBindingSource.DataSource = typeof(CruiseProcessing.StewProductCosts);
            // 
            // StewardshipProductCosts
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(473, 319);
            this.Controls.Add(this.StewardCosts);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.Name = "StewardshipProductCosts";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Stewardship Product Costs";
            ((System.ComponentModel.ISupportInitialize)(this.StewardCosts)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.stewProductCostsBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.DataGridView StewardCosts;
        private System.Windows.Forms.DataGridViewTextBoxColumn costUnitDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn costSpeciesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn costProductDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn costPoundsDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn costCostDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn scalePCDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn includeInReportDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource stewProductCostsBindingSource;
    }
}