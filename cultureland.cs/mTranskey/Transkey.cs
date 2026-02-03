using cultureland.cs.request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cultureland.cs.mTranskey
{
    public class MTransKey
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

        public List<uint> sessionKey;
        public string transkeyUuid;
        public string genSessionKey;
        public string encryptedSessionKey;
        public uint allocationIndex;
        public FetchClient client;

        public MTransKey(FetchClient fetchclient)
        {
            client = fetchclient;
            transkeyUuid = GenerateRandomHex(32);
            genSessionKey = GenerateRandomHex(8);
            sessionKey = new List<uint>(16);
            for (int i = 0; i < 16; i++)
            {
                sessionKey.Add(Convert.ToUInt32(genSessionKey[i].ToString(), 16));
            }
            encryptedSessionKey = RsaEncrypt(genSessionKey);
            allocationIndex = GenerateRandomUInt32(int.MaxValue);
        }

        private static string RsaEncrypt(string text)
        {
            var cert = new X509Certificate2(Encoding.ASCII.GetBytes(CULTURELAND_PUBLICKEY));
            System.Security.Cryptography.RSA rsa = cert.GetRSAPublicKey();
            byte[] encryptedBytes = rsa.Encrypt(Encoding.UTF8.GetBytes(text), RSAEncryptionPadding.OaepSHA1);
            return BitConverter.ToString(encryptedBytes).Replace("-", "").ToLowerInvariant()[..512];
        }

        private static uint GenerateRandomUInt32(uint exclusiveMax)
        {
            byte[] randomBytes = new byte[4];
            uint randomValue;
            do
            {
                using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                {
                    rng.GetBytes(randomBytes);
                }
                randomValue = BitConverter.ToUInt32(randomBytes, 0);
            }
            while (randomValue >= exclusiveMax);
            return randomValue;
        }

        private static string GenerateRandomHex(int bytes)
        {
            var randomBytes = new byte[bytes];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return BitConverter.ToString(randomBytes).Replace("-", "").ToLower();
        }

        public async Task<ServletData> GetServletDataAsync()
        {
            var requestTokenResponse = await client.GetAsync($"transkeyServlet?op=getToken&{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");

            if (requestTokenResponse.Content is null)
                throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");

            var requestTokenMatch = Regex.Match(requestTokenResponse.Content, @"var TK_requestToken=([\d-]+);");
            string requestToken = requestTokenMatch.Success ? requestTokenMatch.Groups[1].Value : "0";

            var initTimeResponse = await client.GetAsync("transkeyServlet", new Dictionary<string, string>() { { "op", "getInitTime" } });

            if (initTimeResponse.Content is null)
                throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");

            var initTimeMatch = Regex.Match(initTimeResponse.Content, @"var initTime='([\d-]+)';");
            string initTime = initTimeMatch.Success ? initTimeMatch.Groups[1].Value : "0";

            var content = new Dictionary<string, string>()
            {
                { "op", "getKeyInfo" },
                {"key" , encryptedSessionKey },
                {"transkeyUuid" , transkeyUuid },
                {"useCert" , "true" },
                {"TK_requestToken" , requestToken },
                {  "mode" , "Mobile"}
            };

            var keyInfoResponse = await client.PostAsync("transkeyServlet", content);
            var keyPositions = keyInfoResponse.Content;

            if (keyPositions is null)
                throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");

            string[] parts = keyPositions.Split("var numberMobile = new Array();");
            var qwertyInfo = ExtractPoints(parts[0], "qwertyMobile.push(key);");
            var numberInfo = ExtractPoints(parts[1], "numberMobile.push(key);");

            return new ServletData
            {
                requestToken = requestToken,
                initTime = initTime,
                keyInfo = new KeyInfo
                {
                    qwerty = qwertyInfo,
                    number = numberInfo
                }
            };

            static List<(int, int)> ExtractPoints(string input, string splitPattern)
            {
                string[] points = input.Split(splitPattern);
                Array.Resize(ref points, points.Length - 1);
                return points.Select(p =>
                {
                    var matches = Regex.Matches(p, @"key\.addPoint\((\d+), (\d+)\);");
                    var match = matches[0];
                    int x = int.Parse(match.Groups[1].Value);
                    int y = int.Parse(match.Groups[2].Value);
                    return (x, y);
                }).ToList();
            }
        }

        public Keypad CreateKeypad(ServletData servletData, string keyboardType, string name, string inputName, string fieldType = "password")
        {
            return new Keypad(
            this,
                servletData,
                keyboardType,
                name,
                inputName,
                fieldType
            );
        }
    }
}