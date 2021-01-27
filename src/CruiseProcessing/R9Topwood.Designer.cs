namespace CruiseProcessing
{
    partial class R9Topwood
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R9Topwood));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.exitButton = new System.Windows.Forms.Button();
            this.excludeALL = new System.Windows.Forms.Button();
            this.excludeOne = new System.Windows.Forms.Button();
            this.includeOne = new System.Windows.Forms.Button();
            this.includeALL = new System.Windows.Forms.Button();
            this.speciesToInclude = new System.Windows.Forms.ListBox();
            this.speciesToExclude = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(305, 79);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label2.Location = new System.Drawing.Point(12, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(135, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "INCLUDE TOPWOOD";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label3.Location = new System.Drawing.Point(217, 72);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "EXCLUDE TOPWOOD";
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(247, 263);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(75, 23);
            this.exitButton.TabIndex = 5;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // excludeALL
            // 
            this.excludeALL.Font = new System.Drawing.Font("Arial", 9.75F);
            this.excludeALL.Location = new System.Drawing.Point(145, 97);
            this.excludeALL.Name = "excludeALL";
            this.excludeALL.Size = new System.Drawing.Size(66, 27);
            this.excludeALL.TabIndex = 6;
            this.excludeALL.Text = "ALL >>";
            this.excludeALL.UseVisualStyleBackColor = true;
            this.excludeALL.Click += new System.EventHandler(this.onExcludeAll);
            // 
            // excludeOne
            // 
            this.excludeOne.Font = new System.Drawing.Font("Arial", 9.75F);
            this.excludeOne.Location = new System.Drawing.Point(145, 142);
            this.excludeOne.Name = "excludeOne";
            this.excludeOne.Size = new System.Drawing.Size(66, 27);
            this.excludeOne.TabIndex = 7;
            this.excludeOne.Text = ">";
            this.excludeOne.UseVisualStyleBackColor = true;
            this.excludeOne.Click += new System.EventHandler(this.onExcludeOne);
            // 
            // includeOne
            // 
            this.includeOne.Font = new System.Drawing.Font("Arial", 9.75F);
            this.includeOne.Location = new System.Drawing.Point(145, 175);
            this.includeOne.Name = "includeOne";
            this.includeOne.Size = new System.Drawing.Size(66, 27);
            this.includeOne.TabIndex = 8;
            this.includeOne.Text = "<";
            this.includeOne.UseVisualStyleBackColor = true;
            this.includeOne.Click += new System.EventHandler(this.onIncludeOne);
            // 
            // includeALL
            // 
            this.includeALL.Font = new System.Drawing.Font("Arial", 9.75F);
            this.includeALL.Location = new System.Drawing.Point(145, 223);
            this.includeALL.Name = "includeALL";
            this.includeALL.Size = new System.Drawing.Size(66, 27);
            this.includeALL.TabIndex = 9;
            this.includeALL.Text = "<< ALL";
            this.includeALL.UseVisualStyleBackColor = true;
            this.includeALL.Click += new System.EventHandler(this.onIncludeAll);
            // 
            // speciesToInclude
            // 
            this.speciesToInclude.FormattingEnabled = true;
            this.speciesToInclude.Location = new System.Drawing.Point(39, 97);
            this.speciesToInclude.Name = "speciesToInclude";
            this.speciesToInclude.Size = new System.Drawing.Size(70, 147);
            this.speciesToInclude.TabIndex = 10;
            // 
            // speciesToExclude
            // 
            this.speciesToExclude.FormattingEnabled = true;
            this.speciesToExclude.Location = new System.Drawing.Point(247, 97);
            this.speciesToExclude.Name = "speciesToExclude";
            this.speciesToExclude.Size = new System.Drawing.Size(70, 147);
            this.speciesToExclude.TabIndex = 11;
            // 
            // R9Topwood
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(368, 298);
            this.Controls.Add(this.speciesToExclude);
            this.Controls.Add(this.speciesToInclude);
            this.Controls.Add(this.includeALL);
            this.Controls.Add(this.includeOne);
            this.Controls.Add(this.excludeOne);
            this.Controls.Add(this.excludeALL);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "R9Topwood";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " R9 Topwood Status";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button exitButton;
        private System.Windows.Forms.Button excludeALL;
        private System.Windows.Forms.Button excludeOne;
        private System.Windows.Forms.Button includeOne;
        private System.Windows.Forms.Button includeALL;
        private System.Windows.Forms.ListBox speciesToInclude;
        private System.Windows.Forms.ListBox speciesToExclude;
    }
}