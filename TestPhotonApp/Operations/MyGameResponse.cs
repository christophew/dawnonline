// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MyGameResponse.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the MyGameResponse type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

using Photon.SocketServer.Rpc;

namespace TestPhotonApp.Operations
{
    #region

    

    #endregion

    public class MyGameResponse
    {
        [DataMember(Code = (byte)MyParameterCodes.Response, IsOptional = false)]
        public string Response { get; set; }
    }
}