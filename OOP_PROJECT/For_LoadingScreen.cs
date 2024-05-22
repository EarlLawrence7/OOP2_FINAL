using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace OOP_PROJECT
{
    public partial class For_LoadingScreen : Form
    {
        public For_LoadingScreen()
        {
            InitializeComponent();
            pictureBox1.Image = Image.FromFile(@"C:\Users\bagui\Downloads\download.gif");
            Region = new Region(RoundedRectangle(ClientRectangle, 20)); // Set rounded corners
        }
        private void For_LoadingScreen_Load(object sender, EventArgs e)
        {
            // Start a new thread for the loading process
            Thread loadingThread = new Thread(new ThreadStart(PerformLoading));
            loadingThread.Start();
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (GraphicsPath path = RoundedRectangle(ClientRectangle, 20))
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                using (Pen pen = new Pen(Color.Gray, 1))
                {
                    e.Graphics.DrawPath(pen, path);
                }
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }

        private void PerformLoading()
        {
            try
            {
                // Simulate some loading process
                for (int i = 0; i <= 100; i++)
                {
                    // Simulate work by sleeping the thread
                    Thread.Sleep(30);
                }
            }
            finally
            {
                // Close the loading screen
                CloseLoadingScreen();
            }
        }

        private void CloseLoadingScreen()
        {
            if (InvokeRequired)
            {
                Invoke(new MethodInvoker(() =>
                {
                    // Create an instance of the main form (For_Login) and show it
                    For_Login mainForm = new For_Login();
                    mainForm.Show();

                    // Hide the loading screen
                    Hide();
                }));
            }
            else
            {
                // Create an instance of the main form (For_Login) and show it
                For_Login mainForm = new For_Login();
                mainForm.Show();
                Hide();
            }
        }


        // Helper method to create a rounded rectangle
        private GraphicsPath RoundedRectangle(Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;
            Rectangle arc = new Rectangle(bounds.Location, new Size(diameter, diameter));

            path.AddArc(arc, 180, 90);
            arc.X = bounds.Right - diameter;
            path.AddArc(arc, 270, 90);
            arc.Y = bounds.Bottom - diameter;
            path.AddArc(arc, 0, 90);
            arc.X = bounds.Left;
            path.AddArc(arc, 90, 90);
            path.CloseFigure();

            return path;
        }
    }
}
