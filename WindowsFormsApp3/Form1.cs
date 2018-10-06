using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp3
{
    public partial class Form1 : Form
    {
        Graphics g;
        List<PointF> point_list;
        bool done_placing = false;

        public Form1()
        {
            InitializeComponent();
            point_list = new List<PointF>();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            g = pictureBox1.CreateGraphics();
            comboBox1.Text = "Line";
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {

        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (done_placing)
                point_list.Clear();
            done_placing = false;
            point_list.Add(new PointF(e.X, e.Y));
            g.DrawEllipse(new Pen(Color.Red), new Rectangle(e.X - 1, e.Y - 1, 3, 3));
        }

        private void button4_Click(object sender, EventArgs e)
        {
            done_placing = true;
            g.Clear(Color.White);
            if (comboBox1.Text == "Line")
            {
                if (point_list.Count > 2) 
                    point_list = point_list.Skip(point_list.Count - 2).ToList(); // последние 2
                g.DrawLines(new Pen(Color.Black), point_list.ToArray());
            }
             else if (comboBox1.Text == "Polygon")
                g.DrawLines(new Pen(Color.Black), point_list.Concat(new List<PointF> { point_list.First() }).ToArray());

            
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {

          
        }
    }
}
