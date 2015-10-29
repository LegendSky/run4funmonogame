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
            this.btnWrite = new System.Windows.Forms.Button();
            this.btnYiss = new System.Windows.Forms.Button();
            this.btnRead = new System.Windows.Forms.Button();
            this.hiscoresListBox = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btnWrite
            // 
            this.btnWrite.Location = new System.Drawing.Point(71, 273);
            this.btnWrite.Name = "btnWrite";
            this.btnWrite.Size = new System.Drawing.Size(149, 64);
            this.btnWrite.TabIndex = 0;
            this.btnWrite.Text = "Write txt";
            this.btnWrite.UseVisualStyleBackColor = true;
            this.btnWrite.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnYiss
            // 
            this.btnYiss.Location = new System.Drawing.Point(71, 203);
            this.btnYiss.Name = "btnYiss";
            this.btnYiss.Size = new System.Drawing.Size(149, 64);
            this.btnYiss.TabIndex = 1;
            this.btnYiss.Text = "Yiss";
            this.btnYiss.UseVisualStyleBackColor = true;
            this.btnYiss.Click += new System.EventHandler(this.btnYiss_Click);
            // 
            // btnRead
            // 
            this.btnRead.Location = new System.Drawing.Point(71, 343);
            this.btnRead.Name = "btnRead";
            this.btnRead.Size = new System.Drawing.Size(149, 64);
            this.btnRead.TabIndex = 2;
            this.btnRead.Text = "Read txt";
            this.btnRead.UseVisualStyleBackColor = true;
            this.btnRead.Click += new System.EventHandler(this.btnRead_Click);
            // 
            // hiscoresListBox
            // 
            this.hiscoresListBox.BackColor = System.Drawing.Color.Gold;
            this.hiscoresListBox.Font = new System.Drawing.Font("Minecrafter", 27.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hiscoresListBox.FormattingEnabled = true;
            this.hiscoresListBox.ItemHeight = 33;
            this.hiscoresListBox.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10"});
            this.hiscoresListBox.Location = new System.Drawing.Point(310, 100);
            this.hiscoresListBox.Name = "hiscoresListBox";
            this.hiscoresListBox.Size = new System.Drawing.Size(752, 466);
            this.hiscoresListBox.TabIndex = 4;
            // 
            // HiscoresForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Run4Fun.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.hiscoresListBox);
            this.Controls.Add(this.btnRead);
            this.Controls.Add(this.btnYiss);
            this.Controls.Add(this.btnWrite);
            this.Name = "HiscoresForm";
            this.Text = "Run4Fun Hiscores";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.HiscoresForm_FormClosed);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnWrite;
        private System.Windows.Forms.Button btnYiss;
        private System.Windows.Forms.Button btnRead;
        private System.Windows.Forms.ListBox hiscoresListBox;
    }
}