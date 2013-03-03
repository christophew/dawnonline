using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DawnPhotonApp
{
    interface IEntityPhotonPacket
    {
        int Id { get; }
        Hashtable CreatePhotonPacket();
        bool HasDeltaChanges(IEntityPhotonPacket previous);

        DateTime LastUpdateSend { get; set; }
    }
}
