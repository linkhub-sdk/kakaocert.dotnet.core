using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Net;
using Linkhub;
using System.Security.Cryptography;


namespace Kakaocert
{

    public class KakaocertService
    {
        private const string ServiceID = "KAKAOCERT";
        private const String ServiceURL_Default = "https://kakaocert-api.linkhub.co.kr";
        private const String ServiceURL_Static = "https://static-kakaocert-api.linkhub.co.kr";
        private const String ServiceURL_GA = "https://ga-kakaocert-api.linkhub.co.kr";

        private const String APIVersion = "2.0";
        private const String CRLF = "\r\n";

        private Dictionary<String, Token> _tokenTable = new Dictionary<String, Token>();
        private bool _IPRestrictOnOff;
        private bool _UseStaticIP;
        private bool _UseGAIP;
        private bool _UseLocalTimeYN = true;
        private String _LinkID;
        private String _SecretKey;
        private Authority _LinkhubAuth;
        private List<String> _Scopes = new List<string>();


        public bool IPRestrictOnOff
        {
            set { _IPRestrictOnOff = value; }
            get { return _IPRestrictOnOff; }
        }

        public bool UseStaticIP
        {
            set { _UseStaticIP = value; }
            get { return _UseStaticIP; }
        }

        public bool UseGAIP
        {
            set { _UseGAIP = value; }
            get { return _UseGAIP; }
        }

        public bool UseLocalTimeYN
        {
            set { _UseLocalTimeYN = value; }
            get { return _UseLocalTimeYN; }
        }

        public KakaocertService(String LinkID, String SecretKey)
        {
            _LinkhubAuth = new Authority(LinkID, SecretKey);
            _Scopes.Add("member");
            _Scopes.Add("310");
            _Scopes.Add("320");
            _Scopes.Add("330");
            _LinkID = LinkID;
            _SecretKey = SecretKey;
            _IPRestrictOnOff = true;
        }

        protected String toJsonString(Object graph)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                DataContractJsonSerializer ser = new DataContractJsonSerializer(graph.GetType());
                ser.WriteObject(ms, graph);
                ms.Seek(0, SeekOrigin.Begin);
                return new StreamReader(ms).ReadToEnd();
            }
        }
        protected T fromJson<T>(Stream jsonStream)
        {
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(T));
            return (T)ser.ReadObject(jsonStream);
        }

        protected string ServiceURL
        {
            get
            {
                if (UseGAIP)
                {
                    return ServiceURL_GA;
                }
                else if (UseStaticIP)
                {
                    return ServiceURL_Static;
                }
                else
                {
                    return ServiceURL_Default;
                }

            }
        }

        private String getSession_Token(String CorpNum)
        {
            Token _token = null;

            if (_tokenTable.ContainsKey(CorpNum))
            {
                _token = _tokenTable[CorpNum];
            }

            bool expired = true;
            if (_token != null)
            {
                DateTime now = DateTime.Parse(_LinkhubAuth.getTime(UseStaticIP, UseLocalTimeYN, UseGAIP));

                DateTime expiration = DateTime.Parse(_token.expiration);

                expired = expiration < now;

            }

            if (expired)
            {
                try
                {
                    if (_IPRestrictOnOff) // IPRestrictOnOff 사용시
                    {
                        _token = _LinkhubAuth.getToken(ServiceID, CorpNum, _Scopes, null, UseStaticIP, UseLocalTimeYN, UseGAIP);
                    }
                    else
                    {
                        _token = _LinkhubAuth.getToken(ServiceID, CorpNum, _Scopes, "*", UseStaticIP, UseLocalTimeYN, UseGAIP);
                    }


                    if (_tokenTable.ContainsKey(CorpNum))
                    {
                        _tokenTable.Remove(CorpNum);
                    }
                    _tokenTable.Add(CorpNum, _token);
                }
                catch (LinkhubException le)
                {
                    throw new KakaocertException(le);
                }
            }

            return _token.session_token;
        }

        protected T httpget<T>(String url, String CorpNum, String UserID)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceURL + url);

            if (String.IsNullOrEmpty(CorpNum) == false)
            {
                String bearerToken = getSession_Token(CorpNum);
                request.Headers.Add("Authorization", "Bearer" + " " + bearerToken);
            }

            request.Headers.Add("x-lh-version", APIVersion);

            request.Headers.Add("Accept-Encoding", "gzip, deflate");

            request.AutomaticDecompression = DecompressionMethods.GZip;

            if (String.IsNullOrEmpty(UserID) == false)
            {
                request.Headers.Add("x-pb-userid", UserID);
            }

            request.Method = "GET";

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stReadData = response.GetResponseStream())
                    {
                        return fromJson<T>(stReadData);
                    }
                }
            }
            catch (Exception we)
            {
                if (we is WebException && ((WebException)we).Response != null)
                {
                    using (Stream stReadData = ((WebException)we).Response.GetResponseStream())
                    {
                        Response t = fromJson<Response>(stReadData);
                        throw new KakaocertException(t.code, t.message);
                    }
                }
                throw new KakaocertException(-99999999, we.Message);
            }

        }

        protected T httppost<T>(String url, String CorpNum, String UserID, String PostData, String httpMethod)
        {
            return httppost<T>(url, CorpNum, UserID, PostData, httpMethod, null);
        }

        protected T httppost<T>(String url, String CorpNum, String UserID, String PostData, String httpMethod, String contentsType)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(ServiceURL + url);

            if (contentsType == null)
            {
                request.ContentType = "application/json;";
            }
            else
            {
                request.ContentType = contentsType;
            }


            if (String.IsNullOrEmpty(CorpNum) == false)
            {
                String bearerToken = getSession_Token(CorpNum);
                request.Headers.Add("Authorization", "Bearer" + " " + bearerToken);
            }

            request.Headers.Add("x-lh-version", APIVersion);

            request.Headers.Add("Accept-Encoding", "gzip, deflate");

            request.AutomaticDecompression = DecompressionMethods.GZip;

            if (String.IsNullOrEmpty(UserID) == false)
            {
                request.Headers.Add("x-pb-userid", UserID);
            }

            if (String.IsNullOrEmpty(httpMethod) == false)
            {
                request.Headers.Add("X-HTTP-Method-Override", httpMethod);
            }

            request.Method = "POST";

            String xDate = _LinkhubAuth.getTime(UseStaticIP, false, UseGAIP);

            request.Headers.Add("x-lh-date", xDate);

            if (String.IsNullOrEmpty(PostData)) PostData = "";

            byte[] btPostDAta = Encoding.UTF8.GetBytes(PostData);

            String HMAC_target = "POST\n";
            HMAC_target += Convert.ToBase64String(SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(PostData))) + "\n";
            HMAC_target += xDate + "\n";
            HMAC_target += APIVersion + "\n";
            HMACSHA256 hmac = new HMACSHA256(Convert.FromBase64String(_SecretKey));

            String hmac_str = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(HMAC_target)));

            request.Headers.Add("x-kc-auth", _LinkID + " " + hmac_str);

            request.ContentLength = btPostDAta.Length;

            request.GetRequestStream().Write(btPostDAta, 0, btPostDAta.Length);

            try
            {
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    using (Stream stReadData = response.GetResponseStream())
                    {
                        return fromJson<T>(stReadData);
                    }
                }
            }
            catch (Exception we)
            {
                if (we is WebException && ((WebException)we).Response != null)
                {
                    using (Stream stReadData = ((WebException)we).Response.GetResponseStream())
                    {
                        Response t = fromJson<Response>(stReadData);
                        throw new KakaocertException(t.code, t.message);
                    }
                }
                throw new KakaocertException(-99999999, we.Message);
            }
        }

        public String requestCMS(String ClientCode, RequestCMS requestObj)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (requestObj == null) throw new KakaocertException(-99999999, "자동이체 출금동의 요청정보가 입력되지 않았습니다.");


            String PostData = toJsonString(requestObj);

            ReceiptIDResponse response = httppost<ReceiptIDResponse>("/SignDirectDebit/Request", ClientCode, "", PostData, ""); ;

            return response.receiptId;
        }

        public String requestVerifyAuth(String ClientCode, RequestVerifyAuth requestObj)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (requestObj == null) throw new KakaocertException(-99999999, "본인인증 요청정보가 입력되지 않았습니다.");


            String PostData = toJsonString(requestObj);

            ReceiptIDResponse response = httppost<ReceiptIDResponse>("/SignIdentity/Request", ClientCode, "", PostData, ""); ;

            return response.receiptId;
        }

        public ResponseESignRequest requestESign(String ClientCode, RequestESign requestObj, bool isAppUseYN = false)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (requestObj == null) throw new KakaocertException(-99999999, "전자서명 요청정보가 입력되지 않았습니다.");

            requestObj.isAppUseYN = false;

            if (isAppUseYN) requestObj.isAppUseYN = true;

            String PostData = toJsonString(requestObj);

            ResponseESignRequest response = httppost<ResponseESignRequest>("/SignToken/Request", ClientCode, "", PostData, ""); ;

            return response;
        }

        public ResultCMS getCMSState(String ClientCode, String ReceiptId)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (String.IsNullOrEmpty(ReceiptId)) throw new KakaocertException(-99999999, "접수아이디가 입력되지 않았습니다.");

            return httpget<ResultCMS>("/SignDirectDebit/Status/" + ReceiptId, ClientCode, null);
        }

        public ResultVerifyAuth getVerifyAuthState(String ClientCode, String ReceiptId)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (String.IsNullOrEmpty(ReceiptId)) throw new KakaocertException(-99999999, "접수아이디가 입력되지 않았습니다.");

            return httpget<ResultVerifyAuth>("/SignIdentity/Status/" + ReceiptId, ClientCode, null);
        }

        public ResultESign getESignState(String ClientCode, String ReceiptId)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (String.IsNullOrEmpty(ReceiptId)) throw new KakaocertException(-99999999, "접수아이디가 입력되지 않았습니다.");
                        
            return httpget<ResultESign>("/SignToken/Status/" + ReceiptId, ClientCode, null);
        }

        public ResponseVerify verifyCMS(String ClientCode, String ReceiptId)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (String.IsNullOrEmpty(ReceiptId)) throw new KakaocertException(-99999999, "접수아이디가 입력되지 않았습니다.");

            return httpget<ResponseVerify>("/SignDirectDebit/Verify/" + ReceiptId, ClientCode, null);
        }

        public ResponseVerify verifyAuth(String ClientCode, String ReceiptId)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (String.IsNullOrEmpty(ReceiptId)) throw new KakaocertException(-99999999, "접수아이디가 입력되지 않았습니다.");

            return httpget<ResponseVerify>("/SignIdentity/Verify/" + ReceiptId, ClientCode, null);
        }

        public ResponseVerify verifyESign(String ClientCode, String ReceiptId, String Signature = null)
        {
            if (String.IsNullOrEmpty(ClientCode)) throw new KakaocertException(-99999999, "이용기관코드가 입력되지 않았습니다.");
            if (String.IsNullOrEmpty(ReceiptId)) throw new KakaocertException(-99999999, "접수아이디가 입력되지 않았습니다.");

            string uri = "/SignToken/Verify/" + ReceiptId;

            if (false == String.IsNullOrEmpty(Signature))
            {
                uri += "/" + Signature;
            }

            return httpget<ResponseVerify>(uri, ClientCode, null);
        }





        [DataContract]
        public class ReceiptIDResponse
        {
            [DataMember]
            public String receiptId;
        }

        [DataContract]
        public class ResponseESignRequest
        {
            [DataMember]
            public String receiptId;
            [DataMember]
            public String tx_id;
        }

    }
}