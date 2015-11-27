﻿using System;
using Cdiscount.OpenApi.ProxyClient.Contract.Common;

namespace Cdiscount.OpenApi.ProxyClient.Contract.PushToCart
{
    /// <summary>
    /// PushToCart response message
    /// </summary>
    public class PushToCartResponse : BaseResponseMessage
    {
        /// <summary>
        /// Cart identifier
        /// </summary>
        public Guid CartGuid { get; set; }

        /// <summary>
        /// URL to checkout
        /// </summary>
        public string CheckoutUrl { get; set; }

        /// <summary>
        /// Error Type
        /// </summary>
        public string ErrorType { get; set; }
    }
}
