using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestClient
{
    class TestClientProgram
    {
        private static TestPeerListener _peer;

        static void Main(string[] args)
        {
            _peer = new TestPeerListener();


            if (_peer.Connect())
            {
                do
                {
                    if (_peer.InstanceId == 0)
                    {
                        _peer.Update();
                        Thread.Sleep(100);
                    }
                    else
                    {
                        _peer.SendTestEvent();
                        _peer.Update();
                        Thread.Sleep(50);
                    }
                } 
                while (true);
            }
        }
    }
}
