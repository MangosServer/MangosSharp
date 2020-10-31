namespace Mangos.Cluster
{
    public class PacketEncryptionState
    {
        public bool IsEncryptionEnabled { get; set; }
        public byte[] Hash { get; set; }
        public byte[] Key { get; } = new byte[4];
    }
}
