using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class Lab11 : System.MarshalByRefObject
    {

        // iloczyn wektorowy
        private int Cross((double, double) o, (double, double) a, (double, double) b)
        {
            double value = (a.Item1 - o.Item1) * (b.Item2 - o.Item2) - (a.Item2 - o.Item2) * (b.Item1 - o.Item1);
            return Math.Abs(value) < 1e-10 ? 0 : value < 0 ? -1 : 1;
        }

        // Etap 1
        // po prostu otoczka wypukła
        public (double, double)[] ConvexHull((double, double)[] points)
        {
            Stack<(double, double)>  S = Jarvis(points.Distinct().ToArray());

            (double, double)[] hull = S.Reverse().ToArray();
            
            // console.log(hull);
            Console.WriteLine();
            Console.Write("path=[{0}], ", string.Join(",", hull));
            Console.WriteLine();
            //
            
            return hull;
        }

        private Stack<(double x, double y)> Jarvis((double x, double y)[] points)
        {
            // wykonujemy permuatcje punktow
            (double x, double y) min = points[0];
            foreach ((double x, double y) point in points)
            {
                if (point.y < min.y || (point.y == min.y && point.x < min.x))
                {
                    min = point;
                }
            }
            
            // przestawiamy ten punkt na poczatek tablicy
            int minIndex = Array.IndexOf(points, min);
            (double x, double y) temp = points[0];
            points[0] = points[minIndex];
            points[minIndex] = temp;
            

            // pusty stos
            Stack<(double x, double y)> S = new Stack<(double x, double y)>();
            S.Push(points[0]);
            ((double x, double y) start, (double x,double y) end) line = ((0,0), (1,0));
            bool[] used = new bool[points.Length];
            used[0] = true;

            while (true)
            {
                // znajdz k takie ze, points[k] minimalizuje kat miedzy prosta l i odcinkiem S.Top() do points[k]
                
                (double x, double y) sTop = S.Peek();
                int k = 0;
                double[] vectorL = new double[] { line.end.x - line.start.x, line.end.y - line.start.y };
                double minAngle = 2 * Math.PI;
                for(int i = 0; i < points.Length; i++)
                { 
                    if(used[i]) continue;
                    if(points[i] == sTop) continue; 
                    
                    double[] vectorsTopToK = new double[] { points[i].x - sTop.x, points[i].y - sTop.y };
                  
                    double angle = FindAngleBetweenVectors(vectorL, vectorsTopToK);
                   if (angle < minAngle)
                   {
                       k = i;
                       minAngle = angle;
                   } else if( minAngle == angle)
                   {
                       // jesli kat jest taki sam to wybieramy punkt ktory ma ktorakolwiek wspolrzedna wieksza
                       if (points[i].x > points[k].x && points[i].y > points[k].y)
                       {
                           k = i;
                       }
                   }
                }
                
                
                
                used[k] = true;
              
                if (k == 0)
                {
                    break;
                }
                // line to prosta zawierajaca S.Top() i points[k]
                line = (S.Peek(), points[k]);
                
                // znajdz wszystkie punkty ktore leza na prostej line
                for (int i = 0; i < points.Length; i++)
                {
                    if (used[i]) continue;
                    if(IsPointOnLine(line.start, line.end, points[i].x, points[i].y))
                    {
                        used[i] = true;
                    }
                    
                }
           
                S.Push(points[k]);
                
            }


            return S;
        }

        // Etap 2
        // oblicza otoczkę dwóch wielokątów wypukłych
        public (double, double)[] ConvexHullOfTwo((double, double)[] poly1, (double, double)[] poly2)
        {
            return null;
        }
        
        
        static double FindAngleBetweenVectors(double[] vector1, double[] vector2)
        {
            double dotProduct = DotProduct(vector1, vector2);
            double magnitude1 = Magnitude(vector1);
            double magnitude2 = Magnitude(vector2);

            // Oblicz kąt w radianach
            double cosTheta = dotProduct / (magnitude1 * magnitude2);
            double angleRad = Math.Acos(cosTheta);
            

            return angleRad;
        }   
        
        
        static double DotProduct(double[] vector1, double[] vector2)
        {
            return vector1[0] * vector2[0] + vector1[1] * vector2[1];
        }

        static double Magnitude(double[] vector)
        {
            return Math.Sqrt(vector[0] * vector[0] + vector[1] * vector[1]);
        }
        
        static bool IsPointOnLine((double x,double y) lineStart, (double x,double y) lineEnd, double x, double y)
        {
            // Obliczenie współczynnika kierunkowego prostej przechodzącej przez dwa punkty
            double slope = (lineEnd.y - lineStart.y) / (lineEnd.x - lineStart.x);

            // Obliczenie przesunięcia (intercept) prostej
            double intercept = lineStart.y - slope * lineStart.x;

            // Sprawdzenie, czy punkt spełnia równanie prostej (y = slope * x + intercept)
            return Math.Abs(y - (slope * x + intercept)) < 0.0001;
        }

    }
}
