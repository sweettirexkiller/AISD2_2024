
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
        
        Dictionary<int, int> vertex_mapping = new Dictionary<int, int>();

        return isIsomorphic(0, ref g, ref h, ref vertex_mapping, ref map);
    }

    private static bool isIsomorphic(int i, ref Graph<int> g, ref Graph<int> h, ref Dictionary<int, int> vertexMapping, ref int[] map)
    {
       
        if(i == g.VertexCount)
        {
            map = vertexMapping.Values.ToArray();
            return true;
        }

        for (int u = 0; u < h.VertexCount; u++)
        {
            if(isMappingValid(i, u, ref vertexMapping))
            {
                vertexMapping.Add(i, u);
                if(isIsomorphic(i + 1, ref g, ref h, ref vertexMapping, ref map))
                {
                    return true;
                }
                vertexMapping.Remove(i);
            }
        }
        
        return false;
    }

    private static bool isMappingValid(int v1, int v2, ref Dictionary<int, int> vertex_mapping)
    {
        // czy v1 ma juz przypisany wierzcholek w v2 ?
        if (vertex_mapping.ContainsKey(v1))
        {
            return vertex_mapping[v1] == v2;
        }
        else
        { // czy v2 jest juz przypisane do innego wierzcholka ?
            if (vertex_mapping.ContainsValue(v2))
            {
                return false;
            }
            else
            {
                // vertex_mapping.Add(v1, v2);
                return true;
            }
            
        }

    }

}

