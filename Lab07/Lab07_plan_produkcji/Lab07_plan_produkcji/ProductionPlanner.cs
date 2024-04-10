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
            DiGraph<int> productionNetwork = new DiGraph<int>(2*production.Length + 4);
            // Wierzchołki:
            // 0 - źródło
            int start = 0;
            // 1 - produkcja w tygodniu 1
            // 2 - produkcja w tygodniu 2
            // ...
            // n - produkcja w tygodniu n
            // n+1 - magazyn
            int magazyn = production.Length + 1;
            // n+2 - kontriahent w tygodni 1
            int kontrahentsStart = production.Length + 2;
            // n +3 - kontrahent w tygodni 2
            // n +4 - kontrahent w tygodni 3
            // ...
            // 2n + 2 - kontrahent w tygodni n
            // 
            // n+n+3 - ujście
            int ujscie = production.Length*2 + 3;
            
            // Krawędzie:
            for (int week = 0; week < production.Length; week++)
            {
                // z źródła do produkcji w tygodniu i - krawędź o przepustowości production[i].quantity
                productionNetwork.AddEdge(start, week + 1, production[week].Quantity);
               
                // z produkcji w tygodniu i do magazynu - krawędź o przepustowości production[i].quantity
                productionNetwork.AddEdge(week + 1, magazyn, production[week].Quantity);
                
                // z produkcji prosto do klienta
                // jesli wiecej wyprodukowano niz sprzedano to mozna sprzedac wszystko
                if(production[week].Quantity >= sales[week].Quantity)
                {
                    // z produkcji w tygodniu i do kontrahenta w tym tygodniu - krawędź o przepustowości sales[i].quantity
                    productionNetwork.AddEdge(week + 1, kontrahentsStart +  week, sales[week].Quantity);
                }
                else // jesli wyprodukowano mniej niz sprzedano to mozna sprzedac tylko tyle ile wyprodukowano
                {
                    // z produkcji w tygodniu i do kontrahenta - krawędź o przepustowości production[i].quantity
                    productionNetwork.AddEdge(week + 1,  kontrahentsStart +  week, production[week].Quantity);
                }
                
                // z magazynu do kontrahenta - (mozna wyciagnac wszystko z magazynu zawsze)
                productionNetwork.AddEdge(magazyn,  kontrahentsStart +  week, sales[week].Quantity);
                
                // z kontrahenta do ujścia - mozna wyciaganc tyle ile chce kupic
                productionNetwork.AddEdge( kontrahentsStart +  week, ujscie, sales[week].Quantity);
                
                // z magazynu do ujścia - mozna pozostawic w magazynie
                productionNetwork.AddEdge(magazyn, ujscie, storageInfo.Quantity);
            }
            
            // znajdz najwiekszy przeplyw produkcji 
            (int maxProduction, DiGraph<int> maxProductionFlow) = Flows.FordFulkerson(productionNetwork, start, ujscie);

            // W drugiej kolejności zbudować sieć, która maksymalizuje zyski ( dla ustalonej maksymalnej produkcji )
            NetworkWithCosts<int, double> profitNetwork = new NetworkWithCosts<int, double>(production.Length*2 + 4);
            // wierzchołki takie same jak w poprzedniej sieci
            // Krawędzie:

            for (int week = 0; week < production.Length; week++)
            {
                if (maxProductionFlow.HasEdge(start, week + 1))
                {
                    // z źródła do produkcji w tygodniu i - krawędź o przepustowości production[i].quantity i koszcie ilosc*koszt
                    profitNetwork.AddEdge(start, week + 1, production[week].Quantity, production[week].Value * production[week].Quantity);
                }
                
                if(maxProductionFlow.HasEdge(week + 1, magazyn))
                {
                    // z produkcji w tygodniu i do magazynu - krawędź o przepustowości production[i].quantity i koszcie 0
                    profitNetwork.AddEdge(week + 1, magazyn, production[week].Quantity,0);
                }
               
                if(maxProductionFlow.HasEdge(magazyn, kontrahentsStart + week))
                {
                    int weeksInStorage = sales.Length - week;
                    double costPerWeek = storageInfo.Value * production[week].Quantity;
                    // z magazynu do kontrahenta - przeniesienie z magazynu do kontrahenta kosztuje w zaleznosci od ilosci tygodni w magazynie
                    profitNetwork.AddEdge(magazyn, kontrahent, storageInfo.Quantity, weeksInStorage * costPerWeek);
                }
            
                if (maxProductionFlow.HasEdge(kontrahent, ujscie))
                {
                    // z kontrahenta do ujścia - krawędź o przepustowości sales[i].quantity i koszcie -sales[i].value
                    profitNetwork.AddEdge(kontrahent, ujscie, sales[week].Quantity, -sales[week].Value*sales[week].Quantity);
                }
                
                if(maxProductionFlow.HasEdge(magazyn, ujscie))
                {
                    // z magazynu do ujścia - krawędź o przepustowości storageInfo.quantity i koszcie 0
                    profitNetwork.AddEdge(magazyn, ujscie, storageInfo.Quantity, 0);
                }
            }
            
            var ( flowValue , flowCost , f ) = Flows.MinCostMaxFlow (profitNetwork, start , ujscie);


                
                
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
            weeklyPlan = new SimpleWeeklyPlan[production.Length];
            return new PlanData
            {
                Value = 0,
                Quantity = maxProduction
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