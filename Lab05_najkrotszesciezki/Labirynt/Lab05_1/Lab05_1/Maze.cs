using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASD
{
    public class Maze : MarshalByRefObject
    {



        private (int? i, int? j) start = (null, null);
        private (int? i, int? j) end = (null, null);

        // private List<(int, int)> multiLevelEnds = new List<(int, int)>();

        public DiGraph<int> MazeToGraph(char[,] maze, bool maszDynamit, int czasBurzenia)
        {
            int height = maze.GetLength(0);
            int width = maze.GetLength(1);

            DiGraph<int> graph = new DiGraph<int>(height * width);

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (maze[i, j] == 'S') // start point
                    {
                        this.start = (i, j);
                        
                        int from = i * width + j;
                        
                        // krwedz w prawo
                        if (j + 1 < width && maze[i, j + 1] != 'X')
                        {
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, 1);
                        } // w prawo ale jest dynamit
                        else if (j + 1 < width && maze[i, j + 1] == 'X' && maszDynamit)
                        {
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, czasBurzenia);
                        } 
                        
                        
                        // lewo 
                        if (j - 1 >= 0 && maze[i, j - 1] != 'X')
                        {
                            int to = i * width + j - 1;
                            graph.AddEdge(from, to, 1);
                        } // lewo ale jest dynamit
                        else if (j - 1 >= 0 && maze[i, j - 1] == 'X' && maszDynamit)
                        {
                            int to = i * width + j - 1;
                            graph.AddEdge(from, to, czasBurzenia);
                        }
                       
                        // w dół
                        if (i + 1 < height && maze[i + 1, j] != 'X')
                        {
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, 1);
                        } // w dół ale jest dynamit
                        else if (i + 1 < height && maze[i + 1, j] == 'X' && maszDynamit)
                        {
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, czasBurzenia);
                        }
                        
                        
                        // w górę
                        if (i - 1 >= 0 && maze[i - 1, j] != 'X')
                        {
                            int to = (i - 1) * width + j;
                            graph.AddEdge(from, to, 1);
                        } // w górę ale jest dynamit
                        else if (i - 1 >= 0 && maze[i - 1, j] == 'X' && maszDynamit)
                        {
                            int to = (i - 1) * width + j;
                            graph.AddEdge(from, to, czasBurzenia);
                        }
                        
                        
                        
                    }
                    
                    if (maze[i, j] == 'E') // end point
                    {
                        this.end = (i, j);   
                    }
                    
                    if (maze[i, j] == 'X' || maze[i,j] == 'O') // wall
                    {
                        // jesli sciana i nie masz dynamitu
                        if(maze[i,j] == 'X' && !maszDynamit) continue;
                        int from = i * width + j;
                        
                        // mozna w prawo
                        if (j + 1 < width && maze[i, j + 1] != 'X')
                        {
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, 1);
                        } // w prawo ale jest dynamit
                        else if (j + 1 < width && maze[i, j + 1] == 'X' && maszDynamit)
                        {
                            int to = i * width + j + 1;
                            graph.AddEdge(from, to, czasBurzenia);
                        }
                        
                        // mozna w lewo
                        if (j - 1 >= 0 && maze[i, j - 1] != 'X')
                        {
                            int to = i * width + j - 1;
                            graph.AddEdge(from, to, 1);
                        } // w lewo ale jest dynamit
                        else if (j - 1 >= 0 && maze[i, j - 1] == 'X' && maszDynamit)
                        {
                            int to = i * width + j - 1;
                            graph.AddEdge(from, to, czasBurzenia);
                        }
                        
                        // mozna w gore
                        if (i - 1 >= 0 && maze[i - 1, j] != 'X')
                        {
                            int to = (i - 1) * width + j;
                            graph.AddEdge(from, to, 1);
                        } // w gore ale jest dynamit
                        else if (i - 1 >= 0 && maze[i - 1, j] == 'X' && maszDynamit)
                        {
                            int to = (i - 1) * width + j;
                            graph.AddEdge(from, to, czasBurzenia);
                        }
                        
                        // mozna w dol
                        if (i + 1 < height && maze[i + 1, j] != 'X')
                        {
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, 1);
                        } // w dol ale jest dynamit
                        else if (i + 1 < height && maze[i + 1, j] == 'X' && maszDynamit)
                        {
                            int to = (i + 1) * width + j;
                            graph.AddEdge(from, to, czasBurzenia);
                        }
                    }
                }
            }



            return graph;
        }

        public DiGraph<int> MazeToGraphWithDynamites(char[,] maze, bool maszDynamit, int czasBurzenia,
            int zapasDynamitu)
        {
            int height = maze.GetLength(0);
            int width = maze.GetLength(1);

            DiGraph<int> graph = new DiGraph<int>(height * width * (zapasDynamitu + 1));
            
          
            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    if (maze[i, j] == 'S') // start point
                    {
                        this.start = (i, j);

                        for (int poziom = 0; poziom <= zapasDynamitu; poziom++)
                        {
                                int from = i * width + j + (poziom * height * width);
                        
                                // krwedz w prawo
                                if (j + 1 < width && maze[i, j + 1] != 'X')
                                {
                                    int to = i * width + j + 1 + (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // w prawo ale jest dynamit
                                else if (j + 1 < width && maze[i, j + 1] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    // idziemy na wyzszy poziom bo uzylismy dynamitu
                                    int to = i * width + j + 1 + ((poziom + 1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                } 
                                
                                // lewo 
                                if (j - 1 >= 0 && maze[i, j - 1] != 'X')
                                {
                                    int to = i * width + j - 1 + (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // lewo ale jest dynamit
                                else if (j - 1 >= 0 && maze[i, j - 1] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    int to = i * width + j - 1 + ((poziom+1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                }
                               
                                // w dół
                                if (i + 1 < height && maze[i + 1, j] != 'X')
                                {
                                    int to = (i + 1) * width + j+ (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // w dół ale jest dynamit
                                else if (i + 1 < height && maze[i + 1, j] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    int to = (i + 1) * width + j+ ((poziom+1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                }
                                
                                
                                // w górę
                                if (i - 1 >= 0 && maze[i - 1, j] != 'X')
                                {
                                    int to = (i - 1) * width + j + (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // w górę ale jest dynamit
                                else if (i - 1 >= 0 && maze[i - 1, j] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    int to = (i - 1) * width + j + ((poziom+1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                }

                        }

                          
                    }
                    
                    if (maze[i, j] == 'E') // end point
                    {
                        this.end = (i, j);
                    }
                    
                    if (maze[i, j] == 'X' || maze[i,j] == 'O') // wall
                    {
                        // jesli sciana i nie masz dynamitu
                        if(maze[i,j] == 'X' && !maszDynamit) continue;
                        for (int poziom = 0; poziom <= zapasDynamitu; poziom++)
                        {
                            int from = i * width + j + (poziom * height * width);
                            
                             // mozna w prawo
                                if (j + 1 < width && maze[i, j + 1] != 'X')
                                {
                                    int to = i * width + j + 1 + (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // w prawo ale jest dynamit
                                else if (j + 1 < width && maze[i, j + 1] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    int to = i * width + j + 1 + ((poziom+1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                }
                                
                                // mozna w lewo
                                if (j - 1 >= 0 && maze[i, j - 1] != 'X')
                                {
                                    int to = i * width + j - 1 + (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // w lewo ale jest dynamit
                                else if (j - 1 >= 0 && maze[i, j - 1] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    int to = i * width + j - 1+ ((poziom+1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                }
                                
                                // mozna w gore
                                if (i - 1 >= 0 && maze[i - 1, j] != 'X')
                                {
                                    int to = (i - 1) * width + j + (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // w gore ale jest dynamit
                                else if (i - 1 >= 0 && maze[i - 1, j] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    int to = (i - 1) * width + j + ((poziom+1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                }
                                
                                // mozna w dol
                                if (i + 1 < height && maze[i + 1, j] != 'X')
                                {
                                    int to = (i + 1) * width + j + (poziom * height * width);
                                    graph.AddEdge(from, to, 1);
                                } // w dol ale jest dynamit
                                else if (i + 1 < height && maze[i + 1, j] == 'X' && maszDynamit && poziom < zapasDynamitu)
                                {
                                    int to = (i + 1) * width + j + ((poziom+1) * height * width);
                                    graph.AddEdge(from, to, czasBurzenia);
                                }
                            
                        }
                        
                    }
                }
            }


            return graph;
        }
            

        /// <summary>
        /// Wersje zadania I oraz II
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt bez dynamitów lub z dowolną ich liczbą
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="withDynamite">informacja, czy dostępne są dynamity 
        /// Wersja I zadania -> withDynamites = false, Wersja II zadania -> withDynamites = true</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany (dotyczy tylko wersji II)</param> 
        public int FindShortestPath(char[,] maze, bool withDynamite, out string path, int t = 0)
        {
            path = "";
            
            StringBuilder pathBuilder = new StringBuilder();
            
            int height = maze.GetLength(0);
            int width = maze.GetLength(1);
            
            DiGraph<int> graph = MazeToGraph(maze, withDynamite, t);
            
            // przekonwertuj na numery wierzchołkow
            int? start = this.start.i.Value * width + this.start.j.Value;
            int? end = this.end.i.Value * width + this.end.j.Value;

            PathsInfo<int> pathInfo = Paths.Dijkstra(graph, (int)start);
            if(!pathInfo.Reachable((int)start,(int)end)) return -1;

            int[] pathIndexes = pathInfo.GetPath((int)start, (int)end);

            int from = pathIndexes[0];
            
            for (int i = 1; i < pathIndexes.Length; i++)
            {
                // przetlumacz numery wierzcholkow na E/W/S/N
                int to = pathIndexes[i];
                int fromI = from / width;
                int fromJ = from % width;
                int toI = to / width;
                int toJ = to % width;
                
                if (fromJ + 1 == toJ) pathBuilder.Append("E");
                else if (fromJ - 1 == toJ) pathBuilder.Append("W");
                else if (fromI + 1 == toI) pathBuilder.Append("S");
                else if (fromI - 1 == toI) pathBuilder.Append("N");
                from = to;
            }
        
            path = pathBuilder.ToString();
            
            return pathInfo.GetDistance((int)start, (int)end);

        }

        

        /// <summary>
        /// Wersja III i IV zadania
        /// Zwraca najkrótszy możliwy czas przejścia przez labirynt z użyciem co najwyżej k lasek dynamitu
        /// </summary>
        /// <param name="maze">labirynt</param>
        /// <param name="k">liczba dostępnych lasek dynamitu, dla wersji III k=1</param>
        /// <param name="path">zwracana ścieżka</param>
        /// <param name="t">czas zburzenia ściany</param>
        public int FindShortestPathWithKDynamites(char[,] maze, int k, out string path, int t)
        {
            path = "";
            
            StringBuilder pathBuilder = new StringBuilder();
            
            int height = maze.GetLength(0);
            int width = maze.GetLength(1);
            
            DiGraph<int> graph = MazeToGraphWithDynamites(maze, true, t, k);
            
            
            // przekonwertuj na numery wierzchołkow
            int? start = this.start.i.Value * width + this.start.j.Value;

            PathsInfo<int> pathInfo = Paths.Dijkstra(graph, (int)start);
            // dla kazdego konca w multileelEnds znajdz najkrotsza sciezke
            int minDist = int.MaxValue;
            int minEndVertexIndex = -1;
            int[] pathIndexes;
            for (int i = 0; i <= k; i++)
            {
                var intPtr = this.end.j;
                if (intPtr != null)
                {
                    int? end = i*width*height + this.end.i * width + intPtr.Value;
                    if(!pathInfo.Reachable((int)start,(int)end)) continue;
                    pathIndexes = pathInfo.GetPath((int)start, (int)end);
                    int distance = pathInfo.GetDistance((int)start, (int)end);
                    if (distance < minDist)
                    {
                        minDist = distance;
                        minEndVertexIndex = end.Value;
                    }
                }
            }
            
            if (minEndVertexIndex == -1) return -1;
            pathIndexes = pathInfo.GetPath((int)start, (int)minEndVertexIndex);
            
            int from = pathIndexes[0];
            
            for (int i = 1; i < pathIndexes.Length; i++)
            {
                // przetlumacz numery wierzcholkow na E/W/S/N
                int to = pathIndexes[i]%(width*height);
                if(from == to) continue;
                int fromI = from / width;
                int fromJ = from % width;
                int toI = to / width;
                int toJ = to % width;
                
                if (fromJ + 1 == toJ) pathBuilder.Append("E");
                else if (fromJ - 1 == toJ) pathBuilder.Append("W");
                else if (fromI + 1 == toI) pathBuilder.Append("S");
                else if (fromI - 1 == toI) pathBuilder.Append("N");
                from = to;
            }
        
            path = pathBuilder.ToString();
            
            return pathInfo.GetDistance((int)start, (int)minEndVertexIndex);
        }
    }
}