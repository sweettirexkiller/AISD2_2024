using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Labratoria_ASD2_2024
{
    public class Lab14 : MarshalByRefObject
    {
        /// <summary>
        /// Znajduje wszystkie maksymalne palindromy długości przynajmniej 2 w zadanym słowie. Wykorzystuje Algorytm Manachera.
        /// 
        /// Palindromy powinny być zwracane jako lista par (indeks pierwszego znaku, długość palindromu), 
        /// tzn. para (i, d) oznacza, że pod indeksem i znajduje się pierwszy znak d-znakowego palindromu.
        /// 
        /// Kolejność wyników nie ma znaczenia.
        /// 
        /// Można założyć, że w tekście wejściowym nie występują znaki '#' i '$' - można je wykorzystać w roli wartowników
        /// </summary>
        /// <param name="text">Tekst wejściowy</param>
        /// <returns>Tablica znalezionych palindromów</returns>
        public (int startIndex, int length)[] FindPalindromes(string text)
        {
            //długości palindromów dla każdego znaku oryginalnego ciągu s
            // List<int> oddPalindromes = ManacherOdd(text);
            List<int> evenPalindromes = ManacherEven(text);
            
            // przeksztalc promienie palindromow na poczatki i dlugosci
            List<(int, int)> result = new List<(int, int)>();
            for (int i = 0; i < text.Length; i++)
            {
                // if (oddPalindromes[i] >= 1)
                // {
                //     result.Add((i - oddPalindromes[i], oddPalindromes[i]*2+1));
                // }
                if (evenPalindromes[i] > 1)
                {
                    result.Add((i - evenPalindromes[i] + 1 , evenPalindromes[i]*2));
                }
            }
       
            return result.ToArray();
        }
        
        public static List<int> ManacherOdd(string s)
        {
            int n = s.Length;
            // lewa i prawa granica aktualnego palindromu (poczatkowo nie ma palindromu)
            int L = 0, R = -1;
            // tablica promieni palindromow
            int[] oddP = new int[n];

            for (int i = 0; i < n; i++)
            {
                // jesli jestesmy wewnatrz obecnego palindromu
                if (i < R)
                {
                    // to promien palindromu i-tego znaku to minimum
                    // z promienia lustrzanego i-tego znaku oraz odleglosci od prawej granicy (by nie wychodzilo poza prawa granice)
                    oddP[i] = Math.Min(oddP[L + R - i] /*lustrzane odbicie*/, R - i);
                }
                else
                { // rozwazamy jakis znak poza obecnymi znalezionymi palindromami
                    oddP[i] = 0;
                }

                // lewa granica aktualnego palindromu
                int left = i - oddP[i] - 1;
                // prawa granica aktualnego palindromu
                int right = i + oddP[i] + 1;

                // poszerzanie palindromu
                while (left >= 0 && right < n && s[left] == s[right])
                {
                    oddP[i]++;
                    left--;
                    right++;
                }

                // jesli nowy palindrom wychodzi poza prawa granice to aktualizujemy granice
                if (i + oddP[i] > R)
                {
                    // nowy lewy koniec ostatniego palindromu
                    L = i - oddP[i];
                    // nowy prawy koniec ostatniego palindromu
                    R = i + oddP[i];
                }
            }

            return new List<int>(oddP);
        }
        
        public static List<int> ManacherEven(string s)
        {
            int n = s.Length;
            int L = 0, R = -1;
            int[] evenP = new int[n];

            for (int i = 0; i < n; i++)
            {
                if (i < R)
                {
                    evenP[i] = Math.Min(evenP[L + R - i], R - i);
                }
                else
                {
                    evenP[i] = 0;
                }

                // to jedyna roznica miedzy parzystymi a nieparzystymi palindromami!!!
                int left = i - evenP[i];
                int right = i + evenP[i] + 1;

                while (left >= 0 && right < n && s[left] == s[right])
                {
                    evenP[i]++;
                    left--;
                    right++;
                }

                if (i + evenP[i] > R)
                {
                    L = i - evenP[i] + 1;
                    R = i + evenP[i];
                }
            }

            return new List<int>(evenP);
        }

    }

}
