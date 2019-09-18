namespace CruiseProcessing
{
    partial class WeightEquations
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WeightEquations));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.weightEquationList = new System.Windows.Forms.DataGridView();
            this.validatorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.persisterDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.speciesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.liveDeadDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.primaryProductDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.weightFactorPrimaryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.percentRemovedPrimaryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.secondaryProductDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.weightFactorSecondaryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.percentRemovedSecondaryDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rowIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dALDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isPersistedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.errorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.weightEquationDOBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.dataGridViewTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cancelButton = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn11 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn12 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.speciesList = new System.Windows.Forms.ComboBox();
            this.liveDeadList = new System.Windows.Forms.ComboBox();
            this.primaryProdList = new System.Windows.Forms.ComboBox();
            this.insertButton = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn13 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.weightEquationList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightEquationDOBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(19, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(140, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "WEIGHT EQUATIONS";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(61, 38);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(304, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Only one weight equation is available -- GENPP004";
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label7.Location = new System.Drawing.Point(61, 54);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(507, 51);
            this.label7.TabIndex = 8;
            this.label7.Text = "For each species code, select a Live/Dead code, enter a primary product code, wei" +
                "ght factor and percent removed (as a whole number, e.g. 95).";
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label10.Location = new System.Drawing.Point(61, 89);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(507, 50);
            this.label10.TabIndex = 11;
            this.label10.Text = "If weights are desired for secondary product, enter a secondary product code, wei" +
                "ght factor and percent removed (as a whole number, e.g. 95).";
            // 
            // button3
            // 
            this.button3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.button3.Location = new System.Drawing.Point(523, 341);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(81, 30);
            this.button3.TabIndex = 21;
            this.button3.Text = "FINISHED";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.onFinished);
            // 
            // weightEquationList
            // 
            this.weightEquationList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.weightEquationList.AutoGenerateColumns = false;
            this.weightEquationList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.weightEquationList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.validatorDataGridViewTextBoxColumn,
            this.persisterDataGridViewTextBoxColumn,
            this.speciesDataGridViewTextBoxColumn,
            this.liveDeadDataGridViewTextBoxColumn,
            this.primaryProductDataGridViewTextBoxColumn,
            this.weightFactorPrimaryDataGridViewTextBoxColumn,
            this.percentRemovedPrimaryDataGridViewTextBoxColumn,
            this.secondaryProductDataGridViewTextBoxColumn,
            this.weightFactorSecondaryDataGridViewTextBoxColumn,
            this.percentRemovedSecondaryDataGridViewTextBoxColumn,
            this.rowIDDataGridViewTextBoxColumn,
            this.dALDataGridViewTextBoxColumn,
            this.isPersistedDataGridViewCheckBoxColumn,
            this.errorDataGridViewTextBoxColumn,
            this.tagDataGridViewTextBoxColumn});
            this.weightEquationList.DataSource = this.weightEquationDOBindingSource;
            this.weightEquationList.Location = new System.Drawing.Point(30, 133);
            this.weightEquationList.Name = "weightEquationList";
            this.weightEquationList.Size = new System.Drawing.Size(574, 170);
            this.weightEquationList.TabIndex = 22;
            this.weightEquationList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.onCellClick);
            // 
            // validatorDataGridViewTextBoxColumn
            // 
            this.validatorDataGridViewTextBoxColumn.DataPropertyName = "Validator";
            this.validatorDataGridViewTextBoxColumn.HeaderText = "Validator";
            this.validatorDataGridViewTextBoxColumn.Name = "validatorDataGridViewTextBoxColumn";
            this.validatorDataGridViewTextBoxColumn.ReadOnly = true;
            this.validatorDataGridViewTextBoxColumn.Visible = false;
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
            this.speciesDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.speciesDataGridViewTextBoxColumn.Width = 65;
            // 
            // liveDeadDataGridViewTextBoxColumn
            // 
            this.liveDeadDataGridViewTextBoxColumn.DataPropertyName = "LiveDead";
            this.liveDeadDataGridViewTextBoxColumn.HeaderText = "LiveDead";
            this.liveDeadDataGridViewTextBoxColumn.Name = "liveDeadDataGridViewTextBoxColumn";
            this.liveDeadDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.liveDeadDataGridViewTextBoxColumn.Width = 75;
            // 
            // primaryProductDataGridViewTextBoxColumn
            // 
            this.primaryProductDataGridViewTextBoxColumn.DataPropertyName = "PrimaryProduct";
            this.primaryProductDataGridViewTextBoxColumn.HeaderText = "Primary Product";
            this.primaryProductDataGridViewTextBoxColumn.Name = "primaryProductDataGridViewTextBoxColumn";
            this.primaryProductDataGridViewTextBoxColumn.Width = 65;
            // 
            // weightFactorPrimaryDataGridViewTextBoxColumn
            // 
            this.weightFactorPrimaryDataGridViewTextBoxColumn.DataPropertyName = "WeightFactorPrimary";
            this.weightFactorPrimaryDataGridViewTextBoxColumn.HeaderText = "Weight Factor Primary";
            this.weightFactorPrimaryDataGridViewTextBoxColumn.Name = "weightFactorPrimaryDataGridViewTextBoxColumn";
            this.weightFactorPrimaryDataGridViewTextBoxColumn.Width = 65;
            // 
            // percentRemovedPrimaryDataGridViewTextBoxColumn
            // 
            this.percentRemovedPrimaryDataGridViewTextBoxColumn.DataPropertyName = "PercentRemovedPrimary";
            this.percentRemovedPrimaryDataGridViewTextBoxColumn.HeaderText = "Percent Removed Primary";
            this.percentRemovedPrimaryDataGridViewTextBoxColumn.Name = "percentRemovedPrimaryDataGridViewTextBoxColumn";
            this.percentRemovedPrimaryDataGridViewTextBoxColumn.Width = 65;
            // 
            // secondaryProductDataGridViewTextBoxColumn
            // 
            this.secondaryProductDataGridViewTextBoxColumn.DataPropertyName = "SecondaryProduct";
            this.secondaryProductDataGridViewTextBoxColumn.HeaderText = "Secondary Product";
            this.secondaryProductDataGridViewTextBoxColumn.Name = "secondaryProductDataGridViewTextBoxColumn";
            this.secondaryProductDataGridViewTextBoxColumn.Width = 65;
            // 
            // weightFactorSecondaryDataGridViewTextBoxColumn
            // 
            this.weightFactorSecondaryDataGridViewTextBoxColumn.DataPropertyName = "WeightFactorSecondary";
            this.weightFactorSecondaryDataGridViewTextBoxColumn.HeaderText = "Weight Factor Secondary";
            this.weightFactorSecondaryDataGridViewTextBoxColumn.Name = "weightFactorSecondaryDataGridViewTextBoxColumn";
            this.weightFactorSecondaryDataGridViewTextBoxColumn.Width = 65;
            // 
            // percentRemovedSecondaryDataGridViewTextBoxColumn
            // 
            this.percentRemovedSecondaryDataGridViewTextBoxColumn.DataPropertyName = "PercentRemovedSecondary";
            this.percentRemovedSecondaryDataGridViewTextBoxColumn.HeaderText = "Percent Removed Secondary";
            this.percentRemovedSecondaryDataGridViewTextBoxColumn.Name = "percentRemovedSecondaryDataGridViewTextBoxColumn";
            this.percentRemovedSecondaryDataGridViewTextBoxColumn.Width = 65;
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
            // weightEquationDOBindingSource
            // 
            this.weightEquationDOBindingSource.DataSource = typeof(CruiseDAL.DataObjects.WeightEquationDO);
            // 
            // dataGridViewTextBoxColumn9
            // 
            this.dataGridViewTextBoxColumn9.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn9.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn9.Name = "dataGridViewTextBoxColumn9";
            this.dataGridViewTextBoxColumn9.Visible = false;
            // 
            // dataGridViewTextBoxColumn10
            // 
            this.dataGridViewTextBoxColumn10.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn10.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn10.Name = "dataGridViewTextBoxColumn10";
            this.dataGridViewTextBoxColumn10.Visible = false;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(427, 341);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 30);
            this.cancelButton.TabIndex = 23;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // dataGridViewTextBoxColumn11
            // 
            this.dataGridViewTextBoxColumn11.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn11.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn11.Name = "dataGridViewTextBoxColumn11";
            this.dataGridViewTextBoxColumn11.Visible = false;
            // 
            // dataGridViewTextBoxColumn12
            // 
            this.dataGridViewTextBoxColumn12.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn12.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn12.Name = "dataGridViewTextBoxColumn12";
            this.dataGridViewTextBoxColumn12.Visible = false;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(31, 331);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 16);
            this.label3.TabIndex = 24;
            this.label3.Text = "SPECIES";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label4.Location = new System.Drawing.Point(118, 331);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 16);
            this.label4.TabIndex = 25;
            this.label4.Text = "LIVE/DEAD";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label5.Location = new System.Drawing.Point(211, 316);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(78, 41);
            this.label5.TabIndex = 26;
            this.label5.Text = "PRIMARY PRODUCT";
            // 
            // speciesList
            // 
            this.speciesList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.speciesList.FormattingEnabled = true;
            this.speciesList.Location = new System.Drawing.Point(35, 350);
            this.speciesList.Name = "speciesList";
            this.speciesList.Size = new System.Drawing.Size(60, 21);
            this.speciesList.TabIndex = 27;
            // 
            // liveDeadList
            // 
            this.liveDeadList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.liveDeadList.FormattingEnabled = true;
            this.liveDeadList.Items.AddRange(new object[] {
            "L",
            "D"});
            this.liveDeadList.Location = new System.Drawing.Point(138, 350);
            this.liveDeadList.Name = "liveDeadList";
            this.liveDeadList.Size = new System.Drawing.Size(39, 21);
            this.liveDeadList.TabIndex = 28;
            // 
            // primaryProdList
            // 
            this.primaryProdList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryProdList.FormattingEnabled = true;
            this.primaryProdList.Location = new System.Drawing.Point(214, 350);
            this.primaryProdList.Name = "primaryProdList";
            this.primaryProdList.Size = new System.Drawing.Size(58, 21);
            this.primaryProdList.TabIndex = 29;
            // 
            // insertButton
            // 
            this.insertButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.insertButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.insertButton.Location = new System.Drawing.Point(295, 341);
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(81, 30);
            this.insertButton.TabIndex = 30;
            this.insertButton.Text = "INSERT";
            this.insertButton.UseVisualStyleBackColor = true;
            this.insertButton.Click += new System.EventHandler(this.onInsertClick);
            // 
            // dataGridViewTextBoxColumn13
            // 
            this.dataGridViewTextBoxColumn13.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn13.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn13.Name = "dataGridViewTextBoxColumn13";
            this.dataGridViewTextBoxColumn13.Visible = false;
            // 
            // tagDataGridViewTextBoxColumn
            // 
            this.tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            this.tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            this.tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            this.tagDataGridViewTextBoxColumn.Visible = false;
            // 
            // WeightEquations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(636, 392);
            this.Controls.Add(this.insertButton);
            this.Controls.Add(this.primaryProdList);
            this.Controls.Add(this.liveDeadList);
            this.Controls.Add(this.speciesList);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.weightEquationList);
            this.Controls.Add(this.button3);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WeightEquations";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Weight Equations";
            ((System.ComponentModel.ISupportInitialize)(this.weightEquationList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.weightEquationDOBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.DataGridView weightEquationList;
        private System.Windows.Forms.BindingSource weightEquationDOBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn9;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn10;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn11;
        private System.Windows.Forms.DataGridViewTextBoxColumn validatorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn persisterDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn speciesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn liveDeadDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn primaryProductDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn weightFactorPrimaryDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn percentRemovedPrimaryDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn secondaryProductDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn weightFactorSecondaryDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn percentRemovedSecondaryDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rowIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dALDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isPersistedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tagDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn12;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox speciesList;
        private System.Windows.Forms.ComboBox liveDeadList;
        private System.Windows.Forms.ComboBox primaryProdList;
        private System.Windows.Forms.Button insertButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn13;
    }
}