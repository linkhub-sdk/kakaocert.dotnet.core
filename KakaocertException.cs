using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Kakaocert
{
    public class KakaocertException : Exception
    {
        public KakaocertException()
            : base()
        {
        }
        public KakaocertException(long code, String Message)
            : base(Message)
        {
            this._code = code;
        }

        private long _code;

        public long code
        {
            get { return _code; }
        }
        public KakaocertException(Linkhub.LinkhubException le)
            : base(le.Message, le)
        {
            this._code = le.code;
        }
    }
}
