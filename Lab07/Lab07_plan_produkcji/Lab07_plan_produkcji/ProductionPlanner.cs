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
            DiGraph<int> productionNetwork = new DiGraph<int>(3*production.Length + 3);
            int productionInWeekBegVertex = 0;
            int kontrahentInWeekBegVertex = production.Length;
            int magazynBegVertex = 2*production.Length;
            int finalMagazynVertex = 3*production.Length - 1;
            int magazynBottleNeckVertex = 3*production.Length;
            int wejscie = 3*production.Length + 1;
            int wyjscie = 3*production.Length + 2;
            
            
            // magazyn finalny -> bottleneck magazyn
            productionNetwork.AddEdge(finalMagazynVertex, magazynBottleNeckVertex, storageInfo.Quantity);
            
            for (int week = 0; week < production.Length; week++)
            {
                // wejscie -> produkcja 
                productionNetwork.AddEdge(wejscie, productionInWeekBegVertex + week, production[week].Quantity);
                
                // produkcja -> kontrahent
                productionNetwork.AddEdge(productionInWeekBegVertex + week, kontrahentInWeekBegVertex + week, production[week].Quantity);
               
                // MAGAZYNY
                for(int delay = week; delay < production.Length - 1; delay++)
                {
                    // produkcja -> magazyn opozniozny o i tydzien
                    productionNetwork.AddEdge(productionInWeekBegVertex + week, magazynBegVertex + delay, production[week].Quantity);
                    // magazyn opozniony o i tydzien -> magazyn STRAZNIK
                    productionNetwork.AddEdge(magazynBegVertex + delay, finalMagazynVertex, storageInfo.Quantity);
                }
                
                // bottleneck magazyn -> kontrahent
                productionNetwork.AddEdge( magazynBottleNeckVertex, kontrahentInWeekBegVertex+week, storageInfo.Quantity);

                // kontrahent -> wyjscie
                productionNetwork.AddEdge(kontrahentInWeekBegVertex + week, wyjscie, sales[week].Quantity);
                
                // magazyn -> wyjscie
                // productionNetwork.AddEdge(finalMagazynVertex, wyjscie, storageInfo.Quantity);
            }
            
            // maksymalna produkcja 
            var (maxProductionQuantity, maxProductionFlow)  = Flows.FordFulkerson(productionNetwork, wejscie, wyjscie);
            
            
            // Sieć, która maksymalizuje zyski
            NetworkWithCosts<int, double> profitNetwork = new NetworkWithCosts<int, double>(3*production.Length + 3);
            
            
            for (int week = 0; week < production.Length; week++)
            {
                // dodawaj tylko jesli maxProductionFlow korzysta z tej krawedzi
                
                // wejscie -> produkcja 
                if (maxProductionFlow.HasEdge(wejscie, productionInWeekBegVertex + week))
                {
                    double cost = production[week].Value;
                    int quantity = maxProductionFlow.GetEdgeWeight(wejscie, productionInWeekBegVertex + week);
                    profitNetwork.AddEdge(wejscie, productionInWeekBegVertex + week, quantity, cost);
                }
                
                // produkcja -> kontrahent
                if(maxProductionFlow.HasEdge(productionInWeekBegVertex + week, kontrahentInWeekBegVertex + week))
                {
                    double cost = 0;
                    int quantity = maxProductionFlow.GetEdgeWeight(productionInWeekBegVertex + week, kontrahentInWeekBegVertex + week);
                    profitNetwork.AddEdge(productionInWeekBegVertex + week, kontrahentInWeekBegVertex + week, quantity, cost);
                }
               
                // MAGAZYNY
                for(int delay = week; delay < production.Length - 1; delay++)
                {
                    // produkcja -> magazyn opozniozny o i tydzien
                    if (maxProductionFlow.HasEdge(productionInWeekBegVertex + week, magazynBegVertex + delay))
                    {
                        int weeksInStorage = delay - week + 1;
                        double cost = weeksInStorage * storageInfo.Value;
                        int quantity = maxProductionFlow.GetEdgeWeight(productionInWeekBegVertex + week, magazynBegVertex + delay);
                        profitNetwork.AddEdge(productionInWeekBegVertex + week, magazynBegVertex + delay, quantity,cost);
                    }
                   
                    // magazyn opozniony o i tydzien -> magazyn STRAZNIK
                    if (maxProductionFlow.HasEdge(magazynBegVertex + delay, finalMagazynVertex))
                    {
                        double cost = 0;
                        int quantity = maxProductionFlow.GetEdgeWeight(magazynBegVertex + delay, finalMagazynVertex);
                        profitNetwork.AddEdge(magazynBegVertex + delay, finalMagazynVertex, quantity, cost);
                    }
                }
                
                // magazyn finalny -> bottleneck magazyn
                if (maxProductionFlow.HasEdge(finalMagazynVertex, magazynBottleNeckVertex))
                {
                    double cost = 0;
                    int quantity = maxProductionFlow.GetEdgeWeight(finalMagazynVertex, magazynBottleNeckVertex);
                    profitNetwork.AddEdge(finalMagazynVertex, magazynBottleNeckVertex, quantity, cost);
                }

                // bottleneck magazyn -> kontrahent
                if (maxProductionFlow.HasEdge(magazynBottleNeckVertex, kontrahentInWeekBegVertex + week))
                {
                    double cost = 0;
                    int quantity = maxProductionFlow.GetEdgeWeight(magazynBottleNeckVertex, kontrahentInWeekBegVertex + week);
                    profitNetwork.AddEdge(magazynBottleNeckVertex, kontrahentInWeekBegVertex + week, quantity, cost);
                }

                // kontrahent -> wyjscie
                if (maxProductionFlow.HasEdge(kontrahentInWeekBegVertex + week, wyjscie))
                {
                    double cost = -sales[week].Value;
                    int quantity = maxProductionFlow.GetEdgeWeight(kontrahentInWeekBegVertex + week, wyjscie);
                    profitNetwork.AddEdge(kontrahentInWeekBegVertex + week, wyjscie, quantity, cost);
                }
            }
            
            // profitNetwork.AddEdge(wejscie, wyjscie, maxProductionQuantity, 0);

            // maksymalny zysk
            var ( flowValue , flowCost , minCostProfitFlow ) = Flows.MinCostMaxFlow(profitNetwork, wejscie , wyjscie ) ;
            
            weeklyPlan = new SimpleWeeklyPlan[production.Length];
            
                // zbuduj plan 
                //int prevWeekStore = 0;
                for (int week = 0; week < production.Length; week++)
                {
                    // produkcja
                    int productionQuantity = 0;
                    if (minCostProfitFlow.HasEdge(wejscie, productionInWeekBegVertex + week))
                    {
                        productionQuantity = minCostProfitFlow.GetEdgeWeight(wejscie, productionInWeekBegVertex + week);
                    }
                    
                    // kontrahent
                    int salesQuantity = 0;
                    if (minCostProfitFlow.HasEdge(kontrahentInWeekBegVertex + week, wyjscie))
                    {
                        
                        salesQuantity = minCostProfitFlow.GetEdgeWeight(kontrahentInWeekBegVertex + week, wyjscie);
                    }
                    
                    // czy cos wyszlo z magazynu w tym tyg
                    // if(minCostProfitFlow.HasEdge(magazynBottleNeckVertex, kontrahentInWeekBegVertex + week))
                    // {
                    //     prevWeekStore -= minCostProfitFlow.GetEdgeWeight(magazynBottleNeckVertex, kontrahentInWeekBegVertex + week);
                    // }

                    // magazyn - czy jest krawedz z magazynu opoznienia do glownego
                    int storageQuantity = 0;
                    // for(int delay = 0; delay < production.Length - 1; delay++)
                    // {
                    //     if (minCostProfitFlow.HasEdge(productionInWeekBegVertex + week, magazynBegVertex + delay))
                    //     {
                    //         storageQuantity += minCostProfitFlow.GetEdgeWeight(productionInWeekBegVertex + week, magazynBegVertex + delay);
                    //     }
                    // }
                    
                    // magazyn - czy jest krawedz z magazynu week do magazynu finalnego
                    if (minCostProfitFlow.HasEdge(magazynBegVertex + week, finalMagazynVertex))
                    {
                        storageQuantity += minCostProfitFlow.GetEdgeWeight(magazynBegVertex + week, finalMagazynVertex);
                    }


                    weeklyPlan[week] = new SimpleWeeklyPlan
                    {
                        UnitsProduced = productionQuantity,
                        UnitsSold = salesQuantity,
                        UnitsStored = storageQuantity,
                    };
                    // prevWeekStore = storageQuantity;
                }
                
            return new PlanData
            {
                Value = -flowCost,
                Quantity = flowValue,
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