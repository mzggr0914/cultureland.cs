using System.Collections.Generic;

namespace cultureland.cs
{
	/// <summary>
	/// @example
	/// "ResponseError" - 서버로부터 잘못된 응답을 받았을 경우
	/// "LoginRequiredError" - 로그인이 되어있지 않은 경우
	/// "InvalidPinError" - 유효하지 않은 핀번호를 사용하였을 경우
	/// "RangeError" - 범위를 벗어난 값을 입력한 경우
	/// "SafeLockRequiredError" - 안심금고가 활성화되어있지 않은 경우
	/// "ItemUnavailableError" - 구매가 불가능한 상품을 구매하려는 경우
	/// "PurchaseRestrictedError" - 이 계정에서 구매가 제한된 경우
	/// "DeliverFailError" - 구매가 완료되었지만 메시지/알림톡/메일 전송에 실패한 경우
	/// "PurchaseError" - 구매에 실패한 경우
	/// "LookupError" - 조회에 실패한 경우
	/// "CaptchaError" - 캡챠 해결에 실패한 경우
	/// "LoginError" - 로그인에 실패한 경우
	/// "LoginRestrictedError" - 계정 혹은 IP 주소가 로그인을 제한당한 경우
	/// "PinValidationError" - 컬쳐랜드 핀번호 형식이 올바르지 않은 경우
	/// </summary>
	public enum CulturelandErrorNames
	{
		ResponseError,
		LoginRequiredError,
		InvalidPinError,
		RangeError,
		SafeLockRequiredError,
		ItemUnavailableError,
		PurchaseRestrictedError,
		DeliverFailError,
		PurchaseError,
		LookupError,
		CaptchaError,
		LoginError,
		LoginRestrictedError,
		PinValidationError,
		UnknownError
	}

	public class CulturelandPinParts
	{
		public string[] Parts = new string[4];
	}

	public class Cookie
	{
		public string key { get; set; }
		public string value { get; set; }
	}

	public class VoucherResponse
	{
		public string resultCd { get; set; }
		public string resultOther { get; set; }
		public List<VoucherItem> resultMsg { get; set; }
	}

	public class VoucherItem
	{
		public string LevyTime { get; set; }
		public string GCSubMemberName { get; set; }
		public string State { get; set; }
		public string levyamount { get; set; }
		public string Store_name { get; set; }
		public string LevyDate { get; set; }
	}

	public class VoucherResultOther
	{
		public int FaceValue { get; set; }
		public string ExpiryDate { get; set; }
		public string RegDate { get; set; }
		public string State { get; set; }
		public string CertNo { get; set; }
		public int Balance { get; set; }
	}

	public class SpendHistory
	{
		/// <summary>
		/// 내역 제목
		/// </summary>
		public string title { get; set; }
		/// <summary>
		/// 사용 가맹점 이름
		/// </summary>
		public string merchantName { get; set; }
		/// <summary>
		/// 사용 금액
		/// </summary>
		public int amount { get; set; }
		/// <summary>
		/// 사용 시각
		/// </summary>
		public long timestamp { get; set; }
	}

	public class CulturelandVoucher
	{
		/// <summary>
		/// 상품권의 금액
		/// </summary>
		public int amount { get; set; }
		/// <summary>
		/// 상품권의 잔액
		/// </summary>
		public int balance { get; set; }
		/// <summary>
		/// 상품권의 발행번호 (인증번호)
		/// </summary>
		public string certNo { get; set; }
		/// <summary>
		/// 상품권의 발행일 | 20241231
		/// </summary>
		public string createdDate { get; set; }
		/// <summary>
		/// 상품권의 만료일 | 20291231
		/// </summary>
		public string expiryDate { get; set; }
		/// <summary>
		/// 상품권 사용 내역
		/// </summary>
		public List<SpendHistory> spendHistory { get; set; }
	}

	public class BalanceResponse
	{
		public string safeDelYn { get; set; }
		public string memberKind { get; set; }
		public string casChargeYN { get; set; }
		public string resultCode { get; set; }
		public string resultMessage { get; set; }
		public string blnWaitCash { get; set; }
		public string walletPinYN { get; set; }
		public string bnkAmt { get; set; }
		public string remainCash { get; set; }
		public string transCash { get; set; }
		public string myCash { get; set; }
		public string blnAmt { get; set; }
		public string walletYN { get; set; }
		public string limitCash { get; set; }
	}

	public class CulturelandBalance
	{
		/// <summary>
		/// 사용 가능 금액
		/// </summary>
		public int balance { get; set; }
		/// <summary>
		/// 보관중인 금액 (안심금고)
		/// </summary>
		public int safeBalance { get; set; }
		/// <summary>
		/// 총 잔액 (사용 가능 금액 + 보관중인 금액)
		/// </summary>
		public int totalBalance { get; set; }
	}

	public class CulturelandCharge
	{
		/// <summary>
		/// 성공 여부 메시지
		/// </summary>
		public string message { get; set; }
		/// <summary>
		/// 충전 금액
		/// </summary>
		public int amount { get; set; }
	}

	public class PhoneInfoResponse
	{
		public string recvType { get; set; }
		public string email2 { get; set; }
		public string errCd { get; set; }
		public string email1 { get; set; }
		public string hpNo2 { get; set; }
		public string hpNo1 { get; set; }
		public string hpNo3 { get; set; }
		public string errMsg { get; set; }
		public string sendType { get; set; }
	}

	public class CulturelandGift
	{
		public Pin pin { get; set; }
		public string url { get; set; }
		public string controlCode { get; set; }
	}

	public class GiftLimitResponse
	{
		public string errCd { get; set; }
		public GiftVO giftVO { get; set; }
		public string errMsg { get; set; }

		public class GiftVO
		{
			public double maxAmount { get; set; }
			public object custCd { get; set; }
			public double balanceAmt { get; set; }
			public double safeAmt { get; set; }
			public double cashGiftRemainAmt { get; set; }
			public double cashGiftSumGift { get; set; }
			public string cashGiftNoLimitYn { get; set; } // "Y" | "N"
			public string cashGiftNoLimitUserYn { get; set; }
			public double cashGiftLimitAmt { get; set; }
			public double cashGiftMGiftRemainDay { get; set; }
			public double cashGiftMGiftRemainMon { get; set; }
			public object toUserId { get; set; }
			public object toUserNm { get; set; }
			public object toMsg { get; set; }
			public object transType { get; set; }
			public object timestamp { get; set; }
			public object certValue { get; set; }
			public object revPhone { get; set; }
			public object paymentType { get; set; }
			public object sendType { get; set; }
			public object sendTypeNm { get; set; }
			public object giftCategory { get; set; }
			public object sendTitl { get; set; }
			public double amount { get; set; }
			public double quantity { get; set; }
			public object controlCd { get; set; }
			public object lgControlCd { get; set; }
			public object contentsCd { get; set; }
			public object contentsNm { get; set; }
			public object svrGubun { get; set; }
			public object payType { get; set; }
			public object levyDate { get; set; }
			public object levyTime { get; set; }
			public object levyDateTime { get; set; }
			public object genreDtl { get; set; }
			public double faceValue { get; set; }
			public double sendCnt { get; set; }
			public double balance { get; set; }
			public object state { get; set; }
			public object lgState { get; set; }
			public object dtlState { get; set; }
			public object selType { get; set; }
			public object strPaymentType { get; set; }
			public object strSendType { get; set; }
			public object strRcvInfo { get; set; }
			public object appUseYn { get; set; }
			public object reSendYn { get; set; }
			public object reSendState { get; set; }
			public object strReSendState { get; set; }
			public object cnclState { get; set; }
			public object strCnclState { get; set; }
			public double page { get; set; }
			public double pageSize { get; set; }
			public double totalCnt { get; set; }
			public double totalSum { get; set; }
			public double totalCntPage { get; set; }
			public object isLastPageYn { get; set; }
			public object reSendType { get; set; }
			public object reSvrGubun { get; set; }
			public object aESImage { get; set; }
			public object sendUserId { get; set; }
			public object sendUserNm { get; set; }
			public object rcvUserKey { get; set; }
			public object rcvUserID { get; set; }
			public object rcvName { get; set; }
			public object rcvHpno { get; set; }
			public object sendMsg { get; set; }
			public object giftType { get; set; }
			public object sendDate { get; set; }
			public object receiveDate { get; set; }
			public object expireDate { get; set; }
			public object cancelDate { get; set; }
			public object cancelType { get; set; }
			public object regdate { get; set; }
			public double waitPage { get; set; }
			public double sendPage { get; set; }
			public double waitCnt { get; set; }
			public double cancelCnt { get; set; }
			public double transCnt { get; set; }
			public double successCnt { get; set; }
			public double nbankMGiftRemainDay { get; set; }
			public string nbankNoLimitUserYn { get; set; }
			public string nbankNoLimitYn { get; set; } // "Y" | "N"
			public string ccashNoLimitUserYn { get; set; }
			public double ccashRemainAmt { get; set; }
			public double ccashMGiftRemainMon { get; set; }
			public double ccashMGiftRemainDay { get; set; }
			public double nbankRemainAmt { get; set; }
			public string rtimeNoLimitUserYn { get; set; }
			public string ccashNoLimitYn { get; set; } // "Y" | "N"
			public double nbankMGiftRemainMon { get; set; }
			public double rtimeMGiftRemainMon { get; set; }
			public double rtimeMGiftRemainDay { get; set; }
			public string rtimeNoLimitYn { get; set; } // "Y" | "N"
			public double rtimeRemainAmt { get; set; }
			public double nbankLimitAmt { get; set; }
			public double rtimeSumGift { get; set; }
			public double ccashLimitAmt { get; set; }
			public double nbankSumGift { get; set; }
			public double nbankSumVacnt { get; set; }
			public double rtimeLimitAmt { get; set; }
			public double ccashSumGift { get; set; }
		}
	}

	public class CulturelandGiftLimit
	{
		/// <summary>
		/// 잔여 선물 한도
		/// </summary>
		public int remain { get; set; }
		/// <summary>
		/// 최대 선물 한도
		/// </summary>
		public int limit { get; set; }
	}

	public class ChangeCoupangCashResponse
	{
		public string resultCd { get; set; }
		public string resultMsg { get; set; }
	}

	public class CulturelandChangeCoupangCash
	{
		/// <summary>
		/// (전환 수수료 6%가 차감된) 전환된 금액
		/// </summary>
		public int amount { get; set; }
	}

	public class ChangeSmileCashResponse
	{
		public string resultCd { get; set; }
		public string resultMsg { get; set; }
	}

	public class CulturelandChangeSmileCash
	{
		/// <summary>
		/// (전환 수수료 5%가 과금된) 과금된 금액
		/// </summary>
		public int amount { get; set; }
	}

	public class CulturelandGooglePlay
	{
		/// <summary>
		/// 기프트 코드 번호
		/// </summary>
		public string pin { get; set; }
		/// <summary>
		/// 자동 입력 URL
		/// </summary>
		public string url { get; set; }
		/// <summary>
		/// 카드번호
		/// </summary>
		public string certNo { get; set; }
	}

	public class GooglePlayBuyResponse
	{
		public string errCd { get; set; }
		public string pinBuyYn { get; set; }
		public string errMsg { get; set; }
	}

	public class GooglePlayHistoryResponse
	{
		public List<ListItem> list { get; set; }
		public CpnVO cpnVO { get; set; }

		public class ListItem
		{
			public Item item { get; set; }
		}

		public class Item
		{
			public string fee { get; set; }
			public string reSendState { get; set; } // "Y" | "N"
			public string cnclState { get; set; } // "Y" | "N"
			public string strLevyDate { get; set; }
			public string CertGroup { get; set; }
			public string ContentsName { get; set; }
			public string PurchaseCertNo { get; set; }
			public string LevyTime { get; set; }
			public string strMaskScrachNo { get; set; }
			public string payType { get; set; } // "컬쳐캐쉬" | "신용카드"
			public string strRcvInfo { get; set; }
			public string ReceiveInfo { get; set; }
			public string culturelandGiftNo { get; set; }
			public string ReSend { get; set; }
			public string culturelandGiftMaskNo { get; set; }
			public string ExSubMemberCode { get; set; }
			public string certGroup { get; set; }
			public string FaceValue { get; set; }
			public string strLevyTime { get; set; }
			public string levyDateTime { get; set; }
			public string ContentsCode { get; set; } // "GOOGLE"
			public string Amount { get; set; }
			public string ControlCode { get; set; }
			public string PinSaleControlCode { get; set; }
			public string cultureGiftFaceValue { get; set; }
			public string RowNumber { get; set; }
			public string CouponCode { get; set; }
			public string GCSubMemberCode { get; set; }
			public string CancelDate { get; set; }
			public string ExMemberCode { get; set; }
			public string State { get; set; }
			public string SubMemberCode { get; set; }
			public string googleDcUserHpCheck { get; set; } // "Y" | "N"
			public string MemberControlCode { get; set; }
			public string CertNo { get; set; }
			public string ScrachNo { get; set; }
			public string LevyDate { get; set; }
			public string cnclLmtDate { get; set; }
		}

		public class CpnVO
		{
			public object buyType { get; set; }
			public object cpgm { get; set; }
			public object couponNm { get; set; }
			public object contentsCd { get; set; }
			public object alertAmt { get; set; }
			public object couponAmt { get; set; }
			public object saleAmt { get; set; }
			public object comments { get; set; }
			public object agreeMsg { get; set; }
			public object serviceStatus { get; set; }
			public object tfsSeq { get; set; }
			public object hpNo1 { get; set; }
			public object hpNo2 { get; set; }
			public object hpNo3 { get; set; }
			public object recvHP { get; set; }
			public object email1 { get; set; }
			public object email2 { get; set; }
			public object recvEmail { get; set; }
			public object sendType { get; set; }
			public object buyCoupon { get; set; }
			public object direction { get; set; }
			public object couponCode { get; set; }
			public object memberCd { get; set; }
			public object pinType { get; set; }
			public object agencyNm { get; set; }
			public object faceval { get; set; }
			public object safeBalance { get; set; }
			public object hp_no1 { get; set; }
			public object hp_no2 { get; set; }
			public object hp_no3 { get; set; }
			public object phoneNumber { get; set; }
			public object prodNo { get; set; }
			public object tmpCLState { get; set; }
			public object res_code { get; set; }
			public object datasize { get; set; }
			public object salePercent { get; set; }
			public object saleBuyLimit { get; set; }
			public int isSale { get; set; }
			public double balance { get; set; }
			public double safeAmt { get; set; }
			public double amount { get; set; }
			public object arrCouponAmt { get; set; }
			public object arrSaleAmt { get; set; }
			public object arrSalePer { get; set; }
			public object arrBuyCoupon { get; set; }
			public object arrAlertAmt { get; set; }
			public object arrCouponCode { get; set; }
			public object arrCouponName { get; set; }
			public object arrComments { get; set; }
			public object couponCodeType { get; set; }
			public object remainMAmount { get; set; }
			public object remainDAmount { get; set; }
			public object remainMAmountUser { get; set; }
			public object remainDAmountUser { get; set; }
			public object maxMAmountUser { get; set; }
			public object maxDAmountUser { get; set; }
			public object feeType { get; set; }
			public int quantity { get; set; }
			public double page { get; set; }
			public double pageSize { get; set; }
			public int buyCnt { get; set; }
			public double totalCnt { get; set; }
			public double feeAmount { get; set; }
			public string fee { get; set; } // "0"
			public string isLastPageYn { get; set; } // "Y" | "N"
			public object controlCd { get; set; }
			public object subMemberCd { get; set; }
			public object pinSaleControlCd { get; set; }
			public object recvInfo { get; set; }
			public object code1 { get; set; }
			public object code2 { get; set; }
			public object code3 { get; set; }
			public object code4 { get; set; }
			public object code5 { get; set; }
			public object recvType { get; set; }
			public object couponContent { get; set; }
			public object oriAmount { get; set; }
			public int isCulSale { get; set; }
			public double deliveryFee { get; set; }
			public string deliveryType { get; set; }
			public string recvNm { get; set; }
			public string recvPost { get; set; }
			public string recvAddr1 { get; set; }
			public string recvAddr2 { get; set; }
			public int envelopeQty { get; set; }
			public string billCheck { get; set; }
			public int isEvnFee { get; set; }
			public string evnFee { get; set; } // "0"
			public double evnFeeAmount { get; set; }
			public double freefeeAmount { get; set; }
			public object eventCode { get; set; }
			public string cpnType { get; set; }
			public object salePer { get; set; }
		}
	}

	public class UserInfoResponse
	{
		public string Del_Yn { get; set; } // "Y" | "N"
		public string callUrl { get; set; }
		public string custCd { get; set; }
		public string certVal { get; set; }
		public string backUrl { get; set; }
		public string authDttm { get; set; }
		public string resultCode { get; set; }
		public string user_key { get; set; }
		public string Status_M { get; set; }
		public string Phone { get; set; }
		public string Status_Y { get; set; }
		public string Status_W { get; set; }
		public string Status { get; set; }
		public string SafeLevel { get; set; }
		public string Status_D { get; set; }
		public string CashPwd { get; set; }
		public string RegDate { get; set; }
		public string resultMessage { get; set; }
		public string userId { get; set; }
		public string userKey { get; set; }
		public string Proc_Date { get; set; }
		public int size { get; set; }
		public string user_id { get; set; }
		public string succUrl { get; set; }
		public string userIp { get; set; }
		public string Mobile_Yn { get; set; } // "Y" | "N"
		public string idx { get; set; }
		public string category { get; set; }
	}

	public class CulturelandUser
	{
		/// <summary>
		/// 휴대폰 번호
		/// </summary>
		public string phone { get; set; }

		/// <summary>
		/// 안심금고 레벨
		/// </summary>
		public int safeLevel { get; set; }

		/// <summary>
		/// 안심금고 비밀번호 여부
		/// </summary>
		public bool safePassword { get; set; }

		/// <summary>
		/// 가입 시각
		/// </summary>
		public long registerDate { get; set; }

		/// <summary>
		/// 컬쳐랜드 ID
		/// </summary>
		public string userId { get; set; }

		/// <summary>
		/// 유저 고유 번호
		/// </summary>
		public string userKey { get; set; }

		/// <summary>
		/// 접속 IP
		/// </summary>
		public string userIp { get; set; }

		/// <summary>
		/// 유저 고유 인덱스
		/// </summary>
		public int index { get; set; }

		/// <summary>
		/// 유저 종류
		/// </summary>
		public string category { get; set; }
	}

	public class CulturelandMember
	{
		/// <summary>
		/// 컬쳐랜드 ID
		/// </summary>
		public string id { get; set; }

		/// <summary>
		/// 멤버의 이름 | 홍*동
		/// </summary>
		public string name { get; set; }

		/// <summary>
		/// 멤버의 인증 등급
		/// </summary>
		public string verificationLevel { get; set; } // "본인인증"
	}

	public class CashLogsResponse : List<CashLogItem> { }

	public class CashLogItem
	{
		public string accDate { get; set; }
		public string memberCode { get; set; }
		public string outAmount { get; set; }
		public string balance { get; set; }
		public string inAmount { get; set; }
		public string NUM { get; set; }
		public string Note { get; set; }
		public string accTime { get; set; }
		public string memberName { get; set; }
		public string accType { get; set; }
		public string safeAmount { get; set; }
	}

	public class CulturelandCashLog
	{
		/// <summary>
		/// 내역 제목
		/// </summary>
		public string title { get; set; }

		/// <summary>
		/// 사용 가맹점 코드
		/// </summary>
		public string merchantCode { get; set; }

		/// <summary>
		/// 사용 가맹점 이름
		/// </summary>
		public string merchantName { get; set; }

		/// <summary>
		/// 사용 금액
		/// </summary>
		public int amount { get; set; }

		/// <summary>
		/// 사용 후 남은 잔액
		/// </summary>
		public int balance { get; set; }

		/// <summary>
		/// 사용 종류
		/// </summary>
		public string spendType { get; set; } // "사용" | "사용취소" | "충전"

		/// <summary>
		/// 사용 시각
		/// </summary>
		public long timestamp { get; set; }
	}

	public class CulturelandLogin
	{
		/// <summary>
		/// 컬쳐랜드 ID
		/// </summary>
		public string userId { get; set; }

		/// <summary>
		/// 로그인 유지 쿠키
		/// </summary>
		public string keepLoginConfig { get; set; }
	}
}