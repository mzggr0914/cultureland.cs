using cultureland.cs.mTranskey;
using cultureland.cs.request;
using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace cultureland.cs
{
    public class Cultureland
    {
        public FetchClient client;
        public CulturelandUser userInfo;
        public string keepLoginInfo;

        public Cultureland(string proxyAddress = null, string certificatePath = null)
        {
            client = new FetchClient(proxyAddress, certificatePath);
        }

        public async Task<CulturelandUser> GetUserInfoAsync()
        {
            if (!await IsLoginAsync())
                throw new CulturelandError(CulturelandErrorNames.LoginRequiredError, "로그인이 필요한 서비스 입니다.");

            var userInfoResponse = await client.PostAsync("tgl/flagSecCash.json");

            if (!userInfoResponse.IsSuccessStatusCode)
                throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");

            var userInfo = JsonConvert.DeserializeObject<UserInfoResponse>(userInfoResponse.Content);

            if (userInfo?.resultMessage != "성공")
                throw new CulturelandError(CulturelandErrorNames.LookupError, userInfo?.resultMessage ?? "잘못된 응답이 반환되었습니다.");

            return new CulturelandUser
            {
                phone = userInfo.Phone,
                safeLevel = int.Parse(userInfo.SafeLevel),
                safePassword = userInfo.CashPwd != "0",
                registerDate = new DateTimeOffset(DateTime.ParseExact(userInfo.RegDate, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture)).ToUnixTimeMilliseconds(),
                userId = userInfo.userId,
                userKey = userInfo.userKey,
                userIp = userInfo.userIp,
                index = int.Parse(userInfo.idx),
                category = userInfo.category
            };
        }

        public async Task<bool> IsLoginAsync()
        {
            var isLoginResponse = await client.PostAsync("mmb/isLogin.json");

            if (!isLoginResponse.IsSuccessStatusCode)
                throw new CulturelandError(CulturelandErrorNames.ResponseError, "Failed to check login status.");

            var isLogin = JsonConvert.DeserializeObject<bool>(isLoginResponse.Content);

            return isLogin;
        }

        private class Credentials
        {
            public string Id { get; set; }
            public string Password { get; set; }
        }

        public async Task<CulturelandLogin> LoginAsync(string keepLoginInfo)
        {
            keepLoginInfo = HttpUtility.UrlDecode(keepLoginInfo);

            client.CookieJar.Add(new Cookie()
            {
                key = "KeepLoginConfig",
                value = keepLoginInfo
            });
            var loginMainRequest = await client.GetAsync("/mmb/loginMain.do", headers: new Dictionary<string, string> {
                    { "Referer", "https://m.cultureland.co.kr/index.do" }
                });

            string loginMain = loginMainRequest.Content;
            string userId = Regex.Match(loginMain, @"<input\s+type=""text""\s+id=""txtUserId""\s+name=""userId""\s+value=""(\w*)""").Groups[1].Value
                ?? throw new CulturelandError(CulturelandErrorNames.LoginError, "입력하신 로그인 유지 정보는 만료된 정보입니다.");

            var transKey = new MTransKey(client);
            var servletData = await transKey.GetServletDataAsync();
            var keypad = transKey.CreateKeypad(servletData, "qwerty", "passwd", "passwd");
            var keypadLayout = await keypad.GetKeypadLayoutAsync();

            var (encrypted, encryptedHmac) = keypad.EncryptPassword(string.Empty, keypadLayout);

            var payload = new
            {
                keepLoginInfo = keepLoginInfo,
                userId = userId,
                keepLogin = "Y",
                seedKey = transKey.encryptedSessionKey,
                initTime = servletData.initTime,
                keyIndex_passwd = keypad.KeyIndex,
                keyboardType_passwd = keypad.keyboardType + "Mobile",
                fieldType_passwd = keypad.fieldType,
                transkeyUuid = transKey.transkeyUuid,
                transkey_passwd = encrypted,
                transkey_HM_passwd = encryptedHmac
            };

            var loginRequest = await client.PostAsync("mmb/loginProcess.do", payload, new Dictionary<string, string>() {
                { "Referer", "https://m.cultureland.co.kr/mmb/loginMain.do"}
            }, allowRedirects: false);

            if (loginRequest.StatusCode == HttpStatusCode.OK)
            {
                string loginData = loginRequest.Content;
                var errorMessageMatch = Regex.Match(loginData, @"<input type=""hidden"" name=""loginErrMsg""  value=""([^""]+)"" \/>");
                if (errorMessageMatch.Success)
                {
                    throw new CulturelandError(CulturelandErrorNames.LoginError, errorMessageMatch.Groups[1].Value.Replace("\\n\\n", ". "));
                }
                throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");
            }
            var url = loginRequest.GetHeaderValue("Location");
            if (url == "/cmp/authConfirm.do")
            {
                var errorPageRequest = await client.GetAsync(loginRequest.GetHeaderValue("Location"));
                string errorPage = errorPageRequest.Content;

                var errorCodeMatch = Regex.Match(errorPage, @"var errCode = ""(\d+)"";");
                string errorCode = errorCodeMatch.Success ? errorCodeMatch.Groups[1].Value : null;
                throw new CulturelandError(CulturelandErrorNames.LoginRestrictedError, $"컬쳐랜드 로그인 정책에 따라 로그인이 제한되었습니다.{(errorCode != null ? $" (제한코드: {errorCode})" : "")}");
            }
            userInfo = await GetUserInfoAsync();
            var keepLoginConfigCookie = client.CookieJar.Cookies.Find(x => x.key == "KeepLoginConfig")
                ?? throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");
            this.keepLoginInfo = keepLoginConfigCookie.value;

            return new CulturelandLogin
            {
                userId = userId,
                keepLoginConfig = HttpUtility.UrlDecode(keepLoginInfo).Replace("+", " ")
            };
        }

        public async Task<CulturelandBalance> GetBalanceAsync()
        {
            if (!await IsLoginAsync())
                throw new CulturelandError(CulturelandErrorNames.LoginRequiredError, "로그인이 필요한 서비스 입니다.");

            var BalanceRequest = await client.PostAsync("/tgl/getBalance.json");

            var BalanceResponse = JObject.Parse(BalanceRequest.Content);
            if ((string)BalanceResponse["resultMessage"] != "성공")
            {
                if (BalanceResponse["resultMessage"] != null)
                    throw new CulturelandError(CulturelandErrorNames.LookupError, (string)BalanceResponse["resultMessage"]);
                throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");
            }

            return new CulturelandBalance()
            {
                balance = int.Parse((string)BalanceResponse["blnAmt"]),
                safeBalance = int.Parse((string)BalanceResponse["bnkAmt"]),
                totalBalance = int.Parse((string)BalanceResponse["myCash"])
            };
        }

        public async Task<CulturelandCharge> ChargeAsync(Pin pin)
        {
            return (await ChargeAsync(new List<Pin> { pin })).FirstOrDefault();
        }

        public async Task<List<CulturelandCharge>> ChargeAsync(List<Pin> pins)
        {
            if (!await IsLoginAsync())
                throw new CulturelandError(CulturelandErrorNames.LoginRequiredError, "로그인이 필요한 서비스 입니다.");

            if (pins == null || pins.Count < 1 || pins.Count > 10)
                throw new CulturelandError(CulturelandErrorNames.RangeError, "핀번호는 1개 이상, 10개 이하여야 합니다.");

            bool onlyMobileVouchers = pins.All(pin => pin.Parts[3].Length == 4);

            await client.GetAsync(onlyMobileVouchers
                ? "csh/cshGiftCard.do"
                : "csh/cshGiftCardOnline.do");

            var transKey = new MTransKey(client);
            var servletData = await transKey.GetServletDataAsync();

            var payload = new Dictionary<string, string>
            {
                { "seedKey", transKey.encryptedSessionKey },
                { "initTime", servletData.initTime },
                { "transkeyUuid", transKey.transkeyUuid }
            };

            for (int i = 0; i < pins.Count; i++)
            {
                var parts = pins[i]?.Parts ?? new[] { "", "", "", "" };
                int pinCount = i + 1;

                string txtScr4 = $"txtScr{pinCount}4";

                var keypad = transKey.CreateKeypad(servletData, "number", txtScr4, $"scr{pinCount}4");
                var keypadLayout = await keypad.GetKeypadLayoutAsync();
                var (encrypted, encryptedHmac) = keypad.EncryptPassword(parts[3], keypadLayout);

                var inputs = new Dictionary<string, string>
                {
                    [$"scr{pinCount}1"] = parts[0],
                    [$"scr{pinCount}2"] = parts[1],
                    [$"scr{pinCount}3"] = parts[2],
                    ["keyIndex_" + txtScr4] = keypad.KeyIndex,
                    ["keyboardType_" + txtScr4] = keypad.keyboardType + "Mobile",
                    ["fieldType_" + txtScr4] = keypad.fieldType,
                    ["transkey_" + txtScr4] = encrypted,
                    ["transkey_HM_" + txtScr4] = encryptedHmac
                };

                foreach (var input in inputs)
                {
                    payload[input.Key] = input.Value;
                }
            }

            if (transKey.encryptedSessionKey != payload["seedKey"]) throw new Exception("seedKey mismatch");
            if (transKey.transkeyUuid != payload["transkeyUuid"]) throw new Exception("uuid mismatch");
            if (servletData.initTime != payload["initTime"]) throw new Exception("initTime mismatch");


            var chargeRequest = await client.PostAsync(
                onlyMobileVouchers
                    ? "csh/cshGiftCardProcess.do"
                    : "csh/cshGiftCardOnlineProcess.do",
                body: payload, allowRedirects: false);

            var chargeResultRequest = await client.GetAsync(chargeRequest.GetHeaderValue("Location"));
            string chargeResult = chargeResultRequest.Content;
            var doc = new HtmlDocument();
            doc.LoadHtml(chargeResult);
            var parsedResults = doc.DocumentNode
                .Descendants("tbody").First()
                .Descendants("tr").ToList();
            var results = new List<CulturelandCharge>();

            for (int i = 0; i < pins.Count; i++)
            {
                var chargeResultRow = parsedResults[i].Descendants("td").ToList();
                results.Add(new CulturelandCharge
                {
                    message = chargeResultRow[2].InnerText,
                    amount = int.Parse(chargeResultRow[3].InnerText.Replace(",", "").Replace("원", ""))
                });
            }

            return results;
        }

        public async Task<CulturelandGift> GiftAsync(int amount, string phoneNumber = null)
        {
            if (!await IsLoginAsync())
                throw new CulturelandError(CulturelandErrorNames.LoginRequiredError, "로그인이 필요한 서비스 입니다.");

            if (amount % 100 != 0 || amount < 1000 || amount > 50000)
                throw new CulturelandError(CulturelandErrorNames.RangeError, "구매 금액은 최소 1천원부터 최대 5만원까지 100원 단위로 입력 가능합니다.");

            var UserInfo = await GetUserInfoAsync();
            var GiftPageResponse = await client.GetAsync("gft/gftPhoneApp.do");

            if (GiftPageResponse.GetHeaderValue("Location") == "/ctf/intgAuthBridge.do")
            {
                throw new CulturelandError(CulturelandErrorNames.PurchaseRestrictedError, "안전한 컬쳐랜드 서비스 이용을 위해 통합본인인증이 필요합니다.");
            }

            if (phoneNumber == null)
            {
                var PhoneInfoRequest = await client.PostAsync("cpn/getGoogleRecvInfo.json", new
                {
                    sendType = "LMS",
                    recvType = "M",
                    cpnType = "GIFT"
                }, headers: new Dictionary<string, string> {
                    { "Referer", "https://m.cultureland.co.kr/gft/gftPhoneApp.do"}
                });

                var PhoneInfo = JObject.Parse(PhoneInfoRequest.Content);
                if ((string)PhoneInfo["errMsg"] != "정상")
                {
                    if (PhoneInfo.ContainsKey("errMsg")) throw new CulturelandError(CulturelandErrorNames.LookupError, (string)PhoneInfo["errMsg"]);
                    throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");
                }
                phoneNumber = $"{(string)PhoneInfo["hpNo1"]}{(string)PhoneInfo["hpNo2"]}{(string)PhoneInfo["hpNo3"]}";
            }


            var SendGiftRequest = await client.PostAsync("gft/gftPhoneCashProc.do", new
            {
                revEmail = "",
                sendType = "S",
                userKey = UserInfo.userKey.ToString(),
                limitGiftBank = "N",
                bankRM = "OK",
                giftCategory = "M",
                quantity = "1",
                amount = amount.ToString(),
                chkLms = "M",
                revPhone = phoneNumber,
                paymentType = "cash",
                agree = "on"
            }, allowRedirects: false);

            var GiftResultRequest = await client.PostAsync(SendGiftRequest.GetHeaderValue("location"));

            string GiftResult = GiftResultRequest.Content;

            if (GiftResult.Contains("<strong> 컬쳐랜드상품권(모바일문화상품권)<br />선물(구매)가 완료되었습니다.</strong>"))
            {
                var barcodeMatch = Regex.Match(GiftResult,
                    @"<input\s+type=""hidden""\s+id=""barcodeImage""\s+name=""barcodeImage""\s+value=""https:\/\/m\.cultureland\.co\.kr\/csh\/mb\.do\?code=([\w\/\+=]+)""\s*\/?>",
                    RegexOptions.IgnoreCase);

                var barcodeCode = barcodeMatch.Success ? barcodeMatch.Groups[1].Value : null;
                if (string.IsNullOrEmpty(barcodeCode))
                    throw new CulturelandError(CulturelandErrorNames.ResponseError, "선물 결과에서 바코드 URL을 찾을 수 없습니다.");


                var controlMatch = Regex.Match(
                    GiftResult,
                    @"<input\s+type=""hidden""\s+id=""controlcode""\s+name=""controlcode""\s+value=""(\w+)""\s*\/?>",
                    RegexOptions.IgnoreCase);

                var controlCode = controlMatch.Success ? controlMatch.Groups[1].Value : null;
                if (string.IsNullOrEmpty(controlCode))
                    throw new CulturelandError(CulturelandErrorNames.ResponseError, "선물 결과에서 발행번호를 찾을 수 없습니다.");

                var barcodePath = $"csh/mb.do?code={barcodeCode}";
                var barcodeDataRequest = await client.GetAsync(barcodePath);

                string barcodeData = barcodeDataRequest.Content;

                string pinCode = barcodeData
                    .Split("<span>바코드번호</span>")[1]
                    .Split("</span>")[0]
                    .Split("<span>")[1];

                return new CulturelandGift()
                {
                    pin = new Pin(pinCode),
                    url = $"https://m.cultureland.co.kr/{barcodePath}",
                    controlCode = controlCode,
                };
            }
            var match = Regex.Match(GiftResult, @"<dt class=""two"">실패 사유 <span class=""right"">(.*?)<\/span><\/dt>");
            var failReason = match.Success ? match.Groups[1].Value.Replace("<br>", " ") : null;

            if (!string.IsNullOrEmpty(failReason))
                throw new CulturelandError(CulturelandErrorNames.PurchaseError, failReason);

            throw new CulturelandError(CulturelandErrorNames.ResponseError, "잘못된 응답이 반환되었습니다.");
        }
    }
}