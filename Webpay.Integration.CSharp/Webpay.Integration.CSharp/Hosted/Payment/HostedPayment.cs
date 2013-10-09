﻿using System.Collections.Generic;
using System.Xml;
using Webpay.Integration.CSharp.Exception;
using Webpay.Integration.CSharp.Hosted.Helper;
using Webpay.Integration.CSharp.Order.Create;
using Webpay.Integration.CSharp.Order.Validator;
using Webpay.Integration.CSharp.Util.Constant;

namespace Webpay.Integration.CSharp.Hosted.Payment
{
    /// <summary>
    /// Description of HostedPayment: Parent to CardPayment, DirectPayment, PayPagePayment 
    /// and PaymentMethodPayment classes. Prepares an order and creates a payment form
    /// to integrate on web page. Uses XmlBuilder to turn formatted order into xml format.
    /// </summary>
    public abstract class HostedPayment
    {
        protected CreateOrderBuilder CrOrderBuilder;
        protected List<HostedOrderRowBuilder> HrowBuilder;
        protected List<string> ExcludedPaymentMethod;
        protected long Amount;
        protected long Vat;
        protected string ReturnUrl;
        protected string CancelUrl;
        protected ExcludePayments Excluded;
        protected string LanguageCode = Util.Constant.LanguageCode.en.ToString();

        public HostedPayment(CreateOrderBuilder createOrderBuilder)
        {
            CrOrderBuilder = createOrderBuilder;
            HrowBuilder = new List<HostedOrderRowBuilder>();
            Excluded = new ExcludePayments();
            ExcludedPaymentMethod = new List<string>();
            ReturnUrl = "";
        }

        public CreateOrderBuilder GetCreateOrderBuilder()
        {
            return CrOrderBuilder;
        }

        public List<HostedOrderRowBuilder> GetRowBuilder()
        {
            return HrowBuilder;
        }

        public List<string> GetExcludedPaymentMethod()
        {
            return ExcludedPaymentMethod;
        }

        public long GetAmount()
        {
            return Amount;
        }

        public long GetVat()
        {
            return Vat;
        }

        public string GetReturnUrl()
        {
            return ReturnUrl;
        }

        public string GetCancelUrl()
        {
            return CancelUrl;
        }

        public string GetPayPageLanguageCode()
        {
            return LanguageCode;
        }

        /// <summary>
        /// ValidateOrder
        /// </summary>
        /// <returns>Error message compilation string</returns>
        public string ValidateOrder()
        {
            var errors = "";
            if (ReturnUrl.Equals(""))
                errors += "MISSING VALUE - Return url is required, SetReturnUrl(...).\n";

            var validator = new HostedOrderValidator();

            //Check if payment method is EU country, PaymentMethod: INVOICE or PAYMENTPLAN    
            var payment = this as PaymentMethodPayment;
            if (payment != null)
            {
                if (payment.GetPaymentMethod().Equals(PaymentMethod.INVOICE) ||
                    payment.GetPaymentMethod().Equals(PaymentMethod.PAYMENTPLAN))
                {
                    if (CrOrderBuilder.GetCountryCode().Equals(CountryCode.NL))
                    {
                        errors += new IdentityValidator().ValidateNlIdentity(CrOrderBuilder);
                    }
                    else if (CrOrderBuilder.GetCountryCode().Equals(CountryCode.DE))
                    {
                        errors += new IdentityValidator().ValidateDeIdentity(CrOrderBuilder);
                    }
                }
            }

            errors += validator.Validate(CrOrderBuilder);

            return errors;
        }

        /// <summary>
        /// CalculateRequestValues
        /// </summary>
        /// <exception cref="SveaWebPayValidationException"></exception>
        public abstract void CalculateRequestValues();

        protected void FormatRequestValues()
        {
            var errors = ValidateOrder();
            if (errors.Length > 0)
                throw new SveaWebPayValidationException(errors);

            var formatter = new HostedRowFormatter<CreateOrderBuilder>();

            HrowBuilder = formatter.FormatRows(CrOrderBuilder);
            Amount = formatter.FormatTotalAmount(HrowBuilder);
            Vat = formatter.FormatTotalVat(HrowBuilder);
        }

        /// <summary>
        /// GetPaymentForm
        /// </summary>
        /// <returns>PaymentForm</returns>
        public PaymentForm GetPaymentForm()
        {
            CalculateRequestValues();
            var xmlBuilder = new HostedXmlBuilder();
            string xml = xmlBuilder.GetXml(this);

            var form = new PaymentForm();
            form.SetXmlMessage(xml);

            form.SetMerchantId(CrOrderBuilder.GetConfig()
                                             .GetMerchantId(PaymentType.HOSTED, CrOrderBuilder.GetCountryCode()));
            form.SetSecretWord(CrOrderBuilder.GetConfig().GetSecret(PaymentType.HOSTED, CrOrderBuilder.GetCountryCode()));

            form.SetSubmitMessage(CrOrderBuilder.GetCountryCode() != 0
                                      ? CrOrderBuilder.GetCountryCode()
                                      : CountryCode.SE);

            form.SetPayPageUrl(CrOrderBuilder.GetConfig().GetEndPoint(PaymentType.HOSTED));

            form.SetForm();
            form.SetHtmlFields();

            return form;
        }

        /// <summary>
        /// GetPaymentSpecificXml
        /// </summary>
        /// <param name="xmlw"></param>
        /// <returns>XMLStreamWriter</returns>
        public abstract XmlWriter GetPaymentSpecificXml(XmlWriter xmlw);

        /// <summary>
        /// WriteSimpleElement
        /// </summary>
        /// <param name="xmlw"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected void WriteSimpleElement(XmlWriter xmlw, string name, string value)
        {
            if (value != null)
            {
                xmlw.WriteStartElement(name);
                xmlw.WriteChars(value.ToCharArray(), 0, value.ToCharArray().Length);
                xmlw.WriteEndElement();
            }
        }
    }
}