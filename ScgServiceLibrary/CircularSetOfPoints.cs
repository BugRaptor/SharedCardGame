using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ScgServiceLibrary
{
    public class CircularSetOfPoints
    {
        private Dictionary<string, Point> _points = new Dictionary<string, Point>();

        public Dictionary<string, Point> Points
        {
            get { return _points; }
        }
        public double CenterX { get; private set; }
        public double CenterY { get; private set; }

        public void Add(Point point, string pointId)
        {
            if (string.IsNullOrEmpty(pointId))
            {
                throw new ArgumentException("The point Id is null or empty.");
            }
            if (_points.ContainsKey(pointId))
            {
                throw new ArgumentException(string.Format("The point Id '{0}' is already used.", pointId));
            }
            if (!_points.Any())
            {
                CenterX = point.X;
                CenterY = point.Y;
            }
            else
            {
                CenterX = (CenterX * _points.Count() + point.X) / (_points.Count() + 1);
                CenterY = (CenterY * _points.Count() + point.Y) / (_points.Count() + 1);         
            }
            _points.Add(pointId, point);
        }

        public Point Remove(string pointId)
        {
            Point removedPoint;
            if (string.IsNullOrEmpty(pointId))
            {
                throw new ArgumentException("The point Id is null or empty.");
            }
            if (!_points.ContainsKey(pointId))
            {
                throw new ArgumentException(string.Format("The point Id '{0}' was not found in CircularSetOfPoints", pointId));
            }
            if (_points.TryGetValue(pointId, out removedPoint))
            {
                if (_points.Count() == 1)
                {
                    CenterX = 0;
                    CenterY = 0;
                }
                else
                {
                    CenterX = (CenterX * _points.Count() - removedPoint.X) / (_points.Count() - 1);
                    CenterY = (CenterY * _points.Count() - removedPoint.Y) / (_points.Count() - 1);
                }
                _points.Remove(pointId);
            }
            return removedPoint;
        }

        public Point? GetPoint(string pointId)
        {
            Point thePoint;
            if (string.IsNullOrEmpty(pointId))
            {
                throw new ArgumentException("The point Id is null or empty.");
            }
            if (_points.TryGetValue(pointId, out thePoint))
            {
                return thePoint;
            }
            return null;
        }

        public void Move(Point point, string pointId)
        {
            Remove(pointId);
            Add(point, pointId);
        }

        public void Clear()
        {
            _points.Clear();
            CenterX = 0;
            CenterY = 0;
        }

        /// <summary>
        /// Computes the the positive angle (measured at center) between the point whose id is originPointId
        /// to the point whose id is pointId and if we turn in the trigonometrical sens (counterclockwise).
        /// This angle is comprised between 0 and 2 * Pi.
        /// </summary>
        /// <param name="pointId">The destination point identifier.</param>
        /// <param name="originPointId">The origin point identifier.</param>
        /// <returns>System.Double.</returns>
        /// <exception cref="System.ArgumentException">
        /// </exception>
        public double RelativeAngleFromCenterBetween(string pointId, string originPointId)
        {
            Point? nA = GetPoint(originPointId);
            Point? nB = GetPoint(pointId);
            if (nA == null)
            {
                throw new ArgumentException(string.Format("The point Id '{0}' was not found in CircularSetOfPoints", pointId));
            } 
            if (nB == null)
            {
                throw new ArgumentException(string.Format("The point Id '{0}' was not found in CircularSetOfPoints", originPointId));
            }
            double aX = ((Point)nA).X;
            double aY = ((Point)nA).Y;
            double bX = ((Point)nB).X;
            double bY = ((Point)nB).Y;
            // Change Origin to Center:
            aX -= CenterX;
            aY -= CenterY;
            bX -= CenterX;
            bY -= CenterY;

            double moduleA = Math.Sqrt(aX * aX + aY * aY);
            double moduleB = Math.Sqrt(bX * bX + bY * bY);

            double scalarProduct = aX * bX + aY * bY;
            double relativeAngle;
            // Compute the aangle between a and b using the saclar product:
            // Note: this angle is a positive angle going from a to b by the shortest way. (between 0 and 2*pi)
            relativeAngle = Math.Acos(scalarProduct / (moduleA * moduleB));
            // Let's now compute the vectorial product in order to know the rotating direction (clockwise or counterclockwise) 
            // when going from a to b by the shortest way:
            double moduleOfVectorialProduct = aX * bY - aY * bX;
            if (moduleOfVectorialProduct > 0)
            {
                // relativeAngle is the angle going from a to b by the shortest way. In that case, it rotates clockwise.
                // We need the complementary angle going from a to b in the trigonometrical direction (counterclockwise):
                relativeAngle = 2 * Math.PI - relativeAngle;
            }
            // if the vectorial product is negative, the angle going from a to b by the shortest way rotates counterclockwise.
            // It's what we want, nothing to change in that cxase.

            return relativeAngle;
        }

        /// <summary>
        /// Sorts all the Points from the point whose id is fromPointId and ordering them clockwise or counterclockwise relatively 
        /// to the Center position.
        /// </summary>
        /// <param name="fromPointId">The starting point id.</param>
        /// <param name="clockwise">if set to <c>true</c> order the points in clockwise direction.</param>
        public void SortCircular(string fromPointId, bool clockwise)
        {
            List<string> pointIds = _points.Keys.ToList();
            var orderedPointIds = pointIds.OrderBy(id => RelativeAngleFromCenterBetween(id, fromPointId)).ToList();
            Dictionary<string, Point> points = new Dictionary<string, Point>();
            if (!clockwise)
            {
                if (orderedPointIds.Count > 0)
                {
                    int i = 0;
                    bool terminated = false;
                    while (!terminated)
                    {
                        string id = orderedPointIds[i];
                        Point pt;
                        if (_points.TryGetValue(id, out pt))
                        {
                            points.Add(id, pt);
                        }
                        i--;
                        if (i < 0)
                        {
                            i = orderedPointIds.Count - 1;
                        }
                        terminated = i == 0;
                    }
                }
            }
            else
            {
                for (int i = 0; i < orderedPointIds.Count; i++)
                {
                    string id = orderedPointIds[i];
                    Point pt;
                    if (_points.TryGetValue(id, out pt))
                    {
                        points.Add(id, pt);
                    }
                }
            }
            _points = points;
        }
    }
}
