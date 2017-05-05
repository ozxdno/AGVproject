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
                OnTrack
            }
        }

        ////////////////////////////////////////////// public attribute ///////////////////////////////////////////

        private static CONFIG config;
        private struct CONFIG
        {
            public object MouseLock;
            public object InformLock;
            public object MapLock;

            public Graphics g;
            public MOUSE mouse;
            public Font font;
            public string inform;
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
            DrawOver = false;
            PushedCursor = false;
            CursorInMap = false;
            
            config.MouseLock = new object();
            config.InformLock = new object();
            config.MapLock = new object();
            config.g = null;
            config.mouse = new MOUSE();
            config.font = new Font("Arial", 10);
            config.inform = "";
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
                
                drawUrgData();
                drawUltraSonicData();
                drawCarPosition();
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
        /// 在图片中显示紧急通知
        /// </summary>
        /// <param name="inform">通知信息</param>
        public static void setInform(string inform)
        {
            lock (config.InformLock) { config.inform = inform; }
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
                mouse.X = config.mouse.X;
                mouse.Position = config.mouse.Position;
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

            MOUSE mouse = new MOUSE();
            foreach (HouseStack.STACK stack in Stacks)
            {

            }

            return config.mouse;
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

        }
        public static void MouseDoubleClicked()
        {
            
        }
        public static void MouseRightClicked_SaveMap()
        {

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

                config.g.FillRectangle(Brushes.LightBlue, xBG, yBG, L, W);

                int X = xBG + L / 2 - (int)size.Width / 2;
                int Y = yBG + W / 2 - (int)size.Height / 2;

                config.g.DrawString(istr, config.font, Brushes.Black, X, Y);
            }
        }
        private static void drawPermitTrack()
        {

        }
        private static void drawCurrentTrack()
        {

        }
        private static void drawMove()
        {

        }
        private static void drawCursor()
        {

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
        private static void drawInform()
        {

        }
    }
}
