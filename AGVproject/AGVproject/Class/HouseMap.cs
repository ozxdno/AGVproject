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
        /// 显示地图图片
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
        /// 挂起地图图片（不允许操作地图图片）
        /// </summary>
        public static bool HoldMap;

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
            public bool HoldMouse;
            public bool HoldInform;

            public bool NoOperate;
            public bool DrawOver;
            public bool DrawMove;
            public bool PushedCursor;
            public bool CursorInMap;

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
            HoldMap = false;

            config.HoldMouse = false;
            config.HoldInform = false;
            config.NoOperate = false;
            config.DrawOver = false;
            config.DrawMove = true;
            config.PushedCursor = false;
            config.CursorInMap = false;

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
            while (HoldMap) ;
            HoldMap = true;


            HoldMap = false;
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
        /// 在图片中显示紧急通知
        /// </summary>
        /// <param name="inform">通知信息</param>
        public static void setInform(string inform)
        {
            while (config.HoldInform) ;
            config.HoldInform = true;
            config.inform = inform;
            config.HoldInform = false;
        }

        /// <summary>
        /// 获取鼠标详细位置信息
        /// </summary>
        /// <returns></returns>
        public static MOUSE getMousePosition()
        {
            while (config.HoldMouse) ;
            config.HoldMouse = true;

            MOUSE mouse = new MOUSE();
            mouse.X = config.mouse.X;
            mouse.X = config.mouse.X;
            mouse.Position = config.mouse.Position;
            mouse.StackNo = config.mouse.StackNo;
            mouse.LineNo = config.mouse.LineNo;
            mouse.TrackNo = config.mouse.TrackNo;

            config.HoldMouse = false;
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
            MOUSE mouse = new MOUSE();



            return config.mouse;
        }
        /// <summary>
        /// 设置鼠标的详细位置信息
        /// </summary>
        /// <param name="mouse">鼠标位置信息</param>
        public static void setMousePosition(MOUSE mouse)
        {
            while (config.HoldMouse) ;
            config.HoldMouse = true;
            config.mouse = mouse;
            config.HoldMouse = false;
        }
        
        public static void MouseLeftClicked()
        {

        }
        public static void MouseDoubleClicked()
        {
            if (!config.CursorInMap) { return; }
        }
        public static void MouseRightClicked_SaveMap()
        {

        }
        public static void MouseMove(int X, int Y)
        {
            if (config.CursorInMap) { setMousePosition(getMousePosition(X,Y)); }
        }
        public static void MouseLeave()
        {
            config.CursorInMap = false;
        }
        public static void MouseEnter()
        {
            config.CursorInMap = true;
        }

        ////////////////////////////////////////////// private method ///////////////////////////////////////////

        private static void drawStacks()
        {

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

        }
        private static void drawUrgData()
        {

        }
        private static void drawCarPosition()
        {

        }
        private static void drawInform()
        {

        }
    }
}
