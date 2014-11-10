using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.AgentMatrix.Brains.Neural;
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
                outGoingEdge.Initialize(1);
            }
        }

        [TestMethod]
        public void Propagate()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].SetValue(10);
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Initialize(1);
            neuralNetwork.LayerNodes[0].OutGoingEdges[0].Initialize(1);
            neuralNetwork.Propagate();

            Assert.AreEqual(10, neuralNetwork.LayerNodes[0].GetValue());
            Assert.AreEqual(0, neuralNetwork.LayerNodes[1].GetValue());
            Assert.AreEqual(0, neuralNetwork.LayerNodes[2].GetValue());

            Assert.AreEqual(10, neuralNetwork.OutputNodes[0].GetValue());
            Assert.AreEqual(0, neuralNetwork.OutputNodes[1].GetValue());
        }

        [TestMethod]
        public void Threshold()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].SetValue(10);
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Initialize(1);
            neuralNetwork.LayerNodes[0].OutGoingEdges[0].Initialize(1);
            neuralNetwork.InputNodes[0].Threshold = 11;
            neuralNetwork.Propagate();

            Assert.AreEqual(0, neuralNetwork.LayerNodes[0].GetValue());
            Assert.AreEqual(0, neuralNetwork.LayerNodes[1].GetValue());
            Assert.AreEqual(0, neuralNetwork.LayerNodes[2].GetValue());

            Assert.AreEqual(0, neuralNetwork.OutputNodes[0].GetValue());
            Assert.AreEqual(0, neuralNetwork.OutputNodes[1].GetValue());

            neuralNetwork.ClearInput();
            neuralNetwork.InputNodes[0].SetValue(10);
            neuralNetwork.InputNodes[0].Threshold = 10;
            neuralNetwork.Propagate();

            Assert.AreEqual(10, neuralNetwork.LayerNodes[0].GetValue());
            Assert.AreEqual(0, neuralNetwork.LayerNodes[1].GetValue());
            Assert.AreEqual(0, neuralNetwork.LayerNodes[2].GetValue());

            Assert.AreEqual(10, neuralNetwork.OutputNodes[0].GetValue());
            Assert.AreEqual(0, neuralNetwork.OutputNodes[1].GetValue());
        }

        [TestMethod]
        public void PropagateAll()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            InitEdgesTo1(neuralNetwork);
            neuralNetwork.InputNodes[0].SetValue(10);
            neuralNetwork.Propagate();

            Assert.AreEqual(10, neuralNetwork.LayerNodes[0].GetValue());
            Assert.AreEqual(10, neuralNetwork.LayerNodes[1].GetValue());
            Assert.AreEqual(10, neuralNetwork.LayerNodes[2].GetValue());

            Assert.AreEqual(30, neuralNetwork.OutputNodes[0].GetValue());
            Assert.AreEqual(30, neuralNetwork.OutputNodes[1].GetValue());
        }

        [TestMethod]
        public void PropagateAll2()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            InitEdgesTo1(neuralNetwork);
            neuralNetwork.InputNodes[0].SetValue(10);
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Initialize(2);
            neuralNetwork.Propagate();

            Assert.AreEqual(20, neuralNetwork.LayerNodes[0].GetValue());
            Assert.AreEqual(10, neuralNetwork.LayerNodes[1].GetValue());
            Assert.AreEqual(10, neuralNetwork.LayerNodes[2].GetValue());

            Assert.AreEqual(40, neuralNetwork.OutputNodes[0].GetValue());
            Assert.AreEqual(40, neuralNetwork.OutputNodes[1].GetValue());
        }

        [TestMethod]
        public void Clear()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].SetValue(10);
            neuralNetwork.Propagate();
            neuralNetwork.ClearInput();

            foreach (var inputNode in neuralNetwork.InputNodes)
            {
                Assert.AreEqual(0, inputNode.GetValue());
            }
            foreach (var layerNode in neuralNetwork.LayerNodes)
            {
                Assert.AreEqual(0, layerNode.GetValue());
            }
            foreach (var outputNode in neuralNetwork.OutputNodes)
            {
                Assert.AreEqual(0, outputNode.GetValue());
            }
        }

        [TestMethod]
        public void Replicate()
        {
            var neuralNetwork = new NeuralNetwork(4, 3, 2);

            neuralNetwork.InputNodes[0].SetValue(10);
            neuralNetwork.InputNodes[0].OutGoingEdges[0].Initialize(1);
            neuralNetwork.LayerNodes[0].OutGoingEdges[0].Initialize(1);
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
