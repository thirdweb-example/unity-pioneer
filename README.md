# pioneer

Unity SDK Template - Smart Wallets, Session Keys and Storage.

Contains three scenes:
- `00_Scene_Main`: Introduction, wallet connection, ERC1155 Claiming using session keys (granting signing permissions to the game).
- `01_Scene_Game`: Placeholder for gameplay, move forward to claim additional ERC20 tokens.
- `02_Scene_Inventory`: Displays total NFTs and Tokens held, showcases IPFS storage upload to share results.

Platforms supported: WebGL, Standalone.

Test in WebGL here: https://thirdweb-example.github.io/pioneer/

_Note: This template showcases connecting to a Smart Wallet using MetaMask, before ultimately granting a session key to a local wallet, therefore behavior in Standalone differs from WebGL, whereby Standalone/Editor builds will show a QR code to scan._

 ## Setup Instructions
 1. Clone this repository.
 2. Open in Unity 2022.3.17f1
 3. Create a [thirdweb api key](https://thirdweb.com/create-api-key)
 4. Make sure `com.thirdweb.pioneer` is an allowlisted bundle id for your API key, and enable Smart Wallets.
 5. If testing in WebGL, set allowlisted domains to `*` or to your localhost url.
 6. Find your `ThirdwebManager` in `00_Scene_Main` and set the client id there.
 7. Press Play!
