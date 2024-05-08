
using System.Collections.Generic;
using System.Linq;
using ASD.Graphs;

/// <summary>
/// Klasa rozszerzająca klasę Graph o rozwiązania problemów największej kliki i izomorfizmu grafów metodą pełnego przeglądu (backtracking)
/// </summary>
public static class Lab10GraphExtender
{
    /// <summary>
    /// Wyznacza największą klikę w grafie i jej rozmiar metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Badany graf</param>
    /// <param name="clique">Wierzchołki znalezionej największej kliki - parametr wyjściowy</param>
    /// <returns>Rozmiar największej kliki</returns>
    /// <remarks>
    /// Nie wolno modyfikować badanego grafu.
    /// </remarks>
    public static int MaxClique(this Graph g, out int[] clique)
    {
        List<int> S = new List<int>(); // S ← ∅
        List<int> bestS = new List<int>(); // bestS ← ∅
        MaxCliqueRec(0, ref g, ref S, ref bestS);
       
        clique = bestS.ToArray();
        return bestS.Count;
    }
    
    // na nastepnych labach mozliwe ze bedzie trzeba znalesc nawiekszy zbior niezalezny backtracingkiem

    private static void MaxCliqueRec(int k, ref Graph g, ref List<int> S, ref List<int> bestS )
    {
        // C ← zbiór wierzchołków z zakresu {k, k + 1, . . . , n − 1} sąsiadujących z każdym wierzchołkiem z S
        List<int> C = new List<int>();
        for (int i = k; i < g.VertexCount; i++)
        {
            bool isNeighbourWithAllFromS = true;
            foreach(int v in S)
            {
                if (g.HasEdge(i, v) == false || g.HasEdge(v, i) == false)
                {
                    isNeighbourWithAllFromS = false;
                    break;
                }
            }
            if (isNeighbourWithAllFromS)
            {
                C.Add(i);
            }
        }
        
        
        if(C.Count + S.Count <= bestS.Count)
        {
            return;
        }
        else
        {
            if(S.Count > bestS.Count)
            {
                bestS = new List<int>(S);
            }
        }
        
       foreach(int m in C)
       {
           S.Add(m);
           MaxCliqueRec(m + 1, ref g, ref S, ref bestS);
           S.Remove(m);
       }
    }
    

    /// <summary>
    /// Bada izomorfizm grafów metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <param name="g">Pierwszy badany graf</param>
    /// <param name="h">Drugi badany graf</param>
    /// <param name="map">Mapowanie wierzchołków grafu h na wierzchołki grafu g (jeśli grafy nie są izomorficzne to null) - parametr wyjściowy</param>
    /// <returns>Informacja, czy grafy g i h są izomorficzne</returns>
    /// <remarks>
    /// 1) Uwzględniamy wagi krawędzi
    /// 3) Nie wolno modyfikować badanych grafów.
    /// </remarks>
    public static bool IsomorphismTest(this Graph<int> g, Graph<int> h, out int[] map)
    {
        map = null;
        if (g.VertexCount != h.VertexCount)
        {
            return false;
        }
        
        if(g.EdgeCount != h.EdgeCount)
        {
            return false;
        }

        int[] permutaion  = new int[g.VertexCount];
        bool[] used = new bool[g.VertexCount];

        return isIsomorphic(0, ref g, ref h, ref permutaion, ref used, ref map);
    }

    private static bool isIsomorphic(int i, ref Graph<int> g, ref Graph<int> h, ref int[] permutation, ref bool[] used, ref int[] map)
    {
       
        if(i == g.VertexCount)
        {
            map = permutation;
            return true;
        }

        for (int u = 0; u < g.VertexCount; u++)
        {
            if(!used[u])
            {
                used[u] = true;
                // wierzcholkowi i z g przypisujemy wierzcholek u z h
                // czy tak mozna ?
                // czy ma ten sam stopien jak wierzcholek i z g
                if(g.OutEdges(i).Count() != h.OutEdges(u).Count())
                {
                    used[u] = false;
                    continue;
                }
                // czy sasiaduje z takimi samymi wierzcholkami jak wierzcholek i z g
                bool isNeighbourhoodValid = true;
                foreach (var edge in g.OutEdges(i))
                {
                    // krwaedz i - v w G
                    int v = edge.To;
                    int w = edge.Weight;
                    // znajdz wierzcholek z h ktory jest przypisany do v
                    if (used[v]) 
                    {
                        int vInH = permutation[v];
                        //jesli v ma mapowanie to krawedz u - vInH musi byc krawedzia w h i miec wage w
                        if (!h.HasEdge(u, vInH) || h.GetEdgeWeight(u, vInH) != w)
                        {
                            isNeighbourhoodValid = false;
                            break;
                        }
                    }
                }
                if (!isNeighbourhoodValid)
                {
                    used[u] = false;
                    continue;
                }
                permutation[i] = u;
                isIsomorphic(i + 1, ref g, ref h, ref permutation, ref used, ref map);
                used[u] = false;
            }
        }
        
        map = null;
        return false;
    }

}

