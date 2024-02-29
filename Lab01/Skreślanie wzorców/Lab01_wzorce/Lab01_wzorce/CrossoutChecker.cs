using System;

namespace ASD
{
    class CrossoutChecker
    {
        /// <summary>
        /// Sprawdza, czy podana lista wzorców zawiera wzorzec x
        /// </summary>
        /// <param name="patterns">Lista wzorców</param>
        /// <param name="x">Jedyny znak szukanego wzorca</param>
        /// <returns></returns>
        bool comparePattern(char[][] patterns, char x)
        {
            foreach (char[] pat in patterns)
            {
                if (pat.Length == 1 && pat[0] == x)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Sprawdza, czy podana lista wzorców zawiera wzorzec xy
        /// </summary>
        /// <param name="patterns">Lista wzorców</param>
        /// <param name="x">Pierwszy znak szukanego wzorca</param>
        /// <param name="y">Drugi znak szukanego wzorca</param>
        /// <returns></returns>
        bool comparePattern(char[][] patterns, char x, char y)
        {
            foreach (char[] pat in patterns)
            {
                if (pat.GetLength(0) == 2 && pat[0] == x && pat[1] == y)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Metoda sprawdza, czy podany ciąg znaków można sprowadzić do ciągu pustego przez skreślanie zadanych wzorców.
        /// Zakładamy, że każdy wzorzec składa się z jednego lub dwóch znaków!
        /// </summary>
        /// <param name="sequence">Ciąg znaków</param>
        /// <param name="patterns">Lista wzorców</param>
        /// <param name="crossoutsNumber">Minimalna liczba skreśleń gwarantująca sukces lub int.MaxValue, jeżeli się nie da</param>
        /// <returns></returns>
        public bool Erasable(char[] sequence, char[][] patterns, out int crossoutsNumber)
        {
            
            // tablica przechowująca informację, czy dany podciąg jest erasable
            bool[,] erasable = new bool[sequence.Length, sequence.Length];
            crossoutsNumber = -1;
            // zewnetrzna petla iteruje po kolejnych znakach ciagu sequence
            for (int i = 0; i < sequence.Length; i++)
            {
                // wewnetrzna petla iteruje sie od tylu po sequence
                for (int j = sequence.Length - 1; j >= 0; j--)
                {
                    // jesli jestesmy ponizej diagonali to pusty podciag, wiec dajemy ze erasable
                    if (j < i)
                    {
                        erasable[i, j] = true;
                    }
                    else
                    {
                        // podciag sekwenji od i do j
                        char[] subsequence = new char[j - i + 1];
                        Array.Copy(sequence, i, subsequence, 0, j - i + 1);
                        // od diagonali i wyzej sprawdzamy kazdy z 3 pozostalych warunkow dla podciagu od i do j
                       // warunek 2. jesli podcaig ma dlugosc 1 lub 2 to sprawdz comparePatterns
                        if (subsequence.Length == 1)
                        {
                            erasable[i, j] = comparePattern(patterns, subsequence[0]);
                        }
                        else if (subsequence.Length == 2)
                        {
                            erasable[i, j] = comparePattern(patterns, subsequence[0], subsequence[1]);
                        }
                        else
                        {
                            // warunek 3. sprawdzamy czy podciag od i do j jest konkatenacja dwoch krotszych podciagow
                            for (int k = i-1, l = sequence.Length- 1;  k >= 0 && l > j; k--, l--)
                            {
                                // o tyle ile mozna sie przesunac w lewo o tyle sume podciagow "od 0 do k" + " od sequence.Length do l"
                                // wspolrzedne 1 podciagu to i, k a wspolrzedne 2 podciagu to l, j
                                if (erasable[i, k] && erasable[l, j])
                                {
                                    erasable[i, j] = true;
                                    break;
                                }
                            }
                            // warunek 4. czy zawiera spojny fragment wycieralny a to co pozostaje jest jednym z wzorcow
                            if (!erasable[i, j])
                            {
                                
                                // spojne fragmenty sa albo w tej samej kolumnie albo w tej samej linii
                                // wszystkie mozliwe spojne fragmenty od poczatku do i
                                for (int k = 0; k < i; k++)
                                {
                                    // czy od 0 do k (w linii )jest wyceiralne ?
                                    if (erasable[0, k])
                                    {
                                        // fragment od k do i to to co pozostaje
                                        char[] subsequence1 = new char[i - k + 1];
                                        Array.Copy(sequence, k, subsequence1, 0, i - k + 1);
                                        //  czy jest on wzorcem ?
                                        // jesli dlugosci 1 to po prostu sprawdzamy czy jest wzorcem
                                        if (subsequence1.Length == 1)
                                        {
                                            if (comparePattern(patterns, subsequence1[0]))
                                            {
                                                erasable[i, j] = true;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            // jesli dlugosci 2 to sprawdzamy czy jest wzorcem
                                            if (subsequence1.Length == 2)
                                            {
                                                if (comparePattern(patterns, subsequence1[0], subsequence1[1]))
                                                {
                                                    erasable[i, j] = true;
                                                    break;
                                                }
                                            }
                                        }
                                    } 
                                    
                                    // czy po kolumnie od j do sequence.Length jest wyceiralne ?
                                    if (j + k < sequence.Length)
                                    {
                                        if (erasable[j + k, sequence.Length - 1])
                                        {
                                            // fragment od j do sequence.Length to to co pozostaje
                                            char[] subsequence2 = new char[sequence.Length - 1 - j + 1];
                                            Array.Copy(sequence, j, subsequence2, 0, sequence.Length - 1 - j + 1);
                                            //  czy jest on wzorcem ?
                                            // jesli dlugosci 1 to po prostu sprawdzamy czy jest wzorcem
                                            if (subsequence2.Length == 1)
                                            {
                                                if (comparePattern(patterns, subsequence2[0]))
                                                {
                                                    erasable[i, j] = true;
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                // jesli dlugosci 2 to sprawdzamy czy jest wzorcem
                                                if (subsequence2.Length == 2)
                                                {
                                                    if (comparePattern(patterns, subsequence2[0], subsequence2[1]))
                                                    {
                                                        erasable[i, j] = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
          
            // ostatnia kolumna pierwszy rzad 
            bool ret = erasable[0,sequence.Length - 1];
           
           return ret;
        }

        /// <summary>
        /// Metoda sprawdza, jaka jest minimalna długość ciągu, który można uzyskać z podanego poprzez skreślanie zadanych wzorców.
        /// Zakładamy, że każdy wzorzec składa się z jednego lub dwóch znaków!
        /// </summary>
        /// <param name="sequence">Ciąg znaków</param>
        /// <param name="patterns">Lista wzorców</param>
        /// <returns></returns>
        public int MinimumRemainder(char[] sequence, char[][] patterns)
        {
            return -1;
        }

        // można dopisać metody pomocnicze

    }
}
