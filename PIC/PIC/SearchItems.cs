using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Top.Api.Domain;
using Top.Api.Request;
using Top.Api.Response;

namespace SearchItem
{
    public class SearchItems
    {
        public static Dictionary<string, Item> Search(SearEntity sear, SellerInfoEntity sellerinfo)
        {
            try
            {
                bool isnum_iid = true;
                string reqName = "";
                StringBuilder strbNumIid = new StringBuilder(); 
                Dictionary<string, Item> itemDic = new Dictionary<string, Item>();
                string type = sear.type;
                string num_iid = sear.numIid;
                string status = sear.statuss;        //查询是否出售中
                string sellerCat = sear.sellerCats;  //查询条件宝贝分类
                string strMainWord =sear.title;  //查询条件宝贝关键字
                int useplatform = sear.useplatform;//平台识别码
                string sellerId = sellerinfo.strUserId;
                GetType(type, ref isnum_iid, ref reqName);
                if(!string.IsNullOrWhiteSpace(num_iid))
                {
                    #region 宝贝id查询
                  return  SearchNumIid(num_iid, sellerinfo, reqName,useplatform);
                    #endregion
                }
                else
                {   //根据条件查询
                    return SearchAll(status, reqName, isnum_iid, strMainWord, sellerCat, sellerinfo,useplatform);
                }
            }
            catch (Exception ex)
            {
                TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "=== ：出现异常:" + ex.ToString());
                return null;
            }
        }

        public static Dictionary<string, Item> SearchAll(string status, string reqName, bool isnum_iid, string strMainWord, string sellerCat, SellerInfoEntity sellerinfo, int useplatform)
        {
            TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "===" + status + " ：:" + sellerinfo.top_session);
            Dictionary<string, Item> itemDic = new Dictionary<string, Item>();
            StringBuilder strbNumIid = new StringBuilder();
            long totalResults = 0; //宝贝总数
            int pageNo = 0;     //api查询页码
            int pageNo2 = 0;     //api查询页码
            status = status == "3" ? null : status;
            if (string.IsNullOrWhiteSpace(status))
            {
                #region 获取出售中的商品
                do
                {
                    pageNo++;
                    ItemsOnsaleGetRequest req = new ItemsOnsaleGetRequest();
                    req.Fields = reqName;
                    req.PageSize = 200L;
                    req.PageNo = Convert.ToInt64(pageNo);
                    req.OrderBy = "list_time:asc";
                    if (strMainWord.Length > 0)
                        req.Q = strMainWord;
                    if (sellerCat.Trim().Length > 0)
                        req.SellerCids = sellerCat;
                    ItemsOnsaleGetResponse response = TopClient.Execute<ItemsOnsaleGetResponse>(req, sellerinfo.top_session, (TopClient.CrmPlatForm)useplatform);
                    if (response.IsError)
                    {
                        TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "=== ：出现异常:" + response.Body);
                    }
                    else
                    {
                        TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "=== ：出现异常:" + response.Body);
                        totalResults = response.TotalResults;
                        foreach (Item item in response.Items.Where(item => !itemDic.ContainsKey(item.NumIid.ToString())))
                        {
                            if (isnum_iid)
                            {
                                strbNumIid.Append(item.NumIid.ToString() + ",");
                            }
                            else
                            {
                                itemDic.Add(item.NumIid.ToString(), item);
                            }
                        }
                    }
                } while (totalResults > pageNo * 200);
                #endregion
                #region 获取仓库中的商品
                do
                {
                    pageNo2++;
                    ItemsInventoryGetRequest req = new ItemsInventoryGetRequest
                    {
                        Fields = reqName,
                        PageSize = 200,
                        PageNo = pageNo2
                    };
                    if (strMainWord.Length > 0)
                        req.Q = strMainWord;
                    if (sellerCat.Trim().Length > 0)
                        req.SellerCids = sellerCat;
                    ItemsInventoryGetResponse response = TopClient.Execute(req, sellerinfo.top_session, (TopClient.CrmPlatForm)useplatform);
                    if (response.IsError)
                    {
                        TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "=== ：出现异常:" + response.Body);
                    }
                    else
                    {
                        totalResults = response.TotalResults;
                        foreach (Item item in response.Items.Where(item => !itemDic.ContainsKey(item.NumIid.ToString())))
                        {
                            if (isnum_iid)
                            {
                                strbNumIid.Append(item.NumIid.ToString() + ",");
                            }
                            else
                            {
                                itemDic.Add(item.NumIid.ToString(), item);
                            }
                        }
                    }
                } while (totalResults > pageNo2 * 200);
                #endregion
            }
            else
            {
                if (status == "0")
                {
                    #region 获取出售中的商品
                    do
                    {
                        pageNo++;
                        ItemsOnsaleGetRequest req = new ItemsOnsaleGetRequest();
                        req.Fields = reqName;
                        req.PageSize = 200L;
                        req.PageNo = Convert.ToInt64(pageNo);
                        req.OrderBy = "list_time:asc";
                        if (strMainWord.Length > 0)
                            req.Q = strMainWord;
                        if (sellerCat.Trim().Length > 0)
                            req.SellerCids = sellerCat;
                        ItemsOnsaleGetResponse response = TopClient.Execute<ItemsOnsaleGetResponse>(req, sellerinfo.top_session, (TopClient.CrmPlatForm)useplatform);
                        if (response.IsError)
                        {
                            TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "=== ：出现异常:" + response.Body);
                        }
                        else
                        {
                            totalResults = response.TotalResults;
                            foreach (Item item in response.Items.Where(item => !itemDic.ContainsKey(item.NumIid.ToString())))
                            {
                                if (isnum_iid)
                                {
                                    strbNumIid.Append(item.NumIid.ToString() + ",");
                                }
                                else
                                {
                                    itemDic.Add(item.NumIid.ToString(), item);
                                }
                            }
                        }
                    } while (totalResults > pageNo * 200);
                    #endregion
                }
                else if (status == "1" )
                {
                    #region 获取仓库中的商品
                    do
                    {
                        pageNo++;
                        ItemsInventoryGetRequest req = new ItemsInventoryGetRequest
                        {
                            Fields = reqName,
                            PageSize = 200,
                            PageNo = pageNo
                        };
                        if (strMainWord.Length > 0)
                            req.Q = strMainWord;
                        if (sellerCat.Trim().Length > 0)
                            req.SellerCids = sellerCat;
                        ItemsInventoryGetResponse response = TopClient.Execute(req, sellerinfo.top_session, (TopClient.CrmPlatForm)useplatform);
                        if (response.IsError)
                        {
                            TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "=== ：出现异常:" + response.Body);
                        }
                        else
                        {
                            totalResults = response.TotalResults;
                            foreach (Item item in response.Items.Where(item => !itemDic.ContainsKey(item.NumIid.ToString())))
                            {
                                if (isnum_iid)
                                {
                                    strbNumIid.Append(item.NumIid.ToString() + ",");
                                }
                                else
                                {
                                    itemDic.Add(item.NumIid.ToString(), item);
                                }
                            }
                        }
                    } while (totalResults > pageNo * 200);
                    #endregion
                }
              
            }
            if (isnum_iid)
            {
                itemDic = SearchNumIid(strbNumIid.ToString(), sellerinfo, reqName,useplatform);
            }
            return itemDic;
        }

        public static Dictionary<string, Item> SearchNumIid(string num_iid, SellerInfoEntity sellerinfo, string reqName,int useplatform)
        {
            Dictionary<string, Item> itemDic = new Dictionary<string, Item>();
            #region 非全店选择批量
            if (string.IsNullOrWhiteSpace(num_iid)) return null;
            string itemid = num_iid;       //商品id
            Regex reg = new Regex(@"(\d+,){0,19}\d+");      //正则获取每次20个
            foreach (Match m in reg.Matches(itemid))
            {
                ItemsSellerListGetRequest itemGetRequest = new ItemsSellerListGetRequest
                {
                    Fields = reqName,
                    NumIids = m.Value
                };
                ItemsSellerListGetResponse itemGetResponse = TopClient.Execute(itemGetRequest, sellerinfo.top_session,(TopClient.CrmPlatForm)useplatform);
                for (int j = 0; j < 3; j++)
                {
                    itemGetResponse = TopClient.Execute(itemGetRequest, sellerinfo.top_session, (TopClient.CrmPlatForm)useplatform);
                    bool isok = string.IsNullOrWhiteSpace(itemGetResponse.SubErrMsg) ? true : false;
                    if (isok)//没有错就跳出  有错误暂停0.5s 继续执行
                        break;
                    else
                        Thread.Sleep(500);
                }
                if (itemGetResponse.IsError)
                {
                    TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "===" + m.Value + "：出现异常:" + itemGetResponse.Body);
                    continue;
                }
                foreach (Item item in itemGetResponse.Items.Where(item => !itemDic.ContainsKey(item.NumIid.ToString())))
                {
                    itemDic.Add(item.NumIid.ToString(), item);
                }
            }
            #endregion
            return itemDic;
        }
        #region 判断是那边调用的
        /// <summary>
        /// 根据传入的类型判断是否走id查询接口
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="isnum_iid">是否调用id查询接口</param>
        /// <param name="reqName">所需参数</param>
        private static void GetType(string type, ref bool isnum_iid, ref string reqName)
        {
            switch (type)
            {//1修改标题,2.修改价格,3.修改邮费,4.修改库存,5修改描述,6.修改类目,7.编码,8.卖点,9.基本属性
                case "1":
                    isnum_iid = false;
                    reqName = "num_iid,title,pic_url,price,postage_id,num,outer_id,seller_cids,cid";
                    break;
                case "2":
                    isnum_iid = true;
                    reqName = "title,pic_url,num_iid,price,postage_id,num,outer_id,seller_cids,approve_status,wap_desc,desc,wireless_desc,sell_point,has_discount,freight_payer,has_invoice,has_warranty,postage_id,sell_promise,after_sale_id,sub_stock,sku";
                    break;
                case "3":
                    isnum_iid = false;
                    reqName = "num_iid,title,pic_url,price,postage_id,num,outer_id,seller_cids,cid";
                    break;
                case "4":
                    isnum_iid = true;
                    reqName = "title,pic_url,num_iid,price,postage_id,num,outer_id,seller_cids,approve_status,wap_desc,desc,wireless_desc,sell_point,has_discount,freight_payer,has_invoice,has_warranty,postage_id,sell_promise,after_sale_id,sub_stock,sku";
                    break;
                case "5":
                    isnum_iid = true;
                    reqName = "num_iid,title,desc,pic_url";
                    break;
                case "6":
                    isnum_iid = false;
                    reqName = "num_iid,title,pic_url,price,postage_id,num,outer_id,seller_cids,cid";
                    break;
                case "7":
                    isnum_iid = false;
                    reqName = "num_iid,title,pic_url,price,postage_id,num,outer_id,seller_cids,cid";
                    break;
                case "8":
                    isnum_iid = true;
                    reqName = "num_iid,title,sell_point";
                    break;
                case "9":
                    isnum_iid = true;
                    reqName = "title,pic_url,num_iid,price,postage_id,num,outer_id,seller_cids,approve_status,wap_desc,desc,wireless_desc,sell_point,has_discount,freight_payer,has_invoice,has_warranty,postage_id,sell_promise,after_sale_id,sub_stock,sku";
                    break;
                case "10":
                    isnum_iid = false;
                    reqName = "num_iid,title,pic_url,price,postage_id,num,outer_id,seller_cids,cid";
                    break;
                case "违禁词":
                    isnum_iid = true;
                    reqName = "num_iid,title,pic_url,item_imgs,desc,item_img";
                    break;
            }
        }
        #endregion
    }

    public class SearEntity {

        public SearEntity(string num_iid,string types)
        {
            numIid = num_iid;type = types;
        }
        public SearEntity(string num_iid, string types,int useplatform)
        {
            numIid = num_iid; type = types; this.useplatform = useplatform;
        }
        public SearEntity(string statuss,string sellerCats,string title, string types)
        {
            this.statuss = statuss; type = types;this.sellerCats = sellerCats;this.title = title; 
        }
        public SearEntity(string statuss, string sellerCats, string title, string types, int useplatform)
        {
            this.statuss = statuss; type = types; this.sellerCats = sellerCats; this.title = title; this.useplatform = useplatform;
        }
        public int useplatform { get; set; }
        /// <summary>
        /// 宝贝id，隔开
        /// </summary>
        public string  numIid { get; set; }
        /// <summary>
        /// 是否出售中 true 是 false 不是
        /// </summary>
        public string statuss { get; set; }
        /// <summary>
        /// 类目id
        /// </summary>
        public string sellerCats { get; set; }
        /// <summary>
        /// 宝贝名称
        /// </summary>
        public string title { get; set; }
        /// <summary>
        /// 调用者
        /// </summary>
        public string type { get; set; }
    }

}
