using System.Numerics;
using Thirdweb;

public static class BlockchainManager
{
    public static BigInteger ChainId = 421614;

    public static string NftContract = "0x02bD4d543CFabe84A20B7113C5bE6902F251bEBA";
    public static string TokenContract = "0xEBB8a39D865465F289fa349A67B3391d8f910da9";

    public static SmartWallet SmartWallet { get; set; }
    public static PrivateKeyWallet LocalWallet { get; set; }
}
