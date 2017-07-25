using System;
using System.Threading;
using System.Windows;

namespace MineBot
{
    internal static class MainLogic
    {
        private const int Width = 64, Height = 48, MinesQty = 777;

        private static MainWindow Form { get; } = Application.Current.Windows[0] as MainWindow;

        private static readonly Cell[,] CellArray = new Cell[Height, Width];

        public static void Begin()
        {
            SetMines();
            UpdateField();
            StartSwiping();
        }

        private static void StartSwiping()
        {
            var r = new Random();
            int h = r.Next(Height), w = r.Next(Width);
            while (CellArray[h, w].IsMine)
            {
                h = r.Next(Height);
                w = r.Next(Width);
            }
            do
            {
                OpenCell(h, w);
                (h, w) = SeekCellForOpen();
            } while (true);
        }

        private static (int, int) SeekCellForOpen()
        {
            throw new NotImplementedException();
        }

        private static void OpenCell(int h, int w)
        {
            if (CellArray[h, w].IsMine)
                throw new Exception("BOOM");

            CellArray[h, w].IsOpened = true;
            UpdateField();
            Thread.Sleep(1000);
            UpdateProbabilities();
            SetFlags();
            UpdateField();
            if (CellArray[h, w].NeighbourMines == 0)
                OpenNeighbours(h, w);

        }

        private static void SetFlags()
        {
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    if (CellArray[i, j].IsOpened || CellArray[i, j].IsFlagged)
                        continue;

                    if (CellArray[i, j].MineProbability >= 1)
                    {
                        CellArray[i, j].IsFlagged = true;
                        UpdateProbabilities();
                    }
                }
            }
        }

        private static void UpdateProbabilities()
        {
            for (var i = 0; i < Height; i++)
                for (var j = 0; j < Width; j++)
                {
                    CellArray[i, j].MineProbability = 0;
                    CellArray[i, j].Neighbours = 0;
                }
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    if (!CellArray[i, j].IsOpened)
                        continue;

                    CountNeighbours(i, j);
                    AddProbability(i, j);
                }
            }
        }

        private static void AddProbability(int h, int w)
        {
            for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, Height); i++)
                for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, Width); j++)
                    if (CellArray[h, w].Neighbours > 0)
                        CellArray[i, j].MineProbability += (double)CellArray[h, w].NeighbourUnknownMines / CellArray[h, w].Neighbours;
        }

        private static void CountNeighbours(int h, int w)
        {
            CellArray[h, w].NeighbourUnknownMines = CellArray[h, w].NeighbourMines;
            for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, Height); i++)
                for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, Width); j++)
                {
                    if (CellArray[i, j].IsFlagged)
                        CellArray[h, w].NeighbourUnknownMines--;
                    if (!CellArray[i, j].IsOpened)
                        CellArray[h, w].Neighbours++;
                }
        }

        private static void OpenNeighbours(int h, int w)
        {
            for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, Height); i++)
                for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, Width); j++)
                    OpenCell(i, j);
        }

        private static void UpdateField()
        {
            var s = string.Empty;
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                    if (CellArray[i, j].IsOpened)
                        s += CellArray[i, j].IsMine ? "+" : CellArray[i, j].NeighbourMines.ToString();
                    else
                        s += "x";

                s += '\n';
            }

            Form.Field.Text = s;
        }

        private static void SetMines()
        {

            for (var i = 0; i < Height; i++)
                for (var j = 0; j < Width; j++)
                    CellArray[i, j] = new Cell();

            var r = new Random();
            var counter = 0;
            while (counter < MinesQty)
            {
                var h = r.Next(Height);
                var w = r.Next(Width);
                if (!CellArray[h, w].IsMine)
                {
                    CellArray[h, w].IsMine = true;
                    for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, Height); i++)
                        for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, Width); j++)
                        {
                            CellArray[i, j].NeighbourMines++;
                            CellArray[i, j].NeighbourUnknownMines++;
                        }

                    counter++;
                }
            }
        }
    }
    public class Cell
    {
        public bool IsOpened, IsMine, IsFlagged;
        public int NeighbourMines, NeighbourUnknownMines, Neighbours;
        public double MineProbability;

        public Cell()
        {
            IsOpened = false;
            MineProbability = 2;
        }

    }
}
