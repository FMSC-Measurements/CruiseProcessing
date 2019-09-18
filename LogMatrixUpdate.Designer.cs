namespace CruiseProcessing
{
    partial class LogMatrixUpdate
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogMatrixUpdate));
            this.label1 = new System.Windows.Forms.Label();
            this.logMatrixGrid = new System.Windows.Forms.DataGridView();
            this.newParameters = new System.Windows.Forms.GroupBox();
            this.descriptorCamprun = new System.Windows.Forms.RadioButton();
            this.descriptorOr = new System.Windows.Forms.RadioButton();
            this.descriptorAnd = new System.Windows.Forms.RadioButton();
            this.addRow = new System.Windows.Forms.Button();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.maxSED = new System.Windows.Forms.TextBox();
            this.descriptor2 = new System.Windows.Forms.ComboBox();
            this.minSED = new System.Windows.Forms.TextBox();
            this.descriptor1 = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.smallEndDiameter = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.grade6 = new System.Windows.Forms.CheckBox();
            this.grade5 = new System.Windows.Forms.CheckBox();
            this.grade4 = new System.Windows.Forms.CheckBox();
            this.grade3 = new System.Windows.Forms.CheckBox();
            this.grade2 = new System.Windows.Forms.CheckBox();
            this.grade1 = new System.Windows.Forms.CheckBox();
            this.grade0 = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.newLogGradeCode = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.speciesList = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.newLogSortDescription = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.currentSaleName = new System.Windows.Forms.TextBox();
            this.currentCruiseNumber = new System.Windows.Forms.TextBox();
            this.clearAll = new System.Windows.Forms.Button();
            this.deleteRow = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.doneButton = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.currentReport = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.logMatrixGrid)).BeginInit();
            this.newParameters.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(724, 60);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // logMatrixGrid
            // 
            this.logMatrixGrid.AllowUserToAddRows = false;
            this.logMatrixGrid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.logMatrixGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.logMatrixGrid.Location = new System.Drawing.Point(91, 427);
            this.logMatrixGrid.Name = "logMatrixGrid";
            this.logMatrixGrid.ReadOnly = true;
            this.logMatrixGrid.Size = new System.Drawing.Size(607, 149);
            this.logMatrixGrid.TabIndex = 1;
            // 
            // newParameters
            // 
            this.newParameters.Controls.Add(this.descriptorCamprun);
            this.newParameters.Controls.Add(this.descriptorOr);
            this.newParameters.Controls.Add(this.descriptorAnd);
            this.newParameters.Controls.Add(this.addRow);
            this.newParameters.Controls.Add(this.label12);
            this.newParameters.Controls.Add(this.label11);
            this.newParameters.Controls.Add(this.maxSED);
            this.newParameters.Controls.Add(this.descriptor2);
            this.newParameters.Controls.Add(this.minSED);
            this.newParameters.Controls.Add(this.descriptor1);
            this.newParameters.Controls.Add(this.label10);
            this.newParameters.Controls.Add(this.label9);
            this.newParameters.Controls.Add(this.smallEndDiameter);
            this.newParameters.Controls.Add(this.label8);
            this.newParameters.Controls.Add(this.grade6);
            this.newParameters.Controls.Add(this.grade5);
            this.newParameters.Controls.Add(this.grade4);
            this.newParameters.Controls.Add(this.grade3);
            this.newParameters.Controls.Add(this.grade2);
            this.newParameters.Controls.Add(this.grade1);
            this.newParameters.Controls.Add(this.grade0);
            this.newParameters.Controls.Add(this.label7);
            this.newParameters.Controls.Add(this.newLogGradeCode);
            this.newParameters.Controls.Add(this.label6);
            this.newParameters.Controls.Add(this.speciesList);
            this.newParameters.Controls.Add(this.label5);
            this.newParameters.Controls.Add(this.newLogSortDescription);
            this.newParameters.Controls.Add(this.label4);
            this.newParameters.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.newParameters.Location = new System.Drawing.Point(15, 104);
            this.newParameters.Name = "newParameters";
            this.newParameters.Size = new System.Drawing.Size(765, 216);
            this.newParameters.TabIndex = 2;
            this.newParameters.TabStop = false;
            this.newParameters.Text = "New Parameters";
            // 
            // descriptorCamprun
            // 
            this.descriptorCamprun.AutoSize = true;
            this.descriptorCamprun.Location = new System.Drawing.Point(569, 113);
            this.descriptorCamprun.Name = "descriptorCamprun";
            this.descriptorCamprun.Size = new System.Drawing.Size(84, 20);
            this.descriptorCamprun.TabIndex = 29;
            this.descriptorCamprun.TabStop = true;
            this.descriptorCamprun.Text = "(camprun)";
            this.descriptorCamprun.UseVisualStyleBackColor = true;
            this.descriptorCamprun.Enter += new System.EventHandler(this.onDescriptorCamprun);
            // 
            // descriptorOr
            // 
            this.descriptorOr.AutoSize = true;
            this.descriptorOr.Location = new System.Drawing.Point(526, 113);
            this.descriptorOr.Name = "descriptorOr";
            this.descriptorOr.Size = new System.Drawing.Size(37, 20);
            this.descriptorOr.TabIndex = 28;
            this.descriptorOr.TabStop = true;
            this.descriptorOr.Text = "or";
            this.descriptorOr.UseVisualStyleBackColor = true;
            this.descriptorOr.Enter += new System.EventHandler(this.onDescriptorOr);
            // 
            // descriptorAnd
            // 
            this.descriptorAnd.AutoSize = true;
            this.descriptorAnd.Location = new System.Drawing.Point(485, 113);
            this.descriptorAnd.Name = "descriptorAnd";
            this.descriptorAnd.Size = new System.Drawing.Size(35, 20);
            this.descriptorAnd.TabIndex = 27;
            this.descriptorAnd.TabStop = true;
            this.descriptorAnd.Text = "&&";
            this.descriptorAnd.UseVisualStyleBackColor = true;
            this.descriptorAnd.Enter += new System.EventHandler(this.onDescriptorAnd);
            // 
            // addRow
            // 
            this.addRow.Location = new System.Drawing.Point(646, 174);
            this.addRow.Name = "addRow";
            this.addRow.Size = new System.Drawing.Size(88, 23);
            this.addRow.TabIndex = 9;
            this.addRow.Text = "ADD ROW";
            this.addRow.UseVisualStyleBackColor = true;
            this.addRow.Click += new System.EventHandler(this.onAddRow);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(377, 156);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(78, 16);
            this.label12.TabIndex = 26;
            this.label12.Text = "Descriptor 2";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(182, 156);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(78, 16);
            this.label11.TabIndex = 25;
            this.label11.Text = "Descriptor 1";
            // 
            // maxSED
            // 
            this.maxSED.Location = new System.Drawing.Point(479, 175);
            this.maxSED.Name = "maxSED";
            this.maxSED.Size = new System.Drawing.Size(43, 22);
            this.maxSED.TabIndex = 24;
            this.maxSED.Leave += new System.EventHandler(this.onmaxSEDchanged);
            // 
            // descriptor2
            // 
            this.descriptor2.FormattingEnabled = true;
            this.descriptor2.Items.AddRange(new object[] {
            "and",
            "thru"});
            this.descriptor2.Location = new System.Drawing.Point(380, 175);
            this.descriptor2.Name = "descriptor2";
            this.descriptor2.Size = new System.Drawing.Size(53, 24);
            this.descriptor2.TabIndex = 23;
            this.descriptor2.TextChanged += new System.EventHandler(this.onDescriptor2Changed);
            // 
            // minSED
            // 
            this.minSED.Location = new System.Drawing.Point(295, 175);
            this.minSED.Name = "minSED";
            this.minSED.Size = new System.Drawing.Size(43, 22);
            this.minSED.TabIndex = 22;
            this.minSED.Leave += new System.EventHandler(this.onMinSEDchanged);
            // 
            // descriptor1
            // 
            this.descriptor1.FormattingEnabled = true;
            this.descriptor1.Items.AddRange(new object[] {
            "greater than",
            "less than",
            "between",
            "none"});
            this.descriptor1.Location = new System.Drawing.Point(185, 175);
            this.descriptor1.Name = "descriptor1";
            this.descriptor1.Size = new System.Drawing.Size(95, 24);
            this.descriptor1.TabIndex = 21;
            this.descriptor1.TextChanged += new System.EventHandler(this.onDescriptor1Changed);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(461, 156);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(65, 16);
            this.label10.TabIndex = 20;
            this.label10.Text = "Maximum";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(292, 156);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(61, 16);
            this.label9.TabIndex = 19;
            this.label9.Text = "Minimum";
            // 
            // smallEndDiameter
            // 
            this.smallEndDiameter.Location = new System.Drawing.Point(20, 175);
            this.smallEndDiameter.Name = "smallEndDiameter";
            this.smallEndDiameter.Size = new System.Drawing.Size(126, 22);
            this.smallEndDiameter.TabIndex = 18;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(17, 156);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(124, 16);
            this.label8.TabIndex = 17;
            this.label8.Text = "Small End Diameter";
            // 
            // grade6
            // 
            this.grade6.AutoSize = true;
            this.grade6.Location = new System.Drawing.Point(399, 113);
            this.grade6.Name = "grade6";
            this.grade6.Size = new System.Drawing.Size(34, 20);
            this.grade6.TabIndex = 13;
            this.grade6.Text = "6";
            this.grade6.UseVisualStyleBackColor = true;
            this.grade6.Enter += new System.EventHandler(this.onGrade6);
            // 
            // grade5
            // 
            this.grade5.AutoSize = true;
            this.grade5.Location = new System.Drawing.Point(359, 113);
            this.grade5.Name = "grade5";
            this.grade5.Size = new System.Drawing.Size(34, 20);
            this.grade5.TabIndex = 12;
            this.grade5.Text = "5";
            this.grade5.UseVisualStyleBackColor = true;
            this.grade5.Enter += new System.EventHandler(this.onGrade5);
            // 
            // grade4
            // 
            this.grade4.AutoSize = true;
            this.grade4.Location = new System.Drawing.Point(319, 113);
            this.grade4.Name = "grade4";
            this.grade4.Size = new System.Drawing.Size(34, 20);
            this.grade4.TabIndex = 11;
            this.grade4.Text = "4";
            this.grade4.UseVisualStyleBackColor = true;
            this.grade4.Enter += new System.EventHandler(this.onGrade4);
            // 
            // grade3
            // 
            this.grade3.AutoSize = true;
            this.grade3.Location = new System.Drawing.Point(279, 113);
            this.grade3.Name = "grade3";
            this.grade3.Size = new System.Drawing.Size(34, 20);
            this.grade3.TabIndex = 10;
            this.grade3.Text = "3";
            this.grade3.UseVisualStyleBackColor = true;
            this.grade3.CheckedChanged += new System.EventHandler(this.onGrade3);
            // 
            // grade2
            // 
            this.grade2.AutoSize = true;
            this.grade2.Location = new System.Drawing.Point(239, 113);
            this.grade2.Name = "grade2";
            this.grade2.Size = new System.Drawing.Size(34, 20);
            this.grade2.TabIndex = 9;
            this.grade2.Text = "2";
            this.grade2.UseVisualStyleBackColor = true;
            this.grade2.CheckedChanged += new System.EventHandler(this.onGrade2);
            // 
            // grade1
            // 
            this.grade1.AutoSize = true;
            this.grade1.Location = new System.Drawing.Point(199, 113);
            this.grade1.Name = "grade1";
            this.grade1.Size = new System.Drawing.Size(34, 20);
            this.grade1.TabIndex = 8;
            this.grade1.Text = "1";
            this.grade1.UseVisualStyleBackColor = true;
            this.grade1.CheckedChanged += new System.EventHandler(this.onGrade1);
            // 
            // grade0
            // 
            this.grade0.AutoSize = true;
            this.grade0.Location = new System.Drawing.Point(159, 113);
            this.grade0.Name = "grade0";
            this.grade0.Size = new System.Drawing.Size(34, 20);
            this.grade0.TabIndex = 7;
            this.grade0.Text = "0";
            this.grade0.UseVisualStyleBackColor = true;
            this.grade0.CheckedChanged += new System.EventHandler(this.onGrade0);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(156, 90);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(437, 16);
            this.label7.TabIndex = 6;
            this.label7.Text = "Click on log grade(s) to include and any descriptor to use between grades.";
            // 
            // newLogGradeCode
            // 
            this.newLogGradeCode.Location = new System.Drawing.Point(20, 111);
            this.newLogGradeCode.Name = "newLogGradeCode";
            this.newLogGradeCode.Size = new System.Drawing.Size(113, 22);
            this.newLogGradeCode.TabIndex = 5;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(17, 90);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(102, 16);
            this.label6.TabIndex = 4;
            this.label6.Text = "Log Grade Code";
            // 
            // speciesList
            // 
            this.speciesList.FormattingEnabled = true;
            this.speciesList.Items.AddRange(new object[] {
            "042",
            "098",
            "242",
            "263",
            "098Y",
            "263Y"});
            this.speciesList.Location = new System.Drawing.Point(417, 52);
            this.speciesList.Name = "speciesList";
            this.speciesList.Size = new System.Drawing.Size(74, 24);
            this.speciesList.TabIndex = 3;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(414, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(55, 16);
            this.label5.TabIndex = 2;
            this.label5.Text = "Species";
            // 
            // newLogSortDescription
            // 
            this.newLogSortDescription.Location = new System.Drawing.Point(20, 54);
            this.newLogSortDescription.Name = "newLogSortDescription";
            this.newLogSortDescription.Size = new System.Drawing.Size(354, 22);
            this.newLogSortDescription.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 30);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 16);
            this.label4.TabIndex = 0;
            this.label4.Text = "Log Sort Description";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(222, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(118, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Current Sale Name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(541, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Current Cruise Number";
            // 
            // currentSaleName
            // 
            this.currentSaleName.Location = new System.Drawing.Point(346, 69);
            this.currentSaleName.Name = "currentSaleName";
            this.currentSaleName.Size = new System.Drawing.Size(177, 20);
            this.currentSaleName.TabIndex = 5;
            // 
            // currentCruiseNumber
            // 
            this.currentCruiseNumber.Location = new System.Drawing.Point(687, 69);
            this.currentCruiseNumber.Name = "currentCruiseNumber";
            this.currentCruiseNumber.Size = new System.Drawing.Size(76, 20);
            this.currentCruiseNumber.TabIndex = 6;
            // 
            // clearAll
            // 
            this.clearAll.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.clearAll.Location = new System.Drawing.Point(255, 386);
            this.clearAll.Name = "clearAll";
            this.clearAll.Size = new System.Drawing.Size(98, 23);
            this.clearAll.TabIndex = 7;
            this.clearAll.Text = "CLEAR ALL";
            this.clearAll.UseVisualStyleBackColor = true;
            this.clearAll.Click += new System.EventHandler(this.onClearAll);
            // 
            // deleteRow
            // 
            this.deleteRow.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.deleteRow.Location = new System.Drawing.Point(417, 386);
            this.deleteRow.Name = "deleteRow";
            this.deleteRow.Size = new System.Drawing.Size(119, 23);
            this.deleteRow.TabIndex = 8;
            this.deleteRow.Text = "DELETE ROW";
            this.deleteRow.UseVisualStyleBackColor = true;
            this.deleteRow.Click += new System.EventHandler(this.onDeleteRow);
            // 
            // label13
            // 
            this.label13.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(114, 329);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(574, 54);
            this.label13.TabIndex = 9;
            this.label13.Text = "To delete a row in the table below, highlight the desired row and click the DELET" +
                "E ROW button.  To remove all rows, click CLEAR ALL.  When finished making change" +
                "s, click DONE to exit and save changes.";
            // 
            // doneButton
            // 
            this.doneButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.doneButton.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.doneButton.Location = new System.Drawing.Point(725, 579);
            this.doneButton.Name = "doneButton";
            this.doneButton.Size = new System.Drawing.Size(75, 23);
            this.doneButton.TabIndex = 10;
            this.doneButton.Text = "DONE";
            this.doneButton.UseVisualStyleBackColor = true;
            this.doneButton.Click += new System.EventHandler(this.onDone);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label14.Location = new System.Drawing.Point(12, 69);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(92, 16);
            this.label14.TabIndex = 11;
            this.label14.Text = "Current Report";
            // 
            // currentReport
            // 
            this.currentReport.Location = new System.Drawing.Point(110, 69);
            this.currentReport.Name = "currentReport";
            this.currentReport.Size = new System.Drawing.Size(72, 20);
            this.currentReport.TabIndex = 12;
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(12, 579);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 13;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // LogMatrixUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(812, 614);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.currentReport);
            this.Controls.Add(this.label14);
            this.Controls.Add(this.doneButton);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.deleteRow);
            this.Controls.Add(this.clearAll);
            this.Controls.Add(this.currentCruiseNumber);
            this.Controls.Add(this.currentSaleName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.newParameters);
            this.Controls.Add(this.logMatrixGrid);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogMatrixUpdate";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Log Matrix Update (R008/R009)";
            ((System.ComponentModel.ISupportInitialize)(this.logMatrixGrid)).EndInit();
            this.newParameters.ResumeLayout(false);
            this.newParameters.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView logMatrixGrid;
        private System.Windows.Forms.GroupBox newParameters;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox newLogSortDescription;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox currentSaleName;
        private System.Windows.Forms.TextBox currentCruiseNumber;
        private System.Windows.Forms.ComboBox speciesList;
        private System.Windows.Forms.TextBox newLogGradeCode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox grade6;
        private System.Windows.Forms.CheckBox grade5;
        private System.Windows.Forms.CheckBox grade4;
        private System.Windows.Forms.CheckBox grade3;
        private System.Windows.Forms.CheckBox grade2;
        private System.Windows.Forms.CheckBox grade1;
        private System.Windows.Forms.CheckBox grade0;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox descriptor2;
        private System.Windows.Forms.TextBox minSED;
        private System.Windows.Forms.ComboBox descriptor1;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox smallEndDiameter;
        private System.Windows.Forms.TextBox maxSED;
        private System.Windows.Forms.Button addRow;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button clearAll;
        private System.Windows.Forms.Button deleteRow;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Button doneButton;
        private System.Windows.Forms.RadioButton descriptorCamprun;
        private System.Windows.Forms.RadioButton descriptorOr;
        private System.Windows.Forms.RadioButton descriptorAnd;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox currentReport;
        private System.Windows.Forms.Button cancelButton;
    }
}