using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Laser
{
    public class DrawRadarImg
    {
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// Attribute（属性域）
        ///////////////////////////////////////////////////////////////////////////////////////////
        // 图像宽度
        private int width;
        // 图像高度
        private int height;
        // 显示比例
        private double rate = 0.05;
        private double rate_B_X = 1.11;
        private double rate_B_Y = 1.33;
        // 像素点大小
        private static int ps = 4;

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// Attribute Modify（属性域修改）
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 图像宽度
        /// </summary>
        public int Width
        {
            get { return width; }
            set { width = value; }
        }
        /// <summary>
        /// 图像高度
        /// </summary>
        public int Height
        {
            get { return height; }
            set { height = value; }
        }
        /// <summary>
        /// 长度比例
        /// </summary>
        public double Rate
        {
            get { return rate; }
            set { rate = value; }
        }
        /// <summary>
        /// 一个测量点大小
        /// </summary>
        public int PS
        {
            get { return ps; }
            set { ps = value; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////
        /// Method（方法域）
        ///////////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 根据距离和强度信息画图
        /// </summary>
        /// <param name="dist">距离信息</param>
        /// <param name="back">强度信息</param>
        /// <returns></returns>
        public Image drawImg(int[] dist, int[] back)
        {
            Bitmap img = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(img);
            for (int i = 0; i < 360; ++i)
            {
                // 计算当前角度、X、Y坐标（偏差90度，与设定相关）
                double ang = ((i + 90) / 180.0) * Math.PI;
                double x = Math.Cos(ang) * dist[i] * rate;
                double y = Math.Sin(ang) * dist[i] * rate;
                // 调整强度显示的颜色
                //Brush brush = (back[i] > 300) ? (Brushes.Red) :
                //    (back[i] > 200) ? (Brushes.Green) :
                //    (back[i] > 100) ? (Brushes.Blue) : Brushes.Purple;
                Brush brush = (back[i] > 300) ? (Brushes.Yellow) : 
                   (back[i] > 150) ? (Brushes.Blue) :
                   (back[i] > 45) ? (Brushes.Green) :
                   (back[i] > 12) ? (Brushes.Red) : Brushes.Purple;
                // 画点
                g.FillEllipse(brush, (int)(x + 200 - ps / 2), (int)(200 - (y - ps / 2)), ps, ps);
                
            }
            return img;
        }
        public Image drawImg_B(int[] dist, int[] back)
        {
            Bitmap img = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(img);
            for (int i = 0; i < 360; ++i)
            {
                // 计算当前角度、X、Y坐标（偏差90度，与设定相关）
                if (back[i] > 350)
                {
                    back[i] = 350;
                }
                double ang = ((i + 90) / 180.0) * Math.PI;
                double x =  i * rate_B_X;
                double y =  back[i] * rate_B_Y;
                // 调整强度显示的颜色
                //Brush brush = (back[i] > 300) ? (Brushes.Red) :
                //    (back[i] > 200) ? (Brushes.Green) :
                //    (back[i] > 100) ? (Brushes.Blue) : Brushes.Purple;
                Brush brush = (back[i] > 300) ? (Brushes.Yellow) :
                   (back[i] > 150) ? (Brushes.Blue) :
                   (back[i] > 45) ? (Brushes.Green) :
                   (back[i] > 12) ? (Brushes.Red) : Brushes.Purple;
                // 画点

                //g.FillEllipse(brush, (int)(x + 200 - ps / 2), (int)(150 - (y - ps / 2)), ps, ps);
                g.FillEllipse(brush, (int)(x), (int)(390 - y), ps, ps);
            }
            return img;
        }
    }

    public class PreRadar//特征提取类
    {
        public int breakCnt = 0;//区域分割点数量
        //private int[] dist =new int[360];//激光雷达返回的数据
        private int Dmax = 600;//区域分割判断阈值
        private int dis;//两点距离
        public int[] breakflag = new int[380];

        //private int[] P = new int[360];
        //private int[] Q = new int[360];

        private int[] RhoCopy = new int[400];
        private int[] ThetaCopy = new int[400];
        int LineCount = 0;//线段分段计数

        public double[] a = new double[50];//拟合直线的斜率
        public double[] b = new double[50];//拟合直线的截距

        public Object readLock = new object();
        public int[] Cor = new int[50];
        int cor = 0;
        public int[] CorKMP
        {
            get
            {
                lock(readLock)
                {
                    return Cor;
                }
            }
            set
            {
                lock(readLock)
                {
                    Cor = value;
                }
            }
        }

        public int BreakRadar(int[] dist)//区域分割，需要原始的距离数据
        {
            for (int s = 0; s < 380;s++ )
            {
                breakflag[s] = 0;
            }
                for (int i = 0; i < 359; i++)
                {
                    dis = Math.Abs(dist[i] - dist[i + 1]);
                    if (dis > Dmax)
                    {
                        breakflag[i] = 1;
                        breakCnt++;
                    }
                }
            return breakCnt;
        }

/*        public void FindArea(int[] dist)
        {
            int[] X=new int[360];
            int[] Y=new int[360];
            for (int i=0;i<breakCnt;i++)
                while(breakflag[i]!=1)
                {
                    X[i]=dist[i];
                    Y[i]=i;
                }
        }*/

        public int FindFolding(int[] A, int[] B, int n, float Eps)//在区域内寻找拐点,需要原始的距离，角度，区域内点的数量，精度要求
        {
            double[] X = new double[360];//极坐标转换为直角坐标
            double[] Y = new double[360];
            for (int i=0;i<360;i++)
            {
                X[i] = A[i] * Math.Cos(B[i]);
                Y[i] = A[i] * Math.Sin(B[i]);
            }

            double dis = Math.Sqrt((double)(((X[0] - X[n - 1]) * (X[0] - X[n - 1])) +
                     ((Y[0] - Y[n - 1]) * (Y[0] - Y[n - 1]))));
            double cosTheta = (X[n - 1] - X[0]) / dis;
            double sinTheta = -(Y[n - 1] - Y[0]) / dis;
            double MaxDis = 0;
            int MaxDisInd = -1;
            double dbDis;//计算当前距离
            for (int i = 1; i < n - 1; i++)
            {
                // 进行坐标旋转，求旋转后的点到x轴的距离
                dbDis = Math.Abs((Y[i] - Y[0]) * cosTheta + (X[i] - X[0]) * sinTheta);
                if (dbDis > MaxDis)
                {
                    MaxDis = dbDis;
                    MaxDisInd = B[i];
                }
            }
            if (MaxDis > Eps)
            {
                Cor[cor] = MaxDisInd;//标记找到的所有拐点，为KMP算法做准备
                cor++;
                return MaxDisInd;
            }
            return 0;
        }

        public void BreakPolyLine(int[] dist)//线段分割，仅需要原始距离信息，区域分割数量为类变量可直接引用
        {
            for (int i=0;i<50;i++)
            {
                Cor[i] = 0;
            }
            int[] X=new int[360];
            int[] Y=new int[360];
            int j = 0;//为特征提取数据预处理做序号标记
            int m = 0;//查看当前点在dist数组中的序号
            int PointCount = 0;//区域内点计数

            for (int i=0;i<breakCnt;i++)
            {
                int n = 0;//对每个区域内的点进行计数用
                while(breakflag[m]!=1)
                {
                    X[n]=dist[m];
                    Y[n]=m;
                    m++;
                    n++;
                    PointCount++;
                }

                //int rho = 0;
                //int theta = 0;

                
                //int LineCount = 0;//线段分段计数
                int N = 0;//线段分割点标记               
                if (PointCount>50)
                    {
                        N = FindFolding(X,Y,PointCount,100);
                        if (N == 0)
                        {
                            LineCount++;
                            for (int j1=0;j1<PointCount;j1++)
                            {
                                RhoCopy[j] = X[j1];
                                ThetaCopy[j] = Y[j1];
                                j++;
                            }
                            RhoCopy[j]=-1;
                            ThetaCopy[j]=500;
                            j++;
                        }
                        else
                        {
                            LineCount+=2;
                            for (int j1=0;j1<N-Y[0];j1++)
                            {
                                RhoCopy[j] = X[j1];
                                ThetaCopy[j] = Y[j1];
                                j++;
                            }
                            RhoCopy[j]=-1;
                            ThetaCopy[j]=500;
                            j++;
                            for (int k=N-Y[0]+1;k<PointCount;k++)
                            {
                                RhoCopy[j] = X[k];
                                ThetaCopy[j] = Y[k];
                                j++;
                            }
                            RhoCopy[j]=-1;
                            ThetaCopy[j]=500;
                            j++;
                        }
                        PointCount = 0;
                        continue;
                    }
                PointCount = 0;
                m++;
                for (int l=0;l<360;l++)
                {
                    X[l]=0;
                    Y[l]=0;
                }
            }
            //return LineCount;
        }

        public void FitLine(int[] A,int[] B)//最小二乘拟合,需要刚才处理过后的RhoCopy和ThetaCopy数组
        {
            double[] X = new double[380];//极坐标转换为直角坐标
            double[] Y = new double[380];
            for (int i = 0; i < 380; i++)
            {
                if (A[i]!=-1 && B[i]!=500)
                {
                    X[i] = A[i] * Math.Cos(B[i]);
                    Y[i] = A[i] * Math.Sin(B[i]);
                } 
                else
                {
                    X[i] = A[i];
                    Y[i] = B[i];
                }
            }

            //double a, b;
            double t1=0, t2=0, t3=0, t4=0;
            for (int i1 = 0; i1 < LineCount;i1++)
            {
                for (int i = 0; i < X.Length; i++)
                {
                    t1 += X[i] * X[i];
                    t2 += X[i];
                    t3 += X[i] * Y[i];
                    t4 += Y[i];
                }
                a[i1] = (t3 * X.Length - t2 * t4) / (t1 * X.Length - t2 * t2);
                b[i1] = (t1 * t4 - t2 * t3) / (t1 * X.Length - t2 * t2);
            }               
        }

        public void Preprocess(int[] dist)
        {
            BreakRadar(dist);
            BreakPolyLine(dist);
            FitLine(RhoCopy, ThetaCopy);
        }

        public Image drawImg(int[] dist)
        {
            Bitmap g = new Bitmap(400, 400);
            Graphics img = Graphics.FromImage(g);
            BreakRadar(dist);
            //BreakPolyLine(dist);
            //FitLine(RhoCopy, ThetaCopy);
            img.DrawLine(Pens.Blue, new Point(0, 0), new Point(300, 300));
            return g;
        }

        //需要写一个求两条直线交点的函数

    }

    public class KMP
    {
        public PreRadar preKMP;
        public Radar radarKMP;
        public int[] flag=new int[20];

        public int[] Corner = new int[50];
        public int corner = 0;

        private double dist1=50000;
        //private int dist2=50000;
        private double Mindist = 50000;
        private int threshold = 100;

        private int ps = 10;
        private double rate = 0.5;

        private Object readLock=new object(); 
        private int[] Cornerpoint = new int[50];

        public int[] cx = new int[50];
        public int[] cy = new int[50];

        public int[] cornerpoint
        {
            get
            {
                lock (readLock)
                {
                    return Cornerpoint;
                }
            }
            set
            {
                lock(readLock)
                {
                    Cornerpoint = value;
                }
            }
        }

        public int Findcorner(int[] A)//记录所有角点并返回个数，需要特征提取类中的Corner数组
        {
            for (int i = 0; i < 50;i++ )
            {
                Corner[i] = A[i];
                if (Corner[i] != 0)
                    corner++;
            }
            return corner;
        }

        public void FindCar(int a,int[] dist,int[] cor)//对所有角点进行模式匹配寻找车辆轮廓，需要角点个数，原始距离以及角点序号数组
        {
            int Prepoint = 0;
            int corner = 0;

            double[] X = new double[360];//极坐标转换为直角坐标
            double[] Y = new double[360];

            for (int i = 0; i < 50;i++ )
            {
                Cornerpoint[i] = 0;
            }

            for (int i = 0; i < 360; i++)
            {
                X[i] = dist[i] * Math.Cos(i);
                Y[i] = dist[i] * Math.Sin(i);
            }

            for (int i = 0; i < a; i++)
            {
                Prepoint = cor[i];
                for (int j=0;j<360;j++)
                {
                    dist1 = Math.Sqrt((X[Prepoint] - X[i]) * (X[Prepoint] - X[i]) + (Y[Prepoint] - Y[i]) * 
                        (Y[Prepoint] - Y[i]));
                    if (dist1 < Mindist)
                        Mindist = dist1;
                }
                if (Mindist<threshold)
                {
                    Cornerpoint[corner] = Prepoint;
                    corner++;
                }
            }
        }

        public Image DrawCar(int[] corner,int[] dist)//将车辆位置信息标记在窗口中，需要找到的角点数组以及原始距离数组
        {
            double[] X = new double[360];//极坐标转换为直角坐标
            double[] Y = new double[360];
            int x, y;
            Brush brush = Brushes.Red;

            for (int i = 0; i < 360; i++)
            {
                double ang = ((i + 90) / 180.0) * Math.PI;
                X[i] = dist[i] * Math.Cos(ang)*rate;
                Y[i] = dist[i] * Math.Sin(ang)*rate;
            }

            Bitmap img = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(img);

            int K = 0;
            //preKMP.Preprocess(dist);
            //K = Findcorner(preKMP.CorKMP);
            //FindCar(K,dist,Corner);
            
            for (int i = 0; i < corner.Length; ++i)
            {
                if (corner[i]!=0)
                {
                    x = (int)X[corner[i]];
                    y = (int)Y[corner[i]]; 
                    // 描绘车辆所在区域
                    g.FillEllipse(brush, (int)(x + 200 - ps / 2), (int)(140 - (y - ps / 2)), ps, ps);
                }
                
            }
            return img;
        }

        public void InitCorner(int[] corner, int[] dist)//标记所有角点坐标
        {
            double[] X = new double[360];//极坐标转换为直角坐标
            double[] Y = new double[360];
            //int x, y;

            for (int i = 0; i < 360; i++)
            {
                double ang = ((i + 90) / 180.0) * Math.PI;
                X[i] = dist[i] * Math.Cos(ang) * rate;
                Y[i] = dist[i] * Math.Sin(ang) * rate;
            }

            for (int i = 0; i < corner.Length; ++i)
            {
                if (corner[i] != 0)
                {
                    cx[i] = (int)X[corner[i]];
                    cy[i] = (int)Y[corner[i]];                   
                }
            }
        }

        public void InitFlag(int[] cx,int[] cy)//标记车位信息
        {
            int x = 100;
            int y = 17;
            int cflag=0;
            
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 10; i++)
                {
                    cflag=10*j+i+1;
                    for (int k = 0; k < 50;k++ )
                    {
                        if (cx[k] > x && cx[k] < (x + 50) && cy[k] > y && cy[k] < y + 25)
                            flag[cflag] = 1;
                        else
                            flag[cflag] = 0;                            
                    }
                        y += 38;
                }
                x += 150;
                y = 17;
            }


        }
    }

    public class ShowResult
    {
        private int count=20;
        KMP k = new KMP();

        //private int[] flag=new int[20];

        public Image DrawPark()
        {
            Bitmap img = new Bitmap(400, 400);          
            Graphics g = Graphics.FromImage(img);    

            int x = 100;
            int y = 17;

            for (int j = 0; j < 2;j++ )
            {
                for (int i = 0; i < 10; i++)
                {
                    g.DrawRectangle(Pens.Blue, x, y, 50, 25);
                    //x += 60;
                    y += 38;
                }
                x += 150;
                y = 17;
            }              
            return img;

        }

        public Image FillPark()
        {
            Bitmap img = new Bitmap(400, 400);
            Graphics g = Graphics.FromImage(img);
            Brush bush1 = new SolidBrush(Color.Red);
            Brush bush2 = new SolidBrush(Color.Green);
/*
            for (int i = 0; i < 500; i++)
                for (int j = 0; j < 500; j++)
                    for (int k = 0; k < 500; k++) ;
*/
            int x = 100;
            int y = 17;

            for (int j = 0; j < count; j++)
            {         
                if (k.flag[j] == 0)                 
                    g.FillRectangle(bush2, x, y, 50, 25);
                else
                    g.FillRectangle(bush1, x, y, 50, 25);                    
                y += 38;
                    //x += 60;
                    
/*                    for (int m = 0; m < 500; m++)
                        for (int n = 0; n < 500; n++)
                            for (int p = 0; p < 500; p++) ;
*/                                   
                x += 150;
                y = 17;
            }           
            return img;
        }
    }
}