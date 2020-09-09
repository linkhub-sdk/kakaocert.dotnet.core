using System;
using System.Runtime.Serialization;

namespace Kakaocert
{
    [DataContract]
    public class ResponseVerify
    {
        [DataMember]
        public String receiptId;
        [DataMember]
        public String signedData;
    }

}
