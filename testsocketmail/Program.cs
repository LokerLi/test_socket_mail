using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace testsocketmail
{
    class Program
    {
        static void Main(string[] args)
        {
            sendmail sm = new sendmail();
            sm.sendall();
            Console.Read();
        }
    }
    class sendmail
    {
        Socket socket;
        string resp = "";
        int state = -1;

        public string base64(string s)
        {
           return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(s));
        }
        private void req(string msg)
        {
             var data = Encoding.UTF8.GetBytes(msg+"\r\n");

            socket.Send(data, data.Length, SocketFlags.None);
            data = new byte[1024];
            //recv data len
            var recv = socket.Receive(data);
            //convert to string
            resp = Encoding.UTF8.GetString(data, 0, recv);
            Console.WriteLine("------------");

            Console.Write(resp);
            var st = System.Text.RegularExpressions.Regex.Match(resp, @"\d{3} ").Value;
            Console.WriteLine(st);
            if (!string.IsNullOrEmpty(st))
            {
                state = Convert.ToInt32(st);
            }
            
        }
        public void sendall()
        {
            var host = "smtp.exmail.qq.com";
            var from="server@xxxx.cn";
            var to="server@xxx.cn;loker@xxxx.cn";
            var user=from;
            var pwd = "xxxxx";

            IPAddress ip = Dns.GetHostAddresses(host)[0];
            IPEndPoint ipEnd = new IPEndPoint(ip, 25);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                socket.Connect(ipEnd);
            }
            catch (SocketException e)
            {
                Console.Write("Fail to connect server");
                Console.Write(e.ToString());
                return;
            }
            req("EHLO " + host);
            req("AUTH LOGIN");
            req(base64(user));
            req(base64(pwd));
            req("MAIL From:"+from);
            //235 need to quit socker
            if (state!=235)
            {
                req("QUIT");
                socket.Close();
                return;
            }
            foreach(var t in to.Split(new char[]{';'},StringSplitOptions.RemoveEmptyEntries))
                req("RCPT To:" + t);
            req("DATA");
            var data1 = string.Format(@"From: {0}
To: {1}
Date: {2}
Subject: {3}
X-Mailer: Loker.Lua.SmtpServer
Content-type:text/plain;Charset=utf8

{4}{5}", 
            from,
            to,
            DateTime.Now.ToString("M"),
            "Subject",
            "body",
            "\r\n");
            var data = Encoding.UTF8.GetBytes(data1);
            //send the data
            socket.Send(data, data.Length, SocketFlags.None);
            req(".");
            req("QUIT");
            socket.Close();
            
        }
    }
}
