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
            NoOperate = false;

            Stacks = new List<STACK>();
            for (int i = 0; i <= HouseMap.TotalStacks; i++)
            { Stacks.Add(RealStack2MapStack(i)); }

            Route = new List<ROUTE>();
        }
        public static void Updata()
        {
            getCursorShape();
            getFont();
            getBaseMapPicture();
            getPermitRoute();
            getCurrentRoute();
            getSelectRoute();
            getUrgData();
            getUltraSonicData();
            getLocateData();
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
            string istr; SizeF size;
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
            if (mouse.No == 0) { Cursor = Cursors.Default; return; }
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
            if (Route.Count <= 1) { return; }
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

        public static HouseMap.STACK MapStack2ReadStack(int No)
        {
            return new HouseMap.STACK();
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

            return pt;
        }

        public static void MouseLeftClicked()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (!CurrsorInMap) { return; }
            if (NoOperate) { return; }
            if (DrawOver) { return; }

            MOUSE mousePos = getMousePosition();

            if (mousePos.No == 0) { PushedCursor = true; Cursor = Cursors.No; return; }
            if (mousePos.Direction == TH_AutoSearchTrack.Direction.Tuning) { PushedCursor = true; Cursor = Cursors.No; return; }

            if (Route != null && Route.Count != 0)
            {
                ROUTE last = Route[Route.Count - 1];
                STACK stack = Stacks[last.No];

                if (Math.Abs(mousePos.x - last.MapPoint.X) > 3 && Math.Abs(mousePos.y - last.MapPoint.Y) > 3)
                { PushedCursor = true; Cursor = Cursors.No; return; }

                int BG = stack.yBG - stack.AisleWidth_U;
                int ED = stack.yBG;
                bool InAisleU = BG < mousePos.y && mousePos.y < ED && BG < last.MapPoint.Y && last.MapPoint.Y < ED;

                BG = stack.yBG + stack.Width;
                ED = BG + stack.AisleWidth_D;
                bool InAisleD = BG < mousePos.y && mousePos.y < ED && BG < last.MapPoint.Y && last.MapPoint.Y < ED;

                BG = stack.xBG - stack.AisleWidth_L;
                ED = stack.xBG;
                bool InAisleL = BG < mousePos.x && mousePos.x < ED && BG < last.MapPoint.X && last.MapPoint.X < ED;
                if (last.IsLeft) { InAisleL = false; }

                BG = stack.xBG + stack.Length;
                ED = BG + stack.AisleWidth_R;
                bool InAisleR = BG < mousePos.x && mousePos.x < ED && BG < last.MapPoint.X && last.MapPoint.X < ED;
                if (!last.IsLeft) { InAisleR = false; }

                if (!InAisleL && !InAisleR && !InAisleU && !InAisleD)
                { PushedCursor = true; Cursor = Cursors.No; return; }
            }

            ROUTE route = new ROUTE();
            route.IsLeft = mousePos.IsLeft;
            route.No = mousePos.No;
            route.Direction = mousePos.Direction;
            route.Distance = mousePos.Distance;
            route.MapPoint = getRouteMapPoint(route);

            if (Route == null) { Route = new List<ROUTE>(); }
            Route.Add(route);
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
