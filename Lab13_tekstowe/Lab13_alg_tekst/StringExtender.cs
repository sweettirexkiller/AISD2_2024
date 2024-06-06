using System;
using System.Text;

namespace Lab15
{
    public static class stringExtender
    {


        static public int[] ComputeP(string s)
        {
            if(s.Length == 1)
            {
                return new int[1]{0};
            }
            int[] P = new int[s.Length+1];
            P[0] = 0;
            P[1] = 0;
            int t = 0;
            for(int j = 2; j <= s.Length; j++)
            {
                while(t > 0 && s[t] != s[j - 1])
                {
                    t = P[t];
                }
                if(s[t] == s[j - 1])
                {
                    t++;
                }
                P[j] = t;
            }
            
            return P;
        }
        /// <summary>
        /// Metoda zwraca okres słowa s, tzn. najmniejszą dodatnią liczbę p taką, że s[i]=s[i+p] dla każdego i od 0 do |s|-p-1.
        /// 
        /// Metoda musi działać w czasie O(|s|)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        static public int Period(this string s)
        {
            if (s.Length == 1) return 1;
            //wystarczy znaleźć najdłuższe słowo będące jednocześnie właściwym prefiksem i sufiksem s
            int[] P = ComputeP(s);
            return s.Length - P[s.Length];
        }

        /// <summary>
        /// Metoda wyznacza największą potęgę zawartą w słowie s.
        /// 
        /// Jeżeli x jest słowem, wówczas przez k-tą potęgę słowa x rozumiemy k-krotne powtórzenie słowa x
        /// (na przykład xyzxyzxyz to trzecia potęga słowa xyz).
        /// 
        /// Należy zwrócić największe k takie, że k-ta potęga jakiegoś słowa jest zawarta w s jako spójny podciąg.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="startIndex">Pierwszy indeks fragmentu zawierającego znalezioną potęgę</param>
        /// <param name="endIndex">Pierwszy indeks po fragmencie zawierającym znalezioną potęgę</param>
        /// <returns></returns>
        static public int MaxPower(this string s, out int startIndex, out int endIndex)
        {
                startIndex = endIndex = -1;
                return -1;
        }
    }
}
