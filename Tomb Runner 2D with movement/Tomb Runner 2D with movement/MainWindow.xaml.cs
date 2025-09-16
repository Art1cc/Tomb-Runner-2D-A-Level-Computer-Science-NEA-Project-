using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Tomb_Runner_2D_with_movement
{
    public partial class MainWindow : Window
    {
        private double characterSpeed = 5.0; // Smaller steps for smoother movement
        private bool UpPressed = false;
        private bool DownPressed = false;
        private bool LeftPressed = false;
        private bool RightPressed = false;
        private DispatcherTimer gameTimer; // For continuous movement
        private int mazeWidth = 20;
        private int mazeHeight = 20;
        private int cellSize = 40;
        private int[,] maze;
        private Random rand = new Random();
        private List<Rectangle> mazeWalls = new List<Rectangle>();

        public MainWindow()
        {
            InitializeComponent();

            // Set up game timer for continuous updates
            gameTimer = new DispatcherTimer();
            gameTimer.Interval = TimeSpan.FromMilliseconds(16); // ~60 FPS
            gameTimer.Tick += GameLoop;
            gameTimer.Start();

            // Set up keyboard event handlers
            KeyDown += KeyPressed;
            KeyUp += KeyReleased;
        }

        private void KeyPressed(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    UpPressed = true;
                    break;
                case Key.A:
                    LeftPressed = true;
                    break;
                case Key.S:
                    DownPressed = true;
                    break;
                case Key.D:
                    RightPressed = true;
                    break;
            }
        }

        private void KeyReleased(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.W:
                    UpPressed = false;
                    break;
                case Key.S:
                    DownPressed = false;
                    break;
                case Key.A:
                    LeftPressed = false;
                    break;
                case Key.D:
                    RightPressed = false;
                    break;
            }
        }

        private void GameLoop(object sender, EventArgs e)
        {
            double left = Canvas.GetLeft(Character);
            double top = Canvas.GetTop(Character);
            double newLeft = left;
            double newTop = top;

            // Handle diagonal movement by normalizing speed
            double moveX = 0;
            double moveY = 0;

            if (UpPressed) moveY -= characterSpeed;
            if (DownPressed) moveY += characterSpeed;
            if (LeftPressed) moveX -= characterSpeed;
            if (RightPressed) moveX += characterSpeed;

            // Normalize diagonal movement
            if (moveX != 0 && moveY != 0)
            {
                double length = Math.Sqrt(moveX * moveX + moveY * moveY);
                moveX = (moveX / length) * characterSpeed;
                moveY = (moveY / length) * characterSpeed;
            }

            newLeft += moveX;
            newTop += moveY;

            // Check canvas boundaries
            newLeft = Math.Max(0, Math.Min(newLeft, Level.ActualWidth - Character.Width));
            newTop = Math.Max(0, Math.Min(newTop, Level.ActualHeight - Character.Height));

            // Check collisions
            if (!CheckCollision(newLeft, newTop))
            {
                Canvas.SetLeft(Character, newLeft);
                Canvas.SetTop(Character, newTop);
            }
            else
            {
                // Try sliding along walls (partial movement)
                if (!CheckCollision(newLeft, top))
                {
                    Canvas.SetLeft(Character, newLeft);
                }
                else if (!CheckCollision(left, newTop))
                {
                    Canvas.SetTop(Character, newTop);
                }
            }
        }

        private bool CheckCollision(double newLeft, double newTop)
        {
            Rect characterRect = new Rect(newLeft, newTop, Character.Width, Character.Height);

            foreach (Rectangle wall in mazeWalls)
            {
                Rect wallRect = new Rect(
                    Canvas.GetLeft(wall),
                    Canvas.GetTop(wall),
                    wall.Width,
                    wall.Height
                );

                if (characterRect.IntersectsWith(wallRect))
                {
                    return true;
                }
            }
            return false;
        }

        private void PlayClicked(object sender, MouseButtonEventArgs e)
        {
            MenuScreen.Visibility = Visibility.Collapsed;
            LevelMenu.Visibility = Visibility.Visible;
        }

        private void ExitClicked(object sender, MouseButtonEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void GenerateClicked(object sender, MouseButtonEventArgs e)
        {
            LevelMenu.Visibility = Visibility.Collapsed;
            Level.Visibility = Visibility.Visible;

            Level.Focus();
            Canvas.SetLeft(QuitLevel, 0);
            Canvas.SetTop(QuitLevel, (Level.ActualHeight - QuitLevel.Height));

            // Clear previous maze
            foreach (Rectangle wall in mazeWalls)
            {
                Level.Children.Remove(wall);
            }
            mazeWalls.Clear();

            // Generate new maze
            GenerateMaze();
            DrawMaze();

            // Place character at starting position
            Canvas.SetLeft(Character, cellSize);
            Canvas.SetTop(Character, cellSize);
        }

        private void BackClicked(object sender, MouseButtonEventArgs e)
        {
            LevelMenu.Visibility = Visibility.Collapsed;
            MenuScreen.Visibility = Visibility.Visible;
        }

        private void CharacterButtonClicked(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("not done yet");
        }

        private void LevelQuit(object sender, MouseButtonEventArgs e)
        {
            Level.Visibility = Visibility.Collapsed;
            LevelMenu.Visibility = Visibility.Visible;
        }

        private void GenerateMaze()
        {
            maze = new int[mazeWidth, mazeHeight];
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    maze[x, y] = 1;
                }
            }
            GenerateMazeRecursive(1, 1);
        }

        private void GenerateMazeRecursive(int x, int y)
        {
            maze[x, y] = 0;
            int[] directions = { 0, 1, 2, 3 };
            Shuffle(directions);

            foreach (int dir in directions)
            {
                int nx = x, ny = y;
                switch (dir)
                {
                    case 0: nx += 2; break; // right
                    case 1: ny += 2; break; // down
                    case 2: nx -= 2; break; // left
                    case 3: ny -= 2; break; // up
                }

                if (nx > 0 && nx < mazeWidth - 1 && ny > 0 && ny < mazeHeight - 1 && maze[nx, ny] == 1)
                {
                    maze[(x + nx) / 2, (y + ny) / 2] = 0;
                    GenerateMazeRecursive(nx, ny);
                }
            }
        }

        private void Shuffle(int[] array)
        {
            for (int i = array.Length - 1; i > 0; i--)
            {
                int j = rand.Next(0, i + 1);
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

        private void DrawMaze()
        {
            for (int x = 0; x < mazeWidth; x++)
            {
                for (int y = 0; y < mazeHeight; y++)
                {
                    if (maze[x, y] == 1)
                    {
                        Rectangle wall = new Rectangle
                        {
                            Width = cellSize,
                            Height = cellSize,
                            Fill = Brushes.Red
                        };
                        Canvas.SetLeft(wall, x * cellSize);
                        Canvas.SetTop(wall, y * cellSize);
                        Level.Children.Add(wall);
                        mazeWalls.Add(wall);
                    }
                }
            }
        }
    }
}