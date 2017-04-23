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
    /// 界面刷新
    /// </summary>
    class TH_UpdataPictureBox
    {
        /////////////////////////////////////////////////// Attribute ///////////////////////////////////////////

        public static Bitmap BaseMapPicture;
        public static Graphics g;
        public static Cursor Cursor;

        public static bool CurrsorInMap;
        public static bool NoOperate;
        public static bool DrawOver;
        public static bool PushedCursor;

        public static MOUSE MousePosition;
        public static Font StrFont;

        public static int MapWidth { get { return (int)(HouseMap.HouseLength / Form_Start.config.PixLength); } }
        public static int MapLength { get { return (int)(HouseMap.HouseWidth / Form_Start.config.PixLength); } }
        public static int BoxLength;
        public static int BoxWidth;

        public static List<STACK> Stacks;
        public static List<ROUTE> Route;

        public static bool IsSetting;
        public static bool IsGetting;
        public static bool IsSettingMousePosition;
        public static bool IsGettingMousePosition;
        
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
            IsSetting = false;
            IsGetting = false;
            NoOperate = false;

            Stacks = new List<STACK>();
            for (int i = 0; i <= HouseMap.TotalStacks; i++)
            { Stacks.Add(RealStack2MapStack(i)); }

            Route = new List<ROUTE>();
        }
        public static void Updata()
        {
            while (IsSetting) ;
            IsGetting = true;

            getCursorShape();
            getFont();
            getBaseMapPicture();
            getPermitRoute();
            getCurrentRoute();
            getSelectRoute();
            getUrgData();
            getUltraSonicData();
            getLocateData();

            IsGetting = false;
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

            // 添加堆垛
            if (Stacks.Count == 0) { return; }
            string istr; SizeF size;
            g.FillRectangle(Brushes.LightBlue, Stacks[0].xBG, Stacks[0].yBG, Stacks[0].Length, Stacks[0].Width);
            for (int i = 1; i < Stacks.Count; i++)
            {
                g.FillRectangle(Brushes.LightBlue, Stacks[i].xBG, Stacks[i].yBG, Stacks[i].Length, Stacks[i].Width);

                istr = i.ToString(); size = g.MeasureString(istr, StrFont);

                int x = Stacks[i].xBG + Stacks[i].Length / 2 - (int)size.Width / 2;
                int y = Stacks[i].yBG + Stacks[i].Width / 2 - (int)size.Height / 2;
                g.DrawString(i.ToString(), StrFont, Brushes.Black, x, y);
            }
        }
        public static void getCursorShape()
        {
            if (PushedCursor) { PushedCursor = false; return; }

            MOUSE mouse = getMousePosition();

            if (!Form_Start.config.CheckMap && !Form_Start.config.CheckRoute) { Cursor = Cursors.Default; return; }
            if (mouse.No == -1) { Cursor = Cursors.Default; return; }
            if (mouse.No == 0) { Cursor = Cursors.Cross; return; }
            if (mouse.Direction == TH_AutoSearchTrack.Direction.Tuning) { Cursor = Cursors.Default; return; }

            Cursor = Cursors.Cross;
        }
        public static void getFont()
        {
            int fontWidth = Math.Min(Math.Min(MapLength, MapWidth), Math.Min(BoxLength, BoxWidth));
            fontWidth /= 50;
            fontWidth++;
            if (fontWidth > 40) { fontWidth = 40; }
            StrFont = new Font("Arial", fontWidth);
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
            SizeF size = g.MeasureString("X", StrFont);
            int centre = MapLength / 2;

            g.DrawString("X: " + point.x.ToString(), StrFont, Brushes.Black, 30, centre - size.Width * 3);
            g.DrawString("Y: " + point.y.ToString(), StrFont, Brushes.Black, 30, centre - size.Width * 1);
            g.DrawString("A: " + point.aCar.ToString(), StrFont, Brushes.Black, 30, centre + size.Width * 1);
            g.DrawString("R: " + point.rCar.ToString(), StrFont, Brushes.Black, 30, centre + size.Width * 3);
        }
        public static void getPermitRoute()
        {
            if (Stacks == null || Stacks.Count == 0) { return; }
            if (!Form_Start.config.CheckRoute) { return; }

            int xBG, xED, yBG, yED;

            foreach (STACK stack in Stacks)
            {
                // 左
                xBG = stack.xBG - stack.SetKeepL;
                xED = xBG;
                yBG = stack.yBG - stack.SetKeepU;
                yED = stack.yBG + stack.Width + stack.SetKeepD;
                g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);

                // 上
                xBG = stack.xBG - stack.SetKeepL;
                xED = stack.xBG + stack.Length + stack.SetKeepR;
                yBG = stack.yBG - stack.SetKeepU;
                yED = yBG;
                g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);

                // 右
                xBG = stack.xBG + stack.Length + stack.SetKeepR;
                xED = xBG;
                yBG = stack.yBG - stack.SetKeepU;
                yED = stack.yBG + stack.Width + stack.SetKeepD;
                g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);

                // 下
                xBG = stack.xBG - stack.SetKeepL;
                xED = stack.xBG + stack.Length + stack.SetKeepR;
                yBG = stack.yBG + stack.Width + stack.SetKeepD;
                yED = yBG;
                g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);
            }
        }
        public static void getCurrentRoute()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (Route == null || Route.Count == 0) { return; }

            // 线
            for (int i = 0; i < Route.Count - 1; i++)
            {
                g.DrawLine(Pens.Blue, Route[i].MapPoint, Route[i + 1].MapPoint);
            }

            // 点
            for (int i = 1; i < Route.Count - 1; i++)
            {
                g.DrawEllipse(Pens.Blue, Route[i].MapPoint.X - 2, Route[i].MapPoint.Y - 2, 4, 4);
            }
            
            // 起点
            g.DrawEllipse(Pens.Red, Route[0].MapPoint.X - 2, Route[0].MapPoint.Y - 2, 4, 4);
            g.DrawString("S", StrFont, Brushes.Red, Route[0].MapPoint);

            // 终点
            if (Route.Count <= 2) { return; }
            g.DrawEllipse(Pens.Red, Route[Route.Count - 1].MapPoint.X - 2, Route[Route.Count - 1].MapPoint.Y - 2, 4, 4);
            g.DrawString("E", StrFont, Brushes.Red, Route[Route.Count - 1].MapPoint);
        }
        public static void getSelectRoute()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (!CurrsorInMap) { return; }
            if (NoOperate) { return; }
            if (DrawOver) { return; }
            if (Route.Count == 0) { return; }

            MOUSE mousePos = getMousePosition();
            Point ptBG = Route[Route.Count - 1].MapPoint;
            Point ptED = new Point(mousePos.x, mousePos.y);
            g.DrawLine(Pens.Blue, ptBG, ptED);
        }

        public static void setMousePosition(int X, int Y)
        {
            int xBG, yBG, xED, yED;
            MOUSE p = new MOUSE();
            p.No = -1;

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
                    p.Distance = xED - X; break;
                }
            }

            p.x = X; p.y = Y;

            IsSettingMousePosition = true;
            while (IsGettingMousePosition) ;
            MousePosition = p;
            IsSettingMousePosition = false;
        }
        public static MOUSE getMousePosition()
        {
            MOUSE currPos = new MOUSE();
            
            while (IsSettingMousePosition) ;
            IsGettingMousePosition = true;

            currPos.IsLeft = MousePosition.IsLeft;
            currPos.No = MousePosition.No;
            currPos.Direction = MousePosition.Direction;
            currPos.Distance = MousePosition.Distance;
            currPos.x = MousePosition.x;
            currPos.y = MousePosition.y;

            IsGettingMousePosition = false;
            return currPos;
        }
        public static MOUSE getMousePosition(int X, int Y)
        {
            int xBG, yBG, xED, yED;
            MOUSE p = new MOUSE();
            p.No = -1;

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
                    p.Distance = xED - X; break;
                }
            }

            p.x = X; p.y = Y; return p;
        }

        public static HouseMap.STACK MapStack2ReadStack(int No)
        {
            return new HouseMap.STACK();
        }
        public static STACK RealStack2MapStack(int No)
        {
            if (No < 0 || No > HouseMap.TotalStacks) { return new STACK(); }
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

            if (No != 0)
            {
                stack.SetKeepU -= 1;
                stack.SetKeepD -= 1;
                stack.SetKeepL -= 4;
                stack.SetKeepR -= 4;
            }
            

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
        public static Point getRouteMapPoint(ROUTE route)
        {
            STACK stack = Stacks[route.No];
            Point pt = new Point();

            if (route.Direction == TH_AutoSearchTrack.Direction.Left)
            {
                pt.X = stack.xBG - stack.SetKeepL;
                pt.Y = stack.yBG + stack.Width - route.Distance;
            }
            if (route.Direction == TH_AutoSearchTrack.Direction.Up)
            {
                pt.X = stack.xBG + route.Distance;
                pt.Y = stack.yBG - stack.SetKeepU;
            }
            if (route.Direction == TH_AutoSearchTrack.Direction.Right)
            {
                pt.X = stack.xBG + stack.Length + stack.SetKeepR;
                pt.Y = stack.yBG + route.Distance;
            }
            if (route.Direction == TH_AutoSearchTrack.Direction.Down)
            {
                pt.X = stack.xBG + stack.Length - route.Distance;
                pt.Y = stack.yBG + stack.Width + stack.SetKeepD;
            }
            if (route.Direction == TH_AutoSearchTrack.Direction.Tuning)
            {
                pt.X = stack.xBG + stack.Length - route.Distance;
                pt.Y = stack.yBG + stack.Width + stack.SetKeepD;
            }

            return pt;
        }

        public static void MouseLeftClicked()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (!CurrsorInMap) { return; }
            if (NoOperate) { return; }
            if (DrawOver) { return; }

            MOUSE mousePos = getMousePosition();

            if (mousePos.No == -1) { PushedCursor = true; Cursor = Cursors.No; return; }
            if (mousePos.No != 0 && mousePos.Direction == TH_AutoSearchTrack.Direction.Tuning)
            { PushedCursor = true; Cursor = Cursors.No; return; }
            if (mousePos.IsLeft && mousePos.Direction == TH_AutoSearchTrack.Direction.Left)
            { PushedCursor = true; Cursor = Cursors.No; return; }
            if (!mousePos.IsLeft && mousePos.Direction == TH_AutoSearchTrack.Direction.Right)
            { PushedCursor = true; Cursor = Cursors.No; return; }

            // 当前关键点
            ROUTE next = new ROUTE();
            next.IsLeft = mousePos.IsLeft;
            next.No = mousePos.No;
            next.Direction = mousePos.Direction;
            next.Distance = mousePos.Distance;
            next.MapPoint = new Point(mousePos.x, mousePos.y);
            next.MapPoint = getRouteMapPoint(next);
            if (Route.Count == 0) { Route.Add(next); return; }

            // 自动添加关键点
            ROUTE last = Route[Route.Count - 1];
            STACK lastStack = Stacks[last.No];
            STACK nextStack = Stacks[next.No];

            // 原点
            if (last.No == 0 && next.No == 0) { Route.Add(next); return; }

            #region 同一通道内

            bool InSameAisleRow =
                (last.IsLeft && next.IsLeft && last.No - next.No == 1 && last.Direction == TH_AutoSearchTrack.Direction.Down && next.Direction == TH_AutoSearchTrack.Direction.Up) ||
                (last.IsLeft && next.IsLeft && next.No - last.No == 1 && last.Direction == TH_AutoSearchTrack.Direction.Up && next.Direction == TH_AutoSearchTrack.Direction.Down) ||
                (!last.IsLeft && !next.IsLeft && last.No - next.No == 1 && last.Direction == TH_AutoSearchTrack.Direction.Up && next.Direction == TH_AutoSearchTrack.Direction.Down) ||
                (!last.IsLeft && !next.IsLeft && next.No - last.No == 1 && last.Direction == TH_AutoSearchTrack.Direction.Down && next.Direction == TH_AutoSearchTrack.Direction.Up) ||
                (last.No == next.No && last.Direction == next.Direction && last.Direction == TH_AutoSearchTrack.Direction.Up) ||
                (last.No == next.No && last.Direction == next.Direction && last.Direction == TH_AutoSearchTrack.Direction.Down);

            if (InSameAisleRow)
            {
                // 先跨越
                bool jump = last.MapPoint.Y != next.MapPoint.Y;
                int gapX = next.MapPoint.X - last.MapPoint.X;

                if (jump)
                {
                    ROUTE jumpRoute = new ROUTE(); jumpRoute.IsLeft = next.IsLeft; jumpRoute.No = next.No; jumpRoute.Direction = next.Direction;
                    jumpRoute.Distance = next.Direction == TH_AutoSearchTrack.Direction.Up ? next.Distance - gapX : next.Distance + gapX;
                    jumpRoute.MapPoint = getRouteMapPoint(jumpRoute);
                    Route.Add(jumpRoute);
                }

                Route.Add(next); return;
            }

            #endregion

            #region 出通道

            bool InCentreRoad = last.Direction == TH_AutoSearchTrack.Direction.Left || last.Direction == TH_AutoSearchTrack.Direction.Right;
            if (!InCentreRoad && last.No != 0)
            {
                ROUTE outAisle = new ROUTE(); outAisle.IsLeft = last.IsLeft; outAisle.No = last.No; outAisle.Direction = last.Direction;
                outAisle.Distance = last.Direction == TH_AutoSearchTrack.Direction.Up ?
                    (last.IsLeft ? lastStack.Length + lastStack.SetKeepR : -lastStack.SetKeepL) :
                    (last.IsLeft ? -lastStack.SetKeepR : lastStack.SetKeepL + lastStack.Length);
                outAisle.MapPoint = getRouteMapPoint(outAisle);

                Route.Add(outAisle);
                last = outAisle;
            }

            #endregion
            
            #region 跳转

            bool jumpX = last.IsLeft != next.IsLeft && last.No != 0 && next.No != 0;

            if (jumpX)
            {
                int X = Stacks[Stacks.Count - 1].xBG + Stacks[Stacks.Count - 1].Length + Stacks[Stacks.Count - 1].SetKeepR;
                int Y = last.MapPoint.Y;

                MOUSE lastpos = getMousePosition(X, Y);
                if (lastpos.No == -1) { return; }

                last.No = lastpos.No;
                last.IsLeft = lastpos.IsLeft;
                last.Direction = lastpos.Direction;
                last.Distance = lastpos.Distance;
                last.MapPoint = new Point(X, Y);

                //last.No = HouseMap.TotalStacks + 1 - last.No;
                lastStack = Stacks[last.No];

                //if (last.IsLeft)
                //{
                //    last.IsLeft = false;
                //    last.Direction = TH_AutoSearchTrack.Direction.Left;
                //    last.Distance = lastStack.yBG + lastStack.Width - last.MapPoint.Y;
                //    last.MapPoint = getRouteMapPoint(last);
                //}
                //else
                //{
                //    last.IsLeft = true;
                //    last.Direction = TH_AutoSearchTrack.Direction.Right;
                //    last.Distance = last.MapPoint.Y - lastStack.yBG;
                //    last.MapPoint = getRouteMapPoint(last);
                //}
                Route.Add(last); return;
            }

            #endregion

            // 中间
            bool jumpY = last.MapPoint.Y != next.MapPoint.Y;

            if (next.No == 0)
            {
                #region 左边

                if (last.IsLeft)
                {
                    if ((last.Direction != TH_AutoSearchTrack.Direction.Right || last.Distance != -lastStack.SetKeepU) &&
                        (last.Direction != TH_AutoSearchTrack.Direction.Up || last.Distance != lastStack.Length + lastStack.SetKeepR))
                    {
                        ROUTE tRoute = new ROUTE(); tRoute.IsLeft = last.IsLeft; tRoute.No = last.No; tRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        tRoute.Distance = -lastStack.SetKeepU;
                        tRoute.MapPoint = getRouteMapPoint(tRoute);
                        Route.Add(tRoute);
                    }

                    for (int i = last.No + 1; i <= HouseMap.TotalStacks; i++)
                    {
                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = true; dRoute.No = i; dRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        dRoute.Distance = Stacks[i].Width + Stacks[i].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);

                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = true; uRoute.No = i; uRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        uRoute.Distance = -Stacks[i].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);
                    }

                    ROUTE oRoute = new ROUTE(); oRoute.IsLeft = false; oRoute.No = 0; oRoute.Direction = TH_AutoSearchTrack.Direction.Tuning;
                    oRoute.Distance = Stacks[0].xBG + Stacks[0].Length - last.MapPoint.X;
                    oRoute.MapPoint = getRouteMapPoint(oRoute);
                    Route.Add(oRoute);
                    Route.Add(next); return;
                }

                #endregion

                #region 右边

                else
                {
                    if ((last.Direction != TH_AutoSearchTrack.Direction.Down || last.Distance != lastStack.Length + lastStack.SetKeepL) &&
                        (last.Direction != TH_AutoSearchTrack.Direction.Left || last.Distance != lastStack.Width + lastStack.SetKeepU))
                    {
                        ROUTE tRoute = new ROUTE(); tRoute.IsLeft = last.IsLeft; tRoute.No = last.No; tRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        tRoute.Distance = lastStack.Width + lastStack.SetKeepU;
                        tRoute.MapPoint = getRouteMapPoint(tRoute);
                        Route.Add(tRoute);
                    }

                    for (int i = last.No - 1; i >= 1; i--)
                    {
                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = false; dRoute.No = i; dRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        dRoute.Distance = -Stacks[i].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);

                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = false; uRoute.No = i; uRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        uRoute.Distance = Stacks[i].Width + Stacks[i].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);
                    }

                    ROUTE oRoute = new ROUTE(); oRoute.IsLeft = false; oRoute.No = 0; oRoute.Direction = TH_AutoSearchTrack.Direction.Tuning;
                    oRoute.Distance = Stacks[0].xBG + Stacks[0].Length - last.MapPoint.X;
                    oRoute.MapPoint = getRouteMapPoint(oRoute);
                    Route.Add(oRoute);
                    Route.Add(next); return;
                }

                #endregion
            }

            if (jumpY)
            {
                #region 从原点出来

                if (last.No == 0)
                {
                    ROUTE oRoute = new ROUTE(); oRoute.IsLeft = false; oRoute.No = 0; oRoute.Direction = TH_AutoSearchTrack.Direction.Tuning;
                    oRoute.Distance = next.IsLeft ?
                        Stacks[0].xBG + Stacks[0].Length + Stacks[0].SetKeepR - (Stacks[Stacks.Count - 1].xBG + Stacks[Stacks.Count - 1].Length + Stacks[Stacks.Count - 1].SetKeepR) :
                        Stacks[0].xBG + Stacks[0].Length + Stacks[0].SetKeepR - Stacks[1].xBG + Stacks[1].SetKeepL;
                    oRoute.MapPoint = getRouteMapPoint(oRoute);
                    Route.Add(oRoute);

                    last.No = next.IsLeft ? HouseMap.TotalStacks : 1;
                    last.Direction = next.IsLeft ? TH_AutoSearchTrack.Direction.Right : TH_AutoSearchTrack.Direction.Left;
                    last.Distance = next.IsLeft ?
                        -Stacks[Stacks.Count - 1].SetKeepU :
                        Stacks[1].Width + Stacks[1].SetKeepU;
                    last.MapPoint = getRouteMapPoint(last);
                    Route.Add(last);
                }

                #endregion

                #region 左边

                if (next.IsLeft)
                {
                    bool Upward = next.No - last.No > 0;
                    bool Downward = next.No - last.No < 0;

                    #region 向上

                    if ((last.Direction != TH_AutoSearchTrack.Direction.Right || last.Distance != -lastStack.SetKeepU) &&
                        (last.Direction != TH_AutoSearchTrack.Direction.Up || last.Distance != lastStack.Length + lastStack.SetKeepR) &&
                        Upward)
                    {
                        ROUTE tRoute = new ROUTE(); tRoute.IsLeft = last.IsLeft; tRoute.No = last.No; tRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        tRoute.Distance = -lastStack.SetKeepU;
                        tRoute.MapPoint = getRouteMapPoint(tRoute);
                        Route.Add(tRoute);
                    }

                    for (int i = last.No + 1; i < next.No; i++)
                    {
                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = true; dRoute.No = i; dRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        dRoute.Distance = Stacks[i].Width + Stacks[i].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);

                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = true; uRoute.No = i; uRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        uRoute.Distance = -Stacks[i].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);
                    }

                    if (Upward)
                    {
                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = true; dRoute.No = next.No; dRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        dRoute.Distance = Stacks[next.No].Width + Stacks[next.No].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);

                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = true; uRoute.No = next.No; uRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        uRoute.Distance = -Stacks[next.No].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        if (next.Direction == TH_AutoSearchTrack.Direction.Up) { Route.Add(uRoute); }
                    }

                    if (Upward && next.MapPoint.X == Route[Route.Count - 1].MapPoint.X)
                    {
                        ROUTE dRoute = Route[Route.Count - 1];
                        if (next.Distance != dRoute.Distance) { Route.Add(next); }
                        return;
                    }

                    #endregion

                    #region 向下

                    if ((last.Direction != TH_AutoSearchTrack.Direction.Right || last.Distance != lastStack.Width + lastStack.SetKeepD) &&
                        (last.Direction != TH_AutoSearchTrack.Direction.Down || last.Distance != -lastStack.SetKeepR) &&
                        Downward)
                    {
                        ROUTE tRoute = new ROUTE(); tRoute.IsLeft = last.IsLeft; tRoute.No = last.No; tRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        tRoute.Distance = lastStack.Width + lastStack.SetKeepD;
                        tRoute.MapPoint = getRouteMapPoint(tRoute);
                        Route.Add(tRoute);
                    }

                    for (int i = last.No - 1; i > next.No; i--)
                    {
                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = true; uRoute.No = i; uRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        uRoute.Distance = -Stacks[i].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);

                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = true; dRoute.No = i; dRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        dRoute.Distance = Stacks[i].Width + Stacks[i].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);
                    }

                    if(Downward)
                    {
                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = true; uRoute.No = next.No; uRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        uRoute.Distance = -Stacks[next.No].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);

                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = true; dRoute.No = next.No; dRoute.Direction = TH_AutoSearchTrack.Direction.Right;
                        dRoute.Distance = Stacks[next.No].Width + Stacks[next.No].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        if (next.Direction == TH_AutoSearchTrack.Direction.Down) { Route.Add(dRoute); }
                    }

                    if (Downward && next.MapPoint.X == Route[Route.Count - 1].MapPoint.X)
                    {
                        ROUTE dRoute = Route[Route.Count - 1];
                        if (next.Distance != dRoute.Distance) { Route.Add(next); }
                        return;
                    }

                    #endregion
                }

                #endregion

                #region 右边

                if (!next.IsLeft)
                {
                    bool Upward = next.No - last.No < 0;
                    bool Downward = next.No - last.No > 0;

                    #region 向上

                    if ((last.Direction != TH_AutoSearchTrack.Direction.Down || last.Distance != lastStack.Length + lastStack.SetKeepL) &&
                        (last.Direction != TH_AutoSearchTrack.Direction.Left || last.Distance != lastStack.Width + lastStack.SetKeepU) &&
                        Upward)
                    {
                        ROUTE tRoute = new ROUTE(); tRoute.IsLeft = last.IsLeft; tRoute.No = last.No; tRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        tRoute.Distance = lastStack.Width + lastStack.SetKeepU;
                        tRoute.MapPoint = getRouteMapPoint(tRoute);
                        Route.Add(tRoute);
                    }

                    for (int i = last.No - 1; i > next.No; i--)
                    {
                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = false; dRoute.No = i; dRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        dRoute.Distance = -Stacks[i].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);

                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = false; uRoute.No = i; uRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        uRoute.Distance = Stacks[i].Width + Stacks[i].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);
                    }

                    if (Upward)
                    {
                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = false; dRoute.No = next.No; dRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        dRoute.Distance = -Stacks[next.No].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);

                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = false; uRoute.No = next.No; uRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        uRoute.Distance = Stacks[next.No].Width + Stacks[next.No].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        if (next.Direction == TH_AutoSearchTrack.Direction.Up) { Route.Add(uRoute); }
                    }

                    if (Upward && next.MapPoint.X == Route[Route.Count - 1].MapPoint.X)
                    {
                        ROUTE dRoute = Route[Route.Count - 1];
                        if (next.Distance != dRoute.Distance) { Route.Add(next); }
                        return;
                    }

                    #endregion

                    #region 向下

                    if ((last.Direction != TH_AutoSearchTrack.Direction.Down || last.Distance != lastStack.Length + lastStack.SetKeepL) &&
                        (last.Direction != TH_AutoSearchTrack.Direction.Left || last.Distance != -lastStack.SetKeepD) &&
                        Downward)
                    {
                        ROUTE tRoute = new ROUTE(); tRoute.IsLeft = last.IsLeft; tRoute.No = last.No; tRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        tRoute.Distance = -lastStack.SetKeepD;
                        tRoute.MapPoint = getRouteMapPoint(tRoute);
                        Route.Add(tRoute);
                    }

                    for (int i = last.No + 1; i < next.No; i++)
                    {
                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = false; uRoute.No = i; uRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        uRoute.Distance = Stacks[i].Width + Stacks[i].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);

                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = false; dRoute.No = i; dRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        dRoute.Distance = -Stacks[i].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        Route.Add(dRoute);
                    }

                    if (Downward)
                    {
                        ROUTE uRoute = new ROUTE(); uRoute.IsLeft = false; uRoute.No = next.No; uRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        uRoute.Distance = Stacks[next.No].Width + Stacks[next.No].SetKeepU;
                        uRoute.MapPoint = getRouteMapPoint(uRoute);
                        Route.Add(uRoute);

                        ROUTE dRoute = new ROUTE(); dRoute.IsLeft = false; dRoute.No = next.No; dRoute.Direction = TH_AutoSearchTrack.Direction.Left;
                        dRoute.Distance = -Stacks[next.No].SetKeepD;
                        dRoute.MapPoint = getRouteMapPoint(dRoute);
                        if (next.Direction == TH_AutoSearchTrack.Direction.Down) { Route.Add(dRoute); }
                    }

                    if (Downward && next.MapPoint.X == Route[Route.Count - 1].MapPoint.X)
                    {
                        ROUTE dRoute = Route[Route.Count - 1];
                        if (next.Distance != dRoute.Distance) { Route.Add(next); }
                        return;
                    }

                    #endregion
                }

                #endregion
            }

            #region 到达目标点

            last = Route[Route.Count - 1];
            lastStack = Stacks[last.No];

            if (next.IsLeft && next.Direction == TH_AutoSearchTrack.Direction.Up)
            {
                if (last.Distance != -lastStack.SetKeepU)
                {
                    last.Direction = TH_AutoSearchTrack.Direction.Right;
                    last.Distance = -lastStack.SetKeepU;
                    last.MapPoint = getRouteMapPoint(last);
                    Route.Add(last);
                }
            }
            if (next.IsLeft && next.Direction == TH_AutoSearchTrack.Direction.Down)
            {
                if (last.Distance != lastStack.Width + lastStack.SetKeepD)
                {
                    last.Direction = TH_AutoSearchTrack.Direction.Right;
                    last.Distance = lastStack.Width + lastStack.SetKeepD;
                    last.MapPoint = getRouteMapPoint(last);
                    Route.Add(last);
                }
            }
            if (!next.IsLeft && next.Direction == TH_AutoSearchTrack.Direction.Up)
            {
                if (last.Distance != lastStack.Width + lastStack.SetKeepU)
                {
                    last.Direction = TH_AutoSearchTrack.Direction.Left;
                    last.Distance = lastStack.Width + lastStack.SetKeepU;
                    last.MapPoint = getRouteMapPoint(last);
                    Route.Add(last);
                }
            }
            if (!next.IsLeft && next.Direction == TH_AutoSearchTrack.Direction.Down)
            {
                if (last.Distance != -lastStack.SetKeepD)
                {
                    last.Direction = TH_AutoSearchTrack.Direction.Left;
                    last.Distance = -lastStack.SetKeepD;
                    last.MapPoint = getRouteMapPoint(last);
                    Route.Add(last);
                }
            }

            Route.Add(next);

            #endregion
        }
        public static void ClieckedClear()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (NoOperate) { return; }

            Route.Clear();
        }
        public static void ClieckedUndo()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (NoOperate) { return; }

            if (Route.Count == 0) { return; }
            Route.RemoveAt(Route.Count - 1);
        }
    }
}
