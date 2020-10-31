namespace Mangos.Cluster
{
    public class Client
    {
        public PacketEncryptionState PacketEncryption { get; }

        public Client()
        {
            PacketEncryption = new PacketEncryptionState();
        }
    }
}
