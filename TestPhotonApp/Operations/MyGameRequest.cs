// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyGameRequest.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the MyGameRequest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Photon.SocketServer;
using Photon.SocketServer.Rpc;

namespace TestPhotonApp.Operations
{
    public class MyGameRequest : Operation
    {
        #region Constructors and Destructors

        public MyGameRequest(IRpcProtocol protocol, OperationRequest operationRequest)
            : base(protocol, operationRequest)
        {
        }

        #endregion
    }
}