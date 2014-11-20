using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheeseFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            CheeseFinder game = new CheeseFinder();
            game.PlayGame();
        }

        
    }

    class CheeseFinder
    {
        Random rng = new Random();
        Point[,] grid;
        public Point Mouse { get; set; }
        public Point Cheese { get; set; }
        public int Round { get; set; }

        public CheeseFinder()
        {
            /// Initalize the grid
            grid = new Point[10, 10];

            // Fill up the grid with PointStatus.Empty Points
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    grid[x, y] = new Point(x, y);
                }
            }

            // Make the Mouse and randomly place on the grid
            int mX = rng.Next(0, grid.GetLength(0));
            int mY = rng.Next(0, grid.GetLength(1));
            grid[mX, mY].Status = Point.PointStatus.Mouse;
            Mouse = grid[mX, mY];

            // Make the Cheese and randomly place on the grid
            int cX;
            int cY;
            do {
                cX = rng.Next(0, grid.GetLength(0));
                cY = rng.Next(0, grid.GetLength(1));
            } while (grid[cX,cY].Status == Point.PointStatus.Mouse);
            // Found X,Y Coords without a mouse
            grid[cX, cY].Status = Point.PointStatus.Cheese;
            Cheese = grid[cX, cY];
        }

        public void PlayGame()
        {
            bool hasCheeseBeenFound = false;

            while (!hasCheeseBeenFound)
            {
                this.DrawGrid();
                hasCheeseBeenFound = this.MoveMouse(this.GetUserMove());
                this.Round++;
            }
            Console.WriteLine("You found the cheese, it only took {0} rounds", this.Round);
            Console.ReadLine();
        }

        public void DrawGrid()
        {
            Console.Clear();
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    switch (grid[x,y].Status)
                    {
                        case Point.PointStatus.Empty: Console.Write("[ ]");
                            break;
                        case Point.PointStatus.Cheese: Console.Write("[C]");
                            break;
                        case Point.PointStatus.Mouse: Console.Write("[M]");
                            break;
                    }
                }
                // Write a new line at the end of the row
                Console.Write("\n");
            }
        }

        public ConsoleKey GetUserMove()
        {
            // Get the user input
            ConsoleKeyInfo userInput = Console.ReadKey();
            switch (userInput.Key)
            {
                case ConsoleKey.DownArrow: 
                case ConsoleKey.LeftArrow: 
                case ConsoleKey.RightArrow:
                case ConsoleKey.UpArrow: return userInput.Key;
                default:
                    Console.WriteLine("Invalid Input");
                    System.Threading.Thread.Sleep(750);
                    return userInput.Key;
            }
        }
        /// <summary>
        /// Figure out if the next move is valid
        /// </summary>
        /// <param name="userKey">The key that the user is pressing</param>
        /// <returns>true if the move is valid</returns>
        public bool ValidMove(ConsoleKey userKey)
        {
            switch (userKey)
            {
                case ConsoleKey.DownArrow: return this.Mouse.YCord < 9;
                case ConsoleKey.LeftArrow: return this.Mouse.XCord > 0;
                case ConsoleKey.RightArrow: return this.Mouse.XCord < 9;
                case ConsoleKey.UpArrow: return this.Mouse.YCord > 0;
            }
            // User pressed the wrong key so that's never a valid move
            return false;
        }

        public bool MoveMouse(ConsoleKey userKey)
        {
            if (ValidMove(userKey))
            {
                int newX;
                int newY;
                switch (userKey)
                {
                    case ConsoleKey.DownArrow:
                        newX = Mouse.XCord;
                        newY = Mouse.YCord + 1;
                        return HasCheese(newX, newY);
                    case ConsoleKey.LeftArrow:
                        newX = Mouse.XCord - 1;
                        newY = Mouse.YCord;
                        return HasCheese(newX, newY);
                    case ConsoleKey.RightArrow:
                        newX = Mouse.XCord + 1;
                        newY = Mouse.YCord;
                        return HasCheese(newX, newY);
                    case ConsoleKey.UpArrow:
                        newX = Mouse.XCord;
                        newY = Mouse.YCord - 1;
                        return HasCheese(newX, newY);
                }
            }
            // Must have hit a incorrect key, always return false
            return false;
        }
        /// <summary>
        /// A helper method to determine if a point on the grid has cheese in it,
        /// if there is no cheese the mouse is moved as well
        /// </summary>
        /// <param name="x">x coord</param>
        /// <param name="y">y coord</param>
        /// <returns>true if cheese is found</returns>
        private bool HasCheese(int x, int y)
        {
            if (grid[x, y].Status != Point.PointStatus.Cheese)
            {
                // No cheese found
                grid[Mouse.XCord, Mouse.YCord].Status = Point.PointStatus.Empty; // Get rid of mouse
                grid[x, y].Status = Point.PointStatus.Mouse; // Move mouse to new place
                Mouse = grid[x, y]; // Update the Mouse property
                return false;
            }
            else { return true; } // Cheese found
        }
    }

    /// <summary>
    /// Class representing the status of a certain point on the grid
    /// </summary>
    class Point
    {
        public enum PointStatus
        {
            Empty, Cheese, Mouse
        }
        public int XCord { get; set; }
        public int YCord { get; set; }
        private PointStatus _pointStatus;

        public PointStatus Status
        {
            get { return _pointStatus; }
            set { _pointStatus = value; }
        }

        public Point(int x, int y)
        {
            this.XCord = x;
            this.YCord = y;
            this.Status = PointStatus.Empty;
        }
        
    }
}

