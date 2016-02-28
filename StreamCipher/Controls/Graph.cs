using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StreamCipher.Controls
{
    public partial class Graph : Form
    {
        private readonly byte[] _seriesDataBytes;
        private readonly double[] _seriesDataDoubles;
        private int _widthOld, _heightOld;

        private int _gridInterval;
        private int GridInterval 
        {
            get { return _gridInterval;  }
            set
            {
                switch (value)
                {
                    case 16: _gridInterval = 8; break;
                    case 32: _gridInterval = 8; break;
                    case 64: _gridInterval = 16; break;
                    case 128: _gridInterval = 16; break;
                    case 256: _gridInterval = 32; break;
                }
            }
        }
        /*private int GridDimension
        {
            get { return _seriesDataBytes.Length / GridInterval; }
        }
        private int PointPerBlock
        {
            get { return _seriesDataBytes.Length / (GridDimension * GridDimension); }
        }*/

        public string FormName 
        {
            set { Text = value; }
        }

        public Graph(double[] arr) : this()
        {
            _seriesDataDoubles = new double[arr.Length];
            Array.Copy(arr, _seriesDataDoubles, arr.Length);

            label5.Visible = false;

            comboBoxMark.Items.Clear();
            comboBoxMark.Items.Add("лінія");
            comboBoxMark.Items.Add("точки");
            comboBoxMark.Items.Add("стовпчик");
            
            comboBoxMark.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 3;
            comboBox4.SelectedIndex = 0;
        }

        public Graph(byte[] arr, double sigma, double correlationCoefficient) : this()
        {
            GridInterval = arr.Length;
            _seriesDataBytes = new byte[arr.Length];
            Buffer.BlockCopy(arr, 0, _seriesDataBytes, 0, arr.Length);

            label5.Text = $"σ = {sigma}         r = {correlationCoefficient}";

            comboBoxMark.Items.Clear();
            comboBoxMark.Items.Add("лінія");
            comboBoxMark.Items.Add("точки");
            comboBoxMark.Items.Add("стовпчик");
            comboBoxMark.Items.Add("гісто - 8");
            comboBoxMark.Items.Add("гісто - 16");

            comboBoxMark.SelectedIndex = 1;
            comboBox2.SelectedIndex = 0;
            comboBox3.SelectedIndex = 3;
            comboBox4.SelectedIndex = 0;
        }

        private Graph()
        {
            InitializeComponent();
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_seriesDataBytes != null)
                showGraph(_seriesDataBytes);
            else
                showGraph(_seriesDataDoubles);

            switch (comboBoxMark.SelectedIndex)
            {
                case 0:
                    chart1.Series[0].ChartType = SeriesChartType.Line;
                    break;
                case 1:
                    chart1.Series[0].ChartType = SeriesChartType.Point;
                    break;
                case 2:
                    chart1.Series[0].ChartType = SeriesChartType.BoxPlot;
                    break;
                case 3:
                    gisto(chart1, 8);
                    break;
                case 4:
                    gisto(chart1, 16);
                    break;
            }
            chart1.Series[0].MarkerStyle = checkBox1.Checked || comboBoxMark.SelectedIndex == 1 ? MarkerStyle.Circle : MarkerStyle.None;
        }

        private void showGraph(byte[] arr)
        {
            Series series1 = new Series {ChartType = SeriesChartType.Line};

            for (int i = 0; i < arr.Length; i++)
                series1.Points.Add(new DataPoint(i, arr[i]));

            chart1.Series.Clear();
            chart1.Series.Add(series1);
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.Series[0].MarkerColor = Color.Red;
            chart1.Series[0].Color = Color.Orange;

            ChartArea chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea.AxisX.MajorGrid.Enabled = true;

            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.IntervalOffset = 0;
            chartArea.AxisX.Maximum = arr.Length;
            chartArea.AxisX.Interval = GridInterval;

            chartArea.AxisY.Maximum = arr.Length;
            chartArea.AxisY.Interval = GridInterval;

            chartArea.AxisX.Title = "Аргумент x";
            chartArea.AxisY.Title = "Функція y(x)";
        }

        private void showGraph(double[] arr)
        {
            Series series1 = new Series { ChartType = SeriesChartType.Line };

            for (int i = 0; i < arr.Length; i++)
                series1.Points.Add(new DataPoint(i, arr[i]));

            chart1.Series.Clear();
            chart1.Series.Add(series1);
            chart1.Series[0].IsVisibleInLegend = false;
            chart1.Series[0].MarkerColor = Color.Red;
            chart1.Series[0].Color = Color.Orange;

            ChartArea chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;
            chartArea.AxisY.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.IntervalOffset = 0;
            chartArea.AxisX.Maximum = arr.Length;
            chartArea.AxisX.Interval = chartArea.AxisX.Maximum / 16;

            chartArea.AxisY.Maximum = 1;
            chartArea.AxisY.Interval = 0.1;

            chartArea.AxisX.Title = "Аргумент τ";
            chartArea.AxisY.Title = "Функція K(τ)";
        }

        private void gisto(Chart chart, int gistoCount)
        {
            Series series1 = new Series{ChartType = SeriesChartType.Column};

            int countPointInInterval = _seriesDataBytes.Length / gistoCount;
            int countInterval = (_seriesDataBytes.Length / countPointInInterval);
            double[] newSeriesData = new double[countInterval];

            double xpositionColumn = countPointInInterval - countPointInInterval / 2.0;
            for (int i = 0, k = 0; i < countInterval; i++, xpositionColumn += (int)(_seriesDataBytes.Length / gistoCount))
            {
                for (int j = i * countPointInInterval; j < i * countPointInInterval + countPointInInterval; j++, k++)
                    newSeriesData[i] += _seriesDataBytes[k];
                series1.Points.Add(new DataPoint(xpositionColumn, newSeriesData[i] / countPointInInterval));
            }

            chart.Series.Clear();
            chart.ChartAreas.Clear();
            chart.ChartAreas.Add("");

            chart.Series.Add(series1);
            chart.Series[0].IsVisibleInLegend = false;
            chart.Series[0].MarkerColor = Color.Red;
            chart.Series[0].Color = Color.Orange;

            ChartArea chartArea = chart1.ChartAreas[0];
            chartArea.AxisX.Minimum = 0;
            chartArea.AxisX.IntervalOffset = 0;
            chartArea.AxisX.Maximum = _seriesDataBytes.Length;
            chartArea.AxisX.Interval = countPointInInterval;
            chartArea.AxisX.MajorGrid.LineDashStyle = ChartDashStyle.Dot;

            chartArea.AxisY.Maximum = _seriesDataBytes.Length;
            chartArea.AxisY.Interval = countPointInInterval;

            chartArea.AxisX.Title = "Аргумент x";
            chartArea.AxisY.Title = "Функція y(x)";
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            chart1.Series[0].MarkerStyle = checkBox1.Checked ? MarkerStyle.Circle : MarkerStyle.None;
        }

        private void Form2_Resize()
        {
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    chart1.Width = Width - 16;
                    chart1.Height = Height - 40 - chart1.Top;
                    break;

                case 1:
                    if (Math.Abs(Height - _heightOld) > Math.Abs(Width - _widthOld))
                    {
                        chart1.Height = Height - 40 - chart1.Top;
                        chart1.Width = chart1.Height;
                    }
                    else
                    {
                        chart1.Width = Width - 16;
                        chart1.Height = chart1.Width;
                    }

                    Height = chart1.Height + 40 + chart1.Top;
                    Width = chart1.Width + 16;                    
                    break;
            }
            label5.Left = (Width - label5.Width)/2 + 10;
        }
       
        private void Form2_ResizeBegin(object sender, EventArgs e)
        {
            _widthOld = Width; 
            _heightOld = Height;
        }

        private void Form2_SizeChanged(object sender, EventArgs e)
        {   
            if (comboBox2.SelectedIndex == 0)
                Form2_Resize();
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            Form2_Resize();
        }

        private void Form2_ResizeEnd(object sender, EventArgs e)
        {
            Form2_Resize();
        }

        private void button_колір_маркера_Click(object sender, EventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog {AnyColor = true, SolidColorOnly = false, Color = Color.Orange};
            if (colorDlg.ShowDialog() == DialogResult.OK)
                chart1.Series[0].Color = colorDlg.Color;
        }

        private void button_колір_точки_Click(object sender, EventArgs e)
        {
            ColorDialog colorDlg = new ColorDialog {AnyColor = true, SolidColorOnly = false, Color = Color.Red};
            if (colorDlg.ShowDialog() == DialogResult.OK)
                chart1.Series[0].MarkerColor = colorDlg.Color;
        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart1.Series[0].MarkerSize = comboBox3.SelectedIndex + 2;
        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e)
        {
            chart1.Series[0].BorderWidth = comboBox4.SelectedIndex + 1;
        }
    }
}