using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace cultureland.cs.request
{
    public class CookieJar
    {
        private List<Cookie> _cookies;

        public CookieJar() : this(new List<Cookie>()) { }

        public CookieJar(List<Cookie> cookies)
        {
            if (cookies == null) throw new ArgumentNullException(nameof(cookies));
            _cookies = new List<Cookie>(cookies.Where(c => c != null));
        }

        public List<Cookie> Cookies => _cookies;

        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            var cookie = _cookies.FirstOrDefault(c => c != null && string.Equals(c.key, key, StringComparison.Ordinal));
            return cookie?.value ?? string.Empty;
        }

        public void Add(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie)) return;
            Add(new List<Cookie> { Parse(cookie) });
        }

        public void Add(List<string> cookies)
        {
            if (cookies == null) throw new ArgumentNullException(nameof(cookies));
            Add(Parse(cookies));
        }

        public void Add(Cookie cookie)
        {
            if (cookie == null) throw new ArgumentNullException(nameof(cookie));
            Add(new List<Cookie> { cookie });
        }

        public void Add(List<Cookie> cookies)
        {
            if (cookies == null) throw new ArgumentNullException(nameof(cookies));

            var normalized = cookies
                .Where(c => c != null && !string.IsNullOrEmpty(c.key))
                .ToList();

            if (normalized.Count == 0) return;

            var distinctLast = normalized
                .GroupBy(c => c.key, StringComparer.Ordinal)
                .Select(g => g.Last())
                .ToList();

            foreach (var c in distinctLast)
                Remove(c.key);

            _cookies.AddRange(distinctLast);
        }

        public void Set(string cookie)
        {
            _cookies = new List<Cookie>();
            if (string.IsNullOrWhiteSpace(cookie)) return;
            _cookies.Add(Parse(cookie));
        }

        public void Set(List<string> cookies)
        {
            if (cookies == null) throw new ArgumentNullException(nameof(cookies));
            _cookies = Parse(cookies);
        }

        public void Set(Cookie cookie)
        {
            if (cookie == null) throw new ArgumentNullException(nameof(cookie));
            _cookies = new List<Cookie> { cookie };
        }

        public void Set(List<Cookie> cookies)
        {
            if (cookies == null) throw new ArgumentNullException(nameof(cookies));
            _cookies = new List<Cookie>(cookies.Where(c => c != null));
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            return _cookies.RemoveAll(c => c != null && string.Equals(c.key, key, StringComparison.Ordinal)) > 0;
        }

        public override string ToString()
        {
            return string.Join("; ",
                _cookies
                    .Where(c => c != null && c.key != null)
                    .Select(c =>
                    {
                        var k = Uri.EscapeDataString(c.key ?? string.Empty);
                        var v = Uri.EscapeDataString(c.value ?? string.Empty);
                        return $"{k}={v}".Replace("%20", "+");
                    })
            );
        }

        public static Cookie Parse(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie))
                throw new ArgumentException("Cookie string is null/empty.", nameof(cookie));

            var first = cookie.Split(';')[0].Trim();
            var parts = first.Split(new[] { '=' }, 2);

            if (parts.Length != 2)
                throw new ArgumentException("Invalid cookie format. Expected 'key=value'.", nameof(cookie));

            var key = WebUtility.UrlDecode(parts[0]) ?? string.Empty;
            var value = WebUtility.UrlDecode(parts[1]) ?? string.Empty;

            return new Cookie { key = key, value = value };
        }

        public static List<Cookie> Parse(List<string> cookies)
        {
            if (cookies == null) throw new ArgumentNullException(nameof(cookies));

            var result = new List<Cookie>();
            foreach (var c in cookies)
            {
                if (string.IsNullOrWhiteSpace(c)) continue;
                result.Add(Parse(c));
            }
            return result;
        }
    }
}
