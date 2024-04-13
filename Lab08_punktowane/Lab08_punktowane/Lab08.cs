using System;
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class Lab08 : MarshalByRefObject
    {
        /// <summary>Etap I: prace przedprojektowe</summary>
        /// <param name="l">Długość działki, którą dysponuje Kameleon Kazik.</param>
        /// <param name="h">Maksymalna wysokość budowli.</param>
        /// <param name="pleasure">Tablica rozmiaru [l,h] zawierająca wartości zadowolenia p(x,y) dla każdych x i y.</param>
        /// <returns>Odpowiedź na pytanie, czy istnieje budowla zadowalająca Kazika.</returns>
        public bool Stage1ExistsBuilding(int l, int h, int[,] pleasure)
        {
            // wierzcholki to bloki, krawedzie to relacje miedzy blokami
            // dodatkowo dodac wierzcholki zrodlowy i ujscia

            int source, sink; 
            DiGraph<int> buildingGraph = buildBuildingGraph(l, h, pleasure,out source,out sink);
            // sprawdzamy czy istnieje sciezka zrodlowy -> ujscie
            var (flowValue, maxPleasureBuilding) = Flows.FordFulkerson(buildingGraph, source, sink);
           
            var reversedGraph = new DiGraph<int>(buildingGraph.VertexCount);
            for(int v=0; v<buildingGraph.VertexCount; v++)
            {
                foreach (var edge in buildingGraph.OutEdges(v))
                {
                    reversedGraph.AddEdge(edge.To, edge.From, edge.Weight);
                }
            }
            
            // usun krawedz z bloku do ujscia jesli jedyna krawedzia wchodzaca jest z zrodla i jedyna wychodzaca do ujscia i blok nie jest na najnizszym poziomie
            
            for(int y = 0; y < h ; y++)
            {
                for (int x = 0; x < l ; x++)
                {
                    if (y > 0 && x < l - 1 && l - 1 - x >= y)
                    {
                        // sprawdzic czy wchodzaca jest tylko jedna i jest ze zrodla
                        if (reversedGraph.OutDegree(x + y * l) == 1)
                        {
                            foreach (var edge in reversedGraph.OutEdges(x + y * l))
                            {
                                if (edge.To == source)
                                {
                                    // sprawdzic czy wychodzaca jest tylko jedna i jest do ujscia
                                    if(maxPleasureBuilding.OutDegree(x + y * l) == 1)
                                    {
                                        foreach (var edge2 in maxPleasureBuilding.OutEdges(x + y * l))
                                        {
                                            if (edge2.To == sink)
                                            {
                                                // usun krawedz
                                                buildingGraph.RemoveEdge(x + y * l, sink);
                                                buildingGraph.RemoveEdge(source, x + y * l);
                                            }
                                        }
                                    }
                                }
                            }
                        }                        
                    }
                }
            }
            
            // zbuduj nowy graf maxPleasureBuilding 
            var (flowValue2, maxPleasureBuilding2) = Flows.FordFulkerson(buildingGraph, source, sink);

            // jesli jest conajmniej jedna krawedz ze zrodla ktora jest nienasycona to znaczy ze jest przyjemnosc

            bool isPleasure = false;
            
            foreach (var edge in maxPleasureBuilding2.OutEdges(source))
            {
                int weight = buildingGraph.GetEdgeWeight(source, edge.To);
                int pleasureFlow = maxPleasureBuilding2.GetEdgeWeight(source, edge.To);
                if (weight > pleasureFlow)
                {
                    isPleasure = true;
                    break;
                }
            }

            return isPleasure;
        }

        private DiGraph<int> buildBuildingGraph(int l, int h, int[,] pleasure, out int source,out int sink)
        {
            DiGraph<int> buildingGraph = new DiGraph<int>(l * h + 2);
            source = l * h;
            sink = l * h + 1;
            
            // wagi krawedzi to wartosci zadowolenia
            
            
            // blok (x,y) ma krawedzie do blokow (x,y-1) i (x+1,y-1) oraz do blokow (x-1,y-1) 
            for(int y = 0; y < h ; y++)
            {
                for (int x = 0; x < l ; x++)
                {

                    // kazdy blok ma do wyjscia o wartosci 1
                    buildingGraph.AddEdge(x + y * l, sink, 1);
                    
                    // kazdy ktory ma pleasure dodatnia to ma krawedz od ujscia  o wartosci przyjenosci 
                    if (pleasure[x, y] > 0 &&  l - 1 - x >= y)
                    {
                        buildingGraph.AddEdge(source, x + y * l, pleasure[x, y]);
                    }
                    
                    // kazdy poza najnizszym wierszem ma krawedzie do blokow (x,y-1) i (x+1,y-1)
                    if (y > 0 && x < l - 1 && l - 1 - x >= y)
                    {
                        buildingGraph.AddEdge(x + y * l, x + (y - 1) * l, int.MaxValue);
                        buildingGraph.AddEdge(x + y * l, x + 1 + (y - 1) * l, int.MaxValue);
                    }
                }
            }
            
            return buildingGraph;
        }

        /// <summary>Etap II: kompletny projekt</summary>
        /// <param name="l">Długość działki, którą dysponuje Kameleon Kazik.</param>
        /// <param name="h">Maksymalna wysokość budowli.</param>
        /// <param name="pleasure">Tablica rozmiaru [l,h] zawierająca wartości zadowolenia p(x,y) dla każdych x i y.</param>
        /// <param name="blockOrder">Argument wyjściowy, w którym należy zwrócić poprawną kolejność ustawienia bloków w znalezionym rozwiązaniu;
        ///     kolejność jest poprawna, gdy przed blokiem (x,y) w tablicy znajdują się bloki (xbuildingGraph = {DiGraph<int>} 0:\n1:\n2:\n3:\n4:\n5:0(1) 1(1)\n6:1(1) 2(1) 11(1) 10(1) 26(1)\n7:2(1) 3(1) 12(3) 11(3) 26(3)\n8:3(1)\n9:\n10:5(1)\n11:16(2) 15(2) 26(2)\n12:8(1)\n13:8(1)\n14:\n15:10(1)\n16:12(1)\n17:12(1) 13(1)\n18:13(1) 23(98) 22(98) 26(98)\n19:\n20:15(1) 16(1)\n21:16(1) 17(1)\n22:17(1)\n23:… View,y-1) i (x+1,y-1) lub gdy y=0. 
        ///     Ustawiane bloki powinny mieć współrzędne niewychodzące poza granice obszaru budowy (0<=x<l, 0<=y<h).
        ///     W przypadku braku rozwiązania należy zwrócić null.</param>
        /// <returns>Maksymalna wartość zadowolenia z budowli; jeśli nie istnieje budowla zadowalająca Kazika, zależy zwrócić null.</returns>
        public int? Stage2GetOptimalBuilding(int l, int h, int[,] pleasure, out (int x, int y)[] blockOrder)
        {
            int source, sink; 
            DiGraph<int> buildingGraph = buildBuildingGraph(l, h, pleasure,out source,out sink);
            // sprawdzamy czy istnieje sciezka zrodlowy -> ujscie
            var (flowValue, maxPleasureBuilding) = Flows.FordFulkerson(buildingGraph, source, sink);

            // jesli jest conajmniej jedna krawedz ze zrodla ktora jest nienasycona to znaczy ze jest przyjemnosc

            bool isPleasure = false;
            int maxPleasure = 0;
            
            foreach (var edge in maxPleasureBuilding.OutEdges(source))
            {
                int weight = buildingGraph.GetEdgeWeight(source, edge.To);
                int pleasureFlow = maxPleasureBuilding.GetEdgeWeight(source, edge.To);
                if (weight > pleasureFlow)
                {
                    isPleasure = true;
                    int diff = weight - pleasureFlow;
                    if (diff > maxPleasure)
                    {
                        maxPleasure = diff;
                    }
                }
            }
            
            // zbuduj graf odwrocony
            var reversedGraph = new DiGraph<int>(buildingGraph.VertexCount);
            for(int v=0; v<buildingGraph.VertexCount; v++)
            {
                foreach (var edge in buildingGraph.OutEdges(v))
                {
                    reversedGraph.AddEdge(edge.To, edge.From, edge.Weight);
                }
            }

            // usun krawedz z bloku do ujscia jesli jedyna krawedzia wchodzaca jest z zrodla i jedyna wychodzaca do ujscia i blok nie jest na najnizszym poziomie
            
            for(int y = 0; y < h ; y++)
            {
                for (int x = 0; x < l ; x++)
                {
                    if (y > 0 && x < l - 1 && l - 1 - x >= y)
                    {
                        // sprawdzic czy wchodzaca jest tylko jedna i jest ze zrodla
                        if (reversedGraph.OutDegree(x + y * l) == 1)
                        {
                            foreach (var edge in reversedGraph.OutEdges(x + y * l))
                            {
                                if (edge.To == source)
                                {
                                    // sprawdzic czy wychodzaca jest tylko jedna i jest do ujscia
                                    if(maxPleasureBuilding.OutDegree(x + y * l) == 1)
                                    {
                                        foreach (var edge2 in maxPleasureBuilding.OutEdges(x + y * l))
                                        {
                                            if (edge2.To == sink)
                                            {
                                                // usun krawedz
                                                buildingGraph.RemoveEdge(x + y * l, sink);
                                                buildingGraph.RemoveEdge(source, x + y * l);
                                            }
                                        }
                                    }
                                }
                            }
                        }                        
                    }
                }
            }
            
            // zbuduj nowy graf maxPleasureBuilding 
            var (flowValue2, maxPleasureBuilding2) = Flows.FordFulkerson(buildingGraph, source, sink);
          
            
            
            // znajdz sumaryczna przyjemnosc czyli sume wag krawedzi wychodzacych z zrodla - przeplyw na tych krawedziach 
            int totalPleasure = 0;
            foreach (var edge in maxPleasureBuilding2.OutEdges(source))
            {
                int weight = buildingGraph.GetEdgeWeight(source, edge.To);
                int pleasureFlow = maxPleasureBuilding2.GetEdgeWeight(source, edge.To);
                totalPleasure += weight - pleasureFlow;
            }

            if (totalPleasure == 0)
            {
                blockOrder = null;
                return null;
            }
            else
            {
                // zbuduj kolejnosc blokow
                List<(int x, int y)> blockOrderList = new List<(int x, int y)>();
                // get all vertices that are not source and sink that are in the flowGraph (have non zero flow)
                List<int> vertices = new List<int>();
                for(int v = 0; v < buildingGraph.VertexCount; v++)
                {
                    if (v != source && v != sink)
                    {
                        // check if there is a flow on the edge
                        if (maxPleasureBuilding2.OutDegree(v) > 0 || maxPleasureBuilding2.InDegree(v) > 0)
                        {
                            vertices.Add(v);
                        }
                    }
                }
                
                // sort vertices by their order in the graph
                vertices.Sort((v1, v2) => v1.CompareTo(v2));
                // translate from vertex number to x,y
                foreach (int v in vertices)
                {
                    int x = v % l;
                    int y = v / l;
                    blockOrderList.Add((x, y));
                }
                
                blockOrder = blockOrderList.ToArray();
                return totalPleasure;
                
                
            }
           
            

        }
    }
}
