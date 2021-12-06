namespace CruiseProcessing
{
    partial class SecurityQuestion
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SecurityQuestion));
            this.label1 = new System.Windows.Forms.Label();
            this.securityAnswer = new System.Windows.Forms.TextBox();
            this.ok_Button = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(217, 54);
            this.label1.TabIndex = 0;
            this.label1.Text = "What is the name of the vet who treated Smokey Bear?  Full name please and all up" +
                "percase.";
            // 
            // securityAnswer
            // 
            this.securityAnswer.Location = new System.Drawing.Point(40, 66);
            this.securityAnswer.Name = "securityAnswer";
            this.securityAnswer.PasswordChar = '*';
            this.securityAnswer.Size = new System.Drawing.Size(148, 20);
            this.securityAnswer.TabIndex = 1;
            // 
            // ok_Button
            // 
            this.ok_Button.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold);
            this.ok_Button.Location = new System.Drawing.Point(79, 106);
            this.ok_Button.Name = "ok_Button";
            this.ok_Button.Size = new System.Drawing.Size(75, 23);
            this.ok_Button.TabIndex = 2;
            this.ok_Button.Text = "ACCEPT";
            this.ok_Button.UseVisualStyleBackColor = true;
            this.ok_Button.Click += new System.EventHandler(this.onOK);
            // 
            // SecurityQuestion
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(241, 141);
            this.Controls.Add(this.ok_Button);
            this.Controls.Add(this.securityAnswer);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SecurityQuestion";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = " Security Question";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox securityAnswer;
        private System.Windows.Forms.Button ok_Button;
    }
}