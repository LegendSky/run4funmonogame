using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Run4Fun
{
    public partial class HiscoresForm : Form
    {
        private List<string> hiscores = new List<string>(10);
        private const string fileName = "hiscores.txt";

        public HiscoresForm()
        {
            InitializeComponent();

            if (!File.Exists(fileName))
            {
                StreamWriter sw = new StreamWriter(fileName);
                sw.Close();
            }

            convertTxtToList();
        }

        public HiscoresForm(string username, int score) : this()
        {
            writeNewHiscoreToTxt(username, score);
            convertTxtToList();
        }

        private void clearTxtFile()
        {
            StreamWriter sw = new StreamWriter(fileName);
            sw.Close();
        }

        private void writeNewHiscoreToTxt(string name, int score)
        {
            StreamWriter sw = new StreamWriter(fileName, true);
            sw.WriteLine(name + " " + score);
            sw.Close();
        }

        private void convertTxtToList()
        {
            StreamReader sr = new StreamReader(fileName);

            hiscores.Clear();

            while (sr.Peek() >= 0)
            {
                string line = sr.ReadLine();
                hiscores.Add(line);
            }

            sortList();

            hiscoresListBox.Items.Clear();

            for (int i = 0; i < hiscores.Count; i++)
            {
                string[] array = hiscores[i].Split(' ');
                hiscoresListBox.Items.Add(i + 1 + ". " + array[0].PadRight(15) + array[1]);
            }

            sr.Close();
        }

        private void sortList()
        {
            List<string> sortedList = new List<string>();

            while (hiscores.Count > 0)
            {
                sortedList.Add(findMaxValueInList(hiscores));
                hiscores.Remove(findMaxValueInList(hiscores));
            }

            hiscores = sortedList;
        }

        private string findMaxValueInList(List<string> list)
        {
            string highestText = list[0];
            int highest = getScoreFromHiscore(list[0]);
            foreach (string text in list)
            {
                int score = getScoreFromHiscore(text);
                if (score >= highest)
                {
                    highest = score;
                    highestText = text;
                }
            }
            return highestText;

        }

        private int getScoreFromHiscore(string hiscore)
        {
            string[] array = hiscore.Split(' ');
            return Convert.ToInt32(array[1]);
        }

        private void HiscoresForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Hide();
            new StartForm().ShowDialog();
            Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            clearTxtFile();
            convertTxtToList();
        }
    }
}
