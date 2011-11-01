using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows;
using ShapeGame2;
using System.Windows.Shapes;
namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestNormalizeAngle()
        {
            Assert.AreEqual(ShapeGame2.MainWindow.NormalizeAngle(0), 0);
            Assert.AreEqual(ShapeGame2.MainWindow.NormalizeAngle(370), 10);
            Assert.AreEqual(ShapeGame2.MainWindow.NormalizeAngle(-370), -10);
            Assert.AreEqual(ShapeGame2.MainWindow.NormalizeAngle(350), -10);
            Assert.AreEqual(ShapeGame2.MainWindow.NormalizeAngle(-180), -180);
            Assert.AreEqual(ShapeGame2.MainWindow.NormalizeAngle(180), -180);
            Assert.AreEqual(ShapeGame2.MainWindow.NormalizeAngle(190), -170);
        }

        [TestMethod]
        public void TestLineToVector()
        {
            Assert.AreEqual(new Line().Init(5.0, 5.0, 10.0, 10.0).ToVector(), new Vector(5, 5));
            Assert.AreNotEqual(new Line().Init(10.0, 10.0, 5.0, 5.0).ToVector(), new Vector(5, 5));
        }
        [TestMethod]
        public void TestAngleBetweenLines()
        {
            
            Line l1 = new Line().Init(0.0, 0.0, 10.0, 0.0);
            Line l2 = new Line().Init(0, 0, 0, 10);
            Assert.AreEqual(l1.AngleBetween(l2), 90.0);
            Assert.AreNotEqual(l2.AngleBetween(l1), 90.0);
            Assert.AreEqual(new Line().Init(0.0, 0.0, 10.0, 0.0).AngleBetween(new Line().Init(0.0, 0.0, 10.0, 0.0)), 0.0);
            Assert.AreEqual(new Line().Init(0.0, 0.0, 10.0, 0.0).AngleBetween(new Line().Init(0.0, 0.0, -10.0, 0.0)), 180.0);
        }
        [TestMethod]
        public void TestThreeLine()
        {
            Vector v = new Vector(3, 0).Tangent(new Vector(10, 0), new Vector(0, 10));
            Dictionary<int, double> d = new Dictionary<int, double>();
            for (int i = 5; i < 100; i++)
            {
                v = new Vector(5, 0).Tangent(new Vector(10, 0), new Vector(0, i));
                double a = v.Angle();
                d.Add(i, a);
            }
            int j = 0;
            
        }
        [TestMethod]
        public void testVectorAngle()
        {

            Assert.AreEqual(new Vector(1, 0).Angle(), 0);
            Assert.AreEqual(new Vector(1, 1).Angle(), 45);
            Assert.AreEqual(new Vector(0, 1).Angle(), 90);
            Assert.AreEqual(new Vector(-1, 1).Angle(), 135);
            Assert.AreEqual(new Vector(-1, 0).Angle(), 180);
            
        }
    }
}
