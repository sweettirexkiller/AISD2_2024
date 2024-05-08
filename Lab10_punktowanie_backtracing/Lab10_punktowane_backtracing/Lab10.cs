using System;
using System.Collections.Generic;
using ASD.Graphs;

namespace ASD
{
    public class Lab10 : MarshalByRefObject
    {

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt>">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <returns>Informację czy istnieje droga przez labirytn oraz tablicę reprezentującą kolejne wierzchołki na drodze. W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.</returns>
        public (bool routeExists, int[] route) FindEscape(Graph labyrinth, int startingTorches, int[] roomTorches, int debt, int[] roomGold)
        {
            bool[] visited = new bool[labyrinth.VertexCount];
            List<int> path = new List<int>();

           
            int goldSum = 0;
            visited[0] = true;
            path.Add(0);
            bool found = FindEscapeRec(labyrinth, 0, ref path ,ref visited, ref startingTorches, roomTorches, ref goldSum, debt, roomGold);
            
            if(found)
            {
                int[] route = path.ToArray();
                return (true, route);
            }
            else
            {
                return (false, null);
            }
            
        }

        private bool FindEscapeRec(Graph labyrinth, int i, ref List<int> path, ref bool[] visited, ref int startingTorches, int[] roomTorches, ref int goldSum, int debt, int[] roomGold)
        {
            
            IEnumerable<int> neighbours = labyrinth.OutNeighbors(i);
            
            // jesli w pokoju jest zloto to je zabieramy
            if(roomGold[i] > 0)
            {
                goldSum += roomGold[i];
            }
            // jesli w tym pokoju sa pochodznie to zbieramy 
            if(roomTorches[i] > 0)
            {
                startingTorches += roomTorches[i];
            }



            // jesli jestesmy w ostatnim pokoju 
            if(i == labyrinth.VertexCount - 1)
            {
                // czy mamy wystarczajaca ilosc zlota i pochodni
                if(goldSum >= debt)
                {
                    // czy ma wystarczajaco pochodni
                    if (startingTorches >= 0)
                    {
                        return true;
                    }
                }
                else
                {
                    // cofamy zevranie zlota i pochodni 
                    if(roomGold[i] > 0)
                    {
                        goldSum -= roomGold[i];
                    }
                    if(roomTorches[i] > 0)
                    {
                        startingTorches -= roomTorches[i];
                    }   
                    return false;
                }
            }
            else if(startingTorches > 0)// nie jestesmy w ostatnim pokoju, zakladamy ze moglismy tu wejsc (sprawdzone wczesniej przed wejsciem w funckje)
            {
                // sprawdezamy czy mozemy przejsc do nastepnego pokoju
                
                foreach(int nextRoom in neighbours)
                {
                    
                    if(!visited[nextRoom])
                    {
                       
                        // wchodzimy wiec zuzywamy jedna pochodznie
                        // czy mamy jeszcze pochodnie by przejsc dalej ?
                        visited[nextRoom] = true;
                        startingTorches--;
                        path.Add(nextRoom);
                        if(FindEscapeRec(labyrinth,nextRoom , ref path, ref visited, ref startingTorches, roomTorches, ref goldSum, debt, roomGold))
                        {
                            return true;
                        }

                        path.Remove(nextRoom);
                        startingTorches++;
                        visited[nextRoom] = false;
                    }
                }
                
                
            }
            
            // cofamy zevranie zlota i pochodni 
            if(roomGold[i] > 0)
            {
                goldSum -= roomGold[i];
            }
            if(roomTorches[i] > 0)
            {
                startingTorches -= roomTorches[i];
            }

            return false;
        }

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <param name="dragonDelay">Opóźnienie z jakim wystartuje smok</param>
        /// <returns>Informację czy istnieje droga przez labirynt oraz tablicę reprezentującą kolejne wierzchołki na drodze. W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.</returns>

        public (bool routeExists, int[] route) FindEscapeWithHeadstart(Graph labyrinth, int startingTorches, int[] roomTorches, int debt, int[] roomGold, int dragonDelay)
        {
            return (false, null);
        }
    }
}
