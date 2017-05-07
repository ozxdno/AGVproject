using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGVproject.Form_Input
{
    public partial class Form_Input : Form
    {
        public Form_Input()
        {
            InitializeComponent();
        }

        public string Input = "";

        private void getInput(object sender, KeyEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            if (e.KeyValue != 13) { return; }
            Input = textbox.Text;

            this.Close();
        }

        private void Form_Input_Load(object sender, EventArgs e)
        {
            this.Location = MousePosition; this.textBox1.Text = Input;
        }
    }
}
