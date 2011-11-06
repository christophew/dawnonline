using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using FarseerPhysics.Dynamics;
using FarseerPhysics.Factories;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;

namespace SimulationTests
{
    [TestClass]
    public class FarseerTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var farSeerWorld = new World(Vector2.Zero);

            var body = new Body(farSeerWorld);
            var fixture = FixtureFactory.AttachCircle(10, 1, body);

            //fixture.
        }
    }
}
