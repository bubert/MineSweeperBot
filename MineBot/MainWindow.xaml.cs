using System;
using System.Collections.Generic;
using System.Windows;

namespace MineBot
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    internal partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_OnClick(object sender, RoutedEventArgs e)
        {
            MainLogic.Begin();
        }

        static class MainLogic
        {
            private const int FieldWidth = 64, FieldHeight = 48, MinesQty = 777;

            private static MainWindow Form { get; } = Application.Current.Windows[0] as MainWindow;

            private static readonly Cell[,] CellArray = new Cell[FieldHeight, FieldWidth];

            public static void Begin()
            {
                SetMines();
                UpdateField();
                StartSwiping();
            }

            private static void StartSwiping()
            {
                var r = new Random();
                int h = r.Next(FieldHeight), w = r.Next(FieldWidth);
                while (CellArray[h, w].IsMine)
                {
                    h = r.Next(FieldHeight);
                    w = r.Next(FieldWidth);
                }

                OpenCell(h, w);
            }

            public static (int, int) SeekCellForOpen()
            {
                double minProbability = 20;
                for (var i = 0; i < FieldHeight; i++)
                {
                    for (var j = 0; j < FieldWidth; j++)
                    {
                        if (CellArray[i, j].IsOpened || CellArray[i, j].IsFlagged)
                            continue;

                        minProbability = Math.Min(minProbability, CellArray[i, j].MineProbability);
                    }
                }

                var list = new List<(int, int)>();
                for (var i = 0; i < FieldHeight; i++)
                {
                    for (var j = 0; j < FieldWidth; j++)
                    {
                        if (Math.Abs(minProbability - CellArray[i, j].MineProbability) < 0.001 && !(CellArray[i, j].IsOpened || CellArray[i, j].IsFlagged))
                            list.Add((i, j));
                    }
                }

                var r = new Random();
                return list[r.Next(list.Count)];
            }

            internal static void OpenCell(int h, int w)
            {
                if (CellArray[h, w].IsMine)
                {
                    MessageBox.Show("BOOM");
                    return;
                }

                CellArray[h, w].IsOpened = true;
                UpdateField();
                UpdateProbabilities();
                UpdateField();
                if (CellArray[h, w].NeighbourMines == 0 && CellArray[h, w].Neighbours != 0)
                    OpenNeighbours(h, w);

            }

            private static void SetFlags()
            {
                for (var i = 0; i < FieldHeight; i++)
                {
                    for (var j = 0; j < FieldWidth; j++)
                    {
                        if (CellArray[i, j].IsOpened || CellArray[i, j].IsFlagged)
                            continue;

                        if (CellArray[i, j].MineProbability >= 1 && CellArray[i, j].MineProbability < 20 && !CellArray[i, j].IsOpened)
                        {
                            CellArray[i, j].IsFlagged = true;
                            UpdateProbabilities();
                        }
                    }
                }
            }

            private static void UpdateProbabilities()
            {
                for (var i = 0; i < FieldHeight; i++)
                    for (var j = 0; j < FieldWidth; j++)
                    {
                        CellArray[i, j].MineProbability = 0;
                        CellArray[i, j].Neighbours = 0;
                        CellArray[i, j].FlaggedNeighbours = 0;
                    }

                for (var i = 0; i < FieldHeight; i++)
                    for (var j = 0; j < FieldWidth; j++)
                        CountNeighbours(i, j);

                for (var i = 0; i < FieldHeight; i++)
                    for (var j = 0; j < FieldWidth; j++)
                    {
                        if (CellArray[i, j].IsOpened && CellArray[i, j].Neighbours > 0 && CellArray[i, j].FlaggedNeighbours == CellArray[i, j].NeighbourMines)
                            FlagNeighbours(i, j);
                        if (CellArray[i, j].Neighbours != 0 && CellArray[i, j].IsOpened)
                            AddProbability(i, j);
                    }
                for (var i = 0; i < FieldHeight; i++)
                    for (var j = 0; j < FieldWidth; j++)
                        if (CellArray[i, j].MineProbability == 0)
                            CellArray[i, j].MineProbability = 20;

                SetFlags();
            }

            private static void FlagNeighbours(int h, int w)
            {
                for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, FieldHeight); i++)
                    for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, FieldWidth); j++)
                        if ((h != i || w != j) && !CellArray[i, j].IsOpened)
                            CellArray[i, j].IsFlagged = true;
            }

            private static void AddProbability(int h, int w)
            {
                for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, FieldHeight); i++)
                    for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, FieldWidth); j++)
                        if (h != i || w != j)
                            if (CellArray[h, w].Neighbours > 0 && !CellArray[i, j].IsOpened)
                                CellArray[i, j].MineProbability += (double)CellArray[h, w].NeighbourMines /
                                                                   CellArray[h, w].Neighbours;
            }
            //TODO: сделать учет отмеченных мин при вычислении вероятности
            private static void CountNeighbours(int h, int w)
            {
                for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, FieldHeight); i++)
                    for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, FieldWidth); j++)
                        if (h != i || w != j)
                            if (!CellArray[i, j].IsOpened)
                            {
                                CellArray[h, w].Neighbours++;
                                if (CellArray[i, j].IsFlagged)
                                    CellArray[i, j].FlaggedNeighbours++;
                            }
            }

            private static void OpenNeighbours(int h, int w)
            {
                for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, FieldHeight); i++)
                    for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, FieldWidth); j++)
                        if ((h != i || w != j) && !CellArray[i, j].IsOpened)
                            OpenCell(i, j);
            }

            private static void UpdateField()
            {
                var s = string.Empty;
                for (var i = 0; i < FieldHeight; i++)
                {
                    for (var j = 0; j < FieldWidth; j++)
                    {
                        if (CellArray[i, j].IsOpened)
                            s += CellArray[i, j].NeighbourMines == 0 ? " " : CellArray[i, j].NeighbourMines.ToString();
                        else
                            s += CellArray[i, j].IsFlagged ? "+" : "x";
                    }
                    s += '\n';
                }

                Form.Field.Text = s;
                UpdateField2();
            }

            private static void UpdateField2()
            {
                var s = string.Empty;
                for (var i = 0; i < FieldHeight; i++)
                {
                    for (var j = 0; j < FieldWidth; j++)
                    {
                        if (CellArray[i, j].IsFlagged)
                            s += CellArray[i, j].IsMine ? "+" : "-";
                        else
                            s += CellArray[i, j].IsOpened ? " " : "x";
                    }
                    s += '\n';
                }

                Form.Field2.Text = s;
            }

            private static void SetMines()
            {

                for (var i = 0; i < FieldHeight; i++)
                    for (var j = 0; j < FieldWidth; j++)
                        CellArray[i, j] = new Cell();

                var r = new Random();
                var counter = 0;
                while (counter < MinesQty)
                {
                    var h = r.Next(FieldHeight);
                    var w = r.Next(FieldWidth);
                    if (!CellArray[h, w].IsMine)
                    {
                        CellArray[h, w].IsMine = true;
                        for (var i = Math.Max(h - 1, 0); i < Math.Min(h + 2, FieldHeight); i++)
                            for (var j = Math.Max(w - 1, 0); j < Math.Min(w + 2, FieldWidth); j++)
                                if (h != i || w != j)
                                    CellArray[i, j].NeighbourMines++;

                        counter++;
                    }
                }
            }
        }

        private class Cell
        {
            public bool IsOpened, IsMine, IsFlagged;
            public int NeighbourMines, Neighbours, FlaggedNeighbours;
            public double MineProbability;

            public Cell()
            {
                IsOpened = false;
                MineProbability = 20;
            }

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var (h, w) = MainLogic.SeekCellForOpen();
            MainLogic.OpenCell(h, w);
        }
    }
}

