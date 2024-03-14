using System;
using ASD.Graphs;
using ASD;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace ASD
{

    public class Lab03GraphFunctions : System.MarshalByRefObject
    {

        // Część 1
        // Wyznaczanie odwrotności grafu
        //   0.5 pkt
        // Odwrotność grafu to graf skierowany o wszystkich krawędziach przeciwnie skierowanych niż w grafie pierwotnym
        // Parametry:
        //   g - graf wejściowy
        // Wynik:
        //   odwrotność grafu
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Graf wynikowy musi być w takiej samej reprezentacji jak wejściowy
        public DiGraph Lab03Reverse(DiGraph g)
        {
            // stworz nowy graph
            DiGraph reversed = new DiGraph(g.VertexCount, g.Representation);
            // dla kazdej krawedzi w grafie g 
            foreach (Edge e in g.DFS().SearchAll())
            {
                // dodaj krawedz w druga strone do nowego grafu
                reversed.AddEdge(e.To, e.From);
            }

            return reversed;

        }

        // Część 2
        // Badanie czy graf jest dwudzielny
        //   0.5 pkt
        // Graf dwudzielny to graf nieskierowany, którego wierzchołki można podzielić na dwa rozłączne zbiory
        // takie, że dla każdej krawędzi jej końce należą do róźnych zbiorów
        // Parametry:
        //   g - badany graf
        //   vert - tablica opisująca podział zbioru wierzchołków na podzbiory w następujący sposób
        //          vert[i] == 1 oznacza, że wierzchołek i należy do pierwszego podzbioru
        //          vert[i] == 2 oznacza, że wierzchołek i należy do drugiego podzbioru
        // Wynik:
        //   true jeśli graf jest dwudzielny, false jeśli graf nie jest dwudzielny (w tym przypadku parametr vert ma mieć wartość null)
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Podział wierzchołków może nie być jednoznaczny - znaleźć dowolny
        //   3) Pamiętać, że każdy z wierzchołków musi być przyporządkowany do któregoś ze zbiorów
        //   4) Metoda ma mieć taki sam rząd złożoności jak zwykłe przeszukiwanie (za większą będą kary!)
        public bool Lab03IsBipartite(Graph g, out int[] vert)
        {
            vert = new int[g.VertexCount];


            for (int i = 0; i < g.VertexCount; i++)
            {
                // nieodwiedzony wierzchołek, wywolaj dfs

                if (vert[i] == 0)
                {
                    // zrob BFS od tego wierzcholka
                    int currentColor = 1;
                    vert[i] = currentColor;
                    IEnumerable<Edge> result = g.BFS().SearchFrom(i);
                    // przejdz wszystkie wierzcholki i na zmiane koloruj 
                    foreach (Edge e in result)
                    {
                        currentColor = vert[e.From];
                        if (vert[e.To] == 0)
                        {
                            vert[e.To] = 3 - currentColor;
                        }
                        else if (vert[e.To] != 3 - currentColor)
                        {
                            vert = null;
                            return false;
                        }

                        currentColor = 3 - currentColor;
                    }
                }


            }

            return true;
        }

        // Część 3
        // Wyznaczanie minimalnego drzewa rozpinającego algorytmem Kruskala
        //   1 pkt
        // Schemat algorytmu Kruskala
        //   1) wrzucić wszystkie krawędzie do "wspólnego worka"
        //   2) wyciągać z "worka" krawędzie w kolejności wzrastających wag
        //      - jeśli krawędź można dodać do drzewa to dodawać, jeśli nie można to ignorować
        //      - punkt 2 powtarzać aż do skonstruowania drzewa (lub wyczerpania krawędzi)
        // Parametry:
        //   g - graf wejściowy
        //   mstw - waga skonstruowanego drzewa (lasu)
        // Wynik:
        //   skonstruowane minimalne drzewo rozpinające (albo las)
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Wykorzystać klasę UnionFind z biblioteki Graph
        //   3) Jeśli graf g jest niespójny to metoda wyznacza las rozpinający
        //   4) Graf wynikowy (drzewo) musi być w takiej samej reprezentacji jak wejściowy
        public Graph<int> Lab03Kruskal(Graph<int> g, out int mstw)
        {
            mstw = 0;
            // szukamy rozpinajacego drzewa o najmniejszej sumie wag krawedzi
            // czyli minimalnego drzewa rozpinajacego 
            // zainicjiuj UNION FIND ze wszystkimi wierzcholkami
            UnionFind uf = new UnionFind(g.VertexCount);
            // zainicjuj graf wynikowy minimal spanning tree 
            Graph<int> mst = new Graph<int>(g.VertexCount, g.Representation);

            // zainicjuj kolejke priorytetowa krawedzi
            PriorityQueue<int, Edge<int>> pq = new PriorityQueue<int, Edge<int>>();
            // dodaj wszystkie krawedzie do kolejki priorytetowej
            foreach (Edge<int> e in g.DFS().SearchAll())
            {
                pq.Insert(e, e.Weight);
            }

            // dopoki kolejka nie jest pusta
            while (!pq.Count.Equals(0))
            {
                // wyjmij krawedz z kolejki
                Edge<int> e = pq.Extract();
                // jesli krawedz nie tworzy cyklu
                if (uf.Find(e.From) != uf.Find(e.To))
                {
                    // dodaj krawedz do drzewa
                    mst.AddEdge(e.From, e.To, e.Weight);
                    // dodaj wage krawedzi do wyniku
                    mstw += e.Weight;
                    // polacz wierzcholki
                    uf.Union(e.From, e.To);
                }
            }

            return mst;
        }

        // Część 4
        // Badanie czy graf nieskierowany jest acykliczny
        //   0.5 pkt
        // Parametry:
        //   g - badany graf
        // Wynik:
        //   true jeśli graf jest acykliczny, false jeśli graf nie jest acykliczny
        // Uwagi:
        //   1) Graf wejściowy pozostaje niezmieniony
        //   2) Najpierw pomysleć jaki, prosty do sprawdzenia, warunek spełnia acykliczny graf nieskierowany
        //      Zakodowanie tego sprawdzenia nie powinno zająć więcej niż kilka linii!
        //      Zadanie jest bardzo łatwe (jeśli wydaje się trudne - poszukać prostszego sposobu, a nie walczyć z trudnym!)
        public bool Lab03IsUndirectedAcyclic(Graph g)
        {
            // sprwadzic czy jest acykliczny czyli czy jest lasem lub drzewem 
            // czyli czy ma n-1 krawedzi i n wierzcholkow 
            // lub jesli ma skladowe to wtedy m skladowych i n - m krawedzi

            // najpierw liczba skladowych 

            int i = 1;
            int count = 0;
            int[] vert = new int[g.VertexCount];
            for (int j = 0; j < g.VertexCount; j++)
            {
                if (vert[j] == 0)
                {
                    vert[j] = i;
                    IEnumerable<Edge> result = g.BFS().SearchFrom(j);
                    foreach (Edge e in result)
                    {
                        vert[e.To] = i;
                    }

                    i++;
                }
            }

            // liczba skladowych
            count = i - 1;

            int edgeCount = g.EdgeCount;

            if (count == 1 && edgeCount == g.VertexCount - 1)
            {
                return true;
            }
            else if (count > 1 && edgeCount == g.VertexCount - count)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }

}
