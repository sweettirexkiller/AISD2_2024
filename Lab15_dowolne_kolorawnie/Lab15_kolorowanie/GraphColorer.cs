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
            bool[] considerAtEnd = new bool[g.VertexCount];
            int colorsCount = 0;


            colored[0] = true;
            coloring[0] = 0;
            visited[0] = true;
            colorsCount++;
            bool[] allneighboursColored = new bool[g.VertexCount];
            
            bool found = FindColoringRec(g, 0,ref coloring,ref colored, ref colorsCount, ref visited, ref considerAtEnd, ref allneighboursColored);
            // sprawdz czy ktorys nieodwiedzony wierzcholek
            for (int i = 0; i < g.VertexCount; i++)
            {
                if (visited[i] == false || colored[i] == false || coloring[i] == -1)
                {
                    if (i == 6)
                    {
                        int x;
                    }
                    found = FindColoringRec(g, i,ref coloring,ref colored, ref colorsCount, ref visited, ref considerAtEnd, ref allneighboursColored);
                }
            }
            
            if (found)
            {
                return (colorsCount, coloring);
            }
            
            return (0, null);
        }
        
        private bool FindColoringRec(Graph g, int v,ref int[] coloring,ref bool[] colored, ref int colorsCount, ref bool[] visited, ref bool[] considerAtEnd, ref bool[] allneighboursColored)
        {
            if(g.OutNeighbors(v).Count() == 0)
            {
                visited[v] = true;
                coloring[v] = 0;
                colored[v] = true;
                return true;
            }
            
            if(allneighboursColored[v])
            {
                // znajdz najmniejszy mozliwy kolor
                for (int color = 0; color < colorsCount; color++)
                {
                    if (IsSafe(g, v, color, coloring))
                    {
                        coloring[v] = color;
                        colored[v] = true;
                        visited[v] = true;
                        return true;
                    }
                }
            }
            
            // if all vertices are colored
            if (v == g.VertexCount - 1)
            {
                // czy udało się pokolorować pozostałe wierzchołki?
                
                for (int i = 0; i < g.VertexCount; i++)
                {
                    if (considerAtEnd[i] == true)
                    {
                        // czy udało się pokolorować pozostałe wierzchołki?
                        // pokoloruj na najmniejszy mozliwy kolor
                        considerAtEnd[i] = false;
                        for (int color = 0; color < colorsCount; color++)
                        {
                            if (IsSafe(g, i, color, coloring))
                            {
                                coloring[i] = color;
                                colored[i] = true;
                                visited[i] = true;
                                if (FindColoringRec(g, i, ref coloring, ref colored, ref colorsCount, ref visited, ref considerAtEnd, ref allneighboursColored))
                                {
                                    return true;
                                }
                                colored[i] = false;
                                visited[i] = false;
                                coloring[i] = -1;
                            }
                        }
                    }
                }
                return true;
            }
            if(g.OutNeighbors(v).Count() == 0)
            {
                visited[v] = true;
                coloring[v] = 0;
                colored[v] = true;
                return true;
            }
            foreach(int w in g.OutNeighbors(v))
            {
                if (considerAtEnd[w])
                {
                    continue;
                }
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
                    
                    if(notColoredNeighbours == 0)
                    {
                        allneighboursColored[w] = true;
                    }
                    
                    if (availableColors > notColoredNeighbours && notColoredNeighbours > 0)
                    {
                        visited[w] = true;
                        considerAtEnd[w] = true;
                        continue;
                    }
                    
                    // ktory kolor wolny ?
                    if (availableColors == 0)
                    {
                        colorsCount++;
                        coloring[w] = colorsCount - 1;
                        colored[w] = true;
                        visited[w] = true;
                        if (FindColoringRec(g, w, ref coloring, ref colored, ref colorsCount, ref visited, ref considerAtEnd, ref allneighboursColored))
                        {
                            
                            return true;
                        }
                        colorsCount--;
                        colored[w] = false;
                        visited[w] = false;
                        coloring[w] = -1;
                        continue;
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
                                    if (FindColoringRec(g, w, ref coloring, ref colored, ref colorsCount, ref visited, ref considerAtEnd, ref allneighboursColored))
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