using System.Collections;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Thirdweb.Redcode.Awaiting;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Assertions;

public class TutorialManager : MonoBehaviour
{
    public Button ConnectButton;
    public TMP_Text PostConnectText;
    public Button NFTButton;
    public Image NFTImage;

    private TMP_Text _connectButtonText;

    private void Awake()
    {
        NFTImage.gameObject.SetActive(false);

        _connectButtonText = ConnectButton.GetComponentInChildren<TMP_Text>();
        _connectButtonText.alpha = 0;

        ConnectButton.onClick.AddListener(Login);
        ConnectButton.interactable = false;
    }

    public void BeginTutorial()
    {
        SoundManager.Instance.PlaySound(SoundName.Charlotte_Inspiring);
        StartCoroutine(ShowConnectButton());
    }

    private IEnumerator ShowConnectButton()
    {
        while (_connectButtonText.alpha < 1)
        {
            _connectButtonText.alpha += Time.deltaTime / 2;
            yield return null;
        }

        ConnectButton.interactable = true;
    }

    private async void Login()
    {
        ConnectButton.interactable = false;
        _connectButtonText.text = "[ Connecting ]";

        var isConnected = await ThirdwebManager.Instance.SDK.wallet.IsConnected();
        if (isConnected)
            await ThirdwebManager.Instance.SDK.wallet.Disconnect();

        // Store local wallet signer address for later use
        var localWalletConnection = new WalletConnection(
            provider: WalletProvider.LocalWallet,
            chainId: 421614
        );
        var localWalletAddress = await ThirdwebManager.Instance.SDK.wallet.Connect(
            localWalletConnection
        );
        PlayerPrefs.SetString("LOCAL_WALLET_ADDRESS", localWalletAddress);

        await ThirdwebManager.Instance.SDK.wallet.Disconnect(); // sanity

        // Connect to canonical smart wallet using MetaMask
        var walletConnection = new WalletConnection(
            provider: WalletProvider.SmartWallet,
            chainId: 421614,
            personalWallet: WalletProvider.Metamask
        );
        var smartWalletAddress = await ThirdwebManager.Instance.SDK.wallet.Connect(
            walletConnection
        );
        var metamaskAddress = await ThirdwebManager.Instance.SDK.wallet.GetSignerAddress();

        // Store MetaMask admin and Smart Wallet addresses for later use
        PlayerPrefs.SetString("SMART_WALLET_ADDRESS", smartWalletAddress);
        PlayerPrefs.SetString("PERSONAL_WALLET_ADDRESS", metamaskAddress);

        // Display MetaMask address since that's what we'll use as final asset destination in this example
        PostConnectText.text = $"Welcome, {metamaskAddress}";
        await ConnectUsingLocalWalletAndGrantSessionKeyIfNeeded(
            smartWalletAddress,
            localWalletAddress
        );

        ConnectButton.onClick.RemoveListener(Login);

        ConnectButton.onClick.AddListener(Claim);
        ConnectButton.interactable = true;
        NFTImage.gameObject.SetActive(true);
        _connectButtonText.text = "[ Claim Reward ]";
    }

    private async Task ConnectUsingLocalWalletAndGrantSessionKeyIfNeeded(
        string smartWalletAddress,
        string localWalletAddress
    )
    {
        // Check if local wallet is an active signer already
        bool needsNewSessionKey = true;
        try
        {
            var allActiveSigners = await ThirdwebManager.Instance.SDK.wallet.GetAllActiveSigners();
            foreach (var signer in allActiveSigners)
            {
                if (signer.signer.ToChecksumAddress() == localWalletAddress.ToChecksumAddress())
                {
                    needsNewSessionKey = false;
                    break;
                }
            }
        }
        catch
        {
            Debug.Log("Smart Wallet not deployed, granting access to app.");
        }

        // If it is, grant a new session key
        if (needsNewSessionKey)
        {
            _connectButtonText.text = "[ Granting Access ]";
            // Create session key granting restricted smart wallet access to the local wallet for 1 day
            var contractsAllowedForInteraction = new List<string>()
            {
                GameContracts.NFT_CONTRACT,
                GameContracts.TOKEN_CONTRACT
            }; // contracts the local wallet is allowed to interact with

            var permissionEndTimestamp = Utils.GetUnixTimeStampNow() + 86400; // 1 day from now
            var result = await ThirdwebManager.Instance.SDK.wallet.CreateSessionKey(
                signerAddress: localWalletAddress,
                approvedTargets: contractsAllowedForInteraction,
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: Utils.GetUnixTimeStampIn10Years().ToString()
            );
        }

        _connectButtonText.text = "[ Connecting ]";

        await ThirdwebManager.Instance.SDK.wallet.Disconnect(); // sanity

        // Connect to the app using the local wallet for a signless experience
        var finalWalletConnection = new WalletConnection(
            provider: WalletProvider.SmartWallet,
            chainId: 421614,
            personalWallet: WalletProvider.LocalWallet,
            smartWalletAccountOverride: smartWalletAddress
        );
        var swAddy = await ThirdwebManager.Instance.SDK.wallet.Connect(finalWalletConnection);
        Assert.IsTrue(smartWalletAddress == swAddy);
    }

    private async void Claim()
    {
        var mmAddress = PlayerPrefs.GetString("PERSONAL_WALLET_ADDRESS");

        ConnectButton.interactable = false;
        _connectButtonText.text = "[ Claiming ]";

        var rand = Random.Range(0, 5);
        var contract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.NFT_CONTRACT);
        var txRes = await contract.ERC1155.ClaimTo(mmAddress, rand.ToString(), 1);

        ConnectButton.onClick.RemoveListener(Claim);
        _connectButtonText.text = "[ Claimed ]";

        var nftInfo = await contract.ERC1155.Get(rand.ToString());
        NFTImage.sprite = await ThirdwebManager.Instance.SDK.storage.DownloadImage(
            nftInfo.metadata.image
        );
        NFTImage.color = Color.white;
        NFTButton.onClick.AddListener(() =>
        {
            Application.OpenURL($"https://sepolia.arbiscan.io/tx/{txRes.receipt.transactionHash}");
        });
        NFTButton.interactable = true;

        await new WaitForSeconds(3);

        _connectButtonText.text = "[ Explore ]";
        ConnectButton.onClick.AddListener(() =>
        {
            SceneManager.LoadScene("01_Scene_Game");
        });
        ConnectButton.interactable = true;
    }

    private void OnDestroy()
    {
        ConnectButton.onClick.RemoveAllListeners();
        NFTButton.onClick.RemoveAllListeners();
    }
}
