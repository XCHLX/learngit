using DbAccess;
using ImageDetection;
using SearchItem;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Top.Api.Domain;
using System.Threading;
using System.Configuration;
using Memcached;
using CrmCommon;
namespace PIC
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            MainThread();
        }
        public Form1(string i)
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            MainThread();
            this.Text ="违禁词检测"+ i;
            this.textBox1.Text = i;
        }
        MySqlHelper db = new MySqlHelper();
        MySqlHelper dbtool = new MySqlHelper(DataType.ToolsProducts);
        public readonly static RedisClientHandle rch = new RedisClientHandle();
        readonly MySqlHelper dbsp = new MySqlHelper("GoodsToolsProducts", DataType.Products);
        int useplatform = 0;
        private void button1_Click(object sender, EventArgs e)
        {
            TxtLog.WriteLine("数据库 chl",dbsp.ConnectionString);
            MainThread();
            
        }
     

        private void MainThread()
        {
            TxtLog.WriteLine("数据库 chl", dbsp.ConnectionString);
            ThreadStart threadStart = delegate
            {
                do
                {
                    DataTable mqsDt = null;
                    string mqsSql = "select * from crm_attributesprodetect where   tools=" + textBox1.Text.Trim();
                    mqsDt = dbtool.ExecuteDataTable(mqsSql);
                    if (mqsDt != null && mqsDt.Rows.Count > 0)
                    {
                        Mypic mp = new Mypic();
                        this.SendMain(mqsDt);
                    }
                    Thread.Sleep(1000 * 10);//休息10s
                } while (true);
                
            };
            Thread thread = new Thread(threadStart);
            thread.IsBackground = true;
            thread.Start();
        }

        #region 开始方法
        public bool SendMain(DataTable dt)
        {
            string activityId = dt.Rows[0]["id"].ToString();
            useplatform = int.Parse(dt.Rows[0]["useplatform"].ToString());
            MySqlHelper dbtool = new MySqlHelper(DataType.ToolsProducts);
            try
            {
                //string sqlActivity = string.Format("select * from crm_attributesprodetect where    id='{0}'", activityId);
                //DataTable dt = dbtool.ExecuteDataTable(sqlActivity);
                Dictionary<string, Item> dicItem = new Dictionary<string, Item>();//存储宝贝信息的集合
                if (dt != null && dt.Rows.Count > 0)
                {
                    ShowSend("检测开始");
                    SellerInfoEntity sellerinfo = GetSellerinfo(dt.Rows[0]);
                    dicItem = GetItem(sellerinfo, dt.Rows[0]);
                    string tabelName = sellerinfo.strUserId.Substring(sellerinfo.strUserId.Length - 1);
                    string cpsql = string.Format("delete from crm_prohibiteword_detail{0} where mainid1 ='{1}'", tabelName, activityId);
                    dbtool.ExecuteNonQuery(cpsql);
                    TaskDetection(dicItem,sellerinfo,activityId, dt.Rows[0]["activity_type"].ToString());
                    ShowSend("检测完成");
                }
                return true;
            }
            catch (Exception ex)
            {
                TxtLog.WriteLine("图片违禁词检测", "主程序异常" + ex.ToString());
                return false;
            }
            finally
            {
                rch.Delete(activityId);
                //结束任务
                string sqlactivity = string.Format("UPDATE crm_attributesprodetect SET tools ='-1',  check_state = 2  WHERE id = '{0}';", activityId);
                dbtool.ExecuteNonQuery(sqlactivity);
                
            };
        }
        #endregion

        #region 开始任务
        public void TaskDetection(Dictionary<string, Item> dicItem, SellerInfoEntity sellerinfo,string activityId, string activityType)
        {
            try
            {
                if (dicItem==null)
                {
                    TxtLog.WriteLine("getItemListErrorLog", sellerinfo.strUserNick + ":" + sellerinfo.strUserId + "=== ：出现异常:获取宝贝为空" + sellerinfo.top_session);
                    return;
                }
                string tableindex = sellerinfo.strUserId.Substring(sellerinfo.strUserId.Length-1,1);
                MySqlHelper dbtool = new MySqlHelper(DataType.ToolsProducts);
                string sql = "select proword from crm_prohibitedword where is_del='0'";
                string sqluser = "select proword from crm_userprohibitedword"+ tableindex + " where is_del='0' and seller_id ="+ sellerinfo.strUserId;
                DataTable dt = dbtool.ExecuteDataTable(sql);
                DataTable dtuser = dbtool.ExecuteDataTable(sqluser);
                string ItemImgsError = string.Empty;
                string titleword = string.Empty;//标题异常
                string desc = string.Empty; //详情异常
                string pictext = string.Empty;
                Mypic myp = new Mypic();
                int itemcount = 0;
                foreach (var item in dicItem.Values)
                {//'任务类型 0,文字检测 1图片检测 3 全部检测'
                    titleword = ""; desc = ""; ItemImgsError = "";
                    switch (activityType)
                    {
                        case "0"://文字检测
                            myp.DetectionTitelDesc(dtuser,dt, item, ref titleword, ref desc);
                            break;
                        case "1"://图片检测
                            myp.DetectionPIC(dtuser, dt, item, ref ItemImgsError, ref pictext);
                            break;
                        case "3"://  全部检测'
                            myp.DetectionTitelDesc(dtuser, dt, item, ref titleword, ref desc);
                            myp.DetectionPIC(dtuser, dt, item, ref ItemImgsError, ref pictext);
                            break;
                        default:
                            break;
                    }
                    if (pictext != null && pictext.Contains("Open api daily request limit reached"))
                    {
                        LogApi.MonitorLog.Write(LogApi.LogType.Error, LogApi.Products.CRM, LogApi.VersionM.Standard, "图片检测欠费", sellerinfo.strUserId, sellerinfo.strUserNick, "", " 检测api欠费:活动id:" + activityId);
                        return;
                    }
                    else if (pictext != null && pictext.Contains("Open api qps request limit reached"))
                    {
                        LogApi.MonitorLog.Write(LogApi.LogType.Error, LogApi.Products.CRM, LogApi.VersionM.Standard, "图片检测欠费", sellerinfo.strUserId, sellerinfo.strUserNick, "", " 检测api限流 活动id:" + activityId);
                        return;
                    }
                    itemcount++;
                    ShowSend("开始检测" + DateTime.Now.ToString() + "宝贝id" + item.NumIid + itemcount + "/" + dicItem.Count + "用户id" + sellerinfo.strUserId);
                    //任务进度写进缓存
                    Sethc(activityId, itemcount + "/" + dicItem.Count);
                    string errors = titleword + desc + ItemImgsError;
                    string tabelName = sellerinfo.strUserId.Substring(sellerinfo.strUserId.Length - 1);
                    if (!string.IsNullOrWhiteSpace(errors))
                    {
                        if (!string.IsNullOrWhiteSpace(ItemImgsError))
                        {
                            ItemImgsError = "[" + ItemImgsError.Substring(0, ItemImgsError.Length - 1) + "]";
                        }
                        string psql = string.Format("INSERT INTO  `crm_prohibiteword_detail{0}` (`id`, `mainid1`, `seller_id`, `num_id`, `title` , `pic_url`, `create_time`, `title_fail_reason`, `sellpoint_fail_reason`, `desc_fail_reason`, picture_fail_reason1,is_error) VALUES ('{1}', '{2}', '{3}', '{4}', '{5}' , '{6}', now(), '{7}', '', '{8}','{9}',{10} );", tabelName, Guid.NewGuid(), activityId, sellerinfo.strUserId, item.NumIid, item.Title.Replace("'", ""), item.PicUrl, titleword, desc, ItemImgsError, "1");
                        dbtool.ExecuteNonQuery(psql);
                    }
                    else
                    {
                        string psql = string.Format("INSERT INTO  `crm_prohibiteword_detail{0}` (`id`, `mainid1`, `seller_id`, `num_id`, `title` , `pic_url`, `create_time`, `title_fail_reason`, `sellpoint_fail_reason`, `desc_fail_reason`, picture_fail_reason1,is_error) VALUES ('{1}', '{2}', '{3}', '{4}', '{5}' , '{6}', now(), '{7}', '', '{8}','{9}' ,{10});", tabelName, Guid.NewGuid(), activityId, sellerinfo.strUserId, item.NumIid, item.Title.Replace("'", ""), item.PicUrl, titleword, desc, ItemImgsError,"0");
                        dbtool.ExecuteNonQuery(psql);
                    }
                    
                }
            }
            catch (Exception ex)
            {
                TxtLog.WriteLine("图片违禁词检测", ex.ToString());
            }
        }
        #endregion

      
        #region 获取用户信息
        /// <summary>
        /// 获取用户信息
        /// </summary>
        /// <param name="dr">任务信息</param>
        /// <returns></returns>
        private SellerInfoEntity GetSellerinfo(DataRow dr)
        {   //获取用户信息
            string seller_id = dr["seller_id"].ToString();//用户id
            MySqlHelper db = new MySqlHelper();
            SellerInfoEntity sellerinfo = new SellerInfoEntity();
            DataTable sellerDt = new DataTable();
            if (dr["useplatform"].ToString() == "2")
            {
                sellerDt = dbsp.ExecuteDataTable("select id,userid,nick,shopname,lastvisit,sessionkey,shoptype from crm_sellerinfo where userid='" + seller_id + "';");
            }
            else
            {
                sellerDt = db.ExecuteDataTable("select id,userid,nick,shopname,lastvisit,sessionkey,shoptype from crm_sellerinfo where userid='" + seller_id + "';");
            }
            //sellerDt = db.ExecuteDataTable("select id,userid,nick,shopname,lastvisit,sessionkey,shoptype from crm_sellerinfo where userid='" + seller_id + "';");
            //绑定卖家信息
            if (sellerDt != null && sellerDt.Rows.Count > 0)
            {
                sellerinfo.strUserId = sellerDt.Rows[0]["userid"].ToString();
                sellerinfo.strUserNick = sellerDt.Rows[0]["nick"].ToString();
                sellerinfo.sellerType = sellerDt.Rows[0]["shopType"].ToString();
                sellerinfo.top_session = sellerDt.Rows[0]["sessionkey"].ToString();
                //sellerinfo.top_session = "61012099c061511621c594ZZ0865780c3e824303bc0ed0e2929003048";

            }
            return sellerinfo;
        }
        #endregion

        #region 获取需要执行的宝贝对象信息
        /// <summary>
        /// 获取需要执行的宝贝对象信息
        /// </summary>
        /// <param name="sellerinfo">用户信息</param>
        /// <param name="dr">任务信息</param>
        /// <returns></returns>
        private Dictionary<string, Item> GetItem(SellerInfoEntity sellerinfo, DataRow dr)
        {
            //获取提交任务的条件判断哪些宝贝需要执行检测
            string seller_id = dr["seller_id"].ToString();//用户id
            string check_type = dr["check_type"].ToString();//宝贝条件  0：出售中商品检测 1：仓库中商品检测' 3 全部
            string total_count = dr["total_count"].ToString(); //提交宝贝总数
            string num_iid = dr["num_iid"].ToString();//宝贝id 为主要条件
            //获取宝贝
            SearEntity sear = null;
            if (string.IsNullOrWhiteSpace(num_iid))
            {
                sear = new SearEntity(check_type, "", "", "违禁词",useplatform);
            }
            else
            {
                sear = new SearEntity(num_iid, "违禁词",useplatform);
            }
            // 是否出售中 ,类目id,标题,调用者
            return SearchItems.Search(sear, sellerinfo);
        }

        #endregion
        #region 获得字符串中开始和结束字符串中间得值
        /// <summary>
        /// 获得字符串中开始和结束字符串中间得值
        /// </summary>
        /// <param name="str">字符串</param>
        /// <param name="s">开始</param>
        /// <param name="e">结束</param>
        /// <returns></returns>
        public MatchCollection MatchValues(string str, string s, string e)
        {
            Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Matches(str);
        }
        #endregion
        #region 写缓存
        private void Sethc(string activityid, string msg)
        {
            Object obj = null;
            obj = rch.Set(activityid, msg);
        }
        #endregion
        #region 发送情况显示到界面
        private void ShowSend(string strMsg)
        {
            try
            {
                this.listBox1.Items.Insert(0, strMsg + "(" + DateTime.Now.ToString() + ")");
            }
            catch (Exception ex)
            {
            }
            //记录文本日志
            if (strMsg.Contains("主线程出错："))
            {
                TxtLog.WriteLine("宝贝属性检测MQS分配ErrorLog_", strMsg);
            }

        }
        #endregion
        #region 重启方法
        /// <summary>
        /// 重启方法
        /// </summary>
        /// <param name="parajson"></param>
        /// <returns></returns>
        public bool RestartMain(string parajson)
        {
            return true;
        }
        #endregion

        private void button2_Click(object sender, EventArgs e)
        {
            
        }
    }
}
