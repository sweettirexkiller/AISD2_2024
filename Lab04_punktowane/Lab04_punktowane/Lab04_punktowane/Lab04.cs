using System;
using ASD.Graphs;
using System.Collections.Generic;
using System.Security.Policy;

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
            Edge<int>?[,] pociagDoNDoGodzinyK = new Edge<int>?[graph.VertexCount,K+1];

            
         
            // O(n)
            for (int n = 0; n < graph.VertexCount; n++)
            {
                // O(K)
                for(int k =8; k < K; k++)
                { 
                    if(n == miastoStartowe && k == 8)
                    {
                        // do godziny 8 mozemy dojechac do stacji n w 0 godzin
                        pociagDoNDoGodzinyK[n, k] = new Edge<int>(miastoStartowe, n, 0);
                    }
                    else
                    {
                        // do godziny k nie mozemy dojechac do stacji n
                        pociagDoNDoGodzinyK[n, k] = null;
                    }
                }
            }

            // inicjiujemy dokad mozna dojechac z miasta startowego o danej godzinie
            foreach (Edge<int> e in graph.OutEdges(miastoStartowe))
            {
                // do godziny e.Weight + 1 mozemy dojechac do stacji e.To pociagiem edge
                if(e.Weight + 1 <= K)
                {
                    pociagDoNDoGodzinyK[e.To, e.Weight +1] = e;
                }
            }
            
            
            // stworz kolejke priorytetowa zainicjowana wiercholkami gdzie priorytet to odlegloscOGodzinie[v,k]
            PriorityQueue<int,Edge<int>?> queue = new PriorityQueue<int,Edge<int>?>();
            
            
            
            for (int n = 0; n < graph.VertexCount; n++)
            {
                for (int k = 8; k < K; k++)
                {                
                    if(pociagDoNDoGodzinyK[n,k] != null)
                    {
                        queue.Insert(pociagDoNDoGodzinyK[n, k],k);
                    }
                }
            }
            
            
            
            
            // dopoki kolejka niepusta
            while (!queue.Count.Equals(0))
            {
                // wyjmij hashset z najmnejszym priorytetm 
                Edge<int>? pociagDojazdowy = queue.Extract();
                
                // dla kazdego wierzcholka w hashsecie wykonaj relaksacje ??
                    int v = pociagDojazdowy.Value.To;
                    int czas = pociagDojazdowy.Value.Weight + 1;
                    foreach (Edge<int> pociagOdjazdowy in graph.OutEdges(v))
                    {
                        // czy moge skorzystac z tego pociagu ?
                        if (czas <= pociagOdjazdowy.Weight)
                        {
                            // jesli ten pociag odjezdza i tak juz po czasie K to nie ma sensu sprawdzac
                            if(pociagOdjazdowy.Weight >= K) continue;
                            // 
                            
                            // jesli tak to sprawdz czy moge dojechac do wierzcholka e.To korzystajac z pociagu o godzinie e.Weight
                            // kosztem mniejszym niz obecny
                            
                            // if (pociagDoNDoGodzinyK[pociagOdjazdowy.To, pociagOdjazdowy.Weight +1].Value.Weight+1 > pociagDoNDoGodzinyK[pociagOdjazdowy.To, ] + 1)
                            // {
                                // pociagDoNDoGodzinyK[e.To, e.Weight] = pociagDoNDoGodzinyK[u, czas] + 1;
                                // znajdz w kolejce hashset z wierzcholkiem e.To i godzina e.Weight
                                
                                // wykonaj zmiane priorytetu dla wierzcholka e.To o godzienie e.Weight 
                                // queue.Insert(new Tuple<int, int>(e.To,e.Weight), pociagDoNDoGodzinyK[e.To, e.Weight]);
                            // }
                        }
                    }
               
            }
            
            // znajdz w odlegosc te ktorych odleglosc jest mniejsza od K - 8
            for(int i = 0; i < graph.VertexCount; i++)
            {
                for (int j = 8; j < K; j++)
                {
                    // if (pociagDoNDoGodzinyK[i, j] <= K - 8)
                    // {
                    //     // dodaj miasto "i" do tablicy miastaMozliweDoOdwiedzenia
                    //     Array.Resize(ref miastaMozliweDoOdwiedzenia, miastaMozliweDoOdwiedzenia.Length + 1);
                    //     miastaMozliweDoOdwiedzenia[miastaMozliweDoOdwiedzenia.Length - 1] = i;
                    // }
                }
            }
            
            
            
            

            return miastaMozliweDoOdwiedzenia;
        }
    }
}
