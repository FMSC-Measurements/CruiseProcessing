namespace CruiseProcessing
{
    partial class QualityAdjEquations
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QualityAdjEquations));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.exitButton = new System.Windows.Forms.Button();
            this.qualityEquationList = new System.Windows.Forms.DataGridView();
            this.validatorDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.persisterDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qualityAdjEqDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.speciesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.yearDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qualityAdjEquationDOBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.cancelButton = new System.Windows.Forms.Button();
            this.dataGridViewTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.label3 = new System.Windows.Forms.Label();
            this.equationList = new System.Windows.Forms.ComboBox();
            this.insertButton = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.qualityEquationList)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityAdjEquationDOBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(23, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(271, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "QUALITY ADJUSTMENT EQUATIONS";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(23, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(444, 55);
            this.label2.TabIndex = 1;
            this.label2.Text = "To enter Quality Adjustment Equations, select the equation number and enter the s" +
                "pecies, year, grade and up to six coeffficients.  Use the slider bar at the bott" +
                "om to view remaining fields.";
            // 
            // exitButton
            // 
            this.exitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(520, 257);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(81, 30);
            this.exitButton.TabIndex = 14;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // qualityEquationList
            // 
            this.qualityEquationList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.qualityEquationList.AutoGenerateColumns = false;
            this.qualityEquationList.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.qualityEquationList.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.validatorDataGridViewTextBoxColumn,
            this.persisterDataGridViewTextBoxColumn,
            this.qualityAdjEqDataGridViewTextBoxColumn,
            this.speciesDataGridViewTextBoxColumn,
            this.yearDataGridViewTextBoxColumn,
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
            this.qualityEquationList.DataSource = this.qualityAdjEquationDOBindingSource;
            this.qualityEquationList.Location = new System.Drawing.Point(26, 101);
            this.qualityEquationList.Name = "qualityEquationList";
            this.qualityEquationList.Size = new System.Drawing.Size(575, 131);
            this.qualityEquationList.TabIndex = 15;
            this.qualityEquationList.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.onCellClick);
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
            // qualityAdjEqDataGridViewTextBoxColumn
            // 
            this.qualityAdjEqDataGridViewTextBoxColumn.DataPropertyName = "QualityAdjEq";
            this.qualityAdjEqDataGridViewTextBoxColumn.HeaderText = "Equation";
            this.qualityAdjEqDataGridViewTextBoxColumn.Name = "qualityAdjEqDataGridViewTextBoxColumn";
            this.qualityAdjEqDataGridViewTextBoxColumn.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.qualityAdjEqDataGridViewTextBoxColumn.Width = 75;
            // 
            // speciesDataGridViewTextBoxColumn
            // 
            this.speciesDataGridViewTextBoxColumn.DataPropertyName = "Species";
            this.speciesDataGridViewTextBoxColumn.HeaderText = "Species";
            this.speciesDataGridViewTextBoxColumn.Name = "speciesDataGridViewTextBoxColumn";
            this.speciesDataGridViewTextBoxColumn.Width = 65;
            // 
            // yearDataGridViewTextBoxColumn
            // 
            this.yearDataGridViewTextBoxColumn.DataPropertyName = "Year";
            this.yearDataGridViewTextBoxColumn.HeaderText = "Year";
            this.yearDataGridViewTextBoxColumn.Name = "yearDataGridViewTextBoxColumn";
            this.yearDataGridViewTextBoxColumn.Width = 45;
            // 
            // gradeDataGridViewTextBoxColumn
            // 
            this.gradeDataGridViewTextBoxColumn.DataPropertyName = "Grade";
            this.gradeDataGridViewTextBoxColumn.HeaderText = "Grade";
            this.gradeDataGridViewTextBoxColumn.Name = "gradeDataGridViewTextBoxColumn";
            this.gradeDataGridViewTextBoxColumn.Width = 45;
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
            // tagDataGridViewTextBoxColumn
            // 
            this.tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            this.tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            this.tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            this.tagDataGridViewTextBoxColumn.Visible = false;
            // 
            // qualityAdjEquationDOBindingSource
            // 
            this.qualityAdjEquationDOBindingSource.DataSource = typeof(CruiseDAL.DataObjects.QualityAdjEquationDO);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(410, 257);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(81, 30);
            this.cancelButton.TabIndex = 16;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // dataGridViewTextBoxColumn1
            // 
            this.dataGridViewTextBoxColumn1.DataPropertyName = "Tag";
            this.dataGridViewTextBoxColumn1.HeaderText = "Tag";
            this.dataGridViewTextBoxColumn1.Name = "dataGridViewTextBoxColumn1";
            this.dataGridViewTextBoxColumn1.Visible = false;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(60, 245);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 16);
            this.label3.TabIndex = 17;
            this.label3.Text = "EQUATION";
            // 
            // equationList
            // 
            this.equationList.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.equationList.FormattingEnabled = true;
            this.equationList.Items.AddRange(new object[] {
            "QUAL0391",
            "QUAL0392"});
            this.equationList.Location = new System.Drawing.Point(63, 264);
            this.equationList.Name = "equationList";
            this.equationList.Size = new System.Drawing.Size(86, 21);
            this.equationList.TabIndex = 18;
            // 
            // insertButton
            // 
            this.insertButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.insertButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.insertButton.Location = new System.Drawing.Point(165, 258);
            this.insertButton.Name = "insertButton";
            this.insertButton.Size = new System.Drawing.Size(81, 30);
            this.insertButton.TabIndex = 19;
            this.insertButton.Text = "INSERT";
            this.insertButton.UseVisualStyleBackColor = true;
            this.insertButton.Click += new System.EventHandler(this.onInsertClick);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.button1.Location = new System.Drawing.Point(483, 65);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(118, 30);
            this.button1.TabIndex = 20;
            this.button1.Text = "DELETE ROW";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.onDelete);
            // 
            // QualityAdjEquations
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(628, 316);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.insertButton);
            this.Controls.Add(this.equationList);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.qualityEquationList);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "QualityAdjEquations";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Enter Quality Adjustment Equations";
            ((System.ComponentModel.ISupportInitialize)(this.qualityEquationList)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qualityAdjEquationDOBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.DataGridView qualityEquationList;
        private System.Windows.Forms.BindingSource qualityAdjEquationDOBindingSource;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridViewTextBoxColumn1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox equationList;
        private System.Windows.Forms.Button insertButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn validatorDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn persisterDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn qualityAdjEqDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn speciesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn yearDataGridViewTextBoxColumn;
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
        private System.Windows.Forms.Button button1;
    }
}