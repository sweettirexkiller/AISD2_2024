using System.Linq;

namespace ASD
{
    using ASD.Graphs;
    using System;
    using System.Collections.Generic;

    public class Lab06 : System.MarshalByRefObject
    {
        public List<int> WidePath(DiGraph<int> G, int start, int end)
        {
            //trasa między punktem początkowym a końcowym, która maksymalizuje szerokość najwęższego połączenia
            // wlasna dijkstra
            int[] smallestWeights = new int[G.VertexCount];
            
            for (int i = 0; i < G.VertexCount; i++)
            {
                smallestWeights[i] = int.MaxValue;
            }
            
            smallestWeights[start] = 0;
            
            //
            
            return new List<int>();
        }


        public List<int> WeightedWidePath(DiGraph<int> G, int start, int end, int[] weights, int maxWeight)
        {
            return new List<int>();
        }
    }
}