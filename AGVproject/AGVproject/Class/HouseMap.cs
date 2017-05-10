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
    /// 绘制地图
    /// </summary>
    class HouseMap
    {
        ////////////////////////////////////////////// public attribute ///////////////////////////////////////////

        /// <summary>
        /// 仓库长度（仓库前后距离） 单位：mm
        /// </summary>
        public static double HouseLength;
        /// <summary>
        /// 仓库宽度（仓库左右距离） 单位：mm
        /// </summary>
        public static double HouseWidth;
        /// <summary>
        /// 一个像素点所代表的长度 单位：mm
        /// </summary>
        public static double PixLength;

        /// <summary>
        /// 显示地图图片（不允许在刷新界面以外的线程使用）
        /// </summary>
        public static Bitmap Map;
        /// <summary>
        /// 显示鼠标形状
        /// </summary>
        public static Cursor Cursor;

        /// <summary>
        /// 地图图片高度 单位：像素
        /// </summary>
        public static int MapHeight { get { return (int)(HouseLength / PixLength); } }
        /// <summary>
        /// 地图图片宽度 单位：像素
        /// </summary>
        public static int MapWidth { get { return (int)(HouseWidth / PixLength); } }

        /// <summary>
        /// 绘图区高度 单位：像素
        /// </summary>
        public static int PictureBoxHeight;
        /// <summary>
        /// 绘图区宽度 单位：像素
        /// </summary>
        public static int PictureBoxWidth;
        /// <summary>
        /// X 方向的滚屏量
        /// </summary>
        public static int xScroll;
        /// <summary>
        /// Y 方向的滚屏量
        /// </summary>
        public static int yScroll;
        /// <summary>
        /// 窗口高度
        /// </summary>
        public static int FormHeight;
        /// <summary>
        /// 窗口宽度
        /// </summary>
        public static int FormWidth;

        /// <summary>
        /// 不允许操作图片
        /// </summary>
        public static bool NoOperate;
        /// <summary>
        /// 路径已画完
        /// </summary>
        public static bool DrawOver;
        /// <summary>
        /// 鼠标形状已被更改
        /// </summary>
        public static bool PushedCursor;
        /// <summary>
        /// 鼠标位于图内
        /// </summary>
        public static bool CursorInMap;
        /// <summary>
        /// 显示允许路径
        /// </summary>
        public static bool ShowPermitTrack;

        /// <summary>
        /// 鼠标位置信息
        /// </summary>
        public struct MOUSE
        {
            /// <summary>
            /// 鼠标在图片上的 X 轴坐标
            /// </summary>
            public int X;
            /// <summary>
            /// 鼠标在图片上的 Y 轴坐标
            /// </summary>
            public int Y;

            /// <summary>
            /// 鼠标与门、堆垛、路径等的相对位置
            /// </summary>
            public POSITION Position;
            /// <summary>
            /// 位于堆垛的哪一边
            /// </summary>
            public TH_AutoSearchTrack.Direction Side;
            /// <summary>
            /// 小车与堆垛边的距离
            /// </summary>
            public double Distance;

            /// <summary>
            /// 当前堆垛编号
            /// </summary>
            public int StackNo;
            /// <summary>
            /// 当前路径编号（1 开始）
            /// </summary>
            public int LineNo;
            /// <summary>
            /// 当前路径编号（0 开始）
            /// </summary>
            public int TrackNo;

            /// <summary>
            /// 鼠标与门、堆垛、路径等的相对位置
            /// </summary>
            public enum POSITION
            {
                /// <summary>
                /// 在门的位置上
                /// </summary>
                OnDoor,
                /// <summary>
                /// 在堆垛上
                /// </summary>
                OnStack,
                /// <summary>
                /// 在路径直线上
                /// </summary>
                OnLine,
                /// <summary>
                /// 在路径端点上
                /// </summary>
                OnTrack,
                /// <summary>
                /// 在障碍物上
                /// </summary>
                OnBarrier,
                /// <summary>
                /// 没在参考物附近
                /// </summary>
                Unknow
            }
        }

        ////////////////////////////////////////////// public attribute ///////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public object MouseLock;
            public object MapLock;

            public Graphics g;
            public MOUSE mouse;
            public Font font;
        }
        
        ////////////////////////////////////////////// public method ///////////////////////////////////////////

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Initial()
        {
            HouseLength = 0;
            HouseWidth = 0;
            PixLength = 0;
            Map = null;
            Cursor = Cursors.Default;

            NoOperate = false;
            DrawOver = true;
            PushedCursor = false;
            CursorInMap = false;
            
            config.MouseLock = new object();
            config.MapLock = new object();
            config.g = null;
            config.mouse = new MOUSE();
            config.font = new Font("Arial", 10);
        }
        /// <summary>
        /// 绘制地图
        /// </summary>
        public static void DrawMap()
        {
            lock (config.MapLock)
            {
                getFont();
                drawStacks();
                drawPermitTrack();
                drawCurrentTrack();
                
                drawUrgData();
                drawUltraSonicData();
                drawCarPosition();

                drawMove();
                drawCursor();

                PushedCursor = false;
            }
        }

        /// <summary>
        /// 把实际长度转换为图片上的长度
        /// </summary>
        /// <param name="length">距离 单位：mm</param>
        /// <returns></returns>
        public static int Length_Real2Map(double length)
        {
            return (int)(length / PixLength);
        }
        /// <summary>
        /// 把图片上的长度转换为实际长度
        /// </summary>
        /// <param name="length">像素跨度 单位：像素</param>
        /// <returns></returns>
        public static double Length_Map2Real(int length)
        {
            return length * PixLength;
        }
        /// <summary>
        /// 把仓库位置坐标转换为图片上的位置坐标
        /// </summary>
        /// <param name="point">仓库位置坐标</param>
        /// <returns></returns>
        public static Point Point_House2Map(CoordinatePoint.POINT point)
        {
            return new Point(Length_Real2Map(point.x), Length_Real2Map(point.y));
        }
        /// <summary>
        /// 把图片上的位置坐标转换为仓库坐标系下的位置坐标
        /// </summary>
        /// <param name="point">图片上的位置坐标</param>
        /// <returns></returns>
        public static CoordinatePoint.POINT Point_Map2House(Point point)
        {
            double x = Length_Map2Real(point.X);
            double y = Length_Map2Real(point.Y);

            return CoordinatePoint.Create_XY(x, y);
        }

        /// <summary>
        /// 获取当前图片的备份
        /// </summary>
        /// <returns></returns>
        public static Bitmap getMap()
        {
            Bitmap copyMap = null;
            lock (config.MapLock) { copyMap = (Bitmap)Map.Clone(); }
            return copyMap;
        }
        /// <summary>
        /// 获取鼠标详细位置信息
        /// </summary>
        /// <returns></returns>
        public static MOUSE getMousePosition()
        {
            MOUSE mouse = new MOUSE();

            lock (config.MouseLock)
            {
                mouse.X = config.mouse.X;
                mouse.Y = config.mouse.Y;
                mouse.Position = config.mouse.Position;
                mouse.Side = config.mouse.Side;
                mouse.Distance = config.mouse.Distance;
                mouse.StackNo = config.mouse.StackNo;
                mouse.LineNo = config.mouse.LineNo;
                mouse.TrackNo = config.mouse.TrackNo;
            }

            return mouse;
        }
        /// <summary>
        /// 获取鼠标详细位置信息
        /// </summary>
        /// <param name="X">鼠标在图上的 X 轴坐标</param>
        /// <param name="Y">鼠标在图上的 Y 轴坐标</param>
        /// <returns></returns>
        public static MOUSE getMousePosition(int X, int Y)
        {
            List<HouseStack.STACK> Stacks = HouseStack.Get();
            List<HouseTrack.TRACK> Track = HouseTrack.Get();
            
            MOUSE mouse = new MOUSE();
            mouse.X = X;
            mouse.Y = Y;
            mouse.StackNo = -1;
            mouse.LineNo = -1;
            mouse.TrackNo = -1;
            mouse.Side = TH_AutoSearchTrack.Direction.Tuning;
            mouse.Distance = 0;
            mouse.Position = MOUSE.POSITION.Unknow;
            if (X > MapWidth || Y > MapHeight) { CursorInMap = false; return mouse; }

            #region 和堆垛的相对位置

            foreach (HouseStack.STACK stack in Stacks)
            {
                int xBG = Length_Real2Map(stack.Position.x);
                int yBG = Length_Real2Map(stack.Position.y);

                int keepU = Length_Real2Map(stack.KeepDistanceU);
                int keepD = Length_Real2Map(stack.KeepDistanceD);
                int keepL = Length_Real2Map(stack.KeepDistanceL);
                int keepR = Length_Real2Map(stack.KeepDistanceR);

                int L = Length_Real2Map(stack.Length);
                int W = Length_Real2Map(stack.Width);

                // OUT
                if (X < xBG - keepL || X > xBG + L + keepR || Y < yBG - keepU || Y > yBG + W + keepD) { continue; }

                // Centre
                if (xBG <= X && X <= xBG + L && yBG <= Y && Y <= yBG + W)
                {
                    mouse.Position = MOUSE.POSITION.OnStack;
                    mouse.Side = TH_AutoSearchTrack.Direction.Tuning;
                    mouse.StackNo = stack.No; break;
                }

                if (!ShowPermitTrack) { continue; }

                // U
                if (xBG <= X && X <= xBG + L && yBG - keepU <= Y && Y <= yBG)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Up;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(X - xBG); break;
                }

                // D
                if (xBG <= X && X <= xBG + L && yBG + W <= Y && Y <= yBG + W + keepD)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Down;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(xBG + L - X); break;
                }

                // L
                if (xBG - keepL <= X && X <= xBG && yBG <= Y && Y <= yBG + W)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Left;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(yBG + W - Y); break;
                }

                // R
                if (xBG + L <= X && X <= xBG + L + keepR && yBG <= Y && Y <= yBG + W)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Right;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(Y - yBG); break;
                }

                // Cross
                int disU = Y - (yBG - keepU);
                int disD = yBG + W + keepD - Y;
                int disL = X - (xBG - keepL);
                int disR = xBG + L + keepR - X;

                if (disU <= disD && disU <= disL && disU <= disR)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Up;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(X - xBG); break;
                }
                if (disD < disU && disD <= disL && disD <= disR)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Down;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(xBG + L - X); break;
                }
                if (disL < disU && disL < disD && disL <= disR)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Left;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(yBG + W - Y); break;
                }
                if (disR < disU && disR < disD && disR < disL)
                {
                    mouse.Side = TH_AutoSearchTrack.Direction.Right;
                    mouse.StackNo = stack.No;
                    mouse.Distance = Length_Map2Real(Y - yBG); break;
                }
            }

            #endregion

            #region 和路径的相对位置

            for (int i = 0; i < Track.Count; i++)
            {
                int xBG = Length_Real2Map(Track[i].TargetPosition.x);
                int yBG = Length_Real2Map(Track[i].TargetPosition.y);

                if (Math.Abs(X - xBG) < 3 && Math.Abs(Y - yBG) < 3)
                { mouse.TrackNo = i; if (mouse.Position == MOUSE.POSITION.Unknow) { mouse.Position = MOUSE.POSITION.OnTrack; } break; }
            }

            for (int i = 0; i < Track.Count - 1; i++)
            {
                if (mouse.Position != MOUSE.POSITION.Unknow) { break; }

                int xBG = Length_Real2Map(Track[i].TargetPosition.x);
                int yBG = Length_Real2Map(Track[i].TargetPosition.y);
                
                int xED = Length_Real2Map(Track[i+1].TargetPosition.x);
                int yED = Length_Real2Map(Track[i+1].TargetPosition.y);

                //if (X < xBG || X > xED || Y < yBG || Y > yED) { continue; }

                int dis = xBG == xED ? Math.Abs(X - xBG) : Math.Abs((int)((double)(X - xBG) / (xED - xBG) * (yED - yBG) + yBG - Y));
                if (dis > 3) { continue; }
                mouse.LineNo = i + 1;
                if (mouse.Position == MOUSE.POSITION.Unknow) { mouse.Position = MOUSE.POSITION.OnLine; } break;
            }

            #endregion

            Color pix = Map == null ? Color.White : Map.GetPixel(X, Y);
            bool onBarrier = pix.R == Color.LightGray.R && pix.G == Color.LightGray.G && pix.B == Color.LightGray.B;
            if (mouse.Position == MOUSE.POSITION.Unknow && onBarrier) { mouse.Position = MOUSE.POSITION.OnBarrier; }

            return mouse;
        }
        /// <summary>
        /// 设置鼠标的详细位置信息
        /// </summary>
        /// <param name="mouse">鼠标位置信息</param>
        public static void setMousePosition(MOUSE mouse)
        {
            lock (config.MouseLock) { config.mouse = mouse; }
        }
        
        public static void MouseLeftClicked()
        {
            // 条件
            if (!Form_Start.config.CheckMap || !Form_Start.config.CheckRoute) { return; }
            if (DrawOver) { return; }
            
            // 目标点不能是障碍物
            MOUSE mouse = getMousePosition();
            if (mouse.Position == MOUSE.POSITION.OnStack && mouse.StackNo != 0)
            { PushedCursor = true; Cursor = Cursors.No; return; }
            if (mouse.Position == MOUSE.POSITION.OnBarrier)
            { PushedCursor = true; Cursor = Cursors.No; return; }

            // 加载维持距离后不能离开堆垛
            if (ShowPermitTrack && mouse.StackNo == -1)
            { PushedCursor = true; Cursor = Cursors.No; return; }
            
            // 不能经过障碍物
            CoordinatePoint.POINT lastPoint = HouseTrack.getTargetPosition(HouseTrack.TotalTrack - 1);
            Point ptBG = Point_House2Map(lastPoint);
            if (HouseTrack.TotalTrack == 0) { ptBG = new Point(mouse.X, mouse.Y); }

            double d = Math.Sqrt((ptBG.X - mouse.X) * (ptBG.X - mouse.X) + (ptBG.Y - mouse.Y) * (ptBG.Y - mouse.Y));

            double xPace = 1 / d * (mouse.X - ptBG.X);
            double yPace = 1 / d * (mouse.Y - ptBG.Y);
            int N = (int)(d / 1);
            for (int i = 1; i <= N; i++)
            {
                int X = ptBG.X + (int)(i * xPace);
                int Y = ptBG.Y + (int)(i * yPace);

                Color pix = Map.GetPixel(X, Y);
                if (pix.R != Color.LightGray.R) { continue; }
                if (pix.G != Color.LightGray.G) { continue; }
                if (pix.B != Color.LightGray.B) { continue; }

                PushedCursor = true; Cursor = Cursors.No;
                return;
            }

            // 添加路径
            HouseTrack.TRACK t = new HouseTrack.TRACK();
            t.IsLeft = HouseStack.getIsLeft(mouse.StackNo);
            t.No = mouse.StackNo;
            t.Direction = mouse.Side;
            t.Distance = mouse.Distance;
            t.Extra = null;
            if (ShowPermitTrack) { HouseTrack.fillPosition(ref t); }
            if (!ShowPermitTrack) { t.TargetPosition = Point_Map2House(new Point(mouse.X, mouse.Y)); }
            HouseTrack.addTrack(t);
        }
        public static void MouseDoubleClicked()
        {
            if (!Form_Start.config.CheckMap && !Form_Start.config.CheckRoute) { return; }
            if (!DrawOver) { return; }

            MOUSE mouse = getMousePosition();

            if (mouse.StackNo != -1 && mouse.Side == TH_AutoSearchTrack.Direction.Tuning && Form_Start.config.CheckMap)
            {
                Form_Stack.Form_Stack formStack = new Form_Stack.Form_Stack(mouse.StackNo);
                formStack.Show(); return;
            }
            if (mouse.Position == MOUSE.POSITION.OnTrack && Form_Start.config.CheckRoute)
            {
                Solution_FollowTrack.Form_Track formTrack = new Solution_FollowTrack.Form_Track(mouse.TrackNo);
                formTrack.Show(); return;
            }
            if (mouse.Position == MOUSE.POSITION.OnLine && Form_Start.config.CheckRoute)
            {
                Form_Line.Form_Line formLine = new Form_Line.Form_Line();
                formLine.Show(); return;
            }
        }
        public static void MouseMove(int X, int Y)
        {
            if (CursorInMap) { setMousePosition(getMousePosition(X,Y)); }
        }
        public static void MouseLeave()
        {
            CursorInMap = false;
        }
        public static void MouseEnter()
        {
            CursorInMap = true;
        }

        /// <summary>
        /// 从配置文件中加载参数
        /// </summary>
        public static void Load()
        {
            Initial();

            HouseLength = Configuration.getFieldValue1_DOUBLE("HouseMap.HouseLength");
            HouseWidth = Configuration.getFieldValue1_DOUBLE("HouseMap.HouseWidth");
            PixLength = Configuration.getFieldValue1_DOUBLE("HouseMap.PixLength");
            ShowPermitTrack = Configuration.getFieldValue1_BOOL("HouseMap.ShowPermitTrack");
        }
        /// <summary>
        /// 保存参数到配置文件中
        /// </summary>
        public static void Save()
        {
            Configuration.setFieldValue("HouseMap.HouseLength", HouseLength);
            Configuration.setFieldValue("HouseMap.HouseWidth", HouseWidth);
            Configuration.setFieldValue("HouseMap.PixLength", PixLength);
            Configuration.setFieldValue("HouseMap.ShowPermitTrack", ShowPermitTrack);
        }

        ////////////////////////////////////////////// private method ///////////////////////////////////////////

        private static void getFont()
        {
            int fontWidth = Math.Min(Math.Min(MapHeight, MapWidth), Math.Min(PictureBoxHeight, PictureBoxWidth));
            fontWidth /= 50; fontWidth++;
            if (fontWidth > 40) { fontWidth = 40; }
            config.font = new Font("Arial", fontWidth);
        }

        private static void drawStacks()
        {
            // 创建图片
            if (Map != null) { config.g.Dispose(); Map.Dispose(); }
            Map = new Bitmap(MapWidth, MapHeight);

            // 创建画笔，纯色填充
            config.g = Graphics.FromImage(Map);
            config.g.FillRectangle(Brushes.White, new Rectangle(0, 0, MapWidth, MapHeight));

            // 是否打开地图
            if (!Form_Start.config.CheckMap) { return; }

            // 获取垛区信息
            List<HouseStack.STACK> Stacks = HouseStack.Get();
            if (Stacks == null || Stacks.Count == 0) { return; }

            // 必要的信息
            int xBG = Length_Real2Map(Stacks[0].Position.x);
            int yBG = Length_Real2Map(Stacks[0].Position.y);
            int L = Length_Real2Map(Stacks[0].Length);
            int W = Length_Real2Map(Stacks[0].Width);

            // 门
            config.g.FillRectangle(Brushes.LightBlue, xBG, yBG, L, W);

            // 绘制垛区
            for (int i = 1; i < Stacks.Count; i++)
            {
                string istr = i.ToString(); SizeF size = config.g.MeasureString(istr, config.font);
                xBG = Length_Real2Map(Stacks[i].Position.x);
                yBG = Length_Real2Map(Stacks[i].Position.y);
                L = Length_Real2Map(Stacks[i].Length);
                W = Length_Real2Map(Stacks[i].Width);

                config.g.FillRectangle(Brushes.LightGray, xBG, yBG, L, W);

                int X = xBG + L / 2 - (int)size.Width / 2;
                int Y = yBG + W / 2 - (int)size.Height / 2;

                config.g.DrawString(istr, config.font, Brushes.Black, X, Y);
            }
        }
        private static void drawPermitTrack()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (!ShowPermitTrack) { return; }

            List<HouseStack.STACK> Stacks = HouseStack.Get();
            foreach (HouseStack.STACK s in Stacks)
            {
                int xBG = Length_Real2Map(s.Position.x);
                int yBG = Length_Real2Map(s.Position.y);

                int keepU = Length_Real2Map(s.KeepDistanceU);
                int keepD = Length_Real2Map(s.KeepDistanceD);
                int keepL = Length_Real2Map(s.KeepDistanceL);
                int keepR = Length_Real2Map(s.KeepDistanceR);

                int L = Length_Real2Map(s.Length);
                int W = Length_Real2Map(s.Width);

                // U
                config.g.DrawLine(Pens.LightBlue, xBG - keepL, yBG - keepU, xBG + L + keepR, yBG - keepU);

                // D
                config.g.DrawLine(Pens.LightBlue, xBG - keepL, yBG + W + keepD, xBG + L + keepR, yBG + W + keepD);

                // L
                config.g.DrawLine(Pens.LightBlue, xBG - keepL, yBG - keepU, xBG - keepL, yBG + W + keepD);

                // R
                config.g.DrawLine(Pens.LightBlue, xBG + L + keepR, yBG - keepU, xBG + L + keepR, yBG + W + keepD);
            }
        }
        private static void drawCurrentTrack()
        {
            if (!Form_Start.config.CheckRoute) { return; }

            // 转换为点坐标
            List<HouseTrack.TRACK> Track = HouseTrack.Get();
            List<Point> points = new List<Point>();
            for (int i = 0; i < Track.Count; i++) { points.Add(Point_House2Map(Track[i].TargetPosition)); }

            // 画线
            for (int i = 0; i < points.Count - 1; i++)
            { config.g.DrawLine(Pens.Blue, points[i], points[i + 1]); }

            // 画点
            for (int i = 1; i < points.Count - 1; i++)
            { config.g.DrawEllipse(Pens.Blue, points[i].X - 2, points[i].Y - 2, 4, 4); }

            // 起始点
            if (points.Count == 0) { return; }
            config.g.DrawEllipse(Pens.Red, points[0].X - 2, points[0].Y - 2, 4, 4);

            // 终止点
            if (points.Count == 1) { return; }
            int N = points.Count - 1;
            config.g.DrawEllipse(Pens.Black, points[N].X - 2, points[N].Y - 2, 4, 4);
        }
        private static void drawMove()
        {
            if (!Form_Start.config.CheckRoute) { return; }
            if (DrawOver) { return; }
            if (!CursorInMap) { return; }
            if (HouseTrack.TotalTrack == 0) { return; }

            MOUSE mouse = getMousePosition();
            CoordinatePoint.POINT lastPoint = HouseTrack.getTargetPosition(HouseTrack.TotalTrack - 1);

            Point ptBG = Point_House2Map(lastPoint);
            Point ptED = new Point(mouse.X, mouse.Y);

            config.g.DrawLine(Pens.Blue, ptBG, ptED);
        }
        private static void drawCursor()
        {
            if (!Form_Start.config.CheckRoute && !Form_Start.config.CheckMap) { return; }
            if (!CursorInMap) { return; }
            if (PushedCursor) { return; }

            MOUSE mouse = getMousePosition();
            if (!DrawOver && mouse.Position == MOUSE.POSITION.OnStack) { Cursor = Cursors.Default; return; }
            if (!DrawOver && mouse.Position == MOUSE.POSITION.OnBarrier) { Cursor = Cursors.Default; return; }
            if (!DrawOver && ShowPermitTrack && mouse.StackNo != 0 && mouse.Side == TH_AutoSearchTrack.Direction.Tuning)
            { Cursor = Cursors.Default; return; }
            if (!DrawOver) { Cursor = Cursors.Cross; return; }

            Cursor = Cursors.Default;
        }
        private static void drawUltraSonicData()
        {
            if (!Form_Start.config.CheckControlPort) { return; }

            int[] SonicData = TH_SendCommand.getUltraSonicData();
            if (SonicData == null) { SonicData = new int[8]; }

            int picLength = Math.Min(MapHeight, MapWidth) / 2;
            int width = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * picLength);
            Font font = new Font("Arial", width / 8 + 1);

            int L = (int)(picLength + Hardware_PlatForm.AxisSideL / Form_Start.config.urgRange * picLength);
            int U = (int)(picLength - Hardware_PlatForm.AxisSideU / Form_Start.config.urgRange * picLength);
            int R = (int)(picLength + Hardware_PlatForm.AxisSideR / Form_Start.config.urgRange * picLength);
            int D = (int)(picLength - Hardware_PlatForm.AxisSideD / Form_Start.config.urgRange * picLength);

            string str = SonicData[(int)TH_SendCommand.Sonic.Head_L_X].ToString();
            SizeF strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, L - strSize.Width, U);


            str = SonicData[(int)TH_SendCommand.Sonic.Head_L_Y].ToString();
            strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, L, U - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Head_R_X].ToString();
            //strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, R, U);

            str = SonicData[(int)TH_SendCommand.Sonic.Head_R_Y].ToString();
            strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, R - strSize.Width, U - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_L_X].ToString();
            strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, L - strSize.Width, D - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_L_Y].ToString();
            //strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, L, D);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_R_X].ToString();
            strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, R, D - strSize.Height);

            str = SonicData[(int)TH_SendCommand.Sonic.Tail_R_Y].ToString();
            strSize = config.g.MeasureString(str, font);
            config.g.DrawString(str, font, Brushes.Black, R - strSize.Width, D);
        }
        private static void drawUrgData()
        {
            if (!Form_Start.config.CheckUrgPort) { return; }

            // 画布一半宽度
            int picLength = Math.Min(MapHeight, MapWidth) / 2;

            // 画出车的位置
            int xCar = (int)(picLength + Hardware_PlatForm.AxisSideL / Form_Start.config.urgRange * picLength);
            int yCar = (int)(picLength - Hardware_PlatForm.AxisSideU / Form_Start.config.urgRange * picLength);
            int CarL = (int)(Hardware_PlatForm.Length / Form_Start.config.urgRange * picLength);
            int CarW = (int)(Hardware_PlatForm.Width / Form_Start.config.urgRange * picLength);

            config.g.FillRectangle(Brushes.LightGray, xCar, yCar, CarW, CarL);
            config.g.FillEllipse(Brushes.Red, picLength - 4, picLength - 4, 8, 8); // 原点

            // 周围环境信息
            List<CoordinatePoint.POINT> points = TH_MeasureSurrounding.getSurroundingD(0, Form_Start.config.urgRange);
            for (int i = 0; i < points.Count; i++)
            {
                int x = (int)(picLength * points[i].x / Form_Start.config.urgRange + picLength);
                int y = (int)(picLength - picLength * points[i].y / Form_Start.config.urgRange);

                config.g.FillEllipse(Brushes.Black, x, y, 2, 2);
            }
        }
        private static void drawCarPosition()
        {
            if (!Form_Start.config.CheckLocatePort) { return; }
            
            CoordinatePoint.POINT pos = TH_MeasurePosition.getPosition();
            
            int X = Length_Real2Map(pos.x);
            int Y = Length_Real2Map(pos.y);
            
            config.g.FillEllipse(Brushes.Red, X - 4, Y - 4, 8, 8);

            string pstr = "( " + ((int)pos.x).ToString() + ", " + ((int)pos.y).ToString() + ", " + ((int)pos.aCar).ToString() + " )";
            Font font = new Font("Arial", 12);
            SizeF size = config.g.MeasureString(pstr, font);

            bool U = Y < MapHeight / 2;
            bool L = X < MapWidth / 2;

            if (U && L) { config.g.DrawString(pstr, font, Brushes.Black, X, Y); }
            if (U && !L) { config.g.DrawString(pstr, font, Brushes.Black, X - (int)size.Width, Y); }
            if (!U && L) { config.g.DrawString(pstr, font, Brushes.Black, X, Y - (int)size.Height); }
            if (!U && !L) { config.g.DrawString(pstr, font, Brushes.Black, X - (int)size.Width, Y - (int)size.Height); }
        }
    }
}
