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
        List<PointF> two_points;
        float[,] transofmed_points;
        float[,] orignal_points;
        PointF center;

        public Form1()
        {
            InitializeComponent();
            point_list = new List<PointF>();
            two_points = new List<PointF>();
           
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

        private float[,] multiply_matrix(float [,] m1, float [,] m2)
        {
            float[,] res = new float[m1.GetLength(0), m2.GetLength(1)];
            for (int i = 0; i < m1.GetLength(0); i++)
            {
                for (int j = 0; j < m2.GetLength(1); j++)
                {
                    for (int k = 0; k < m2.GetLength(0); k++)
                    {
                        res[i, j] += m1[i, k] * m2[k, j];
                    }
                }
            }
            return res;
        }

        private PointF get_center()
        {
            float x = 0;
            float y = 0;
            int cnt = 0;
            for (int i = 0; i < transofmed_points.GetLength(0); ++i)
            {
                x += transofmed_points[i, 0];
                y += transofmed_points[i, 1];
                cnt += 1;
            }
            return new PointF(x/cnt, y/cnt);
        }






        private void offset(float tX, float tY)
        {
            float[,] transferalMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { tX, tY, 1 } };

            transofmed_points = multiply_matrix(transofmed_points, transferalMatrix);
           
            
           
        }

        private void ValueChanged(object sender, EventArgs e)
        {
            apply_transforms();
        }

        private void apply_transforms() {
            int t = trackBar3.Value; // angle
            float tx = trackBar1.Value; // offset x;
            float ty = trackBar5.Value; // offset y
            float scale_x = (float)((trackBar2.Value - 6)/2.5); // scale x
            float scale_y = (float)((trackBar2.Value - 6)/2.5); // scale y


            float[,] transferalMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { tx, ty, 1 } };
            float[,] transferalToXYMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { -center.X, -center.Y, 1 } };
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(t * Math.PI / 180), (float)Math.Sin(t * Math.PI / 180), 0 }, { (float)-(Math.Sin(t * Math.PI / 180)), (float)Math.Cos(t * Math.PI / 180), 0 }, { 0, 0, 1 } };
            float[,] transferalFromXYMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { center.X, center.Y, 1 } };
            float[,] scaleMatrix = new float[,] { { scale_x, 0, 0 }, { 0, scale_y, 0 }, { 0,0, 1 } };


            transofmed_points = multiply_matrix(orignal_points, transferalMatrix);
            transofmed_points = multiply_matrix(transofmed_points, transferalToXYMatrix);
            transofmed_points = multiply_matrix(transofmed_points, rotationMatrix);
            transofmed_points = multiply_matrix(transofmed_points, scaleMatrix);
            transofmed_points = multiply_matrix(transofmed_points, transferalFromXYMatrix);

            g.Clear(Color.White);
            g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle((int)(center.X - 1.5), (int)(center.Y - 1.5), 3, 3));
            for (var i = 0; i < point_list.Count - 1; i++)
            {
                g.DrawLine(new Pen(Color.Black), transofmed_points[i, 0], transofmed_points[i, 1], transofmed_points[i+1, 0], transofmed_points[i+1, 1]);
            }
            if (comboBox1.Text == "Polygon")
                g.DrawLine(new Pen(Color.Black), transofmed_points[0, 0], transofmed_points[0, 1], transofmed_points[transofmed_points.GetLength(0) - 1, 0], transofmed_points[transofmed_points.GetLength(0) - 1, 1]);

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

        private void point_of_intersection()
        {
            if (FindIntersection(two_points[0], two_points[1], point_list[0], point_list[1], out PointF intersec))
                g.DrawEllipse(new Pen(Color.Fuchsia, 4), new Rectangle((int)intersec.X - 1, (int)intersec.Y - 1, 4, 4));
            two_points.Clear();
        }

        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (checkBox2.Checked) {
                center = e.Location;
                g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle((int)(center.X - 1.5), (int)(center.Y - 1.5), 3, 3));
            }
            else if (checkBox1.Checked && point_list.Count == 2)
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
            else if (checkBox3.Checked && point_list.Count == 2)
            {
                if (two_points.Count < 2)
                {
                    two_points.Add(new PointF(e.X, e.Y));
                    g.DrawEllipse(new Pen(Color.Red), new Rectangle(e.X - 1, e.Y - 1, 3, 3));
                }
                if (two_points.Count == 2)
                {
                    g.DrawLines(new Pen(Color.Black), two_points.ToArray());
                    point_of_intersection();
                }
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

            trackBar1.Value = 0;
            trackBar5.Value = 0;
            trackBar3.Value = 0;
            trackBar2.Value = 5;

            transofmed_points = new float[point_list.Count, 3];   // матрица начальных точек отрезков + столбец для матричных вычислений аффинных преобразований
            orignal_points = new float[point_list.Count, 3];

            for (int i = 0; i < point_list.Count; ++i)
            {
                transofmed_points[i, 0] = point_list[i].X;
                transofmed_points[i, 1] = point_list[i].Y;
                transofmed_points[i, 2] = 1;

                orignal_points[i, 0] = point_list[i].X;
                orignal_points[i, 1] = point_list[i].Y;
                orignal_points[i, 2] = 1;    

            }

            center = get_center();
            g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle((int)(center.X - 1.5), (int)(center.Y - 1.5), 3, 3));

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            if (!checkBox3.Checked)
                two_points.Clear();
        }

    }
}
