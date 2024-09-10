using System.Collections;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.Assertions;
using Thirdweb.Unity;

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

        // Create local wallet
        BlockchainManager.LocalWallet ??= await PrivateKeyWallet.Generate(
            client: ThirdwebManager.Instance.Client
        );

        // Connect to canonical smart wallet using MetaMask
        var externalSmartWalletOptions = new WalletOptions(
            provider: Application.platform == RuntimePlatform.WebGLPlayer
                ? WalletProvider.MetaMaskWallet
                : WalletProvider.WalletConnectWallet,
            chainId: BlockchainManager.ChainId,
            smartWalletOptions: new SmartWalletOptions(sponsorGas: true)
        );
        BlockchainManager.SmartWallet =
            await ThirdwebManager.Instance.ConnectWallet(externalSmartWalletOptions) as SmartWallet;

        var externalWalletAddress = await (
            await BlockchainManager.SmartWallet.GetPersonalWallet()
        ).GetAddress();

        var maybeEns = await Utils.GetENSFromAddress(
            ThirdwebManager.Instance.Client,
            externalWalletAddress
        );

        // Display MetaMask address since that's what we'll use as final asset destination in this example
        PostConnectText.text = $"Welcome, {maybeEns}";

        await ConnectUsingLocalWalletAndGrantSessionKeyIfNeeded();

        ConnectButton.onClick.RemoveListener(Login);

        ConnectButton.onClick.AddListener(Claim);
        ConnectButton.interactable = true;
        NFTImage.gameObject.SetActive(true);
        _connectButtonText.text = "[ Claim Reward ]";
    }

    private async Task ConnectUsingLocalWalletAndGrantSessionKeyIfNeeded()
    {
        var localWalletAddress = await BlockchainManager.LocalWallet.GetAddress();
        Debug.Log($"Local wallet address: {localWalletAddress}");

        // Check if local wallet is an active signer already
        bool needsNewSessionKey = true;
        try
        {
            var allActiveSigners = await BlockchainManager.SmartWallet.GetAllActiveSigners();
            foreach (var signer in allActiveSigners)
            {
                Debug.Log($"Active signer: {signer.Signer}");
                if (signer.Signer.ToChecksumAddress() == localWalletAddress.ToChecksumAddress())
                {
                    Debug.Log("Local wallet is an active signer, no need to grant access.");
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
                BlockchainManager.NftContract,
                BlockchainManager.TokenContract
            }; // contracts the local wallet is allowed to interact with

            var permissionEndTimestamp = Utils.GetUnixTimeStampNow() + 86400; // 1 day from now
            _ = await BlockchainManager.SmartWallet.CreateSessionKey(
                signerAddress: localWalletAddress,
                approvedTargets: contractsAllowedForInteraction,
                nativeTokenLimitPerTransactionInWei: "0",
                permissionStartTimestamp: "0",
                permissionEndTimestamp: permissionEndTimestamp.ToString(),
                reqValidityStartTimestamp: "0",
                reqValidityEndTimestamp: (Utils.GetUnixTimeStampNow() + 3600).ToString() // 1 hour from now
            );
        }

        _connectButtonText.text = "[ Connecting ]";

        // Connect to the app using the local wallet for a signless experience
        BlockchainManager.SmartWallet = await SmartWallet.Create(
            personalWallet: BlockchainManager.LocalWallet,
            chainId: BlockchainManager.ChainId,
            gasless: true,
            // Smart wallet we want to reconnect to with a different signer
            accountAddressOverride: await BlockchainManager.SmartWallet.GetAddress()
        );
    }

    private async void Claim()
    {
        ConnectButton.interactable = false;
        _connectButtonText.text = "[ Claiming ]";

        var smartWallet = BlockchainManager.SmartWallet;
        var personalWallet = await smartWallet.GetPersonalWallet();

        var rand = Random.Range(0, 5);
        var contract = await ThirdwebManager.Instance.GetContract(
            BlockchainManager.NftContract,
            BlockchainManager.ChainId
        );
        var txRes = await contract.DropERC1155_Claim(
            wallet: smartWallet,
            receiverAddress: await personalWallet.GetAddress(),
            tokenId: rand,
            quantity: 1
        );

        ConnectButton.onClick.RemoveListener(Claim);
        _connectButtonText.text = "[ Claimed ]";

        var nft = await contract.ERC1155_GetNFT(rand);
        NFTImage.sprite = await nft.GetNFTSprite(client: ThirdwebManager.Instance.Client);
        NFTImage.color = Color.white;
        NFTButton.onClick.AddListener(() =>
        {
            Application.OpenURL($"https://sepolia.arbiscan.io/tx/{txRes.TransactionHash}");
        });
        NFTButton.interactable = true;

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
