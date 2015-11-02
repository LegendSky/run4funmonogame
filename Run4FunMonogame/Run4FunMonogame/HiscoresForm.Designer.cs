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
            this.btnClear = new System.Windows.Forms.Button();
            this.btnBackStart = new System.Windows.Forms.Button();
            this.lbUsername = new System.Windows.Forms.Label();
            this.lbScore = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // hiscoresListBox
            // 
            this.hiscoresListBox.BackColor = System.Drawing.Color.Gold;
            this.hiscoresListBox.Font = new System.Drawing.Font("Consolas", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hiscoresListBox.FormattingEnabled = true;
            this.hiscoresListBox.ItemHeight = 75;
            this.hiscoresListBox.Location = new System.Drawing.Point(460, 175);
            this.hiscoresListBox.Name = "hiscoresListBox";
            this.hiscoresListBox.Size = new System.Drawing.Size(1000, 679);
            this.hiscoresListBox.TabIndex = 4;
            // 
            // btnClear
            // 
            this.btnClear.ForeColor = System.Drawing.Color.Red;
            this.btnClear.Location = new System.Drawing.Point(1600, 790);
            this.btnClear.Name = "btnClear";
            this.btnClear.Size = new System.Drawing.Size(149, 64);
            this.btnClear.TabIndex = 10;
            this.btnClear.Text = "Clear hiscores";
            this.btnClear.UseVisualStyleBackColor = true;
            this.btnClear.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnBackStart
            // 
            this.btnBackStart.ForeColor = System.Drawing.Color.Black;
            this.btnBackStart.Location = new System.Drawing.Point(1600, 690);
            this.btnBackStart.Name = "btnBackStart";
            this.btnBackStart.Size = new System.Drawing.Size(149, 64);
            this.btnBackStart.TabIndex = 11;
            this.btnBackStart.Text = "Back to start";
            this.btnBackStart.UseVisualStyleBackColor = true;
            this.btnBackStart.Click += new System.EventHandler(this.btnBackStart_Click);
            // 
            // lbUsername
            // 
            this.lbUsername.AutoSize = true;
            this.lbUsername.BackColor = System.Drawing.Color.Transparent;
            this.lbUsername.Font = new System.Drawing.Font("Consolas", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbUsername.ForeColor = System.Drawing.Color.Gold;
            this.lbUsername.Location = new System.Drawing.Point(447, 88);
            this.lbUsername.Name = "lbUsername";
            this.lbUsername.Size = new System.Drawing.Size(312, 75);
            this.lbUsername.TabIndex = 12;
            this.lbUsername.Text = "Username";
            // 
            // lbScore
            // 
            this.lbScore.AutoSize = true;
            this.lbScore.BackColor = System.Drawing.Color.Transparent;
            this.lbScore.Font = new System.Drawing.Font("Consolas", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lbScore.ForeColor = System.Drawing.Color.Gold;
            this.lbScore.Location = new System.Drawing.Point(1155, 88);
            this.lbScore.Name = "lbScore";
            this.lbScore.Size = new System.Drawing.Size(207, 75);
            this.lbScore.TabIndex = 17;
            this.lbScore.Text = "Score";
            // 
            // HiscoresForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Run4Fun.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.lbScore);
            this.Controls.Add(this.lbUsername);
            this.Controls.Add(this.btnBackStart);
            this.Controls.Add(this.btnClear);
            this.Controls.Add(this.hiscoresListBox);
            this.Name = "HiscoresForm";
            this.Text = "Run4Fun Hiscores";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HiscoresForm_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListBox hiscoresListBox;
        private System.Windows.Forms.Button btnClear;
        private System.Windows.Forms.Button btnBackStart;
        private System.Windows.Forms.Label lbUsername;
        private System.Windows.Forms.Label lbScore;
    }
}