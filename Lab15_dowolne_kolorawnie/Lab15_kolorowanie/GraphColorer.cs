using System;
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;

namespace ASD2
{
    public class GraphColorer : MarshalByRefObject
    {
        /// <summary>
        /// Metoda znajduje kolorowanie zadanego grafu g używające najmniejsze możliwej liczby kolorów.
        /// </summary>
        /// <param name="g">Graf (nieskierowany)</param>
        /// <returns>Liczba użytych kolorów i kolorowanie (coloring[i] to kolor wierzchołka i). Kolory mogą być dowolnymi liczbami całkowitymi.</returns>
        public (int numberOfColors, int[] coloring) FindBestColoring(Graph g)
        {
            
            
            int[] smallestColoring = null;
            int smallestColoringColorsCount = g.VertexCount;
            List<int>[] colorsAvailable = new List<int>[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
            {
                colorsAvailable[i] = new List<int>();
            }
            int[] coloring = new int[g.VertexCount];
            // initialice coloring as -1 
            for (int i = 0; i < g.VertexCount; i++)
            {
                coloring[i] = -1;
            }

            bool[] visited = new bool[g.VertexCount];
            bool[] colored = new bool[g.VertexCount];
            int colorsCount = 0;

            
            int[] notColoredNeighboursCount = new int[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
            {
                notColoredNeighboursCount[i] = g.Degree(i);
            }

            colored[0] = true;
            coloring[0] = 0;
            visited[0] = true;
            colorsCount++;
            bool[] allneighboursColored = new bool[g.VertexCount];
            
            List<int>[] neighboursSorted = new List<int>[g.VertexCount];
            for (int i = 0; i < g.VertexCount; i++)
            {
                neighboursSorted[i] = new List<int>();
                foreach (int neighbour in g.OutNeighbors(i))
                {
                    neighboursSorted[i].Add(neighbour);
                }
                neighboursSorted[i].Sort((x, y) => g.Degree(x).CompareTo(g.Degree(y)));
            }
            
            FindColoringRec(ref g, 0,ref coloring,ref colored, ref colorsCount, ref visited, ref allneighboursColored, ref smallestColoring , ref smallestColoringColorsCount, ref colorsAvailable, ref neighboursSorted);
           

            return (smallestColoringColorsCount, smallestColoring);
        }
        
        private void FindColoringRec(ref Graph g, int v,ref int[] coloring,ref bool[] colored,
            ref int colorsCount, ref bool[] visited, ref bool[] allNeighboursColored,
            ref int[] smallestColoring, ref int smallestColoringColorsCunt, ref List<int>[] colorsAvailable,
            ref List<int>[] neighboursSorted)
        {
            // warunkiem koncowym jest, że wszystkie wierzchołki są pokolorowane
            if (colored.All(x => x == true))
            {
                if (colorsCount <= smallestColoringColorsCunt)
                {
                    smallestColoringColorsCunt = colorsCount;
                    smallestColoring = coloring.ToArray();
                }
                return;
            }
            // jesli liczba kolorow jest wieksza niz najmniejsza znaleziona to przerwij
            if (colorsCount > smallestColoringColorsCunt)
            {
                return;
            }


            foreach(int w in neighboursSorted[v])
            {
                if (colored[w] == false)
                {
                    // 1. policz ile niepokolororwanych sasiadow
                    int notColoredNeighbours = 0;
                    foreach (int neighbour in neighboursSorted[w])
                    {
                        if (!colored[neighbour])
                        {
                            notColoredNeighbours++;
                        }
                    }
                    // 2. Policz liczbe dostepnych kolorow dla wierzcholka w
                    int availableColors = 0;
                    for (int color = 0; color < colorsCount; color++)
                    {
                        if (IsSafe(color, coloring, ref neighboursSorted[w]))
                        {
                            colorsAvailable[w].Add(color);
                            availableColors++;
                        }
                        else
                        {
                            colorsAvailable[w].Remove(color);
                        }
                    }

                    // Jeżeli liczba kolorów dostępnych dla jakiegoś wierzchołka w
                    // jest wieksza niż liczba jego niepokolorowanych sąsiadów,
                    // możemy usunąć wierzchołek v z dalszych rozważań (i -
                    // jeżeli udało się pokolorować pozostałe wierzchołki
                    //  - dokolorować v na końcu)
                    if (availableColors > notColoredNeighbours)
                    {
                        continue;
                    }
                    
                    if(notColoredNeighbours == 0)
                    {
                        allNeighboursColored[w] = true;
                    }
                    
                    
                    // ktory kolor wolny ?
                    if (availableColors == 0)
                    {
                        colorsCount++;
                        coloring[w] = colorsCount - 1;
                        colored[w] = true;
                        visited[w] = true;
                        if (allNeighboursColored[w] == false)
                        {
                            FindColoringRec(ref g, w, ref coloring, ref colored, ref colorsCount, ref visited,
                                ref allNeighboursColored, ref smallestColoring, ref smallestColoringColorsCunt,
                                ref colorsAvailable, ref neighboursSorted);
                            colored[w] = false;
                            visited[w] = false;
                            coloring[w] = -1;
                        }
                       
                    }
                    else
                    {
                        foreach (int color in colorsAvailable[w])
                        {
                            coloring[w] = color;
                            colored[w] = true;
                            visited[w] = true;
                            if (allNeighboursColored[w] == false)
                            {
                                FindColoringRec(ref g, w, ref coloring, ref colored, ref colorsCount, ref visited,
                                    ref allNeighboursColored, ref smallestColoring, ref smallestColoringColorsCunt,
                                    ref colorsAvailable, ref neighboursSorted);
                                colored[w] = false;
                                visited[w] = false;
                                coloring[w] = -1;
                            }
                        }
                    }
                }
            }
            allNeighboursColored[v] = true;
           
            // jesli jakis wierzcholek nie zostal odwiedzony to koloruj 
            for (int i = 0; i < g.VertexCount; i++)
            {
               // dla kazdego mozliwego koloru wykonaj kolorowanie 
                if (visited[i] == false)
                {
                    for (int color = 0; color < colorsCount; color++)
                    {
                        if (IsSafe(color, coloring, ref neighboursSorted[i]))
                        {
                            coloring[i] = color;
                            colored[i] = true;
                            visited[i] = true;
                            if (allNeighboursColored[i] == false)
                            {
                                FindColoringRec(ref g, i, ref coloring, ref colored, ref colorsCount, ref visited,
                                    ref allNeighboursColored, ref smallestColoring, ref smallestColoringColorsCunt,
                                    ref colorsAvailable, ref neighboursSorted);
                                colored[i] = false;
                                visited[i] = false;
                                coloring[i] = -1;
                            }
                        }
                    }
                }
            }
            // warunkiem koncowym jest, że wszystkie wierzchołki są pokolorowane
            if (colored.All(x => x == true))
            {
                if (colorsCount <= smallestColoringColorsCunt)
                {
                    smallestColoringColorsCunt = colorsCount;
                    smallestColoring = coloring.ToArray();
                }
            }
        }

        private bool IsSafe(int color, int[] coloring, ref List<int> neighboursSorted)
        {
            // czy mozna pomalowac vetex w kolorze color
            foreach (int neighbour in neighboursSorted)
            {
                if (coloring[neighbour] == color)
                {
                    return false;
                }
            }
            return true;
        }
    }
}