namespace CruiseProcessing
{
    partial class ReportsDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReportsDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.reportsSelected = new System.Windows.Forms.ListBox();
            this.availableReports = new System.Windows.Forms.ListBox();
            this.additionalData = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.exitButton = new System.Windows.Forms.Button();
            this.addOne = new System.Windows.Forms.Button();
            this.addAll = new System.Windows.Forms.Button();
            this.removeAll = new System.Windows.Forms.Button();
            this.removeOne = new System.Windows.Forms.Button();
            this.reportGroups = new System.Windows.Forms.ComboBox();
            this.regionList = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label1.Location = new System.Drawing.Point(19, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Report Groups";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(594, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(48, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "Region";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label3.Location = new System.Drawing.Point(675, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(108, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Reports Selected";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label4.Location = new System.Drawing.Point(19, 54);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(108, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Available Reports";
            // 
            // reportsSelected
            // 
            this.reportsSelected.FormattingEnabled = true;
            this.reportsSelected.Location = new System.Drawing.Point(678, 34);
            this.reportsSelected.Name = "reportsSelected";
            this.reportsSelected.Size = new System.Drawing.Size(68, 212);
            this.reportsSelected.Sorted = true;
            this.reportsSelected.TabIndex = 6;
            // 
            // availableReports
            // 
            this.availableReports.FormattingEnabled = true;
            this.availableReports.Location = new System.Drawing.Point(22, 73);
            this.availableReports.Name = "availableReports";
            this.availableReports.SelectionMode = System.Windows.Forms.SelectionMode.MultiSimple;
            this.availableReports.Size = new System.Drawing.Size(548, 173);
            this.availableReports.TabIndex = 7;
            // 
            // additionalData
            // 
            this.additionalData.Font = new System.Drawing.Font("Arial", 9.75F);
            this.additionalData.Location = new System.Drawing.Point(22, 262);
            this.additionalData.Name = "additionalData";
            this.additionalData.Size = new System.Drawing.Size(105, 23);
            this.additionalData.TabIndex = 8;
            this.additionalData.Text = "Additional Data";
            this.additionalData.UseVisualStyleBackColor = true;
            this.additionalData.Click += new System.EventHandler(this.onAdditional);
            // 
            // cancelButton
            // 
            this.cancelButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.cancelButton.Location = new System.Drawing.Point(613, 262);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 9;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.onCancel);
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(703, 262);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 10;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // addOne
            // 
            this.addOne.Font = new System.Drawing.Font("Arial", 9.75F);
            this.addOne.Location = new System.Drawing.Point(597, 73);
            this.addOne.Name = "addOne";
            this.addOne.Size = new System.Drawing.Size(53, 23);
            this.addOne.TabIndex = 11;
            this.addOne.Text = "-->";
            this.addOne.UseVisualStyleBackColor = true;
            this.addOne.Click += new System.EventHandler(this.AddOne_Click);
            // 
            // addAll
            // 
            this.addAll.Font = new System.Drawing.Font("Arial", 9.75F);
            this.addAll.Location = new System.Drawing.Point(597, 102);
            this.addAll.Name = "addAll";
            this.addAll.Size = new System.Drawing.Size(53, 23);
            this.addAll.TabIndex = 12;
            this.addAll.Text = "==>>";
            this.addAll.UseVisualStyleBackColor = true;
            this.addAll.Click += new System.EventHandler(this.AddAll_Click);
            // 
            // removeAll
            // 
            this.removeAll.Font = new System.Drawing.Font("Arial", 9.75F);
            this.removeAll.Location = new System.Drawing.Point(597, 194);
            this.removeAll.Name = "removeAll";
            this.removeAll.Size = new System.Drawing.Size(53, 23);
            this.removeAll.TabIndex = 13;
            this.removeAll.Text = "<<==";
            this.removeAll.UseVisualStyleBackColor = true;
            this.removeAll.Click += new System.EventHandler(this.removeAll_Click);
            // 
            // removeOne
            // 
            this.removeOne.Font = new System.Drawing.Font("Arial", 9.75F);
            this.removeOne.Location = new System.Drawing.Point(597, 223);
            this.removeOne.Name = "removeOne";
            this.removeOne.Size = new System.Drawing.Size(53, 23);
            this.removeOne.TabIndex = 14;
            this.removeOne.Text = "<--";
            this.removeOne.UseVisualStyleBackColor = true;
            this.removeOne.Click += new System.EventHandler(this.removeOne_Click);
            // 
            // reportGroups
            // 
            this.reportGroups.FormattingEnabled = true;
            this.reportGroups.Items.AddRange(new object[] {
            "A--Tree level information reports",
            "BLM--BLM reports",
            "EX--Variable log length (Export grade) reports",
            "L--Log level reports",
            "LD--Live/Dead Summary reports",
            "LV--Leave Tree Reports",
            "R--Regional reports",
            "SC--Stem count reports (FIXCNT Method only)",
            "ST--Statistic reports for sample populations & strata/sale",
            "TC--Stand tables --Cut trees only",
            "TL--Stand tables -- Leave trees only",
            "TIM--SUM File",
            "UC--Unit level reports",
            "VSM--Volume Summary reports",
            "VPA--Volume Per Acre Summary reports",
            "VAL--Dollar Value reports",
            "WT--Weight reports"});
            this.reportGroups.Location = new System.Drawing.Point(22, 26);
            this.reportGroups.Name = "reportGroups";
            this.reportGroups.Size = new System.Drawing.Size(287, 21);
            this.reportGroups.TabIndex = 15;
            this.reportGroups.SelectedIndexChanged += new System.EventHandler(this.onSelectedIndexChanged);
            // 
            // regionList
            // 
            this.regionList.FormattingEnabled = true;
            this.regionList.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "8",
            "9",
            "10",
            "IDL"});
            this.regionList.Location = new System.Drawing.Point(597, 26);
            this.regionList.Name = "regionList";
            this.regionList.Size = new System.Drawing.Size(45, 21);
            this.regionList.TabIndex = 16;
            this.regionList.SelectedIndexChanged += new System.EventHandler(this.onRegionSelectedIndexChanged);
            // 
            // ReportsDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(791, 297);
            this.Controls.Add(this.regionList);
            this.Controls.Add(this.reportGroups);
            this.Controls.Add(this.removeOne);
            this.Controls.Add(this.removeAll);
            this.Controls.Add(this.addAll);
            this.Controls.Add(this.addOne);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.additionalData);
            this.Controls.Add(this.availableReports);
            this.Controls.Add(this.reportsSelected);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ReportsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Reports Selection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListBox reportsSelected;
        private System.Windows.Forms.ListBox availableReports;
        private System.Windows.Forms.Button additionalData;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button addOne;
        private System.Windows.Forms.Button addAll;
        private System.Windows.Forms.Button removeAll;
        private System.Windows.Forms.Button removeOne;
        private System.Windows.Forms.ComboBox reportGroups;
        private System.Windows.Forms.ComboBox regionList;
    }
}