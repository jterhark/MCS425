using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MCS425_Demo.Models.JwtModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MCS425_Demo.Controllers
{
    public class JwtController : Controller
    {
        public IActionResult Index(IndexModel model)
        {
            if (!CheckLogin()) {
                return RedirectToAction("Login", "Account");
            }

            var items = new List<SelectListItem>();
            items.Add(new SelectListItem()
            {
                Text = "site1",
                Value = "site1url"
            });
            //items.Add(new SelectListItem()
            //{
            //    Text = "site2",
            //    Value = "site2url"
            //});

            if (model == null|| model.Minutes == 0 || model.Site == null) {
                model = new IndexModel();
                model.Sites = items;

                return View(model);
            }


            var options = new CookieOptions();
            //options.Domain = "mcs425-jterhark.azurewebsites.net";
            options.Domain = "localhost";

            var token = GenerateToken(model.Site, model.Minutes);

            Response.Cookies.Append(model.Site, token, options);
            
            return View(new  IndexModel{Token = token, Sites = items});
            
        }

        private string GenerateToken(string audience, int minutes) {
            var header = new JObject(
                new JProperty("alg", "HS256"),
                new JProperty("typ", "JWT")
            );

            //get epoch times
            var issued = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            var expires = (int)(DateTime.Now.AddMinutes(minutes) - new DateTime(1970, 1, 1)).TotalSeconds;

            var payload = new JObject(
                new JProperty("iss", "localhost"),
                new JProperty("sub", HttpContext.Session.GetString("email")),
                new JProperty("name", HttpContext.Session.GetString("name")),
                new JProperty("exp", expires),
                new JProperty("iat", issued),
                new JProperty("aud", audience)
            );

            var headerEncoded = UrlEncode(header.ToString(Formatting.None));
            var payloadEncoded = UrlEncode(payload.ToString(Formatting.None));

            var token = $"{headerEncoded}.{payloadEncoded}";
            var signature = HMACSHA256(token, "WOl3brI9HCoHGPsbhrAoe5XhAv7wCKqOPNrQoI9DLkLuYEEt276C3IGojXR5qnsX");
            return $"{token}.{signature}";
            
        }

        private static string UrlEncode(string x)
        {
            return Convert.ToBase64String(
                Encoding.ASCII.GetBytes(x)
            ).Replace("/", "_").Replace("=", "").Replace('+', '-');
        }

        private static string HMACSHA256(string plainText, string key)
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            var keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] hash;

            using (var hasher = new HMACSHA256(keyBytes))
            {
                hash = hasher.ComputeHash(plainBytes);

            }

            return Convert.ToBase64String(hash).Replace("/", "_").Replace("=", "").Replace('+', '-');
        }

        private bool CheckLogin() {
            return HttpContext.Session.TryGetValue("name", out var temp)&&
                   HttpContext.Session.TryGetValue("email", out temp);
        }
    }
}