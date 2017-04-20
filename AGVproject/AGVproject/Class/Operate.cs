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
    class OperateMap
    {
        public static Bitmap BaseMapPicture;
        public static Graphics g;
        public static Cursor Cursor;
        public static bool CurrsorInMap;
        
        public static MOUSE MousePosition;

        public static int MapWidth { get { return (int)(HouseMap.HouseLength / Form_Start.config.PixLength); } }
        public static int MapLength { get { return (int)(HouseMap.HouseWidth / Form_Start.config.PixLength); } }

        public static List<STACK> Stacks;
        public static MOUSE_EVENT MouseEvent;
        public static OPERATE Operate;

        public enum MOUSE_EVENT { Wait,Move,LeftClicked,RightClicked,DoubleClicked }
        public enum OPERATE { Wait,EditPoint,EditKeepDistance,Finish,Save }
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

        public static void Initial()
        {
            Form_Start.config.PixLength = 100;

            Stacks = new List<STACK>();
            for (int i = 0; i <= HouseMap.TotalStacks; i++)
            { Stacks.Add(RealStack2MapStack(i)); }
        }

        public static void getBaseMapPicture()
        {
            // 创建图片
            if (BaseMapPicture != null) { BaseMapPicture.Dispose(); }
            BaseMapPicture = new Bitmap(MapWidth, MapLength);

            // 创建画笔，纯色填充
            g = Graphics.FromImage(BaseMapPicture);
            g.FillRectangle(Brushes.White, new Rectangle(0, 0, MapWidth, MapLength));

            // 是否打开地图
            if (!Form_Start.config.CheckMap) { return; }

            // 添加堆垛
            for (int i = 0; i < Stacks.Count; i++)
            {
                g.FillRectangle(Brushes.LightBlue, Stacks[i].xBG, Stacks[i].yBG, Stacks[i].Length, Stacks[i].Width);
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

        public static void getUltraSonicData()
        {
            if (!Form_Start.config.CheckControlPort) { return; }

            Font stringfont = new Font("Arial", 10);
            int[] SonicData = TH_SendCommand.getUltraSonicData();
            if (SonicData == null) { SonicData = new int[8]; }

            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Head_L_X].ToString(), stringfont, Brushes.Black, 0, 20);
            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Head_L_Y].ToString(), stringfont, Brushes.Black, 20, 0);
            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Head_R_X].ToString(), stringfont, Brushes.Black, 100, 20);
            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Head_R_Y].ToString(), stringfont, Brushes.Black, 80, 0);
            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Tail_L_X].ToString(), stringfont, Brushes.Black, 0, 150);
            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Tail_L_Y].ToString(), stringfont, Brushes.Black, 20, 170);
            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Tail_R_X].ToString(), stringfont, Brushes.Black, 100, 150);
            g.DrawString(SonicData[(int)TH_SendCommand.Sonic.Tail_R_Y].ToString(), stringfont, Brushes.Black, 80, 170);
        }
        public static void getUrgData()
        {
            if (!Form_Start.config.CheckUrgPort) { return; }

            // 画出车的位置
            int xCar = (int)(180 + Hardware_PlatForm.AxisSideL / Form_Start.config.urgRange * 180);
            int yCar = (int)(180 - Hardware_PlatForm.AxisSideU / Form_Start.config.urgRange * 180);
            int CarL = (int)(Hardware_PlatForm.Length / Form_Start.config.urgRange * 180);
            int CarW = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * 180);

            g.FillRectangle(Brushes.LightGray, xCar, yCar, CarW, CarL);
            g.FillEllipse(Brushes.Red, 176, 176, 8, 8); // 原点

            // 周围环境信息
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingD(0, Form_Start.config.urgRange);
            for (int i = 0; i < points.Count; i++)
            {
                int x = (int)(180 * points[i].x / Form_Start.config.urgRange + 180);
                int y = (int)(180 - 180 * points[i].y / Form_Start.config.urgRange);

                g.FillEllipse(Brushes.Black, x, y, 2, 2);
            }
        }
        public static void getLocateData()
        {
            if (!Form_Start.config.CheckLocatePort) { return; }

            CoordinatePoint.POINT point = TH_MeasurePosition.getPosition();
            Font stringfont = new Font("Arial", 10);

            g.DrawString("X: " + point.x.ToString(), stringfont, Brushes.Black, 30, 260);
            g.DrawString("Y: " + point.y.ToString(), stringfont, Brushes.Black, 30, 280);
            g.DrawString("A: " + point.aCar.ToString(), stringfont, Brushes.Black, 30, 300);
            g.DrawString("R: " + point.rCar.ToString(), stringfont, Brushes.Black, 30, 320);
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
            stack.SetKeepD = (int)(HouseMap.Stacks[No].KeepDistanceU / Form_Start.config.PixLength);
            stack.SetKeepL = (int)(HouseMap.Stacks[No].KeepDistanceU / Form_Start.config.PixLength);
            stack.SetKeepR = (int)(HouseMap.Stacks[No].KeepDistanceU / Form_Start.config.PixLength);

            stack.SetKeepU = 3;
            stack.SetKeepD = 3;
            stack.SetKeepL = 10;
            stack.SetKeepR = 10;

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

    class OperateRoute
    {
        public static List<ROUTE> Route;
        public struct ROUTE
        {
            public bool IsLeft;
            public int No;
            public TH_AutoSearchTrack.Direction Direction;
            public int Distance;

            public Point MapPoint;
        }

        public static void getPermitRoute()
        {
            if (OperateMap.Stacks == null || OperateMap.Stacks.Count == 0) { return; }
            if (!Form_Start.config.CheckRoute) { return; }

            int xBG, xED, yBG, yED;

            foreach (OperateMap.STACK stack in OperateMap.Stacks)
            {
                // 左
                xBG = stack.xBG - stack.SetKeepL;
                xED = xBG;
                yBG = stack.yBG - stack.SetKeepU;
                yED = stack.yBG + stack.Width + stack.SetKeepD;
                OperateMap.g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);

                // 上
                xBG = stack.xBG - stack.SetKeepL;
                xED = stack.xBG + stack.Length + stack.SetKeepR;
                yBG = stack.yBG - stack.SetKeepU;
                yED = yBG;
                OperateMap.g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);

                // 右
                xBG = stack.xBG + stack.Length + stack.SetKeepR;
                xED = xBG;
                yBG = stack.yBG - stack.SetKeepU;
                yED = stack.yBG + stack.Width + stack.SetKeepD;
                OperateMap.g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);

                // 下
                xBG = stack.xBG - stack.SetKeepL;
                xED = stack.xBG + stack.Length + stack.SetKeepR;
                yBG = stack.yBG + stack.Width + stack.SetKeepD;
                yED = yBG;
                OperateMap.g.DrawLine(Pens.LightBlue, xBG, yBG, xED, yED);
            }
        }
        public static void getCurrentRoute()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (Route == null || Route.Count == 0) { return; }
            
            for (int i = 0; i < Route.Count - 1; i++)
            {
                OperateMap.g.DrawLine(Pens.Blue, Route[i].MapPoint, Route[i + 1].MapPoint);
            }

            for (int i = 1; i < Route.Count - 1; i++)
            {
                OperateMap.g.DrawEllipse(Pens.Blue, Route[i].MapPoint.X - 2, Route[i].MapPoint.Y - 2, 4, 4);
            }

            OperateMap.g.DrawEllipse(Pens.Red, Route[0].MapPoint.X - 2, Route[0].MapPoint.Y - 2, 4, 4);
            OperateMap.g.DrawEllipse(Pens.Red, Route[Route.Count - 1].MapPoint.X - 2, Route[Route.Count - 1].MapPoint.Y - 2, 4, 4);
        }

        public static void MouseLeftClicked()
        {
            if (OperateMap.MouseEvent != OperateMap.MOUSE_EVENT.LeftClicked) { return; }
            OperateMap.MouseEvent = OperateMap.MOUSE_EVENT.Wait;

            if (!Form_Start.config.CheckRoute) { return; }
            if (!OperateMap.CurrsorInMap) { return; }
            if (OperateMap.Operate == OperateMap.OPERATE.Finish) { return; }
            

            OperateMap.MOUSE mousePos = OperateMap.getMousePosition();

            if (mousePos.No == 0) { return; }
            if (mousePos.Direction == TH_AutoSearchTrack.Direction.Tuning) { return; }

            ROUTE route = new ROUTE();
            route.IsLeft = mousePos.IsLeft;
            route.No = mousePos.No;
            route.Direction = mousePos.Direction;
            route.Distance = mousePos.Distance;
            route.MapPoint = getRouteMapPoint(route);

            
            if (Route == null) { Route = new List<ROUTE>(); }
            Route.Add(route);
        }
        public static void MouseMove()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (!OperateMap.CurrsorInMap) { return; }
            if (OperateMap.Operate == OperateMap.OPERATE.Finish) { return; }
            if (OperateMap.Operate == OperateMap.OPERATE.Save) { return; }
            if (Route == null || Route.Count == 0) { return; }

            OperateMap.MOUSE mousePos = OperateMap.getMousePosition();

            Point ptBG = Route[Route.Count - 1].MapPoint;
            Point ptED = new Point(mousePos.x, mousePos.y);

            OperateMap.g.DrawLine(Pens.Blue, ptBG, ptED);
        }

        public static void Save()
        {
            if (OperateMap.Operate != OperateMap.OPERATE.Save) { return; }
            OperateMap.Operate = OperateMap.OPERATE.Finish;
            if (!Form_Start.config.CheckMap && !Form_Start.config.CheckRoute) { return; }

            string exe_path = Application.ExecutablePath;
            exe_path = exe_path.Substring(0, exe_path.LastIndexOf('\\'));
            string FullPath = exe_path + "\\testMap.jpg";

            OperateMap.BaseMapPicture.Save(FullPath);
        }

        public static Point getRouteMapPoint(ROUTE route)
        {
            OperateMap.STACK stack = OperateMap.Stacks[route.No];
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
    }
}
