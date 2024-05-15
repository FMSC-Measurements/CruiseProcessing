namespace CruiseProcessing
{
    partial class LocalVolume
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LocalVolume));
            this.label4 = new System.Windows.Forms.Label();
            this.SpeciesGroups = new System.Windows.Forms.DataGridView();
            this.GroupsSelected = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.label5 = new System.Windows.Forms.Label();
            this.numOtrees = new System.Windows.Forms.TextBox();
            this.regressTopwood = new System.Windows.Forms.CheckBox();
            this.helpButton = new System.Windows.Forms.Button();
            this.doRegression = new System.Windows.Forms.Button();
            this.finishedButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.useGross = new System.Windows.Forms.RadioButton();
            this.useNet = new System.Windows.Forms.RadioButton();
            this.UnitOfMeasure = new System.Windows.Forms.ComboBox();
            this.resetSelection = new System.Windows.Forms.Button();
            this.rgSpeciesDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rgProductDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rgLiveDeadDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.regressGroupsBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.SpeciesGroups)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.regressGroupsBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(49, 45);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(342, 19);
            this.label4.TabIndex = 7;
            this.label4.Text = "Select one or more groups from the list below.";
            // 
            // SpeciesGroups
            // 
            this.SpeciesGroups.AllowUserToAddRows = false;
            this.SpeciesGroups.AllowUserToDeleteRows = false;
            this.SpeciesGroups.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SpeciesGroups.AutoGenerateColumns = false;
            this.SpeciesGroups.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.SpeciesGroups.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GroupsSelected,
            this.rgSpeciesDataGridViewTextBoxColumn,
            this.rgProductDataGridViewTextBoxColumn,
            this.rgLiveDeadDataGridViewTextBoxColumn});
            this.SpeciesGroups.DataSource = this.regressGroupsBindingSource;
            this.SpeciesGroups.Location = new System.Drawing.Point(52, 74);
            this.SpeciesGroups.Name = "SpeciesGroups";
            this.SpeciesGroups.RowTemplate.Height = 24;
            this.SpeciesGroups.Size = new System.Drawing.Size(359, 194);
            this.SpeciesGroups.TabIndex = 8;
            this.SpeciesGroups.CellContentClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.groupSelected);
            // 
            // GroupsSelected
            // 
            this.GroupsSelected.FalseValue = "0";
            this.GroupsSelected.HeaderText = "Select";
            this.GroupsSelected.Name = "GroupsSelected";
            this.GroupsSelected.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.GroupsSelected.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.GroupsSelected.TrueValue = "1";
            this.GroupsSelected.Width = 50;
            // 
            // label5
            // 
            this.label5.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(74, 289);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(150, 19);
            this.label5.TabIndex = 9;
            this.label5.Text = "# of Trees Selected";
            // 
            // numOtrees
            // 
            this.numOtrees.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.numOtrees.Location = new System.Drawing.Point(209, 287);
            this.numOtrees.Name = "numOtrees";
            this.numOtrees.ReadOnly = true;
            this.numOtrees.Size = new System.Drawing.Size(41, 26);
            this.numOtrees.TabIndex = 10;
            // 
            // regressTopwood
            // 
            this.regressTopwood.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.regressTopwood.Location = new System.Drawing.Point(77, 315);
            this.regressTopwood.Name = "regressTopwood";
            this.regressTopwood.Size = new System.Drawing.Size(232, 57);
            this.regressTopwood.TabIndex = 11;
            this.regressTopwood.Text = "Regress Primary plus Secondary to Estimate Topwood Volume";
            this.regressTopwood.UseVisualStyleBackColor = true;
            this.regressTopwood.CheckedChanged += new System.EventHandler(this.regressTopwoodClicked);
            // 
            // helpButton
            // 
            this.helpButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.helpButton.AutoSize = true;
            this.helpButton.Location = new System.Drawing.Point(309, 326);
            this.helpButton.Name = "helpButton";
            this.helpButton.Size = new System.Drawing.Size(38, 29);
            this.helpButton.TabIndex = 12;
            this.helpButton.Text = "?";
            this.helpButton.UseVisualStyleBackColor = true;
            this.helpButton.Click += new System.EventHandler(this.displayHelp);
            // 
            // doRegression
            // 
            this.doRegression.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.doRegression.AutoSize = true;
            this.doRegression.Location = new System.Drawing.Point(54, 376);
            this.doRegression.Name = "doRegression";
            this.doRegression.Size = new System.Drawing.Size(127, 29);
            this.doRegression.TabIndex = 13;
            this.doRegression.Text = "Do Regression";
            this.doRegression.UseVisualStyleBackColor = true;
            this.doRegression.Click += new System.EventHandler(this.onRegression);
            // 
            // finishedButton
            // 
            this.finishedButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.finishedButton.AutoSize = true;
            this.finishedButton.Location = new System.Drawing.Point(309, 376);
            this.finishedButton.Name = "finishedButton";
            this.finishedButton.Size = new System.Drawing.Size(95, 29);
            this.finishedButton.TabIndex = 14;
            this.finishedButton.Text = "FINISHED";
            this.finishedButton.UseVisualStyleBackColor = true;
            this.finishedButton.Click += new System.EventHandler(this.onFinished);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(124, 19);
            this.label1.TabIndex = 0;
            this.label1.Text = "Unit of measure";
            // 
            // useGross
            // 
            this.useGross.AutoSize = true;
            this.useGross.Location = new System.Drawing.Point(271, 10);
            this.useGross.Name = "useGross";
            this.useGross.Size = new System.Drawing.Size(87, 23);
            this.useGross.TabIndex = 5;
            this.useGross.TabStop = true;
            this.useGross.Text = "GROSS";
            this.useGross.UseVisualStyleBackColor = true;
            this.useGross.CheckedChanged += new System.EventHandler(this.grossClicked);
            // 
            // useNet
            // 
            this.useNet.AutoSize = true;
            this.useNet.Location = new System.Drawing.Point(364, 9);
            this.useNet.Name = "useNet";
            this.useNet.Size = new System.Drawing.Size(61, 23);
            this.useNet.TabIndex = 6;
            this.useNet.TabStop = true;
            this.useNet.Text = "NET";
            this.useNet.UseVisualStyleBackColor = true;
            this.useNet.CheckedChanged += new System.EventHandler(this.netClicked);
            // 
            // UnitOfMeasure
            // 
            this.UnitOfMeasure.FormattingEnabled = true;
            this.UnitOfMeasure.Items.AddRange(new object[] {
            "01 -- Board foot",
            "02 -- Cords",
            "03 -- Cubic foot"});
            this.UnitOfMeasure.Location = new System.Drawing.Point(121, 6);
            this.UnitOfMeasure.Name = "UnitOfMeasure";
            this.UnitOfMeasure.Size = new System.Drawing.Size(129, 26);
            this.UnitOfMeasure.TabIndex = 15;
            this.UnitOfMeasure.SelectedIndexChanged += new System.EventHandler(this.onIndexChanged);
            // 
            // resetSelection
            // 
            this.resetSelection.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.resetSelection.AutoSize = true;
            this.resetSelection.Location = new System.Drawing.Point(178, 376);
            this.resetSelection.Name = "resetSelection";
            this.resetSelection.Size = new System.Drawing.Size(132, 29);
            this.resetSelection.TabIndex = 16;
            this.resetSelection.Text = "Reset Selection";
            this.resetSelection.UseVisualStyleBackColor = true;
            this.resetSelection.Click += new System.EventHandler(this.onResetSelection);
            // 
            // rgSpeciesDataGridViewTextBoxColumn
            // 
            this.rgSpeciesDataGridViewTextBoxColumn.DataPropertyName = "rgSpecies";
            this.rgSpeciesDataGridViewTextBoxColumn.HeaderText = "Species";
            this.rgSpeciesDataGridViewTextBoxColumn.Name = "rgSpeciesDataGridViewTextBoxColumn";
            this.rgSpeciesDataGridViewTextBoxColumn.ReadOnly = true;
            this.rgSpeciesDataGridViewTextBoxColumn.Width = 65;
            // 
            // rgProductDataGridViewTextBoxColumn
            // 
            this.rgProductDataGridViewTextBoxColumn.DataPropertyName = "rgProduct";
            this.rgProductDataGridViewTextBoxColumn.HeaderText = "Product";
            this.rgProductDataGridViewTextBoxColumn.Name = "rgProductDataGridViewTextBoxColumn";
            this.rgProductDataGridViewTextBoxColumn.ReadOnly = true;
            this.rgProductDataGridViewTextBoxColumn.Width = 65;
            // 
            // rgLiveDeadDataGridViewTextBoxColumn
            // 
            this.rgLiveDeadDataGridViewTextBoxColumn.DataPropertyName = "rgLiveDead";
            this.rgLiveDeadDataGridViewTextBoxColumn.HeaderText = "LiveDead";
            this.rgLiveDeadDataGridViewTextBoxColumn.Name = "rgLiveDeadDataGridViewTextBoxColumn";
            this.rgLiveDeadDataGridViewTextBoxColumn.ReadOnly = true;
            this.rgLiveDeadDataGridViewTextBoxColumn.Width = 65;
            // 
            // regressGroupsBindingSource
            // 
            this.regressGroupsBindingSource.DataSource = typeof(CruiseProcessing.Models.RegressGroups);
            // 
            // LocalVolume
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(437, 437);
            this.Controls.Add(this.resetSelection);
            this.Controls.Add(this.UnitOfMeasure);
            this.Controls.Add(this.finishedButton);
            this.Controls.Add(this.doRegression);
            this.Controls.Add(this.helpButton);
            this.Controls.Add(this.regressTopwood);
            this.Controls.Add(this.numOtrees);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.SpeciesGroups);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.useNet);
            this.Controls.Add(this.useGross);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "LocalVolume";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Local Volume";
            ((System.ComponentModel.ISupportInitialize)(this.SpeciesGroups)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.regressGroupsBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView SpeciesGroups;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox numOtrees;
        private System.Windows.Forms.CheckBox regressTopwood;
        private System.Windows.Forms.Button helpButton;
        private System.Windows.Forms.Button doRegression;
        private System.Windows.Forms.Button finishedButton;
        private System.Windows.Forms.BindingSource regressGroupsBindingSource;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton useGross;
        private System.Windows.Forms.RadioButton useNet;
        private System.Windows.Forms.ComboBox UnitOfMeasure;
        private System.Windows.Forms.DataGridViewCheckBoxColumn GroupsSelected;
        private System.Windows.Forms.DataGridViewTextBoxColumn rgSpeciesDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rgProductDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rgLiveDeadDataGridViewTextBoxColumn;
        private System.Windows.Forms.Button resetSelection;
    }
}