using System.Collections.Generic;

namespace KhanoumiPaymentGrpc.Models.Constants.KhanoumiPaymentServiceConstants
{
    public class ConstantsProvider
    {

        public Dictionary<string, string> AuthenticationPairs;
        public const string GrpcKeyForNewCodeBase = "F6D0005F-3286-4992-B0BF-6CB85EE1EFB3";
        public const string GrpcPassword = "P@$sw0rd";

        public ConstantsProvider()
        {
            AuthenticationPairs.Add(GrpcKeyForNewCodeBase, GrpcPassword);
        }






    }
}
