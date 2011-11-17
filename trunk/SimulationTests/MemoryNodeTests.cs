using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation.Brains.Neural;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulationTests
{
    [TestClass]
    public class MemoryNodeTests
    {
        [TestMethod]
        public void AddSameValue()
        {
            var node = new MemoryInputNode(new TimeSpan(0, 0, 0, 1, 0));

            node.CurrentValue = 10;
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 100));
            Assert.AreEqual(10, node.CurrentValue);

            node.CurrentValue = 10;
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 10));
            Assert.AreEqual(10, node.CurrentValue);

            node.CurrentValue = 10;
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 1));
            Assert.AreEqual(10, node.CurrentValue);
        }

        [TestMethod]
        public void AddDifferentValue()
        {
            var node = new MemoryInputNode(new TimeSpan(0, 0, 0, 1, 0));

            node.CurrentValue = 10;
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 100));
            Assert.AreEqual(10, node.CurrentValue);

            node.CurrentValue = 10;
            node.AddRememberedValue(1, new TimeSpan(0, 0, 0, 0, 100));
            Assert.AreEqual(9.1, node.CurrentValue);

            node.CurrentValue = 10;
            node.AddRememberedValue(0.1, new TimeSpan(0, 0, 0, 0, 100));
            Assert.AreEqual(9.01, node.CurrentValue);
        }

        [TestMethod]
        public void StartFromZero()
        {
            var node = new MemoryInputNode(new TimeSpan(0, 0, 0, 1, 0));

            node.CurrentValue = 0;
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 10));
            Assert.AreEqual(0.1, node.CurrentValue);
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 10));
            //Assert.AreEqual(0.2, node.CurrentValue);
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 10));
            //Assert.AreEqual(0.3, node.CurrentValue);
            node.AddRememberedValue(10, new TimeSpan(0, 0, 0, 0, 10));
            //Assert.AreEqual(0.4, node.CurrentValue);
        }
    }
}
