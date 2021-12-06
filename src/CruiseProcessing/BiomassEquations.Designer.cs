namespace CruiseProcessing
{
    partial class BiomassEquations
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BiomassEquations));
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.biomassEquationList = new System.Windows.Forms.DataGridView();
            this.validatorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.persisterDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.speciesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.productDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.componentDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.equationDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.percentMoistureDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.percentRemovedDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.metaDataDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rowIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dALDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isPersistedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.errorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.livedead = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.wgtFactor = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.biomassEquationDOBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.exitButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.insertButton = new System.Windows.Forms.Button();
            this.speciesList = new System.Windows.Forms.ComboBox();
            this.equationList = new System.Windows.Forms.ComboBox();
            this.componentList = new System.Windows.Forms.ListBox();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.wgt_factors = new System.Windows.Forms.ComboBox();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.biomassEquationList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.biomassEquationDOBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(177, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(424, 85);
            this.label2.TabIndex = 1;
            this.label2.Text = resources.GetString("label2.Text");
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(29, 74);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 74);
            this.label3.TabIndex = 2;
            this.label3.Text = "SPECIES PRODUCT LIVE/DEAD COMBINATIONS";
            // 
            // biomassEquationList
            // 
            this.biomassEquationList.AutoGenerateColumns = false;
            this.biomassEquationList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.biomassEquationList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.validatorDataGridViewTextBoxColumn,
            this.persisterDataGridViewTextBoxColumn,
            this.speciesDataGridViewTextBoxColumn,
            this.productDataGridViewTextBoxColumn,
            this.componentDataGridViewTextBoxColumn,
            this.equationDataGridViewTextBoxColumn,
            this.percentMoistureDataGridViewTextBoxColumn,
            this.percentRemovedDataGridViewTextBoxColumn,
            this.metaDataDataGridViewTextBoxColumn,
            this.rowIDDataGridViewTextBoxColumn,
            this.dALDataGridViewTextBoxColumn,
            this.isPersistedDataGridViewCheckBoxColumn,
            this.errorDataGridViewTextBoxColumn,
            this.livedead,
            this.wgtFactor,
            this.tagDataGridViewTextBoxColumn});
            this.biomassEquationList.DataSource = this.biomassEquationDOBindingSource;
            this.biomassEquationList.Location = new System.Drawing.Point(22, 243);
            this.biomassEquationList.Name = "biomassEquationList";
            this.biomassEquationList.Size = new System.Drawing.Size(555, 163);
            this.biomassEquationList.TabIndex = 25;
            // 
            // validatorDataGridViewTextBoxColumn
            // 
            this.validatorDataGridViewTextBoxColumn.DataPropertyName = "Validator";
            this.validatorDataGridViewTextBoxColumn.HeaderText = "Validator";
            this.validatorDataGridViewTextBoxColumn.Name = "validatorDataGridViewTextBoxColumn";
            this.validatorDataGridViewTextBoxColumn.ReadOnly = true;
            this.validatorDataGridViewTextBoxColumn.Visible = false;
            this.validatorDataGridViewTextBoxColumn.Width = 65;
            // 
            // persisterDataGridViewTextBoxColumn
            // 
            this.persisterDataGridViewTextBoxColumn.DataPropertyName = "Persister";
            this.persisterDataGridViewTextBoxColumn.HeaderText = "Persister";
            this.persisterDataGridViewTextBoxColumn.Name = "persisterDataGridViewTextBoxColumn";
            this.persisterDataGridViewTextBoxColumn.ReadOnly = true;
            this.persisterDataGridViewTextBoxColumn.Visible = false;
            // 
            // speciesDataGridViewTextBoxColumn
            // 
            this.speciesDataGridViewTextBoxColumn.DataPropertyName = "Species";
            this.speciesDataGridViewTextBoxColumn.HeaderText = "Species";
            this.speciesDataGridViewTextBoxColumn.Name = "speciesDataGridViewTextBoxColumn";
            this.speciesDataGridViewTextBoxColumn.Width = 55;
            // 
            // productDataGridViewTextBoxColumn
            // 
            this.productDataGridViewTextBoxColumn.DataPropertyName = "Product";
            this.productDataGridViewTextBoxColumn.HeaderText = "Product";
            this.productDataGridViewTextBoxColumn.Name = "productDataGridViewTextBoxColumn";
            this.productDataGridViewTextBoxColumn.Width = 50;
            // 
            // componentDataGridViewTextBoxColumn
            // 
            this.componentDataGridViewTextBoxColumn.DataPropertyName = "Component";
            this.componentDataGridViewTextBoxColumn.HeaderText = "Component";
            this.componentDataGridViewTextBoxColumn.Name = "componentDataGridViewTextBoxColumn";
            this.componentDataGridViewTextBoxColumn.Width = 85;
            // 
            // equationDataGridViewTextBoxColumn
            // 
            this.equationDataGridViewTextBoxColumn.DataPropertyName = "Equation";
            this.equationDataGridViewTextBoxColumn.HeaderText = "Equation";
            this.equationDataGridViewTextBoxColumn.Name = "equationDataGridViewTextBoxColumn";
            this.equationDataGridViewTextBoxColumn.Width = 55;
            // 
            // percentMoistureDataGridViewTextBoxColumn
            // 
            this.percentMoistureDataGridViewTextBoxColumn.DataPropertyName = "PercentMoisture";
            this.percentMoistureDataGridViewTextBoxColumn.HeaderText = "Percent Moisture";
            this.percentMoistureDataGridViewTextBoxColumn.Name = "percentMoistureDataGridViewTextBoxColumn";
            this.percentMoistureDataGridViewTextBoxColumn.Width = 65;
            // 
            // percentRemovedDataGridViewTextBoxColumn
            // 
            this.percentRemovedDataGridViewTextBoxColumn.DataPropertyName = "PercentRemoved";
            this.percentRemovedDataGridViewTextBoxColumn.HeaderText = "Percent Removed";
            this.percentRemovedDataGridViewTextBoxColumn.Name = "percentRemovedDataGridViewTextBoxColumn";
            this.percentRemovedDataGridViewTextBoxColumn.Width = 65;
            // 
            // metaDataDataGridViewTextBoxColumn
            // 
            this.metaDataDataGridViewTextBoxColumn.DataPropertyName = "MetaData";
            this.metaDataDataGridViewTextBoxColumn.HeaderText = "MetaData";
            this.metaDataDataGridViewTextBoxColumn.Name = "metaDataDataGridViewTextBoxColumn";
            this.metaDataDataGridViewTextBoxColumn.Width = 150;
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
            // livedead
            // 
            this.livedead.HeaderText = "Live/ Dead";
            this.livedead.Name = "livedead";
            this.livedead.Width = 35;
            // 
            // wgtFactor
            // 
            this.wgtFactor.HeaderText = "Weight Factor";
            this.wgtFactor.Name = "wgtFactor";
            this.wgtFactor.Width = 55;
            // 
            // tagDataGridViewTextBoxColumn
            // 
            this.tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            this.tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            this.tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            this.tagDataGridViewTextBoxColumn.Visible = false;
            // 
            // biomassEquationDOBindingSource
            // 
            this.biomassEquationDOBindingSource.DataSource = typeof(CruiseDAL.DataObjects.BiomassEquationDO);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(511, 415);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(81, 30);
            this.exitButton.TabIndex = 28;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(14, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(149, 16);
            this.label1.TabIndex = 29;
            this.label1.Text = "BIOMASS EQUATIONS";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(151, 94);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(91, 16);
            this.label5.TabIndex = 31;
            this.label5.Text = "COMPONENT";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(361, 94);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(75, 16);
            this.label6.TabIndex = 32;
            this.label6.Text = "EQUATION";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(412, 415);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 30);
            this.cancelButton.TabIndex = 34;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // insertButton
            // 
            this.insertButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.insertButton.Location = new System.Drawing.Point(496, 207);
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(81, 30);
            this.insertButton.TabIndex = 35;
            this.insertButton.Text = "INSERT";
            this.insertButton.UseVisualStyleBackColor = true;
            this.insertButton.Click += new System.EventHandler(this.onInsertClick);
            // 
            // speciesList
            // 
            this.speciesList.FormattingEnabled = true;
            this.speciesList.Location = new System.Drawing.Point(32, 151);
            this.speciesList.Name = "speciesList";
            this.speciesList.Size = new System.Drawing.Size(95, 21);
            this.speciesList.TabIndex = 36;
            this.speciesList.SelectedIndexChanged += new System.EventHandler(this.onSpeciesSelectIndexChanged);
            // 
            // equationList
            // 
            this.equationList.FormattingEnabled = true;
            this.equationList.Location = new System.Drawing.Point(364, 113);
            this.equationList.Name = "equationList";
            this.equationList.Size = new System.Drawing.Size(87, 21);
            this.equationList.TabIndex = 38;
            this.equationList.SelectedIndexChanged += new System.EventHandler(this.equationSelectIndexChanged);
            // 
            // componentList
            // 
            this.componentList.FormattingEnabled = true;
            this.componentList.Items.AddRange(new object[] {
            "Use Defaults for all components",
            "Total Tree",
            "Live Branches",
            "Dead Branches",
            "Foliage",
            "Tip",
            "Mainstem Sawtimber",
            "Mainstem Topwood"});
            this.componentList.Location = new System.Drawing.Point(154, 113);
            this.componentList.Name = "componentList";
            this.componentList.Size = new System.Drawing.Size(159, 108);
            this.componentList.TabIndex = 40;
            this.componentList.SelectedIndexChanged += new System.EventHandler(this.componentSelectIndexChanged);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn1.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Visible = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(361, 151);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(117, 16);
            this.label4.TabIndex = 41;
            this.label4.Text = "WEIGHT FACTOR";
            // 
            // wgt_factors
            // 
            this.wgt_factors.FormattingEnabled = true;
            this.wgt_factors.Location = new System.Drawing.Point(364, 171);
            this.wgt_factors.Name = "wgt_factors";
            this.wgt_factors.Size = new System.Drawing.Size(87, 21);
            this.wgt_factors.TabIndex = 42;
            this.wgt_factors.SelectedIndexChanged += new System.EventHandler(this.wgtFactorSelectIndexChanged);
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn2.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Visible = false;
            // 
            // BiomassEquations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(604, 457);
            this.Controls.Add(this.wgt_factors);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.componentList);
            this.Controls.Add(this.equationList);
            this.Controls.Add(this.speciesList);
            this.Controls.Add(this.insertButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.biomassEquationList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BiomassEquations";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Biomass Equations";
            ((System.ComponentModel.ISupportInitialize)(this.biomassEquationList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.biomassEquationDOBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridView biomassEquationList;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button insertButton;
        private System.Windows.Forms.ComboBox speciesList;
        private System.Windows.Forms.ComboBox equationList;
        private System.Windows.Forms.BindingSource biomassEquationDOBindingSource;
        private System.Windows.Forms.ListBox componentList;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox wgt_factors;
        private System.Windows.Forms.DataGridViewTextBoxColumn validatorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn persisterDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn speciesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn productDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn livedead;
        private System.Windows.Forms.DataGridViewTextBoxColumn componentDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn equationDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn percentMoistureDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn percentRemovedDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn wgtFactor;
        private System.Windows.Forms.DataGridViewTextBoxColumn metaDataDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rowIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dALDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isPersistedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tagDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;

    }
}