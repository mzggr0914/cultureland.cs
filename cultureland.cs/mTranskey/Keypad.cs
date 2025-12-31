using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace cultureland.cs.mTranskey
{
    public class Keypad
    {
        private static readonly string BlankHash = "205e36a6fe3b8ccdf6449862cc8fcfce";

        private static readonly List<string> SpecialChars = new List<string> {
            "`", "~", "!", "@", "#", "$", "%", "^", "&", "*", "(", ")", "-", "_", "=", "+",
            "[", "{", "]", "}", "\\", "|", ";", ":", "/", "?", ",", "<", ".", ">", "'", "\"",
            "+", "-", "*", "/"
        };

        private static readonly List<string> LowerChars = new List<string> {
            "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
            "q", "w", "e", "r", "t", "y", "u", "i", "o", "p",
            "a", "s", "d", "f", "g", "h", "j", "k", "l",
            "z", "x", "c", "v", "b", "n", "m"
        };

        private static readonly List<string> NumberKeyHashes = new List<string>
        {
            "dbd6475edeeeec441b2179f69bd02240", // 0
            "26ba8686ec2907bee02d15ca5ae8bd7b", // 1
            "25b09759f226527b429a517be2ba9d49", // 2
            "5dd0fe9c1542baf4c99dffbb48b12999", // 3
            "37e845d4bf666a9a464b762d237dc8bf", // 4
            "fd5729a4b298e15080cb4f91a445db12", // 5
            "780cb931708168745816a648a343e42c", // 6
            "d75ee876c025382789ffba30b1d68131", // 7
            "1fe3880633b72ecaa8f612a8867f6cce", // 8
            "565a40b511b2c04dc01021d14617dee3", // 9
            "e07028b84dce0ae9498968ed9c24e687" // empty
		};

        public string KeyIndex = "";
        public readonly MTransKey mTranskey;
        public readonly ServletData servletData;
        public readonly string keyboardType;
        public readonly string name;
        public readonly string inputName;
        public readonly string fieldType;

        public Keypad(MTransKey mTranskey, ServletData servletData, string keyboardType, string name, string inputName, string fieldType)
        {
            this.mTranskey = mTranskey;
            this.servletData = servletData;
            this.keyboardType = keyboardType;
            this.name = name;
            this.inputName = inputName;
            this.fieldType = fieldType;
        }

        /// <summary>
        /// 비밀번호를 키패드 배열에 따라 암호화합니다.
        /// </summary>
        /// <param name="pw">비밀번호</param>
        /// <param name="layout">키패드 배열</param>
        /// <returns>암호화된 비밀번호와 HMAC</returns>
        public (string encrypted, string encryptedHmac) EncryptPassword(string pw, List<int> layout)
        {
            string encrypted = string.Empty;
            foreach (char c in pw)
            {
                List<(int, int)> geoList = keyboardType == "qwerty"
                    ? servletData.keyInfo.qwerty
                    : servletData.keyInfo.number;

                int indexToFind = SpecialChars.Contains(c.ToString())
                    ? SpecialChars.IndexOf(c.ToString())
                    : (keyboardType == "qwerty"
                        ? LowerChars.IndexOf(c.ToString().ToLower())
                        : int.Parse(c.ToString()));

                int geoIndex = layout.IndexOf(indexToFind);

                if (geoIndex < 0 || geoIndex >= geoList.Count)
                {
                    throw new Exception("ERROR_GEO_NOT_FOUND");
                }

                var geo = geoList[geoIndex];

                string geoString = $"{geo.Item1} {geo.Item2}";

                if (keyboardType == "qwerty")
                {
                    if (SpecialChars.Contains(c.ToString()))
                        geoString = "s " + geoString;
                    else if (c.ToString() == c.ToString().ToUpper())
                        geoString = "u " + geoString;
                    else
                        geoString = "l " + geoString;
                }

                string newencr = "$" + Seed.SeedEnc(geoString, mTranskey.sessionKey.ToArray());
                encrypted += newencr;
            }

            string hmac = CreateHmacSha256(encrypted, mTranskey.genSessionKey);


            return (encrypted, hmac);
        }

        static string CreateHmacSha256(string data, string keyString)
        {
            var keyBytes = Encoding.UTF8.GetBytes(keyString);
            var dataBytes = Encoding.UTF8.GetBytes(data);
            using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
            var hash = hmac.ComputeHash(dataBytes);
            return string.Concat(hash.Select(b => b.ToString("x2")));
        }

        static string GetBmp24Md5(SKBitmap src)
        {
            if (src == null || src.Width <= 0 || src.Height <= 0) throw new InvalidOperationException();

            int w = src.Width, h = src.Height;
            int stride = ((w * 3) + 3) & ~3, img = stride * h, off = 54, size = off + img;

            using var rgb = new SKBitmap(w, h, SKColorType.Bgra8888, SKAlphaType.Unpremul);
            using (var g = new SKCanvas(rgb)) { g.Clear(SKColors.White); g.DrawBitmap(src, 0, 0); }

            using var md5 = MD5.Create();
            using var cs = new CryptoStream(Stream.Null, md5, CryptoStreamMode.Write);
            using var bw = new BinaryWriter(cs);

            bw.Write((byte)'B'); bw.Write((byte)'M');
            bw.Write(size); bw.Write(0); bw.Write(off);
            bw.Write(40); bw.Write(w); bw.Write(h);
            bw.Write((short)1); bw.Write((short)24);
            bw.Write(0); bw.Write(img); bw.Write(2835); bw.Write(2835);
            bw.Write(0); bw.Write(0);

            var pad = new byte[stride - w * 3];
            for (int y = h - 1; y >= 0; y--)
            {
                for (int x = 0; x < w; x++) { var c = rgb.GetPixel(x, y); bw.Write(c.Blue); bw.Write(c.Green); bw.Write(c.Red); }
                if (pad.Length > 0) bw.Write(pad);
            }

            cs.FlushFinalBlock();
            var hex = BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
            return hex;
        }

        public async Task<List<int>> GetKeypadLayoutAsync()
        {
            object content = new
            {
                op = "getKeyIndex",
                name = name,
                keyType = keyboardType == "qwerty" ? "lower" : "single",
                keyboardType = $"{keyboardType}Mobile",
                fieldType = fieldType,
                inputName = inputName,
                parentKeyboard = "false",
                transkeyUuid = mTranskey.transkeyUuid,
                exE2E = "false",
                TK_requestToken = servletData.requestToken,
                allocationIndex = mTranskey.allocationIndex.ToString(),
                keyIndex = KeyIndex,
                initTime = servletData.initTime,
                talkBack = "true"
            };

            var keyIndexResponse = await mTranskey.client.PostAsync("transkeyServlet", content);
            KeyIndex = keyIndexResponse.Content.Trim();
            var query = new Dictionary<string, string>
            {
                { "op", "getKey" },
                { "name", name },
                { "keyType", keyboardType == "qwerty" ? "lower" : "single" },
                { "keyboardType", $"{keyboardType}Mobile" },
                { "fieldType", fieldType },
                { "inputName", inputName },
                { "parentKeyboard", "false" },
                { "transkeyUuid", mTranskey.transkeyUuid },
                { "exE2E", "false" },
                { "TK_requestToken", servletData.requestToken },
                { "allocationIndex", mTranskey.allocationIndex.ToString() },
                { "keyIndex", KeyIndex },
                { "initTime", servletData.initTime },
                { "talkBack", "true" }
            };
            var keyImageBufferResponse = await mTranskey.client.GetAsync("transkeyServlet", query);
            var buffer = keyImageBufferResponse.RawBytes;

            using SKBitmap keyImage = SKBitmap.Decode(buffer);

            var keys = new List<SKBitmap>();
            try
            {
                for (int y = 0; y < (keyboardType == "qwerty" ? 4 : 3); y++)
                {
                    for (int x = 0; x < (keyboardType == "qwerty" ? 11 : 4); x++)
                    {
                        if (keyboardType == "qwerty" &&
                            ((x == 0 && y == 3) || // shift
                             ((x == 9 || x == 10) && y == 3)))
                        {
                            continue;
                        }

                        int left = keyboardType == "qwerty" ? x * 54 + 22 : x * 160 + 70;
                        int top = keyboardType == "qwerty" ? y * 80 + 30 : y * 102 + 45;
                        int width = keyboardType == "qwerty" ? 15 : 20;
                        int height = keyboardType == "qwerty" ? 45 : 25;
                        var cropRect = new SKRectI(left, top, left + width, top + height);

                        var croppedBitmap = new SKBitmap(cropRect.Width, cropRect.Height);

                        using (var canvas = new SKCanvas(croppedBitmap))
                        {
                            canvas.DrawBitmap(keyImage, cropRect, new SKRect(0, 0, cropRect.Width, cropRect.Height));
                        }

                        keys.Add(croppedBitmap);
                    }
                }

                var layout = new List<int>();
                int i = 0;
                foreach (var key in keys)
                {
                    var keyHash = GetBmp24Md5(key);

                    if (keyboardType == "qwerty")
                    {
                        if (keyHash == BlankHash) layout.Add(-1);
                        else layout.Add(i++);
                    }
                    else
                    {
                        layout.Add(NumberKeyHashes.IndexOf(keyHash));
                    }
                }

                return layout;
            }
            finally
            {
                foreach (var key in keys)
                {
                    key.Dispose();
                }
            }
        }
    }
}