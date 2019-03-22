using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Top.Schema.Fields;
using Top.Schema.Values;
using Top.Schema.Factory;
using Top.Api.Request;
using Top.Api.Response;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Data;
using System.Text.RegularExpressions;
using DbAccess;
using Top.Api.Util;
using System.Xml;
using Newtonsoft.Json.Linq;
//using GoodsTaobaoCommon;

namespace CopyItems
{
    public class TmallCopy
    {
        /// <summary>
        /// 标题
        /// </summary>
        public string title = string.Empty;
        /// <summary>
        /// 价格
        /// </summary>
        public double price = 0;
        /// <summary>
        /// 详情
        /// </summary>
        public string desc = string.Empty;
        /// <summary>
        /// 类目id
        /// </summary>
        public string cid = string.Empty;
        public string sessionKey;
        public int useplatform;
        public string remarks = string.Empty;
        /// <summary>
        /// 图片空间
        /// </summary>
        public string tpkj = string.Empty;
        /// <summary>
        /// 运费模板
        /// </summary>
        public string yfmb = string.Empty;
        /// <summary>
        /// 品牌id
        /// </summary>
        public string bid = string.Empty;
        public string ptitle = string.Empty;
        public DataTable dt = new DataTable();
        public string whiteimg = string.Empty;
        public string q = string.Empty;
        public Dictionary<string, string> odic = new Dictionary<string, string>();
        /// <summary>
        /// 品牌名id
        /// </summary>
        public string BrandName = string.Empty;
        /// <summary>
        /// 天猫店铺宝贝复制主体
        /// </summary>
        /// <param name="id">宝贝id</param>
        /// <param name="oldid">明细表记录id</param>
        /// <param name="sellderid">卖家sellerid</param>
        public void AddTaobao(string id, string oldid, string sellderid, string txtnum)
        {
            try
            {
                odic.Clear();
                //sessionKey = "6202012650b03844ff62ZZ0beZZb4f5fa78bf73dc3e0cb92090357450";
                //sellderid = "2090357450";
                Dictionary<string, string> dic = new Dictionary<string, string>();
                remarks = string.Empty;
                string url = "https://detail.tmall.com/item.htm?id=" + id + "";//输入id拼接url 
                tpkj = dt.Rows[0]["tpkj"].ToString();
                MyWebBrowser my = new MyWebBrowser();
                WebBrowser wb = my.GetPage(url);
                string newurl = wb.Document.Url.ToString();//获取真实的url
                StreamReader sr = new StreamReader(wb.DocumentStream, Encoding.GetEncoding(wb.Document.Encoding));
                wb.Dispose();
                string html = sr.ReadToEnd();//获取页面源代码
                html = GetHtml(url); 
                string urljson = AddPic(html, newurl);
                // 获取cid
                cid = GetCid(html);
                //cid = "124942005";
                string pid = AddPicType();
                PictureUploadRequest req = new PictureUploadRequest();
                req.PictureCategoryId = Convert.ToInt64(pid);
                req.Img = new FileItem(@"D:\tm.png");
                req.ImageInputTitle = "whiteimg";
                req.IsHttps = true;
                PictureUploadResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                if (rsp.Picture != null)
                {
                    whiteimg = rsp.Picture.PicturePath;
                }
                else
                {
                    remarks += "透明素材上传失败";
                }
                if (string.IsNullOrWhiteSpace(cid))
                {
                    //f.showlist("【宝贝id：" + id + "】cid获取错误",sellderid);
                    remarks += "【该宝贝已下架！】";
                    updateChildCopy(id, remarks);
                    return;
                }
                else
                {
                    TxtLog.WriteLine("获取商品详情", newurl+"获取商品详情开始");
                    desc = System.Web.HttpUtility.HtmlDecode(GetDetails(newurl, html)); 
                    q = GetTitle(newurl, html);
                    TxtLog.WriteLine("获取pid", q + "获取pid开始");
                    Dictionary<string, string> dics = new Dictionary<string, string>();
                    string productid = UpdateTmallProduct(Convert.ToInt64(cid), GetProps(html, cid, newurl));
                    TxtLog.WriteLine("获取pid",productid);
                    if (string.IsNullOrWhiteSpace(productid))
                    {
                        MGetProps(html, cid, newurl, 0, txtnum);
                        odic.Add("product_images", "{\"product_image_0\":\"" + whiteimg + "\"}");
                        productid = UPdateProduct(Convert.ToInt64(cid));
                        if (string.IsNullOrWhiteSpace(productid) && !string.IsNullOrWhiteSpace(bid))
                        {
                            ProductsSearchRequest req1 = new ProductsSearchRequest();
                            req1.Fields = "product_id,name,pic_url,cid,props,price,tsc";
                            req1.Q = BrandName;
                            req1.Cid = Convert.ToInt64(cid);
                            req1.Props = "20000:" + bid;
                            ProductsSearchResponse rsp1 = TopClient.Execute(req1);
                            if (rsp1.Products != null && rsp1.Products.Count > 0)
                            {
                                productid = rsp1.Products[0].ProductId.ToString();
                            }
                        }

                    }

                    if (string.IsNullOrWhiteSpace(productid))
                    {
                        remarks += "该类目产品暂不支持！";
                        updateChildCopy(id, remarks);
                        return;
                    }
                    else
                    {
                        #region 修改标题
                        title = GetTitle(newurl, html);
                        title = dt.Rows[0]["firsttitle"].ToString() + title + dt.Rows[0]["lasttitle"].ToString();
                        if (string.IsNullOrWhiteSpace(title))
                        {
                            remarks += "【该宝贝已下架！】";
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
                            if (title.Length > 30)
                            {
                                title = title.Substring(0, 30);
                            }
                            //f.showlist("【宝贝id："+id+"】标题已复制",sellderid);

                            dic.Add("title", title);
                        }
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
                        if (dt.Rows[0]["fj"].ToString() == "1")//是否保留分角0:保留，1;不保留
                        {
                            price = Convert.ToInt32(price);
                        }
                        dic.Add("price", price.ToString());
                        #endregion
                        #region 宝贝详情去除关键词，替换关键词

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
                        if (desc.Length > 25000)
                        {
                            desc.Substring(0, 24999);
                        }
                        if (desc.Length < 6)
                        {
                            desc += "待补充描述详情！";
                        }
                        dic.Add("description", desc);
                        #endregion
                        #region 运费模板
                        yfmb = dt.Rows[0]["yfmb"].ToString();
                        dic.Add("postage_id", yfmb);
                        #endregion
                        #region 获取卖家城市，省份
                        dic.Add("location", "{\"prov\":\"上海\",\"city\":\"上海\"}");//所在地
                        #endregion
                        #region 添加商品图片
                        dic.Add("item_images", urljson);
                        #endregion
                        dic.Add("item_type", "b");//发布类型一口价
                        dic.Add("stuff_status", "5");//宝贝类型全新
                        dic.Add("sub_stock", "false");//库存立减

                        dic.Add("auction_point", "0.5");//返点比例

                        dic.Add("item_status", "2");//商品状态仓库中
                        dic.Add("quantity", "100");//商品数量
                        dic.Add("delivery_way", "{0:2}");//提取方式 2:邮递
                        dic.Add("valid_thru", "7");//有效期 7天
                        dic.Add("sell_points", "{\"sell_point_0\":\"北欧风情 十二星座 防水防霉 简洁设计\"}");//卖点
                        dic.Add("white_bg_image", whiteimg);//透明素材图
                        dic.Add("food_security.prd_license_no", "QS530114020085");

                        string newid = UpdateTmall(Convert.ToInt64(cid), dic, Convert.ToInt64(productid));
                        if (!string.IsNullOrWhiteSpace(newid))
                        {
                            try
                            {
                                Convert.ToInt64(newid);
                            }
                            catch (Exception ex)
                            {
                                remarks += newid;
                                newid = null;
                            }
                        }
                        if (!string.IsNullOrWhiteSpace(remarks) && newid == null)
                        {
                            updateChildCopy(id, remarks);

                        }
                        else
                        {
                            updateChildCopys(newid, oldid, title);
                        }
                    }
                }
            }
            catch (Exception ex) { remarks += "该类目宝贝不支持！"; }
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                updateChildCopy(id, remarks);
            }
        }


        public void writlog(string name, string desc)
        {
            string sFilePath = "d:\\宝贝复制log\\" + DateTime.Now.ToString("yyyyMMdd");
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
        /// <summary>
        /// 发布tmallxml
        /// </summary>
        /// <param name="num_iid">类目id</param>
        /// <param name="dic">xml参数</param>
        /// <returns></returns>
        public string UpdateTmall(long num_iid, Dictionary<string, string> dic, long productid)
        {
            try
            {
                TmallItemAddSchemaGetRequest req = new TmallItemAddSchemaGetRequest();
                req.CategoryId = num_iid;
                req.ProductId = Convert.ToInt64(productid);
                req.Type = "b";
                req.IsvInit = true;
                TmallItemAddSchemaGetResponse response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                while (response.AddItemResult == null)
                {
                    response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                }
                //XmlDocument xml1 = new XmlDocument();
                //xml1.LoadXml(response.AddItemResult);
                //XmlNodeList nodeList = (xml1.SelectSingleNode("//field[@id='sku']").ChildNodes[1]).ChildNodes;
                //string a = string.Empty;
                //foreach (XmlNode node1 in nodeList)
                //{
                //    a += node1.Attributes["id"].Value + ",";
                //}
                List<Top.Schema.Fields.Field> list_fld = SchemaReader.ReadXmlForList(response.AddItemResult);
                List<Field> f_list = new List<Field>();

                foreach (Field f in list_fld)
                {
                    string must = f.GetRuleByName("requiredRule") == null ? "" : (f.GetRuleByName("requiredRule").Value);
                    if (must == "true")
                    {

                    }
                    if (f is InputField)
                    {
                        InputField inf = (InputField)f;

                        if (dic.ContainsKey(inf.Id))
                        {
                            inf.Value = dic[inf.Id];
                            f_list.Add(inf);
                        }
                        else
                        {
                            if (must == "true")
                            {
                                string v = inf.GetDefaultValue();
                                if (!string.IsNullOrEmpty(v))
                                {
                                    inf.Value = v;
                                    f_list.Add(inf);
                                }
                                else
                                {
                                    inf.Value = "";
                                    f_list.Add(inf);
                                }
                            }
                        }
                    }
                    if (f is SingleCheckField)
                    {
                        SingleCheckField inf = (SingleCheckField)f;
                        if (dic.ContainsKey(inf.Id))
                        {
                            inf.SetValue(dic[inf.Id]);
                            f_list.Add(inf);
                        }
                        else
                        {
                            if (must == "true")
                            {
                                string v = inf.GetDefaultValue();
                                if (!string.IsNullOrEmpty(v))
                                {
                                    inf.SetValue(v);
                                    f_list.Add(inf);
                                }
                                else
                                {
                                    inf.SetValue("");
                                    f_list.Add(inf);
                                }
                            }
                        }
                    }
                    if (f is ComplexField)
                    {
                        ComplexField inf = (ComplexField)f;
                        if (inf.Id == "location" || inf.Id == "item_images" || inf.Id == "sell_points")
                        {
                            ComplexValue complexValue = new ComplexValue();
                            foreach (Field fd in inf.GetFieldList())
                            {

                                //SingleCheckField cf = (SingleCheckField)fd;
                                var o = JObject.Parse(dic[inf.Id]);
                                foreach (JToken child in o.Children())
                                {
                                    var property1 = child as JProperty;
                                    if (fd.Id == property1.Name)
                                    {
                                        string newurl = (property1.Value).ToString().Replace("https://img.alicdn.com/imgextra/", "");
                                        complexValue.SetInputFieldValue(fd.Id, inf.Id == "item_images" ? newurl : property1.Value.ToString());
                                    }
                                    //cf.SetComplexValue(complexValue1);
                                }
                            }
                            inf.SetComplexValue(complexValue);
                            f_list.Add(inf);
                        }
                        else
                        {
                            if (dic.ContainsKey(inf.Id))
                            {
                                ComplexValue complexValue = new ComplexValue();
                                complexValue.SetInputFieldValue(inf.Id, dic[inf.Id]);
                                inf.SetComplexValue(complexValue);
                                f_list.Add(inf);
                            }
                            else
                            {
                                if (must == "true")
                                {
                                    inf.SetComplexValue(inf.GetDefaultComplexValue());
                                    f_list.Add(inf);
                                }
                            }
                        }
                    }
                    if (f is MultiCheckField)
                    {
                        MultiCheckField inf = (MultiCheckField)f;
                        if (dic.ContainsKey(inf.Id))
                        {
                            //inf.SetDefaultValueDO(inf.GetDefaultValuesDO());
                            inf.SetValues(inf.GetDefaultValuesDO());
                            f_list.Add(inf);
                            //List<Value> clist = new List<Value>();
                            //var o = JObject.Parse(dic[inf.Id]);
                            //foreach (JToken child in o.Children())
                            //{

                            //    var property1 = child as JProperty;
                            //    Value v = new Value(property1.Name.ToString(), property1.Value.ToString());
                            //    clist.Add(v);
                            //    //cf.SetComplexValue(complexValue1);
                            //}
                            //inf.SetValues(clist);
                            //f_list.Add(inf);
                        }
                        else
                        {
                            if (must == "true")
                            {
                                inf.SetValues(inf.GetDefaultValuesDO());
                                f_list.Add(inf);
                            }
                            else
                            {
                                //inf.SetDefaultValueDO(inf.GetDefaultValuesDO());
                            }
                        }
                    }
                    if (f is LabelField)
                    {
                        LabelField inf = (LabelField)f;
                        inf.SetLabelGroup(inf.GetLabelGroup());
                        f_list.Add(inf);
                    }
                    if (f is MultiComplexField)
                    {
                        try
                        {
                            MultiComplexField inf = (MultiComplexField)f;
                            if (inf.Id == "sku" || inf.Id == "prop_extend_1627207")
                            {
                                List<ComplexValue> clist = new List<ComplexValue>();
                                ComplexValue complexValue = new ComplexValue();
                                foreach (Field fd in inf.GetFieldList())
                                {

                                    //SingleCheckField cf = (SingleCheckField)fd;
                                    var o = JObject.Parse(dic[inf.Id]);
                                    foreach (JToken child in o.Children())
                                    {
                                        var property1 = child as JProperty;
                                        if (fd.Id == property1.Name)
                                        {
                                            complexValue.SetInputFieldValue(fd.Id, property1.Value.ToString());
                                        }
                                        //cf.SetComplexValue(complexValue1);
                                    }
                                }
                                clist.Add(complexValue);
                                inf.SetComplexValues(clist);
                            }

                            if (inf.GetDefaultComplexValues() != null)
                            {
                                inf.SetComplexValues(inf.GetDefaultComplexValues());
                            }
                            f_list.Add(inf);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                MultiCheckField inf_add = new MultiCheckField();
                inf_add.Id = "update_fields";
                inf_add.Name = "更新字段列表";
                List<Value> values2 = new List<Value>();
                foreach (string dic_key in dic.Keys)
                {
                    values2.Add(new Value(dic_key));
                }
                inf_add.SetValues(values2);
                f_list.Add(inf_add);

                string xml = SchemaWriter.WriteParamXmlString(f_list);
                //string back_msg = "";
                string back_msg = Updates(num_iid, xml, productid);
                TxtLog.WriteLine("创建宝贝", "num_iid" + num_iid + "productid"+ productid + "back_msg" + back_msg);
                if (!string.IsNullOrEmpty(back_msg))
                {
                    return back_msg;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                //TxtLog.WriteLine("批量修改接口调用错误日志", "宝贝ID：" + num_iid + "  修改字段：基本属性   错误信息：" + ex.ToString());
                return ex.ToString();
            }
        }

        /// <summary>
        ///更新xml
        /// </summary>
        /// <param name="key_name"></param>
        /// <param name="num_iid"></param>
        /// <param name="xml_data"></param>
        private string Updates(long num_iid, string xml_data, long productid)
        {
            TmallItemSchemaAddRequest req = new TmallItemSchemaAddRequest();
            req.CategoryId = num_iid;
            req.ProductId = productid;
            req.XmlData = xml_data;
            TmallItemSchemaAddResponse response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
            if (response.IsError)
            {
                string err = string.IsNullOrEmpty(response.ErrMsg) ? "出错了" : response.ErrMsg + ";详细错误：" + response.SubErrMsg;
                //记录错误日志
                //err = response.AddItemResult;
                return err;
            }
            else
            {
                return response.AddItemResult;
            }

        }

        /// <summary>
        ///获取产品id
        /// </summary>
        /// <param name="key_name"></param>
        /// <param name="num_iid"></param>
        /// <param name="xml_data"></param>
        private string Update(long num_iid, string xml_data)
        {
            try
            {
                //增量修改
                TmallProductSchemaMatchRequest req = new TmallProductSchemaMatchRequest();
                req.CategoryId = num_iid;
                req.Propvalues = xml_data;
                TmallProductSchemaMatchResponse response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                TxtLog.WriteLine("增量修改异常", "增量修改异常" + "num_iid" + num_iid + response.Body);
                if (response.IsError)
                {
                    
                    string err = string.IsNullOrEmpty(response.ErrMsg) ? "出错了" : response.ErrMsg + ";详细错误：" + response.SubErrMsg;
                    //记录错误日志
                    err = response.MatchResult;
                    return err;
                }
                else
                {
                    return response.MatchResult;
                }
            }
            catch (Exception ex)
            {
                TxtLog.WriteLine("增量修改异常", "增量修改异常" + ex.ToString());
                return ex.ToString();
            }
         

        }

        private string UPdateProduct(long num_iid)
        {
            try
            {
                if (string.IsNullOrEmpty(bid))
                {
                    ItemcatsAuthorizeGetRequest areq = new ItemcatsAuthorizeGetRequest();
                    areq.Fields = "brand.vid, brand.name";
                    ItemcatsAuthorizeGetResponse arsp = TopClient.Execute(areq, sessionKey, (TopClient.CrmPlatForm)useplatform);
                    if (!arsp.IsError)
                    {
                        for (int i = 0; i < arsp.SellerAuthorize.Brands.Count; i++)
                        {
                            if (ptitle == arsp.SellerAuthorize.Brands[i].Name)
                            {
                                bid = arsp.SellerAuthorize.Brands[i].Vid.ToString();
                            }
                        }

                    }
                }
                TmallProductAddSchemaGetRequest req = new TmallProductAddSchemaGetRequest();
                req.CategoryId = num_iid;
                if (!string.IsNullOrWhiteSpace(bid))
                {
                    req.BrandId = Convert.ToInt64(bid);
                }
                TmallProductAddSchemaGetResponse response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                while (response.AddProductRule == null)
                {
                    response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                }

                List<Top.Schema.Fields.Field> list_fld = SchemaReader.ReadXmlForList(response.AddProductRule);
                List<Field> f_list = new List<Field>();

                foreach (Field f in list_fld)
                {
                    string must = f.GetRuleByName("requiredRule") == null ? "" : (f.GetRuleByName("requiredRule").Value);
                    if (must == "true")
                    {

                    }
                    if (f is InputField)
                    {
                        InputField inf = (InputField)f;

                        if (odic.ContainsKey(inf.Id))
                        {
                            inf.Value = odic[inf.Id];
                            f_list.Add(inf);
                        }
                        else
                        {
                            if (must == "true")
                            {
                                string v = inf.GetDefaultValue();
                                if (!string.IsNullOrEmpty(v))
                                {
                                    inf.Value = v;
                                    f_list.Add(inf);
                                }
                                else
                                {
                                    inf.Value = "";
                                    f_list.Add(inf);
                                }
                            }
                        }
                    }
                    if (f is SingleCheckField)
                    {
                        SingleCheckField inf = (SingleCheckField)f;
                        if (odic.ContainsKey(inf.Id))
                        {
                            inf.SetValue(odic[inf.Id]);
                            f_list.Add(inf);
                        }
                        else
                        {
                            if (must == "true")
                            {
                                string v = inf.GetDefaultValue();
                                if (!string.IsNullOrEmpty(v))
                                {
                                    inf.SetValue(v);
                                    f_list.Add(inf);
                                }
                                else
                                {
                                    inf.SetValue("");
                                    f_list.Add(inf);
                                }
                            }
                        }
                    }
                    if (f is ComplexField)
                    {
                        ComplexField inf = (ComplexField)f;
                        if (inf.Id == "location" || inf.Id == "item_images" || inf.Id == "sell_points" || inf.Id == "product_images")
                        {
                            ComplexValue complexValue = new ComplexValue();
                            foreach (Field fd in inf.GetFieldList())
                            {

                                //SingleCheckField cf = (SingleCheckField)fd;
                                var o = JObject.Parse(odic[inf.Id]);
                                foreach (JToken child in o.Children())
                                {
                                    var property1 = child as JProperty;
                                    if (fd.Id == property1.Name)
                                    {
                                        string newurl = (property1.Value).ToString().Replace("https://img.alicdn.com/imgextra/", "");
                                        complexValue.SetInputFieldValue(fd.Id, inf.Id == "item_images" ? newurl : property1.Value.ToString());
                                    }
                                    //cf.SetComplexValue(complexValue1);
                                }
                            }
                            inf.SetComplexValue(complexValue);
                            f_list.Add(inf);
                        }
                        else
                        {
                            if (odic.ContainsKey(inf.Id))
                            {
                                ComplexValue complexValue = new ComplexValue();
                                complexValue.SetInputFieldValue(inf.Id, odic[inf.Id]);
                                inf.SetComplexValue(complexValue);
                                f_list.Add(inf);
                            }
                            else
                            {
                                if (must == "true")
                                {
                                    inf.SetComplexValue(inf.GetDefaultComplexValue());
                                    f_list.Add(inf);
                                }
                            }
                        }
                    }
                    if (f is MultiCheckField)
                    {
                        MultiCheckField inf = (MultiCheckField)f;
                        if (odic.ContainsKey(inf.Id))
                        {
                            inf.SetValues(inf.GetDefaultValuesDO());
                            f_list.Add(inf);
                            //List<Value> clist = new List<Value>();
                            //var o = JObject.Parse(odic[inf.Id]);
                            //foreach (JToken child in o.Children())
                            //{

                            //    var property1 = child as JProperty;
                            //    Value v = new Value(property1.Name.ToString(), property1.Value.ToString());
                            //    clist.Add(v);
                            //    //cf.SetComplexValue(complexValue1);
                            //}
                            //inf.SetValues(clist);
                            //f_list.Add(inf);
                        }
                        else
                        {
                            if (must == "true")
                            {
                                inf.SetValues(inf.GetDefaultValuesDO());
                                f_list.Add(inf);
                            }
                            else
                            {
                                //inf.SetDefaultValueDO(inf.GetDefaultValuesDO());

                            }
                        }
                    }
                    if (f is LabelField)
                    {
                        LabelField inf = (LabelField)f;
                        inf.SetLabelGroup(inf.GetLabelGroup());
                        f_list.Add(inf);
                    }
                    if (f is MultiComplexField)
                    {
                        try
                        {
                            MultiComplexField inf = (MultiComplexField)f;
                            if (inf.Id == "sku" || inf.Id == "prop_extend_1627207")
                            {
                                List<ComplexValue> clist = new List<ComplexValue>();
                                ComplexValue complexValue = new ComplexValue();
                                foreach (Field fd in inf.GetFieldList())
                                {

                                    //SingleCheckField cf = (SingleCheckField)fd;
                                    var o = JObject.Parse(odic[inf.Id]);
                                    foreach (JToken child in o.Children())
                                    {
                                        var property1 = child as JProperty;
                                        if (fd.Id == property1.Name)
                                        {
                                            complexValue.SetInputFieldValue(fd.Id, property1.Value.ToString());
                                        }
                                        //cf.SetComplexValue(complexValue1);
                                    }
                                }
                                clist.Add(complexValue);
                                inf.SetComplexValues(clist);
                            }

                            if (inf.GetDefaultComplexValues() != null)
                            {
                                inf.SetComplexValues(inf.GetDefaultComplexValues());
                            }
                            f_list.Add(inf);
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                }

                MultiCheckField inf_add = new MultiCheckField();
                inf_add.Id = "update_fields";
                inf_add.Name = "更新字段列表";
                List<Value> values2 = new List<Value>();
                foreach (string dic_key in odic.Keys)
                {
                    values2.Add(new Value(dic_key));
                }
                inf_add.SetValues(values2);
                f_list.Add(inf_add);

                string xml = SchemaWriter.WriteParamXmlString(f_list);
                //string back_msg = "";
               

                TmallProductSchemaAddRequest req1 = new TmallProductSchemaAddRequest();
                req1.CategoryId = num_iid;

                req1.BrandId = Convert.ToInt64(bid);
                req1.XmlData = xml;
                TmallProductSchemaAddResponse rsp1 = TopClient.Execute(req1, sessionKey, (TopClient.CrmPlatForm)useplatform);
                if (!rsp1.IsError)
                {
                    XmlDocument xml1 = new XmlDocument();
                    xml1.LoadXml(rsp1.AddProductResult);
                    return (((xml1.SelectSingleNode("//field[@id='product_id']")).ChildNodes)[0]).InnerText;
                }
                else
                {
                    //remarks = rsp1.SubErrMsg.ToString();
                    return "";
                }
            }
            catch (Exception ex)
            {
                remarks = "产品发布失败！";
                return "";
            }
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
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
            System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(Url);//ServiceEntry.GetRadomCompany((UserConfig)this.Tag));
            request.Method = "GET";
            request.ServicePoint.ConnectionLimit = 5;
            request.Accept = "*/*";
            request.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT6.1)";
            request.Headers["Accept-Language"] = "zh-cn";
            request.Timeout = 5000;
            using (System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse())
            {
                TxtLog.WriteLine("请求网页详情代码", Url + "请求失败");
                if (response.StatusCode == System.Net.HttpStatusCode.OK && response.ContentLength < 1024 * 1024)
                {
                    using (System.IO.StreamReader sr = new System.IO.StreamReader(response.GetResponseStream(), System.Text.Encoding.GetEncoding("GB2312")))
                    {
                        TxtLog.WriteLine("请求网页详情代码", Url + "请求成功败");
                        str = sr.ReadToEnd();
                    }
                }
            }
            return str;
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

        public int updateChildCopys(string id, string oldid, string title)
        {
            MySqlHelper sql = new MySqlHelper("ToolsProducts", DbAccess.DataType.tools);
            //MySqlHelper sql = new MySqlHelper("GoodsProducts");
            title = title.Replace("'", "");
            int res = sql.ExecuteNonQuery("update crm_productscopychild  set oldtitle='" + title + "',newid='" + id + "',state='1' where id='" + oldid + "' ");
            return res;
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
                //f.showlist("【宝贝id：" + id + "】图片分类id获取错误",sid);
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
                        if (!childs.Contains("?") && (childs.Contains(".jpg") || childs.Contains(".png") || childs.Contains(".gif")))
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

                try
                {
                    //image = System.Drawing.Image.FromStream(webresponse.GetResponseStream());
                    path = "D:" + name + type;
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
        /// 添加商品主图
        /// </summary>
        /// <param name="id"></param>
        /// <param name="html"></param>
        public string AddPic(string html, string newurl)
        {
            //
            string pid = AddPicType();
            StringBuilder urljson = new StringBuilder();

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
            if (srcurl.Count > 0)
            {
                urljson.Append("{");

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
                        PictureUploadRequest req = new PictureUploadRequest();
                        req.PictureCategoryId = Convert.ToInt64(pid);
                        req.Img = new FileItem(path);
                        req.ImageInputTitle = name;
                        req.IsHttps = true;
                        PictureUploadResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                        if (rsp.Picture != null)
                        {
                            if (i == srcurl.Count - 1)
                            {
                                urljson.Append("\"item_image_" + i + "\":\"" + rsp.Picture.PicturePath + "\"");
                            }
                            else
                            {
                                urljson.Append("\"item_image_" + i + "\":\"" + rsp.Picture.PicturePath + "\",");
                            }
                            //whiteimg = rsp.Picture.PicturePath;
                        }
                        else
                        {
                        }
                        System.IO.FileInfo file = new System.IO.FileInfo(path);
                        file.Delete();
                    }
                }
                urljson.Append("}");
            }
            else
            {
            }
            TxtLog.WriteLine("添加商品主图", urljson.ToString());
            return urljson.ToString();
        }

        /// <summary>
        /// 获取productid
        /// </summary>
        /// <param name="num_iid">类目id</param>
        /// <param name="dic">xml参数</param>
        /// <returns></returns>
        public string UpdateTmallProduct(long num_iid, Dictionary<string, string> dic)
        {
            try
            {

                TmallProductMatchSchemaGetRequest req = new TmallProductMatchSchemaGetRequest();
                req.CategoryId = num_iid;
                TmallProductMatchSchemaGetResponse response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                while (response.MatchResult == null)
                {
                    response = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
                }
                List<Top.Schema.Fields.Field> list_fld = SchemaReader.ReadXmlForList(response.MatchResult);
                List<Field> f_list = new List<Field>();

                foreach (Field f in list_fld)
                {
                    if (f is InputField)
                    {
                        InputField inf = (InputField)f;

                        if (dic.ContainsKey(inf.Id))
                        {
                            inf.Value = dic[inf.Id];
                            f_list.Add(inf);
                        }
                        else
                        {
                            string v = inf.GetDefaultValue();
                            if (!string.IsNullOrEmpty(v))
                            {
                                inf.Value = v;
                                f_list.Add(inf);
                            }
                            else
                            {
                                inf.Value = "";
                                f_list.Add(inf);
                            }
                        }
                    }
                    if (f is SingleCheckField)
                    {
                        SingleCheckField inf = (SingleCheckField)f;
                        if (dic.ContainsKey(inf.Id))
                        {
                            inf.SetValue(dic[inf.Id]);
                            f_list.Add(inf);
                        }
                        else
                        {
                            string v = inf.GetDefaultValue();
                            if (!string.IsNullOrEmpty(v))
                            {
                                inf.SetValue(v);
                                f_list.Add(inf);
                            }
                            else
                            {
                                inf.SetValue("");
                                f_list.Add(inf);
                            }
                        }
                    }
                    if (f is ComplexField)
                    {
                        ComplexField inf = (ComplexField)f;
                        if (dic.ContainsKey(inf.Id))
                        {
                            ComplexValue complexValue = new ComplexValue();
                            complexValue.SetInputFieldValue(inf.Id, dic[inf.Id]);
                            inf.SetComplexValue(complexValue);
                            f_list.Add(inf);
                        }
                        else
                        {
                            inf.SetComplexValue(inf.GetDefaultComplexValue());
                            f_list.Add(inf);
                        }
                    }
                    if (f is MultiCheckField)
                    {
                        MultiCheckField inf = (MultiCheckField)f;

                        //inf.SetDefaultValueDO(inf.GetDefaultValuesDO());
                        inf.SetValues(inf.GetDefaultValuesDO());
                        f_list.Add(inf);
                    }
                    if (f is LabelField)
                    {
                        LabelField inf = (LabelField)f;
                        inf.SetLabelGroup(inf.GetLabelGroup());
                        f_list.Add(inf);
                    }
                    if (f is MultiComplexField)
                    {
                        MultiComplexField inf = (MultiComplexField)f;
                        if (inf.GetDefaultComplexValues() != null)
                        {
                            inf.SetComplexValues(inf.GetDefaultComplexValues());
                        }
                        f_list.Add(inf);
                    }
                }

                MultiCheckField inf_add = new MultiCheckField();
                inf_add.Id = "update_fields";
                inf_add.Name = "更新字段列表";
                List<Value> values2 = new List<Value>();
                foreach (string dic_key in dic.Keys)
                {
                    values2.Add(new Value(dic_key));
                }
                inf_add.SetValues(values2);
                f_list.Add(inf_add);

                string xml = SchemaWriter.WriteParamXmlString(f_list);

                string back_msg = Update(num_iid, xml);
                TxtLog.WriteLine("获取产品id", back_msg);
                if (!string.IsNullOrEmpty(back_msg))
                {
                    return back_msg;
                }
                else
                {
                    return "";
                }
            }
            catch (Exception ex)
            {
                //TxtLog.WriteLine("批量修改接口调用错误日志", "宝贝ID：" + num_iid + "  修改字段：基本属性   错误信息：" + ex.ToString());
                return ex.ToString();
            }
        }

        /// <summary>
        /// 获取叶子id下的子属性字符串（props）
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetProps(string html, string cid, string url)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //dic.Add("prop_20000", "10446017");//品牌
            //dic.Add("prop_13138467", "3373494");//货号
            html = html.Replace("\t", "");
            html = html.Replace("\n", "");
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
            TmallProductMatchSchemaGetRequest req = new TmallProductMatchSchemaGetRequest();
            req.CategoryId = Convert.ToInt64(cid);
            TmallProductMatchSchemaGetResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
            if (rsp.MatchResult == null)
            {
                //rsp = TopClient.Execute(req, sessionKey);
                throw new ArgumentOutOfRangeException("接口获取失败");
            }
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(rsp.MatchResult);
            XmlNodeList nodeList = xml.SelectNodes("//field");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string name = nodeList[i].Attributes["name"].Value.ToString();
                string id = nodeList[i].Attributes["id"].Value.ToString();
                for (int j = 0; j < obj1.Count; j++)
                {
                    if (name == obj1[j].ToString())
                    {
                        string oname = obj2[j].ToString().TrimStart().TrimEnd(); ;//获取品牌名，型号名，货号名
                        if (name == "品牌")
                        {
                            ptitle = oname;
                        }
                        XmlNode nodeList1 = nodeList[i].SelectSingleNode("//option[@displayName='" + oname + "']");
                        if (nodeList1 != null)
                        {
                            dic.Add(id, nodeList1.Attributes["value"].Value.ToString());
                        }
                        else
                        {
                            if (name == "品牌")
                            {

                                XmlNode nodeList2 = nodeList[i].SelectSingleNode("//option");
                                if (nodeList2 != null)
                                {
                                    BrandName = nodeList2.Attributes["displayName"].Value.ToString();
                                    dic.Add(id, nodeList2.Attributes["value"].Value.ToString());
                                    bid = nodeList2.Attributes["value"].Value.ToString();
                                }
                                // += "未获取该品牌的授权:" + oname;
                            }
                            else
                            {

                                if (!rsp.MatchResult.Contains("in_"))
                                {
                                    dic.Add(id, oname);
                                }
                                else
                                {
                                    if (id.Contains("in_"))
                                    {
                                        dic.Add(id, oname);
                                    }
                                    else
                                    {
                                        dic.Add(id, "-1");
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return dic;
        }

        /// <summary>
        /// 获取叶子id下的子属性字符串（props）
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetPropss(string html, string cid, string url)
        {
            Dictionary<string, string> dic = new Dictionary<string, string>();
            //dic.Add("prop_20000", "10446017");//品牌
            //dic.Add("prop_13138467", "3373494");//货号
            html = html.Replace("\t", "");
            html = html.Replace("\n", "");
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
            TmallProductAddSchemaGetRequest req = new TmallProductAddSchemaGetRequest();
            req.CategoryId = 150704;
            req.BrandId = 140274740;
            TmallProductAddSchemaGetResponse rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
            while (rsp.AddProductRule == null)
            {
                rsp = TopClient.Execute(req, sessionKey, (TopClient.CrmPlatForm)useplatform);
            }
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(rsp.AddProductRule);
            XmlNodeList nodeList = xml.SelectNodes("//field");
            for (int i = 0; i < nodeList.Count; i++)
            {
                string name = nodeList[i].Attributes["name"].Value.ToString();
                string id = nodeList[i].Attributes["id"].Value.ToString();
                for (int j = 0; j < obj1.Count; j++)
                {
                    if (name == obj1[j].ToString())
                    {
                        string oname = obj2[j].ToString().TrimStart().TrimEnd(); ;//获取品牌名，型号名，货号名
                        XmlNode nodeList1 = nodeList[i].SelectSingleNode("//option[@displayName='" + oname + "']");
                        if (nodeList1 != null)
                        {
                            dic.Add(id, nodeList1.Attributes["value"].Value.ToString());
                        }
                        else
                        {
                            if (name == "品牌")
                            {
                                XmlNode nodeList2 = nodeList[i].SelectSingleNode("//option");
                                if (nodeList2 != null)
                                {
                                    dic.Add(id, nodeList2.Attributes["value"].Value.ToString());
                                    bid = nodeList2.Attributes["value"].Value.ToString();
                                }
                                // += "未获取该品牌的授权:" + oname;
                            }
                            else
                            {

                                if (!rsp.AddProductRule.Contains("in_"))
                                {
                                    dic.Add(id, oname);
                                }
                                else
                                {
                                    if (id.Contains("in_"))
                                    {
                                        dic.Add(id, oname);
                                    }
                                    else
                                    {
                                        dic.Add(id, "-1");
                                    }
                                }
                            }
                        }
                    }
                }

            }
            return dic;
        }

        /// <summary>
        /// 获取叶子id下的子属性字符串（props）
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public void MGetProps(string html, string cid, string url, int tid, string txtnum)
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
            if (File.Exists("D:\\宝贝复制类目信息\\" + txtnum + "\\" + cid + ".txt"))
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
                    System.IO.File.WriteAllText("D:\\宝贝复制类目信息\\" + txtnum + "\\" + cid + ".txt", xmldate);
                    //sql.ExecuteNonQuery("insert into crm_productsciddata (id,xmldata,addtime,cid) values ('" + Guid.NewGuid().ToString() + "','" + System.Web.HttpUtility.HtmlEncode(xmldate) + "','" + System.DateTime.Now.ToString() + "','" + cid + "') ");
                }
                else
                {
                    remarks += "【商品类目接口异常！】";

                }
            }
            //if (dt.Rows.Count > 0)
            //{
            //    xmldate = System.Web.HttpUtility.HtmlDecode(dt.Rows[0]["xmldata"].ToString());
            //}
            //else
            //{
            //    ItempropsGetRequest req = new ItempropsGetRequest();//获取子栏目下所有子属性
            //    req.Fields = "pid,name,must,multi,prop_values,is_key_prop,is_sale_prop,is_color_prop,is_enum_prop,is_item_prop";
            //    req.Cid = Convert.ToInt64(cid);
            //    ItempropsGetResponse rsp = TopClient.Execute(req, sessionKey);
            //    if (rsp.ItemProps != null && rsp.ItemProps.Count > 0)
            //    {
            //        xmldate = rsp.Body.ToString();
            //        sql.ExecuteNonQuery("insert into crm_productsciddata (id,xmldata,addtime,cid) values ('" + Guid.NewGuid().ToString() + "','" + System.Web.HttpUtility.HtmlEncode(xmldate) + "','" + System.DateTime.Now.ToString() + "','" + cid + "') ");

            //    }
            //    else
            //    {
            //        remarks += "【商品类目接口异常！】";
            //    }

            //}
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
                    if (tid > PropValues.Length)
                    {
                        tid = PropValues.Length;
                    }
                    if (must)//必填项
                    {
                        bool bmust = false;//判断是否需要自定义属性
                        //bool gmust=false;//判断必填项是否已经赋值
                        #region 匹配属性名
                        for (int j = 0; j < obj1.Count; j++)
                        {
                            string obj1_name = obj1[j].ToString();//页面获取属性名
                            string obj2_name = obj2[j].ToString();//页面获取属性值 
                            if (obj1_name == name && obj1_name == "品牌")
                            {
                                odic.Add("prop_20000", bid);
                                childpidschild("20000:" + bid + ";", cid, tid);
                            }
                            if (obj1_name == name && obj1_name != "品牌")
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
                                                odic.Add("prop_" + pid, "" + PropValues[m]["vid"].ToString() + "");
                                                childpidschild(pid + ":" + PropValues[m]["vid"].ToString() + ";", cid, tid);
                                                bmust = true;
                                            }

                                            if (bmust == false)//模糊匹配
                                            {
                                                if (PropValues[m]["name"].ToString().Contains(obj2_name))
                                                {
                                                    lp = lp + pid + ":" + PropValues[m]["vid"].ToString() + ";";
                                                    odic.Add("prop_" + pid, "" + PropValues[m]["vid"].ToString() + "");
                                                    childpidschild(pid + ":" + PropValues[m]["vid"].ToString() + ";", cid, tid);
                                                    bmust = true;
                                                }
                                            }
                                        }
                                        if (bmust == false)//匹配不到,自定义属性
                                        {
                                            lp = lp + pid + ":" + PropValues[tid]["vid"].ToString() + ";";
                                            odic.Add("prop_" + pid, "" + PropValues[tid]["vid"].ToString() + "");
                                            childpidschild(pid + ":" + PropValues[tid]["vid"].ToString() + ";", cid, tid);
                                            bmust = true;
                                        }
                                    }
                                    else
                                    {
                                    }
                                }

                            }
                        }
                        #endregion
                        if (bmust == false)
                        {
                            lp = lp + pid + ":" + PropValues[0]["vid"].ToString() + ";";
                            odic.Add("prop_" + pid, "" + PropValues[0]["vid"].ToString() + "");
                            childpidschild(pid + ":" + PropValues[0]["vid"].ToString() + ";", cid, tid);
                        }
                    }
                    else
                    {
                        if (pid == "122216962")
                        {
                            lp = lp + pid + ":" + PropValues[tid]["vid"].ToString() + ";";
                            odic.Add("prop_" + pid, "" + PropValues[tid]["vid"].ToString() + "");
                            childpidschild(pid + ":" + PropValues[tid]["vid"].ToString() + ";", cid, tid);
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
                                                odic.Add("prop_" + pid, "" + PropValues[m]["vid"].ToString() + "");
                                                childpidschild(pid + ":" + PropValues[m]["vid"].ToString() + ";", cid, tid);
                                                bmust = true;
                                            }

                                            if (bmust == false)//模糊匹配
                                            {
                                                if (PropValues[m]["name"].ToString().Contains(obj2_name))
                                                {
                                                    lp = lp + pid + ":" + PropValues[m]["vid"].ToString() + ";";
                                                    odic.Add("prop_" + pid, "" + PropValues[m]["vid"].ToString() + "");
                                                    childpidschild(pid + ":" + PropValues[m]["vid"].ToString() + ";", cid, tid);
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



        }

        public void childpidschild(string pids, string cid, int tid)
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
                if (tid > rsp.ItemProps.Count)
                {
                    tid = rsp.ItemProps.Count;
                }
                for (int i = 0; i < rsp.ItemProps.Count; i++)
                {
                    a = true;
                    #region 匹配属性名
                    if (rsp.ItemProps[i].PropValues.Count > 0)
                    {
                        //b = b + rsp.ItemProps[i].Pid.ToString() + ":" + rsp.ItemProps[i].PropValues[0].Vid.ToString() + ";";
                        odic.Add("prop_" + rsp.ItemProps[i].Pid.ToString(), rsp.ItemProps[i].PropValues[tid].Vid.ToString());
                    }
                    else
                    {
                        odic.Add("in_prop_" + rsp.ItemProps[i].Pid.ToString(), rsp.ItemProps[i].PropValues[tid].Vid.ToString());
                    }
                    #endregion
                }
            }
        }
    }
}
