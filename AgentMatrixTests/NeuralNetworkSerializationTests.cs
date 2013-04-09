using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using DawnOnline.AgentMatrix.Brains.Neural;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AgentMatrixTests
{
    [TestClass]
    public class NeuralNetworkSerializationTests
    {
        [TestMethod]
        public void TestThresholds()
        {
            const int startCounter = -10;

            var neuralNetwork = new NeuralNetwork(4, 10, 4, 10);

            int counter = startCounter;
            foreach (var node in neuralNetwork.InputNodes)
            {
                node.Threshold = counter++;
            }
            foreach (var node in neuralNetwork.LayerNodes)
            {
                node.Threshold = counter++;
            }
            foreach (var node in neuralNetwork.OutputNodes)
            {
                node.Threshold = counter++;
            }
            foreach (var node in neuralNetwork.ReinforcementInputNodes)
            {
                node.Threshold = counter++;
            }

            var stream = new MemoryStream();
            neuralNetwork.Serialize(stream);
            stream.Close();

            var restoredNetwork = new NeuralNetwork(4, 10, 4, 10);
            var restoredStream = new MemoryStream(stream.GetBuffer());
            restoredNetwork.Deserialize(restoredStream);

            // test correct values
            counter = startCounter;
            foreach (var node in neuralNetwork.InputNodes)
            {
                Assert.AreEqual(counter++, node.Threshold);
            }
            foreach (var node in neuralNetwork.LayerNodes)
            {
                Assert.AreEqual(counter++, node.Threshold);
            }
            foreach (var node in neuralNetwork.OutputNodes)
            {
                Assert.AreEqual(counter++, node.Threshold);
            }
            foreach (var node in neuralNetwork.ReinforcementInputNodes)
            {
                Assert.AreEqual(counter++, node.Threshold);
            }
        }        
        
        [TestMethod]
        public void TestMultipliers()
        {
            const double startCounter = -1.0;


            var neuralNetwork = new NeuralNetwork(4, 10, 4, 10);

            double counter = startCounter;
            foreach (var node in neuralNetwork.InputNodes)
            {
                foreach (var edge in node.OutGoingEdges)
                {
                    edge.Multiplier = counter;
                    counter += 0.01;
                }
            }
            foreach (var node in neuralNetwork.LayerNodes)
            {
                foreach (var edge in node.OutGoingEdges)
                {
                    edge.Multiplier = counter;
                    counter += 0.01;
                }
            }
            foreach (var node in neuralNetwork.ReinforcementInputNodes)
            {
                foreach (var edge in node.OutGoingEdges)
                {
                    edge.Multiplier = counter;
                    counter += 0.01;
                }
            }

            var stream = new MemoryStream();
            neuralNetwork.Serialize(stream);
            stream.Close();

            var restoredNetwork = new NeuralNetwork(4, 10, 4, 10);
            var restoredStream = new MemoryStream(stream.GetBuffer());
            restoredNetwork.Deserialize(restoredStream);

            // test correct values
            counter = startCounter;
            foreach (var node in neuralNetwork.InputNodes)
            {
                foreach (var edge in node.OutGoingEdges)
                {
                    Assert.AreEqual(counter, edge.Multiplier);
                    counter += 0.01;
                }
            }
            foreach (var node in neuralNetwork.LayerNodes)
            {
                foreach (var edge in node.OutGoingEdges)
                {
                    Assert.AreEqual(counter, edge.Multiplier);
                    counter += 0.01;
                }
            }
            foreach (var node in neuralNetwork.ReinforcementInputNodes)
            {
                foreach (var edge in node.OutGoingEdges)
                {
                    Assert.AreEqual(counter, edge.Multiplier);
                    counter += 0.01;
                }
            }

        }
    }
}
