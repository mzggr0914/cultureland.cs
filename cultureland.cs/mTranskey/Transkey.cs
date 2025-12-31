using cultureland.cs.request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace cultureland.cs.mTranskey
{
    public class MTransKey
    {
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
            encryptedSessionKey = RSA.RsaEncrypt(genSessionKey);
            allocationIndex = GenerateRandomUInt32(int.MaxValue);
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
            var requestTokenMatch = Regex.Match(requestTokenResponse.Content, @"var TK_requestToken=([\d-]+);");
            string requestToken = requestTokenMatch.Success ? requestTokenMatch.Groups[1].Value : "0";

            var initTimeResponse = await client.GetAsync("transkeyServlet", new Dictionary<string, string>() { { "op", "getInitTime" } });
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