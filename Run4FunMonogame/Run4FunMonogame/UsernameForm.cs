using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Run4Fun
{
    public partial class UsernameForm : Form
    {
        private int score;

        public UsernameForm(int score)
        {
            InitializeComponent();
            this.score = score;
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            Hide();
            new HiscoresForm(tbUsername.Text ,score).ShowDialog();
            Close();
        }

        private void btnSkip_Click(object sender, EventArgs e)
        {
            Hide();
            new StartForm().ShowDialog();
            Close();
        }
    }
}
