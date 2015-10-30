/*  This file is part of the "Simple IAP System" project by Rebound Games.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using System.Collections;
using Prime31;

namespace SIS
{
    /// <summary>
    /// Prime31 cross-platform IAP product wrapper class.
    /// Stores properties of either StoreKitProduct or GoogleSkuInfo
    /// </summary>
    public class IAPArticle
    {
        /// <summary>
        /// product id
        /// </summary>
        public string id;

        /// <summary>
        /// product title
        /// </summary>
        public string title;

        /// <summary>
        /// product description
        /// </summary>
        public string description;

        /// <summary>
        /// product price
        /// </summary>
        public string price;


        #if UNITY_IPHONE
        /// <summary>
        /// create new instance based on OS. iOS version
        /// </summary>
	    public IAPArticle(StoreKitProduct prod)
	    {
		    id = prod.productIdentifier;
		    title = prod.title;
            description = prod.description;
		    price = prod.price;
	    }
        #endif
        #if UNITY_ANDROID
        /// <summary>
        /// create new instance based on OS. Android version
        /// </summary>
        public IAPArticle(GoogleSkuInfo prod)
        {
            id = prod.productId;
            title = prod.title;
            description = prod.description;
            price = prod.price;
        }
        #endif
    }
}