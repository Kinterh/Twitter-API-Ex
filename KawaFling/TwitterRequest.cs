﻿using System;
using System.Text;
using System.Net;
using System.Security.Cryptography;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace KawaFling
{
    class TwitterRequest
    {
        static string oauth_token;
        static string oauth_token_secret;
        static string oauth_verifier;

        static string Callback = "oob";
        static string ConsumerKey = "xUV8UwzpJk4aMyTFzJXXXv4eQ";
        static string SignatureMethod = "HMAC-SHA1";
        static string version = "1.0";

        public static string Consumer_secret {private get; set; }

        public static void GetAccessToken()
        {

        }

        public static void GetRequestToken()
        {
            string Timestamp = GetTimestamp();
            string Nonce = GetNonce(Timestamp);
            string SignatureBaseString = GetSignatureBaseString(Timestamp, Nonce);
            string SHA1 = GetSha1Hash(Consumer_secret, SignatureBaseString);

            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(@"https://api.twitter.com/oauth/request_token?oauth_callback=oob");
            httpWebRequest.Method = "POST";
            httpWebRequest.ContentLength = 0;
            httpWebRequest.UseDefaultCredentials = true;

            string Header =
                "OAuth realm=\"Twitter API\"," +
                "oauth_consumer_key=" + '"' + ConsumerKey + '"' + "," +
                "oauth_nonce=" + '"' + Nonce + '"' + "," +
                "oauth_version=" + '"' + version + '"' + "," +
                "oauth_signature_method=" + '"' + SignatureMethod + '"' + "," +
                "oauth_timestamp=" + '"' + Timestamp + '"' + "," +
                "oauth_signature = " + '"' + Uri.EscapeDataString(SHA1) + '"';


            httpWebRequest.Headers.Add("Authorization", Header);

            var webResponse = httpWebRequest.GetResponse();

            Stream responseStream = webResponse.GetResponseStream();
            string responseBody = String.Empty;
            if (responseStream != null) responseBody = new StreamReader(responseStream).ReadToEnd();

            oauth_token = ParseQuerystringParameter("oauth_token", responseBody);
            oauth_token_secret = ParseQuerystringParameter("oauth_token_secret", responseBody);
            oauth_verifier = ParseQuerystringParameter("oauth_verifier", responseBody);
        }

        static private string GetTimestamp() { return ((int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString(); }
        static private string GetNonce(string Timestamp) { return Convert.ToBase64String(Encoding.UTF8.GetBytes(Timestamp + Timestamp + Timestamp)); }
        private static string ParseQuerystringParameter(string parameterName, string text)
        {
            Match expressionMatch = Regex.Match(text, string.Format(@"{0}=(?<value>[^&]+)", parameterName));

            if (!expressionMatch.Success)
            {
                return string.Empty;
            }

            return expressionMatch.Groups["value"].Value;
        }

        public static string GetSha1Hash(string key, string message)
        {
            string Sha1Result = String.Empty;

            string keys = string.Format(
                CultureInfo.InvariantCulture,
                "{0}&", Uri.EscapeDataString(key));

            using (HMACSHA1 SHA1 = new HMACSHA1(Encoding.UTF8.GetBytes(keys)))
            {
                var Hashed = SHA1.ComputeHash(Encoding.UTF8.GetBytes(message));
                Sha1Result = Convert.ToBase64String(Hashed);
            }
            Console.WriteLine(Sha1Result);
            Console.WriteLine();
            return Sha1Result;
        }

        private static string GetSignatureBaseString(string timestamp, string nonce)
        {
            //1.Convert the HTTP Method to uppercase and set the output string equal to this value.
            string Signature_Base_String = "POST";
            Signature_Base_String = Signature_Base_String.ToUpper();

            //2.Append the ‘&’ character to the output string.
            Signature_Base_String = Signature_Base_String + "&";

            //3.Percent encode the URL and append it to the output string.
            string PercentEncodedURL = Uri.EscapeDataString(@"https://api.twitter.com/oauth/request_token");

            Signature_Base_String = Signature_Base_String + PercentEncodedURL;

            //4.Append the ‘&’ character to the output string.
            Signature_Base_String = Signature_Base_String + "&";

            //5.append parameter string to the output string.
            Signature_Base_String = Signature_Base_String + Uri.EscapeDataString(
                "oauth_callback=" + Callback +
                "&oauth_consumer_key=" + ConsumerKey +
                "&oauth_nonce=" + nonce +
                "&oauth_signature_method=" + SignatureMethod +
                "&oauth_timestamp=" + timestamp +
                "&oauth_version=" + version);
            
            return Signature_Base_String;
        }
    }
}
