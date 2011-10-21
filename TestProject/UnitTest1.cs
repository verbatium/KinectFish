using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
    }
}
