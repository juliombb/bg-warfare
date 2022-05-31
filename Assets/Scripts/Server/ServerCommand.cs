namespace Server
{
    public enum ServerCommand: byte
    {
        PositionOfPlayer,
        PositionOfPlayers,
        OtherPlayerDisconnected,
        ClientPeerId,
        Shot
    }
}