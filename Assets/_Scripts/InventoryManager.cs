using System.Collections;
using System.Collections.Generic;
using System.IO;
using Thirdweb;
using TMPro;
using UnityEngine;
using Thirdweb.Redcode.Awaiting;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InventoryManager : MonoBehaviour
{
    public TMP_Text NFTsEarnedText;
    public TMP_Text TokensEarnedText;
    public TMP_Text TotalNFTsText;
    public TMP_Text TotalTokensText;
    public Button UploadButton;

    private Contract _nftContract;
    private Contract _tokenContract;

    private async void Awake()
    {
        var uploadButtonText = UploadButton.GetComponentInChildren<TMP_Text>();
        uploadButtonText.alpha = 0;

        _nftContract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.NFT_CONTRACT);
        _tokenContract = ThirdwebManager.Instance.SDK.GetContract(GameContracts.TOKEN_CONTRACT);

        var addy = PlayerPrefs.GetString("PERSONAL_WALLET_ADDRESS");

        var nftsEarnedText = "NFTs earned...1";
        await StartCoroutine(SetText(NFTsEarnedText, nftsEarnedText));
        var tokensEarnedText = "Tokens earned...100";
        await StartCoroutine(SetText(TokensEarnedText, tokensEarnedText));

        var nftsOwned = await _nftContract.ERC1155.GetOwned(addy);
        int nftBalance = 0;
        foreach (NFT nft in nftsOwned)
        {
            nftBalance += nft.quantityOwned;
        }

        var tokenBalance = (await _tokenContract.ERC20.BalanceOf(addy)).value.ToEth(0, false);

        var totalNftsText = nftBalance + " Total NFTs";
        await StartCoroutine(SetText(TotalNFTsText, totalNftsText));
        var totalTokensText = tokenBalance + " Total Tokens";
        await StartCoroutine(SetText(TotalTokensText, totalTokensText));

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

        await new WaitForSeconds(3f);

        var response = await ThirdwebManager.Instance.SDK.storage.UploadFromPath(fullPath);

        UploadButton.interactable = true;
        UploadButton.GetComponentInChildren<TMP_Text>().text = "[ View ]";
        UploadButton.onClick.AddListener(() =>
        {
            Application.OpenURL(
                $"https://{ThirdwebManager.Instance.clientId}.ipfscdn.io/ipfs/{response.IpfsHash}"
            );
            UploadButton.GetComponentInChildren<TMP_Text>().text = "[ Play Again ]";
            UploadButton.onClick.RemoveAllListeners();
            UploadButton.onClick.AddListener(Restart);
        });
    }

    IEnumerator SetText(TMP_Text text, string value)
    {
        text.alpha = 0f;
        text.text = value;

        while (text.alpha < 1)
        {
            text.alpha += Time.deltaTime / 2;
            yield return null;
        }
    }

    private async void Restart()
    {
        await ThirdwebManager.Instance.SDK.wallet.Disconnect(); // sanity
        SceneManager.LoadScene("00_Scene_Main");
    }
}
