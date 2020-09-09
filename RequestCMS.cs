using System;
using System.Runtime.Serialization;

namespace Kakaocert
{
    [DataContract]
    public class RequestCMS
    {
        [DataMember]
        public String CallCenterNum;
        [DataMember]
        public int? Expires_in;
        [DataMember]
        public String PayLoad;
        [DataMember]
        public String ReceiverBirthDay;
        [DataMember]
        public String ReceiverHP;
        [DataMember]
        public String ReceiverName;
        [DataMember]
        public String SubClientID;
        [DataMember]
        public String TMSMessage;
        [DataMember]
        public String TMSTitle;
        [DataMember]
        public String Token;
        [DataMember]
        public bool? isAllowSimpleRegistYN;
        [DataMember]
        public bool? isVerifyNameYN;


        [DataMember]
        public String BankAccountName;
        [DataMember]
        public String BankAccountNum;
        [DataMember]
        public String BankCode;
        [DataMember]
        public String ClientUserID;

    }
}
