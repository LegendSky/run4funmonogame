namespace Run4Fun
{
    partial class HiscoresForm
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
            this.hiscoresListBox = new System.Windows.Forms.ListBox();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.lbUsername = new System.Windows.Forms.Label();
            this.btnConfirm = new System.Windows.Forms.Button();
            this.lbScore = new System.Windows.Forms.Label();
            this.tbScore = new System.Windows.Forms.TextBox();
            this.btnClear = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hiscoresListBox
            // 
            this.hiscoresListBox.BackColor = System.Drawing.Color.Gold;
            this.hiscoresListBox.Font = new System.Drawing.Font("Consolas", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hiscoresListBox.FormattingEnabled = true;
            this.hiscoresListBox.ItemHeight = 75;
            this.hiscoresListBox.Location = new System.Drawing.Point(500, 100);
            this.hiscoresListBox.Name = "hiscoresListBox";
            this.hiscoresListBox.Size = new System.Drawing.Size(932, 754);
            this.hiscoresListBox.TabIndex = 4;
            // 
            // tbUsername
            // 
            this.tbUsername.Location = new System.Drawing.Point(235, 112);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(100, 20);
            this.tbUsername.TabIndex = 5;
            // 
            // lbUsername
            // 
            this.lbUsername.AutoSize = true;
            this.lbUsername.Location = new System.Drawing.Point(162, 115);
            this.lbUsername.Name = "lbUsername";
            this.lbUsername.Size = new System.Drawing.Size(58, 13);
            this.lbUsername.TabIndex = 6;
            this.lbUsername.Text = "Username:";
            // 
            // btnConfirm
            // 
            this.btnConfirm.Location = new System.Drawing.Point(367, 110);
            this.btnConfirm.Name = "btnConfirm";
            this.btnConfirm.Size = new System.Drawing.Size(75, 23);
            this.btnConfirm.TabIndex = 7;
            this.btnConfirm.Text = "Confirm";
            this.btnConfirm.UseVisualStyleBackColor = true;
            this.btnConfirm.Click += new System.EventHandler(this.btnConfirm_Click);
            // 
            // lbScore
            // 
            this.lbScore.AutoSize = true;
            this.lbScore.Location = new System.Drawing.Point(162, 147);
            this.lbScore.Name = "lbScore";
            this.lbScore.Size = new System.Drawing.Size(38, 13);
            this.lbScore.TabIndex = 9;
            this.lbScore.Text = "Score:";
            // 
            // tbScore
            // 
            this.tbScore.Location = new System.Drawing.Point(235, 144);
            this.tbScore.Name = "tbScore";
            this.tbScore.Size = new System.Drawing.Size(100, 20);
            this.tbScore.TabIndex = 8;
            // 
            // btnClear
            // 
            this.btnClear.Location = new System.Drawing.Point(60, 413);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(149, 64);
            this.btnClear.TabIndex = 10;
            this.btnClear.Text = "Clear";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // HiscoresForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Run4Fun.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.lbScore);
            this.Controls.Add(this.tbScore);
            this.Controls.Add(this.btnConfirm);
            this.Controls.Add(this.lbUsername);
            this.Controls.Add(this.tbUsername);
            this.Controls.Add(this.hiscoresListBox);
            this.Name = "HiscoresForm";
            this.Text = "Run4Fun Hiscores";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HiscoresForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox hiscoresListBox;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label lbUsername;
        private System.Windows.Forms.Button btnConfirm;
        private System.Windows.Forms.Label lbScore;
        private System.Windows.Forms.TextBox tbScore;
        private System.Windows.Forms.Button btnClear;
    }
}