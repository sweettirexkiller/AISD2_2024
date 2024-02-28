
using System;
using System.Data;

namespace ASD
{

    class ChangeMaking
    {

        /// <summary>
        /// Metoda wyznacza rozwiązanie problemu wydawania reszty przy pomocy minimalnej liczby monet
        /// bez ograniczeń na liczbę monet danego rodzaju
        /// </summary>
        /// <param name="amount">Kwota reszty do wydania</param>
        /// <param name="coins">Dostępne nominały monet</param>
        /// <param name="change">Liczby monet danego nominału użytych przy wydawaniu reszty</param>
        /// <returns>Minimalna liczba monet potrzebnych do wydania reszty</returns>
        /// <remarks>
        /// coins[i]  - nominał monety i-tego rodzaju
        /// change[i] - liczba monet i-tego rodzaju (nominału) użyta w rozwiązaniu
        /// Jeśli dostepnymi monetami nie da się wydać danej kwoty to change = null,
        /// a metoda również zwraca null
        ///
        /// Wskazówka/wymaganie:
        /// Dodatkowa uzyta pamięć powinna (musi) być proporcjonalna do wartości amount ( czyli rzędu o(amount) )
        /// </remarks>
        public int? NoLimitsDynamic(int amount, int[] coins, out int[] change)
        {

            int[] T = new int[amount + 1];
            int[] P = new int[amount + 1];
            T[0] = 0;
            int kk;
            for(kk = 1; kk <= amount; kk++)
            {
                // i to kwota, tab[i] to liczba monet potrzebna do wydania
                T[kk] = int.MaxValue - 1;
                for (int l = 0; l< coins.Length; l++)
                {
                    if(kk - coins[l] >= 0)
                    {
                        int c = 1 + T[kk - coins[l]];
                        if (c < T[kk])
                        {
                            T[kk] = c;
                            P[kk] = l;
                        }
                    }

                }
            }

            int[] A = new int[coins.Length];
            kk = amount;
            change = A;

            if (T[amount] == (int.MaxValue - 1))
            {
                change = null;
                return null;
            }

            
            while(kk > 0)
            {
                A[P[kk]]++;
                kk -= coins[P[kk]];
            }

       
            return T[amount];
        }

        /// <summary>
        /// Metoda wyznacza rozwiązanie problemu wydawania reszty przy pomocy minimalnej liczby monet
        /// z uwzględnieniem ograniczeń na liczbę monet danego rodzaju
        /// </summary>
        /// <param name="amount">Kwota reszty do wydania</param>
        /// <param name="coins">Dostępne nominały monet</param>
        /// <param name="limits">Liczba dostępnych monet danego nomimału</param>
        /// <param name="change">Liczby monet danego nominału użytych przy wydawaniu reszty</param>
        /// <returns>Minimalna liczba monet potrzebnych do wydania reszty</returns>
        /// <remarks>
        /// coins[i]  - nominał monety i-tego rodzaju
        /// limits[i] - dostepna liczba monet i-tego rodzaju (nominału)
        /// change[i] - liczba monet i-tego rodzaju (nominału) użyta w rozwiązaniu
        /// Jeśli dostepnymi monetami nie da się wydać danej kwoty to change = null,
        /// a metoda również zwraca null
        ///
        /// Wskazówka/wymaganie:
        /// Dodatkowa uzyta pamięć powinna (musi) być proporcjonalna do wartości iloczynu amount*(liczba rodzajów monet)
        /// ( czyli rzędu o(amount*(liczba rodzajów monet)) )
        /// </remarks>
        public int? Dynamic(int amount, int[] coins, int[] limits, out int[] change)
        {
            int[,] optimalNumberOfCoins = new int[coins.Length, amount + 1];
            // [i,j] - minimalna liczba monet potrzebna do wydania kwoty j przy użyciu monet od 0 do i
            // wypełniamy wierszamy czyli glowna petla to kwota, wewnetrzna to monety
            
            for (int i = 0; i < coins.Length; i++)
            {
                for (int j = 0; j <= amount; j++)
                {
                    // kwote zero mozemy zawsze wydac
                    if (j == 0)
                    {
                        optimalNumberOfCoins[i, j] = 0;
                        // ustawiamy ilosc monet i-tego rodzaju na zero
                    }
                    else if (i == 0) // mamy tylko jedna monete
                    {
                        // jesli kwota jest wielokrotnoscia monety i nie przekracza limitu to wydajemy
                        if (j % coins[i] == 0 && j / coins[i] <= limits[i])
                        {
                            optimalNumberOfCoins[i, j] = j / coins[i];
                            // ustawiamy ilosc monet i-tego rodzaju na j / coins[i]
                        }
                        else // w przeciwnym wypadku nie da sie wydac
                        {
                            optimalNumberOfCoins[i, j] = int.MaxValue;
                        }
                    }
                    else // jesli kwota jest inna od zera i mamy wiecej niz jedna monete
                    {
                        int min = int.MaxValue; // poczatkowa minimalna liczba monet potrzebna do wydania kwoty j
                        // sprawdzamy ile monet i-tego rodzaju mozemy wydac i wybieramy najlepsza opcje
                        
                        // wczesniej nie uzywalismy monety i-tego rodzaju
                        for (int k = 0; k <= limits[i] && k * coins[i] <= j; k++)
                        {
                            if (optimalNumberOfCoins[i - 1, j - k * coins[i]] != int.MaxValue)
                            {
                                if(amount==10 && j ==10 && i == 2)
                                {
                                    int x = 0;
                                }
                                // wybieramy minimalna liczbe monet potrzebna do wydania kwoty j - k * coins[i] i dodajemy k monet i-tego rodzaju
                                min = Math.Min(min, k + optimalNumberOfCoins[i - 1, j - k * coins[i]]);
                            }
                        }
                        optimalNumberOfCoins[i, j] = min;
                    }
                }
            }
            
            // jesli nie da sie wydac danej kwoty to change = null
            if (optimalNumberOfCoins[coins.Length - 1, amount] == int.MaxValue)
            {
                change = null;
                return null;
            }
            else // w przeciwnym wypadku wybieramy ilosc monet i-tego rodzaju wydanych przy wydawaniu reszty
            {
                change = new int[coins.Length];
                int amountCopy = amount;
                for (int i = coins.Length - 1; i >= 0; i--)
                {
                    // wybieramy ile monet i-tego rodzaju wydajemy
                    if (i == 0)
                    {
                        change[i] = optimalNumberOfCoins[i, amountCopy];
                    }
                    else
                    {
                        for (int j = 0; j <= limits[i] && amountCopy > 0; j++)
                        {
                            if (optimalNumberOfCoins[i, amountCopy] == j + optimalNumberOfCoins[i - 1, amountCopy - j * coins[i]])
                            {
                                change[i] = j;
                                amountCopy -= j * coins[i];
                                break;
                            }
                        }
                    }
                   
                }
                
                return optimalNumberOfCoins[coins.Length - 1, amount];
            }
        }

    }

}
