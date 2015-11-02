using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Run4Fun
{
    public partial class HiscoresForm : Form
    {
        private List<string> hiscores = new List<string>();
        private const string fileName = "hiscores.txt";

        public HiscoresForm()
        {
            InitializeComponent();

            Console.WriteLine(DateTime.Now.ToString("HH:mm:ss"));

            if (!File.Exists(fileName))
            {
                StreamWriter sw = new StreamWriter(fileName);
                sw.Close();
            }

            convertTxtToListAndPutInListBox();
        }

        public HiscoresForm(string username, int score) : this()
        {
            writeNewHiscoreToTxt(username, score);
            convertTxtToListAndPutInListBox();
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

        private void convertTxtToListAndPutInListBox()
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
                int count = i + 1;
                hiscoresListBox.Items.Add(count + ". " + array[0].PadRight(18 - count.ToString().Length) + array[1]);
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
            backToStartForm();
        }

        private void backToStartForm()
        {
            Hide();
            new StartForm().ShowDialog();
            Close();
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            clearTxtFile();
            convertTxtToListAndPutInListBox();
        }

        private void btnBackStart_Click(object sender, EventArgs e)
        {
            backToStartForm();
        }
    }
}
