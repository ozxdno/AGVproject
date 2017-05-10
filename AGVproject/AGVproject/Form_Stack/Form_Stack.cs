using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using AGVproject.Class;

namespace AGVproject.Form_Stack
{
    public partial class Form_Stack : Form
    {
        public Form_Stack(int No)
        {
            InitializeComponent(); StackNo = No;
        }
        
        private static int StackNo;
        private static double StackLength;
        private static double StackWidth;

        private static double AisleWidth_U;
        private static double AisleWidth_D;
        private static double AisleWidth_L;
        private static double AisleWidth_R;

        private static double SetKeepU;
        private static double SetKeepD;
        private static double SetKeepL;
        private static double SetKeepR;

        private static double MapZoomRate;

        private void Form_Stack_FormClosing(object sender, FormClosingEventArgs e)
        {
            try { StackLength = double.Parse(this.iLength.Text); } catch { }
            try { StackWidth = double.Parse(this.iWidth.Text); } catch { }

            try { AisleWidth_U = double.Parse(this.iAisleWidthU.Text); } catch { }
            try { AisleWidth_D = double.Parse(this.iAisleWidthD.Text); } catch { }
            try { AisleWidth_L = double.Parse(this.iAisleWidthL.Text); } catch { }
            try { AisleWidth_R = double.Parse(this.iAisleWidthR.Text); } catch { }

            try { SetKeepU = double.Parse(this.iSetKeepU.Text); } catch { }
            try { SetKeepD = double.Parse(this.iSetKeepD.Text); } catch { }
            try { SetKeepL = double.Parse(this.iSetKeepL.Text); } catch { }
            try { SetKeepR = double.Parse(this.iSetKeepR.Text); } catch { }

            try { MapZoomRate = double.Parse(this.textBox1.Text); } catch { }

            HouseMap.PixLength = MapZoomRate;

            if (StackNo == -1)
            {
                HouseStack.DefaultStackLength = StackLength;
                HouseStack.DefaultStackWidth = StackWidth;
                HouseStack.DefaultAisleWidthLR = Math.Min(AisleWidth_L, AisleWidth_R);
                HouseStack.DefaultAisleWidthUD = Math.Min(AisleWidth_U, AisleWidth_D);
                return;
            }

            HouseStack.STACK stack = HouseStack.getStack(StackNo);
            stack.Length = StackLength;
            stack.Width = StackWidth;
            stack.AisleWidth_U = AisleWidth_U;
            stack.AisleWidth_D = AisleWidth_D;
            stack.AisleWidth_L = AisleWidth_L;
            stack.AisleWidth_R = AisleWidth_R;
            stack.KeepDistanceU = SetKeepU;
            stack.KeepDistanceD = SetKeepD;
            stack.KeepDistanceL = SetKeepL;
            stack.KeepDistanceR = SetKeepR;

            HouseStack.setStack(stack.No, stack);

            HouseStack.Fit(); HouseTrack.Fit();
            if (Form_Start.config.SelectedMap != -1)
            { HouseStack.Save(Form_Start.config.Map[Form_Start.config.SelectedMap].Full); }
        }
        private void Form_Stack_Load(object sender, EventArgs e)
        {
            this.label11.Text = "库房尺寸：" + HouseMap.HouseLength.ToString() + " / " + HouseMap.HouseWidth.ToString();

            this.listbox1.Items.Clear();
            this.listbox1.Items.Add("默认");
            this.listbox1.Items.Add("门");
            for (int i = 1; i <= HouseStack.TotalStacks; i++) { this.listbox1.Items.Add(i.ToString()); }
            this.listbox1.SelectedIndex = StackNo + 1;

            Updata(StackNo);
        }
        private void listbox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            Updata(this.listbox1.SelectedIndex - 1);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Updata(StackNo);
        }

        private void Updata(int No)
        {
            StackNo = No;

            HouseStack.STACK stack = HouseStack.getStack(StackNo);
            
            StackLength = stack.Length;
            StackWidth = stack.Width;
            AisleWidth_U = stack.AisleWidth_U;
            AisleWidth_D = stack.AisleWidth_D;
            AisleWidth_L = stack.AisleWidth_L;
            AisleWidth_R = stack.AisleWidth_R;
            SetKeepU = stack.KeepDistanceU;
            SetKeepD = stack.KeepDistanceD;
            SetKeepL = stack.KeepDistanceL;
            SetKeepR = stack.KeepDistanceR;
            MapZoomRate = HouseMap.PixLength;

            if (StackNo == -1)
            {
                AisleWidth_L = HouseStack.DefaultAisleWidthLR;
                AisleWidth_R = HouseStack.DefaultAisleWidthLR;
                AisleWidth_U = HouseStack.DefaultAisleWidthUD;
                AisleWidth_D = HouseStack.DefaultAisleWidthUD;

                StackLength = HouseStack.DefaultStackLength;
                StackWidth = HouseStack.DefaultStackWidth;
            }

            this.textBox1.Text = MapZoomRate.ToString();

            this.StackNoLabel.Text = StackNo.ToString();
            if (StackNo == -1) { this.StackNoLabel.Text = "D"; }
            if (StackNo == 0) { this.StackNoLabel.Text = "门"; }

            this.iLength.Text = StackLength.ToString();
            this.iWidth.Text = StackWidth.ToString();

            this.iAisleWidthU.Text = AisleWidth_U.ToString();
            this.iAisleWidthD.Text = AisleWidth_D.ToString();
            this.iAisleWidthL.Text = AisleWidth_L.ToString();
            this.iAisleWidthR.Text = AisleWidth_R.ToString();

            this.iSetKeepU.Text = SetKeepU.ToString();
            this.iSetKeepD.Text = SetKeepD.ToString();
            this.iSetKeepL.Text = SetKeepL.ToString();
            this.iSetKeepR.Text = SetKeepR.ToString();
        }
    }
}
