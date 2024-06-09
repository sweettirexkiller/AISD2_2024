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
            
            int[] coloring = new int[g.VertexCount];
            // initialice coloring as -1 
            for (int i = 0; i < g.VertexCount; i++)
            {
                coloring[i] = -1;
            }

            bool[] visited = new bool[g.VertexCount];
            bool[] colored = new bool[g.VertexCount];
            int colorsCount = 0;


            colored[0] = true;
            coloring[0] = 0;
            visited[0] = true;
            colorsCount++;
            bool[] allneighboursColored = new bool[g.VertexCount];
            
            bool found = FindColoringRec(g, 0,ref coloring,ref colored, ref colorsCount, ref visited, ref allneighboursColored);
           // czy wszystkie pokoolorowane
            for (int i = 0; i < g.VertexCount; i++)
            {

                if (visited[i] == false)
                {
                    // znajdz najmniejszy kolor
                    for (int color = 0; color < colorsCount; color++)
                    {
                        if (IsSafe(g, i, color, coloring))
                        {
                            coloring[i] = color;
                            colored[i] = true;
                            visited[i] = true;
                            found = FindColoringRec(g, i,ref coloring,ref colored, ref colorsCount, ref visited, ref allneighboursColored);
                            break;
                        }
                    }
                }
            }

            if (found)
            {
                return (colorsCount, coloring);
            }
            
            return (0, null);
        }
        
        private bool FindColoringRec(Graph g, int v,ref int[] coloring,ref bool[] colored, ref int colorsCount, ref bool[] visited, ref bool[] allneighboursColored)
        {
            // jesli jestesmy na koncu to zwroc true, bo to znaczy ze doszlismy do ostatniego wierzcholka
            if (v == g.VertexCount - 1)
            {
                return true;
            }
            
            // wez sasiadow i posortuj malejaco wedlug liczby sasiadow
            // List<int> neighbours = new List<int>();
            // foreach (int w in g.OutNeighbors(v))
            // {
            //     neighbours.Add(w);
            // }
            // neighbours.Sort((x, y) => g.OutNeighbors(y).Count().CompareTo(g.OutNeighbors(x).Count()));
            //


            foreach(int w in g.OutNeighbors(v))
            {
                if (colored[w] == false)
                {
                    // 1. policz ile niepokolororwanych sasiadow
                    int notColoredNeighbours = 0;
                    foreach (int neighbour in g.OutNeighbors(w))
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
                        if (IsSafe(g, w, color, coloring))
                        {
                            availableColors++;
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
                        allneighboursColored[w] = true;
                    }
                    
                    
                    // ktory kolor wolny ?
                    if (availableColors == 0)
                    {
                        colorsCount++;
                        coloring[w] = colorsCount - 1;
                        colored[w] = true;
                        visited[w] = true;
                        if (allneighboursColored[w] == false)
                        {
                            if (FindColoringRec(g, w, ref coloring, ref colored, ref colorsCount, ref visited, ref allneighboursColored))
                            {
                                return true;
                            }
                            colored[w] = false;
                            visited[w] = false;
                            coloring[w] = -1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        for (int color = 0; color < colorsCount; color++)
                        {
                            // czy mozna pomalowac w  w kolorze i
                            if (IsSafe(g, w, color, coloring))
                            {
                                coloring[w] = color;
                                colored[w] = true;
                                visited[w] = true;
                                if (allneighboursColored[w] == false)
                                {
                                    if (FindColoringRec(g, w, ref coloring, ref colored, ref colorsCount, ref visited, ref allneighboursColored))
                                    {
                                        return true;
                                    }
                                    colored[w] = false;
                                    visited[w] = false;
                                    coloring[w] = -1;
                                }
                                else
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            allneighboursColored[v] = true;

            return true;
        }

        private bool IsSafe(Graph graph, int vertex, int color, int[] coloring)
        {
            // czy mozna pomalowac vetex w kolorze color
            foreach (int neighbour in graph.OutNeighbors(vertex))
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