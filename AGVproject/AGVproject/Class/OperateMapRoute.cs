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
        public static Cursor Cursor;
        public static bool CurrsorInMap;

        public static Graphics g;
        
        public static double PixLength;
        public static int MapWidth { get { return (int)(HouseMap.HouseLength / PixLength); } }
        public static int MapLength { get { return (int)(HouseMap.HouseWidth / PixLength); } }
        public static Point MousePosition;

        public static List<STACK> Stacks;
        public static List<ROUTE> Route;

        public static OPERATE Operate;

        public enum OPERATE { Start, SelectPoint, SetPoint, SelectLine, SetLinePos, Save, Finished }
        public struct STACK
        {
            public bool IsLeft;
            public bool IsCross;
            public int No;
            public int StackUL, StackUR, StackDL, StackDR;

            public int xBG, yBG, xED, yED;
            public int Length, Width;
            public int AisleWidth_U, AisleWidth_D, AisleWidth_L, AisleWidth_R;
            
            public TH_AutoSearchTrack.Direction Direction;
            public int SetKeepU, SetKeepD, SetKeepL, SetKeepR;
        }
        public struct ROUTE
        {
            public bool IsCross;
            public int StackUL, StackUR, StackDL, StackDR;

            public int No;
            public Point MapPosition;

            public TH_AutoSearchTrack.Direction Direction;
            public double Distance;
        }
        
        public static void Initial()
        {
            Cursor = Cursors.Default;
            CurrsorInMap = true;

            PixLength = 100;
            MousePosition = new Point(0, 0);

            Stacks = new List<STACK>();
            for (int i = 1; i <= HouseMap.TotalStacks; i++) { Stacks.Add(getHouseStackInfo(i)); }
            Route = new List<ROUTE>();

            Operate = OPERATE.SelectPoint;
        }
        
        public static void RefreshMap()
        {
            getBaseMapPicture();
            getUltraSonicData();
            getUrgData();
            getLocateData();

            getCursorShape();
            getMouseMove();
            getMouseClicked();

            getCurrentRoute();
            getPermitRoute();
            SaveMapPicture();

            if (Operate != OPERATE.Finished) { Operate = OPERATE.SelectPoint; }
            
            g.Dispose();
        }

        private static void getBaseMapPicture()
        {
            // 创建图片
            if (BaseMapPicture != null) { BaseMapPicture.Dispose(); }
            BaseMapPicture = new Bitmap(MapWidth,MapLength);

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
        private static void getPermitRoute()
        {
            if (!Form_Start.config.CheckMap) { return; }
            if (!Form_Start.config.CheckRoute) { return; }

            foreach (STACK stack in Stacks)
            {
                Point BG_U = new Point(stack.xBG - stack.SetKeepL, stack.yBG - stack.SetKeepU);
                Point ED_U = new Point(stack.xED + stack.SetKeepR, stack.yBG - stack.SetKeepU);

                Point BG_D = new Point(stack.xBG - stack.SetKeepL, stack.yED + stack.SetKeepD);
                Point ED_D = new Point(stack.xED + stack.SetKeepR, stack.yED + stack.SetKeepD);

                Point BG_L = new Point(stack.xBG - stack.SetKeepL, stack.yBG - stack.SetKeepU);
                Point ED_L = new Point(stack.xBG - stack.SetKeepL, stack.yED + stack.SetKeepD);

                Point BG_R = new Point(stack.xED + stack.SetKeepR, stack.yBG - stack.SetKeepU);
                Point ED_R = new Point(stack.xED + stack.SetKeepR, stack.yED + stack.SetKeepD);

                
                g.DrawLine(Pens.LightBlue, BG_U, ED_U);
                g.DrawLine(Pens.LightBlue, BG_D, ED_D);
                if (stack.IsLeft) { g.DrawLine(Pens.LightBlue, BG_R, ED_R); }
                if (!stack.IsLeft) { g.DrawLine(Pens.LightBlue, BG_L, ED_L); }

                g.FillEllipse(Brushes.Red, BG_U.X - 2, BG_U.Y - 2, 4, 4);
                g.FillEllipse(Brushes.Red, ED_U.X - 2, ED_U.Y - 2, 4, 4);
                g.FillEllipse(Brushes.Red, BG_D.X - 2, BG_D.Y - 2, 4, 4);
                g.FillEllipse(Brushes.Red, ED_D.X - 2, ED_D.Y - 2, 4, 4);

                if (stack.IsLeft)
                {
                    g.FillEllipse(Brushes.Red, BG_R.X - 2, BG_U.Y - 2, 4, 4);
                    g.FillEllipse(Brushes.Red, ED_R.X - 2, ED_U.Y - 2, 4, 4);
                }
                else
                {
                    g.FillEllipse(Brushes.Red, BG_L.X - 2, BG_U.Y - 2, 4, 4);
                    g.FillEllipse(Brushes.Red, ED_L.X - 2, ED_U.Y - 2, 4, 4);
                }
            }
        }
        private static void getUltraSonicData()
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
        private static void getUrgData()
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
        private static void getLocateData()
        {
            if (!Form_Start.config.CheckLocatePort) { return; }

            CoordinatePoint.POINT point = TH_MeasurePosition.getPosition();
            Font stringfont = new Font("Arial", 10);

            g.DrawString("X: " + point.x.ToString(), stringfont, Brushes.Black, 30, 260);
            g.DrawString("Y: " + point.y.ToString(), stringfont, Brushes.Black, 30, 280);
            g.DrawString("A: " + point.aCar.ToString(), stringfont, Brushes.Black, 30, 300);
            g.DrawString("R: " + point.rCar.ToString(), stringfont, Brushes.Black, 30, 320);
        }

        private static void getCursorShape()
        {
            Cursor = Cursors.Default;

            // 是否执行本函数
            if (!Form_Start.config.CheckMap) { return; }
            if (!CurrsorInMap) { return; }

            // 获取形状
            STACK mousePos = getMousePosition();
            if (mousePos.No == 0) { Cursor = Cursors.Default; }
            if (mousePos.Direction == TH_AutoSearchTrack.Direction.Tuning) { Cursor = Cursors.No; }
            if (mousePos.IsCross) { Cursor = Cursors.Cross; }

            //TH_AutoSearchTrack.Direction line = getMouseOnLine(mousePos);
            //if (line == TH_AutoSearchTrack.Direction.Up) { Cursor = Cursors.SizeNS; }
            //if (line == TH_AutoSearchTrack.Direction.Down) { Cursor = Cursors.SizeNS; }
            //if (line == TH_AutoSearchTrack.Direction.Left) { Cursor = Cursors.SizeWE; }
            //if (line == TH_AutoSearchTrack.Direction.Right) { Cursor = Cursors.SizeWE; }
        }
        private static void getMouseMove()
        {
            // 是否执行本函数
            if (!Form_Start.config.CheckMap) { return; }
            if (!CurrsorInMap) { return; }
            if (Operate != OPERATE.SelectPoint) { return; }

            // 连线
            if (Route.Count == 0) { return; }
            
            Pen p = new Pen(Color.Blue, 4);
            p.StartCap = LineCap.Square;
            p.EndCap = LineCap.ArrowAnchor;
            g.DrawLine(p, Route[Route.Count - 1].MapPosition, MousePosition);
        }
        private static void getMouseClicked()
        {
            // 是否执行本函数
            if (!Form_Start.config.CheckMap) { return; }
            if (!CurrsorInMap) { return; }
            if (Operate != OPERATE.SetPoint) { return; }
            
            // 获取鼠标位置
            STACK mousePos = getMousePosition();
            if (mousePos.No == 0 && !mousePos.IsCross) { return; }

            ROUTE mark = new ROUTE();
            mark.IsCross = false;
            mark.No = mousePos.No;
            mark.Direction = TH_AutoSearchTrack.Direction.Tuning;

            // 标点
            if (mousePos.Direction == TH_AutoSearchTrack.Direction.Left && !mousePos.IsLeft)
            {
                int x = mousePos.xBG - mousePos.SetKeepL;
                int y = MousePosition.Y;

                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Left;
                mark.Distance = (mousePos.yED - MousePosition.Y) * PixLength;
            }
            if (mousePos.Direction == TH_AutoSearchTrack.Direction.Right && mousePos.IsLeft)
            {
                int x = mousePos.xED + mousePos.SetKeepR;
                int y = MousePosition.Y;

                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Right;
                mark.Distance = (MousePosition.Y - mousePos.yBG) * PixLength;
            }
            if (mousePos.Direction == TH_AutoSearchTrack.Direction.Up)
            {
                int x = MousePosition.X;
                int y = mousePos.yBG - mousePos.SetKeepU;

                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Up;
                mark.Distance = (MousePosition.X - mousePos.xBG) * PixLength;
            }
            if (mousePos.Direction == TH_AutoSearchTrack.Direction.Down)
            {
                int x = MousePosition.X;
                int y = mousePos.yED + mousePos.SetKeepD;

                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Up;
                mark.Distance = (mousePos.xED - MousePosition.X) * PixLength;
            }

            if (mousePos.IsCross && mousePos.StackUL != 0 && !mousePos.IsLeft)
            {
                int x = mousePos.xBG - mousePos.SetKeepL;
                int y = mousePos.yBG - mousePos.SetKeepU;

                mark.IsCross = true;
                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Tuning;
            }
            if (mousePos.IsCross && mousePos.StackUR != 0 && mousePos.IsLeft)
            {
                int x = mousePos.xED + mousePos.SetKeepR;
                int y = mousePos.yBG - mousePos.SetKeepU;

                mark.IsCross = true;
                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Tuning;
            }
            if (mousePos.IsCross && mousePos.StackDL != 0 && !mousePos.IsLeft)
            {
                int x = mousePos.xBG - mousePos.SetKeepL;
                int y = mousePos.yED + mousePos.SetKeepD;

                mark.IsCross = true;
                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Tuning;
            }
            if (mousePos.IsCross && mousePos.StackDR != 0 && mousePos.IsLeft)
            {
                int x = mousePos.xED + mousePos.SetKeepR;
                int y = mousePos.yED + mousePos.SetKeepD;

                mark.IsCross = true;
                mark.MapPosition = new Point(x, y);
                mark.Direction = TH_AutoSearchTrack.Direction.Tuning;
            }

            // 判断能否标点
            if (mark.IsCross || (mark.Direction != TH_AutoSearchTrack.Direction.Tuning && !mark.IsCross))
            {
                if (Route.Count == 0) { Route.Add(mark); return; }

                ROUTE Last = Route[Route.Count - 1];
                ROUTE Next = mark;

                Route.Add(mark);
            }
        }
        private static void getCurrentRoute()
        {
            // 是否执行本函数
            if (!Form_Start.config.CheckMap) { return; }
            if (Route.Count == 0) { return; }
            if (!Form_Start.config.CheckRoute) { return; }
            
            // 划线
            for (int i = 0; i < Route.Count - 1; i++)
            {
                Pen p = new Pen(Color.Blue, 4);
                p.StartCap = LineCap.Square;
                p.EndCap = LineCap.ArrowAnchor;
                g.DrawLine(p, Route[i].MapPosition, Route[i + 1].MapPosition);
            }

            // 标点
            g.FillEllipse(Brushes.Red, Route[0].MapPosition.X - 4, Route[0].MapPosition.Y - 4, 8, 8);
            if (Operate == OPERATE.Finished)
            { g.FillEllipse(Brushes.Red, Route[Route.Count - 1].MapPosition.X - 4, Route[Route.Count - 1].MapPosition.Y - 4, 8, 8); }

            for (int i = 0; i < Route.Count; i++)
            {
                Font stringfont = new Font("Arial", 8);
                g.DrawString(i.ToString(), stringfont, Brushes.Black, Route[i].MapPosition);
            }
        }

        private static void SaveMapPicture()
        {
            // 是否执行本函数
            if (!Form_Start.config.CheckMap) { return; }
            if (Operate != OPERATE.Save) { return; }

            string exe_path = Application.ExecutablePath;
            exe_path = exe_path.Substring(0, exe_path.LastIndexOf('\\'));
            string FullPath = exe_path + "\\testMap.jpg";

            BaseMapPicture.Save(FullPath);
            Operate = OPERATE.Finished;
        }

        private static STACK getMousePosition()
        {
            STACK MousePosition = new STACK();
            int x = OperateMap.MousePosition.X;
            int y = OperateMap.MousePosition.Y;

            foreach (STACK stack in Stacks)
            {
                MousePosition = stack;

                int xBG = stack.xBG - stack.SetKeepL;
                int xED = stack.xED + stack.SetKeepR;
                int yBG = stack.yBG - stack.SetKeepU;
                int yED = stack.yED + stack.SetKeepD;

                if (x < xBG || x > xED) { continue; }
                if (y < yBG || y > yED) { continue; }

                if (xBG <= x && x < stack.xBG && stack.yBG < y && y < stack.yED)
                { MousePosition.Direction = TH_AutoSearchTrack.Direction.Left; return MousePosition; }

                if (stack.xBG < x && x < stack.xED && yBG <= y && y < stack.yBG)
                { MousePosition.Direction = TH_AutoSearchTrack.Direction.Up; return MousePosition; }

                if (stack.xBG < x && x < stack.xED && stack.yED < y && y <= yED)
                { MousePosition.Direction = TH_AutoSearchTrack.Direction.Down; return MousePosition; }

                if (stack.xED < x && x <= xED && stack.yBG < y && y < stack.yED)
                { MousePosition.Direction = TH_AutoSearchTrack.Direction.Right; return MousePosition; }

                if (stack.xBG <= x && x <= stack.xED && stack.yBG <= y && y <= stack.yED)
                { MousePosition.Direction = TH_AutoSearchTrack.Direction.Tuning; return MousePosition; }

                if (xBG <= x && x < stack.xBG && yBG <= y && y < stack.yBG)
                { MousePosition.IsCross = true; MousePosition.StackUL = stack.No; return MousePosition; }

                if (xBG <= x && x < stack.xBG && stack.yED < y && y <= yED)
                { MousePosition.IsCross = true; MousePosition.StackDL = stack.No; return MousePosition; }

                if (stack.xED < x && x <= xED && yBG <= y && y < stack.yBG)
                { MousePosition.IsCross = true; MousePosition.StackUR = stack.No; return MousePosition; }

                if (stack.xED < x && x <= xED && stack.yED < y && y <= yED)
                { MousePosition.IsCross = true; MousePosition.StackUL = stack.No; return MousePosition; }
            }

            return new STACK();
        }
        private static STACK getHouseStackInfo(int No)
        {
            if (No < 1 || No > HouseMap.TotalStacks) { return new STACK(); }
            STACK stack = new STACK();

            stack.IsLeft = HouseMap.Stacks[No].IsLeft;
            stack.No = No;
            
            stack.Length = (int)(HouseMap.Stacks[No].Length / PixLength);
            stack.Width = (int)(HouseMap.Stacks[No].Width / PixLength);
            stack.AisleWidth_U = (int)(HouseMap.Stacks[No].AisleWidth_U / PixLength);
            stack.AisleWidth_D = (int)(HouseMap.Stacks[No].AisleWidth_D / PixLength);
            stack.AisleWidth_L = (int)(HouseMap.Stacks[No].AisleWidth_L / PixLength);
            stack.AisleWidth_R = (int)(HouseMap.Stacks[No].AisleWidth_R / PixLength);

            stack.Direction = TH_AutoSearchTrack.Direction.Tuning;
            stack.SetKeepU = stack.AisleWidth_U / 2;
            stack.SetKeepD = stack.AisleWidth_D / 2;
            stack.SetKeepL = HouseMap.Stacks[No].IsLeft ? stack.AisleWidth_L : stack.AisleWidth_L / 2;
            stack.SetKeepR = HouseMap.Stacks[No].IsLeft ? stack.AisleWidth_R / 2 : stack.AisleWidth_R;

            //stack.SetKeepU -= 1;
            //stack.SetKeepD -= 1;
            //stack.SetKeepL -= 1;
            //stack.SetKeepR -= 1;

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

            stack.xBG = (int)(xBG / PixLength);
            stack.yBG = (int)(yBG / PixLength);
            stack.xED = stack.xBG + stack.Length;
            stack.yED = stack.yBG + stack.Width; return stack;
        }
        private static TH_AutoSearchTrack.Direction getMouseOnLine(STACK stack)
        {
            int U = stack.yBG - stack.SetKeepU;
            int D = stack.yED + stack.SetKeepD;
            int L = stack.xBG - stack.SetKeepL;
            int R = stack.xED + stack.SetKeepR;

            int X = MousePosition.X;
            int Y = MousePosition.Y;

            if (Math.Abs(Y - U) < 3) { return TH_AutoSearchTrack.Direction.Up; }
            if (Math.Abs(Y - D) < 3) { return TH_AutoSearchTrack.Direction.Down; }
            if (stack.IsLeft && Math.Abs(X - R) < 3) { return TH_AutoSearchTrack.Direction.Right; }
            if (!stack.IsLeft && Math.Abs(X - L) < 3) { return TH_AutoSearchTrack.Direction.Left; }

            return TH_AutoSearchTrack.Direction.Tuning;
        }
    }

    class OperateRoute
    {
    }
}
