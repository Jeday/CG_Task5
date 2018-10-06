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
        List<PointF> pred_point;

        public Form1()
        {
            InitializeComponent();
            point_list = new List<PointF>();
            pred_point = new List<PointF>();
        }


        /// <summary>
        ///  Считает, пересекаются ли отрезки (p1,p2) и (p3,p4). 
        ///  
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        /// <param name="p3"></param>
        /// <param name="p4"></param>
        /// <returns></returns>
        private bool FindIntersection(PointF p1, PointF p2, PointF p3, PointF p4, out PointF intersection)
        {
            // Get the segments' parameters.
            float dx12 = p2.X - p1.X;
            float dy12 = p2.Y - p1.Y;
            float dx34 = p4.X - p3.X;
            float dy34 = p4.Y - p3.Y;

            // Solve for t1 and t2
            float denominator = (dy12 * dx34 - dx12 * dy34);

            float t1 = ((p1.X - p3.X) * dy34 + (p3.Y - p1.Y) * dx34) / denominator;
            if (float.IsInfinity(t1))
            {
                // The lines are parallel (or close enough to it).
                intersection = new PointF(float.NaN, float.NaN);
                return false;
            }
            
            float t2 = ((p3.X - p1.X) * dy12 + (p1.Y - p3.Y) * dx12) / -denominator;

            // Find the point of intersection.
            intersection = new PointF(p1.X + dx12 * t1, p1.Y + dy12 * t1);

            // The segments intersect if t1 and t2 are between 0 and 1.
            return  ((t1 >= 0) && (t1 <= 1) &&  (t2 >= 0) && (t2 <= 1));
        }


        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            g = pictureBox1.CreateGraphics();
            comboBox1.Text = "Line";
        }

        private void trackBar2_Scroll(object sender, EventArgs e)
        {

        }

        private void trackBar3_Scroll(object sender, EventArgs e)
        {

        }

        void point_in_polygon(PointF n)
        {
            if (point_list.Contains(n))
                g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle((int)n.X - 1, (int)n.Y - 1, 3, 3));
            else
            {
                int cnt_intersec = 0;
                PointF intersec;
                PointF finish = new PointF(0, 0);
                for(var i = 0; i < point_list.Count - 1; ++i)
                {
                    if (FindIntersection(n, finish, point_list[i], point_list[i + 1], out intersec) && intersec != point_list[i])
                        ++cnt_intersec;
                }

                if (FindIntersection(n, finish, point_list[point_list.Count - 1], point_list[0], out intersec) && intersec != point_list[point_list.Count - 1])
                    ++cnt_intersec;

                if (cnt_intersec % 2 != 0)     // принадлежит многоугольнику
                    g.DrawEllipse(new Pen(Color.Blue), new Rectangle((int)n.X - 1, (int)n.Y - 1, 3, 3));
                else                           // не принадлежит
                    g.DrawEllipse(new Pen(Color.Green), new Rectangle((int)n.X - 1, (int)n.Y - 1, 3, 3));
            }
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {        
            if (checkBox1.Checked && point_list.Count == 2)
            {
                float pos = (point_list[1].X - point_list[0].X) * (e.Y - point_list[0].Y) - (point_list[1].Y - point_list[0].Y) * (e.X - point_list[0].X);
                if (pos < 0.0)            // слева
                    g.DrawEllipse(new Pen(Color.Blue), new Rectangle(e.X - 1, e.Y - 1, 3, 3));
                else if (pos > 0.0)       // справа
                    g.DrawEllipse(new Pen(Color.Green), new Rectangle(e.X - 1, e.Y - 1, 3, 3));
                else                     // на прямой
                    g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle(e.X - 1, e.Y - 1, 3, 3));
            }
            else if (checkBox1.Checked && point_list.Count > 2)
            {
                point_in_polygon(new Point(e.X, e.Y));
            }
            else
            {
                if (done_placing)
                {
                    point_list.Clear();
                }
                done_placing = false;
                point_list.Add(new PointF(e.X, e.Y));
                g.DrawEllipse(new Pen(Color.Red), new Rectangle(e.X - 1, e.Y - 1, 3, 3));
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked == false)
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
            
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
        }
    }
}
