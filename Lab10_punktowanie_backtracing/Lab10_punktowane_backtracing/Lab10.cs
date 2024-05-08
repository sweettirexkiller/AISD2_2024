using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        public (bool routeExists, int[] route) FindEscape(Graph labyrinth, int startingTorches, int[] roomTorches,
            int debt, int[] roomGold)
        {
            bool[] visited = new bool[labyrinth.VertexCount];
            List<int> path = new List<int>();


            int goldSum = 0;
            visited[0] = true;
            path.Add(0);
            bool found = FindEscapeRec(labyrinth, 0, ref path, ref visited, ref startingTorches, roomTorches,
                ref goldSum, debt, roomGold);

            if (found)
            {
                int[] route = path.ToArray();
                return (true, route);
            }
            else
            {
                return (false, null);
            }

        }

        private bool FindEscapeRec(Graph labyrinth, int i, ref List<int> path, ref bool[] visited,
            ref int startingTorches, int[] roomTorches, ref int goldSum, int debt, int[] roomGold)
        {

            // jesli w pokoju jest zloto to je zabieramy
            if (roomGold[i] > 0)
            {
                goldSum += roomGold[i];
            }

            // jesli w tym pokoju sa pochodznie to zbieramy 
            if (roomTorches[i] > 0)
            {
                startingTorches += roomTorches[i];
            }

            // jesli jestesmy w ostatnim pokoju 
            if (i == labyrinth.VertexCount - 1)
            {
                // czy mamy wystarczajaca ilosc zlota i pochodni
                if (goldSum >= debt)
                {
                  return true;
                }
            }
            else  // nie jestesmy w ostatnim pokoju, zakladamy ze moglismy tu wejsc (sprawdzone wczesniej przed wejsciem w funckje czy mamy wystarczajaco pochodni)
            {
                // sprawdezamy czy mozemy przejsc do nastepnego pokoju

                foreach (int nextRoom in labyrinth.OutNeighbors(i))
                {

                    // jesli nie bylismy jeszcze w tym pokoju
                    if (!visited[nextRoom] && startingTorches > 0)
                    {

                        // wchodzimy wiec zuzywamy jedna pochodznie
                        // czy mamy jeszcze pochodnie by przejsc dalej ?
                        visited[nextRoom] = true;
                        startingTorches--;
                        path.Add(nextRoom);
                        if (FindEscapeRec(labyrinth, nextRoom, ref path, ref visited, ref startingTorches, roomTorches,
                                ref goldSum, debt, roomGold))
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
            if (roomGold[i] > 0)
            {
                goldSum -= roomGold[i];
            }

            if (roomTorches[i] > 0)
            {
                startingTorches -= roomTorches[i];
            }

            return false;
        }
        
        public int[] FindLoop(LinkedList<int> dragonToHeroPath)
        {
            if(dragonToHeroPath.First == null || dragonToHeroPath.First.Next == null)
                return null;
            

            LinkedListNode<int> slowRunner = dragonToHeroPath.First;
            LinkedListNode<int> fastRunner = dragonToHeroPath.First;

            // Znajdowanie miejsca spotkania biegaczy
            while (fastRunner != null && fastRunner.Next != null)
            {
                slowRunner = slowRunner.Next;
                fastRunner = fastRunner.Next.Next;

                if (slowRunner == fastRunner)
                    break;
            }

            // Sprawdzanie czy istnieje pętla
            if (slowRunner != fastRunner)
                return null;

            // Ustawienie jednego z biegaczy na głowę listy
            slowRunner = dragonToHeroPath.First;

            List<LinkedListNode<int>> loopNodes = new List<LinkedListNode<int>>();

            // Przesuwanie biegaczy po jednym kroku, aż się spotkają ponownie
            while (slowRunner != fastRunner)
            {
                slowRunner = slowRunner.Next;
                fastRunner = fastRunner.Next;
            }

            // Dodawanie wierzchołków pętli do tablicy
            do
            {
                loopNodes.Add(slowRunner);
                slowRunner = slowRunner.Next;
            } while (slowRunner != fastRunner);

            // Zwracanie tablicy wierzchołków pętli
            return loopNodes.Select(node => node.Value).ToArray();
        }

        /// <param name="labyrinth">Graf reprezentujący labirynt</param>
        /// <param name="startingTorches">Ilość pochodni z jaką startują bohaterowie</param>
        /// <param name="roomTorches">Ilość pochodni w poszczególnych pokojach</param>
        /// <param name="debt">Ilość złota jaką bohaterowie muszą zebrać</param>
        /// <param name="roomGold">Ilość złota w poszczególnych pokojach</param>
        /// <param name="dragonDelay">Opóźnienie z jakim wystartuje smok</param>
        /// <returns>Informację czy istnieje droga przez labirynt oraz tablicę reprezentującą kolejne wierzchołki na drodze. W przypadku, gdy zwracany jest false, wartość tego pola powinna być null.</returns>

        public (bool routeExists, int[] route) FindEscapeWithHeadstart(Graph labyrinth, int startingTorches,
            int[] roomTorches, int debt, int[] roomGold, int dragonDelay)
        {
            // ile razy odwiedzony pokoj, zainicjalizowane na 0
            int[] visited = new int[labyrinth.VertexCount];
            bool[] visitedByDragon = new bool[labyrinth.VertexCount];
            // przebyta sciezka przez bohatera 
            List<int> path = new List<int>();
            // sciezka przebyta przez smoka
            List<int> dragonPath = new List<int>();
            // lista z wiercholkami najkrotszej sciezki do przejscia przez smoka do bohatera 
            LinkedList<int> pathToFollowByDragon = new LinkedList<int>();

            
            // najlepsza ilosc zebranego zlota i ilosc pochodni z jakimi pojawil sie bohater w wierzcholku
            // jesli pojawil sie kilka razy do wybieramy najmniejszy dlug, jesli sa takie same to wybieramy ten z wieksza iloscia pochodni
            // <zloto, pochodnie>
            Tuple<int, int>[] bestResultInVertex = new Tuple<int, int>[labyrinth.VertexCount];
            for (int i = 0; i < labyrinth.VertexCount; i++)
            {
                bestResultInVertex[i] = new Tuple<int, int>(debt+1, -1);
            }

            int goldSum = 0;
            visited[0] = 1;
            path.Add(0);
            pathToFollowByDragon.AddFirst(0);
            
            bool found = FindEscapeRecWithDragon(labyrinth,ref bestResultInVertex, ref pathToFollowByDragon, ref visitedByDragon, 0, ref path,ref dragonPath,ref dragonDelay, ref visited, ref startingTorches, roomTorches,
                ref goldSum, debt, roomGold);

            if (found)
            {
                int[] route = path.ToArray();
                return (true, route);
            }
            else
            {
                return (false, null);
            }
        }
        /// <summary>
        /// Backtracking z smokiem 
        /// </summary>
        /// <param name="labyrinth"></param>
        /// <param name="pathToFollowByDragon"></param>
        /// <param name="visitedByDragon"></param>
        /// <param name="i"></param>
        /// <param name="path"></param>
        /// <param name="dragonPath"></param>
        /// <param name="dragonDelay"></param>
        /// <param name="visited"></param>
        /// <param name="startingTorches"></param>
        /// <param name="roomTorches"></param>
        /// <param name="goldSum"></param>
        /// <param name="debt"></param>
        /// <param name="roomGold"></param>
        /// <returns>czy istnieje sciezka ze smok nie dogoni, starczy latarni i zbierze sie wystarczajaco zlota by wrocic</returns>
        private bool FindEscapeRecWithDragon(Graph labyrinth,ref Tuple<int, int>[] bestResultInVertex , ref  LinkedList<int> pathToFollowByDragon, ref bool[] visitedByDragon, int i, ref List<int> path, ref List<int> dragonPath, ref int dragonDelay, ref int[] visited,
            ref int startingTorches, int[] roomTorches, ref int goldSum, int debt, int[] roomGold)
        {

            // Console.WriteLine();
            // Console.Write("i=" + i + ", ");
            // Console.Write("path=[{0}], ", string.Join(",", path));
            // Console.Write("dragonPath=[{0}], ", string.Join(",", dragonPath));
            // Console.Write("pathToFollow=[{0}], ", string.Join(",", pathToFollowByDragon));
            // Console.Write("visited=[{0}], ", string.Join(",", visited));
            // Console.Write("visitedByDragon=[{0}], ", string.Join(",", visitedByDragon.Select(x => Convert.ToInt16(x))));

            int torchesTaken = 0;
            int goldTaken = 0;
            

            // jesli w pokoju jest zloto to je zabieramy
            if (roomGold[i] > 0)
            {
                goldSum += roomGold[i];
                goldTaken = roomGold[i];
                roomGold[i] = 0;
            }

            // jesli w tym pokoju sa pochodznie to zbieramy 
            if (roomTorches[i] > 0)
            {
                startingTorches += roomTorches[i];
                torchesTaken = roomTorches[i];
                roomTorches[i] = 0;
            }
            
            if(visitedByDragon[i])
            {
               return false;
            }
            
           
            // jesli jestesmy w ostatnim pokoju
            if (i == labyrinth.VertexCount - 1)
            {
                // czy mamy wystarczajaca ilosc zlota i pochodni
                if (goldSum >= debt)
                {
                    return true;
                }
            }
            
           if(startingTorches> 0) 
           {
               IEnumerable<int> neigh = labyrinth.OutNeighbors(i);
                // sprawdezamy czy mozemy przejsc do nastepnego pokoju
                foreach (int nextRoom in neigh)
                {
                    // smok jeszcze nie odwiedzil i mamy pochodnie by isc do nastepnego pokoju
                    if (!visitedByDragon[nextRoom] && startingTorches > 0)
                    {
                        // wchodzimy wiec zuzywamy jedna pochodznie
                        // czy mamy jeszcze pochodnie by przejsc dalej ?
                        visited[nextRoom]++;
                        startingTorches--;
                        path.Add(nextRoom);
                        dragonDelay--;
                        // dodajemy wierzcholek do sciezki ktora smok bedzie podazal
                        pathToFollowByDragon.AddLast(nextRoom);
                        LinkedList<int> copyWithLoops = new LinkedList<int>();
                        foreach (var element in pathToFollowByDragon)
                        {
                            copyWithLoops.AddLast(element);
                        }
                        
                        // kopiuj liczb odwiedzen wierzcholkow
                        int[] visitedCopy = new int[labyrinth.VertexCount];
                        for (int j = 0; j < visited.Length; j++)
                        {
                            visitedCopy[j] = visited[j];
                        }

                        int nextRoomForDragon = int.MaxValue;

                        if(dragonDelay < 0) // smok wychodzi z jamy i zaczyna gonic bohatera
                        {
                            
                            // trzeba znaelzc najkrotsza sciezke do hero z miejsca gdzie jest smok - usuwamy petle w pathToFollowByDragon
                            // petle przez ktore przechodzil bohater ale smok nie bedzie w nie wchodzic
                            // rozwazamy ten wierzcholek jako nastepny wierzcholek do odwiedzenia przez smoka
                           
                            // odwiedz kazdy wiechrzolek z pathToFollowByDragon po kolei i znajdz wszystie wierzcholki ktore sa w petlach
                            LinkedListNode<int> node = pathToFollowByDragon.First;
                            while(node != null && node.Next != null)
                            {
                                if(visited[node.Value]>1)
                                {
                                    // znalezlismy wierzcholek w petli
                                    // znajdzmy wszystkie wierzcholki w petli
                                    // sprawdz czy ta petla na pewno istnieje zanim usuniesz wierzcholki 
                                    
                                    bool isLoop = false;
                                    LinkedListNode<int> tmp = node;
                                    while(tmp.Next != null)
                                    {
                                        if(tmp.Next.Value == node.Value)
                                        {
                                            isLoop = true;
                                            break;
                                        }
                                        tmp = tmp.Next;
                                    }
                                    if(!isLoop)
                                    {
                                        node = node.Next;
                                        continue;
                                    }
                                    
                                    visited[node.Value]--;
                                    LinkedListNode<int> loopElement = node;
                                    //usuwamy ten element
                                    LinkedListNode<int> loopElementToRemove = loopElement;
                                    loopElement = loopElement.Next;
                                    
                                    pathToFollowByDragon.Remove(pathToFollowByDragon.Find(loopElementToRemove.Value));
                                    
                                    while (loopElement.Value != node.Value && loopElement.Next != null)
                                    {
                                        loopElementToRemove = loopElement;
                                        loopElement = loopElement.Next;
                                        pathToFollowByDragon.Remove(pathToFollowByDragon.Find(loopElementToRemove.Value));
                                    }
                                    node = loopElement;
                                }

                                node = node.Next;
                            }

                            // smok wykonuje krok za bohaterem
                            nextRoomForDragon = pathToFollowByDragon.First.Value;
                            dragonPath.Add(nextRoomForDragon);
                            visitedByDragon[nextRoomForDragon] = true;
                            pathToFollowByDragon.RemoveFirst();
                            // nextRoomForDragon to wierzcholek do ktorego smok wchodzi w tym kroku
                            // jesli smok jest w wierzcholku w ktorym jest bohater to koniec
                            if (nextRoomForDragon == nextRoom)
                            {
                                dragonPath.RemoveAt(dragonPath.Count - 1);
                                visitedByDragon[nextRoomForDragon] = false;
                                pathToFollowByDragon = copyWithLoops;
                                visited = visitedCopy;
                                if (pathToFollowByDragon.Count > 0)
                                {
                                    pathToFollowByDragon.RemoveLast();
                                }
                       
                                dragonDelay++;
                                path.RemoveAt(path.Count - 1);
                                startingTorches++;
                                visited[nextRoom]--;
                                
                                continue;
                            }

                        }

                        if (FindEscapeRecWithDragon(labyrinth,ref bestResultInVertex,ref pathToFollowByDragon,ref visitedByDragon, nextRoom, ref path, ref dragonPath,ref dragonDelay, ref visited, ref startingTorches,
                                roomTorches, ref goldSum, debt, roomGold))
                        {
                            return true;
                        }

                        // cofamy poprzedni ruch smoka
                        if (dragonDelay < 0 && nextRoomForDragon != int.MaxValue)
                        {
                            // usun ostatnio dodany element do sciezki smoka
                            dragonPath.RemoveAt(dragonPath.Count - 1);
                            visitedByDragon[nextRoomForDragon] = false;
                            pathToFollowByDragon = copyWithLoops;
                            visited = visitedCopy;
                          
                        }

                        if (pathToFollowByDragon.Count > 0)
                        {
                            pathToFollowByDragon.RemoveLast();
                        }
                       
                        dragonDelay++;
                        path.RemoveAt(path.Count - 1);
                        startingTorches++;
                        visited[nextRoom]--;
                    }
                }
            }
            
    
            
            // cofamy zevranie zlota i pochodni
            if (goldTaken > 0)
            {
                goldSum -= goldTaken;
                roomGold[i] = goldTaken;
            }
            
            if (torchesTaken > 0)
            {
                startingTorches -= torchesTaken;
                roomTorches[i] = torchesTaken;
            }
            
            
            return false;
        }
    }
}
