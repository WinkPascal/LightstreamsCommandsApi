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


        static void Main(string[] args)
        {

            //  Console.WriteLine(createAccount("PascalWink1!"));
            string owner = "0x6c9Db8b7fC5A753C5DD41E8569371D22aa6C379a";
            string password = "PascalWink1";


            string data = "{'name' : 'PascalWink', " +
                         "'email' : 'PascalWink@gmail.com'}";
            string meta = "QmWpX3uK7sTqyb6gbbUnqVGKSp6GMjtZ46FDmJgGT214LA";
            string ext = "ext";
            string acl = "0xa23E343d450C1Cb5B3C7eD0900c5DA38e884b43c";

            //string to = "0xc887BB6E74761E39f19d4898eEE4908491C05322";  //lokaal op node 1 (smart hub)
            string to = "0x63A8273478bECf8F606897242A1f1E4E3F9F75Ea"; //op siebrand zijn pc node 1 


            //            Console.WriteLine(grantAccess(acl, owner, password, to, "read"));s
            Console.WriteLine("receiver balance: " + getBalance(to));
            Console.WriteLine("sender balance: " + getBalance(owner));
            Console.WriteLine("=======================================================");

            Console.WriteLine(transferBalance(owner, password, to, "64727502999997899998"));

            Console.WriteLine("=======================================================");
            Console.WriteLine("receiver balance: " + getBalance(to));
            Console.WriteLine("sender balance: " + getBalance(owner));
        }
        private static void LogFile(string accountid)
        {
            string[] a = System.IO.File.ReadAllLines(LogPath);
            Boolean exists = false;
            foreach (string line in a)
            {
                string[] account = Regex.Split(line, " || ");
                if (account[0] == accountid)
                {
                    exists = true;
                }
            }
            if (!exists)
            {
                a.Append(accountid + " || 0" );
            }
            Console.WriteLine("=======================================================");
            Console.WriteLine("=======================================================");
            Console.WriteLine("=======================================================");
            Console.WriteLine("=======================================================");
            Console.WriteLine("=======================================================");
            Console.WriteLine("=======================================================");
            Console.WriteLine("=======================================================");

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(LogPath)){

                foreach (string line in a)
                {
                    string[] account = Regex.Split(line, " || ");
                    account[1] = getBalance(account[0]);
                    file.WriteLine(account[0] + " || " + account[1]);
                    Console.WriteLine(account[0] + " || " + account[1]);
                }
            }
            File.Open(LogPath, FileMode.Open);
        }

        public static string transferBalance(string from, string password, string to, string PHT_amount)
        {
            //transfer balance to other account lightstreams
            String reqUrl = "http://localhost:9091/wallet/transfer";

            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            // Write request body
            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                String json = "{\"from\": \"" + from + "\", " +
                               "\"password\": \"" + password + "\", " +
                               "\"to\": \"" + to + "\", " +
                               "\"amount_wei\": \"" + PHT_amount + "\"}";
                streamWriter.Write(json);
            }

            // Get response body
            var autResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(autResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
//                LogFile(to);
          //      LogFile(from);
                return JObject.Parse(result).ToString();
            }
        }

        public static JObject update(string data, String ext, String owner, String meta)
        {
            //update new info in blockchain
            String reqUrl = "http://localhost:9091/storage/update-raw";

            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            // Write request body
            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                String json = "{\"data\": \"" + data + "\", " +
                   "\"ext\": \"" + ext + "\", " +
                   "\"owner\": \"" + owner + "\", " +
                   "\"meta\": \"" + meta + "\"}";
                streamWriter.Write(json);
            }

            // Get response body
            var autResponse = (HttpWebResponse)Req.GetResponse();
            using (var streamReader = new StreamReader(autResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                return JObject.Parse(result);
            }
        }

            private static String createToken(String account, String password)
        {
            // GENERATE AUTHENTICATION TOKEN WITH POST REQUEST
            // Declare variables
            String token;
            String autUrl = "http://localhost:9091/user/signin";

            // Set up request
            HttpWebRequest autReq = (HttpWebRequest)WebRequest.Create(autUrl);
            autReq.ContentType = "application/json";
            autReq.Method = "POST";

            // Write request body
            using (var streamWriter = new StreamWriter(autReq.GetRequestStream()))
            {
                String json = "{\"account\": \"" + account + "\"," +
                              "\"password\": \"" + password + "\"}";
                streamWriter.Write(json);
            }

            // Get response body
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
            // GET ACCOUNT BALANCE WITH GET REQUEST
            // Declare variables
            String balance;
            String balUrl = "http://localhost:9091/wallet/balance?account=" + account;

            // Set up request
            HttpWebRequest balReq = (HttpWebRequest)WebRequest.Create(balUrl);
            balReq.ContentType = "application/json";
            balReq.Method = "GET";

            // Get response body
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
            // CREATE ACCOUNT WITH POST REQUEST
            // Declare variables
            String account;
            String reqUrl = "http://localhost:9091/user/signup";

            // Set up request
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            // Write request body
            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                string json = "{\"password\": \"" + wachtwoord + "\"}";
                streamWriter.Write(json);
            }

            // Get response body
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
            // GET FILE WITH GET REQUEST
            // Declare variables
            String result;
            String getUrl = "http://localhost:9091/storage/stream?meta=" + meta + "&token=" + token;
            Debug.WriteLine(getUrl);
            Debug.WriteLine(meta);
            Debug.WriteLine(token);

            // Set up request
            HttpWebRequest getReq = (HttpWebRequest)WebRequest.Create(getUrl);
            getReq.ContentType = "application/json";
            getReq.Method = "GET";

            // Get response body
            var response = (HttpWebResponse)getReq.GetResponse();
            using (var streamReader = new StreamReader(response.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            return result;
        }

        private static Boolean grantAccess(String acl, String owner, String password, String to, String permission)
        {
            // GRANT ACCESS WITH POST REQUEST
            // Declare variables
            String resp;
            String reqUrl = "http://localhost:9091/acl/grant";

            // Set up request
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            // Write request body
            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                string json = "{\"acl\": \"" + acl + "\"," +
                    "\"owner\": \"" + owner + "\"," +
                    "\"password\": \"" + password + "\"," +
                    "\"to\": \"" + to + "\"," +
                    "\"permission\": \"" + permission + "\"}";
                streamWriter.Write(json);
            }

            // Get response body
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
            // GRANT ACCESS WITH POST REQUEST
            // Declare variables
            String resp;
            String reqUrl = "http://localhost:9091/storage/add-raw";

            // Set up request
            HttpWebRequest Req = (HttpWebRequest)WebRequest.Create(reqUrl);
            Req.ContentType = "application/json";
            Req.Method = "POST";

            // Write request body
            using (var streamWriter = new StreamWriter(Req.GetRequestStream()))
            {
                string json = "{\"data\": \"" + data + "\", " +
                    "\"ext\": \"" + ext + "\", " +
                    "\"owner\": \"" + owner + "\", " +
                    "\"password\": \"" + password + "\"}";
                streamWriter.Write(json);
            }
            
            // Get response body
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
