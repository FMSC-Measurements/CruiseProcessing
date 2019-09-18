namespace CruiseProcessing
{
    partial class ModifyWeightFactors
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ModifyWeightFactors));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.biomassFactorsList = new System.Windows.Forms.DataGridView();
            this.biomassEquationDOBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.finished_Button = new System.Windows.Forms.Button();
            this.cancel_Button = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.validatorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.speciesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.productDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.componentDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.liveDeadDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.fIAcodeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.equationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.weightFactorPrimaryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.weightFactorSecondaryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.percentMoistureDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.percentRemovedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metaDataDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rowIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dALDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isPersistedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.errorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.biomassFactorsList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.biomassEquationDOBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(501, 38);
            this.label1.TabIndex = 0;
            this.label1.Text = "Modification of the items shown here applies ONLY to the current sale.  Changes m" +
                "ade do NOT affect regional defaults.";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(12, 47);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(501, 19);
            this.label2.TabIndex = 1;
            this.label2.Text = "Changes may be made to the weight factor, percent moisture and percent removed.";
            // 
            // biomassFactorsList
            // 
            this.biomassFactorsList.AllowUserToAddRows = false;
            this.biomassFactorsList.AllowUserToDeleteRows = false;
            this.biomassFactorsList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.biomassFactorsList.AutoGenerateColumns = false;
            this.biomassFactorsList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.biomassFactorsList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.validatorDataGridViewTextBoxColumn,
            this.speciesDataGridViewTextBoxColumn,
            this.productDataGridViewTextBoxColumn,
            this.componentDataGridViewTextBoxColumn,
            this.liveDeadDataGridViewTextBoxColumn,
            this.fIAcodeDataGridViewTextBoxColumn,
            this.equationDataGridViewTextBoxColumn,
            this.weightFactorPrimaryDataGridViewTextBoxColumn,
            this.weightFactorSecondaryDataGridViewTextBoxColumn,
            this.percentMoistureDataGridViewTextBoxColumn,
            this.percentRemovedDataGridViewTextBoxColumn,
            this.metaDataDataGridViewTextBoxColumn,
            this.rowIDDataGridViewTextBoxColumn,
            this.dALDataGridViewTextBoxColumn,
            this.isPersistedDataGridViewCheckBoxColumn,
            this.errorDataGridViewTextBoxColumn,
            this.tagDataGridViewTextBoxColumn});
            this.biomassFactorsList.DataSource = this.biomassEquationDOBindingSource;
            this.biomassFactorsList.Location = new System.Drawing.Point(15, 69);
            this.biomassFactorsList.Name = "biomassFactorsList";
            this.biomassFactorsList.Size = new System.Drawing.Size(560, 150);
            this.biomassFactorsList.TabIndex = 2;
            // 
            // biomassEquationDOBindingSource
            // 
            this.biomassEquationDOBindingSource.DataSource = typeof(CruiseDAL.DataObjects.BiomassEquationDO);
            // 
            // finished_Button
            // 
            this.finished_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.finished_Button.Font = new System.Drawing.Font("Arial", 9.75F);
            this.finished_Button.Location = new System.Drawing.Point(499, 234);
            this.finished_Button.Name = "finished_Button";
            this.finished_Button.Size = new System.Drawing.Size(76, 23);
            this.finished_Button.TabIndex = 3;
            this.finished_Button.Text = "FINISHED";
            this.finished_Button.UseVisualStyleBackColor = true;
            this.finished_Button.Click += new System.EventHandler(this.onFinished);
            // 
            // cancel_Button
            // 
            this.cancel_Button.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancel_Button.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancel_Button.Location = new System.Drawing.Point(405, 234);
            this.cancel_Button.Name = "cancel_Button";
            this.cancel_Button.Size = new System.Drawing.Size(75, 23);
            this.cancel_Button.TabIndex = 4;
            this.cancel_Button.Text = "Cancel";
            this.cancel_Button.UseVisualStyleBackColor = true;
            this.cancel_Button.Click += new System.EventHandler(this.onCancel);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn1.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Visible = false;
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn2.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Visible = false;
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn3.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.Visible = false;
            // 
            // validatorDataGridViewTextBoxColumn
            // 
            this.validatorDataGridViewTextBoxColumn.DataPropertyName = "Validator";
            this.validatorDataGridViewTextBoxColumn.HeaderText = "Validator";
            this.validatorDataGridViewTextBoxColumn.Name = "validatorDataGridViewTextBoxColumn";
            this.validatorDataGridViewTextBoxColumn.ReadOnly = true;
            this.validatorDataGridViewTextBoxColumn.Visible = false;
            // 
            // speciesDataGridViewTextBoxColumn
            // 
            this.speciesDataGridViewTextBoxColumn.DataPropertyName = "Species";
            this.speciesDataGridViewTextBoxColumn.HeaderText = "Species";
            this.speciesDataGridViewTextBoxColumn.Name = "speciesDataGridViewTextBoxColumn";
            this.speciesDataGridViewTextBoxColumn.ReadOnly = true;
            this.speciesDataGridViewTextBoxColumn.Width = 65;
            // 
            // productDataGridViewTextBoxColumn
            // 
            this.productDataGridViewTextBoxColumn.DataPropertyName = "Product";
            this.productDataGridViewTextBoxColumn.HeaderText = "Product";
            this.productDataGridViewTextBoxColumn.Name = "productDataGridViewTextBoxColumn";
            this.productDataGridViewTextBoxColumn.ReadOnly = true;
            this.productDataGridViewTextBoxColumn.Width = 65;
            // 
            // componentDataGridViewTextBoxColumn
            // 
            this.componentDataGridViewTextBoxColumn.DataPropertyName = "Component";
            this.componentDataGridViewTextBoxColumn.HeaderText = "Component";
            this.componentDataGridViewTextBoxColumn.Name = "componentDataGridViewTextBoxColumn";
            this.componentDataGridViewTextBoxColumn.ReadOnly = true;
            this.componentDataGridViewTextBoxColumn.Width = 125;
            // 
            // liveDeadDataGridViewTextBoxColumn
            // 
            this.liveDeadDataGridViewTextBoxColumn.DataPropertyName = "LiveDead";
            this.liveDeadDataGridViewTextBoxColumn.HeaderText = "Live Dead";
            this.liveDeadDataGridViewTextBoxColumn.Name = "liveDeadDataGridViewTextBoxColumn";
            this.liveDeadDataGridViewTextBoxColumn.ReadOnly = true;
            this.liveDeadDataGridViewTextBoxColumn.Width = 45;
            // 
            // fIAcodeDataGridViewTextBoxColumn
            // 
            this.fIAcodeDataGridViewTextBoxColumn.DataPropertyName = "FIAcode";
            this.fIAcodeDataGridViewTextBoxColumn.HeaderText = "FIAcode";
            this.fIAcodeDataGridViewTextBoxColumn.Name = "fIAcodeDataGridViewTextBoxColumn";
            this.fIAcodeDataGridViewTextBoxColumn.Visible = false;
            // 
            // equationDataGridViewTextBoxColumn
            // 
            this.equationDataGridViewTextBoxColumn.DataPropertyName = "Equation";
            this.equationDataGridViewTextBoxColumn.HeaderText = "Equation";
            this.equationDataGridViewTextBoxColumn.Name = "equationDataGridViewTextBoxColumn";
            this.equationDataGridViewTextBoxColumn.Visible = false;
            // 
            // weightFactorPrimaryDataGridViewTextBoxColumn
            // 
            this.weightFactorPrimaryDataGridViewTextBoxColumn.DataPropertyName = "WeightFactorPrimary";
            this.weightFactorPrimaryDataGridViewTextBoxColumn.HeaderText = "Weight Factor Primary";
            this.weightFactorPrimaryDataGridViewTextBoxColumn.Name = "weightFactorPrimaryDataGridViewTextBoxColumn";
            this.weightFactorPrimaryDataGridViewTextBoxColumn.Width = 50;
            // 
            // weightFactorSecondaryDataGridViewTextBoxColumn
            // 
            this.weightFactorSecondaryDataGridViewTextBoxColumn.DataPropertyName = "WeightFactorSecondary";
            this.weightFactorSecondaryDataGridViewTextBoxColumn.HeaderText = "Weight Factor Secondary";
            this.weightFactorSecondaryDataGridViewTextBoxColumn.Name = "weightFactorSecondaryDataGridViewTextBoxColumn";
            this.weightFactorSecondaryDataGridViewTextBoxColumn.Width = 60;
            // 
            // percentMoistureDataGridViewTextBoxColumn
            // 
            this.percentMoistureDataGridViewTextBoxColumn.DataPropertyName = "PercentMoisture";
            this.percentMoistureDataGridViewTextBoxColumn.HeaderText = "Percent Moisture";
            this.percentMoistureDataGridViewTextBoxColumn.Name = "percentMoistureDataGridViewTextBoxColumn";
            this.percentMoistureDataGridViewTextBoxColumn.Width = 50;
            // 
            // percentRemovedDataGridViewTextBoxColumn
            // 
            this.percentRemovedDataGridViewTextBoxColumn.DataPropertyName = "PercentRemoved";
            this.percentRemovedDataGridViewTextBoxColumn.HeaderText = "Percent Removed";
            this.percentRemovedDataGridViewTextBoxColumn.Name = "percentRemovedDataGridViewTextBoxColumn";
            this.percentRemovedDataGridViewTextBoxColumn.Width = 55;
            // 
            // metaDataDataGridViewTextBoxColumn
            // 
            this.metaDataDataGridViewTextBoxColumn.DataPropertyName = "MetaData";
            this.metaDataDataGridViewTextBoxColumn.HeaderText = "MetaData";
            this.metaDataDataGridViewTextBoxColumn.Name = "metaDataDataGridViewTextBoxColumn";
            this.metaDataDataGridViewTextBoxColumn.Visible = false;
            // 
            // rowIDDataGridViewTextBoxColumn
            // 
            this.rowIDDataGridViewTextBoxColumn.DataPropertyName = "rowID";
            this.rowIDDataGridViewTextBoxColumn.HeaderText = "rowID";
            this.rowIDDataGridViewTextBoxColumn.Name = "rowIDDataGridViewTextBoxColumn";
            this.rowIDDataGridViewTextBoxColumn.Visible = false;
            // 
            // dALDataGridViewTextBoxColumn
            // 
            this.dALDataGridViewTextBoxColumn.DataPropertyName = "DAL";
            this.dALDataGridViewTextBoxColumn.HeaderText = "DAL";
            this.dALDataGridViewTextBoxColumn.Name = "dALDataGridViewTextBoxColumn";
            this.dALDataGridViewTextBoxColumn.Visible = false;
            // 
            // isPersistedDataGridViewCheckBoxColumn
            // 
            this.isPersistedDataGridViewCheckBoxColumn.DataPropertyName = "IsPersisted";
            this.isPersistedDataGridViewCheckBoxColumn.HeaderText = "IsPersisted";
            this.isPersistedDataGridViewCheckBoxColumn.Name = "isPersistedDataGridViewCheckBoxColumn";
            this.isPersistedDataGridViewCheckBoxColumn.ReadOnly = true;
            this.isPersistedDataGridViewCheckBoxColumn.Visible = false;
            // 
            // errorDataGridViewTextBoxColumn
            // 
            this.errorDataGridViewTextBoxColumn.DataPropertyName = "Error";
            this.errorDataGridViewTextBoxColumn.HeaderText = "Error";
            this.errorDataGridViewTextBoxColumn.Name = "errorDataGridViewTextBoxColumn";
            this.errorDataGridViewTextBoxColumn.ReadOnly = true;
            this.errorDataGridViewTextBoxColumn.Visible = false;
            // 
            // tagDataGridViewTextBoxColumn
            // 
            this.tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            this.tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            this.tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            this.tagDataGridViewTextBoxColumn.Visible = false;
            // 
            // ModifyWeightFactors
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 271);
            this.Controls.Add(this.cancel_Button);
            this.Controls.Add(this.finished_Button);
            this.Controls.Add(this.biomassFactorsList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ModifyWeightFactors";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Modify Weight Factors";
            ((System.ComponentModel.ISupportInitialize)(this.biomassFactorsList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.biomassEquationDOBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.DataGridView biomassFactorsList;
        private System.Windows.Forms.BindingSource biomassEquationDOBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Button finished_Button;
        private System.Windows.Forms.Button cancel_Button;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        //private System.Windows.Forms.DataGridViewTextBoxColumn persisterDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn validatorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn speciesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn productDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn componentDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn liveDeadDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn fIAcodeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn equationDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn weightFactorPrimaryDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn weightFactorSecondaryDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn percentMoistureDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn percentRemovedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn metaDataDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rowIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dALDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isPersistedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tagDataGridViewTextBoxColumn;
    }
}