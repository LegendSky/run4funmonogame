using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Run4Fun
{
    public partial class HiscoresForm : Form
    {
        public List<string> scores = new List<string>(10);
        public int score = 0;
        private const string fileName = "test.txt";

        public HiscoresForm()
        {
            InitializeComponent();

            for (int i = 0; i < 10; i++)
            {
                scores.Add("-");
            }
        }

        public HiscoresForm(int score)
        {
            InitializeComponent();

            /*for (int i = 0; i < 10; i++)
            {
                scores.Add("-");
            }*/

            scores.Add("score: " + score);


        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            StreamWriter sw = new StreamWriter(fileName);
            //sw.Write("First line");
            //sw.Write("Second line");
            //sw.WriteLine("gewagwea");
            //sw.WriteLine("geawgawe");
            sw.Close();
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(fileName);
            while (sr.Peek() >= 0)
            {
                hiscoresListBox.Items.Add(sr.ReadLine());
            }
        }

        private void btnYiss_Click(object sender, EventArgs e)
        {
            StreamReader sr = new StreamReader(fileName);
            while (sr.Peek() >= 0)
            {
                scores.Add(sr.ReadLine());
            }
        }

        public void SaveHighScore()
        {
            //if (score > scores[9])
            //    scores[9] = score;

            scores.Sort();
            scores.Reverse();

            IsolatedStorageFile highScoreStorage = IsolatedStorageFile.GetUserStoreForDomain();

            IsolatedStorageFileStream userDataFile = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, highScoreStorage);

            // create a writer to the stream...
            StreamWriter writeStream = new StreamWriter(userDataFile);

            // write strings to the Isolated Storage file...
            for (int i = 0; i < 10; i++)
            {
                writeStream.WriteLine(scores[i].ToString());
            }

            // Tidy up by flushing the stream buffer and then closing
            // the streams...
            writeStream.Flush();
            writeStream.Close();
            userDataFile.Close();
        }

        public void LoadHighScore()
        {
            if (System.IO.File.Exists(fileName))
            {
                IsolatedStorageFile highScoreStorage = IsolatedStorageFile.GetUserStoreForDomain();

                if (highScoreStorage.FileExists(fileName))
                {
                    IsolatedStorageFileStream userDataFile = new IsolatedStorageFileStream(fileName, FileMode.Open, highScoreStorage);

                    // create a reader to the stream...
                    StreamReader readStream = new StreamReader(userDataFile);

                    for (int i = 0; i < 10; i++)
                    {
                        //scores[i] = Convert.ToInt32(readStream.ReadLine());
                    }

                    scores.Sort();
                    scores.Reverse();

                    // Tidy up by closing the streams...
                    readStream.Close();
                    userDataFile.Close();
                }
                else
                {
                    SaveHighScore();
                }
            }
            else
            {
                System.IO.File.Create(fileName);

                SaveHighScore();
            }

        }

        private void HiscoresForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Hide();
            new StartForm().ShowDialog();
            Close();
        }
    }
}
