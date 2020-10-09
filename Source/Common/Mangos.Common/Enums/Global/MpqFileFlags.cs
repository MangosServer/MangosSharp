
namespace Mangos.Common.Enums.Global
{
    public enum MpqFileFlags : long
    {
        MPQ_Changed = 1L,                     // &H00000001
        MPQ_Protected = 2L,                   // &H00000002
        MPQ_CompressedPK = 256L,              // &H00000100
        MPQ_CompressedMulti = 512L,           // &H00000200
        MPQ_Compressed = 65280L,              // &H0000FF00
        MPQ_Encrypted = 65536L,               // &H00010000
        MPQ_FixSeed = 131072L,                // &H00020000
        MPQ_SingleUnit = 16777216L,           // &H01000000
        MPQ_Unknown_02000000 = 33554432L,     // &H02000000 - The file is only 1 byte long and its name is a hash
        MPQ_FileHasMetadata = 67108864L,      // &H04000000 - Indicates the file has associted metadata.
        MPQ_Exists = 2147483648L             // &H80000000
    }
}