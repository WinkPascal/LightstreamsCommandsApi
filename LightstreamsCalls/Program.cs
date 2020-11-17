using System;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

using System.Text.RegularExpressions;
namespace LightstreamsCalls
{
    class Program
    {
        static string LogPath = @"logfile.txt";

        /*
        sign up) password => accountId
        sign in) username, password => token


        add-raw) data, ext, owner, password => meta, acl
        update-raw) data, ext, owner, meta => meta, acl

        stream) meta, token ==> file.txt

        grant) acl, owner, password, to, persmission => is_granted
        */

        // token is alleen nodig voor het streamen van een file 
        // inloggen in de smart vault is dus alleen nodig voor de data gatherer
        private static JArray accounts;
        private static string acl;
        private static string to;
        private static string owner;
        private static string data;
        private static string password;
        private static string meta;
        private static string ext;
        private static string permission;
        private static string amount;
        enum UseCase
        {
            transferBalance,
            update,
            createToken,
            getBalance,
            createAccount,
            readFile,
            grantAccess,
            addRaw
        }
        static void Main(string[] args)
        {
            accounts = new JArray();
            accounts.Add(new JObject { { "accountId", "0x63A8273478bECf8F606897242A1f1E4E3F9F75Ea" }, { "password", "" } }); // siebrand local node 1 
            accounts.Add(new JObject { { "accountId", "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a" }, { "password", "PascalWink1" } }); // pascal local node 1 

            switch (UseCase.transferBalance)
            {
                case UseCase.transferBalance:
                    owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
                    password = "PascalWink1";
                    to = "0x63A8273478bECf8F606897242A1f1E4E3F9F75Ea";
                    amount = "10000";

                    transferBalance(owner, password, to, amount);
                    break;

                case UseCase.update:
                    data = "{'name' : 'PascalWink', " +
                                "'email' : 'PascalWink@gmail.com'}";
                    ext = "ext";
                    owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
                    meta = "QmWpX3uK7sTqyb6gbbUnqVGKSp6GMjtZ46FDmJgGT214LA";

                    update(data, ext, owner, meta);
                    break;
                
                case UseCase.createToken:
                    owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
                    password = "PascalWink1";

                    createToken(owner, password);
                    break;
                
                case UseCase.getBalance:
                    owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
                    getBalance(owner);
                    break;
                
                case UseCase.createAccount:
                    password = "PascalWink1";

                    createAccount(password);
                    break;
                
                case UseCase.readFile:
                    owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
                    password = "PascalWink1";

                    string token = createToken(owner, password);
                    meta = "QmWpX3uK7sTqyb6gbbUnqVGKSp6GMjtZ46FDmJgGT214LA";

                    readFile(token, meta);
                    break;
                
                case UseCase.grantAccess:
                    acl = "0xa23E343d450C1Cb5B3C7eD0900c5DA38e884b43c";
                    owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
                    password = "PascalWink1";
                    to = "0x63A8273478bECf8F606897242A1f1E4E3F9F75Ea";
                    permission = "noaccess";

                    grantAccess(acl, owner, password, to, permission);
                    break;
                
                case UseCase.addRaw:
                    data = "{'name' : 'PascalWink', " +
                            "'email' : 'PascalWink@gmail.com'}";
                    ext = "ext";
                    owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
                    password = "PascalWink1";

                    addRaw(data, ext, owner, password);
                    break;
            }
        }
        
        private static JObject getAccount(String accountId)
        {

            foreach (JObject account in accounts)
            {
                if (account.GetValue("accountId").ToString().Equals(accountId))
                {
                    return account;
                }
            }
            return null;
        }


        public static string transferBalance(string from, string password, string to, string PHT_amount)
        {
            String reqUrl = "http://localhost:9091/wallet/transfer";

            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                String json = "{\"from\": \"" + from + "\", " +
                               "\"password\": \"" + password + "\", " +
                               "\"to\": \"" + to + "\", " +
                               "\"amount_wei\": \"" + PHT_amount + "\"}";
                streamWriter.Write(json);
            }

            var autResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(autResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return JObject.Parse(result).ToString();
            }
        }

        public static JObject update(string data, String ext, String owner, String meta)
        {
            String reqUrl = "http://localhost:9091/storage/update-raw";

            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                String json = "{\"data\": \"" + data + "\", " +
                   "\"ext\": \"" + ext + "\", " +
                   "\"owner\": \"" + owner + "\", " +
                   "\"meta\": \"" + meta + "\"}";
                streamWriter.Write(json);
            }

            var autResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(autResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return JObject.Parse(result);
            }
        }

            private static String createToken(String account, String password)
        {
            String token;
            String autUrl = "http://localhost:9091/user/signin";

            HttpWebRequest autReq = (HttpWebRequest)WebRequest.Create(autUrl);
            autReq.ContentType = "application/json";
            autReq.Method = "POST";

            using (var streamWriter = new StreamWriter(autReq.GetRequestStream()))
            {
                String json = "{\"account\": \"" + account + "\"," +
                              "\"password\": \"" + password + "\"}";
                streamWriter.Write(json);
            }

            try
            {
                var autResponse = (HttpWebResponse)autReq.GetResponse();
                using (var streamReader = new StreamReader(autResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    JObject tokenJson = JObject.Parse(result);
                    token = tokenJson["token"].ToString();
                }

                return token;
            }
            catch (Exception e)
            {
                return "Error " + e;
            }
        }
        private static String getBalance(String account)
        {
            String balance;
            String balUrl = "http://localhost:9091/wallet/balance?account=" + account;

            HttpWebRequest balReq = (HttpWebRequest)WebRequest.Create(balUrl);
            balReq.ContentType = "application/json";
            balReq.Method = "GET";

            var balResponse = (HttpWebResponse)balReq.GetResponse();
            using (var streamReader = new StreamReader(balResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                JObject balanceJson = JObject.Parse(result);
                balance = balanceJson["balance"].ToString();
            }

            return balance;
        }

        private static String createAccount(String wachtwoord)
        {
            String account;
            String reqUrl = "http://localhost:9091/user/signup";

            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                string json = "{\"password\": \"" + wachtwoord + "\"}";
                streamWriter.Write(json);
            }

            var reqResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(reqResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                JObject accJson = JObject.Parse(result);
                account = accJson["account"].ToString();
            }

            return account;
        }

        private static String readFile(String token, String meta)
        {
            String result;
            String getUrl = "http://localhost:9091/storage/stream?meta=" + meta + "&token=" + token;
            Debug.WriteLine(getUrl);
            Debug.WriteLine(meta);
            Debug.WriteLine(token);

            HttpWebRequest getReq = (HttpWebRequest)WebRequest.Create(getUrl);
            getReq.ContentType = "application/json";
            getReq.Method = "GET";

            var response = (HttpWebResponse)getReq.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        private static Boolean grantAccess(String acl, String owner, String password, String to, String permission)
        {
            String resp;
            String reqUrl = "http://localhost:9091/acl/grant";

            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                string json = "{\"acl\": \"" + acl + "\"," +
                    "\"owner\": \"" + owner + "\"," +
                    "\"password\": \"" + password + "\"," +
                    "\"to\": \"" + to + "\"," +
                    "\"permission\": \"" + permission + "\"}";
                streamWriter.Write(json);
            }

            var reqResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(reqResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                JObject json = JObject.Parse(result);
                resp = json["is_granted"].ToString();

            }
            return true;
        }

        private static Boolean addRaw (String data, String ext, String owner, String password)
        {
            String resp;
            String reqUrl = "http://localhost:9091/storage/add-raw";

            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                string json = "{\"data\": \"" + data + "\", " +
                    "\"ext\": \"" + ext + "\", " +
                    "\"owner\": \"" + owner + "\", " +
                    "\"password\": \"" + password + "\"}";
                streamWriter.Write(json);
            }
            
            var reqResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(reqResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                JObject json = JObject.Parse(result);
                resp = json["meta"].ToString();
                resp = json["acl"].ToString();
            }
            return true;
        }
    }
}
