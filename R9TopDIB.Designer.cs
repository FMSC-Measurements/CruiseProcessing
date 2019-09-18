namespace CruiseProcessing
{
    partial class R9TopDIB
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(R9TopDIB));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.DIBspecies = new System.Windows.Forms.TextBox();
            this.sawtimberDIB = new System.Windows.Forms.TextBox();
            this.nonSawDIB = new System.Windows.Forms.TextBox();
            this.completedDIBs = new System.Windows.Forms.DataGridView();
            this.exitButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.completedDIBs)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(21, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(325, 63);
            this.label1.TabIndex = 0;
            this.label1.Text = resources.GetString("label1.Text");
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Arial", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(47, 72);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(288, 50);
            this.label2.TabIndex = 1;
            this.label2.Text = "*Sawtimber top DIB/DOB is only used to calculate sawtimber heights.  If sawtimber" +
    " height was measured for a tree, the calculated top DIB/DOB at that height is us" +
    "ed.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label3.Location = new System.Drawing.Point(21, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "SPECIES";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label4.Location = new System.Drawing.Point(122, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(92, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "SAWTIMBER*";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Arial", 9.75F);
            this.label5.Location = new System.Drawing.Point(240, 122);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "NON-SAW";
            // 
            // DIBspecies
            // 
            this.DIBspecies.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.DIBspecies.Location = new System.Drawing.Point(24, 141);
            this.DIBspecies.Name = "DIBspecies";
            this.DIBspecies.ReadOnly = true;
            this.DIBspecies.Size = new System.Drawing.Size(58, 13);
            this.DIBspecies.TabIndex = 5;
            // 
            // sawtimberDIB
            // 
            this.sawtimberDIB.Location = new System.Drawing.Point(145, 141);
            this.sawtimberDIB.Name = "sawtimberDIB";
            this.sawtimberDIB.Size = new System.Drawing.Size(42, 20);
            this.sawtimberDIB.TabIndex = 6;
            this.sawtimberDIB.Leave += new System.EventHandler(this.onSawChanged);
            // 
            // nonSawDIB
            // 
            this.nonSawDIB.Location = new System.Drawing.Point(252, 141);
            this.nonSawDIB.Name = "nonSawDIB";
            this.nonSawDIB.Size = new System.Drawing.Size(42, 20);
            this.nonSawDIB.TabIndex = 7;
            this.nonSawDIB.Leave += new System.EventHandler(this.onNonSawChanged);
            // 
            // completedDIBs
            // 
            this.completedDIBs.AllowUserToAddRows = false;
            this.completedDIBs.AllowUserToDeleteRows = false;
            this.completedDIBs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.completedDIBs.Location = new System.Drawing.Point(24, 178);
            this.completedDIBs.Name = "completedDIBs";
            this.completedDIBs.ReadOnly = true;
            this.completedDIBs.Size = new System.Drawing.Size(311, 107);
            this.completedDIBs.TabIndex = 8;
            this.completedDIBs.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.onCellEnter);
            // 
            // exitButton
            // 
            this.exitButton.Font = new System.Drawing.Font("Arial", 9.75F);
            this.exitButton.Location = new System.Drawing.Point(245, 302);
            this.exitButton.Name = "exitButton";
            this.exitButton.Size = new System.Drawing.Size(90, 23);
            this.exitButton.TabIndex = 9;
            this.exitButton.Text = "FINISHED";
            this.exitButton.UseVisualStyleBackColor = true;
            this.exitButton.Click += new System.EventHandler(this.onFinished);
            // 
            // R9TopDIB
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(356, 337);
            this.Controls.Add(this.exitButton);
            this.Controls.Add(this.completedDIBs);
            this.Controls.Add(this.nonSawDIB);
            this.Controls.Add(this.sawtimberDIB);
            this.Controls.Add(this.DIBspecies);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "R9TopDIB";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " Minimum Top DIBs/DOBs";
            ((System.ComponentModel.ISupportInitialize)(this.completedDIBs)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox DIBspecies;
        private System.Windows.Forms.TextBox sawtimberDIB;
        private System.Windows.Forms.TextBox nonSawDIB;
        private System.Windows.Forms.DataGridView completedDIBs;
        private System.Windows.Forms.Button exitButton;
    }
}