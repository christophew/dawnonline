using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.Simulation.Brains.Neural;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SimulationTests
{
    [TestClass]
    public class NeuralNetworkTests
    {
        [TestMethod]
        public void Construction()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            Assert.AreEqual(4, neuralNetwork.InputNodes.Length);
            Assert.AreEqual(3, neuralNetwork.LayerNodes.Length);
            Assert.AreEqual(2, neuralNetwork.OutputNodes.Length);
        }

        private static void InitEdgesTo1(NeuralNetwork neuralNetwork)
        {
            foreach (var inputNode in neuralNetwork.InputNodes)
            {
                InitEdgesTo1(inputNode);
            }
            foreach (var layerNode in neuralNetwork.LayerNodes)
            {
                InitEdgesTo1(layerNode);
            }
        }

        private static void InitEdgesTo1(Node inputNode)
        {
            foreach (var outGoingEdge in inputNode.OutGoingEdges)
            {
                outGoingEdge.Multiplier = 1;
            }
        }

        [TestMethod]
        public void Propagate()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].CurrentValue = 10;
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 1;
            neuralNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = 1;
            neuralNetwork.Propagate(new TimeSpan());

            Assert.AreEqual(10, neuralNetwork.LayerNodes[0].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.LayerNodes[1].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.LayerNodes[2].CurrentValue);

            Assert.AreEqual(10, neuralNetwork.OutputNodes[0].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.OutputNodes[1].CurrentValue);
        }

        [TestMethod]
        public void Threshold()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].CurrentValue = 10;
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 1;
            neuralNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = 1;
            neuralNetwork.InputNodes[0].Threshold = 11;
            neuralNetwork.Propagate(new TimeSpan());

            Assert.AreEqual(0, neuralNetwork.LayerNodes[0].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.LayerNodes[1].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.LayerNodes[2].CurrentValue);

            Assert.AreEqual(0, neuralNetwork.OutputNodes[0].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.OutputNodes[1].CurrentValue);

            neuralNetwork.Reset();
            neuralNetwork.InputNodes[0].CurrentValue = 10;
            neuralNetwork.InputNodes[0].Threshold = 10;
            neuralNetwork.Propagate(new TimeSpan());

            Assert.AreEqual(10, neuralNetwork.LayerNodes[0].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.LayerNodes[1].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.LayerNodes[2].CurrentValue);

            Assert.AreEqual(10, neuralNetwork.OutputNodes[0].CurrentValue);
            Assert.AreEqual(0, neuralNetwork.OutputNodes[1].CurrentValue);
        }

        [TestMethod]
        public void PropagateAll()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            InitEdgesTo1(neuralNetwork);
            neuralNetwork.InputNodes[0].CurrentValue = 10;
            neuralNetwork.Propagate(new TimeSpan());

            Assert.AreEqual(10, neuralNetwork.LayerNodes[0].CurrentValue);
            Assert.AreEqual(10, neuralNetwork.LayerNodes[1].CurrentValue);
            Assert.AreEqual(10, neuralNetwork.LayerNodes[2].CurrentValue);

            Assert.AreEqual(30, neuralNetwork.OutputNodes[0].CurrentValue);
            Assert.AreEqual(30, neuralNetwork.OutputNodes[1].CurrentValue);
        }

        [TestMethod]
        public void PropagateAll2()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            InitEdgesTo1(neuralNetwork);
            neuralNetwork.InputNodes[0].CurrentValue = 10;
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 2;
            neuralNetwork.Propagate(new TimeSpan());

            Assert.AreEqual(20, neuralNetwork.LayerNodes[0].CurrentValue);
            Assert.AreEqual(10, neuralNetwork.LayerNodes[1].CurrentValue);
            Assert.AreEqual(10, neuralNetwork.LayerNodes[2].CurrentValue);

            Assert.AreEqual(40, neuralNetwork.OutputNodes[0].CurrentValue);
            Assert.AreEqual(40, neuralNetwork.OutputNodes[1].CurrentValue);
        }

        [TestMethod]
        public void Clear()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].CurrentValue = 10;
            neuralNetwork.Propagate(new TimeSpan());
            neuralNetwork.Reset();

            foreach (var inputNode in neuralNetwork.InputNodes)
            {
                Assert.AreEqual(0, inputNode.CurrentValue);
            }
            foreach (var layerNode in neuralNetwork.LayerNodes)
            {
                Assert.AreEqual(0, layerNode.CurrentValue);
            }
            foreach (var outputNode in neuralNetwork.OutputNodes)
            {
                Assert.AreEqual(0, outputNode.CurrentValue);
            }
        }

        [TestMethod]
        public void Replicate()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].CurrentValue = 10;
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Multiplier = 1;
            neuralNetwork.LayerNodes[0].OutGoingEdges[0].Multiplier = 1;
            var replicated = neuralNetwork.Replicate();

            Assert.AreEqual(1, replicated.InputNodes[0].OutGoingEdges[0].Multiplier);
            Assert.AreEqual(0, replicated.InputNodes[1].OutGoingEdges[0].Multiplier);
            Assert.AreEqual(0, replicated.InputNodes[2].OutGoingEdges[0].Multiplier);
            Assert.AreEqual(0, replicated.InputNodes[3].OutGoingEdges[0].Multiplier);

            Assert.AreEqual(1, replicated.LayerNodes[0].OutGoingEdges[0].Multiplier);
            Assert.AreEqual(0, replicated.LayerNodes[1].OutGoingEdges[0].Multiplier);
            Assert.AreEqual(0, replicated.LayerNodes[2].OutGoingEdges[0].Multiplier);
        }
    }
}
