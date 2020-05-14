using System;
using System.Text;
using System.Runtime.Serialization;

namespace Kakaocert
{
    [DataContract]
    public class Response
    {
        [DataMember]
        public long code;
        [DataMember]
        public String message;
    }
}
