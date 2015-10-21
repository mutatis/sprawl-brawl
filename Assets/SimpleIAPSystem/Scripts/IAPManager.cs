/*  This file is part of the "Simple IAP System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them directly or indirectly
 *  from Rebound Games. You shall not license, sublicense, sell, resell, transfer, assign,
 *  distribute or otherwise make available to any third party the Service or the Content. 
 */

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Prime31;

namespace SIS
{
    /// <summary>
    /// Prime31 IAP cross-platform wrapper for real money purchases,
    /// as well as for virtual ingame purchases (for virtual currency)
    /// </summary>
	public class IAPManager : MonoBehaviour
	{
        /// <summary>
        /// whether this script should print debug messages
        /// </summary>
        public bool debug;

        /// <summary>
        /// your Google developer id
        /// </summary>
        public string googleStoreKey;

        /// <summary>
        /// your server url for online IAP verification or remote config
        /// </summary>
        public string serverUrl;

        /// <summary>
        /// type for online IAP verification
        /// </summary>
        public VerificationType verificationType = VerificationType.none;

        /// <summary>
        /// relative url to your php verification file
        /// </summary>
        public string verificationFileName;

        /// <summary>
        /// type for processing remotely hosted configs
        /// </summary>
        public RemoteType remoteType = RemoteType.none;

        /// <summary>
        /// relative url to your remotely hosted config file
        /// </summary>
        public string remoteFileName;

        /// <summary>
        /// static reference to this script
        /// </summary>
        private static IAPManager instance;

        /// <summary>
        /// object for downloading hosted configs
        /// </summary>
        private WWW request;

        //disable platform specific warnings, because Unity throws them
        //for unused variables however they are used in this context
        #pragma warning disable 0414
        /// <summary>
        /// array of real money iap ids
        /// </summary>
        private string[] ids = null;

        /// <summary>
        /// cached online product data
        /// </summary>
        private List<IAPArticle> productCache = new List<IAPArticle>();
        #pragma warning restore 0414

        /// <summary>
        /// In app products, set in the IAP Settings editor
        /// </summary>
        [HideInInspector]
        public List<IAPGroup> IAPs = new List<IAPGroup>();

        /// <summary>
        /// list of virtual currency,
        /// set in the IAP Settings editor
        /// </summary>
        [HideInInspector]
        public List<IAPCurrency> currency = new List<IAPCurrency>();

        /// <summary>
        /// list of ingame content items,
        /// set in the IAP Settings editor
        /// </summary>
        [HideInInspector]
        public List<IAPGroup> IGCs = new List<IAPGroup>();

        /// <summary>
        /// dictionary of product ids,
        /// mapped to the corresponding IAPObject for quick lookup
        /// </summary>
        public Dictionary<string, IAPObject> IAPObjects = new Dictionary<string, IAPObject>();

        /// <summary>
        /// fired when a purchase succeeds, delivering its product id
        /// </summary>
        public static event Action<string> purchaseSucceededEvent;

        /// <summary>
        /// fired when a purchase fails, delivering its product id
        /// </summary>
        public static event Action<string> purchaseFailedEvent;

        /// <summary>
        /// fired when a server request fails, delivering the error message
        /// </summary>
        public static event Action<string> inventoryRequestFailedEvent;


        // initialize IAPs, billing systems and database,
        // as well as shop components in this order
        void Awake()
        {
            //make sure we keep one instance of this script in the game
            if (instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(this);

            //ensure that we are always developing on a mobile platform,
            //otherwise print a warning
            #if !UNITY_ANDROID && !UNITY_IPHONE
            Debug.LogWarning("IAPManager: Detected non-mobile platform. Purchases for real money are"
                             + " not supported on this platform. Please switch to iOS/Android.");
            #endif

            //set static reference
            instance = this;
            //populate IAP dictionary and arrays with product ids
            InitIds();

            //map Prime31 IAP handler delegates to fire corresponding methods
            #if UNITY_ANDROID
            GoogleIABManager.billingSupportedEvent += RequestProductData;
            GoogleIABManager.billingNotSupportedEvent += BillingNotSupported;
            GoogleIABManager.queryInventorySucceededEvent += ProductDataReceived;
            GoogleIABManager.queryInventoryFailedEvent += BillingNotSupported;
            GoogleIABManager.purchaseSucceededEvent += PurchaseSucceeded;
            GoogleIABManager.purchaseFailedEvent += PurchaseFailed;
            GoogleIABManager.consumePurchaseSucceededEvent += ConsumeSucceeded;
            GoogleIABManager.consumePurchaseFailedEvent += PurchaseFailed;

            //initializes Google's billing system
            //by passing in your google dev key
            if (!string.IsNullOrEmpty(googleStoreKey))
                GoogleIAB.init(googleStoreKey);
            else
                Debug.LogWarning("IAPManager: Google Store Key missing on IAP Manager prefab. "
                               + "Purchases for real money won't be supported on the device.");

            //enables high detail logging to the console,
            //do not comment out in production versions!
            //this is for testing only
            GoogleIAB.enableLogging(false);

            #elif UNITY_IPHONE
            StoreKitManager.productListReceivedEvent += ProductDataReceived;
            StoreKitManager.productListRequestFailedEvent += BillingNotSupported;
            StoreKitManager.purchaseSuccessfulEvent += PurchaseSucceeded;
            StoreKitManager.purchaseFailedEvent += PurchaseFailed;
            StoreKitManager.purchaseCancelledEvent += PurchaseFailed;
            StoreKitManager.restoreTransactionsFinishedEvent += RestoreSucceeded;
            StoreKitManager.restoreTransactionsFailedEvent += RestoreFailed;
            
            //initializes Apple's billing system
            //when parental controls aren't active
            if(StoreKitBinding.canMakePayments())
                RequestProductData();
            else
                BillingNotSupported("Apple App Store not available.");
            #endif

            //initialize database, remote and shop managers
            GetComponent<DBManager>().Init();
            StartCoroutine(RemoteDownload());
            OnLevelWasLoaded(-1);
        }


        /// <summary>
        /// initiate shop manager initialization on scene change
        /// </summary>
        public void OnLevelWasLoaded(int level)
        {
            if (instance != this)
                return;

            ShopManager shop = null;
            GameObject shopGO = GameObject.Find("Shop Manager");
            if (shopGO) shop = shopGO.GetComponent<ShopManager>();
            if (shop)
            {
                shop.Init();
                #if !UNITY_EDITOR
                    ShopManager.OverwriteWithFetch(productCache);
                #endif
            }
        }


        /// <summary>
        /// returns a static reference to this script
        /// </summary>
        public static IAPManager GetInstance()
        {
            return instance;
        }


        // unbind delegates on the active instance
        void OnDestroy()
        {
            if (instance != this)
                return;

            #if UNITY_ANDROID
                GoogleIABManager.billingSupportedEvent -= RequestProductData;
                GoogleIABManager.billingNotSupportedEvent -= BillingNotSupported;
                GoogleIABManager.queryInventorySucceededEvent -= ProductDataReceived;
                GoogleIABManager.queryInventoryFailedEvent -= BillingNotSupported;
                GoogleIABManager.purchaseSucceededEvent -= PurchaseSucceeded;
                GoogleIABManager.purchaseFailedEvent -= PurchaseFailed;
                GoogleIABManager.consumePurchaseSucceededEvent -= ConsumeSucceeded;
                GoogleIABManager.consumePurchaseFailedEvent -= PurchaseFailed;
            #elif UNITY_IPHONE
                StoreKitManager.productListReceivedEvent -= ProductDataReceived;
                StoreKitManager.productListRequestFailedEvent -= BillingNotSupported;
                StoreKitManager.purchaseSuccessfulEvent -= PurchaseSucceeded;
                StoreKitManager.purchaseFailedEvent -= PurchaseFailed;
                StoreKitManager.purchaseCancelledEvent -= PurchaseFailed;
                StoreKitManager.restoreTransactionsFinishedEvent -= RestoreSucceeded;
                StoreKitManager.restoreTransactionsFailedEvent -= RestoreFailed;
            #endif
        }


        // initialize IAP ids:
        // populate IAP dictionary and arrays with product ids
        private void InitIds()
        {
            //create temporary list for all IAPGroups,
            //as well as a list only for real money purchases
            List<IAPGroup> idsList = GetIAPs();
            List<string> ids = new List<string>();

            if (idsList.Count == 0)
                Debug.LogError("Initializing IAPManager, but IAP List is empty."
                               + " Did you set up IAPs in the IAP Settings?");

            //loop over all groups
            for (int i = 0; i < idsList.Count; i++)
            {
                //cache current group
                IAPGroup group = idsList[i];
                //loop over items in this group
                for (int j = 0; j < group.items.Count; j++)
                {
                    //cache item
                    IAPObject obj = group.items[j];
                    if (String.IsNullOrEmpty(obj.id))
                        Debug.LogError("Found IAP Object in IAP Settings without an ID."
                                       + " This will cause errors during runtime.");

                    //add this IAPObject to the dictionary of id <> IAPObject
                    IAPObjects.Add(obj.id, obj);
                    //if it's an IAP for real money, also add it to the id list
                    if (obj.type == IAPType.consumable || obj.type == IAPType.nonConsumable
                       || obj.type == IAPType.subscription)
                        ids.Add(obj.GetIdentifier());
                }
            }

            //don't add the restore button to the list of online purchases
            if (ids.Contains("restore")) ids.Remove("restore");
            //convert and store list of real money IAP ids as string array,
            //this array is being used for initializing Google/Apple's billing system 
            this.ids = ids.ToArray();
        }


        // when billing is supported on the device,
        // this method requests IAP product data from Google/Apple
        private void RequestProductData()
        {
            #if UNITY_IPHONE
                StoreKitBinding.requestProductData(ids);
            #elif UNITY_ANDROID
                GoogleIAB.queryInventory(ids);
            #endif
        }


        #if UNITY_IPHONE
        // iOS version: receive StoreKitProducts.
        // Optionally: verify old purchases online.
        // Once we've received the productList, we create a
        // cross-platform version of it and overwrite
        // the existing shop item values with this online data
        private void ProductDataReceived(List<StoreKitProduct> list)
        {
			if ((verificationType == VerificationType.onStart || verificationType == VerificationType.both)
                && !string.IsNullOrEmpty(serverUrl))
                VerifyReceipts();
        
            productCache = new List<IAPArticle>();
            for(int i = 0; i < list.Count; i++)
                productCache.Add(new IAPArticle(list[i]));
        
            if(ShopManager.GetInstance())
                ShopManager.OverwriteWithFetch(productCache);
        }
        #endif
        #if UNITY_ANDROID
        /// <summary>
        /// cache purchased products for possible restore, set below
        /// </summary>
        static List<GooglePurchase> prods;

        // Android version: receive GoogleSkuInfos.
        // Optionally: verify old purchases online.
        // Once we've received the productList, we create a
        // cross-platform version of it and overwrite
        // the existing shop item values with this online data
        void ProductDataReceived(List<GooglePurchase> purchases, List<GoogleSkuInfo> list)
        {
            //store old purchases for verification purposes
            prods = purchases;

            if ((verificationType == VerificationType.onStart || verificationType == VerificationType.both)
                && !string.IsNullOrEmpty(serverUrl))
                VerifyReceipts();

            productCache = new List<IAPArticle>();
            for (int i = 0; i < list.Count; i++)
                productCache.Add(new IAPArticle(list[i]));

            if (ShopManager.GetInstance())
                ShopManager.OverwriteWithFetch(productCache);
        }
        #endif


        /// <summary>
        /// purchase consumable product based on its product id.
        /// If the productId matches "restore", we restore iaps instead.
        /// Our delegates then fire the appropriate succeeded/fail event
        /// </summary>
        public static void PurchaseConsumableProduct(string productId)
        {
            if (productId == "restore")
                RestoreTransactions();
            else
            {
                productId = GetIAPObject(productId).GetIdentifier();
                #if UNITY_ANDROID
                    GoogleIAB.purchaseProduct(productId, "consume");
                #elif UNITY_IPHONE
                    StoreKitBinding.purchaseProduct(productId, 1);
                #endif
            }
        }


        /// <summary>
        /// purchase non-consumable product based on its product id.
        /// Our delegates then fire the appropriate succeeded/fail event.
        /// Additionally, non-consumables are being saved to the database
        /// </summary>
        public static void PurchaseNonconsumableProduct(string productId)
        {
            productId = GetIAPObject(productId).GetIdentifier();
            #if UNITY_ANDROID
                GoogleIAB.purchaseProduct(productId, "nonconsume");
            #elif UNITY_IPHONE
                StoreKitBinding.purchaseProduct(productId, 1);
            #endif
        }


        /// <summary>
        /// purchase subscription based on its product id.
        /// Our delegates then fire the appropriate succeeded/fail event.
        /// On Android we have to check whether subscriptions are supported
        /// </summary>
        public static void PurchaseSubscription(string productId)
        {
            productId = GetIAPObject(productId).GetIdentifier();
            #if UNITY_ANDROID
            if (GoogleIAB.areSubscriptionsSupported())
                GoogleIAB.purchaseProduct(productId);
            else
                BillingNotSupported("Subscriptions not available.");
            #elif UNITY_IPHONE
                StoreKitBinding.purchaseProduct(productId, 1);
            #endif
        }


        #if UNITY_IPHONE
        // fired when a purchase succeeds, iOS version.
        // also fired for each transaction when restoring transactions
        // Optionally: verify new product transaction online
        private void PurchaseSucceeded(StoreKitTransaction prod)
        {
            string id = GetIAPIdentifier(prod.productIdentifier);

            if ((verificationType == VerificationType.onPurchase || verificationType == VerificationType.both)
                && !string.IsNullOrEmpty(serverUrl))
            {
                MakeRequest(prod);
                return;
            }

            PurchaseVerified(id);
        }
        #endif
        #if UNITY_ANDROID
        // fired when a purchase succeeds, Android version.
        // Optionally: verify new product transaction online
        private void PurchaseSucceeded(GooglePurchase prod)
        {
            string id = GetIAPIdentifier(prod.productId);

            if ((verificationType == VerificationType.onPurchase || verificationType == VerificationType.both)
                && !string.IsNullOrEmpty(serverUrl))
            {
                MakeRequest(prod);
                return;
            }

            PurchaseVerified(id);
        }


        // fired after a successful consumption (see PurchaseVerified()).
        // Method that fires the purchase succeeded action
        private void ConsumeSucceeded(GooglePurchase prod)
        {
            purchaseSucceededEvent(GetIAPIdentifier(prod.productId));
        }


        // online verification request (Android version)
        // here we build the POST request to our external server,
        // that will forward the request to Google's servers
        private void MakeRequest(GooglePurchase prod)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("store", "Android");
            dic.Add("pid", prod.productId);
            dic.Add("tid", prod.orderId);
            dic.Add("rec", prod.purchaseToken);
            IAPObject obj = GetIAPObject(prod.productId);
            if (obj != null && obj.type == IAPType.subscription)
                dic.Add("type", "subs");
            StartCoroutine(WaitForRequest(dic));
        }
        #endif
        #if UNITY_IPHONE
        // online verification request (iOS version)
        // here we build the POST request to our external server,
        // that will forward the request to Apple's servers
        private void MakeRequest(StoreKitTransaction prod)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("store", "IOS");
            dic.Add("pid", prod.productIdentifier);
            dic.Add("rec", prod.base64EncodedTransactionReceipt);
            StartCoroutine(WaitForRequest(dic));
        }
        #endif


        // handles an online verification request and response from 
        // our external server. www.text can returns true or false.
        // true: purchase verified, false: not verified (fake?) purchase
        IEnumerator WaitForRequest(Dictionary<string, string> dic)
        {
            //cache requested product id
            string id = dic["pid"];

            //build POST request with transaction data
            WWWForm form = new WWWForm();
            foreach (string key in dic.Keys)
                form.AddField(key, dic[key]);
            //create URL and execute until we have a respone
            WWW www = new WWW(serverUrl + verificationFileName, form);
            yield return www;

            //check for URL errors
            if (www.error == null)
            {
                //we have a successful response from our server,
                //but it returned false (fake purchase)
                if (!bool.Parse(www.text))
                {
                    if (debug) Debug.Log("The receipt for '" + id + "' could not be verified: " + www.text);
                    //PurchaseFailed("The receipt for '" + id + "' could not be verified.");

                    id = GetIAPIdentifier(id);
                    //remove purchase from the database and update item state
                    if (DBManager.isPurchased(id))
                    {
                        IAPItem item = null;
                        if (ShopManager.GetInstance())
                            item = ShopManager.GetIAPItem(id);
                        if (item) item.Purchased(false);
                        DBManager.RemovePurchased(id);
                    }
                    yield break;
                }
                else
                {
                    //successful response, verified transaction
                    if (debug) Debug.Log(dic["pid"] + " verification success.");
                }
            }
            else
            {
                //we can't reach our external server, do nothing:
                //in this case we handle the purchase as without verification 
                if (debug) Debug.Log("Verification URL error: " + www.text);
            }

            id = GetIAPIdentifier(id);
            PurchaseVerified(id);
        }


        // set product to purchased after successful verification (or without)
        // Consumable IAPs must be consumed
        // For non consumable IAPs or subscriptions, alter database entry
        private void PurchaseVerified(string id)
        {
            if (!IAPObjects.ContainsKey(id)) return;
            IAPObject obj = IAPObjects[id];

            #if UNITY_ANDROID
            if (obj.type == IAPType.consumable)
            {
                GoogleIAB.consumeProduct(obj.GetIdentifier()); //local id
                return;
            }
            #endif

            //don't continue if the product is already purchased,
            //for example if we just want to verify an existing product again
            if (DBManager.isPurchased(id)) return;

            if (obj.type == IAPType.nonConsumable || obj.type == IAPType.subscription)
            {
                DBManager.SetToPurchased(id);
            }

            purchaseSucceededEvent(id);
        }


        /// <summary>
        /// purchase consumable virtual product based on its product id
        /// </summary>
        public static void PurchaseConsumableVirtualProduct(string productId)
        {
            //check whether the player has enough funds
            bool didSucceed = DBManager.VerifyVirtualPurchase(GetIAPObject(productId));
            if (instance.debug) Debug.Log("purchasing consumable virtual product " + productId + " result: " + didSucceed);
            //on success, save new substracted fund value to the database
            //and fire the succeeded event
            if (didSucceed)
            {
                DBManager.Save();
                purchaseSucceededEvent(productId);
            }
            //otherwise show fail message
            else
                PurchaseFailed("Insufficient funds.");
        }


        /// <summary>
        /// purchase non-consumable virtual product based on its product id
        /// </summary>
        public static void PurchaseNonconsumableVirtualProduct(string productId)
        {
            //if already owned, do nothing
            if (DBManager.isPurchased(productId))
            {
                PurchaseFailed("Product already purchased.");
                return;
            }

            //check whether the player has enough funds
            bool didSucceed = DBManager.VerifyVirtualPurchase(GetIAPObject(productId));
            if (instance.debug) Debug.Log("purchasing non-consumable virtual product " + productId + " result: " + didSucceed);
            //on success, non-consumables are being saved to the database
            //this automatically saves the new substracted fund value to the database
            //and fire the succeeded event
            if (didSucceed)
            {
                DBManager.SetToPurchased(productId);
                purchaseSucceededEvent(productId);
            }
            //otherwise show fail message
            else
                PurchaseFailed("Insufficient funds.");
        }


        /// <summary>
        /// restore already purchased user's transactions for non consumable iaps.
        /// For Android we use the received list for detecting previous purchases
        /// </summary>
        public static void RestoreTransactions()
        {
            #if UNITY_IPHONE
                StoreKitBinding.restoreCompletedTransactions();
            #elif UNITY_ANDROID
            if (prods == null)
            {
                RestoreFailed("Restoring transactions failed. Please try again.");
                return;
            }

            for (int i = 0; i < prods.Count; i++)
            {
                string id = GetIAPIdentifier(prods[i].productId);
                if (!DBManager.isPurchased(id))
                    DBManager.SetToPurchased(id);
            }
            RestoreSucceeded();
            #endif

            //update ShopManager GUI items
            if (ShopManager.GetInstance())
                ShopManager.SetItemState();
        }

        #if UNITY_IPHONE
        private List<StoreKitTransaction> FilterTransactions(List<StoreKitTransaction> list)
        {
            Dictionary<string, StoreKitTransaction> transactions = new Dictionary<string, StoreKitTransaction>();
            for (int i = 0; i < list.Count; i++)
            {
                string key = list[i].productIdentifier;
                if (transactions.ContainsKey(key))
                    transactions[key] = list[i];
                else
                    transactions.Add(key, list[i]);
            }

            list.Clear();
            foreach (string key in transactions.Keys)
                list.Add(transactions[key]);

            return list;
        }
        #endif
        #if UNITY_ANDROID
        // tries to consume all previous purchases stored on Google Servers.
        // Do not use for production versions! this is for testing only
        private void DebugConsumeProducts()
        {
            Debug.Log("Attempting to consume all purchases.");
            for (int i = 0; i < prods.Count; i++)
                GoogleIAB.consumeProduct(prods[i].productId);
        }
        #endif
        #if UNITY_ANDROID || UNITY_IPHONE
        // check for purchases on online servers at billing initialization.
        // If a purchase is not registered, set local purchase state back to false
        private void VerifyReceipts()
        {
            //get list of old purchases: on iOS the saved and filtered transactions (avoiding duplicates),
            //on Android we use the old purchases list received from Google
            #if UNITY_IPHONE
                List<StoreKitTransaction> prods = FilterTransactions(StoreKitBinding.getAllSavedTransactions());
            #endif
            if (prods == null || prods.Count == 0) return;

            //loop over all IAP items to check if a valid receipt exists
            for (int i = 0; i < ids.Length; i++)
            {
                //cache IAP id,
                //only verify purchased items
                string localId = ids[i];
                string globalId = GetIAPIdentifier(localId);
                if (DBManager.isPurchased(globalId))
                {
                    //initialize item as faked and loop over receipts
                    bool faked = true;
                    for (int j = 0; j < prods.Count; j++)
                    {
                        //find corresponding transaction class
                        string identifier = "";
                        #if UNITY_IPHONE
                            StoreKitTransaction purchase = prods[j];
                            identifier = purchase.productIdentifier;
                        #elif UNITY_ANDROID
                            GooglePurchase purchase = prods[j];
                            identifier = purchase.productId;
                        #endif
                        if (identifier == localId)
                        {
                            //we found a receipt for this product on the device,
                            //unset fake purchase and let our external
                            //server decide what happens with this transaction
                            faked = false;
                            MakeRequest(purchase);
                            break;
                        }
                    }
                    //we haven't found a receipt for this item, yet it is
                    //set to purchased. This can't be, maybe our external server
                    //response or the database has been hacked with fake data 
                    if (faked)
                    {
                        IAPItem item = null;
                        if (ShopManager.GetInstance())
                            item = ShopManager.GetIAPItem(globalId);
                        if (item) item.Purchased(false);
                        DBManager.RemovePurchased(globalId);
                    }
                }
            }
        }
        #endif

        /// <summary>
        /// unbind from Google billing service.
        /// Good practice, but isn't really necessary
        /// </summary>
        public static void Terminate()
        {
            #if UNITY_ANDROID
                GoogleIAB.unbindService();
            #endif
        }


        //initiates the download process of your remotely hosted
        //config file for virtual products. Differs between types:
        //cached: stores config on the device, changes on next bootup
        //overwrite: only preserves changes in the current session
        private IEnumerator RemoteDownload()
        {
            //build file url
            string url = serverUrl + remoteFileName;

            switch (remoteType)
            {
                case RemoteType.cached:
                    //load cached file string and overwrite virtual IAPs
                    DBManager.LoadRemoteConfig();
                    //download new config
                    yield return StartCoroutine(Download(url));
                    //save downloaded file
                    DBManager.SaveRemoteConfig(request.text);
                    break;
                case RemoteType.overwrite:
                    //download new config 
                    yield return StartCoroutine(Download(url));
                    //parse string and overwrite virtual IAPs
                    DBManager.ConvertToIAPs(request.text);
                    break;
            }
        }


        //downloads the remotely hosted config file
        private IEnumerator Download(string url)
        {
            request = new WWW(url);
            yield return request;

            if (!string.IsNullOrEmpty(request.error))
                Debug.Log("Failed remote config download with error: " + request.error);
            else if (debug)
                Debug.Log("Downloaded remotely hosted config file: \n" + request.text);
        }


        // method that fires a product request error
        private static void BillingNotSupported(string error)
        {
            if (instance.debug) Debug.Log("IAPManager reports: BillingNotSupported. Error: " + error);
            if (inventoryRequestFailedEvent != null)
                inventoryRequestFailedEvent(error);
        }


        // method that fires a purchase error
        private static void PurchaseFailed(string error)
        {
            if (instance.debug) Debug.Log("IAPManager reports: PurchaseFailed. Error: " + error);
            if (purchaseFailedEvent != null)
                purchaseFailedEvent(error);
        }

		
		// Prime31 API overload
        private static void PurchaseFailed(string error, int code)
        {   PurchaseFailed(error + ", " + code);    }
		

        // method that fires the restore succeed action 
        private static void RestoreSucceeded()
        {
            purchaseSucceededEvent("restore");
        }


        // method that fires a restore error
        // through the purchase failed event
        private static void RestoreFailed(string error)
        {
            if (instance.debug) Debug.Log("IAPManager reports: RestoreFailed. Error: " + error);
            if (purchaseFailedEvent != null)
                purchaseFailedEvent(error);
        }

		
		/// <summary>
        /// returns a list of all upgrade ids associated to a product
        /// </summary>
        public static List<string> GetIAPUpgrades(string productId)
        {
            List<string> list = new List<string>();
            IAPObject obj = GetIAPObject(productId);
            
            while (!string.IsNullOrEmpty(obj.req.nextId))
            {
                list.Add(obj.req.nextId);
                obj = GetIAPObject(obj.req.nextId);
            }
           
            return list;
        }


        /// <summary>
        /// returns the last purchased upgrade id of a product,
        /// or the main product itself if it hasn't been purchased yet
        /// </summary>
        public static string GetCurrentUpgrade(string productId)
        {
            if (!DBManager.isPurchased(productId))
                return productId;

            string id = productId;
            List<string> upgrades = GetIAPUpgrades(productId);

            for (int i = upgrades.Count - 1; i >= 0; i--)
            {
                if (DBManager.isPurchased(upgrades[i]))
                {
                    id = upgrades[i];
                    break;
                }
            }

            return id;
        }


        /// <summary>
        /// returns the next unpurchased upgrade id of a product
        /// </summary>
        public static string GetNextUpgrade(string productId)
        {
            string id = GetCurrentUpgrade(productId);
            IAPObject obj = GetIAPObject(id);

            if (!DBManager.isPurchased(id) || string.IsNullOrEmpty(obj.req.nextId)) return id;
            else return obj.req.nextId;
        }
		

        /// <summary>
        /// returns the global identifier of an in-app product,
        /// specified in the IAP Settings editor
        /// </summary>
        public static string GetIAPIdentifier(string id)
        {
            foreach (IAPObject obj in instance.IAPObjects.Values)
            {
                if (obj.type == IAPType.consumableVirtual ||
                    obj.type == IAPType.nonConsumableVirtual)
                    continue;

                if (obj.GetIdentifier() == id)
                    return obj.id;
            }

            return id;
        }


        /// <summary>
        /// returns the list of currencies
        /// defined in the IAP Settings editor
        /// </summary>
        public static List<IAPCurrency> GetCurrency()
        {
            return instance.currency;
        }


        /// <summary>
        /// returns the list of IAPGroups
        /// defined in the IAP Settings editor
        /// </summary>
        public static List<IAPGroup> GetIAPs()
        {
            List<IAPGroup> list = new List<IAPGroup>();
            list.AddRange(instance.IAPs);
            list.AddRange(instance.IGCs);
            return list;
        }


        /// <summary>
        /// returns a string array of all IAP ids.
        /// Used by DBManager
        /// </summary>
        public static string[] GetIAPKeys()
        {
            string[] ids = new string[instance.IAPObjects.Count];
            instance.IAPObjects.Keys.CopyTo(ids, 0);
            return ids;
        }


        /// <summary>
        /// returns the IAPObject with a specific id
        /// </summary>
        public static IAPObject GetIAPObject(string id)
        {
            if (!instance || !instance.IAPObjects.ContainsKey(id))
                return null;
            return instance.IAPObjects[id];
        }


        /// <summary>
        /// returns the group name of a specific product id.
        /// Used by DBManager, depends on platform
        /// <summary>
        public static string GetIAPObjectGroupName(string id)
        {
            if (instance.IAPObjects.ContainsKey(id))
            {
                //cache object and create temporary group list
                IAPObject obj = GetIAPObject(id);
                List<IAPGroup> groups = GetIAPs();
                //loop over groups to find the product id,
                //then return the name of the group
                for (int i = 0; i < groups.Count; i++)
                    if (groups[i].items.Contains(obj))
                        return groups[i].name;
            }
            //if the corresponding group has not been found
            return null;
        }
    }


    /// <summary>
    /// supported billing platforms
    /// </summary>
    public enum IAPPlatform
    {
        GooglePlay = 0,
        iOSAppStore = 1
        //WindowsPhone8 = 2
    }


    /// <summary>
    /// supported IAP types enum
    /// </summary>
    public enum IAPType
    {
        consumable,
        nonConsumable,
        subscription,
        consumableVirtual,
        nonConsumableVirtual
    }


    /// <summary>
    /// online IAP verification on app launch or purchase
    /// </summary>
    public enum VerificationType
    {
        none,
        onStart,
        onPurchase,
        both
    }


    /// <summary>
    /// remotely hosted config type
    /// </summary>
    public enum RemoteType
    {
        none,
        cached,
        overwrite
    }


    /// <summary>
    /// IAP group properties.
    /// Each group holds a list of IAP objects
    /// </summary>
    [System.Serializable]
    public class IAPGroup
    {
        public string id;
        public string name;
        public List<IAPObject> items = new List<IAPObject>();
    }


    /// <summary>
    /// IAP object properties.
    /// This is a meta-class for an IAP item
    /// </summary>
    [System.Serializable]
    public class IAPObject
    {
        public string id;
        public List<IAPIdentifier> localId = new List<IAPIdentifier>();
        public bool fetch = false;
        public IAPType type = IAPType.consumable;
        public string title;
        public string description;
        public string realPrice;
        public Sprite icon;
        public List<IAPCurrency> virtualPrice = new List<IAPCurrency>();
        public IAPRequirement req = new IAPRequirement();

        public bool platformFoldout = false;

        public string GetIdentifier()
        {
            string local = null;

            switch (Application.platform)
            {
                case RuntimePlatform.Android:
                    local = localId[(int)IAPPlatform.GooglePlay].GetIdentifier();
                    break;
                case RuntimePlatform.IPhonePlayer:
                    local = localId[(int)IAPPlatform.iOSAppStore].GetIdentifier();
                    break;
            }

            if (!string.IsNullOrEmpty(local)) return local;
            else return id;
        }
    }


    /// <summary>
    /// Local identifier for in-app products,
    /// per store platform
    /// </summary>
    [System.Serializable]
    public class IAPIdentifier
    {
        public bool overridden = false;
        public string id;

        public string GetIdentifier()
        {
            if (overridden) return id;
            else return null;
        }
    }


    /// <summary>
    /// IAP currency, stored in the database
    /// </summary>
    [System.Serializable]
    public class IAPCurrency
    {
        public string name;
        public int amount;
    }


    /// <summary>
    /// IAP unlock requirement, stored in the database
    /// </summary>
    [System.Serializable]
    public class IAPRequirement
    {
        //database entry id
        public string entry;
        //goal/value to reach
        public int target;
        //label text that describes the requirement
        public string labelText;
		
		//next upgrade product
        public string nextId;
    }
}