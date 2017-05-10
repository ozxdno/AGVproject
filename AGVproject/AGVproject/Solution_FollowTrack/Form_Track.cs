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

namespace AGVproject.Solution_FollowTrack
{
    public partial class Form_Track : Form
    {
        public Form_Track(int TrackNo)
        {
            InitializeComponent(); Solution_FollowTrack.Form_Track.TrackNo = TrackNo;
        }

        private static int TrackNo;
        private static double X;
        private static double Y;
        private static double A;
        private static int StackNo;
        private static int Direction;
        private static double Distance;

        private static double xK, xL, xA, xB, xC, xD, x1, x2;
        private static double yK, yL, yA, yB, yC, yD, y1, y2;

        private void Form_Track_Load(object sender, EventArgs e)
        {
            Updata(TrackNo, true);
        }
        private void Form_Track_FormClosed(object sender, FormClosedEventArgs e)
        {
            TrackNo = this.comboBox1.SelectedIndex;
            if (TrackNo == -1) { return; }

            try { X = double.Parse(this.textBox1.Text); } catch { }
            try { Y = double.Parse(this.textBox2.Text); } catch { }
            try { A = double.Parse(this.textBox3.Text); } catch { }

            StackNo = this.comboBox3.SelectedIndex;
            Direction = this.comboBox2.SelectedIndex;
            try { Distance = double.Parse(this.textBox5.Text); } catch { }

            try { xK = double.Parse(this.textBox6.Text); } catch { }
            try { xL = double.Parse(this.textBox7.Text); } catch { }
            try { xA = double.Parse(this.textBox8.Text); } catch { }
            try { xB = double.Parse(this.textBox9.Text); } catch { }
            try { xC = double.Parse(this.textBox10.Text); } catch { }
            try { xD = double.Parse(this.textBox11.Text); } catch { }
            try { x1 = double.Parse(this.textBox12.Text); } catch { }
            try { x2 = double.Parse(this.textBox13.Text); } catch { }

            try { yK = double.Parse(this.textBox21.Text); } catch { }
            try { yL = double.Parse(this.textBox20.Text); } catch { }
            try { yA = double.Parse(this.textBox19.Text); } catch { }
            try { yB = double.Parse(this.textBox18.Text); } catch { }
            try { yC = double.Parse(this.textBox17.Text); } catch { }
            try { yD = double.Parse(this.textBox16.Text); } catch { }
            try { y1 = double.Parse(this.textBox15.Text); } catch { }
            try { y2 = double.Parse(this.textBox14.Text); } catch { }

            CorrectPosition.CORRECT correct = new CorrectPosition.CORRECT();
            correct.xInvalid = this.textBox6.ReadOnly;
            correct.yInvalid = this.textBox21.ReadOnly;
            correct.xK = xK;
            correct.xL = xL;
            correct.xA = xA;
            correct.xB = xB;
            correct.xC = xC;
            correct.xD = xD;
            correct.x1 = x1;
            correct.x2 = x2;
            correct.yK = yK;
            correct.yL = yL;
            correct.yA = yA;
            correct.yB = yB;
            correct.yC = yC;
            correct.yD = yD;
            correct.y1 = y1;
            correct.y2 = y2;

            HouseTrack.TRACK track = new HouseTrack.TRACK();
            track.TargetPosition = CoordinatePoint.Create_XY(X, Y);
            track.TargetPosition.aCar = A;
            track.TargetPosition.rCar = A * Math.PI / 180;
            track.IsLeft = StackNo > HouseStack.TotalStacksR;
            track.No = StackNo;
            track.Direction = (TH_AutoSearchTrack.Direction)Direction;
            track.Distance = Distance;
            track.Extra = correct;
            HouseTrack.setTrack(TrackNo, track);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int index = this.comboBox1.SelectedIndex;
            if (index == -1) { return; }
            Updata(index);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Updata(TrackNo);
        }

        private void Updata(int TrackNo, bool resetIndex = false)
        {
            Solution_FollowTrack.Form_Track.TrackNo = TrackNo;

            HouseTrack.TRACK track = HouseTrack.getTrack(TrackNo);
            X = track.TargetPosition.x;
            Y = track.TargetPosition.y;
            A = track.TargetPosition.aCar;
            StackNo = track.No;
            Direction = (int)track.Direction;
            Distance = track.Distance;

            CorrectPosition.CORRECT correct = track.Extra == null ? new CorrectPosition.CORRECT() :
                (CorrectPosition.CORRECT)track.Extra;

            xK = correct.xK;
            xL = correct.xL;
            xA = correct.xA;
            xB = correct.xB;
            xC = correct.xC;
            xD = correct.xD;
            x1 = correct.x1;
            x2 = correct.x2;

            yK = correct.yK;
            yL = correct.yL;
            yA = correct.yA;
            yB = correct.yB;
            yC = correct.yC;
            yD = correct.yD;
            y1 = correct.y1;
            y2 = correct.y2;
            
            int N = HouseStack.TotalStacks; this.comboBox3.Items.Clear();
            for (int i = 0; i <= N; i++) { this.comboBox3.Items.Add(i.ToString()); }
            
            this.textBox1.Text = X.ToString();
            this.textBox2.Text = Y.ToString();
            this.textBox3.Text = A.ToString();
            this.comboBox3.SelectedIndex = StackNo;
            this.comboBox2.SelectedIndex = Direction;
            this.textBox5.Text = Distance.ToString();

            this.textBox6.Text = xK.ToString();
            this.textBox7.Text = xL.ToString();
            this.textBox8.Text = xA.ToString();
            this.textBox9.Text = xB.ToString();
            this.textBox10.Text = xC.ToString();
            this.textBox11.Text = xD.ToString();
            this.textBox12.Text = x1.ToString();
            this.textBox13.Text = x2.ToString();

            this.textBox21.Text = yK.ToString();
            this.textBox20.Text = yL.ToString();
            this.textBox19.Text = yA.ToString();
            this.textBox18.Text = yB.ToString();
            this.textBox17.Text = yC.ToString();
            this.textBox16.Text = yD.ToString();
            this.textBox15.Text = y1.ToString();
            this.textBox14.Text = y2.ToString();

            this.textBox6.ReadOnly = correct.xInvalid;
            this.textBox7.ReadOnly = correct.xInvalid;
            this.textBox8.ReadOnly = correct.xInvalid;
            this.textBox9.ReadOnly = correct.xInvalid;
            this.textBox10.ReadOnly = correct.xInvalid;
            this.textBox11.ReadOnly = correct.xInvalid;
            this.textBox12.ReadOnly = correct.xInvalid;
            this.textBox13.ReadOnly = correct.xInvalid;

            this.textBox21.ReadOnly = correct.yInvalid;
            this.textBox20.ReadOnly = correct.yInvalid;
            this.textBox19.ReadOnly = correct.yInvalid;
            this.textBox18.ReadOnly = correct.yInvalid;
            this.textBox17.ReadOnly = correct.yInvalid;
            this.textBox16.ReadOnly = correct.yInvalid;
            this.textBox15.ReadOnly = correct.yInvalid;
            this.textBox14.ReadOnly = correct.yInvalid;

            if (resetIndex)
            {
                N = HouseTrack.TotalTrack; this.comboBox1.Items.Clear();
                for (int i = 0; i < N; i++) { this.comboBox1.Items.Add(i.ToString()); }
                this.comboBox1.SelectedIndex = TrackNo >= HouseTrack.TotalTrack ? -1 : TrackNo;
            }
        }
    }
}
