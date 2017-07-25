using System;
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
            throw new NotImplementedException();
        }

        private static void OpenCell(int h, int w)
        {
            
        }
        public static void UpdateField()
        {
            var s = string.Empty;
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    if (CellArray[i, j].IsOpened)
                        s += CellArray[i, j].IsMine ? "+" : CellArray[i, j].NeighboursQty.ToString();
                    else
                        s += "x";
                }
                s += '\n';
            }
            Form.Field.Text = s;
        }

        public static Cell[,] SetMines()
        {
            
            for (var i = 0; i < Height; i++)
            {
                for (var j = 0; j < Width; j++)
                {
                    CellArray[i, j] = new Cell();
                }
            }
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
                    {
                        for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, Width); j++)
                        {
                            CellArray[i, j].NeighboursQty++;
                        }
                    }
                    counter++;
                }
            }
            return CellArray;
        }
    }
    public class Cell
    {
        public bool IsOpened, IsMine;
        public int NeighboursQty;
        public double MineProbability;

        public Cell()
        {
            IsOpened = false;
            MineProbability = 1;
        }
    }
}
