using System;
using System.Collections.Generic;
using System.Drawing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScgServiceLibrary;

namespace UnitTestProject1

{
    [TestClass]
    public class CircularSetOfPointsUnitTest
    {
        [TestMethod]
        public void MainTest()
        {
            CircularSetOfPoints testSet = new CircularSetOfPoints();
            testSet.Add(new Point(100,100),"P1");
            testSet.Add(new Point(100, -100), "P2");
            testSet.Add(new Point(-100, 100), "P3");
            testSet.Add(new Point(-100, -100), "P4");
            Point? p1 = testSet.GetPoint("P1");
            Assert.IsNotNull(p1);
            Point? p2 = testSet.GetPoint("P2");
            Assert.IsNotNull(p2);
            Point? p3 = testSet.GetPoint("P3");
            Assert.IsNotNull(p3);
            Point? p4 = testSet.GetPoint("P4");
            Assert.IsNotNull(p4);
            Point? p5 = testSet.GetPoint("P5");
            Assert.IsNull(p5);
            double centerX = testSet.CenterX; 
            double centerY = testSet.CenterY;
            Assert.IsTrue(Math.Abs(centerX) < 0.000001);
            Assert.IsTrue(Math.Abs(centerY) < 0.000001);
            double angle = testSet.RelativeAngleFromCenterBetween("P1", "P4");
            Assert.IsTrue(Math.Abs(angle - 2 * Math.PI / 2) < 0.000001);
            angle = testSet.RelativeAngleFromCenterBetween("P2", "P4");
            Assert.IsTrue(Math.Abs(angle - 3 * Math.PI / 2) < 0.000001);
            angle = testSet.RelativeAngleFromCenterBetween("P3", "P4");
            Assert.IsTrue(Math.Abs(angle - 1 * Math.PI / 2) < 0.000001);
            angle = testSet.RelativeAngleFromCenterBetween("P4", "P4");
            Assert.IsTrue(Math.Abs(angle - 0 * Math.PI / 2) < 0.000001);

            testSet.SortCircular("P4", true);
            List<string> points = new List<string>();
            foreach (string s in testSet.Points.Keys)
            {
                points.Add(s);
            }
            Assert.IsTrue(points[0] == "P4");
            Assert.IsTrue(points[1] == "P3");
            Assert.IsTrue(points[2] == "P1");
            Assert.IsTrue(points[3] == "P2");

            points.Clear(); 
            testSet.SortCircular("P3", false);
            foreach (string s in testSet.Points.Keys)
            {
                points.Add(s);
            }
            Assert.IsTrue(points[0] == "P3");
            Assert.IsTrue(points[1] == "P4");
            Assert.IsTrue(points[2] == "P2");
            Assert.IsTrue(points[3] == "P1");

            testSet.Clear();            
            Assert.IsTrue(testSet.Points.Count == 0);

            Dictionary<string, Point> pointsDictionary = new Dictionary<string, Point>();
            Random random = new Random();
            for (int i = 0; i < 360; i++)
            {
                double alpha = i*2*Math.PI/360;

                double dist = 150 + random.Next(100);
                Point point = new Point(200 + (int)(dist * Math.Cos(alpha)), 200 + (int)(dist * Math.Sin(alpha)));
                pointsDictionary.Add(i.ToString(),point);
            }

            List<string> thePointNames = new List<string>();
            List<Point> thePoints = new List<Point>();
            foreach (string key in pointsDictionary.Keys)
            {
                Point thePoint;
                if (pointsDictionary.TryGetValue(key, out thePoint))
                {
                    thePointNames.Add(key);
                    thePoints.Add(thePoint);
                }
            }
            while (thePoints.Count > 0)
            {
                int index = random.Next(thePoints.Count);
                testSet.Add(thePoints[index], thePointNames[index]);
                thePoints.RemoveAt(index);
                thePointNames.RemoveAt(index);
            }

            testSet.SortCircular("359", true);

            points.Clear(); 
            foreach (string s in testSet.Points.Keys)
            {
                points.Add(s);
            }

            var j = 0;
            for (int i = 359; i >= 0; i--)
            {
                Assert.IsTrue(points[j] == i.ToString());
                j++;
            }

        }
    }
}
