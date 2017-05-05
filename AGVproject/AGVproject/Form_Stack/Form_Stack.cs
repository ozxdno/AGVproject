using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AGVproject.Form_Stack
{
    public partial class Form_Stack : Form
    {
        public Form_Stack()
        {
            InitializeComponent();
        }

        public static bool CANCLE = false;

        public static int StackNo;
        public static int Direction;
        public static double Distance;

        public static double Length;
        public new static double Width;

        public static double AisleWidth_U;
        public static double AisleWidth_D;
        public static double AisleWidth_L;
        public static double AisleWidth_R;

        public static double SetKeepU;
        public static double SetKeepD;
        public static double SetKeepL;
        public static double SetKeepR;

        private void Form_Stack_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { Direction = int.Parse(this.iDirection.Text); } catch { }
            try { Distance = double.Parse(this.iDistance.Text); } catch { }

            try { Length = double.Parse(this.iLength.Text); } catch { }
            try { Width = double.Parse(this.iWidth.Text); } catch { }

            try { AisleWidth_U = double.Parse(this.iAisleWidthU.Text); } catch { }
            try { AisleWidth_D = double.Parse(this.iAisleWidthD.Text); } catch { }
            try { AisleWidth_L = double.Parse(this.iAisleWidthL.Text); } catch { }
            try { AisleWidth_R = double.Parse(this.iAisleWidthR.Text); } catch { }

            try { SetKeepU = double.Parse(this.iSetKeepU.Text); } catch { }
            try { SetKeepD = double.Parse(this.iSetKeepD.Text); } catch { }
            try { SetKeepL = double.Parse(this.iSetKeepL.Text); } catch { }
            try { SetKeepR = double.Parse(this.iSetKeepR.Text); } catch { }
        }

        private void Form_Stack_Load(object sender, EventArgs e)
        {
            CANCLE = true;

            this.StackNoLabel.Text = StackNo.ToString();
            if (StackNo == 0) { this.StackNoLabel.Text = "门"; }
            this.iDirection.SelectedIndex = Direction;
            this.iDistance.Text = Distance.ToString();

            this.iLength.Text = Length.ToString();
            this.iWidth.Text = Width.ToString();

            this.iAisleWidthU.Text = AisleWidth_U.ToString();
            this.iAisleWidthD.Text = AisleWidth_D.ToString();
            this.iAisleWidthL.Text = AisleWidth_L.ToString();
            this.iAisleWidthR.Text = AisleWidth_R.ToString();

            this.iSetKeepU.Text = SetKeepU.ToString();
            this.iSetKeepD.Text = SetKeepD.ToString();
            this.iSetKeepL.Text = SetKeepL.ToString();
            this.iSetKeepR.Text = SetKeepR.ToString();
        }

        private void Confirm_Click(object sender, EventArgs e)
        {
            CANCLE = false; MessageBox.Show("Confirmed !");
        }
    }
}
