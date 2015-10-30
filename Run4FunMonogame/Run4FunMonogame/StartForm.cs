﻿using EV3MessengerLib;
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
    public partial class StartForm : Form
    {
        private EV3Messenger ev3Messenger;
        private const string EV3_SERIAL_PORT = "COM6";

        public StartForm()
        {
            InitializeComponent();

            // EV3: Create an EV3Messenger object which you can use to talk to the EV3.
            ev3Messenger = new EV3Messenger();

            // EV3: Connect to the EV3 serial port over Bluetooth.
            ev3Messenger.Connect(EV3_SERIAL_PORT);
        }

        private void StartForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawImage(Properties.Resources.logo, 40, 20);//Draw logo
        }

        // Start of run button methods.
        private void runButton_Click(object sender, EventArgs e)
        {
            Hide();
            new Run4FunGame(ev3Messenger).Run();
            Close();
        }
        private void runButton_MouseEnter(object sender, EventArgs e)
        {
            // Button glow on hover.
            runButton.Image = Properties.Resources.run_button_on;
        }
        private void runButton_MouseLeave(object sender, EventArgs e)
        {
            // Button without glow.
            runButton.Image = Properties.Resources.run_button_off;
        }
        private void runButton_MouseDown(object sender, MouseEventArgs e)
        {
            // Button pressed.
            runButton.Image = Properties.Resources.run_button_off;//TODO: replace with button press image?
        }

        // Start of hiscores button methods.
        private void hiscoresButton_Click(object sender, EventArgs e)
        {
            Hide();
            new HiscoresForm().ShowDialog();
            Close();
        }

        private void hiscoresButton_MouseEnter(object sender, EventArgs e)
        {
            hiscoresButton.Image = Properties.Resources.hiscores_button_on;
        }

        private void hiscoresButton_MouseLeave(object sender, EventArgs e)
        {
            hiscoresButton.Image = Properties.Resources.hiscores_button_off;
        }
        private void hiscoresButton_MouseDown(object sender, MouseEventArgs e)
        {
            hiscoresButton.Image = Properties.Resources.hiscores_button_off;
        }

        // Start of quit button methods.
        private void quitButton_Click(object sender, EventArgs e)
        {
            disconnectEV3AndClose();
        }

        private void quitButton_MouseEnter(object sender, EventArgs e)
        {
            quitButton.Image = Properties.Resources.quit_button_on;
        }

        private void quitButton_MouseLeave(object sender, EventArgs e)
        {
            quitButton.Image = Properties.Resources.quit_button_off;
        }

        private void quitButton_MouseDown(object sender, MouseEventArgs e)
        {
            quitButton.Image = Properties.Resources.quit_button_off;
        }

        private void StartForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            disconnectEV3AndClose();
        }

        private void disconnectEV3AndClose()
        {
            ev3Messenger.Disconnect();
            Environment.Exit(0);
        }
    }
}
