using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AGVproject.Class
{
    /// <summary>
    /// 备用线程，界面刷新
    /// </summary>
    class TH_UpdataFormStart
    {
        /////////////////////////////////////////////////// Attribute ///////////////////////////////////////////

        public static Bitmap BaseMapPicture;
        public static Graphics g;
        public static Cursor Cursor;
        public static bool CurrsorInMap;

        public static MOUSE MousePosition;
        public static Font StrFont;

        public static int MapWidth { get { return (int)(HouseMap.HouseLength / Form_Start.config.PixLength); } }
        public static int MapLength { get { return (int)(HouseMap.HouseWidth / Form_Start.config.PixLength); } }

        public static List<STACK> Stacks;
        public static OPERATE Operate;
        public static List<ROUTE> Route;
        
        public enum OPERATE { Wait, Clear, Undo, Finish, No }
        public struct STACK
        {
            /// <summary>
            /// 是否是左边堆垛
            /// </summary>
            public bool IsLeft;
            /// <summary>
            /// 堆垛编号
            /// </summary>
            public int No;

            /// <summary>
            /// 堆垛左上角在图上的 X 轴坐标
            /// </summary>
            public int xBG;
            /// <summary>
            /// 堆垛左上角在图上的 Y 轴坐标
            /// </summary>
            public int yBG;

            /// <summary>
            /// 堆垛在图上的长度
            /// </summary>
            public int Length;
            /// <summary>
            /// 堆垛在图上的宽度
            /// </summary>
            public int Width;

            /// <summary>
            /// 堆垛上方的通道宽度
            /// </summary>
            public int AisleWidth_U;
            /// <summary>
            /// 堆垛下方的通道宽度
            /// </summary>
            public int AisleWidth_D;
            /// <summary>
            /// 堆垛左方的通道宽度
            /// </summary>
            public int AisleWidth_L;
            /// <summary>
            /// 堆垛右方的通道宽度
            /// </summary>
            public int AisleWidth_R;

            /// <summary>
            /// 小车所处方向（相对所在堆垛来说）
            /// </summary>
            public TH_AutoSearchTrack.Direction Direction;
            /// <summary>
            /// 小车与所在堆垛边（上、下、左、右）左端的距离
            /// </summary>
            public int Distance;

            /// <summary>
            /// 设定与堆垛上边维持距离
            /// </summary>
            public int SetKeepU;
            /// <summary>
            /// 设定与堆垛下边维持距离
            /// </summary>
            public int SetKeepD;
            /// <summary>
            /// 设定与堆垛左边维持距离
            /// </summary>
            public int SetKeepL;
            /// <summary>
            /// 设定与堆垛右边维持距离
            /// </summary>
            public int SetKeepR;
        }
        public struct ROUTE
        {
            public bool IsLeft;
            public int No;
            public TH_AutoSearchTrack.Direction Direction;
            public int Distance;

            public Point MapPoint;
        }
        public struct MOUSE
        {
            public bool IsSetting;
            public bool IsGetting;

            public bool IsLeft;
            public int No;
            public TH_AutoSearchTrack.Direction Direction;
            public int Distance;

            public int x;
            public int y;
        }

        ////////////////////////////////////////////////////////// Method //////////////////////////////////////////

        public static void Initial()
        {
            Stacks = new List<STACK>();
            for (int i = 0; i <= HouseMap.TotalStacks; i++)
            { Stacks.Add(RealStack2MapStack(i)); }
        }

        public static void Updata()
        {
            getFont();
            getBaseMapPicture();
        }

        

        public static void getBaseMapPicture()
        {
            // 创建图片
            if (BaseMapPicture != null) { g.Dispose(); BaseMapPicture.Dispose(); }
            BaseMapPicture = new Bitmap(MapWidth, MapLength);

            // 创建画笔，纯色填充
            g = Graphics.FromImage(BaseMapPicture);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, MapWidth, MapLength));

            // 是否打开地图
            if (!Form_Start.config.CheckMap) { return; }

            // 获取字体
            int picLength = Math.Min(MapLength, MapWidth) / 2;
            int width = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * picLength);
            Font font = new Font("Arial", width / 6 + 1);

            // 添加堆垛
            string istr; SizeF size;
            for (int i = 1; i < Stacks.Count; i++)
            {
                g.FillRectangle(Brushes.LightBlue, Stacks[i].xBG, Stacks[i].yBG, Stacks[i].Length, Stacks[i].Width);

                istr = i.ToString(); size = g.MeasureString(istr, font);

                int x = Stacks[i].xBG + Stacks[i].Length / 2 - (int)size.Width / 2;
                int y = Stacks[i].yBG + Stacks[i].Width / 2 - (int)size.Height / 2;
                g.DrawString(i.ToString(), font, Brushes.Black, x, y);
            }
        }
        public static void getCursorShape()
        {
            MOUSE mouse = getMousePosition();

            if (!Form_Start.config.CheckMap && !Form_Start.config.CheckRoute) { Cursor = Cursors.Default; return; }
            if (mouse.No == 0) { Cursor = Cursors.Default; return; }
            if (mouse.Direction == TH_AutoSearchTrack.Direction.Tuning) { Cursor = Cursors.Default; return; }
            if (Operate == OPERATE.Finish) { Cursor = Cursors.Default; return; }

            Cursor = Cursors.Cross;
        }
        public static void getFont()
        {
            int picLength = Math.Min(MapLength, MapWidth) / 2;
            int width = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * picLength);
            StrFont = new Font("Arial", width / 6 + 1);
        }

        public static void getUltraSonicData()
        {
            if (!Form_Start.config.CheckControlPort) { return; }

            int[] SonicData = TH_SendCommand.getUltraSonicData();
            if (SonicData == null) { SonicData = new int[8]; }

            int picLength = Math.Min(MapLength, MapWidth) / 2;
            int width = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * picLength);
            Font font = new Font("Arial", width / 8 + 1);

            int L = (int)(picLength + Hardware_PlatForm.AxisSideL / Form_Start.config.urgRange * picLength);
            int U = (int)(picLength - Hardware_PlatForm.AxisSideU / Form_Start.config.urgRange * picLength);
            int R = (int)(picLength + Hardware_PlatForm.AxisSideR / Form_Start.config.urgRange * picLength);
            int D = (int)(picLength - Hardware_PlatForm.AxisSideD / Form_Start.config.urgRange * picLength);

            string str = SonicData[(int)TH_SendCommand.Sonic.Head_L_X].ToString();
            SizeF strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, L - strSize.Width, U);


            str = SonicData[(int)TH_SendCommand.Sonic.Head_L_Y].ToString();
            strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, L, U - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Head_R_X].ToString();
            //strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, R, U);

            str = SonicData[(int)TH_SendCommand.Sonic.Head_R_Y].ToString();
            strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, R - strSize.Width, U - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_L_X].ToString();
            strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, L - strSize.Width, D - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_L_Y].ToString();
            //strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, L, D);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_R_X].ToString();
            strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, R, D - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_R_Y].ToString();
            strSize = g.MeasureString(str, font);
            g.DrawString(str, font, Brushes.Black, R - strSize.Width, D);
        }
        public static void getUrgData()
        {
            if (!Form_Start.config.CheckUrgPort) { return; }

            // 画布一半宽度
            int picLength = Math.Min(MapLength, MapWidth) / 2;

            // 画出车的位置
            int xCar = (int)(picLength + Hardware_PlatForm.AxisSideL / Form_Start.config.urgRange * picLength);
            int yCar = (int)(picLength - Hardware_PlatForm.AxisSideU / Form_Start.config.urgRange * picLength);
            int CarL = (int)(Hardware_PlatForm.Length / Form_Start.config.urgRange * picLength);
            int CarW = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * picLength);

            g.FillRectangle(Brushes.LightGray, xCar, yCar, CarW, CarL);
            g.FillEllipse(Brushes.Red, picLength - 4, picLength - 4, 8, 8); // 原点

            // 周围环境信息
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingD(0, Form_Start.config.urgRange);
            for (int i = 0; i < points.Count; i++)
            {
                int x = (int)(picLength * points[i].x / Form_Start.config.urgRange + picLength);
                int y = (int)(picLength - picLength * points[i].y / Form_Start.config.urgRange);

                g.FillEllipse(Brushes.Black, x, y, 2, 2);
            }
        }
        public static void getLocateData()
        {
            if (!Form_Start.config.CheckLocatePort) { return; }

            CoordinatePoint.POINT point = TH_MeasurePosition.getPosition();
            int picLength = Math.Min(MapLength, MapWidth) / 2;
            int width = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * picLength);
            Font font = new Font("Arial", width / 6 + 1);

            SizeF size = g.MeasureString("X", font);
            int centre = MapLength / 2;

            g.DrawString("X: " + point.x.ToString(), font, Brushes.Black, 30, centre - size.Width * 3);
            g.DrawString("Y: " + point.y.ToString(), font, Brushes.Black, 30, centre - size.Width * 1);
            g.DrawString("A: " + point.aCar.ToString(), font, Brushes.Black, 30, centre + size.Width * 1);
            g.DrawString("R: " + point.rCar.ToString(), font, Brushes.Black, 30, centre + size.Width * 3);
        }

        public static MOUSE setMousePosition(int X, int Y)
        {
            int xBG, yBG, xED, yED;
            MOUSE p = new MOUSE();

            foreach (STACK stack in Stacks)
            {
                // 左
                xBG = stack.xBG - stack.SetKeepL;
                yBG = stack.yBG - stack.SetKeepU;
                xED = stack.xBG;
                yED = stack.yBG + stack.Width;

                if (xBG <= X && X < xED && yBG <= Y && Y <= yED)
                {
                    p.IsLeft = stack.IsLeft;
                    p.No = stack.No;
                    p.Direction = TH_AutoSearchTrack.Direction.Left;
                    p.Distance = yED - Y; break;
                }

                // 上
                xBG = stack.xBG;
                yBG = stack.yBG - stack.SetKeepU;
                xED = stack.xBG + stack.Length + stack.SetKeepR;
                yED = stack.yBG;

                if (xBG <= X && X <= xED && yBG <= Y && Y < yED)
                {
                    p.IsLeft = stack.IsLeft;
                    p.No = stack.No;
                    p.Direction = TH_AutoSearchTrack.Direction.Up;
                    p.Distance = X - xBG; break;
                }

                // 右
                xBG = stack.xBG + stack.Length;
                yBG = stack.yBG;
                xED = xBG + stack.SetKeepR;
                yED = yBG + stack.Width + stack.SetKeepD;

                if (xBG < X && X <= xED && yBG <= Y && Y <= yED)
                {
                    p.IsLeft = stack.IsLeft;
                    p.No = stack.No;
                    p.Direction = TH_AutoSearchTrack.Direction.Right;
                    p.Distance = Y - yBG; break;
                }

                // 下
                xBG = stack.xBG - stack.SetKeepL;
                yBG = stack.yBG + stack.Width;
                xED = stack.xBG + stack.Length;
                yED = yBG + stack.SetKeepD;

                if (xBG <= X && X <= xED && yBG < Y && Y <= yED)
                {
                    p.IsLeft = stack.IsLeft;
                    p.No = stack.No;
                    p.Direction = TH_AutoSearchTrack.Direction.Down;
                    p.Distance = xED - X; break;
                }

                // 中
                xBG = stack.xBG;
                yBG = stack.yBG;
                xED = xBG + stack.Length;
                yED = yBG + stack.Width;

                if (xBG <= X && X <= xED && yBG <= Y && Y <= yED)
                {
                    p.IsLeft = stack.IsLeft;
                    p.No = stack.No;
                    p.Direction = TH_AutoSearchTrack.Direction.Tuning;
                    p.Distance = 0; break;
                }
            }

            p.x = X; p.y = Y; return p;
        }
        public static MOUSE getMousePosition()
        {
            MOUSE currPos = new MOUSE();

            MousePosition.IsGetting = true;
            while (MousePosition.IsSetting) ;

            currPos.IsLeft = MousePosition.IsLeft;
            currPos.No = MousePosition.No;
            currPos.Direction = MousePosition.Direction;
            currPos.Distance = MousePosition.Distance;
            currPos.x = MousePosition.x;
            currPos.y = MousePosition.y;

            MousePosition.IsGetting = false;
            return currPos;
        }

        public static void MapStack2ReadStack(int No)
        {

        }
        public static STACK RealStack2MapStack(int No)
        {
            if (No < 1 || No > HouseMap.TotalStacks) { return new STACK(); }
            STACK stack = new STACK();

            stack.IsLeft = HouseMap.Stacks[No].IsLeft;
            stack.No = No;

            stack.Length = (int)(HouseMap.Stacks[No].Length / Form_Start.config.PixLength);
            stack.Width = (int)(HouseMap.Stacks[No].Width / Form_Start.config.PixLength);
            stack.AisleWidth_U = (int)(HouseMap.Stacks[No].AisleWidth_U / Form_Start.config.PixLength);
            stack.AisleWidth_D = (int)(HouseMap.Stacks[No].AisleWidth_D / Form_Start.config.PixLength);
            stack.AisleWidth_L = (int)(HouseMap.Stacks[No].AisleWidth_L / Form_Start.config.PixLength);
            stack.AisleWidth_R = (int)(HouseMap.Stacks[No].AisleWidth_R / Form_Start.config.PixLength);

            stack.Direction = TH_AutoSearchTrack.Direction.Tuning;
            stack.SetKeepU = (int)(HouseMap.Stacks[No].KeepDistanceU / Form_Start.config.PixLength);
            stack.SetKeepD = (int)(HouseMap.Stacks[No].KeepDistanceD / Form_Start.config.PixLength);
            stack.SetKeepL = (int)(HouseMap.Stacks[No].KeepDistanceL / Form_Start.config.PixLength);
            stack.SetKeepR = (int)(HouseMap.Stacks[No].KeepDistanceR / Form_Start.config.PixLength);

            //stack.SetKeepU -= 1;
            //stack.SetKeepD -= 1;
            //stack.SetKeepL -= 4;
            //stack.SetKeepR -= 4;

            double xBG = 0, yBG = 0;
            if (HouseMap.Stacks[No].IsLeft)
            {
                for (int i = HouseMap.TotalStacks; i > No; i--)
                { yBG += HouseMap.Stacks[i].AisleWidth_U + HouseMap.Stacks[i].Width; }
                xBG = HouseMap.Stacks[No].AisleWidth_L;
                yBG += HouseMap.Stacks[No].AisleWidth_U;
            }
            else
            {
                for (int i = 1; i < No; i++)
                { yBG += HouseMap.Stacks[i].AisleWidth_U + HouseMap.Stacks[i].Width; }
                xBG = HouseMap.HouseWidth - HouseMap.Stacks[No].Length - HouseMap.Stacks[No].AisleWidth_R;
                yBG += HouseMap.Stacks[No].AisleWidth_U;
            }

            stack.xBG = (int)(xBG / Form_Start.config.PixLength);
            stack.yBG = (int)(yBG / Form_Start.config.PixLength); return stack;
        }
    }
}
