using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ASD
{
    public static class FlowExtender
    {

        // metoda zwracajaca przekroj dla danego grafu i danego maksymalnego przeplywu w grafie

        private static Edge<double>[] FindMinCut(Graph<double> g, DiGraph<double> maxFlow, int s)
        {
            Edge<double>[] minCut;
            DiGraph<double> residualGraph = new DiGraph<double>(g.VertexCount);
            
            // przejdz po wszystkich wiercholkach 
            for(int v = 0; v < g.VertexCount; v++)
            {
                // na podstweaie przeplywu zbuduj siec rezydualna
                // jesli przeplyw jest mniejszy od wagi krawedzi to dodaj krawedz do sieci rezydualnej
                // w przeciwmym kierunku o wartosci rownej przeplywowi i w tym samym kierunku o wartosci rownej wadze krawedzi - przeplyw
                foreach (Edge<double> e in g.OutEdges(v))
                {
                    if (maxFlow.HasEdge(e.From, e.To))
                    {
                        if(maxFlow.GetEdgeWeight(e.From, e.To) < e.Weight)
                        {
                            residualGraph.AddEdge(e.From, e.To, e.Weight - maxFlow.GetEdgeWeight(e.From, e.To));
                        }
                        residualGraph.AddEdge(e.To, e.From, maxFlow.GetEdgeWeight(e.From, e.To));

                    }
                    else // jesli tej krawedzi nie ma w przeplywie to dodaj ja z waga rowna wadze krawedzi
                    {
                        residualGraph.AddEdge(e.From,e.To, e.Weight);
                    }
                }
            }
            
            
            // tablica booli do oznaczania odwiedzonych krawedzi z s i t w sieci rezydualnej 
            bool[] visited = new bool[g.VertexCount];
            visited[s] = true;
            // wykonaj dfs na sieci rezydualnej
            foreach(Edge<double> e in residualGraph.DFS().SearchFrom(s))
            {
                visited[e.To] = true;
            }
            
            // zbuduj tablice krawedzi minimalnego przekroju (znajdz krwaedzie ktore sa w grafie wejsciowym i nie sa odwiedzone)
            List<Edge<double>> minCutList = new List<Edge<double>>();
            foreach(Edge<double> e in g.DFS().SearchAll())
            {
                if(visited[e.From] && !visited[e.To])
                {
                    minCutList.Add(e);
                }
            }
            
            minCut = minCutList.ToArray();
            
            return minCut;
            
        }

        /// <summary>
        /// Metod wylicza minimalny s-t-przekrój.
        /// </summary>
        /// <param name="undirectedGraph">Nieskierowany graf</param>
        /// <param name="s">wierzchołek źródłowy</param>
        /// <param name="t">wierzchołek docelowy</param>
        /// <param name="minCut">minimalny przekrój</param>
        /// <returns>wartość przekroju</returns>
        public static double MinCut(this Graph<double> undirectedGraph, int s, int t, out Edge<double>[] minCut)
        {
            minCut = null;
            // wykonaj forda fulkersona
            var (flowValue, flow) = Flows.FordFulkerson(undirectedGraph, s, t);
            minCut = FindMinCut(undirectedGraph, flow, s);

            return flowValue;
        }

        /// <summary>
        /// Metada liczy spójność krawędziową grafu oraz minimalny zbiór rozcinający.
        /// </summary>
        /// <param name="undirectedGraph">nieskierowany graf</param>
        /// <param name="cutingSet">zbiór krawędzi rozcinających</param>
        /// <returns>spójność krawędziowa</returns>
        public static int EdgeConnectivity(this Graph<double> undirectedGraph, out Edge<double>[] cutingSet)
        {
            cutingSet = null;
            double minSum = double.MaxValue;
            int s = 0;
            for(int v = 1; v < undirectedGraph.VertexCount; v++)
            {
                var (flowValue, flow) = Flows.FordFulkerson(undirectedGraph, s, v);
                Edge<double>[] minCutList = FindMinCut(undirectedGraph,flow ,s);
                double sum = minCutList.Sum(e => e.Weight);
                if (sum < minSum)
                {
                    minSum = sum;
                    cutingSet = minCutList;
                }
            }
            return minSum == double.MaxValue ? 0 : (int)minSum;
        }
        
    }
}
