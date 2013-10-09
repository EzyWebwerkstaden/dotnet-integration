﻿using System.Xml;
using Webpay.Integration.CSharp.Exception;
using Webpay.Integration.CSharp.Order.Create;
using Webpay.Integration.CSharp.Util.Constant;

namespace Webpay.Integration.CSharp.Hosted.Payment
{
    /// <summary>
    /// Defines one payment method. Directs directly to method without going through PayPage.
    /// </summary>
    public class PaymentMethodPayment : HostedPayment
    {
        private PaymentMethod _paymentMethod;
        
        public PaymentMethodPayment(CreateOrderBuilder createOrderBuilder, PaymentMethod paymentMethod) : base(createOrderBuilder)
        {
            _paymentMethod = paymentMethod;
        }

    
        public PaymentMethod GetPaymentMethod() {
    	    return _paymentMethod;
        }

        /// <summary>
        /// Only used in CardPayment and DirectPayment
        /// </summary>
        public PaymentMethodPayment SetReturnUrl(string returnUrl)
        {
            ReturnUrl = returnUrl;
            return this;
        }

        public PaymentMethodPayment SetCancelUrl(string returnUrl)
        {
            CancelUrl = returnUrl;
            return this;
        }

        public PaymentMethodPayment SetPayPageLanguageCode(LanguageCode languageCode)
        {
            LanguageCode = languageCode.ToString().ToLower();
            return this;
        }

        public PaymentMethodPayment ConfigureExcludedPaymentMethod() 
        {
            return this;
        }

        /// <summary>
        /// CalculateRequestValues
        /// </summary>
        /// <exception cref="SveaWebPayValidationException"></exception>
        public override void CalculateRequestValues()
        {
            FormatRequestValues();
            ConfigureExcludedPaymentMethod();
        }

        public override XmlWriter GetPaymentSpecificXml(XmlWriter xmlw)
        {
            if (_paymentMethod != null) 
            {
                WriteSimpleElement(xmlw, "paymentmethod", _paymentMethod.Value);
            }
        
            return xmlw;
        }
    }
}