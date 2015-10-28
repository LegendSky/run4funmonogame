namespace Run4Fun
{
    partial class StartForm
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
            this.hiscoresButton = new System.Windows.Forms.Button();
            this.quitButton = new System.Windows.Forms.Button();
            this.runButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // hiscoresButton
            // 
            this.hiscoresButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.hiscoresButton.Font = new System.Drawing.Font("Impact", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hiscoresButton.Image = global::Run4Fun.Properties.Resources.hiscores_button_off;
            this.hiscoresButton.Location = new System.Drawing.Point(1200, 400);
            this.hiscoresButton.Name = "hiscoresButton";
            this.hiscoresButton.Size = new System.Drawing.Size(330, 162);
            this.hiscoresButton.TabIndex = 4;
            this.hiscoresButton.UseVisualStyleBackColor = true;
            this.hiscoresButton.Click += new System.EventHandler(this.hiscoresButton_Click);
            this.hiscoresButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.hiscoresButton_MouseDown);
            this.hiscoresButton.MouseEnter += new System.EventHandler(this.hiscoresButton_MouseEnter);
            this.hiscoresButton.MouseLeave += new System.EventHandler(this.hiscoresButton_MouseLeave);
            // 
            // quitButton
            // 
            this.quitButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.quitButton.Font = new System.Drawing.Font("Impact", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.quitButton.Image = global::Run4Fun.Properties.Resources.quit_button_off;
            this.quitButton.Location = new System.Drawing.Point(1200, 600);
            this.quitButton.Name = "quitButton";
            this.quitButton.Size = new System.Drawing.Size(330, 162);
            this.quitButton.TabIndex = 5;
            this.quitButton.UseVisualStyleBackColor = true;
            this.quitButton.Click += new System.EventHandler(this.quitButton_Click);
            this.quitButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.quitButton_MouseDown);
            this.quitButton.MouseEnter += new System.EventHandler(this.quitButton_MouseEnter);
            this.quitButton.MouseLeave += new System.EventHandler(this.quitButton_MouseLeave);
            // 
            // runButton
            // 
            this.runButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.runButton.Font = new System.Drawing.Font("Impact", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.runButton.Image = global::Run4Fun.Properties.Resources.run_button_off;
            this.runButton.Location = new System.Drawing.Point(1200, 200);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(330, 162);
            this.runButton.TabIndex = 6;
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            this.runButton.MouseDown += new System.Windows.Forms.MouseEventHandler(this.runButton_MouseDown);
            this.runButton.MouseEnter += new System.EventHandler(this.runButton_MouseEnter);
            this.runButton.MouseLeave += new System.EventHandler(this.runButton_MouseLeave);
            // 
            // StartForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Run4Fun.Properties.Resources.bg;
            this.ClientSize = new System.Drawing.Size(1904, 1041);
            this.Controls.Add(this.runButton);
            this.Controls.Add(this.quitButton);
            this.Controls.Add(this.hiscoresButton);
            this.Name = "StartForm";
            this.Text = "Run4Fun";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.StartForm_Paint);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button hiscoresButton;
        private System.Windows.Forms.Button quitButton;
        private System.Windows.Forms.Button runButton;
    }
}