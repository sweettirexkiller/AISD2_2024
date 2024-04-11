using System;
using System.Collections.Generic;
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
            DiGraph<int> buildingGraph = new DiGraph<int>(l * h + 2);
            int source = l * h;
            int sink = l * h + 1;
            
            // wagi krawedzi to wartosci zadowolenia
            
            
            // blok (x,y) ma krawedzie do blokow (x,y-1) i (x+1,y-1) oraz do blokow (x-1,y-1) 
            for(int y = 0; y < h - 1; y++)
            {
                for(int x = 0; x < l - 1; x++)
                {
                    
                    // przyjemnosc z postawienie bloku jeden wyzej
                    
                    if (pleasure[x, y] - 1 < 0)
                    {
                        buildingGraph.AddEdge(x + (y + 1) * l,x + y * l,   -(pleasure[x, y] - 1));
                    }
                    else
                    {
                        buildingGraph.AddEdge(x + y * l, x + (y + 1) * l,    pleasure[x, y] - 1);
                    }
                    
                    // przyjemnosc z postawienia bloku jeden wyzej i jeden w prawo (tylko jesli mozna)
                    if (x > 0)
                    {
                        if (pleasure[x, y] - 1 < 0)
                        {
                            buildingGraph.AddEdge( x - 1 + (y + 1) * l,x + y * l,  -(pleasure[x, y] - 1));
                        }
                        else
                        {
                            buildingGraph.AddEdge(x + y * l, x - 1 + (y + 1) * l,  pleasure[x, y] - 1);
                        }

                    }
                  
                    
                    // dodatkowo kazdy do ujscia 
                    int pleasureValue = pleasure[x, y] - 1;
                    if (pleasureValue < 0)
                    {
                        buildingGraph.AddEdge(sink,x + y * l,  -pleasureValue);
                    }
                    else
                    {
                        buildingGraph.AddEdge(x + y * l, sink, pleasureValue);
                    }
                }
            }
           
            // zrodlowy ma krawedzie do blokow (0,0), (1,0), ..., (l-1,0) z wagami przyjemnosci z postawienia bloku [0,i]
            for(int x = 0; x < l; x++)
            {
                if(pleasure[x, 0] - 1 < 0)
                {
                    buildingGraph.AddEdge(x,source, -(pleasure[x, 0] - 1));
                }
                else
                {
                    buildingGraph.AddEdge(x, source, pleasure[x, 0] - 1);
                }
            }
            
            // bloki (x,h-1) maja krawedzie do ujscia z wagami 0
            for(int x = 0; x < l; x++)
            {
                int pleasureValue = pleasure[x, h - 1] - 1;
                if (pleasureValue - 1 < 0)
                {
                    buildingGraph.AddEdge(sink, x + (h - 1) * l, -pleasureValue);
                }
                else
                {
                    buildingGraph.AddEdge(x + (h - 1) * l, sink, pleasureValue );
                }
            }
            
            // sprawdzamy czy istnieje sciezka zrodlowy -> ujscie
            var (flowValue, maxPleasureBuilding)= Flows.FordFulkerson(buildingGraph, source, sink);


            return (flowValue > 0) ? true : false;
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
            blockOrder = null;
            return null;
        }
    }
}
