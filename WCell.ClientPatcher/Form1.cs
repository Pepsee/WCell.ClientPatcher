using System;
using System.Windows.Forms;

namespace WCell.ClientPatcher
{
    public partial class Form1 : Form
    {
        private string fileName;

        public Form1()
        {
            InitializeComponent();
        }

        private void Patch_Click(object sender, EventArgs e)
        {
            richTextBox1.AppendText("Patching in progress, please wait...\n");
            string result;
            var patcher = new ClientPatcher(fileName);
            patcher.Patch(richTextBox1);
        }

        private void Open_Click(object sender, EventArgs e)
        {
            // Display the openFile dialog.
            DialogResult result = openFileDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                fileName = openFileDialog1.FileName;
                textBox1.Text = fileName;
                Patch.Enabled = true;
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }
            
        }
    }
}
