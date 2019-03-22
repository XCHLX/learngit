using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;
using Top.Api.Response;
using Top.Api.Request;
using System.Net.Security;
using Top.Api.Util;
using System.IO;
using System.Windows.Forms;
using DbAccess;
using System.Data;
using Top.Api;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
//using GoodsTaobaoCommon;
using TaobaoCommon;

namespace CopyItems
{

    public class Copy
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title = string.Empty;
        /// <summary>
        /// 价格
        /// </summary>
        public double price;
        /// <summary>
        /// 尺寸,颜色
        /// </summary>
        public string size = string.Empty;
        /// <summary>
        /// 活动
        /// </summary>
        public string activity = string.Empty;
        /// <summary>
        /// 店铺网址
        /// </summary>
        public string shopurl = string.Empty;
        /// <summary>
        /// 分类id
        /// </summary>
        public string parentid = string.Empty;
        /// <summary>
        /// 总页码
        /// </summary>
        public string pageNum = string.Empty;
        /// <summary>
        /// id集合
        /// </summary>
        public string ids = string.Empty;
        /// <summary>
        /// 库存
        /// </summary>
        public string num = "0";
        /// <summary>
        /// 宝贝详情
        /// </summary>
        public string desc = string.Empty;
        /// <summary>
        /// 生产许可证号
        /// </summary>
        public string prd_license_no = string.Empty;
        /// <summary>
        /// 产品标准号
        /// </summary>
        public string design_code = string.Empty;
        /// <summary>
        /// 厂名
        /// </summary>
        public string factory = string.Empty;
        /// <summary>
        /// 厂址
        /// </summary>
        public string factory_site = string.Empty;
        /// <summary>
        /// 厂家联系方式
        /// </summary>
        public string contact = string.Empty;
        /// <summary>
        /// 配料表
        /// </summary>
        public string mix = string.Empty;
        /// <summary>
        /// 储藏方法
        /// </summary>
        public string plan_storage = string.Empty;
        /// <summary>
        /// 保质期
        /// </summary>
        public string period = string.Empty;
        /// <summary>
        /// 食品添加剂
        /// </summary>
        public string food_additive = string.Empty;
        /// <summary>
        /// 供货商
        /// </summary>
        public string supplier = "无";
        /// <summary>
        /// 生产开始日期
        /// </summary>
        public string product_date_start = string.Empty;
        /// <summary>
        /// 生产结束日期
        /// </summary>
        public string product_date_end = string.Empty;
        /// <summary>
        /// 进货开始日期
        /// </summary>
        public string stock_date_start = System.DateTime.Now.ToString("yyyy-MM-dd");
        /// <summary>
        /// 进货结束日期
        /// </summary>
        public string stock_date_end = System.DateTime.Now.ToString("yyyy-MM-dd");

        public string childnamepids = string.Empty;
        public string _pids = string.Empty;
        public string _str = string.Empty;
        public string tpkj = string.Empty;
        public string cid = string.Empty;
        public string sessionKey;
        public int useplatform;
        public DataTable dt = new DataTable();
        public string input_str = string.Empty;
        public string input_pids = string.Empty;
        public string remarks = string.Empty;
        //public Form1 f = new Form1();
        //public Form1.SetTextCallback s = new Form1.SetTextCallback(f.showlist);
        public void AddTaobao(string id, string oldid, string sellderid, string txtnum)
        {
            //sessionKey = "6200304676afhj707ZZa49af4c100ffc7aa2d3aa741d459550825126";
            remarks = string.Empty;
            string url = "https://detail.tmall.com/item.htm?id=" + id + "";//输入id拼接url 
            tpkj = dt.Rows[0]["tpkj"].ToString();
            MyWebBrowser my = new MyWebBrowser();
            WebBrowser wb = my.GetPage(url);
            string newurl = wb.Document.Url.ToString();//获取真实的url
            StreamReader sr = new StreamReader(wb.DocumentStream, Encoding.GetEncoding(wb.Document.Encoding));
            wb.Dispose();
            string html = sr.ReadToEnd();//获取页面源代码
            //html = GetHtml(url);
            #region 修改标题
            title = GetTitle(newurl, html);
            title = dt.Rows[0]["firsttitle"].ToString() + title + dt.Rows[0]["lasttitle"].ToString();
            if (string.IsNullOrWhiteSpace(title))
            {
                remarks += "【获取宝贝信息失败 ,当前宝贝不支持复制！】";
                //f.showlist(remarks,sellderid);
                updateChildCopy(id, remarks);
            }
            else
            {
                string dgjc = dt.Rows[0]["dgjc"].ToString();//需要删除的关键词
                if (!string.IsNullOrWhiteSpace(dgjc))
                {
                    title = title.Replace(dgjc, "");
                }
                string oldgjc = dt.Rows[0]["oldgjc"].ToString();//需要替换的关键词
                string newgjc = dt.Rows[0]["newgjc"].ToString();//替换后的关键词
                if (!string.IsNullOrWhiteSpace(oldgjc))
                {
                    title = title.Replace(oldgjc, newgjc);
                }
                if (Encoding.GetEncoding("GB2312").GetBytes(title).Length > 60)
                {
                    title = title.Substring(0, 30);
                }
                //f.showlist("【宝贝id："+id+"】标题已复制",sellderid);
                #endregion
            #region 修改价格
                price = Convert.ToDouble(GetPrice(newurl, html, id, sellderid));
                string bili = dt.Rows[0]["bili"].ToString();
                string addmoney = dt.Rows[0]["addmoney"].ToString();
                if (!string.IsNullOrWhiteSpace(bili))
                {//是否有百分比
                    price = price * Convert.ToDouble(bili) / 100;
                }
                if (!string.IsNullOrWhiteSpace(addmoney))//是否由额外金额
                {
                    price = price + Convert.ToDouble(addmoney);
                }
                if (dt.Rows[0]["fj"].ToString() == "0")//是否保留分角0:不保留，1;保留
                {
                    price = Math.Floor(Convert.ToDouble(price));
                }
                #endregion
            #region 宝贝详情去除关键词，替换关键词
                desc = System.Web.HttpUtility.HtmlDecode(GetDetails(newurl, html));
                Regex r = new Regex(@"<a[^>]*>|</a>$");
                desc = r.Replace(desc, "");
                desc = desc.Replace("可瑞特", "");
                try
                {
                    desc = GetSrc(desc, id, sellderid);
                }
                catch (Exception ex)
                {
                    remarks += "【图片空间异常！】";
                    //f.showlist("【宝贝id：" + id + "】图片空间异常",sellderid);
                    updateChildCopy(id, remarks);
                    return;
                }
                if (string.IsNullOrWhiteSpace(desc))
                {
                    remarks += "【宝贝详情设置错误！】";
                    updateChildCopy(id, remarks);
                    return;
                }
                //desc += "      ";
                if (desc.Length > 24999)
                {
                    desc = desc.Substring(0, 24999);
                }
                if (desc.Length < 6)
                {
                    desc += "待补充描述详情！";
                }
                desc += "　　　　　";
                //writlog("desc",desc.ToString());
                #endregion
            #region 获取cid
                cid = GetCid(html);
                if (string.IsNullOrWhiteSpace(cid))
                {
                    //f.showlist("【宝贝id：" + id + "】cid获取错误",sellderid);
                    updateChildCopy(id, remarks);
                    return;
                }
                #endregion
            #region 获取卖家城市，省份
                #endregion
            #region props获取属性相关的值,属性不一致的代码部分
                string props = GetProps(html, cid, newurl, txtnum);
                #endregion
            #region 添加宝贝
                ItemAddRequest req = new ItemAddRequest();
                req.Title = title;//标题
                req.Price = Math.Round(price, 2).ToString();//价格
                req.Desc = desc.Replace("\\", "");//描述
                //req.Desc = "暂无咱去！";
                req.LocationCity = "常州";//城市
                req.LocationState = "江苏";//省份
                req.StuffStatus = "new";//新旧程度
                req.Cid = Convert.ToInt64(cid);//
                if (!string.IsNullOrWhiteSpace(dt.Rows[0]["bbfl"].ToString()))
                {
                    req.SellerCids = dt.Rows[0]["bbfl"].ToString();//商品类目
                }
                req.IsXinpin = true;
                req.Num = 30;//库存
                req.Type = "fixed";//类型
                req.ApproveStatus = "instock";//状态,默认仓库中
                req.InputPids = input_pids;//"154054574,154032675"
                req.InputStr = input_str;//"10m,5天"input_str
                req.Props = props;
                if (!string.IsNullOrWhiteSpace(dt.Rows[0]["yfmb"].ToString()))
                {
                    req.PostageId = Convert.ToInt64(dt.Rows[0]["yfmb"]);
                }
                req.FoodSecurityContact = contact;
                req.FoodSecurityDesignCode = design_code;
                req.FoodSecurityFactory = factory;
                req.FoodSecurityFactorySite = factory_site;
                req.FoodSecurityFoodAdditive = food_additive;
                req.FoodSecurityMix = mix;
                req.FoodSecurityPeriod = period;
                req.FoodSecurityPlanStorage = plan_storage;
                req.FoodSecurityPrdLicenseNo = prd_license_no;
                if (cid != "50016792")
                {
                    req.FoodSecurityProductDateStart = product_date_start;
                    req.FoodSecurityProductDateEnd = product_date_end;
                    req.FoodSecurityStockDateEnd = stock_date_end;
                    req.FoodSecurityStockDateStart = stock_date_start;
                    req.FoodSecuritySupplier = supplier;
                }
                ItemAddResponse response_req = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                writlog("response_req", response_req.ErrMsg);
                if (response_req.Item == null)
                {
                    if (!string.IsNullOrWhiteSpace(response_req.SubErrMsg))
                    {
                        remarks += response_req.SubErrMsg.ToString();
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(response_req.ErrMsg))
                        {
                            remarks += response_req.ErrMsg.ToString();
                        }
                        else
                        {
                            remarks += "【接口异常!】";
                            //f.showlist("【宝贝id：" + id + "】接口异常",sellderid);
                        }
                    }
                }
                else
                {
                    try
                    {
                        AddPic(response_req.Item.NumIid.ToString(), html, newurl);
                        if (dt.Rows[0]["sjxq"].ToString() == "0" && dt.Rows[0]["shoptype"].ToString() == "0")
                        {
                            string mainid = Guid.NewGuid().ToString();
                            MySqlHelper sql1 = new MySqlHelper("ToolsProducts", DbAccess.DataType.tools);
                            //MySqlHelper sql1 = new MySqlHelper("GoodsProducts");
                            sql1.ExecuteNonQuery("insert into crm_mobiledetailmqs(id,seller_id,status,create_time) values('" + mainid + "','" + sellderid + "','0','" + DateTime.Now.ToString() + "') ");
                            sql1.ExecuteNonQuery("insert into dm_mobiledetail_" + GetTableIndex(sellderid) + "(id,seller_id,num_iid,mobile_state,last_time) values('" + Text.GetHalfUnixID() + "','" + sellderid + "','" + response_req.Item.NumIid.ToString() + "','0','" + DateTime.Now.ToString() + "') ");
                            ToolsCenterCommon.ToolsCenter.SaveOnceTask(ToolsCenterCommon.ToolsCenter.Funtype.phonedetail, mainid + ",single");
                        }
                    }
                    catch (Exception ex)
                    {
                        //f.showlist("【宝贝id：" + id + "】主图上传失败!",sellderid);
                    }
                    //GetTaoBaoSku(html, response_req.Item.NumIid.ToString());
                }
                if (!string.IsNullOrWhiteSpace(remarks) && response_req.Item == null)
                {
                    updateChildCopy(id, remarks);
                }
                else
                {
                    updateChildCopys(response_req.Item.NumIid.ToString(), oldid, title);
                }
            }
            #endregion
        }

        public int updateChildCopy(string id, string remarks)
        {
            if (remarks.Length > 4000)
            {
                remarks = remarks.Substring(0, 3000);
            }
            MySqlHelper sql = new MySqlHelper("ToolsProducts", DbAccess.DataType.tools);
            //MySqlHelper sql = new MySqlHelper("GoodsProducts");
            int res = sql.ExecuteNonQuery("update crm_productscopychild  set remark='" + System.Web.HttpUtility.HtmlEncode(remarks) + "',state='2' where bbid='" + id + "' ");
            return res;
        }

        public void writlog(string name, string desc)
        {
            string sFilePath = "d:\\desc\\" + DateTime.Now.ToString("yyyyMMdd");
            string sFileName = name + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            sFileName = sFilePath + "\\" + sFileName; //文件的绝对路径
            if (!Directory.Exists(sFilePath))//验证路径是否存在
            {
                Directory.CreateDirectory(sFilePath);
                //不存在则创建
            }
            FileStream fs;
            StreamWriter sw;
            if (File.Exists(sFileName))
            //验证文件是否存在，有则追加，无则创建
            {
                fs = new FileStream(sFileName, FileMode.Append, FileAccess.Write);
            }
            else
            {
                fs = new FileStream(sFileName, FileMode.Create, FileAccess.Write);
            }
            sw = new StreamWriter(fs);
            sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "   ---   " + desc);
            sw.Close();
            fs.Close();
        }
        public int updateChildCopys(string id, string oldid, string title)
        {
            MySqlHelper sql = new MySqlHelper("ToolsProducts", DbAccess.DataType.tools);
            //MySqlHelper sql = new MySqlHelper("GoodsProducts");
            title = title.Replace("'","");
            int res = sql.ExecuteNonQuery("update crm_productscopychild  set oldtitle='" + title + "', newid='" + id + "',state='1' where id='" + oldid + "' ");
            return res;
        }


        /// <summary>
        /// 获取活动详情
        /// </summary>
        /// <param name="url"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetActivity(string url, string html)
        {
            if (CheckTmallOrTaobao(url))
            {
                activity = MatchValue(html, "<p>", "</p>").ToString().Trim();
            }
            else
            {
                activity = MatchValue(html, " <p class=\"tb-subtitle\">", "</p>").ToString().Trim();
            }
            return activity;
        }
        /// <summary>
        /// 获取标题
        /// </summary>
        /// <returns></returns>
        private string GetTitle(string url, string html)
        {
            if (CheckTmallOrTaobao(url))
            {
                title = MatchValue(html, "\"title\":\"", "\"").ToString().Trim();
            }
            else
            {
                title = MatchValue(html, "data-title=\"", "\"").ToString().Trim();
            }
            return title;
        }
        /// <summary>
        /// 获取价格
        /// </summary>
        /// <returns></returns>
        private double GetPrice(string url, string html, string id, string sid)
        {
            string a = string.Empty;
            if (CheckTmallOrTaobao(url))
            {
                a = MatchValue(html, "\"price\":\"", "\"").ToString().Trim();
                if (string.IsNullOrWhiteSpace(a))
                {
                    a = MatchValue(html, "\"defaultItemPrice\":\"", "\"").ToString().Trim();
                }
            }
            else
            {
                a = MatchValue(html, "<em class=\"tb-rmb-num\">", "</em>").ToString().Trim();

            }
            if (a.Contains("-"))
            {
                a = (a.Split('-'))[0].ToString();
            }
            try
            {
                price = Convert.ToDouble(a);
            }
            catch (Exception e)
            {
                price = 0;
                //f.showlist("【宝贝id：" + id + "】价格获取失败",sid);

            }
            return price;
        }
        /// <summary>
        /// 判断天猫还是淘宝店铺
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private bool CheckTmallOrTaobao(string url)
        {
            if (url.Contains("tmall"))
            {

                return true;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// 淘宝店铺抓取信息
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetByTaobao(string html)
        {
            activity = MatchValue(html, " <p class=\"tb-subtitle\">", "</p>").ToString().Trim();
            title = MatchValue(html, "data-title=\"", "\"").ToString().Trim();
            price = Convert.ToDouble(MatchValue(html, "<em class=\"tb-rmb-num\">", "</em>").ToString().Trim());
            return title + "|" + activity + "|" + price; ;
        }

        /// <summary>
        /// 天猫店铺抓取信息
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        private string GetByTianMao(string html)
        {
            activity = MatchValue(html, "<p>", "</p>").ToString().Trim();
            title = MatchValue(html, "\"title\":\"", "\"").ToString().Trim();
            price = Convert.ToDouble(MatchValue(html, "\"price\":\"", "\"").ToString().Trim());
            return title + "|" + activity + "|" + price;
        }

        /// <summary>
        /// 获取宝贝详情
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        private string GetDetails(string url, string html)
        {
            //string itemhtml = GetHtml(url);
            string itemhtml = html;
            string detailurl = string.Empty;
            if (CheckTmallOrTaobao(url))
            {
                detailurl = MatchValue(itemhtml, "\"descUrl\":\"//", "\",");//获取宝贝详情链接

            }
            else
            {
                detailurl = MatchValue(itemhtml, "//desc.alicdn.com", "',");//获取宝贝详情链接
                detailurl = "desc.alicdn.com" + detailurl;
            }
            MyWebBrowser my = new MyWebBrowser();
            WebBrowser wb = my.GetPage(detailurl);
            string newurl = wb.Document.Url.ToString();//获取真实的url
            StreamReader sr = new StreamReader(wb.DocumentStream, Encoding.GetEncoding(wb.Document.Encoding));
            wb.Dispose();
            string itmedetailhtml = sr.ReadToEnd();
            //string itmedetailhtml = GetHtml("https://" + detailurl);
            string itemdetail = MatchValue(itmedetailhtml, "var desc='", "';");//获取宝贝详情实际源代码
            return itemdetail;
        }



        public bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {   // 总是接受  
            return true;
        }
        /// <summary>
        /// 获取带宝贝详情的页面源代码
        /// </summary>
        /// <param name="Url"></param>
        /// <returns></returns>
        public string GetHtml(string Url)
        {
            string str = "";

            ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
            //ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Url);//ServiceEntry.GetRadomCompany((UserConfig)this.Tag));
            request.Method = "GET";
            request.ServicePoint.ConnectionLimit = 5;
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT6.1)";
            request.Headers["Accept-Language"] = "zh-cn";
            request.Timeout = 5000;
            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
            {

                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.ContentLength < 1024 * 1024)
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("GB2312")))
                    {
                        str = sr.ReadToEnd();
                    }
                }

            }
            return str;
        }

        /// <summary>
        /// 获取宝贝id集合
        /// </summary>
        /// <returns></returns>
        public void GetIds(string url)
        {
            //string url1 = "https:/" + shopurl + "//i/asynSearch.htm?mid=w-2423736466-0&wid=2423736466&path=/search.htm&search=y&parentcid=" + parentid + "&pageNum=" + pageNum + "#anchor";
            string allurl = url + "/search.htm";//所有商品url
            MyWebBrowser my = new MyWebBrowser();
            WebBrowser wb = my.GetPage(url);
            string newurl = wb.Document.Url.ToString();//获取真实的url
            wb.Dispose();
            WebBrowser wb1 = my.GetPage(allurl);
            StreamReader sr = new StreamReader(wb1.DocumentStream, Encoding.GetEncoding(wb1.Document.Encoding));
            string html = sr.ReadToEnd();
            wb1.Dispose();
            //string html = GetHtml(allurl);
            string shopurl = MatchValue(html, "value=\"/i/asynSearch.htm", "\"");//获取宝贝所有idurl

            string pagehtmlurl = url + "/i/asynSearch.htm" + shopurl;
            //WebBrowser wb2 = my.GetPage(pagehtmlurl);
            //StreamReader sr1 = new StreamReader(wb2.DocumentStream, Encoding.GetEncoding(wb2.Document.Encoding));
            //string pagehtml = sr1.ReadToEnd();
            //wb2.Dispose();
            string pagehtml = GetHtml(pagehtmlurl);

            string b = string.Empty;
            //if (newurl.Contains("tmall"))
            //{
            //    b = MatchValue(pagehtml, ">1/", "</b>");
            //}
            //else if (newurl.Contains("taobao"))
            //{
            //    b = MatchValue(pagehtml, ">1/", "</span>");
            //}
            //else if (newurl.Contains("jiyoujia"))
            //{
            //    b = MatchValue(pagehtml, ">1/", "</span>");
            //}
            b = MatchValue(pagehtml, "共搜索到<span>", "</span>");
            try { int ib = Convert.ToInt32(b); b = (ib / 24 + 2).ToString(); }
            catch (Exception ex) { b = "1"; }
            for (int j = 1; j < Convert.ToInt32(b); j++)
            {
                string pageshopurl = url + "/i/asynSearch.htm" + shopurl + "&pageNum=" + j;
                WebBrowser wb3 = my.GetPage(pageshopurl);
                StreamReader sr2 = new StreamReader(wb3.DocumentStream, Encoding.GetEncoding(wb3.Document.Encoding));
                string shophtml = sr2.ReadToEnd();
                wb3.Dispose();
                //string shophtml = GetHtml(pageshopurl);
                MatchCollection obj = MatchValues(shophtml, "data-id=", ">");
                MatchCollection obj2 = MatchValues(shophtml, "item.taobao.com/item.htm", "&");
                if (obj.Count == 0 && obj2.Count == 0)
                {
                    break;
                }
                else
                {
                    for (int i = 0; i < obj.Count; i++)//拼接宝贝id
                    {
                        string nid = obj[i].ToString().Replace(@"\", "");
                        nid = nid.Replace("\"", "");
                        if (!string.IsNullOrWhiteSpace(ids))
                        {
                            if (!ids.Contains(nid))
                            {
                                ids = ids + "," + nid;
                            }
                        }
                        else
                        {
                            ids = nid;
                        }
                    }
                    if (obj.Count == 0 && obj2.Count != 0)
                    {
                        for (int i = 0; i < obj2.Count; i++)//拼接宝贝id
                        {
                            string nid = obj2[i].ToString().Replace("?id=", "");
                            if (!string.IsNullOrWhiteSpace(ids))
                            {
                                if (!ids.Contains(nid))
                                {
                                    ids = ids + "," + nid;
                                }
                            }
                            else
                            {
                                ids = nid;
                            }
                        }
                    }

                }
            }

        }



        /// <summary>
        /// 获取天猫店铺sku信息
        /// </summary>
        /// <returns></returns>
        public string GetTianMaoSku(string html)
        {
            string skulist = MatchValue(html, "\"skuList\":", ",\"skuMap\"");//skulist:json格式
            string skumap = MatchValue(html, ",\"skuMap\"", "},\"valLoginIndicator\"");
            return "";
        }

        /// <summary>
        /// 获取淘宝店铺sku信息并添加
        /// </summary>
        /// <returns></returns>
        public string GetTaoBaoSku(string html, string id)
        {
            string skumap = MatchValue(html, "skuMap     : ", "}}");
            skumap = skumap + "}}";
            DataTable dt = ToDataTable(skumap, id);
            return "";
        }

        public string GetSkuid(string html)
        {
            MatchCollection obj = MatchValues(html, "data-id=", ">");
            return "";
        }

        /// <summary>
        /// 获取类目id
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string GetCid(string html)
        {
            html = html.Replace(" ", "");
            string obj = MatchValue(html, "\ncid:'", "'").ToString();
            if (string.IsNullOrWhiteSpace(obj))
            {
                obj = MatchValue(html, "categoryId:", ",").ToString();
            }
            return obj.ToString(); ;
        }

        /// <summary>
        /// 更新宝贝详情中图片url
        /// </summary>
        /// <param name="html"></param>
        public string GetSrc(string html, string id, string sid)
        {
            //获取图片原来地址
            //https://assets.alicdn.com/sys/wangwang/smiley/48x48/32.gif
            Regex secondregex_jpg = new Regex(@"(_[\d]+x[\d]+\.jpg)", RegexOptions.Singleline | RegexOptions.IgnoreCase);//jpg图片
            Regex secondregex_gif = new Regex(@"(_[\d]+x[\d]+\.gif)", RegexOptions.Singleline | RegexOptions.IgnoreCase);//gif图片
            MatchCollection secondmatches = MatchValues(html, "=\"", "\"");
            string pid = string.Empty;
            try
            {
                if (tpkj == "0")
                {
                    pid = AddPicType();
                }
                else
                {
                    pid = tpkj;
                }
            }
            catch (Exception e)
            {

            }
            if (!string.IsNullOrWhiteSpace(pid))
            {
                if (secondmatches.Count > 0)
                {
                    for (int i = 0; i < secondmatches.Count; i++)
                    {
                        string childs = secondmatches[i].ToString();
                        if (i == 506)
                        {
                            bool b = true;
                        }
                        if (!childs.Contains("?") && (childs.Contains(".jpeg") || childs.Contains(".jpg") || childs.Contains(".png") || childs.Contains(".gif")))
                        {
                            try
                            {
                                html = downpic(childs, html, id, pid);
                            }
                            catch (Exception ex)
                            {
                                html = html.Replace(childs, "");
                            }
                        }
                        else if (childs.Contains("?") && childs.Contains("http"))
                        {
                            html = html.Replace(childs, "");
                        }

                    }
                }
                MatchCollection secondmatches1 = secondregex_gif.Matches(html);
                if (secondmatches1.Count > 0)
                {
                    for (int i = 0; i < secondmatches1.Count; i++)
                    {
                        string childs = secondmatches1[i].ToString();
                        if (!childs.Contains("?") && childs.Contains("http"))
                        {
                            html = downpic(childs, html, id, pid);
                        }
                        else if (childs.Contains("?") && childs.Contains("http"))
                        {
                            html = html.Replace(childs, "");
                        }
                    }
                }
            }
            return html;
        }

        /// <summary>
        /// 将图片上传至店铺图片服务器中,返回新的图片url并替换原有的url
        /// </summary>
        /// <param name="url"></param>
        /// <param name="html"></param>
        /// <returns></returns>
        public string downpic(string url, string html, string id, string pid)
        {
            id = "524050172820";
            string path = string.Empty;
            string type = string.Empty;
            string realurl = string.Empty;
            if (!url.Contains("https:"))
            {
                realurl = "https:" + url;
            }
            else
            {
                realurl = url;
            }
            WebClient mywebclient = new WebClient();
            if (!url.Contains("assets.alicdn.com"))//阿里图片表情不作处理
            {
                string[] urlchild = url.Split('/');
                string name = urlchild[urlchild.Length - 1].ToString();
                if (url.Contains(".jpg"))
                {
                    name = name.Replace(".jpg", "");
                    type = ".jpg";
                }
                else if (url.Contains(".gif"))
                {
                    name = name.Replace(".gif", "");
                    type = ".gif";
                }
                else if (url.Contains(".png"))
                {
                    name = name.Replace(".png", "");
                    type = ".png";
                }
                if (url.Contains(".jpeg"))
                {
                    name = name.Replace(".jpeg", "");
                    type = ".jpeg";
                }

                try
                {
                    //image = System.Drawing.Image.FromStream(webresponse.GetResponseStream());
                    path = @"D:\" + name + type;
                    mywebclient.DownloadFile(realurl, path);
                    //filename = newfilename;
                    //image.Save(path); //保存在本地文件夹  
                    //image.Dispose(); //释放资源 
                }
                catch (Exception ex)
                {
                }
                try
                {
                    //上传图片
                    PictureUploadRequest req = new PictureUploadRequest();
                    req.PictureCategoryId = Convert.ToInt64(pid);
                    req.Img = new FileItem(path);
                    req.ImageInputTitle = name;
                    req.IsHttps = true;
                    PictureUploadResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                    if (rsp.Picture != null)
                    {
                        html = html.Replace(url.ToString().Trim(), @rsp.Picture.PicturePath);
                    }
                    else
                    {
                        remarks += "[" + rsp.SubErrMsg.ToString() + "]";
                    }
                    System.IO.FileInfo file = new System.IO.FileInfo(path);
                    file.Delete();
                }
                catch (Exception ex)
                {
                    html = html.Replace(url.ToString().Trim(), "");
                }
            }
            return html;
        }

        /// <summary>
        /// 新增上传宝贝分组
        /// </summary>
        /// <returns></returns>
        public string AddPicType()
        {
            PictureCategoryGetRequest req = new PictureCategoryGetRequest();
            req.PictureCategoryName = "宝贝复制图片(请勿删除)";
            PictureCategoryGetResponse rsp = TopClient.Execute<PictureCategoryGetResponse>(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
            //remarks += rsp.Body+"["+sessionKey+"]";
            if (rsp.PictureCategories.Count != 0)
            {
                return rsp.PictureCategories[0].PictureCategoryId.ToString();
            }
            else
            {
                PictureCategoryAddRequest req1 = new PictureCategoryAddRequest();
                req1.PictureCategoryName = "宝贝复制图片(请勿删除)";
                req1.ParentId = 0;
                PictureCategoryAddResponse rsp1 = TopClient.Execute(req1, sessionKey, (TopClient.CrmPlatForm)useplatform);
                //Console.WriteLine(rsp1.Body.ToString());
                return rsp1.PictureCategory.PictureCategoryId.ToString();
            }
        }



        /// <summary>
        /// （旧）获取叶子id下的子属性字符串（props）
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        //public string GetPropss(string html, string cid, string url)
        //{
        //    bool result = false;
        //    string props = string.Empty;
        //    string lp = string.Empty;
        //    html = html.Replace("\t", "");
        //    html = html.Replace("\n", "");
        //    //html = html.Replace(" ", "");
        //    string a = MatchValue(html, "attributes-list\">", "</ul>");
        //    MatchCollection obj1 = null;//子属性名集合
        //    MatchCollection obj2 = null;//子属性名值集合


        //    if (CheckTmallOrTaobao(url))
        //    {
        //        if (html.Contains("\"isNewProGroup\":false"))
        //        {

        //            string newhtml = MatchValue(html, "<ul id=\"J_AttrUL\">", "</ul>");
        //            newhtml = newhtml.Replace("：", ":");
        //            newhtml = System.Web.HttpUtility.HtmlDecode(newhtml);
        //            obj1 = MatchValues(newhtml.Trim(), "\">", ":");
        //            obj2 = MatchValues(newhtml, "title=\"", "\">");
        //        }
        //        else
        //        {
        //            obj1 = MatchValues(html.Trim(), "\"name\":\"", "\",");
        //            obj2 = MatchValues(html, "\"value\":\"", "\"}");
        //        }
        //    }
        //    else
        //    {
        //        obj1 = MatchValues(a.Trim(), "\">", ":");//子属性名集合
        //        obj2 = MatchValues(a.Trim(), "title=\"", "\">");//子属性名集合
        //    }
        //    ItempropsGetRequest req = new ItempropsGetRequest();//获取子栏目下所有子属性
        //    req.Fields = "pid,name,must,multi,prop_values,is_key_prop";
        //    req.Cid = Convert.ToInt64(cid);
        //    ItempropsGetResponse rsp = TopClient.Execute(req, sessionKey);
        //    try
        //    {
        //        for (int i = 0; i < rsp.ItemProps.Count; i++)
        //        {
        //            if (rsp.ItemProps[i].Name.ToString() == "采购地")
        //            {
        //                lp = lp + rsp.ItemProps[i].Pid + ":" + rsp.ItemProps[i].PropValues[0].Vid + ";";
        //            }
        //            for (int j = 0; j < obj1.Count; j++)
        //            {
        //                string obj123 = obj1[j].ToString();
        //                if (obj123 == "型号")
        //                {
        //                    obj123 = "型号";
        //                }
        //                if (obj1[j].ToString() == rsp.ItemProps[i].Name.ToString())
        //                {
        //                    string _name = obj2[j].ToString();
        //                    if (_name == "常规款" && (rsp.ItemProps[i].Pid.ToString() == "122216562" || rsp.ItemProps[i].Pid.ToString() == "122216507"))
        //                    {
        //                        _name = "常规";
        //                    }
        //                    if (_name == "休闲" && rsp.ItemProps[i].Pid.ToString() == "122216515")
        //                    {
        //                        _name = "日常";
        //                    }
        //                    if (_name == "其他休闲" && rsp.ItemProps[i].Pid.ToString() == "122216515")
        //                    {
        //                        _name = "休闲";
        //                    }
        //                    if (_name.Contains("男女通用"))
        //                    {
        //                        _name = _name.Replace("男女通用", "通用");
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "121516899" && _name == "无屏幕")
        //                    {
        //                        _name = "其他";
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "21425" && _name.Contains("其它"))
        //                    {
        //                        _name = "其它特大特小款式";
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "122216906")
        //                    {
        //                        if (_name.Contains("片"))
        //                        {
        //                            _name = "12片";
        //                        }
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "122216608" || rsp.ItemProps[i].Pid.ToString() == "1626698" || rsp.ItemProps[i].Pid.ToString() == "1626982")
        //                    {
        //                        if (_name.Contains("("))
        //                        {
        //                            string[] names = _name.Split('(');
        //                            _name = names[0].ToString();
        //                        }
        //                        if (_name.Contains("（"))
        //                        {
        //                            string[] names = _name.Split('（');
        //                            _name = names[0].ToString();
        //                        }
        //                        if (rsp.ItemProps[i].Pid.ToString() == "1626982")
        //                        {
        //                            _name = _name.Replace("镜架", "");
        //                        }
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "31501")
        //                    {
        //                        if (_name == "其他过家家玩具")
        //                        {
        //                            _name = "其他";
        //                        }
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "13328588")
        //                    {
        //                        if (_name == "95%以上")
        //                        {
        //                            _name = "96%及以上";
        //                        }
        //                    }
        //                    if (_name == "轿车/跑车")
        //                    {
        //                        _name = "小轿车";
        //                    }
        //                    if (_name == "坦克/战车")
        //                    {
        //                        _name = "坦克车";
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "122216906")
        //                    {
        //                        if (!_name.Contains("片"))
        //                        {
        //                            if (!_name.Contains("/ml") && !_name.Contains("/mL"))
        //                            {
        //                                if ((_name.Split('m'))[0].ToString().Trim() == "250" || (_name.Split('m')[0].ToString()).Trim() == "500" || (_name.Split('m')[0].ToString().Trim()) == "600")
        //                                {
        //                                    _name = ((_name.Split('m'))[0].ToString() + "g/mL").Trim();
        //                                }
        //                                else
        //                                {
        //                                    _name = ((_name.Split('m'))[0].ToString() + "g/ml").Trim();
        //                                }
        //                            }
        //                        }
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "1628059")
        //                    {
        //                        if (_name.Trim() == "卸妆乳/霜")
        //                        {
        //                            _name = "卸妆乳";
        //                        }
        //                        if (_name.Trim() == "卸妆水/液")
        //                        {
        //                            _name = "卸妆水";
        //                        }
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "20509")
        //                    {
        //                        _name = "L S M XL XXL";
        //                    }
        //                    //if (rsp.ItemProps[i].Pid.ToString() == "148870378")
        //                    //{
        //                    //    _name = Regex.Match(_name, @"^[\d]*").Value;
        //                    //    if (string.IsNullOrWhiteSpace(_name))
        //                    //    {
        //                    //        _name = "1";
        //                    //    }
        //                    //    //_name = "1";
        //                    //}
        //                    if (rsp.ItemProps[i].Pid.ToString() == "122216962")
        //                    {
        //                        if (_name.Contains("卷"))
        //                        {
        //                            _name = "卷";
        //                        }

        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "164656271")
        //                    {
        //                        if (_name == "仅墙纸")
        //                        {
        //                            _name = "纯墙纸";
        //                        }

        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "122276203" || rsp.ItemProps[i].Pid.ToString() == "21299")
        //                    {
        //                        if (_name.Contains("其他"))
        //                        {
        //                            _name = "其他/other";
        //                        }
        //                    }
        //                    if (rsp.ItemProps[i].Pid.ToString() == "20608")
        //                    {
        //                        if (_name.Contains("北欧"))
        //                        {
        //                            _name = "北欧/宜家";
        //                        }
        //                    }
        //                    if (obj2[j].ToString().Contains(" "))
        //                    {
        //                        _name = _name.Replace(" ", " ");
        //                    }
        //                    if (_name.Substring(0, 1) == " ")
        //                    {
        //                        _name = _name.Remove(0, 1);
        //                    }
        //                    // _name = _name.Replace("&nbsp;"," ");
        //                    string[] b = _name.Split(' ');
        //                    string pid = rsp.ItemProps[i].Pid.ToString();

        //                    //if (b.Length > 1)
        //                    //{
        //                    //}
        //                    //else
        //                    //{
        //                    //    lp = lp + pid + ":";
        //                    //}
        //                    if (pid != "5919063" && pid != "122276380" && pid != "122216679" && pid != "148774212" && pid != "149422948" && pid != "1627207")
        //                    {
        //                        string asasa = "21";
        //                        //if (pid == "122216679")
        //                        //{
        //                        //     asasa = "0";
        //                        //}
        //                        if (b.Length > 1 && pid != "20000" && pid != "8366967" && pid != "1627839" && pid != "1627207")
        //                        {
        //                            int bl = b.Length;
        //                            if (pid == "141410212")
        //                            {
        //                                if (b.Length > 3)
        //                                {
        //                                    bl = 3;
        //                                }
        //                            }
        //                            for (int k = 0; k < bl; k++)
        //                            {
        //                                result = false;
        //                                for (int m = 0; m < rsp.ItemProps[i].PropValues.Count; m++)
        //                                {
        //                                    string bbsize = b[k].ToString().Trim();
        //                                    if (bbsize == "XS-胸围25-30cm")
        //                                    {
        //                                        bbsize = "XS-超小型";
        //                                    }
        //                                    if (bbsize == "S-胸围30-35cm")
        //                                    {
        //                                        bbsize = "S-小型";
        //                                    }
        //                                    if (bbsize == "M-胸围35-40cm")
        //                                    {
        //                                        bbsize = "M-中型";
        //                                    }
        //                                    if (bbsize == "L-胸围40-45cm")
        //                                    {
        //                                        bbsize = "L-大型";
        //                                    }
        //                                    if (bbsize == "XL-胸围45-50cm")
        //                                    {
        //                                        bbsize = "XL-超大型";
        //                                    }
        //                                    if (bbsize.Trim() == rsp.ItemProps[i].PropValues[m].Name.ToString())
        //                                    {

        //                                        lp = lp + pid + ":" + rsp.ItemProps[i].PropValues[m].Vid + ";";
        //                                        string child = childpids(pid + ":" + rsp.ItemProps[i].PropValues[m].Vid + ";", cid, obj1, obj2, pid);
        //                                        lp = lp + child;
        //                                        result = true;

        //                                    }
        //                                }
        //                                if (!result && !string.IsNullOrWhiteSpace(b[k].ToString()))//判断是否需要补充自定义属性与对应值
        //                                {
        //                                    if (pid == "122216447")
        //                                    {
        //                                        lp = lp + "122216447:14863995;";
        //                                    }
        //                                    if (pid == "20000")
        //                                    {
        //                                        string child = childpids(pid + ":" + rsp.ItemProps[i].PropValues[2].Vid + ";", cid, obj1, obj2, pid);
        //                                        lp = lp + child;
        //                                    }
        //                                    else
        //                                    {
        //                                        if (string.IsNullOrWhiteSpace(input_pids))
        //                                        {
        //                                            input_pids = pid;
        //                                        }
        //                                        else
        //                                        {
        //                                            input_pids = input_pids + "," + pid;
        //                                        }
        //                                        //if()
        //                                        //{}
        //                                        //if (string.IsNullOrWhiteSpace(input_str))
        //                                        //{
        //                                        //    input_str = b[k].ToString();
        //                                        //}
        //                                        //else
        //                                        //{
        //                                        //    input_str = input_str + "," + b[k].ToString();
        //                                        //}


        //                                        string objj = b[k].ToString();
        //                                        if (pid == "148774212")//功率
        //                                        {
        //                                            objj = MatchValue(objj + " ", "W", " ");
        //                                            objj = b[k].ToString().Replace("W" + objj, "");
        //                                        }
        //                                        //if (pid == "148870378")
        //                                        //{
        //                                        //    objj = Regex.Match(obj2[j].ToString(), @"^[\d]*").Value;
        //                                        //    if (string.IsNullOrWhiteSpace(objj))
        //                                        //    {
        //                                        //        objj = "1";
        //                                        //    }
        //                                        //}
        //                                        //if (pid == "122216962")
        //                                        //{
        //                                        //    objj = "卷";
        //                                        //}
        //                                        if (string.IsNullOrWhiteSpace(input_str))
        //                                        {
        //                                            input_str = objj;
        //                                        }
        //                                        else
        //                                        {
        //                                            input_str = input_str + "," + objj;
        //                                        }
        //                                    }
        //                                }
        //                                //else if (!result && !string.IsNullOrWhiteSpace(b[k].ToString()) && rsp.ItemProps[i].Must == true)
        //                                //{
        //                                //    lp = lp + pid + ":" + rsp.ItemProps[i].PropValues[0].Vid + ";";
        //                                //}
        //                            }
        //                        }

        //                        else if (pid != "1627207")
        //                        {
        //                            result = false;
        //                            for (int m = 0; m < rsp.ItemProps[i].PropValues.Count; m++)
        //                            {
        //                                string _name1 = obj2[j].ToString();
        //                                if (_name1.Contains("男女通用"))
        //                                {
        //                                    _name1 = _name1.Replace("男女通用", "通用");
        //                                }
        //                                if (_name1 == "常规款" && (rsp.ItemProps[i].Pid.ToString() == "122216562" || rsp.ItemProps[i].Pid.ToString() == "122216507"))
        //                                {
        //                                    _name1 = "常规";
        //                                }
        //                                if (_name1 == "休闲" && rsp.ItemProps[i].Pid.ToString() == "122216515")
        //                                {
        //                                    _name1 = "日常";
        //                                }
        //                                if (_name1 == "其他休闲" && rsp.ItemProps[i].Pid.ToString() == "122216515")
        //                                {
        //                                    _name1 = "休闲";
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "121516899" && _name1 == "无屏幕")
        //                                {
        //                                    _name1 = "其他";
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "21425" && _name1.Contains("其它"))
        //                                {
        //                                    _name1 = "其它特大特小款式";
        //                                }
        //                                //if (rsp.ItemProps[i].Pid.ToString() == "122216906")
        //                                //{
        //                                //    if (_name1.Contains("片"))
        //                                //    {
        //                                //        _name1 = "12片";
        //                                //    }
        //                                //}
        //                                if (rsp.ItemProps[i].Pid.ToString() == "122216608" || rsp.ItemProps[i].Pid.ToString() == "1626698" || rsp.ItemProps[i].Pid.ToString() == "1626982")
        //                                {
        //                                    if (_name1.Contains("("))
        //                                    {
        //                                        string[] names = _name1.Split('(');
        //                                        _name1 = names[0].ToString();
        //                                    }
        //                                    if (_name1.Contains("（"))
        //                                    {
        //                                        string[] names = _name1.Split('（');
        //                                        _name1 = names[0].ToString();
        //                                    }
        //                                    if (rsp.ItemProps[i].Pid.ToString() == "1626982")
        //                                    {
        //                                        _name1 = _name1.Replace("镜架", "");
        //                                    }
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "31501")
        //                                {
        //                                    if (_name1 == "其他过家家玩具")
        //                                    {
        //                                        _name1 = "其他";
        //                                    }
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "13328588")
        //                                {
        //                                    if (_name1 == "95%以上")
        //                                    {
        //                                        _name1 = "96%及以上";
        //                                    }
        //                                }
        //                                if (_name1 == "轿车/跑车")
        //                                {
        //                                    _name1 = "小轿车";
        //                                }
        //                                if (_name1 == "坦克/战车")
        //                                {
        //                                    _name1 = "坦克车";
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "122216906")
        //                                {
        //                                    if (!_name1.Contains("片"))
        //                                    {
        //                                        if (!_name1.Contains("/ml") && !_name.Contains("/mL"))
        //                                        {
        //                                            if (_name1.Split('m')[0].ToString().Trim() == "250" || (_name1.Split('m')[0].ToString().Trim()) == "500" || (_name1.Split('m')[0].ToString().Trim()) == "600")
        //                                            {
        //                                                _name1 = ((_name1.Split('m'))[0].ToString() + "g/mL").Trim();
        //                                            }
        //                                            else
        //                                            {
        //                                                _name1 = ((_name1.Split('m'))[0].ToString() + "g/ml").Trim();
        //                                            }
        //                                        }
        //                                    }
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "1628059")
        //                                {
        //                                    if (_name1.Trim() == "卸妆乳/霜")
        //                                    {
        //                                        _name1 = "卸妆乳";
        //                                    }
        //                                    if (_name1.Trim() == "卸妆水/液")
        //                                    {
        //                                        _name1 = "卸妆水";
        //                                    }
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "20509")
        //                                {
        //                                    _name1 = "L S M XL XXL";
        //                                }
        //                                //if (rsp.ItemProps[i].Pid.ToString() == "148870378")
        //                                //{
        //                                //    _name1 = Regex.Match(_name, @"^[\d]*").Value;
        //                                //    if (string.IsNullOrWhiteSpace(_name))
        //                                //    {
        //                                //        _name1 = "1";
        //                                //    }
        //                                //    //_name = "1";
        //                                //}
        //                                if (rsp.ItemProps[i].Pid.ToString() == "122216962")
        //                                {
        //                                    if (_name1.Contains("卷"))
        //                                    {
        //                                        _name1 = "卷";
        //                                    }
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "164656271")
        //                                {
        //                                    if (_name1 == "仅墙纸")
        //                                    {
        //                                        _name1 = "纯墙纸";
        //                                    }

        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "122276203" || rsp.ItemProps[i].Pid.ToString() == "21299")
        //                                {
        //                                    if (_name1.Contains("其他"))
        //                                    {
        //                                        _name1 = "其他/other";
        //                                    }
        //                                }
        //                                if (rsp.ItemProps[i].Pid.ToString() == "20608")
        //                                {
        //                                    if (_name1.Contains("北欧"))
        //                                    {
        //                                        _name1 = "北欧/宜家";
        //                                    }
        //                                }
        //                                if (_name1.Trim() == rsp.ItemProps[i].PropValues[m].Name.ToString())
        //                                {

        //                                    lp = lp + pid + ":" + rsp.ItemProps[i].PropValues[m].Vid + ";";
        //                                    string child = childpids(pid + ":" + rsp.ItemProps[i].PropValues[m].Vid + ";", cid, obj1, obj2, pid);
        //                                    lp = lp + child;
        //                                    result = true;
        //                                }
        //                            }
        //                            if (!result && !string.IsNullOrWhiteSpace(obj2[j].ToString()))//判断是否需要补充自定义属性与对应值
        //                            {
        //                                if (pid == "122216447")
        //                                {
        //                                    lp = lp + "122216447:14863995;";
        //                                }
        //                                if (pid == "20000" || pid == "169348213")
        //                                {
        //                                    string child = childpids(pid + ":" + rsp.ItemProps[i].PropValues[2].Vid + ";", cid, obj1, obj2, pid);
        //                                    lp = lp + pid + ":" + rsp.ItemProps[i].PropValues[2].Vid + ";" + child;
        //                                }
        //                                else
        //                                {
        //                                    if (string.IsNullOrWhiteSpace(input_pids))
        //                                    {
        //                                        input_pids = pid;
        //                                    }
        //                                    else
        //                                    {
        //                                        input_pids = input_pids + "," + pid;
        //                                    }
        //                                    string objj = obj2[j].ToString();
        //                                    if (pid == "148774212")//功率
        //                                    {
        //                                        objj = MatchValue(objj + " ", "W", " ");
        //                                        objj = obj2[j].ToString().Replace("W" + objj, "");
        //                                    }
        //                                    //if (pid == "148870378")
        //                                    //{
        //                                    //    objj = Regex.Match(obj2[j].ToString(), @"^[\d]*").Value;
        //                                    //    if (string.IsNullOrWhiteSpace(objj))
        //                                    //    {
        //                                    //        objj = "1";
        //                                    //    }

        //                                    //}
        //                                    //if (pid == "122216962")
        //                                    //{
        //                                    //    objj = "卷";
        //                                    //}
        //                                    if (string.IsNullOrWhiteSpace(input_str))
        //                                    {
        //                                        input_str = objj;
        //                                    }
        //                                    else
        //                                    {
        //                                        input_str = input_str + "," + objj;
        //                                    }

        //                                }
        //                            }
        //                            //else if (!result && !string.IsNullOrWhiteSpace(obj2[j].ToString()) && rsp.ItemProps[i].Must == true)
        //                            //{
        //                            //    lp = lp + pid + ":" + rsp.ItemProps[i].PropValues[0].Vid + ";";
        //                            //}
        //                        }
        //                    }
        //                }
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    { }

        //    //string skumap = MatchValue(html, "skuMap     : ", "}}");
        //    //skumap = skumap + "}}";
        //    //MatchCollection aa = MatchValues(skumap, "\";", "\":{");
        //    //for (int i = 0; i < aa.Count; i++)
        //    //{
        //    //    lp = lp + aa[i].ToString();
        //    //}
        //    //更新食品类信息
        //    if (html.Contains("生产许可证编号"))
        //    {
        //        for (int i = 0; i < obj1.Count; i++)
        //        {
        //            switch (obj1[i].ToString())
        //            {
        //                case "生产许可证编号":
        //                    prd_license_no = obj2[i].ToString();
        //                    break;
        //                case "产品标准号":
        //                    design_code = obj2[i].ToString();
        //                    break;
        //                case "厂名":
        //                    factory = obj2[i].ToString();
        //                    break;
        //                case "厂址":
        //                    factory_site = obj2[i].ToString();
        //                    break;
        //                case "厂家联系方式":
        //                    contact = obj2[i].ToString();
        //                    break;
        //                case "配料表":
        //                    mix = obj2[i].ToString();
        //                    break;
        //                case "储藏方法":
        //                    plan_storage = obj2[i].ToString();
        //                    break;
        //                case "保质期":
        //                    period = obj2[i].ToString();
        //                    break;
        //                case "食品添加剂":
        //                    food_additive = obj2[i].ToString();
        //                    break;
        //                case "供货商":
        //                    supplier = obj2[i].ToString();
        //                    break;
        //            }
        //        }
        //        string start = MatchValue(html, "生产日期:", "至").ToString().Trim();
        //        string end = MatchValue(html, "至", "</div>").ToString().Trim();
        //        try
        //        {
        //            if (string.IsNullOrWhiteSpace(start))
        //            {
        //                start = System.DateTime.Now.ToString();
        //            }
        //            if (string.IsNullOrWhiteSpace(end))
        //            {
        //                end = System.DateTime.Now.ToString();
        //            }
        //            product_date_start = Convert.ToDateTime(start).ToString("yyyy-MM-dd");
        //            product_date_end = Convert.ToDateTime(end).ToString("yyyy-MM-dd");
        //        }
        //        catch (Exception ex)
        //        {
        //            remarks += "【生产日期:】" + ex.ToString();
        //        }

        //    }
        //    return lp;
        //}

        /// <summary>
        /// 添加产品主图片
        /// </summary>
        /// <param name="id"></param>
        /// <param name="html"></param>
        public void AddPic(string id, string html, string newurl)
        {
            //
            MatchCollection srcurl;
            if (CheckTmallOrTaobao(newurl))
            {
                string a = MatchValue(html, "id=\"J_UlThumb\"", "</ul>");
                srcurl = MatchValues(a, "src=\"//", "\"");
            }
            else
            {
                srcurl = MatchValues(html, "data-src=\"//", "\"");
            }
            for (int i = 0; i < srcurl.Count; i++)
            {
                string url = srcurl[i].ToString().Trim();
                string name = MatchValue(url, "TB", ".jpg");
                string path = string.Empty;
                string aa = MatchValue(url, ".jpg", ".jpg");
                if (string.IsNullOrWhiteSpace(aa))
                {
                    aa = MatchValue(url, ".png", ".jpg");
                }
                if (string.IsNullOrWhiteSpace(aa))
                {
                    aa = MatchValue(url, ".gif", ".jpg");
                }
                if (string.IsNullOrWhiteSpace(aa))
                {
                    aa = MatchValue(url, ".jpeg", ".jpg");
                }
                url = "http://" + url.Replace(aa + ".jpg", "");
                name = name.Replace(aa, "");
                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
                HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();
                System.Drawing.Image image = null;
                if (webresponse.StatusCode == HttpStatusCode.OK)
                {
                    image = System.Drawing.Image.FromStream(webresponse.GetResponseStream());
                    path = "D:" + name + ".jpg";
                    image.Save(path); //保存在本地文件夹  
                    image.Dispose(); //释放资源  
                }
                if (image != null)
                {
                    //上传图片
                    ItemImgUploadRequest req = new ItemImgUploadRequest();
                    req.NumIid = Convert.ToInt64(id);
                    req.Image = new FileItem(path);
                    if (i == 0)
                    {
                        req.IsMajor = true;
                    }
                    else
                    {
                        req.IsMajor = false;
                    }
                    ItemImgUploadResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                    System.IO.FileInfo file = new System.IO.FileInfo(path);
                    file.Delete();
                }
            }





        }


        #region 添加sku信息
        /// <summary>
        /// 添加sku信息
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public DataTable ToDataTable(string strJson, string id)
        {
            // //取出表名  
            //Regex rg = new Regex(@"(?<={)[^:]+(?=:/[)", RegexOptions.IgnoreCase);
            //string strName = rg.Match(strJson).Value;
            ////去除表名  
            //strJson = strJson.Substring(strJson.IndexOf("[") + 1);
            //strJson = strJson.Substring(0, strJson.IndexOf("]"));

            MatchCollection a = MatchValues(strJson, "\";", "\":{");
            MatchCollection prices = MatchValues(strJson, "\"price\":\"", "\",");
            MatchCollection skuIds = MatchValues(strJson, "\"skuId\":\"", "\",");
            for (int i = 0; i < a.Count; i++)
            {
                ItemSkuAddRequest sku = new ItemSkuAddRequest();
                sku.NumIid = Convert.ToInt64(id);
                sku.Properties = a[i].ToString();
                sku.Quantity = 2;
                sku.Price = prices[i].ToString();//sku价格
                ItemSkuAddResponse response_sku = TopClient.Execute(sku, sessionKey, (TopClient.CrmPlatForm)useplatform);
            }
            return null;
        }
        #endregion

        /// <summary>
        /// 获得字符串中开始和结束字符串中间得值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="s">开始</param>
        /// <param name="e">结束</param>
        /// <returns></returns>
        public static string MatchValue(string str, string s, string e)
        {
            Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Match(str).Value;
        }

        /// <summary>
        /// 获得字符串中开始和结束字符串中间得值的集合
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="s">开始</param>
        /// <param name="e">结束</param>
        /// <returns></returns>
        public static MatchCollection MatchValues(string str, string s, string e)
        {
            Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            //Regex rg = new Regex("(?<=\<!-- 正文内容 begin --\>).*(?=\<!-- 正文内容 end --\>)", RegexOptions.Multiline | RegexOptions.Singleline);

            return rg.Matches(str);
        }

        //public string childpids(string pids,string cid,MatchCollection obj1,MatchCollection obj2,string pid)
        //{

        //    string b = string.Empty;

        //    if (pid == "20000" || pid == "6939376" || pid == "21299" || pid == "169348213" || pid == "122276315" || pid == "20021")
        //    {
        //        ItempropsGetRequest req = new ItempropsGetRequest();//获取子栏目下所有子属性
        //        req.Fields = "pid,name,must,multi,prop_values";
        //        req.Cid = Convert.ToInt64(cid);
        //        req.ChildPath = pids;
        //        ItempropsGetResponse rsp = TopClient.Execute(req, sessionKey);
        //        if (rsp.ItemProps.Count>0&&rsp.ItemProps!=null)
        //        {
        //            for (int i = 0; i < obj1.Count; i++)
        //            {
        //                for (int j = 0; j < rsp.ItemProps.Count; j++)
        //                {

        //                    if (obj1[i].ToString() == rsp.ItemProps[j].Name.ToString())
        //                    {
        //                        bool a = false;
        //                        for (int n = 0; n < rsp.ItemProps[j].PropValues.Count; n++)
        //                        {
        //                            if (obj2[i].ToString() == rsp.ItemProps[j].PropValues[n].Name.ToString())
        //                            {

        //                                b += rsp.ItemProps[j].Pid + ":" + rsp.ItemProps[j].PropValues[n].Vid + ";";
        //                                string childb = childpidschild(b, cid, obj1, obj2, pid);
        //                                b += childb;
        //                                a = true;
        //                            }
        //                        }
        //                        if (a == false)
        //                        {
        //                            if (string.IsNullOrWhiteSpace(input_pids))
        //                            {
        //                                input_pids = rsp.ItemProps[j].Pid.ToString();
        //                            }
        //                            else
        //                            {
        //                                input_pids = input_pids + "," + rsp.ItemProps[j].Pid.ToString();
        //                            }
        //                            if (string.IsNullOrWhiteSpace(input_str))
        //                            {
        //                                input_str = obj2[i].ToString();
        //                            }
        //                            else
        //                            {
        //                                input_str = input_str + "," + obj2[i].ToString();
        //                            }
        //                        }
        //                    }


        //                }

        //            }
        //            if (string.IsNullOrWhiteSpace(b))
        //            {
        //                b = rsp.ItemProps[0].Pid + ":" + rsp.ItemProps[0].PropValues[0].Vid + ";";
        //                string childb = childpidschild(b, cid, obj1, obj2, pid);
        //                b += childb;
        //            }
        //        }
        //        else
        //        {
        //            b = pids;
        //            //for (int i = 0; i < obj1.Count; i++)
        //            //{
        //            //        if (obj1[i].ToString() =="型号")
        //            //        {
        //            //            bool a = false;
        //            //                if (string.IsNullOrWhiteSpace(input_pids))
        //            //                {
        //            //                    input_pids = "1632501";
        //            //                }
        //            //                else
        //            //                {
        //            //                    input_pids = input_pids + ",1632501";
        //            //                }
        //            //                if (string.IsNullOrWhiteSpace(input_str))
        //            //                {
        //            //                    input_str = obj2[i].ToString();
        //            //                }
        //            //                else
        //            //                {
        //            //                    input_str = input_str + "," + obj2[i].ToString();
        //            //                }
        //            //                b = "20000:1632501;";
        //            //        }
        //            //}
        //        }
        //    }

        //    return b;
        //}

        /// <summary>
        /// (旧)获取子类目
        /// </summary>
        /// <param name="pids"></param>
        /// <param name="cid"></param>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public string childpidschilds(string pids, string cid, MatchCollection obj1, MatchCollection obj2, string pid)
        {

            string b = string.Empty;

            ItempropsGetRequest req = new ItempropsGetRequest();//获取子栏目下所有子属性
            req.Fields = "pid,name,must,multi,prop_values";
            req.Cid = Convert.ToInt64(cid);
            req.ChildPath = pids;
            ItempropsGetResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
            if (rsp.ItemProps.Count > 0 && rsp.ItemProps != null)
            {
                for (int i = 0; i < obj1.Count; i++)
                {
                    for (int j = 0; j < rsp.ItemProps.Count; j++)
                    {
                        if (rsp.ItemProps[j].Pid.ToString() != "1627207")
                        {
                            if (obj1[i].ToString() == rsp.ItemProps[j].Name.ToString())
                            {
                                bool a = false;
                                for (int n = 0; n < rsp.ItemProps[j].PropValues.Count; n++)
                                {
                                    if (obj2[i].ToString() == rsp.ItemProps[j].PropValues[n].Name.ToString())
                                    {

                                        b += rsp.ItemProps[j].Pid + ":" + rsp.ItemProps[j].PropValues[n].Vid + ";";
                                        a = true;
                                    }
                                }
                                if (a == false)
                                {
                                    if (string.IsNullOrWhiteSpace(input_pids))
                                    {
                                        input_pids = rsp.ItemProps[j].Pid.ToString();
                                    }
                                    else
                                    {
                                        input_pids = input_pids + "," + rsp.ItemProps[j].Pid.ToString();
                                    }
                                    if (string.IsNullOrWhiteSpace(input_str))
                                    {
                                        input_str = obj2[i].ToString().Trim();
                                    }
                                    else
                                    {
                                        input_str = input_str + "," + obj2[i].ToString().Trim();
                                    }
                                }
                            }
                        }


                    }

                }
                if (string.IsNullOrWhiteSpace(b))
                {
                    b = rsp.ItemProps[0].Pid + ":" + rsp.ItemProps[0].PropValues[0].Vid + ";";
                }
            }
            else
            {
                b = pids;
            }

            return b;
        }
        /// <summary>
        /// 获取叶子id下的子属性字符串（props）
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public string GetProps(string html, string cid, string url, string txtnum)
        {
            bool result = false;
            string props = string.Empty;
            string lp = string.Empty;
            html = html.Replace("\t", "");
            html = html.Replace("\n", "");
            //html = html.Replace(" ", "");
            string a = MatchValue(html, "attributes-list\">", "</ul>");
            MatchCollection obj1 = null;//子属性名集合
            MatchCollection obj2 = null;//子属性名值集合


            if (CheckTmallOrTaobao(url))
            {
                if (html.Contains("\"isNewProGroup\":false"))
                {

                    string newhtml = MatchValue(html, "<ul id=\"J_AttrUL\">", "</ul>");
                    newhtml = newhtml.Replace("：", ":");
                    newhtml = System.Web.HttpUtility.HtmlDecode(newhtml);
                    obj1 = MatchValues(newhtml.Trim(), "\">", ":");
                    obj2 = MatchValues(newhtml, "title=\"", "\">");
                }
                else
                {
                    obj1 = MatchValues(html.Trim(), "\"name\":\"", "\",");
                    obj2 = MatchValues(html, "\"value\":\"", "\"}");
                }
            }
            else
            {
                obj1 = MatchValues(a.Trim(), "\">", ":");//子属性名集合
                obj2 = MatchValues(a.Trim(), "title=\"", "\">");//子属性值集合
            }
            MySqlHelper sql = new MySqlHelper("ToolsProducts", DbAccess.DataType.tools);
            //MySqlHelper sql = new MySqlHelper("GoodsProducts");
            string xmldate = string.Empty;
            DataSet ds = new DataSet();
            if (File.Exists("D:\\宝贝复制类目信息\\" + txtnum + "\\"+cid+".txt"))
            {  
                System.Text.Encoding code = System.Text.Encoding.GetEncoding("UTF-8");
                FileStream fs = new FileStream("D:\\宝贝复制类目信息\\" + txtnum + "\\" + cid + ".txt", FileMode.Open, FileAccess.Read);
                //仅 对文本 执行  读写操作     
                StreamReader sr = new StreamReader(fs, code);
                xmldate = sr.ReadToEnd();    
                sr.Close();
                fs.Close();
            }
            else
            {
                ItempropsGetRequest req = new ItempropsGetRequest();//获取子栏目下所有子属性
                req.Fields = "pid,name,must,multi,prop_values,is_key_prop,is_sale_prop,is_color_prop,is_enum_prop,is_item_prop";
                req.Cid = Convert.ToInt64(cid);
                ItempropsGetResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                if (rsp.ItemProps != null && rsp.ItemProps.Count > 0)
                {
                    xmldate = rsp.Body.ToString();
                    if (!Directory.Exists("D:\\宝贝复制类目信息\\" + txtnum + ""))//判断文件夹是否存在 
                    {
                        Directory.CreateDirectory("D:\\宝贝复制类目信息\\" + txtnum + "");//不存在则创建文件夹 
                    }
                    //File.Create(@"E:\宝贝复制类目信息\" + txtnum + ".txt");//创建该文件
                    System.IO.File.WriteAllText("D:\\宝贝复制类目信息\\" + txtnum + "\\"+cid+".txt", xmldate);
                    //sql.ExecuteNonQuery("insert into crm_productsciddata (id,xmldata,addtime,cid) values ('" + Guid.NewGuid().ToString() + "','" + System.Web.HttpUtility.HtmlEncode(xmldate) + "','" + System.DateTime.Now.ToString() + "','" + cid + "') ");
                }
                else
                {
                    remarks += "【商品类目接口异常！】";
                    return "";
                }
            }
            TextReader tr = new StringReader(xmldate);
            ds.ReadXml(tr);
            DataTable itemdt = ds.Tables["item_prop"];//属性表
            DataTable prop_values = ds.Tables["prop_values"];//关联表
            DataTable prop_value = ds.Tables["prop_value"];//子属性表
            for (int i = 0; i < itemdt.Rows.Count; i++)
            {
                try
                {
                    string name = itemdt.Rows[i]["name"].ToString();//属性名
                    bool must = Convert.ToBoolean(itemdt.Rows[i]["must"]);//是否必填属性
                    bool is_key_prop = Convert.ToBoolean(itemdt.Rows[i]["is_key_prop"]);//是否关键属性
                    bool is_sale_prop = Convert.ToBoolean(itemdt.Rows[i]["is_sale_prop"]);//是否销售属性
                    bool is_color_prop = Convert.ToBoolean(itemdt.Rows[i]["is_color_prop"]);//是否颜色属性
                    bool is_enum_prop = Convert.ToBoolean(itemdt.Rows[i]["is_enum_prop"]);//下拉选择或手输
                    bool is_item_prop = Convert.ToBoolean(itemdt.Rows[i]["is_item_prop"]);//是否商品属性
                    bool multi = Convert.ToBoolean(itemdt.Rows[i]["multi"]);//是否多选
                    string pid = itemdt.Rows[i]["pid"].ToString();
                    string guanlianid = itemdt.Rows[i]["item_prop_Id"].ToString();
                    string pgid = string.Empty;//对应子属性id
                    string name1 = string.Empty;//已处理的属性名
                    DataRow[] pdr = prop_values.Select("item_prop_Id='" + guanlianid + "'");
                    DataRow[] PropValues = prop_value.Select("prop_values_Id='" + (pdr.Length == 0 ? "-1" : pdr[0]["prop_values_Id"].ToString()) + "'");
                    //if (name == "计价单位")
                    //{
                    //    lp = lp + "148870378:3243699;";
                    //}
                    if (must || name == "材质")//必填项
                    {
                        bool bmust = false;//判断是否需要自定义属性
                        //bool gmust=false;//判断必填项是否已经赋值
                        #region 匹配属性名
                        for (int j = 0; j < obj1.Count; j++)
                        {
                            string obj1_name = obj1[j].ToString();//页面获取属性名
                            string obj2_name = obj2[j].ToString();//页面获取属性值 
                            if (obj1_name == name)
                            {
                                if (multi)//是否多选
                                {

                                }
                                else
                                {
                                    if (PropValues.Length > 0)
                                    {
                                        for (int m = 0; m < PropValues.Length; m++)
                                        {

                                            if (PropValues[m]["name"].ToString() == obj2_name.TrimStart() && bmust == false)//完全匹配
                                            {
                                                lp = lp + pid + ":" + PropValues[m]["vid"].ToString() + ";";
                                                lp = lp + childpidschild(pid + ":" + PropValues[m]["vid"].ToString() + ";", cid);
                                                bmust = true;
                                            }

                                            if (bmust == false)//模糊匹配
                                            {
                                                if (PropValues[m]["name"].ToString().Contains(obj2_name))
                                                {
                                                    lp = lp + pid + ":" + PropValues[m]["vid"].ToString() + ";";
                                                    lp = lp + childpidschild(pid + ":" + PropValues[m]["vid"].ToString() + ";", cid);
                                                    bmust = true;
                                                }
                                            }
                                        }
                                        if (bmust == false)//匹配不到,自定义属性
                                        {
                                            lp = lp + pid + ":" + PropValues[0]["vid"].ToString() + ";";
                                            lp = lp + childpidschild(pid + ":" + PropValues[0]["vid"].ToString() + ";", cid);
                                            bmust = true;
                                        }
                                    }
                                    else
                                    {
                                        if (bmust == false)//匹配不到,自定义属性
                                        {


                                            if (string.IsNullOrWhiteSpace(input_pids))
                                            {
                                                input_pids = pid;
                                            }
                                            else
                                            {
                                                input_pids = input_pids + "," + pid;
                                            }
                                            if (string.IsNullOrWhiteSpace(input_str))
                                            {
                                                if (pid == "152056919")
                                                {
                                                    input_str = "0.09㎡";
                                                }
                                                else if (pid == "166344375")
                                                {
                                                    input_str = "1";
                                                }
                                                else
                                                {
                                                    input_str = obj2_name.Trim();
                                                }

                                            }
                                            else
                                            {
                                                if (pid == "152056919")
                                                {
                                                    input_str = input_str + ",0.09㎡";
                                                }
                                                else if (pid == "166344375")
                                                {
                                                    input_str = input_str + ",1";
                                                }
                                                else
                                                {
                                                    input_str = input_str + "," + obj2_name.Trim();
                                                }
                                            }
                                            bmust = true;
                                        }
                                    }
                                }

                            }
                        }
                        #endregion
                        if (bmust == false)
                        {
                            lp = lp + pid + ":" + PropValues[0]["vid"].ToString() + ";";
                            lp = lp + childpidschild(pid + ":" + PropValues[0]["vid"].ToString() + ";", cid);
                        }
                    }
                    else
                    {
                        if (pid == "122216962")
                        {
                            lp = lp + pid + ":" + PropValues[0]["vid"].ToString() + ";";
                            lp = lp + childpidschild(pid + ":" + PropValues[0]["vid"].ToString() + ";", cid);
                        }
                        else
                        {
                            bool bmust = false;//判断必填项是否已经赋值
                            #region 匹配属性名
                            for (int j = 0; j < obj1.Count; j++)
                            {
                                string obj1_name = obj1[j].ToString();//页面获取属性名
                                string obj2_name = obj2[j].ToString();//页面获取属性值 
                                if (obj1_name == name)
                                {
                                    if (multi)//是否多选
                                    {

                                    }
                                    else
                                    {
                                        for (int m = 0; m < PropValues.Length; m++)
                                        {

                                            if (PropValues[m]["name"].ToString() == obj2_name && bmust == false)//完全匹配
                                            {
                                                lp = lp + pid + ":" + PropValues[m]["vid"].ToString() + ";";
                                                lp = lp + childpidschild(pid + ":" + PropValues[m]["vid"].ToString() + ";", cid);
                                                bmust = true;
                                            }

                                            if (bmust == false)//模糊匹配
                                            {
                                                if (PropValues[m]["name"].ToString().Contains(obj2_name))
                                                {
                                                    lp = lp + pid + ":" + PropValues[m]["vid"].ToString() + ";";
                                                    lp = lp + childpidschild(pid + ":" + PropValues[m]["name"].ToString() + ";", cid);
                                                    bmust = true;
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                            #endregion
                        }
                    }
                }
                catch (Exception ex)
                {
                    //MessageBox.Show(i.ToString());
                }
            }


            if (html.Contains("生产许可证编号") || html.Contains("保质期"))
            {
                for (int i = 0; i < obj1.Count; i++)
                {
                    switch (obj1[i].ToString())
                    {
                        case "生产许可证编号":
                            prd_license_no = obj2[i].ToString();
                            break;
                        case "产品标准号":
                            design_code = obj2[i].ToString();
                            break;
                        case "厂名":
                            factory = obj2[i].ToString();
                            break;
                        case "厂址":
                            factory_site = obj2[i].ToString();
                            break;
                        case "厂家联系方式":
                            contact = obj2[i].ToString();
                            break;
                        case "配料表":
                            mix = obj2[i].ToString();
                            break;
                        case "储藏方法":
                            plan_storage = obj2[i].ToString();
                            break;
                        case "保质期":
                            period = obj2[i].ToString();
                            break;
                        case "食品添加剂":
                            food_additive = obj2[i].ToString();
                            break;
                        case "供货商":
                            supplier = obj2[i].ToString();
                            break;
                    }
                }
                string start = MatchValue(html, "生产日期:", "至").ToString().Trim();
                string end = MatchValue(html, "至", "</div>").ToString().Trim();
                try
                {
                    if (string.IsNullOrWhiteSpace(start))
                    {
                        start = System.DateTime.Now.ToString();
                    }
                    if (string.IsNullOrWhiteSpace(end) || end.Contains("<"))
                    {
                        end = System.DateTime.Now.ToString();
                    }
                    product_date_start = Convert.ToDateTime(start).ToString("yyyy-MM-dd");
                    product_date_end = Convert.ToDateTime(end).ToString("yyyy-MM-dd");
                }
                catch (Exception ex)
                {
                    remarks += "【生产日期:】" + ex.ToString();
                }
            }
            return lp;
        }


        public string childpidschild(string pids, string cid)
        {

            string b = string.Empty;
            bool a = false;
            ItempropsGetRequest req = new ItempropsGetRequest();//获取子栏目下所有子属性
            req.Fields = "pid,name,must,multi,prop_values";
            req.Cid = Convert.ToInt64(cid);
            req.ChildPath = pids;
            ItempropsGetResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
            if (rsp.ItemProps.Count > 0 && rsp.ItemProps != null)
            {
                for (int i = 0; i < rsp.ItemProps.Count; i++)
                {
                    a = true;
                    #region 匹配属性名
                    if (rsp.ItemProps[i].PropValues.Count > 0)
                    {
                        b = b + rsp.ItemProps[i].Pid.ToString() + ":" + rsp.ItemProps[i].PropValues[0].Vid.ToString() + ";";
                    }
                    else
                    {
                        if (rsp.ItemProps[i].Must)
                        {
                            if (string.IsNullOrWhiteSpace(input_pids))
                            {
                                input_pids = rsp.ItemProps[i].Pid.ToString();
                            }
                            else
                            {
                                input_pids = input_pids + "," + rsp.ItemProps[i].Pid.ToString();
                            }
                            if (string.IsNullOrWhiteSpace(input_str))
                            {
                                input_str = "暂无";
                            }
                            else
                            {
                                input_str = input_str + ",暂无";
                            }
                        }
                        //else
                        //{
                        //    if (rsp.ItemProps[i].Name == "型号")
                        //    {
                        //        foreach (string cname in name) {
                        //            if (cname == rsp.ItemProps[i].Name) { 
                        //            }

                        //        }

                        //    }
                        //}
                    }
                    #endregion
                }
            }

            return b;
        }

        /// <summary>
        /// 获取分表
        /// </summary>
        /// <param name="sellerid"></param>
        /// <returns></returns>
        private string GetTableIndex(string sellerid)
        {
            int newName = Int32.Parse(sellerid.Substring(sellerid.Length - 2, 2));
            return newName.ToString("D2");
        }
    }
}
