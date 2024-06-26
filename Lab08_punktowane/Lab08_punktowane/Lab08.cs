﻿using System;
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class Lab08 : MarshalByRefObject
    {

        private DiGraph<int> buildResidual(DiGraph<int> g, DiGraph<int> maxFlow)
        {
            DiGraph<int> residualGraph = new DiGraph<int>(g.VertexCount);
            
            // przejdz po wszystkich wiercholkach 
            for(int v = 0; v < g.VertexCount; v++)
            {
                // na podstweaie przeplywu zbuduj siec rezydualna
                // jesli przeplyw jest mniejszy od wagi krawedzi to dodaj krawedz do sieci rezydualnej
                // w przeciwmym kierunku o wartosci rownej przeplywowi i w tym samym kierunku o wartosci rownej wadze krawedzi - przeplyw
                foreach (Edge<int> e in g.OutEdges(v))
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

            return residualGraph;
        }

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

            // jesli jest conajmniej jedna krawedz ze zrodla ktora jest nienasycona to znaczy ze jest przyjemnosc

            bool isPleasure = false;
            
            // przeplyw to ilosc wbudowanych blokow 
            
            foreach (var edge in maxPleasureBuilding.OutEdges(source))
            {
                int weight = buildingGraph.GetEdgeWeight(source, edge.To);
                int pleasureFlow = maxPleasureBuilding.GetEdgeWeight(source, edge.To);
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
            h = Math.Min(h, l);
            DiGraph<int> buildingGraph = new DiGraph<int>(l * h + 2);
            source = l * h;
            sink = l * h + 1;
            
            // wagi krawedzi to wartosci zadowolenia
            
            
            // blok (x,y) ma krawedzie do blokow (x,y-1) i (x+1,y-1) oraz do blokow (x-1,y-1) 
            for(int y = 0; y < h ; y++)
            {
                for (int x = 0; x < l - y ; x++)
                {
                    // if(l - 1 - x < y) continue;

                    // kazdy blok ma do wyjscia o wartosci 1
                    buildingGraph.AddEdge(x + y * l, sink, 1);
                    
                    // kazdy ktory ma pleasure dodatnia to ma krawedz z wejscia o wartosci przyjenosci 
                    if (pleasure[x, y] > 0 &&  l - 1 - x >= y)
                    {
                        buildingGraph.AddEdge(source, x + y * l, pleasure[x, y]);
                    }
                    
                    // kazdy poza najnizszym wierszem ma krawedzie do blokow (x,y-1) i (x+1,y-1)
                    if (y > 0 && x < l - 1 && l  - 1 - x >=y)
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
            int flowValue;
            DiGraph<int> maxPleasureBuilding;
            
            List<int> blocks = new List<int>();
            bool blockRemoved = false;
            
            // zbuduj rezydualna 
            
            
            do
            {
                blocks.Clear();
                blockRemoved = false;
                (flowValue, maxPleasureBuilding) = Flows.FordFulkerson(buildingGraph, source, sink);
                DiGraph<int> residualGraph = buildResidual(buildingGraph, maxPleasureBuilding);
                foreach(Edge<int> e in residualGraph.BFS().SearchFrom(source))
                {
                    blocks.Add(e.To);
                }
                blocks.RemoveAll((i => i == source));
                // blocks.RemoveAll((i => i == sink));
                // remove duplicates
                // blocks = blocks.Distinct().ToList();
                foreach (int block in blocks)
                {
                    // znajdz numer bloku ponizej
                    int x = block % l;
                    int y = block / l;
                    if (y > 0 && x < l - 1 && l - 1 - x >= y)
                    {
                        int blockBelow = x + (y - 1) * l;
                        int blockBelowRight = x + 1 + (y - 1) * l;
                        if (!blocks.Contains(blockBelow) || !blocks.Contains(blockBelowRight))
                        {
                            // usun krawedzie z bloku do blokow ponizej
                            buildingGraph.RemoveEdge(block, blockBelow);
                            buildingGraph.RemoveEdge(block, blockBelowRight);
                            buildingGraph.RemoveEdge(source, block);
                            buildingGraph.RemoveEdge(block, sink);
                            blockRemoved = true;
                        }
                    }
                }

            } while (blockRemoved);
            
            int totalPleasure = 0;
            foreach (var edge in maxPleasureBuilding.OutEdges(source))
            {
                int pleasureFromBlock = buildingGraph.GetEdgeWeight(source, edge.To);
                int cost = maxPleasureBuilding.GetEdgeWeight(source, edge.To);
                totalPleasure += pleasureFromBlock - cost;
            }

            if (totalPleasure <= 0)
            {
                blockOrder = null;
                return null;
            }
            else
            {
                
                blocks.Sort();
                blocks = blocks.Distinct().ToList();
                // zbuduj tablice blokow
                blockOrder = new (int x, int y)[blocks.Count];
                
                for(int i = 0; i < blocks.Count; i++)
                {
                    blockOrder[i] = (blocks[i] % l, blocks[i] / l);
                }
                
                return totalPleasure;
            }

        }
    }
}
