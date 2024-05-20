using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD
{
    public class WaterCalculator : MarshalByRefObject
    {

        /*
         * Metoda sprawdza, czy przechodząc p1->p2->p3 skręcamy w lewo 
         * (jeżeli idziemy prosto, zwracany jest fałsz).
         */
        private bool leftTurn(Point p1, Point p2, Point p3)
        {
            Point w1 = new Point(p2.x - p1.x, p2.y - p1.y);
            Point w2 = new Point(p3.x - p2.x, p3.y - p2.y);
            double vectProduct = w1.x * w2.y - w2.x * w1.y;
            return vectProduct > 0;
        }


        /*
         * Metoda wyznacza punkt na odcinku p1-p2 o zadanej współrzędnej y.
         * Jeżeli taki punkt nie istnieje (bo cały odcinek jest wyżej lub niżej), zgłaszany jest wyjątek ArgumentException.
         */
        private Point getPointAtY(Point p1, Point p2, double y)
        {
            if (p1.y != p2.y)
            {
                double newX = p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y);
                if ((newX - p1.x) * (newX - p2.x) > 0)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point(p1.x + (p2.x - p1.x) * (y - p1.y) / (p2.y - p1.y), y);
            }
            else
            {
                if (p1.y != y)
                    throw new ArgumentException("Odcinek p1-p2 nie zawiera punktu o zadanej współrzędnej y!");
                return new Point((p1.x + p2.x) / 2, y);
            }
        }


        /// <summary>
        /// Funkcja zwraca tablice t taką, że t[i] jest głębokością, na jakiej znajduje się punkt points[i].
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double[] PointDepths(Point[] points)
        {
            if(points.Length < 3)
               return new double[points.Length];
            // Console.WriteLine();
            // Console.Write("path=[{0}], ", string.Join(",", points.Select(p => $"({p.x},{p.y})").ToArray()));
            // Console.WriteLine();
            double[] depths = new double[points.Length];
            double[] heightsFromLeft = new double[points.Length];
            double[] heightsFromRight = new double[points.Length];
            // znajdź wszystkie zagłębienia na terenie, czyli miejsca, gdzie teren jest niższy niż otaczające go obszary

            // przeglądaj kolejne trójkąty utworzone przez sąsiednie wierzchołki na liście
            Point highestToLeft = points[0];
            Point highestToRight = points[points.Length - 1];


            for (int i = 1; i < points.Length - 1; i++)
            {

                heightsFromLeft[i] = highestToLeft.y;
                Point pointFromLeft = points[i];

                if (pointFromLeft.y > highestToLeft.y)
                {
                    highestToLeft = pointFromLeft;
                }

               

            }
            
            for(int j = points.Length - 2; j > 0; j--)
            {
                heightsFromRight[j] = highestToRight.y;
                
                Point pointFromRight = points[j];
                if (pointFromRight.y > highestToRight.y)
                {
                    highestToRight = pointFromRight;
                }
            }
            
            for (int i = 1; i < points.Length - 1; i++)
            {
                if(points[i].y < heightsFromLeft[i] && points[i].y < heightsFromRight[i])
                {
                    depths[i] = Math.Min(heightsFromLeft[i], heightsFromRight[i]) - points[i].y;
                }
                else
                {
                    depths[i] = 0;
                }
            }

            
            return depths;
    }

        /// <summary>
        /// Funkcja zwraca objętość wody, jaka zatrzyma się w górach.
        /// 
        /// Przyjmujemy, że pierwszy punkt z tablicy points jest lewym krańcem, a ostatni - prawym krańcem łańcucha górskiego.
        /// </summary>
        public double WaterVolume(Point[] points)
        {
            // wielokaty od glebokosci 0 do 0 
            
            // najlepiej obliczyc metoda trapezow ?
            
            double[] depths = PointDepths(points);
            double volume = 0;
            for (int i = 0; i < points.Length - 1; i++)
            {
                // jesli trojkat
                if (depths[i] == 0 && depths[i +1] != 0)
                {
                    // najpierw trzeba znalesc punkt przeciecia
                    Point p1 = points[i];
                    Point p2 = points[i + 1];
                    // jaka jest wspolrzedna y wysokosic tafli wody ?
                    
                    double y = points[i+1].y + depths[i+1];
                    Point p3 = getPointAtY(p1, p2, y);
                    // pole trojkata p3 p1 0 
                    volume += depths[i + 1] * (points[i + 1].x - p3.x) / 2;
                } else if(depths[i] != 0 && depths[i + 1] == 0)
                {
                    Point p1 = points[i];
                    Point p2 = points[i + 1];
                    double y = points[i].y + depths[i];
                    Point p3 = getPointAtY(p1, p2, y);
                    
                    volume += depths[i] * (p3.x - points[i].x) / 2;
                } else if(depths[i] != 0 && depths[i + 1] != 0) // jesli trapez
                {
                    
                    // przekatna 1 
                    double[] d1 = new double[2]{ points[i].x - points[i+1].x, 0 - depths[i+1]};
                    // przekartna 2
                    double[] d2 = new double[2]{ points[i+1].x - points[i].x, 0 - depths[i]};
                    // kat miedzy przekatnymi
                    double angle = Math.Acos((d1[0] * d2[0] + d1[1] * d2[1]) / (Math.Sqrt(d1[0] * d1[0] + d1[1] * d1[1]) * Math.Sqrt(d2[0] * d2[0] + d2[1] * d2[1])));
                    // pole 
                    double l1 = Math.Sqrt(d1[0] * d1[0] + d1[1] * d1[1]);
                    double l2 = Math.Sqrt(d2[0] * d2[0] + d2[1] * d2[1]);
                    volume += 0.5 * l1 * l2 * Math.Sin(angle);
                }
                
            }

            return volume;
        }
    }

    [Serializable]
    public struct Point
    {
        public double x, y;
        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
