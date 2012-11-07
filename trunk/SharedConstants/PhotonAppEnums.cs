
namespace SharedConstants
{
    /// <summary>
    /// Defines custom operation codes
    /// 
    /// !!!!!!!!!! ADD TO MyPeer.OnOperationRequest !!!!!!!!!!!!!!
    /// 
    /// </summary>
    public enum MyOperationCodes : byte
    {
        EchoOperation = 100,
        GameOperation = 101,
        AvatarCommand = 102,
        LoadWorld = 103,
        AddEntity = 104,
    }

    public enum EventCode : byte
    {
        WorldInfo = 101,
        Destroyed = 103,
        BulkPositionUpdate = 104,
    }

    /// <summary>
    /// Defines custom paramter codes
    /// </summary>
    public enum MyParameterCodes : byte
    {
        Text = 100,
        Response = 101,
    }
}
