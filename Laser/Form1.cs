using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Laser {
    public partial class Form1 : Form 
    {

        // 激光雷达对象
        private Radar radar;
        private DrawRadarImg dImg = new DrawRadarImg();//数据获取类对象实例化
        private DrawRadarImg bImg = new DrawRadarImg();//数据预处理类对象实例化
        //private KMP car = new KMP();//模式匹配类对象实例化
        private ShowResult result = new ShowResult();//停车场示意图实例化
        private ShowResult fill = new ShowResult();//填充停车场车位识别信息
        //数据库操作语句
        string insertMysql = "insert into sys values() "
        public Form1() {
            InitializeComponent();

            // 绑定更新界面方法
            updateBoardDel += new updateBoard(this.updateBoardMethod);
            //绑定强度图更新页面方法
            updateBoardDe2 += new updateBoard(this.updateBoardMethod_B);
        }

        /// <summary>
        /// 打开关闭串口方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e) {
            radar = Radar.getInstance(this.textBox1.Text.Trim());
            Radar.afterProcessDel = new Radar.afterProcess(this.processData);
            MysqlConnection mysqlconnection = new MysqlConnection();
            //Radar.afterProcessDel += new Radar.afterProcess(this.showData);//数据处理后显示数据
            if (!radar.IsOpen) {
                radar.openPort();
                this.button2.Text = "closePort";
                this.textBox1.Enabled = false;
                try
                {
                    mysqlconnection.MysqlCommand()
                }
                catch (Exception)
                {

                    throw;
                }
                
            }
            else {
                radar.closePort();
                this.button2.Text = "openPort";
            }
        }

        /// <summary>
        /// 处理完数据执行画图方法
        /// </summary>
        public void processData() {
            Image img = dImg.drawImg(radar.Dist, radar.Back);
            //Image img = result.DrawPark();
            //Image img1 = fill.FillPark();
            //Image img1 = car.DrawCar(car.cornerpoint,radar.Dist);
            Image img1 = bImg.drawImg(radar.Dist,radar.Back);
            Image img_B = bImg.drawImg_B(radar.Dist, radar.Back);
            
            if (updateBoardDel != null) {
                this.Invoke(updateBoardDel,img,img1);
                
                //this.Invoke(updateBoardDel,img1);
            }
            if (updateBoardDe2 != null)
            {
              
                this.Invoke(updateBoardDe2, img_B, img_B);
                //this.Invoke(updateBoardDel,img1);
            }
        }

        /// <summary>
        /// 更新窗口委托
        /// </summary>
        /// <param name="img"></param>
        public delegate void updateBoard(Image img,Image img1);
        public updateBoard updateBoardDel;
        public updateBoard updateBoardDe2;//强度图委托

        public void updateBoardMethod(Image img,Image img1) {
            this.pictureBox1.Image = img1;
            this.pictureBox1.BackgroundImage = img;
            //this.pictureBox1.Image = img1;       

            this.textSpeed.Text = radar.Speed.ToString();
            this.textBox2.Text  = radar.Distant.ToString();
        }
        //强度图 更新
        public void updateBoardMethod_B(Image img, Image img1)
        {
            this.pictureBox2.Image = img1;
            //this.pictureBox2.BackgroundImage = img;
            //this.pictureBox1.Image = img1;
        
            //this.textSpeed.Text = radar.Speed.ToString();
            //this.textBox2.Text = radar.Distant.ToString();
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
