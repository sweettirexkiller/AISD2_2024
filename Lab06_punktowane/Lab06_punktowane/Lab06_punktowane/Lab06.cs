using ASD.Graphs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
	public class Lab06 : MarshalByRefObject
	{
		private DiGraph<int> buildTravelingGraph(DiGraph<int> colorShiftGraph, Graph<int> cityGraph, int start, int target, out int superStartVertex, out int superTargetVertex)
		{
			DiGraph<int> travelingGraph = new DiGraph<int>(cityGraph.VertexCount*colorShiftGraph.VertexCount + 2);

			superStartVertex  = cityGraph.VertexCount*colorShiftGraph.VertexCount;
			superTargetVertex = cityGraph.VertexCount*colorShiftGraph.VertexCount + 1;
			
			int vertexCountInLayer = cityGraph.VertexCount;
			
			for(int layerColor = 0; layerColor < colorShiftGraph.VertexCount; layerColor++)
			{
				travelingGraph.AddEdge(superStartVertex, layerColor*vertexCountInLayer + start, 0);
				travelingGraph.AddEdge(layerColor*vertexCountInLayer + target, superTargetVertex, 0);

				foreach(Edge<int> street in cityGraph.BFS().SearchAll())
				{
					int colorRequired = street.Weight;
					int layerFrom = street.From + layerColor*vertexCountInLayer;
					int layerTo = street.To + layerColor*vertexCountInLayer;
					
					// krawedz jest w kolorze colorRequired wiec zostajemy na poziomie
					if (layerColor == colorRequired)
					{
						travelingGraph.AddEdge(layerFrom, layerTo, 1);
					}
					else
					{
						// jesli kameleon potrafii zmienic kolor to zmiec poziom i przejdz na ten wiercholek na tym poziomie
						if(colorShiftGraph.HasEdge(layerColor, colorRequired))
						{
							int toInDiffLayer = street.To + colorRequired*vertexCountInLayer;
							double effort = colorShiftGraph.GetEdgeWeight(layerColor, colorRequired);
							travelingGraph.AddEdge(layerFrom, toInDiffLayer, (int)effort+1);
						}
					}
				}
			}


			return travelingGraph;
		}
		/// <summary>Etap 1</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="start">Wierzchołek startowy (wejście z lasu).</param>
		/// <returns>Pierwszy element pary to informacja, czy rozwiązanie istnieje. Drugi element pary, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (bool possible, int[] path) Stage1(
			int n, DiGraph<int> c, Graph<int> g, int target, int start
		)
		{
			// int superStartVertex, superTargetVertex;
			// DiGraph<int> travelingGraph = this.buildTravelingGraph(c, g, start, target,out superStartVertex,out superTargetVertex);
			//
			// PathsInfo<int> pathInfo = Paths.Dijkstra(travelingGraph, (int)superStartVertex);
			//
			//
			//
			// if(!pathInfo.Reachable((int)superStartVertex,(int)superTargetVertex)) return (false, new int[0]);
			//
			// int[] pathIndexes = pathInfo.GetPath((int)superStartVertex, (int)superTargetVertex);
			//
			// // nie bedzie tam superStartVertex i superTargetVertex,reszta przetlumaczona na wierzcholki w g
			// int[] finalPath = new int[pathIndexes.Length - 2];
			//
			// // przetlucz wiercholki travelingGraph na wierzcholki w g (pomijamy superStartVertex i superTargetVertex)
			// for (int i = 1; i < pathIndexes.Length - 1; i++)
			// {
			// 	finalPath[i - 1] = pathIndexes[i] % g.VertexCount;
			// }
			
			
		    // return (true, finalPath);
		    
		    bool[,] visited = new bool[g.VertexCount, n];
		    (int,int)[,] prev = new (int,int)[g.VertexCount, n];
		    
		    // DFS / BFS
		    Stack<(int,int)> stack = new Stack<(int, int)>();
		    for (int i = 0; i < n; i++)
		    {
			    stack.Insert((start, i));
			    visited[start, i] = true;
		    }
		    bool found = false;
		    
		    while(stack.Count > 0 && !found)
		    {
		    	(int vertex, int enteringColor) = stack.Extract();
		        
		    	foreach(Edge<int> edge in g.OutEdges(vertex))
		    	{
		    		int colorRequired = edge.Weight;
		            if (enteringColor == colorRequired && !visited[edge.To, colorRequired])
		            {
			             stack.Insert((edge.To, colorRequired));
			             visited[edge.To, colorRequired] = true;
			             prev[edge.To, colorRequired] = (vertex, enteringColor);
			             if (edge.To == target)
			             {
				             found = true;
				             break;
			             }
		            }

		            if (c.HasEdge(enteringColor, colorRequired))
		            {
			            if (!visited[edge.To, colorRequired])
			            {
				            stack.Insert((edge.To, colorRequired));
				            visited[edge.To, colorRequired] = true;
				            prev[edge.To, colorRequired] = (vertex, enteringColor);
				            if (edge.To == target)
				            {
					            found = true;
					            break;
				            }
			            }
		            }
		        }
		    }
		    
		    for (int i = 0; i < n; i++)
		    {
			    if (visited[target, i])
			    {
				    List<int> path = new List<int>();
				    (int vertex, int color) = (target, i);
				    while (vertex != start)
				    {
					    path.Add(vertex);
					    (vertex, color) = prev[vertex, color];
				    }
				    path.Add(start);
				    path.Reverse();
				    return (true, path.ToArray());
			    }
		    }
		    
		    return (false, new int[0]);
		}

		/// <summary>Drugi etap</summary>
		/// <param name="n">Liczba kolorów (równa liczbie wierzchołków w c)</param>
		/// <param name="c">Graf opisujący możliwe przejścia między kolorami. Waga to wysiłek.</param>
		/// <param name="g">Graf opisujący drogi w mieście. Waga to kolor drogi.</param>
		/// <param name="target">Wierzchołek docelowy (dom Grzesia).</param>
		/// <param name="starts">Wierzchołki startowe (wejścia z lasu).</param>
		/// <returns>Pierwszy element pary to koszt najlepszego rozwiązania lub null, gdy rozwiązanie nie istnieje. Drugi element pary, tak jak w etapie 1, to droga będąca rozwiązaniem: sekwencja odwiedzanych wierzchołków (pierwszy musi być start, ostatni target). W przypadku, gdy nie ma rozwiązania, ma być tablica o długości 0.</returns>
		public (int? cost, int[] path) Stage2(
			int n, DiGraph<int> c, Graph<int> g, int target, int[] starts
		)
		{
			int superStartVertex, superTargetVertex;
			DiGraph<int> travelingGraph = this.buildTravelingGraphWithMultipleStarts(c, g, starts, target,out superStartVertex,out superTargetVertex);
			
			PathsInfo<int> pathInfo = Paths.Dijkstra(travelingGraph, (int)superStartVertex);
			
			
			
			if(!pathInfo.Reachable((int)superStartVertex,(int)superTargetVertex)) return (null, new int[0]);
			
			int[] pathIndexes = pathInfo.GetPath((int)superStartVertex, (int)superTargetVertex);
			// oblicz koszt
			int cost = 0;
			for (int i = 0; i < pathIndexes.Length - 1; i++)
			{
				cost += travelingGraph.GetEdgeWeight(pathIndexes[i], pathIndexes[i + 1]);
			}
			
			// nie bedzie tam superStartVertex i superTargetVertex,reszta przetlumaczona na wierzcholki w g
			int[] finalPath = new int[pathIndexes.Length - 2];
			
			// przetlucz wiercholki travelingGraph na wierzcholki w g (pomijamy superStartVertex i superTargetVertex)
			for (int i = 1; i < pathIndexes.Length - 1; i++)
			{
				finalPath[i - 1] = pathIndexes[i] % g.VertexCount;
			}
			
			
		    return (cost, finalPath);
		}

		private DiGraph<int> buildTravelingGraphWithMultipleStarts(DiGraph<int> colorShiftGraph, Graph<int> cityGraph, int[] starts, int target, out int superStartVertex, out int superTargetVertex)
		{
			DiGraph<int> travelingGraph = new DiGraph<int>(cityGraph.VertexCount*colorShiftGraph.VertexCount + 2);
			DiGraph<double> inverseShifts = new DiGraph<double>(colorShiftGraph.VertexCount);
			for (int i = 0; i < colorShiftGraph.VertexCount; i++)
			{
				foreach (Edge<int> edge in colorShiftGraph.OutEdges(i))
				{
					inverseShifts.AddEdge(edge.To, edge.From, edge.Weight);
				}
			}

			superStartVertex  = cityGraph.VertexCount*colorShiftGraph.VertexCount;
			superTargetVertex = cityGraph.VertexCount*colorShiftGraph.VertexCount + 1;
			
			int vertexCountInLayer = cityGraph.VertexCount;
			
			for(int layerColor = 0; layerColor < colorShiftGraph.VertexCount; layerColor++)
			{
			
				foreach(int start in starts)
				{
					travelingGraph.AddEdge(superStartVertex, layerColor*vertexCountInLayer + start, 0);
				}
				travelingGraph.AddEdge(layerColor*vertexCountInLayer + target, superTargetVertex, 0);
			}
			
			for(int i = 0; i < cityGraph.VertexCount; i++)
			{
				foreach(Edge<int> street in  cityGraph.OutEdges(i))
				{
					int colorRequired = street.Weight;
					// bez zmiany koloru
					int layerFrom = street.From + colorRequired*vertexCountInLayer;
					int layerTo = street.To + colorRequired*vertexCountInLayer;
					travelingGraph.AddEdge(layerFrom, layerTo, 1);
					
					
					foreach(Edge<double> inverseColorShift in inverseShifts.OutEdges(colorRequired))
					{
						int enteringLayer = inverseColorShift.To;
						
						layerFrom = street.From + enteringLayer*vertexCountInLayer;
						layerTo = street.To + colorRequired*vertexCountInLayer;
						travelingGraph.AddEdge(layerFrom, layerTo, (int)inverseColorShift.Weight+1);
					}

				}
					
			}


			return travelingGraph;
		}
	}
}
