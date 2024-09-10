using System.Collections;
using System.Collections.Generic;
using System.IO;
using Thirdweb;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Thirdweb.Unity;
using System.Threading.Tasks;
using System.Numerics;

public class InventoryManager : MonoBehaviour
{
    public TMP_Text NFTsEarnedText;
    public TMP_Text TokensEarnedText;
    public TMP_Text TotalNFTsText;
    public TMP_Text TotalTokensText;
    public Button UploadButton;

    private ThirdwebContract _nftContract;
    private ThirdwebContract _tokenContract;

    private async void Awake()
    {
        var uploadButtonText = UploadButton.GetComponentInChildren<TMP_Text>();
        uploadButtonText.alpha = 0;

        _nftContract = await ThirdwebManager.Instance.GetContract(
            BlockchainManager.NftContract,
            BlockchainManager.ChainId
        );
        _tokenContract = await ThirdwebManager.Instance.GetContract(
            BlockchainManager.TokenContract,
            BlockchainManager.ChainId
        );

        var nftsEarnedText = "NFTs earned...1";
        await SetText(NFTsEarnedText, nftsEarnedText);
        var tokensEarnedText = "Tokens earned...100";
        await SetText(TokensEarnedText, tokensEarnedText);

        var externalWalletAddress = await (
            await BlockchainManager.SmartWallet.GetPersonalWallet()
        ).GetAddress();

        var nftsOwned = await _nftContract.ERC1155_GetOwnedNFTs(externalWalletAddress);
        BigInteger nftBalance = 0;
        foreach (NFT nft in nftsOwned)
        {
            nftBalance += nft.QuantityOwned ?? 0;
        }

        var tokenBalance = (await _tokenContract.ERC20_BalanceOf(externalWalletAddress))
            .ToString()
            .ToEth(0, false);

        var totalNftsText = nftBalance + " Total NFTs";
        await SetText(TotalNFTsText, totalNftsText);
        var totalTokensText = tokenBalance + " Total Tokens";
        await SetText(TotalTokensText, totalTokensText);

        UploadButton.onClick.AddListener(OnUpload);
        UploadButton.interactable = true;
        await SetText(uploadButtonText, "[ Upload Results ]");
    }

    public async void OnUpload()
    {
        UploadButton.interactable = false;
        UploadButton.onClick.RemoveListener(OnUpload);
        UploadButton.GetComponentInChildren<TMP_Text>().text = "[ Smile! ]";

        string fullPath = Application.temporaryCachePath + "/pioneerSS.png";
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        ScreenCapture.CaptureScreenshot(fullPath);

        await Task.Delay(100);

        var response = await ThirdwebStorage.Upload(ThirdwebManager.Instance.Client, fullPath);

        UploadButton.interactable = true;
        UploadButton.GetComponentInChildren<TMP_Text>().text = "[ View ]";
        UploadButton.onClick.AddListener(() =>
        {
            Application.OpenURL(
                $"https://{ThirdwebManager.Instance.Client.ClientId}.ipfscdn.io/ipfs/{response.IpfsHash}"
            );
            UploadButton.GetComponentInChildren<TMP_Text>().text = "[ Play Again ]";
            UploadButton.onClick.RemoveAllListeners();
            UploadButton.onClick.AddListener(Restart);
        });
    }

    private async Task SetText(TMP_Text text, string value)
    {
        text.alpha = 0f;
        text.text = value;

        while (text.alpha < 1)
        {
            text.alpha += Time.deltaTime / 2;
            await Task.Yield();
        }
    }

    private void Restart()
    {
        BlockchainManager.SmartWallet = null;
        SceneManager.LoadScene("00_Scene_Main");
    }
}
