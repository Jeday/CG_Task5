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
        
        List<PointF> point_list;


        float[,] orignal_points;

        /// <summary>
        ///  All data that is needed for redraw
        /// </summary>

        private List<PointF> two_points; // LONE SEGMENT
        private float[,] transofmed_points; // MATRIX OF FIGURE
        private PointF center; // CENTER OF FIGURE
        private List<PointF> new_points; // new points
        private string figure_type; // type of figure
        private List<PointF> check_points; // points in/out of figure Right/Left of Line


        ///

        public Form1()
        {
            InitializeComponent();
            point_list = new List<PointF>();
            two_points = new List<PointF>();
            new_points = new List<PointF>();
            transofmed_points = new float[0,0];
            orignal_points = new float[0, 0];
            check_points = new List<PointF>();
            figure_type = comboBox1.Text;

        }
        

        ///  Считает, пересекаются ли отрезки (p1,p2) и (p3,p4). 
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


     

        private void Form1_Load(object sender, EventArgs e)
        {
           
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

    

        private void ValueChanged(object sender, EventArgs e)
        {
            apply_transforms();
        }

        private void apply_transforms() {
            int t = trackBar3.Value; // angle
            float tx = trackBar1.Value; // offset x;
            float ty = trackBar5.Value; // offset y
            float scale_x = (float)(trackBar2.Value)/10; // scale x
            float scale_y = (float)(trackBar4.Value)/10; // scale y


            float[,] transferalMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { tx, -ty, 1 } };
            float[,] scaleMatrix = new float[,] { { scale_x, 0, 0 }, { 0, scale_y, 0 }, { 0,0, 1 } };
            float[,] transferalToXYMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { -center.X, -center.Y, 1 } };
            float[,] rotationMatrix = new float[,] { { (float)Math.Cos(t * Math.PI / 180), (float)Math.Sin(t * Math.PI / 180), 0 }, { (float)-(Math.Sin(t * Math.PI / 180)), (float)Math.Cos(t * Math.PI / 180), 0 }, { 0, 0, 1 } };
            float[,] transferalFromXYMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { center.X, center.Y, 1 } };
            float[,] initMatrix = new float[,] { { 1, 0, 0 }, { 0, 1, 0 }, { 0, 0, 1 } };



            transofmed_points =transferalMatrix;
            transofmed_points = multiply_matrix(transofmed_points, transferalToXYMatrix);
            transofmed_points = multiply_matrix(transofmed_points, scaleMatrix); 
            transofmed_points = multiply_matrix(transofmed_points, rotationMatrix); 
            transofmed_points = multiply_matrix(transofmed_points, transferalFromXYMatrix);
            


            transofmed_points = multiply_matrix(orignal_points, transofmed_points);
            pictureBox1.Invalidate();
        }


       private int point_in_polygon(PointF n)
        {

            for (int i = 0; i < transofmed_points.GetLength(0); ++i) 
                if (transofmed_points[i, 0] == n.X && transofmed_points[i, 1] == n.Y)
                    return 0;
                

            int cnt_intersec = 0;
            PointF intersec;
            PointF finish = new PointF(0, 0);
            for(var i = 0; i < transofmed_points.GetLength(0)-1; ++i)
            {
                if (FindIntersection(n, finish, new PointF(transofmed_points[i, 0], transofmed_points[i, 1]), new PointF(transofmed_points[i + 1, 0], transofmed_points[i + 1, 1]), out intersec) 
                    && intersec != new PointF(transofmed_points[i, 0], transofmed_points[i, 1]))
                ++cnt_intersec;
            }

            PointF p1 = new PointF(transofmed_points[transofmed_points.GetLength(0) - 1, 0], transofmed_points[transofmed_points.GetLength(0) - 1, 1]);
            PointF p2 = new PointF(transofmed_points[0, 0], transofmed_points[0, 1]);
            if (FindIntersection(n, finish, p1, p2, out intersec) && intersec != p1)
                ++cnt_intersec;

            if (cnt_intersec % 2 != 0)     // принадлежит многоугольнику
                return 1;
            else                           // не принадлежит
                return -1;
        }


        private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (checkBox2.Checked) { // Place center
                center = e.Location;
            }
            else if (checkBox1.Checked) // Place Point
            {

                check_points.Add(e.Location);

            }
            else if (checkBox3.Checked)
            {
                if (two_points.Count < 2)
                {
                    two_points.Add(new PointF(e.X, e.Y));
                    
                }
                else 
                {
                    two_points.Clear();
                    two_points.Add(new PointF(e.X, e.Y));
 
                }
            }
            else
            {
                new_points.Add(new PointF(e.X, e.Y));  
            }
            pictureBox1.Invalidate();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            figure_type = comboBox1.Text;
            point_list.Clear();
            List<PointF> t = point_list;
            point_list = new_points;
            new_points = t;
            two_points.Clear();
            check_points.Clear();

            if (figure_type == "Line")
            {
                if (point_list.Count > 2)
                    point_list = point_list.Skip(point_list.Count - 2).ToList(); // последние 2  
            }
           

            trackBar1.Value = 0;
            trackBar5.Value = 0;
            trackBar3.Value = 0;
            trackBar2.Value = 10;
            trackBar4.Value = 10;

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

            if(point_list.Count >0)
            center = get_center();

            pictureBox1.Invalidate();


        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

         
            for (var i = 0; i < transofmed_points.GetLength(0) - 1; i++)
            {
                g.DrawLine(new Pen(Color.Black), transofmed_points[i, 0], transofmed_points[i, 1], transofmed_points[i + 1, 0], transofmed_points[i + 1, 1]);
            }
            if (figure_type == "Polygon" && transofmed_points.GetLength(0)>1)
                g.DrawLine(new Pen(Color.Black), transofmed_points[0, 0], transofmed_points[0, 1], transofmed_points[transofmed_points.GetLength(0) - 1, 0], transofmed_points[transofmed_points.GetLength(0) - 1, 1]);

            if (point_list.Count > 0)
                g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle((int)(center.X - 1.5), (int)(center.Y - 1.5), 3, 3));

            foreach(var p in new_points)
                g.DrawEllipse(new Pen(Color.Red), new Rectangle((int)(p.X - (float)1.5), (int)(p.Y - (float)1.5), 3, 3));


            foreach (PointF p in check_points) {
                Point pi = new Point((int)p.X, (int)p.Y);
                if (figure_type == "Line" && point_list.Count >0)
                {
                    float pos = (transofmed_points[1, 0] - transofmed_points[0, 0]) * (pi.Y - transofmed_points[0, 1]) - (transofmed_points[1, 1] - transofmed_points[0, 1]) * (pi.X - transofmed_points[0, 0]);
                    if (pos < 0.0)            // слева
                        g.DrawEllipse(new Pen(Color.Blue), new Rectangle(pi.X - 1, pi.Y - 1, 3, 3));
                    else if (pos > 0.0)       // справа
                        g.DrawEllipse(new Pen(Color.Green), new Rectangle(pi.X - 1, pi.Y - 1, 3, 3));
                    else                     // на прямой
                        g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle(pi.X - 1, pi.Y - 1, 3, 3));
                }
                else if (figure_type == "Polygon" && point_list.Count > 0) {
                    switch (point_in_polygon(p))
                    {
                        case 1:
                            g.DrawEllipse(new Pen(Color.Blue), new Rectangle(pi.X - 1, pi.Y - 1, 3, 3));
                            break;
                        case -1:
                            g.DrawEllipse(new Pen(Color.Green), new Rectangle(pi.X - 1, pi.Y - 1, 3, 3));
                            break;
                        default:
                        case 0:
                            g.DrawEllipse(new Pen(Color.Chocolate), new Rectangle(pi.X - 1, pi.Y - 1, 3, 3));
                            break;
                    }
                }
            }

            if (two_points.Count == 1)
                g.DrawEllipse(new Pen(Color.Red), new Rectangle((int)two_points.First().X - 1, (int)two_points.First().Y - 1, 3, 3));
            else if (two_points.Count == 2) {
                g.DrawLines(new Pen(Color.Black), two_points.ToArray());

                for (var i = 0; i < transofmed_points.GetLength(0) - 1; i++)
                    if (FindIntersection(two_points[0], two_points[1], new PointF(transofmed_points[i, 0], transofmed_points[i, 1]), new PointF(transofmed_points[i + 1, 0], transofmed_points[i + 1, 1]), out PointF intersec))
                        g.DrawEllipse(new Pen(Color.Fuchsia, 4), new Rectangle((int)intersec.X - 1, (int)intersec.Y - 1, 4, 4));
                if (figure_type == "Polygon" && transofmed_points.GetLength(0) > 1)
                     if (FindIntersection(two_points[0], two_points[1], new PointF(transofmed_points[0, 0], transofmed_points[0, 1]), new PointF(transofmed_points[transofmed_points.GetLength(0) - 1, 0], transofmed_points[transofmed_points.GetLength(0) - 1, 1]), out PointF intersec))
                        g.DrawEllipse(new Pen(Color.Fuchsia, 4), new Rectangle((int)intersec.X - 1, (int)intersec.Y - 1, 4, 4));

            }


        }

        private void checkBox2_Click(object sender, EventArgs e)
        {
            CheckBox c = sender as CheckBox;
            if (c.Checked) {
                checkBox1.Checked = false;
                checkBox2.Checked = false;
                checkBox3.Checked = false;
                c.Checked = true;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (point_list.Count > 0)
            {
                center = get_center();
                pictureBox1.Invalidate();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            orignal_points = transofmed_points;
            trackBar1.Value = 0;
            trackBar5.Value = 0;
            trackBar3.Value = 0;
            trackBar2.Value = 10;
            trackBar4.Value = 10;
        }
    }
}
