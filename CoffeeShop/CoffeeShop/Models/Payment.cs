using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Models
{
    public class Payment
    {
        private IConfiguration _configuration;
        public Payment(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// creating merchant info change
        /// </summary> 
        /// <returns> TBD </returns>
        public string Run()
        {
            // declare that we are using the Sandbox account. 

            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX;

            // set merchant info
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType()
            {
                //or actual authnet hard coded data
                name = _configuration["AuthNetAPILogin"],
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _configuration["AuthNetTransactionKey"]
            };

            // Create CC.

            var creditCard = new creditCardType
            {
                cardNumber = "4111111111111111",
                expirationDate = "1120"
            };

            // make call out to the external method 
            customerAddressType billingAddress = GetAddress();

            // only payment type we are accepting is CC
            paymentType paymentType = new paymentType { Item = creditCard };

            // consolidates all of the information about transaction before sending it to AUth.NET. 
            // info to be on transaction request

            transactionRequestType transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                amount = 1.0m,
                payment = paymentType,
                billTo = billingAddress,
                //lineItems = 
            };

            createTransactionRequest request = new createTransactionRequest
            {
                transactionRequest = transactionRequest
            };

            // make call to AUth.NET with the requset that was created
            var controller = new createTransactionController(request);
            // execute the call
            controller.Execute();

            //reseponse call we made above
            var response = controller.GetApiResponse();

            if (response != null)
            {
                if (response.messages.resultCode == messageTypeEnum.Ok)
                {
                    if (response.transactionResponse != null)
                    {
                        Console.WriteLine("Success");
                    }
                }
                else
                {
                    if (response.transactionResponse != null)
                    {
                        Console.WriteLine("Transaction Error");
                    }
                }
            }

            return "Error";
        }

        /// <summary>
        /// address of user
        /// bring in user as parameter
        /// bring in user id? ...maybe...
        /// </summary>
        /// <returns> address</returns>
        private customerAddressType GetAddress()
        {

            customerAddressType address = new customerAddressType()
            {

                firstName = "Donkey",
                lastName = "Kong",
                address = "123 Donkey lane",
                city = "New York",
                zip = "08069"
            };
            return address;
        }

        /// <summary>
        /// gets line item list and the count of the items.
        /// </summary>
        /// <param name="products"></param>
        /// <returns> line item </returns>
        private lineItemType[] GetLineItemTypes(List<Coffee> products)
        {
            lineItemType[] lineitems = new lineItemType[products.Count];
            int count = 0;

            foreach (var prod in products)
            {
                lineitems[count] = new lineItemType
                {
                    itemId = "1",
                    name = " product name",
                    quantity = 11,
                    unitPrice = new Decimal(5.00)
                };
                count++;
            }
            return lineitems;
        }
    }

}

