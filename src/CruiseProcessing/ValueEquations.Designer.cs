namespace CruiseProcessing
{
    partial class ValueEquations
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ValueEquations));
            this.label1 = new System.Windows.Forms.Label();
            this.regionNum = new System.Windows.Forms.ComboBox();
            this.valueEquationList = new System.Windows.Forms.DataGridView();
            this.validatorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.persisterDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueEquationNumberDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.speciesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.primaryProductDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.gradeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coefficient1DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coefficient2DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coefficient3DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coefficient4DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coefficient5DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.coefficient6DataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rowIDDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dALDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.isPersistedDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.errorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.valueEquationDOBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.exitButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cancelButton = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label4 = new System.Windows.Forms.Label();
            this.equationNumber = new System.Windows.Forms.ComboBox();
            this.insertButton = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridViewTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.speciesList = new System.Windows.Forms.ComboBox();
            this.primaryProdList = new System.Windows.Forms.ComboBox();
            this.delete_Button = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.valueEquationList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueEquationDOBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(32, 322);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(58, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "REGION";
            // 
            // regionNum
            // 
            this.regionNum.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.regionNum.FormattingEnabled = true;
            this.regionNum.Items.AddRange(new object[] {
            "01",
            "02",
            "03",
            "04",
            "05",
            "08",
            "09",
            "10"});
            this.regionNum.Location = new System.Drawing.Point(35, 346);
            this.regionNum.Name = "regionNum";
            this.regionNum.Size = new System.Drawing.Size(39, 24);
            this.regionNum.TabIndex = 5;
            this.regionNum.SelectedIndexChanged += new System.EventHandler(this.onRegionSelected);
            // 
            // valueEquationList
            // 
            this.valueEquationList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.valueEquationList.AutoGenerateColumns = false;
            this.valueEquationList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.valueEquationList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.validatorDataGridViewTextBoxColumn,
            this.persisterDataGridViewTextBoxColumn,
            this.valueEquationNumberDataGridViewTextBoxColumn,
            this.speciesDataGridViewTextBoxColumn,
            this.primaryProductDataGridViewTextBoxColumn,
            this.gradeDataGridViewTextBoxColumn,
            this.coefficient1DataGridViewTextBoxColumn,
            this.coefficient2DataGridViewTextBoxColumn,
            this.coefficient3DataGridViewTextBoxColumn,
            this.coefficient4DataGridViewTextBoxColumn,
            this.coefficient5DataGridViewTextBoxColumn,
            this.coefficient6DataGridViewTextBoxColumn,
            this.rowIDDataGridViewTextBoxColumn,
            this.dALDataGridViewTextBoxColumn,
            this.isPersistedDataGridViewCheckBoxColumn,
            this.errorDataGridViewTextBoxColumn,
            this.tagDataGridViewTextBoxColumn});
            this.valueEquationList.DataSource = this.valueEquationDOBindingSource;
            this.valueEquationList.Location = new System.Drawing.Point(25, 85);
            this.valueEquationList.Name = "valueEquationList";
            this.valueEquationList.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.valueEquationList.Size = new System.Drawing.Size(657, 221);
            this.valueEquationList.TabIndex = 11;
            this.valueEquationList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.onCellClick);
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
            // valueEquationNumberDataGridViewTextBoxColumn
            // 
            this.valueEquationNumberDataGridViewTextBoxColumn.DataPropertyName = "ValueEquationNumber";
            this.valueEquationNumberDataGridViewTextBoxColumn.HeaderText = "Value Equation Number";
            this.valueEquationNumberDataGridViewTextBoxColumn.Name = "valueEquationNumberDataGridViewTextBoxColumn";
            this.valueEquationNumberDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            // 
            // speciesDataGridViewTextBoxColumn
            // 
            this.speciesDataGridViewTextBoxColumn.DataPropertyName = "Species";
            this.speciesDataGridViewTextBoxColumn.HeaderText = "Species";
            this.speciesDataGridViewTextBoxColumn.Name = "speciesDataGridViewTextBoxColumn";
            this.speciesDataGridViewTextBoxColumn.Width = 65;
            // 
            // primaryProductDataGridViewTextBoxColumn
            // 
            this.primaryProductDataGridViewTextBoxColumn.DataPropertyName = "PrimaryProduct";
            this.primaryProductDataGridViewTextBoxColumn.HeaderText = "Primary Product";
            this.primaryProductDataGridViewTextBoxColumn.Name = "primaryProductDataGridViewTextBoxColumn";
            this.primaryProductDataGridViewTextBoxColumn.Width = 75;
            // 
            // gradeDataGridViewTextBoxColumn
            // 
            this.gradeDataGridViewTextBoxColumn.DataPropertyName = "Grade";
            this.gradeDataGridViewTextBoxColumn.HeaderText = "Grade";
            this.gradeDataGridViewTextBoxColumn.Name = "gradeDataGridViewTextBoxColumn";
            this.gradeDataGridViewTextBoxColumn.Width = 55;
            // 
            // coefficient1DataGridViewTextBoxColumn
            // 
            this.coefficient1DataGridViewTextBoxColumn.DataPropertyName = "Coefficient1";
            this.coefficient1DataGridViewTextBoxColumn.HeaderText = "Coefficient 1";
            this.coefficient1DataGridViewTextBoxColumn.Name = "coefficient1DataGridViewTextBoxColumn";
            this.coefficient1DataGridViewTextBoxColumn.Width = 70;
            // 
            // coefficient2DataGridViewTextBoxColumn
            // 
            this.coefficient2DataGridViewTextBoxColumn.DataPropertyName = "Coefficient2";
            this.coefficient2DataGridViewTextBoxColumn.HeaderText = "Coefficient 2";
            this.coefficient2DataGridViewTextBoxColumn.Name = "coefficient2DataGridViewTextBoxColumn";
            this.coefficient2DataGridViewTextBoxColumn.Width = 70;
            // 
            // coefficient3DataGridViewTextBoxColumn
            // 
            this.coefficient3DataGridViewTextBoxColumn.DataPropertyName = "Coefficient3";
            this.coefficient3DataGridViewTextBoxColumn.HeaderText = "Coefficient 3";
            this.coefficient3DataGridViewTextBoxColumn.Name = "coefficient3DataGridViewTextBoxColumn";
            this.coefficient3DataGridViewTextBoxColumn.Width = 70;
            // 
            // coefficient4DataGridViewTextBoxColumn
            // 
            this.coefficient4DataGridViewTextBoxColumn.DataPropertyName = "Coefficient4";
            this.coefficient4DataGridViewTextBoxColumn.HeaderText = "Coefficient 4";
            this.coefficient4DataGridViewTextBoxColumn.Name = "coefficient4DataGridViewTextBoxColumn";
            this.coefficient4DataGridViewTextBoxColumn.Width = 70;
            // 
            // coefficient5DataGridViewTextBoxColumn
            // 
            this.coefficient5DataGridViewTextBoxColumn.DataPropertyName = "Coefficient5";
            this.coefficient5DataGridViewTextBoxColumn.HeaderText = "Coefficient 5";
            this.coefficient5DataGridViewTextBoxColumn.Name = "coefficient5DataGridViewTextBoxColumn";
            this.coefficient5DataGridViewTextBoxColumn.Width = 70;
            // 
            // coefficient6DataGridViewTextBoxColumn
            // 
            this.coefficient6DataGridViewTextBoxColumn.DataPropertyName = "Coefficient6";
            this.coefficient6DataGridViewTextBoxColumn.HeaderText = "Coefficient 6";
            this.coefficient6DataGridViewTextBoxColumn.Name = "coefficient6DataGridViewTextBoxColumn";
            this.coefficient6DataGridViewTextBoxColumn.Width = 70;
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
            // valueEquationDOBindingSource
            // 
            this.valueEquationDOBindingSource.DataSource = typeof(CruiseDAL.DataObjects.ValueEquationDO);
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Location = new System.Drawing.Point(601, 340);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(81, 30);
            this.exitButton.TabIndex = 14;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(22, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 16);
            this.label2.TabIndex = 15;
            this.label2.Text = "VALUE EQUATIONS";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(159, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(402, 66);
            this.label3.TabIndex = 16;
            this.label3.Text = "To enter value equations, select the region number, equation number, species and " +
                "product from the lists.  Enter up to six coefficients.  Use the slider bar at th" +
                "e bottom to view remaining fields.";
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn1.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Visible = false;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Location = new System.Drawing.Point(495, 340);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 30);
            this.cancelButton.TabIndex = 17;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // dataGridViewTextBoxColumn2
            // 
            this.dataGridViewTextBoxColumn2.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn2.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn2.Name = "dataGridViewTextBoxColumn2";
            this.dataGridViewTextBoxColumn2.Visible = false;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(96, 322);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 16);
            this.label4.TabIndex = 18;
            this.label4.Text = "EQUATION";
            // 
            // equationNumber
            // 
            this.equationNumber.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.equationNumber.FormattingEnabled = true;
            this.equationNumber.Location = new System.Drawing.Point(97, 346);
            this.equationNumber.Name = "equationNumber";
            this.equationNumber.Size = new System.Drawing.Size(83, 24);
            this.equationNumber.TabIndex = 19;
            // 
            // insertButton
            // 
            this.insertButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.insertButton.Location = new System.Drawing.Point(353, 340);
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(81, 30);
            this.insertButton.TabIndex = 20;
            this.insertButton.Text = "INSERT";
            this.insertButton.UseVisualStyleBackColor = true;
            this.insertButton.Click += new System.EventHandler(this.onInsertEquation);
            // 
            // dataGridViewTextBoxColumn3
            // 
            this.dataGridViewTextBoxColumn3.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn3.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn3.Name = "dataGridViewTextBoxColumn3";
            this.dataGridViewTextBoxColumn3.Visible = false;
            // 
            // dataGridViewTextBoxColumn4
            // 
            this.dataGridViewTextBoxColumn4.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn4.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn4.Name = "dataGridViewTextBoxColumn4";
            this.dataGridViewTextBoxColumn4.Visible = false;
            // 
            // dataGridViewTextBoxColumn5
            // 
            this.dataGridViewTextBoxColumn5.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn5.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn5.Name = "dataGridViewTextBoxColumn5";
            this.dataGridViewTextBoxColumn5.Visible = false;
            // 
            // dataGridViewTextBoxColumn6
            // 
            this.dataGridViewTextBoxColumn6.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn6.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn6.Name = "dataGridViewTextBoxColumn6";
            this.dataGridViewTextBoxColumn6.Visible = false;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(272, 309);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(75, 34);
            this.label5.TabIndex = 21;
            this.label5.Text = "PRIMARY PRODUCT";
            // 
            // label6
            // 
            this.label6.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(192, 322);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 16);
            this.label6.TabIndex = 22;
            this.label6.Text = "SPECIES";
            // 
            // speciesList
            // 
            this.speciesList.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.speciesList.FormattingEnabled = true;
            this.speciesList.Location = new System.Drawing.Point(196, 346);
            this.speciesList.Name = "speciesList";
            this.speciesList.Size = new System.Drawing.Size(60, 24);
            this.speciesList.TabIndex = 23;
            // 
            // primaryProdList
            // 
            this.primaryProdList.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.primaryProdList.FormattingEnabled = true;
            this.primaryProdList.Location = new System.Drawing.Point(275, 346);
            this.primaryProdList.Name = "primaryProdList";
            this.primaryProdList.Size = new System.Drawing.Size(58, 24);
            this.primaryProdList.TabIndex = 24;
            // 
            // delete_Button
            // 
            this.delete_Button.Location = new System.Drawing.Point(576, 45);
            this.delete_Button.Name = "delete_Button";
            this.delete_Button.Size = new System.Drawing.Size(106, 30);
            this.delete_Button.TabIndex = 25;
            this.delete_Button.Text = "DELETE ROW";
            this.delete_Button.UseVisualStyleBackColor = true;
            this.delete_Button.Click += new System.EventHandler(this.onDelete);
            // 
            // dataGridViewTextBoxColumn7
            // 
            this.dataGridViewTextBoxColumn7.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn7.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn7.Name = "dataGridViewTextBoxColumn7";
            this.dataGridViewTextBoxColumn7.Visible = false;
            // 
            // tagDataGridViewTextBoxColumn
            // 
            this.tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            this.tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            this.tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            this.tagDataGridViewTextBoxColumn.Visible = false;
            // 
            // ValueEquations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 382);
            this.Controls.Add(this.delete_Button);
            this.Controls.Add(this.primaryProdList);
            this.Controls.Add(this.speciesList);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.insertButton);
            this.Controls.Add(this.equationNumber);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.valueEquationList);
            this.Controls.Add(this.regionNum);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ValueEquations";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Enter Value Equations";
            ((System.ComponentModel.ISupportInitialize)(this.valueEquationList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueEquationDOBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox regionNum;
        private System.Windows.Forms.DataGridView valueEquationList;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.BindingSource valueEquationDOBindingSource;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn validatorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn persisterDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn valueEquationNumberDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn speciesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn primaryProductDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn gradeDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coefficient1DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coefficient2DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coefficient3DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coefficient4DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coefficient5DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn coefficient6DataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rowIDDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn dALDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn isPersistedDataGridViewCheckBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn errorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn tagDataGridViewTextBoxColumn;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox equationNumber;
        private System.Windows.Forms.Button insertButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox speciesList;
        private System.Windows.Forms.ComboBox primaryProdList;
        private System.Windows.Forms.Button delete_Button;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn7;
    }
}