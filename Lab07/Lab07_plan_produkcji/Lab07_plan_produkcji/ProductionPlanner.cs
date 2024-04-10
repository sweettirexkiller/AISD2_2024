using System;
using System.Linq;
using ASD.Graphs;

namespace ASD
{
    public class ProductionPlanner : MarshalByRefObject
    {
        /// <summary>
        /// Flaga pozwalająca na włączenie wypisywania szczegółów skonstruowanego planu na konsolę.
        /// Wartość <code>true</code> spoeoduje wypisanie planu.
        /// </summary>
        public bool ShowDebug { get; } = true;

        /// <summary>
        /// Część 1. zadania - zaplanowanie produkcji telewizorów dla pojedynczego kontrahenta.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających maksymalną produkcję i zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się maksymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateSimplePlan(PlanData[] production, PlanData[] sales, PlanData storageInfo,
            out SimpleWeeklyPlan[] weeklyPlan)
        {
            //production - quantitiy - limit produkcji w danym tygodniu, value - koszt produkcji jednej sztuki
            
            // sales - quantity - zapotrzebowanie kontrahenta w danym tygodniu, value - cena sprzedaży jednej sztuki
            
            // storageInfo - quantity - pojemność magazynu, value - koszt przechowania jednego telewizora w magazynie przez jeden tydzień

            // Zbudowac sieci maksymalizująca w pierwszej kolejności produkcję a w drugiej kolejności zyski
            
            // Sieć, która maksymalizuje tylko produkcję (nie bierze pod uwage kosztow)
            NetworkWithCosts<int, double> productionNetwork = new NetworkWithCosts<int, double>(production.Length + 4);
            // Wierzchołki:
            // 0 - źródło
            int start = 0;
            // 1 - produkcja w tygodniu 1
            // 2 - produkcja w tygodniu 2
            // ...
            // n - produkcja w tygodniu n
            // n+1 - magazyn
            int magazyn = production.Length + 1;
            // n+2 - kontrahent
            int kontrahent = production.Length + 2;
            // n+3 - ujście
            int ujscie = production.Length + 3;
            
            // Krawędzie:
            for (int week = 0; week < production.Length; week++)
            {
                // z źródła do produkcji w tygodniu i - krawędź o przepustowości production[i].quantity i koszcie ilosc*koszt
                productionNetwork.AddEdge(start, week + 1, production[week].Quantity, production[week].Value * production[week].Quantity);
               
                // z produkcji w tygodniu i do magazynu - krawędź o przepustowości production[i].quantity i koszcie 0
                productionNetwork.AddEdge(week + 1, magazyn, production[week].Quantity,0);
                
                int weeksInStorage = sales.Length - week;
                double costPerWeek = storageInfo.Value * production[week].Quantity;
                // z magazynu do kontrahenta - krawędź o przepustowości storageInfo.quantity i koszcie storageInfo.value
                productionNetwork.AddEdge(magazyn, kontrahent, storageInfo.Quantity, weeksInStorage * costPerWeek);
                
                // z kontrahenta do ujścia - krawędź o przepustowości sales[i].quantity i koszcie -sales[i].value
                productionNetwork.AddEdge(kontrahent, ujscie, sales[week].Quantity, -sales[week].Value*sales[week].Quantity);
                
                // z magazynu do ujścia - krawędź o przepustowości storageInfo.quantity i koszcie 0
                productionNetwork.AddEdge(magazyn, ujscie, storageInfo.Quantity, 0);
            }

            // W drugiej kolejności zbudować sieć, która maksymalizuje zyski
            NetworkWithCosts<int, double> profitNetwork = new NetworkWithCosts<int, double>(production.Length + 4);
            // wierzchołki takie same jak w poprzedniej sieci
            // Krawędzie:

            for (int week = 0; week < production.Length; week++)
            {
                // z źródła do produkcji w tygodniu i - krawędź o przepustowości production[i].quantity i koszcie production[i].value
                profitNetwork.AddEdge(start, week + 1, production[week].Quantity, 0);
                // z produkcji w tygodniu i do magazynu - krawędź o przepustowości production[i].quantity i koszcie 0
                profitNetwork.AddEdge(week + 1, magazyn, production[week].Quantity, 0);
                // z magazynu do kontrahenta - krawędź o przepustowości storageInfo.quantity i koszcie storageInfo.value
                profitNetwork.AddEdge(magazyn, kontrahent, storageInfo.Quantity, 0);
                // z kontrahenta do ujścia - krawędź o przepustowości sales[i].quantity i koszcie -sales[i].value
                profitNetwork.AddEdge(kontrahent, ujscie, sales[week].Quantity, sales[week].Value * sales[week].Quantity);
                // z magazynu do ujścia - krawędź o przepustowości storageInfo.quantity i koszcie 0
                profitNetwork.AddEdge(magazyn, ujscie, storageInfo.Quantity, 0);
            }
            // Wyznaczyć maksymalną produkcję
            // Wyznaczyć maksymalny zysk
            // Wyznaczyć plan produkcji
            // Wyznaczyć plan sprzedaży
            // Wyznaczyć plan magazynowania
            
            // Zwrócić obiekt PlanData z maksymalną produkcją i zyskiem
            // Zwrócić tablicę weeklyPlan z planem produkcji, sprzedaży i magazynowania
            
            



            // weeklyPlan - tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach
            // UnitsProduced - ile wyprodukowano
            // UnitsProduced, ile sprzedano
            // UnitsSold, ile przechowano


            // quantity - maksymalna liczba wyprodukowanych telewizorów w danym tygodniu
            // value - zysk fabryki w danym tygodniu – różnicę przychodów ze sprzedaży i kosztów produkcji i magazynowania
            weeklyPlan = null;
            return new PlanData
            {
                Value = 0,
                Quantity = 0
            };
        }

        /// <summary>
        /// Część 2. zadania - zaplanowanie produkcji telewizorów dla wielu kontrahentów.
        /// </summary>
        /// <remarks>
        /// Do przeprowadzenia testów wyznaczających produkcję dającą maksymalny zysk wymagane jest jedynie zwrócenie obiektu <see cref="PlanData"/>.
        /// Testy weryfikujące plan wymagają przypisania tablicy z planem do parametru wyjściowego <see cref="weeklyPlan"/>.
        /// </remarks>
        /// <param name="production">
        /// Tablica obiektów zawierających informacje o produkcji fabryki w kolejnych tygodniach.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza limit produkcji w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - koszt produkcji jednej sztuki.
        /// </param>
        /// <param name="sales">
        /// Dwuwymiarowa tablica obiektów zawierających informacje o sprzedaży w kolejnych tygodniach.
        /// Pierwszy wymiar tablicy jest równy liczbie kontrahentów, zaś drugi - liczbie tygodni w planie.
        /// Wartości pola <see cref="PlanData.Quantity"/> oznaczają maksymalną sprzedaż w danym tygodniu,
        /// a pola <see cref="PlanData.Value"/> - cenę sprzedaży jednej sztuki.
        /// Każdy wiersz tablicy odpowiada jednemu kontrachentowi.
        /// </param>
        /// <param name="storageInfo">
        /// Obiekt zawierający informacje o magazynie.
        /// Wartość pola <see cref="PlanData.Quantity"/> oznacza pojemność magazynu,
        /// a pola <see cref="PlanData.Value"/> - koszt przechowania jednego telewizora w magazynie przez jeden tydzień.
        /// </param>
        /// <param name="weeklyPlan">
        /// Parametr wyjściowy, przez który powinien zostać zwrócony szczegółowy plan sprzedaży.
        /// </param>
        /// <returns>
        /// Obiekt <see cref="PlanData"/> opisujący wyznaczony plan.
        /// W polu <see cref="PlanData.Quantity"/> powinna znaleźć się optymalna liczba wyprodukowanych telewizorów,
        /// a w polu <see cref="PlanData.Value"/> - wyznaczony maksymalny zysk fabryki.
        /// </returns>
        public PlanData CreateComplexPlan(PlanData[] production, PlanData[,] sales, PlanData storageInfo,
            out WeeklyPlan[] weeklyPlan)
        {
            weeklyPlan = null;
            return new PlanData
            {
                Value = 0,
                Quantity = 0,
            };
        }
    }
}