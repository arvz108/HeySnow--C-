using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Snow {
	public partial class MainForm : Form {
		#region Classes

		/// <summary>
		/// This Container class represents the snowflake falling and rendered to the screen
		/// </summary>
		private class SnowFlake {
			public float Rotation;
			public float RotVelocity;
			public float Scale;
			public float X;
			public float XVelocity;
			public float Y;
			public float YVelocity;
		}

		#endregion

		#region Properties

		private Bitmap m_Snow;

		/// <summary>
		/// The (cached) Image of a 32x32 snowflake
		/// </summary>
		private Bitmap Snow {
			get {
				if (m_Snow == null) {
					///First Time - Create Image
					m_Snow = new Bitmap(32, 32);

					using (Graphics g = Graphics.FromImage(m_Snow)) {
						g.SmoothingMode = SmoothingMode.AntiAlias;
						g.Clear(Color.Transparent);

						g.TranslateTransform(16, 16, MatrixOrder.Append);

						Color black = Color.FromArgb(1, 1, 1);
						Color white = Color.FromArgb(255, 255, 255);

						DrawSnow(g, new SolidBrush(black), new Pen(black, 3f));
						DrawSnow(g, new SolidBrush(white), new Pen(white, 2f));

						g.Save();
					}
				}

				return m_Snow;
			}
		}

		#endregion

		private static readonly Random Random = new Random();

		/// <summary>
		/// Contains all Snowflakes currently active
		/// </summary>
		private readonly List<SnowFlake> SnowFlakes = new List<SnowFlake>();

		private int Tick = 0;

		public MainForm() {
			InitializeComponent();

			//We paint our control ourself and need a double buffer to prevent flimmering
			SetStyle(
				ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint |
				ControlStyles.DoubleBuffer, true);

			//Resize and relocate form to match Screen settings
			Location = new Point(Screen.PrimaryScreen.Bounds.X, Screen.PrimaryScreen.Bounds.Y);
			Width = Screen.PrimaryScreen.Bounds.Width;
			Height = Screen.PrimaryScreen.Bounds.Height;
		}

		/// <summary>
		/// Called when the form is loaded
		/// Starts the tick timer
		/// </summary>
		/// <param name="sender">-</param>
		/// <param name="e">-</param>
		private void MainForm_Load(object sender, EventArgs e) {
			Timer timer = new Timer();
			timer.Interval = 20;
			timer.Tick += OnTick;
			timer.Start();
		}

		/// <summary>
		/// Creates, moves and deletes snowflakes
		/// Causes form redraw
		/// </summary>
		/// <param name="sender">-</param>
		/// <param name="args">-</param>
		private void OnTick(object sender, EventArgs args) {
			//Tick..
			Tick++;

			//Spawn new Flakes
			if (Tick%3 == 0 && Random.NextDouble() < 0.70) {
				SnowFlake s = new SnowFlake();
				s.X = Random.Next(-50, Width + 50); //All over the screen...
				s.Y = Random.Next(-20, -7); //Customize height further
				s.XVelocity = (float) (Random.NextDouble() - 0.5f)*2f;
				s.YVelocity = (float) (Random.NextDouble()*3) + 1f;
				s.Rotation = Random.Next(0, 359);
				s.RotVelocity = Random.Next(-3, 3)*2;

				if (s.RotVelocity == 0) {
					s.RotVelocity = 3; //No rotation suxx, really.
				}

				s.Scale = (float) (Random.NextDouble()/2) + 0.75f;
				SnowFlakes.Add(s);
			}

			//Move current flakes (and add them to del list, if they exceed the screen)
			List<SnowFlake> del = new List<SnowFlake>();
			foreach (SnowFlake s in SnowFlakes) {
				s.X += s.XVelocity;
				s.Y += s.YVelocity;
				s.Rotation += s.RotVelocity;

				//Make them move snowflake like
				s.XVelocity += ((float) Random.NextDouble() - 0.5f)*0.7f;
				s.XVelocity = Math.Max(s.XVelocity, -2f);
				s.XVelocity = Math.Min(s.XVelocity, +2f);

				if (s.YVelocity > Height + 10) //Out of Screen
				{
					del.Add(s);
				}
			}

			//Delete them
			foreach (SnowFlake s in del) {
				SnowFlakes.Remove(s);
			}

			//Redraw our control
			Refresh();
		}

		/// <summary>
		/// Renders all Snowflakes
		/// Called when form.Refresh() is called
		/// </summary>
		/// <param name="sender">-</param>
		/// <param name="e">PaintEventArgs (Graphics Object)</param>
		private void MainForm_Paint(object sender, PaintEventArgs e) {
			Graphics g = e.Graphics;
			g.SmoothingMode = SmoothingMode.HighSpeed; //other things may be to slow

			foreach (SnowFlake s in SnowFlakes) {
				g.ResetTransform();
				g.TranslateTransform(-16, -16, MatrixOrder.Append); //Align our flakes to the center
				g.ScaleTransform(s.Scale, s.Scale, MatrixOrder.Append); //Scale them..
				g.RotateTransform(s.Rotation, MatrixOrder.Append); //Rotate them..
				g.TranslateTransform(s.X, s.Y, MatrixOrder.Append); //Move them to their appropriate location
				g.DrawImage(Snow, 0, 0); //draw them
			}
		}

		/// <summary>
		/// Called when the user clicks double on the tray icon
		/// Closes the form/application
		/// </summary>
		/// <param name="sender">-</param>
		/// <param name="e">-</param>
		private void notifyIcon1_DoubleClick(object sender, EventArgs e) {
			notifyIcon1.Visible = false; //Otherwise there may appear ghost icons some times
			Close();
		}

		#region Helpers
		/// <summary>
		/// Draws a snow flake on the specified graphics object
		/// </summary>
		/// <param name="g">Graphics object to draw on</param>
		/// <param name="b">Brush (Color) of the middle part</param>
		/// <param name="p">Pen (Color, Size) of the lines</param>
		private static void DrawSnow(Graphics g, Brush b, Pen p)
		{
			const int a = 6;
			const int a2 = a + 2;
			const int r = 2;

			g.DrawLine(p, -a, -a, +a, +a);
			g.DrawLine(p, -a, +a, +a, -a);

			g.DrawLine(p, -a2, 0, +a2, 0);
			g.DrawLine(p, 0, -a2, 0, +a2);

			g.FillEllipse(b, -r, -r, r * 2, r * 2);
		}
		#endregion
	}
}