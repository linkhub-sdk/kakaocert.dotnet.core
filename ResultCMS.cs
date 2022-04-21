using System;
using System.Runtime.Serialization;

namespace Kakaocert
{
    [DataContract]
    public class ResultCMS
    {
        [DataMember] public String receiptID;
        [DataMember] public String regDT;
        [DataMember] public int? state;
        [DataMember] public int? expires_in;
        [DataMember] public String callCenterNum;
        [DataMember] public bool? allowSimpleRegistYN;

        [DataMember] public bool? verifyNameYN;
        [DataMember] public String payload;
        [DataMember] public String requestDT;
        [DataMember] public String expireDT;
        [DataMember] public String clientCode;
        [DataMember] public String clientName;
        [DataMember] public String tmstitle;
        [DataMember] public String tmsmessage;
        [DataMember] public String subClientName;
        [DataMember] public String subClientCode;
        [DataMember] public String viewDT;
        [DataMember] public String completeDT;
        [DataMember] public String verifyDT;
        [DataMember] public bool? appUseYN;

    }
}
