﻿using System;
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
        public Mouse Mouse { get; set; }
        public Point Cheese { get; set; }
        public int CheeseCount { get; set; }
        public int Round { get; set; }
        List<Cat> lazyCats = new List<Cat>();
        bool nextMoveIsACat = false;

        public CheeseFinder()
        {
            /// Initalize the grid
            grid = new Point[10, 10];

            // Fill up the grid with Point.PointStatus.Empty Points
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
            grid[mX, mY] = new Mouse(mX, mY);
            if (grid[mX, mY] is Mouse) { Mouse = (Mouse)grid[mX, mY]; }

            // Randomly place the cheese
            PlaceCheese();
        }

        public void MoveCat(Cat theCat)
        {
            // Based on the type of cat, the move chance is different
            // Tiger has 99% chance
            // Housecat has a 70% chance
            // The distracted kitten has a 40% chance
            switch (theCat.Type)
            {
                case Cat.CatType.Kitten: if (rng.Next(0, 101) < 41) { MoveCatObj(theCat); }
                    break;
                case Cat.CatType.HouseCat: if (rng.Next(0, 101) < 71) { MoveCatObj(theCat); }
                    break;
                case Cat.CatType.Tiger: if (rng.Next(0, 101) < 99) { MoveCatObj(theCat); }
                    break;
            }
        }
        /// <summary>
        /// A method that does basic pathfinding to move the cat
        /// in the direction of the mouse
        /// </summary>
        /// <param name="theCat">the cat object to be evaluated</param>
        public void MoveCatObj(Cat theCat)
        {
            // Get the relative position of the mouse
            int xDiff = this.Mouse.XCord - theCat.XCord;
            int yDiff = this.Mouse.YCord - theCat.YCord;
            bool tryLeft = xDiff < 0;
            bool tryRight = xDiff > 0;
            bool tryUp = yDiff < 0;
            bool tryDown = yDiff > 0;
            Point targetPosition = (Cat)theCat.Clone();
            bool validMove = false;

            while (!validMove && !nextMoveIsACat && (tryLeft || tryRight || tryUp || tryDown))
            {
                // try diagonal moves first, but kitten can't do it because it's dumb
                if (theCat.Type != Cat.CatType.Kitten)
                {
                    if (tryUp && tryRight && !validMove) { if (IsMoveOnBoard(targetPosition.XCord + 1, targetPosition.YCord - 1)) { targetPosition.XCord++; targetPosition.YCord--; validMove = true; } }
                    if (tryDown && tryRight && !validMove) { if (IsMoveOnBoard(targetPosition.XCord + 1, targetPosition.YCord + 1)) { targetPosition.XCord++; targetPosition.YCord++; validMove = true; } }
                    if (tryDown && tryLeft && !validMove) { if (IsMoveOnBoard(targetPosition.XCord - 1, targetPosition.YCord + 1)) { targetPosition.XCord--; targetPosition.YCord++; validMove = true; } }
                    if (tryUp && tryLeft && !validMove) { if (IsMoveOnBoard(targetPosition.XCord - 1, targetPosition.YCord - 1)) { targetPosition.XCord--; targetPosition.YCord--; validMove = true; } }
                }
                // Try a direction if we can't get a more efficent diagonal move
                if (tryUp && !validMove) { if (IsMoveOnBoard(targetPosition.XCord, targetPosition.YCord - 1)) { targetPosition.YCord--; validMove = true; } }
                if (tryDown && !validMove) { if (IsMoveOnBoard(targetPosition.XCord, targetPosition.YCord + 1)) { targetPosition.YCord++; validMove = true; } }
                if (tryRight && !validMove) { if (IsMoveOnBoard(targetPosition.XCord + 1, targetPosition.YCord)) { targetPosition.XCord++; validMove = true; } }
                if (tryLeft && !validMove) { if (IsMoveOnBoard(targetPosition.XCord - 1, targetPosition.YCord)) { targetPosition.XCord--; validMove = true; } }

                
            }

            // A valid move was found

            if (!nextMoveIsACat) // Only move if the next move isn't a cat
            {
                // Clear the old cat point
                if (grid[theCat.XCord, theCat.YCord].Status == Point.PointStatus.CatAndCheese)
                {// set old spot back to cheese
                    grid[theCat.XCord, theCat.YCord] = new Point(theCat.XCord, theCat.YCord);
                    grid[theCat.XCord, theCat.YCord].Status = Point.PointStatus.Cheese;
                    // Set the cat back to just cat
                    targetPosition.Status = Point.PointStatus.Cat;
                }
                else
                {// set old spot back to empty
                    grid[theCat.XCord, theCat.YCord] = new Point(theCat.XCord, theCat.YCord);
                }

                // Check where the new move position is going to be
                if (grid[targetPosition.XCord, targetPosition.YCord].Status == Point.PointStatus.Mouse)
                {// Totally found the mouse
                    this.Mouse.hasBeenPouncedOn = true;
                    // The square should now hold the cat
                    grid[targetPosition.XCord, targetPosition.YCord] = targetPosition;
                    lazyCats.Remove(theCat);
                    lazyCats.Add((Cat)targetPosition);
                }
                else if (grid[targetPosition.XCord, targetPosition.YCord].Status == Point.PointStatus.Cheese)
                {// The cat is moving onto a cheese position
                    grid[targetPosition.XCord, targetPosition.YCord] = targetPosition;
                    grid[targetPosition.XCord, targetPosition.YCord].Status = Point.PointStatus.CatAndCheese;
                    lazyCats.Remove(theCat);
                    lazyCats.Add((Cat)targetPosition);
                }
                else
                { // Moved to an empty space
                    grid[targetPosition.XCord, targetPosition.YCord] = targetPosition;
                    lazyCats.Remove(theCat);
                    lazyCats.Add((Cat)targetPosition);
                }
            }
            else
            {
                // Reset this for the next cats so they can move
                nextMoveIsACat = false;
            }
        }
        /// <summary>
        /// A quick function to tell if a move will be
        /// within the board geometry
        /// </summary>
        /// <param name="x">future x position</param>
        /// <param name="y">future y position</param>
        /// <returns></returns>
        public bool IsMoveOnBoard(int x, int y)
        {
            if (x < 0 || y < 0) { return false; }
            if (x > this.grid.GetLength(0) || y > this.grid.GetLength(1)) { return false; }
            // If it's on the board, check if it's a valid move for the cat
            if (IsNextMoveACat(grid[x, y]))
            {
                nextMoveIsACat = true; // Update if the next move is another cat, to avoid an infinite loop
            } 
            return IsValidCatMove(grid[x, y]);
        }
        public bool IsValidCatMove(Point targetPosition)
        {
            return targetPosition.Status == Point.PointStatus.Empty || targetPosition.Status == Point.PointStatus.Mouse || targetPosition.Status == Point.PointStatus.Cheese;
        }

        public bool IsNextMoveACat(Point targetPosition)
        {
            return targetPosition.Status == Point.PointStatus.Cat || targetPosition.Status == Point.PointStatus.CatAndCheese;
        }

        /// <summary>
        /// Add a Cat to the lazycats list
        /// </summary>
        public void AddCat()
        {
            lazyCats.Add(PlaceCat());
        }

        public void PlaceCheese()
        {
            PlaceThing(Point.PointStatus.Cheese);
        }

        public Cat PlaceCat()
        {
            return (Cat)PlaceThing(Point.PointStatus.Cat);
        }

        private Point PlaceThing(Point.PointStatus thingToPlace)
        {
            int cX;
            int cY;
            do
            {
                cX = rng.Next(0, grid.GetLength(0));
                cY = rng.Next(0, grid.GetLength(1));
            } while (grid[cX, cY].Status != Point.PointStatus.Empty);
            // Found an empty point, place this type of object in the new place
            switch (thingToPlace)
            {
                case Point.PointStatus.Cheese:
                    grid[cX, cY].Status = Point.PointStatus.Cheese; return grid[cX, cY];
                case Point.PointStatus.Cat:
                    if (CheeseCount < 5)
                    {
                        grid[cX, cY] = new Cat(cX, cY, Cat.CatType.Kitten); return grid[cX, cY];
                    }
                    else if (rng.Next(0, 101) < 21)
                    {
                        // 20% chance of Tigers
                        grid[cX, cY] = new Cat(cX, cY, Cat.CatType.Tiger); return grid[cX, cY];
                    }

                    if (rng.Next(0, 101) < 31)
                    {   // 30% chance of kitten after the first two cats
                        grid[cX, cY] = new Cat(cX, cY, Cat.CatType.Kitten); return grid[cX, cY];
                    }
                    else
                    {   // 70% chance of housecat after the first two cats
                        grid[cX, cY] = new Cat(cX, cY, Cat.CatType.HouseCat); return grid[cX, cY];
                    }
            }
            return grid[cX, cY];
        }

        public void PlayGame()
        {
            while (this.Mouse.Energy > 0 && !this.Mouse.hasBeenPouncedOn)
            {
                this.DrawGrid();
                this.MoveMouse(this.GetUserMove());
                // Move all of the existing cats
                for (int i = 0; i < lazyCats.Count(); i++)
                {
                    this.MoveCat(lazyCats[i]);
                }
                this.Round++;
            }
            this.DrawGrid();
            // Display the results of the game
            if (this.Mouse.Energy == 0) { Console.WriteLine("You ran out of energy"); }
            else
            { Console.WriteLine("You lasted {0} rounds before getting caught by a cat.", this.Round);}
            Console.ReadLine();
        }

        public void DrawGrid()
        {
            Console.Clear();
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                for (int x = 0; x < grid.GetLength(0); x++)
                {
                    switch (grid[x, y].Status)
                    {
                        case Point.PointStatus.Empty: Console.Write("[ ]");
                            break;
                        case Point.PointStatus.Cheese:
                            Console.BackgroundColor = ConsoleColor.Yellow; Console.ForegroundColor = ConsoleColor.Black;
                            Console.Write("[C]"); Console.ResetColor();
                            break;
                        case Point.PointStatus.Mouse: Console.Write("[M]");
                            break;
                        case Point.PointStatus.Cat: Console.Write("[");
                            // Set a different color for a cat based on the difficulty of the cat
                            // kitten = green, housecat = yellow, tiger= red
                            Cat tempCat = (Cat)grid[x, y];
                            switch (tempCat.Type)
                            {
                                case Cat.CatType.Kitten: Console.ForegroundColor = ConsoleColor.Green; Console.Write("@"); break;
                                case Cat.CatType.HouseCat: Console.ForegroundColor = ConsoleColor.DarkYellow; Console.Write("@"); break;
                                case Cat.CatType.Tiger: Console.ForegroundColor = ConsoleColor.Red; Console.Write("@"); break;
                            }
                            Console.ResetColor();
                            Console.Write("]");
                            break;
                        case Point.PointStatus.CatAndCheese: Console.Write("[X]"); 
                            break;
                    }
                }
                // Write a new line at the end of the row
                Console.Write("\n");
            }
            // Display the mouse energy status at the bottom
            Console.WriteLine("Current mouse energy: {0}", this.Mouse.Energy);
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
                case ConsoleKey.DownArrow: return this.Mouse.YCord < grid.GetLength(1) - 1;
                case ConsoleKey.LeftArrow: return this.Mouse.XCord > 0;
                case ConsoleKey.RightArrow: return this.Mouse.XCord < grid.GetLength(0) - 1;
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
            // Decrease the energy on every move
            Mouse.Energy--;

            if (grid[x, y].Status == Point.PointStatus.Cat || grid[x, y].Status == Point.PointStatus.CatAndCheese)
            {
                // if the mouse tries to move at the cat, he dies
                this.Mouse.hasBeenPouncedOn = true;
                return false;
            }
            else if (grid[x, y].Status != Point.PointStatus.Cheese)
            {
                // No cheese found
                MoveMouseObject(x, y);
                return false;
            }
            else
            {
                // Cheese found
                CheeseCount++;
                // Give the mouse some energy for eating the cheese
                this.Mouse.Energy += 10;
                // Remove the found cheese
                grid[x, y].Status = Point.PointStatus.Empty;
                // Move the mouse to it's new position
                MoveMouseObject(x, y);
                // Place a new cheese
                PlaceCheese();

                if (CheeseCount % 2 == 0) { AddCat(); }

                return true;
            }
        }

        private void MoveMouseObject(int x, int y)
        {
            grid[Mouse.XCord, Mouse.YCord] = new Point(x, y); // Get rid of mouse
            Mouse.XCord = x; // update x coord
            Mouse.YCord = y; // update y coord
            grid[x, y] = Mouse; // Move mouse to new place
        }
    }

    class Cat : Point, ICloneable
    {
        public enum CatType
        {
            Kitten, HouseCat, Tiger
        }
        public CatType Type { get; set; }
        public Cat(int x, int y, CatType typeOfCat)
            : base(x, y)
        {
            this.Status = PointStatus.Cat;
            this.Type = typeOfCat;
        }
        // Needed to do a deep clone instead of a shallow clone
        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    class Mouse : Point
    {
        public int Energy { get; set; }
        public bool hasBeenPouncedOn { get; set; }
        /// <summary>
        /// Create a Mouse with an Energy off 50
        /// </summary>
        public Mouse(int x, int y)
            : base(x, y)
        {
            this.Status = PointStatus.Mouse;
            this.Energy = 50;
            this.hasBeenPouncedOn = false;
        }
    }

    /// <summary>
    /// Class representing the status of a certain point on the grid
    /// </summary>
    class Point
    {
        public enum PointStatus
        {
            Empty, Cheese, Mouse, Cat, CatAndCheese
        }
        public int XCord { get; set; }
        public int YCord { get; set; }
        public PointStatus Status { get; set; }
        public Point(int x, int y)
        {
            this.XCord = x;
            this.YCord = y;
            this.Status = PointStatus.Empty;
        }

    }
}

