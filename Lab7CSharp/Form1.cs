using System;
using System.Drawing;
using System.Windows.Forms;

namespace Lab7CSharp
{
    // Базовий клас для всіх фігур
    public abstract class Shape
    {
        public Color Color { get; set; }
        public Point Location { get; set; }

        public abstract void Draw(Graphics g);
        public abstract void Move(int dx, int dy);
    }

    // Клас для дуги
    public class Arc : Shape
    {
        public int Radius { get; set; }
        public int StartAngle { get; set; }
        public int SweepAngle { get; set; }

        public override void Draw(Graphics g)
        {
            g.DrawArc(new Pen(Color), Location.X, Location.Y, Radius * 2, Radius * 2, StartAngle, SweepAngle);
        }

        public override void Move(int dx, int dy)
        {
            Location = new Point(Location.X + dx, Location.Y + dy);
        }
    }

    // Клас для сектора
    public class Sector : Shape
    {
        public int Radius { get; set; }
        public int Angle { get; set; }

        public override void Draw(Graphics g)
        {
            g.FillPie(new SolidBrush(Color), Location.X, Location.Y, Radius * 2, Radius * 2, 0, Angle);
        }

        public override void Move(int dx, int dy)
        {
            Location = new Point(Location.X + dx, Location.Y + dy);
        }
    }

    // Клас для еліпса
    public class Ellipse : Shape
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public override void Draw(Graphics g)
        {
            g.FillEllipse(new SolidBrush(Color), Location.X, Location.Y, Width, Height);
        }

        public override void Move(int dx, int dy)
        {
            Location = new Point(Location.X + dx, Location.Y + dy);
        }
    }

    // Клас для прямокутника з заокругленими кутами
    public class RoundedRectangle : Shape
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int Radius { get; set; }

        public override void Draw(Graphics g)
        {
            var path = new System.Drawing.Drawing2D.GraphicsPath();
            path.AddArc(Location.X, Location.Y, Radius, Radius, 180, 90);
            path.AddArc(Location.X + Width - Radius, Location.Y, Radius, Radius, 270, 90);
            path.AddArc(Location.X + Width - Radius, Location.Y + Height - Radius, Radius, Radius, 0, 90);
            path.AddArc(Location.X, Location.Y + Height - Radius, Radius, Radius, 90, 90);
            path.CloseFigure();
            g.FillPath(new SolidBrush(Color), path);
        }

        public override void Move(int dx, int dy)
        {
            Location = new Point(Location.X + dx, Location.Y + dy);
        }
    }

    // Підклас Panel з включеним подвійним буфером
    public class DoubleBufferedPanel : Panel
    {
        public DoubleBufferedPanel()
        {
            this.DoubleBuffered = true;
        }
    }

    public partial class Form1 : Form
    {
        private bool isDrawing = false;
        private Point lastPoint;
        private Color currentColor = Color.Black;
        private Bitmap drawingBitmap;
        private Graphics graphics;

        private Timer graphTimer;
        private PictureBox graphBox;
        private int graphX = 1;

        private Panel drawingPanel;
        private Panel graphPanel;
        private Panel drawingShapesPanel;

        private Random rand = new Random();
        private Shape[] shapes;

        public Form1()
        {
            this.Text = "Три задачі";
            this.Size = new Size(800, 600);
            this.DoubleBuffered = true;

            // Кнопки перемикання режимів
            Button drawModeButton = new Button { Text = "Малювання", Location = new Point(10, 10) };
            Button graphModeButton = new Button { Text = "Графік", Location = new Point(110, 10) };
            Button drawShapesButton = new Button { Text = "Побудова малюнка", Location = new Point(210, 10) };
            this.Controls.Add(drawModeButton);
            this.Controls.Add(graphModeButton);
            this.Controls.Add(drawShapesButton);

            drawModeButton.Click += (s, e) => SwitchToDraw();
            graphModeButton.Click += (s, e) => SwitchToGraph();
            drawShapesButton.Click += (s, e) => SwitchToDrawingShapes();

            CreateDrawingPanel();
            CreateGraphPanel();
            CreateDrawingShapesPanel();

            SwitchToDraw();
        }

        private void CreateDrawingPanel()
        {
            drawingPanel = new DoubleBufferedPanel
            {
                Location = new Point(0, 50),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 50),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            drawingBitmap = new Bitmap(drawingPanel.Width, drawingPanel.Height);
            graphics = Graphics.FromImage(drawingBitmap);
            graphics.Clear(Color.White);

            Button newBtn = new Button { Text = "Нова картинка", Location = new Point(10, 10) };
            drawingPanel.Controls.Add(newBtn);
            newBtn.Click += (s, e) =>
            {
                graphics.Clear(Color.White);
                drawingPanel.Invalidate();
            };

            string[] colorNames = { "Чорний", "Червоний", "Зелений", "Синій" };
            Color[] colors = { Color.Black, Color.Red, Color.Green, Color.Blue };
            for (int i = 0; i < colors.Length; i++)
            {
                Button colorBtn = new Button
                {
                    Text = colorNames[i],
                    BackColor = colors[i],
                    Location = new Point(120 + i * 80, 10),
                    Width = 70
                };
                colorBtn.Click += (s, e) => currentColor = ((Button)s).BackColor;
                drawingPanel.Controls.Add(colorBtn);
            }

            drawingPanel.MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    isDrawing = true;
                    lastPoint = e.Location;
                }
            };

            drawingPanel.MouseMove += (s, e) =>
            {
                if (isDrawing)
                {
                    Pen pen = new Pen(currentColor, 2);
                    graphics.DrawLine(pen, lastPoint, e.Location);
                    lastPoint = e.Location;
                    drawingPanel.Invalidate();
                }
            };

            drawingPanel.MouseUp += (s, e) => isDrawing = false;

            drawingPanel.Paint += (s, e) =>
            {
                e.Graphics.DrawImageUnscaled(drawingBitmap, Point.Empty);
            };

            this.Controls.Add(drawingPanel);
        }

        private void CreateGraphPanel()
        {
            graphPanel = new DoubleBufferedPanel
            {
                Location = new Point(0, 50),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 50),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            graphBox = new PictureBox
            {
                Size = graphPanel.Size,
                Location = new Point(0, 0),
                BackColor = Color.White
            };
            graphPanel.Controls.Add(graphBox);

            Button fontBtn = new Button { Text = "Шрифт", Location = new Point(10, 10) };
            graphPanel.Controls.Add(fontBtn);

            FontDialog fontDialog = new FontDialog();

            fontBtn.Click += (s, e) =>
            {
                if (fontDialog.ShowDialog() == DialogResult.OK)
                    graphBox.Font = fontDialog.Font;
            };

            graphTimer = new Timer();
            graphTimer.Interval = 50;
            graphTimer.Tick += (s, e) =>
            {
                Bitmap bmp = new Bitmap(graphBox.Width, graphBox.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    Pen pen = new Pen(Color.Blue, 2);

                    float scaleX = graphBox.Width / 4f;
                    float scaleY = graphBox.Height / 2f;

                    for (float x = 0.01f; x < graphX / 100f && x < 4f; x += 0.01f)
                    {
                        float y = (float)(Math.Sin(x) / x);
                        float nextX = x + 0.01f;
                        float nextY = (float)(Math.Sin(nextX) / nextX);
                        g.DrawLine(pen,
                            x * scaleX,
                            graphBox.Height / 2 - y * scaleY,
                            nextX * scaleX,
                            graphBox.Height / 2 - nextY * scaleY);
                    }
                }

                graphBox.Image?.Dispose();
                graphBox.Image = bmp;

                graphX++;
                if (graphX > 400) graphTimer.Stop();
            };

            this.Controls.Add(graphPanel);
        }

        private void CreateDrawingShapesPanel()
        {
            drawingShapesPanel = new DoubleBufferedPanel
            {
                Location = new Point(0, 50),
                Size = new Size(this.ClientSize.Width, this.ClientSize.Height - 50),
                BackColor = Color.White,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right
            };

            Button generateShapesButton = new Button { Text = "Генерація фігур", Location = new Point(10, 10) };
            drawingShapesPanel.Controls.Add(generateShapesButton);

            generateShapesButton.Click += (s, e) => GenerateRandomShapes();

            this.Controls.Add(drawingShapesPanel);
        }

        private void GenerateRandomShapes()
        {
            graphics = drawingShapesPanel.CreateGraphics();
            graphics.Clear(Color.White); // Очищуємо перед малюванням

            // Створюємо масив фігур
            shapes = new Shape[5]; // Наприклад, 5 фігур

            // Створюємо випадкові фігури
            for (int i = 0; i < shapes.Length; i++)
            {
                int shapeType = rand.Next(4); // 4 типи фігур
                int x = rand.Next(50, drawingShapesPanel.Width - 50);
                int y = rand.Next(50, drawingShapesPanel.Height - 50);

                switch (shapeType)
                {
                    case 0: // Дуга
                        shapes[i] = new Arc
                        {
                            Color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)),
                            Location = new Point(x, y),
                            Radius = rand.Next(30, 70),
                            StartAngle = rand.Next(0, 360),
                            SweepAngle = rand.Next(0, 180)
                        };
                        break;
                    case 1: // Сектор
                        shapes[i] = new Sector
                        {
                            Color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)),
                            Location = new Point(x, y),
                            Radius = rand.Next(30, 70),
                            Angle = rand.Next(30, 180)
                        };
                        break;
                    case 2: // Еліпс
                        shapes[i] = new Ellipse
                        {
                            Color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)),
                            Location = new Point(x, y),
                            Width = rand.Next(40, 80),
                            Height = rand.Next(40, 80)
                        };
                        break;
                    case 3: // Прямокутник з заокругленими кутами
                        shapes[i] = new RoundedRectangle
                        {
                            Color = Color.FromArgb(rand.Next(256), rand.Next(256), rand.Next(256)),
                            Location = new Point(x, y),
                            Width = rand.Next(60, 100),
                            Height = rand.Next(40, 80),
                            Radius = rand.Next(10, 30)
                        };
                        break;
                }
            }

            // Малюємо фігури
            foreach (var shape in shapes)
            {
                shape.Draw(graphics);
            }
        }

        private void SwitchToDraw()
        {
            drawingPanel.Visible = true;
            graphPanel.Visible = false;
            drawingShapesPanel.Visible = false;
        }

        private void SwitchToGraph()
        {
            drawingPanel.Visible = false;
            graphPanel.Visible = true;
            drawingShapesPanel.Visible = false;
            graphX = 1;
            graphTimer.Start();
        }

        private void SwitchToDrawingShapes()
        {
            drawingPanel.Visible = false;
            graphPanel.Visible = false;
            drawingShapesPanel.Visible = true;
        }
    }
}
