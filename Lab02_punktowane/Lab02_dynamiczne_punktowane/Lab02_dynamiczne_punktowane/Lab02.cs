using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Lab02 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - wyznaczenie najtańszej trasy, zgodnie z którą pionek przemieści się z pozycji poczatkowej (0,0) na pozycję docelową
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="moves">tablica z dostępnymi ruchami i ich kosztami (di - o ile zwiększamy numer wiersza, dj - o ile zwiększamy numer kolumnj, cost - koszt ruchu)</param>
        /// <returns>(bool result, int cost, (int, int)[] path) - result ma wartość true jeżeli trasa istnieje, false wpp., cost to minimalny koszt, path to wynikowa trasa</returns>
        public (bool result, int cost, (int i, int j)[] path) Lab02Stage1(int n, int m, ((int di, int dj) step, int cost)[] moves)
        {
            // w reachable zapisujemy inteks ruchu, którym osiągnęliśmy dany punkt
            // -1 nieosiągalne
            // 0 - ruchem step[0]
            // 1 - ruchem step[1] ... etc
            // int.maxValue - 1 początkowe pole
            int[,] reachable = new int[n, m];
            int[,] cost = new int[n, m];
            
            // zainicjalizuj tablicę reachable na wartosciami  -1
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    reachable[i, j] = -1;
                   // cost[i,j] = int.MaxValue/2;
                }
            }


            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                   
                    if(i == 0 && j == 0)
                    {
                        // dodajemy wszystkie mozliwe poczatkowe ruchy z (0,0)
                        reachable[i, j] = int.MaxValue / 2;
                        int moveIndex = 0;
                        foreach (var move in moves)
                        {
                            int newI = i + move.step.di;
                            int newJ = j + move.step.dj;
                            if (newI >= 0 && newI < n && newJ >= 0 && newJ < m)
                            {
                                reachable[newI, newJ] = moveIndex;
                                cost[newI, newJ] = move.cost;
                            }
                            moveIndex++;
                        }
                        continue;
                    }
                    else 
                    {
                        if(reachable[i, j] != -1) // to znaczy ze dalo sie wejsc do tego pola w ruchu wczesniejszym
                        {
                            int moveIndex = 0;
                            foreach (var move in moves) // sprawdzamy wszystkie kolejne ruchy
                            {
                                int newI = i + move.step.di;
                                int newJ = j + move.step.dj;
                                // jesli ten ruch nie wyjdzie z planszy
                                if (newI >= 0 && newI < n && newJ >= 0 && newJ < m)
                                {
                                    // jesli wczesniejsze pole bylo nieosiagalne to inicjujemy ze mozemy wejsc do tego pola
                                    if (reachable[newI, newJ] == -1)
                                    {
                                        reachable[newI, newJ] = moveIndex;
                                        cost[newI, newJ] = cost[i, j] + move.cost;
                                    }
                                    else // jesli byl osiagalny i jego nowy koszt bylby mniejszy to aktualizujemy
                                    {
                                        if (cost[newI, newJ] > cost[i, j] + move.cost)
                                        {
                                            reachable[newI, newJ] = moveIndex;
                                            cost[newI, newJ] = cost[i, j] + move.cost;
                                        }
                                    }
                                }
                                moveIndex++;
                            }
                        }
                        
                    }
                }

            }
                
            
            // sprawdzamy czy w ostatnim wierszu jest jakies pole osiagalne
            int minCost = int.MaxValue;
            int minCostIndex = -1;
            int reachedRow = n - 1;
            int reachedCol = -1;
            for (int j = 0; j < m; j++)
            {
                if (reachable[n - 1, j] != -1)
                {
                    if (cost[n - 1, j] < minCost)
                    {
                        minCost = cost[n - 1, j];
                        minCostIndex = j;
                        reachedCol = j;
                    }
                }
            }
            
            
            if (minCostIndex == -1)
            {
                return (false, int.MaxValue, null);
            }
            else
            {
                List<(int, int)> path = new List<(int, int)>();
                int i = reachedRow;
                int j = reachedCol;
                while (i != 0 || j != 0)
                {
                    path.Add((i, j));
                    int moveIndex = reachable[i, j];
                    i -= moves[moveIndex].step.di;
                    j -= moves[moveIndex].step.dj;
                }
                path.Add((0, 0));
                path.Reverse();
                return (true, minCost, path.ToArray());
            }
            
        }


        /// <summary>
        /// Etap 2 - wyznaczenie najtańszej trasy, zgodnie z którą pionek przemieści się z pozycji poczatkowej (0,0) na pozycję docelową - dodatkowe założenie, każdy ruch może być wykonany co najwyżej raz
        /// </summary>
        /// <param name="n">wysokość prostokąta</param>
        /// <param name="m">szerokość prostokąta</param>
        /// <param name="moves">tablica z dostępnymi ruchami i ich kosztami (di - o ile zwiększamy numer wiersza, dj - o ile zwiększamy numer kolumnj, cost - koszt ruchu)</param>
        /// <returns>(bool result, int cost, (int, int)[] path) - result ma wartość true jeżeli trasa istnieje, false wpp., cost to minimalny koszt, path to wynikowa trasa</returns>
        public (bool result, int cost, (int i, int j)[] pat) Lab02Stage2(int n, int m, ((int di, int dj) step, int cost)[] moves)
        {
            // w reachable zapisujemy inteks ruchu, którym osiągnęliśmy dany punkt
            // -1 nieosiągalne
            // 0 - ruchem step[0]
            // 1 - ruchem step[1] ... etc
            // int.maxValue - 1 początkowe pole
            int[,] reachable = new int[n, m];
            int[,] cost = new int[n, m];
            // w tej tablicy zapisujemy ile razy dany ruch zostal juz uzyty
            int[,,] usageCount = new int[n, m, moves.Length];
            
            // zainicjalizuj tablicę reachable na wartosciami  -1
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    reachable[i, j] = -1;
                    // cost[i,j] = int.MaxValue/2;
                }
            }


            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < m; j++)
                {
                   
                    if(i == 0 && j == 0)
                    {
                        // dodajemy wszystkie mozliwe poczatkowe ruchy z (0,0)
                        reachable[i, j] = int.MaxValue / 2;
                        int moveIndex = 0;
                        foreach (var move in moves)
                        {
                            int newI = i + move.step.di;
                            int newJ = j + move.step.dj;
                            if (newI >= 0 && newI < n && newJ >= 0 && newJ < m)
                            {
                                reachable[newI, newJ] = moveIndex;
                                cost[newI, newJ] = move.cost;
                                usageCount[newI, newJ, moveIndex] = 1;
                            }
                            moveIndex++;
                        }
                        continue;
                    }
                    else 
                    {
                        if(reachable[i, j] != -1) // to znaczy ze dalo sie wejsc do tego pola w ruchu wczesniejszym
                        {
                            // ktorych ruchow juz uzylismy zeby dojsc do tego pola
                            int[] movesUsed = new int[moves.Length];
                            for (int k = 0; k < moves.Length; k++)
                            {
                                movesUsed[k] = usageCount[i, j, k];
                            }
                            int moveIndex = 0;
                            foreach (var move in moves) // sprawdzamy wszystkie kolejne ruchy
                            {
                                // czy ten ruch nie zostal juz raz wykonany ?
                                if (movesUsed[moveIndex] == 0)
                                {
                                    int newI = i + move.step.di;
                                    int newJ = j + move.step.dj;
                                    // jesli ten ruch nie wyjdzie z planszy
                                    if (newI >= 0 && newI < n && newJ >= 0 && newJ < m)
                                    {
                                        // jesli wczesniejsze pole bylo nieosiagalne to inicjujemy ze mozemy wejsc do tego pola
                                        if (reachable[newI, newJ] == -1)
                                        {
                                            reachable[newI, newJ] = moveIndex;
                                            cost[newI, newJ] = cost[i, j] + move.cost;
                                            
                                            // skopiuj usageCount z poprzedniego pola
                                            for (int k = 0; k < moves.Length; k++)
                                            {
                                                usageCount[newI, newJ, k] = usageCount[i, j, k];
                                            }
                                            usageCount[newI, newJ, moveIndex] = 1;
                                        }
                                        else // jesli byl osiagalny i jego nowy koszt bylby mniejszy to aktualizujemy
                                        {
                                            if (cost[newI, newJ] > cost[i, j] + move.cost)
                                            {
                                                reachable[newI, newJ] = moveIndex;
                                                cost[newI, newJ] = cost[i, j] + move.cost;
                                                // skopiuj usageCount z poprzedniego pola
                                                for (int k = 0; k < moves.Length; k++)
                                                {
                                                    usageCount[newI, newJ, k] = usageCount[i, j, k];
                                                }
                                                usageCount[newI, newJ, moveIndex] = 1;
                                            }
                                        }
                                    }
                                    
                                }
                                moveIndex++;
                                
                            }
                        }
                        
                    }
                }

            }
                
            // sprawdzamy czy w ostatnim wierszu jest jakies pole osiagalne
            int minCost = int.MaxValue;
            int minCostIndex = -1;
            int reachedRow = n - 1;
            int reachedCol = -1;
            for (int j = 0; j < m; j++)
            {
                if (reachable[n - 1, j] != -1)
                {
                    if (cost[n - 1, j] < minCost)
                    {
                        minCost = cost[n - 1, j];
                        minCostIndex = j;
                        reachedCol = j;
                    }
                }
            }
            
            
            if (minCostIndex == -1)
            {
                return (false, int.MaxValue, null);
            }
            else
            {
                List<(int, int)> path = new List<(int, int)>();
                int i = reachedRow;
                int j = reachedCol;
                while (i != 0 || j != 0)
                {
                    path.Add((i, j));
                    int moveIndex = reachable[i, j];
                    i -= moves[moveIndex].step.di;
                    j -= moves[moveIndex].step.dj;
                }
                path.Add((0, 0));
                path.Reverse();
                return (true, minCost, path.ToArray());
            }
           

        }
    }
}