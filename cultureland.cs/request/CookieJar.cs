using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace cultureland.cs.request
{
    public class CookieJar
    {
        private List<Cookie> _cookies;

        public CookieJar() : this(new List<Cookie>()) { }

        public CookieJar(List<Cookie> cookies)
        {
            _cookies = cookies;
        }

        // 저장된 쿠키 반환
        public List<Cookie> Cookies => _cookies;

        // 쿠키 가져오기
        public string Get(string key)
        {
            var cookie = _cookies.FirstOrDefault(c => c.key == key);
            if (cookie != null)
                return cookie.value;
            else
                return string.Empty;
        }

        // 쿠키 추가
        public void Add(string cookie)
        {
            Cookie parsedCookie = Parse(cookie);
            if (parsedCookie != null)
            {
                Add(new List<Cookie> { parsedCookie });
            }
        }

        public void Add(List<string> cookies)
        {
            List<Cookie> parsedCookies = Parse(cookies);
            Add(parsedCookies);
        }

        public void Add(Cookie cookie)
        {
            Add(new List<Cookie> { cookie });
        }

        public void Add(List<Cookie> cookies)
        {
            if (cookies == null)
                throw new ArgumentNullException(nameof(cookies));

            var reversed = cookies.AsEnumerable().Reverse().ToList();
            cookies = cookies.Where((cookie, i) =>
                i == (cookies.Count - 1) - reversed.FindIndex(c => c.key == cookie.key)).ToList();

            foreach (var cookie in cookies)
            {
                Remove(cookie.key);
            }

            _cookies.AddRange(cookies);
        }

        // 쿠키 설정 (모든 쿠키 덮어쓰기)
        public void Set(string cookie)
        {
            Cookie parsedCookie = Parse(cookie);
            _cookies = parsedCookie != null ? new List<Cookie> { parsedCookie } : new List<Cookie>();
        }

        public void Set(List<string> cookies)
        {
            _cookies = Parse(cookies);
        }

        public void Set(Cookie cookie)
        {
            _cookies = new List<Cookie> { cookie };
        }

        public void Set(List<Cookie> cookies)
        {
            _cookies = cookies;
        }

        public bool Remove(string key)
        {
            var removed = _cookies.RemoveAll(c => c.key == key);
            return removed > 0;
        }

        public override string ToString()
        {
            return string.Join("; ",
                _cookies.Select(c =>
                    $"{Uri.EscapeDataString(c.key)}={Uri.EscapeDataString(c.value)}".Replace("%20", "+")
                )
            );
        }

        public static Cookie Parse(string cookie)
        {
            var first = cookie.Split(';')[0];
            var parts = first.Split('=');
            if (parts.Length < 2) throw new ArgumentException("Invalid cookie format", nameof(cookie));
            var key = HttpUtility.UrlDecode(parts[0]);
            var value = HttpUtility.UrlDecode(string.Join("=", parts.Skip(1)));
            return new Cookie { key = key, value = value };
        }

        public static List<Cookie> Parse(List<string> cookies)
        {
            if (cookies == null)
                throw new ArgumentNullException(nameof(cookies));
            var data = cookies.Select(Parse).Where(c => c != null).ToList();
            return data;
        }
    }
}