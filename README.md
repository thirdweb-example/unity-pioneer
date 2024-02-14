# pioneer

Unity SDK Template - Smart Wallets, Session Keys and Storage.

![pioneer](https://github.com/thirdweb-example/pioneer/assets/43042585/c224fd85-37e6-41a7-b16f-c5483a0b39a5)

Contains three scenes:
- `00_Scene_Main`: Introduction, wallet connection, ERC1155 Claiming using session keys (granting signing permissions to the game).
- `01_Scene_Game`: Placeholder for gameplay, move forward to claim additional ERC20 tokens. (WASD or arrow keys to move, go forward for next step)
- `02_Scene_Inventory`: Displays total NFTs and Tokens held, showcases IPFS storage upload to share results. (Press `Play Again` to reconnect without having to sign with MM again using session keys)

Platforms supported: WebGL, Standalone.

Test in WebGL here: https://thirdweb-example.github.io/pioneer/

_Note: This template showcases connecting to a Smart Wallet using MetaMask, before ultimately granting a session key to a local wallet, therefore behavior in Standalone differs from WebGL, whereby Standalone/Editor builds will show a QR code to scan. This example also shows how to override MetamaskUI.cs and the WalletProvider_Metamask default prefab._

 ## Setup Instructions
 1. Clone this repository.
 2. Open in Unity 2022.3.17f1
 3. Create a [thirdweb api key](https://thirdweb.com/create-api-key)
 4. Make sure `com.thirdweb.pioneer` is an allowlisted bundle id for your API key, and enable Smart Wallets.
 5. If testing in WebGL, set allowlisted domains to `*` or to your localhost url.
 6. Find your `ThirdwebManager` in `00_Scene_Main` and set the client id there.
 7. Press Play!

To build the game, make sure you follow our build instructions [here](https://github.com/thirdweb-dev/unity-sdk#build).
