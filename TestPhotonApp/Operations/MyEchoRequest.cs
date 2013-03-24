// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyEchoRequest.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the MyEchoRequest type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Photon.SocketServer;
using Photon.SocketServer.Rpc;

namespace TestPhotonApp.Operations
{
    public class MyEchoRequest : Operation
    {
        #region Constructors and Destructors

        public MyEchoRequest(IRpcProtocol protocol, OperationRequest operationRequest)
            : base(protocol, operationRequest)
        {
        }

        #endregion

        #region Properties

        [DataMember(Code = (byte)MyParameterCodes.Text, IsOptional = false)]
        public string Text { get; set; }

        #endregion
    }
}