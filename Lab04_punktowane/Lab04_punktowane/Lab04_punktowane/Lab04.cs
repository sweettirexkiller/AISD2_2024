using System;
using ASD.Graphs;
using System.Collections.Generic;
using System.Linq;

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
             var miastaMozliweDoOdwiedzenia = new List<int>();
             
            // O(mlogn) - Dijkstra z kolejka priorytetowa
            int[] czasDojazdu = Enumerable.Repeat(int.MaxValue, graph.VertexCount).ToArray();

            czasDojazdu[miastoStartowe] = 8;
            
            // stworz kolejke priorytetowa zainicjowana wiercholkami gdzie priorytet to odleglosc[v]
            SafePriorityQueue<int, int> queue = new SafePriorityQueue<int, int>();
            for (int i = 0; i < graph.VertexCount; i++)
            {
                queue.Insert(i, czasDojazdu[i]);
            }
            
            //dijkstra
            while (!queue.Count.Equals(0))
            {
                int u = queue.Extract();

                foreach (Edge<int> e in graph.OutEdges(u))
                {
                    // nie warto rozważać krawędzi ktorego pociag dojedzie po K
                    if (e.Weight + 1 > K) continue;

                    // czy tym pociagiem mozna dojechac do e.To szybciej niz dotychczas?
                    // z pociagu mozna skorzystac jesli dojechalismy na stacje przed odjazdem 
                    if (czasDojazdu[e.To] > e.Weight + 1 && czasDojazdu[u] <= e.Weight)
                    {
                        czasDojazdu[e.To] = e.Weight + 1;
                        queue.UpdatePriority(e.To, czasDojazdu[e.To]);
                    }
                }
            }


            
            // wyłuskaj te wierzcholki ktore sa dostepne przed K
            for (int i = 0; i < graph.VertexCount; i++)
            {
                if (czasDojazdu[i] <= K)
                {
                    // dodaj miasto "i" do tablicy miastaMozliweDoOdwiedzenia
                    miastaMozliweDoOdwiedzenia.Add(i);
                }
            }
            
            miastaMozliweDoOdwiedzenia.Sort();
            return miastaMozliweDoOdwiedzenia.ToArray();
        }
    }
}
