using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace CopyItems
{
    public class MyWebBrowser
    {
        public MyWebBrowser()
        { }

        /// <summary>
        /// 开始一个抓图并返回图象
        /// </summary>
        /// <param name="Url">要抓取的网页地址</param>
        /// <returns></returns>
        public string StartSnap(string Url)
        {
            WebBrowser myWB = this.GetPage(Url);
            string newurl = myWB.Document.Url.ToString();
            StreamReader sr = new StreamReader(myWB.DocumentStream, Encoding.GetEncoding(myWB.Document.Encoding));
            myWB.Dispose();
            string d = sr.ReadToEnd();
            return d; 
        }

        public WebBrowser GetPage(string Url)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls;
            WebBrowser myWB = new WebBrowser();
            myWB.ScrollBarsEnabled = false;
            myWB.ScriptErrorsSuppressed = true;
            myWB.Navigate(Url);
            while (myWB.ReadyState != WebBrowserReadyState.Complete)
            {
                System.Windows.Forms.Application.DoEvents();
                TxtLog.WriteLine("请求失败", Url + "请求失败"); 
            }
            TxtLog.WriteLine("请求失败", Url + "请求成功");
            return myWB;
        }

    }
}
