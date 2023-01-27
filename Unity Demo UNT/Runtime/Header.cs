namespace Unt.Demo.Runtime
{
    public enum Header : byte
    {
        None,
        Connected,
        ClientConnected,
        ClientDisconnected,
        Data,
        RPC,
    }
}
