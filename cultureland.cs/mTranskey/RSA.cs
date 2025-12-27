using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace cultureland.cs.mTranskey
{
    public class RSA
    {
        public const string CULTURELAND_PUBLICKEY = @"-----BEGIN CERTIFICATE-----
MIIDhTCCAm2gAwIBAgIJAO4t+//wr+lZMA0GCSqGSIb3DQEBCwUAMGcxCzAJBgNV
BAYTAktSMR0wGwYDVQQKExRSYW9uU2VjdXJlIENvLiwgTHRkLjEaMBgGA1UECxMR
UXVhbGl0eSBBc3N1cmFuY2UxHTAbBgNVBAMTFFJhb25TZWN1cmUgQ28uLCBMdGQu
MB4XDTIyMTAyNzAyMDI1NFoXDTQyMTAyMjAyMDI1NFowgYAxCzAJBgNVBAYTAkFV
MRMwEQYDVQQIDApTb21lLVN0YXRlMSEwHwYDVQQKDBhJbnRlcm5ldCBXaWRnaXRz
IFB0eSBMdGQxOTA3BgNVBAMMMFQ9UCZEPTE5NzQ4NjQ2Q0Y3NTE0NENEMzc2RUM2
RkI0RkUwMDQ5MEQ5NEYyNjQmaDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoC
ggEBAM4mPj/ZWCZNpRQWvjmOQtiT34VoUeVjWDd/pClqzLFpW3ckU7b7nfUwYzc5
ZI21vc7Fb5tDWNlmNa9kapbC/9q/yWMZB0qpmslElAcSJexD9M4eA9ydC2309Wxd
LCsudDw4NlcN5kqs6C2cNZd1aDkP4ZamfdGbWjDsZqjQQFqdFg7HrYHzPn5m5dpC
k4qmrYyLdDzA+HtKSVT7wceDAwRuUDz7tDDDeidQOm/5rkA/UeMRsH1PAF6SV0Xq
P5xsKtADPkHtl/0k4ikt4zNkM9kvwcIv/tcmRcRDpnmsUsZMEBxnvbo4mjJ239FT
mvnquM75bPVlvrtojafWCCI5CksCAwEAAaMaMBgwCQYDVR0TBAIwADALBgNVHQ8E
BAMCBeAwDQYJKoZIhvcNAQELBQADggEBABXyYfzQK63C5m16/SXxX2BKeUdVXxnE
EyI/9dfReDEsj8yzVQipDSK8FiH05JtLqRpDKnfezXEDCYNMqIs3eRxBG2aO+ZCP
aqSFllio2igSz3ENt7PbneX1qV8lTqnVg5/8qRteztSynKkECfbyV0VJBPw2gpeE
1EheMXOAPu1zvdCYd29pgNlW3vPPDIXHUEZvlOCV8WhTfeE4jjOyVfLsVYSmnqIY
c1ptdCPILwf0cp0s8feOAgeUN1VJ1TvoEXw4CZz7MSqruPUzt6MqoX7ShkGnq4ZD
MRkVnInsKo2fzW+QNPrOzwO/yOsB/0bY+iQHLSpNYF3YRllCiE8L8XU=
-----END CERTIFICATE-----";

        public static string RsaEncrypt(string text)
        {
            var cert = new X509Certificate2(Encoding.ASCII.GetBytes(CULTURELAND_PUBLICKEY));
            System.Security.Cryptography.RSA rsa = cert.GetRSAPublicKey();
			byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.OaepSHA1);
			return BitConverter.ToString(encryptedBytes).Replace("-", "").ToLowerInvariant()[..512];
		}
    }
}