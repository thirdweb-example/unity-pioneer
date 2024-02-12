using System.Collections;
using System.Collections.Generic;
using MetaMask.Unity;
using Thirdweb.Wallets;
using UnityEngine;

public class MetamaskUI_Override : MetamaskUI
{
    public void RegenerateQR()
    {
        MetaMaskUnity.Instance.Disconnect(true);
        MetaMaskUnity.Instance.Connect();
    }
}
