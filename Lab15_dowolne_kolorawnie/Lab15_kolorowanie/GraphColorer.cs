using System;
using System.Collections.Generic;
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
            
            bool found = FindColoringRec(g, 0,ref coloring,ref colored, ref colorsCount, ref visited);
            if (found)
            {
                return (colorsCount, coloring);
            }
            
            return (0, null);
        }
        
        private bool FindColoringRec(Graph g, int v,ref int[] coloring,ref bool[] colored, ref int colorsCount, ref bool[] visited)
        {
            
            // if all vertices are colored
            if (v == g.VertexCount - 1)
            {
                return true;
            }
            foreach(int w in g.OutNeighbors(v))
            {
                if (colored[w] == false)
                {
                    // ktory kolor wolny ?
                    bool foundColor = false;
                    for (int color = 0; color < colorsCount; color++)
                    {
                        // czy mozna pomalowac w  w kolorze i
                        if (IsSafe(g, w, color, coloring))
                        {
                            foundColor = true;
                            coloring[w] = color;
                            colored[w] = true;
                            visited[w] = true;
                            if (FindColoringRec(g, w, ref coloring, ref colored, ref colorsCount, ref visited))
                            {
                                return true;
                            }
                            colored[w] = false;
                            visited[w] = false;
                            coloring[w] = -1;
                        }
                    }

                    if (!foundColor)
                    {
                        colorsCount++;
                        coloring[w] = colorsCount - 1;
                        colored[w] = true;
                        if (FindColoringRec(g, w, ref coloring, ref colored, ref colorsCount, ref visited))
                        {
                            return true;
                        }
                        colorsCount--;
                        colored[w] = false;
                        coloring[w] = -1;
                    }
                }
            }
            for(int i = 0; i < g.VertexCount; i++)
            {
                
                if (!visited[i])
                {
                    for (int color = 0; color < colorsCount; color++)
                    {
                        if (IsSafe(g, i, color, coloring))
                        {
                            coloring[i] = color;
                            colored[i] = true;
                            visited[i] = true;
                            if (FindColoringRec(g, i, ref coloring, ref colored, ref colorsCount, ref visited))
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
            
            return false;
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