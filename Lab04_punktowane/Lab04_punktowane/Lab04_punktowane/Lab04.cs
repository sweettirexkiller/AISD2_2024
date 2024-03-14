using System;
using ASD.Graphs;
using System.Collections.Generic;

namespace ASD
{
    public class Lab04 : MarshalByRefObject
    {
        /// <summary>
        /// Etap 1 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego
        /// przy zalozeniu, ze pociagi odjezdzaja co godzine.
        /// </summary>
        /// <param name="graph">Graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
        public int[] Lab04Stage1(DiGraph graph, int miastoStartowe, int K)
        {
            int[] miastaMozliweDoOdwiedzenia = new int[] { miastoStartowe };

            // musimy obliczyc dlugosc najkrotszej sciezki do kazdego miasta z miasta startowego
            // jesli jest krotsza od K - 8 to znaczy ze mozemy tam dojechac przed godzina K startujac o godzinie 8 
            // (bo pociagi odjezdzaja co godzine)

            bool[] visited = new bool[graph.VertexCount];
            int[] odleglosc = new int[graph.VertexCount];
            
            // zainicjiuj na maksa 
            for (int i = 0; i < odleglosc.Length; i++)
            {
                odleglosc[i] = int.MaxValue;
                visited[i] = false;
            }
            
            odleglosc[miastoStartowe] = 0;
            visited[miastoStartowe] = true;
            
            foreach (Edge e in graph.BFS().SearchFrom(miastoStartowe))
            {
                if (!visited[e.To])
                {
                    visited[e.To] = true;
                    odleglosc[e.To] = odleglosc[e.From] + 1;   
                }
            }
            
            // znajdz w odlegosc te ktorych odleglosc jest mniejsza od K - 8
            for(int i = 0; i < odleglosc.Length; i++)
            {
                if(i == miastoStartowe) continue;
                if (odleglosc[i] <= K - 8)
                {
                    // dodaj miasto "i" do tablicy miastaMozliweDoOdwiedzenia
                    Array.Resize(ref miastaMozliweDoOdwiedzenia, miastaMozliweDoOdwiedzenia.Length + 1);
                    miastaMozliweDoOdwiedzenia[miastaMozliweDoOdwiedzenia.Length - 1] = i;
                }
            }
            
            
            // posortuj rosnaco mozliwe do odwiedzenia 
            Array.Sort(miastaMozliweDoOdwiedzenia);


            return miastaMozliweDoOdwiedzenia;
        }

        /// <summary>
        /// Etap 2 - Szukanie mozliwych do odwiedzenia miast z grafu skierowanego.
        /// Waga krawedzi oznacza, ze pociag rusza o tej godzinie
        /// </summary>
        /// <param name="graph">Wazony graf skierowany przedstawiający siatke pociagow</param>
        /// <param name="miastoStartowe">Numer miasta z ktorego zaczyna sie podroz pociagiem</param>
        /// <param name="K">Godzina o ktorej musi zakonczyc sie nasza podroz</param>
        /// <returns>Tablica numerow miast ktore mozna odwiedzic. Posortowana rosnaco.</returns>
        public int[] Lab04Stage2(DiGraph<int> graph, int miastoStartowe, int K)
        {
            // O(n*K + mlogn) ????
            int[] miastaMozliweDoOdwiedzenia = new int[] { miastoStartowe };
            // jesli moze byc  w danym wierzcholku o danej godzienie to zapisujemy z ktorego wierzcholka przyszedl
            int[,] dokadDojadeOGodzinie = new int[graph.VertexCount,K];
            bool[,] visited = new bool[graph.VertexCount,K];

            
            dokadDojadeOGodzinie[miastoStartowe, 8] = 0;
            // O(n)
            for (int n = 0; n < graph.VertexCount; n++)
            {
                // O(K)
                for(int k =8; k < K; k++)
                { 
                    dokadDojadeOGodzinie[n, k] = int.MaxValue;
                    visited[n, k] = false;
                    foreach (Edge<int> e in graph.OutEdges(n))
                   {
                       if(e.Weight == k) 
                       {
                           // mozna o tej godzinie wejsc to do tego pociagu i pojechac do e.To
                           dokadDojadeOGodzinie[n, k] = e.To;
                       }
                   }
                }
            }
            visited[miastoStartowe, 8] = true;
            
            
            
            
            // O(mlogn) - Dijkstra z kolejka priorytetowa
            int[] czasDojazdu = new int[graph.VertexCount];
            for (int i = 0; i < graph.VertexCount; i++)
            {
                czasDojazdu[i] = int.MaxValue;
            }
            
            czasDojazdu[miastoStartowe] = 0;
            
            
            // stworz kolejke priorytetowa zainicjowana wiercholkami gdzie priorytet to odleglosc[v]
            PriorityQueue<int, int> queue = new PriorityQueue<int, int>();
            for (int i = 0; i < graph.VertexCount; i++)
            {
                queue.Insert(i, czasDojazdu[i]);
            }
            
            
            
            
            // dopoki kolejka niepusta
            while (!queue.Count.Equals(0))
            {
                // wyjmij ten element z najmnejszym priorytetem
                int u = queue.Extract();
                foreach (Edge<int> e in graph.OutEdges(u))
                {
                    // czy moge skorzystac z tego pociagu ?
                    if (dokadDojadeOGodzinie[u, czasDojazdu[u]] == e.To)
                    {
                        // jesli tak to sprawdz czy moge dojechac do tego miasta
                        if (czasDojazdu[e.To] > czasDojazdu[u] + 1)
                        {
                            czasDojazdu[e.To] = czasDojazdu[u] + 1;
                            queue.Insert(e.To, czasDojazdu[e.To]);
                        }
                    }
                }
            }
            
            
            
            

            return miastaMozliweDoOdwiedzenia;
        }
    }
}
