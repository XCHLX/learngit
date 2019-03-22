using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using CrmCommon;
using DbAccess;
using Top.Api.Request;
using Top.Api.Response;
using Top.Api.Domain;
using System.IO;
using System.Threading;
//using GoodsTaobaoCommon;
using TaobaoCommon;
using System.Net;
using LogApi; 
namespace CopyItems
{
    public partial class Form1 : Form
    {
        public bool state = false;
        public bool isrunning = false;
        private DateTime recordtime = DateTime.Now;
        private int QueryTime = 1000;
        readonly MySqlHelper dbsp = new MySqlHelper("GoodsToolsProducts", DataType.Products);
        MySqlHelper sql1 = new MySqlHelper("Main", DbAccess.DataType.Main);
        MySqlHelper sql = new MySqlHelper("ToolsProducts", DbAccess.DataType.tools);
        //MySqlHelper sql = new MySqlHelper("GoodsProducts");
        //MySqlHelper sql1 = new MySqlHelper("GoodsProducts");
        public string sellderid = string.Empty;
        public Form1()
        {

            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            //TmallItemUpdateSchemaGetRequest req = new TmallItemUpdateSchemaGetRequest();
            //req.ItemId = 527194401533;
            //TmallItemUpdateSchemaGetResponse aa = TopClient.Execute(req, "6200a285984a4af78549ad39b0f87bf7ac24ZZ1e8ZZ2f4e644627753");

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
        /// 获取一条复制的宝贝
        /// </summary>
        /// <returns></returns>
        public void Getids()
        {
            string shoptype = string.Empty;//复制类型
            DataTable dt = sql.ExecuteDataTable("select * from crm_productscopy where state='4'    and shoptype='0' ");//获取未处理的宝贝,id复制
            //DataTable dt = sql.ExecuteDataTable("select * from crm_productscopy where state='0' and shoptype='0' and sellderid like '%" + textBox1.Text.ToString().Trim() + "'");//获取未处理的宝贝,id复制
            //DataTable dt = sql.ExecuteDataTable("select * from crm_productscopy where state='0' and shoptype='1' and sellderid like '%" + textBox1.Text.ToString().Trim() + "'");//获取未处理的宝贝,全店复制


            //#region 异常任务重新复制
            //DataTable dt = sql.ExecuteDataTable("select * from crm_productscopychild  WHERE TIMESTAMPDIFF(MINUTE,addtime,now())>100 and addtime is not null and state=0");
            //if (dt != null)
            //{
            //    foreach (DataRow dr in dt.Rows)
            //    {
            //        this.listBox1.Items.Insert(0, "【" + sellderid + "】【单个宝贝id:" + dr["id"].ToString() + "】【任务ID:" + dr["pid"].ToString() + "】重新复制(" + DateTime.Now.ToString() + ")");
            //        sql.ExecuteNonQuery("delete  from crm_productscopychild where id='" + dr["id"].ToString() + "'");
            //        sql.ExecuteNonQuery("update crm_productscopy set state=0 where id='" + dr["pid"].ToString() + "'");
            //    }
            //}
            //#endregion


            #region 复制功能
            //DataTable dt = sql.ExecuteDataTable("select * from crm_productscopy where state='0' and shoptype='1' and sellderid like '%"+textBox1.Text.ToString().Trim()+"'");//获取未处理的宝贝,全店复制
            Copy cp = new Copy();
            TmallCopy tc = new TmallCopy();
            string ids = string.Empty;
            if (dt != null && dt.Rows.Count > 0)
            {
                int couts = 0;
                cp.dt = dt;
                tc.dt = dt;
                sellderid = dt.Rows[0]["sellderid"].ToString();
                string uid = dt.Rows[0]["sellderid"].ToString();
                string useplatform = dt.Rows[0]["useplatform"].ToString();
                cp.useplatform = Convert.ToInt32( useplatform);
                tc.useplatform = Convert.ToInt32(useplatform);
                // DataTable dt1 = sql1.ExecuteDataTable("select * from crm_sellerinfo where userid='" + uid + "'")
                DataTable dt1 = null;
                if (useplatform =="2")
                {
                      dt1 = dbsp.ExecuteDataTable("select * from crm_sellerinfo where userid='" + uid + "'");
                }
                else
                {
                    dt1 = sql1.ExecuteDataTable("select * from crm_sellerinfo where userid='" + uid + "'");
                   
                }
                cp.sessionKey = dt1.Rows[0]["SessionKey"].ToString();
                tc.sessionKey = dt1.Rows[0]["SessionKey"].ToString();
                //cp.sessionKey = "6201a25e36ce1973ce987c52b05163a6egi61aZZf135ab61805984332";
                // tc.sessionKey = "61011132b3c5a6d3941df1260bZZ477b572153033640b952769513611";
                ids = dt.Rows[0]["oldid"].ToString();
                shoptype = dt.Rows[0]["shoptype"].ToString();//0:id复制,1:全店复制
                if (shoptype == "1")
                {
                    ShopGetRequest req = new ShopGetRequest();
                    req.Fields = "sid,cid,title,nick,desc,bulletin,pic_path,created,modified";
                    req.Nick = ids;
                    ShopGetResponse rsp = TopClient.Execute(req);
                    if (rsp.Shop != null)
                    {
                        string url = "https://shop" + rsp.Shop.Sid + ".taobao.com/";
                        cp.GetIds(url);
                        ids = cp.ids;
                        //writlog("ids", ids);
                    }
                }
                if (ids.Contains(","))
                {
                    couts = ids.Substring(ids.Length - 1) == "," ? ids.Split(',').Length - 1 : ids.Split(',').Length;
                }
                else
                {
                    couts = 1;
                }
                sql.ExecuteNonQuery("update crm_productscopy set state='3',nums='" + (couts > 500 ? 500 : couts) + "' where id='" + dt.Rows[0]["id"].ToString() + "'");
                int nums = 0;//当前执行的总数量
               string stype = dt1.Rows[0]["shopType"].ToString();//b店 or C店
               // string stype = "B";
                if (ids.Contains(","))
                {
                    string[] aa = ids.Split(',');
                    foreach (string id in aa)
                    {
                        if ((sql.ExecuteDataTable("select id from crm_productscopychild where bbid='" + id + "' ")).Rows.Count == 0)
                        {
                            nums = nums + 1;
                        }
                        if (nums == 0)
                        {
                            nums = Convert.ToInt32((sql.ExecuteDataTable("select id from crm_productscopychild where pid='" + dt.Rows[0]["id"].ToString() + "'")).Rows.Count);
                        }
                        if (nums > 500)
                        {
                            //this.listBox1.Items.Insert(0, nums + ":0:" + dt.Rows[0]["id"].ToString());
                            this.listBox1.Items.Insert(0, "【" + sellderid + "】【已复制500个宝贝】");
                            break;
                        }
                        //if (aa.Length > 500)
                        //{
                        //    if (aa[500].ToString() == id)
                        //    {
                        //        //this.listBox1.Items.Insert(0, nums+":1");
                        //        this.listBox1.Items.Insert(0, "【" + sellderid + "】【已复制500个宝贝】");
                        //        break;
                        //    }
                        //}
                        if (dt.Rows[0]["oldid"].ToString() == "大牌小价01")
                        {
                            sql.ExecuteNonQuery("insert into crm_productscopychild(id,pid,bbid,state,addtime,remark) values ('" + Guid.NewGuid().ToString() + "','" + dt.Rows[0]["id"].ToString() + "','" + id + "','2','" + System.DateTime.Now.ToString() + "','因类目调整当前类目已被淘宝网关闭，请您将商品发布在其他相应类目下, 本类目已不能发布商品, 请移至其他类目进行发布!')");
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(id))
                            {
                                if (checkid(id, dt.Rows[0]["id"].ToString(), dt.Rows[0]["is_gl"].ToString()))
                                {
                                    string a = AddChildCopy(id, dt.Rows[0]["id"].ToString(), shoptype, sellderid, dt.Rows[0]["is_gl"].ToString());
                                    if (a != "0")//添加宝贝复制
                                    {
                                        this.listBox1.Items.Insert(0, "【" + sellderid + "】【任务ID:" + a + "】开始复制(" + DateTime.Now.ToString() + ")");
                                        if (stype == "B")
                                        {
                                            try
                                            {
                                                tc.AddTaobao(id, a, sellderid, this.textBox1.Text);
                                            }
                                            catch (Exception ex)
                                            {
                                                writlog("copy", ex.ToString());
                                            }
                                        }
                                        else
                                        {
                                            //tc.AddTaobao(id, a, sellderid);
                                            try
                                            {
                                                cp.AddTaobao(id, a, sellderid, this.textBox1.Text);
                                            }
                                            catch (Exception ex)
                                            {
                                                writlog("copy", ex.ToString());
                                            }
                                        }
                                        this.listBox1.Items.Insert(0, "【" + sellderid + "】【任务ID:" + a + "】复制成功(" + DateTime.Now.ToString() + ")");
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {

                    string a = AddChildCopy(ids, dt.Rows[0]["id"].ToString(), shoptype, sellderid, dt.Rows[0]["is_gl"].ToString());
                    if (a != "0")//添加宝贝复制
                    {
                        this.listBox1.Items.Insert(0, "【" + sellderid + "】【任务ID:" + a + "】开始复制(" + DateTime.Now.ToString() + ")");
                        if (stype == "B")
                        {
                            tc.AddTaobao(ids, a, sellderid, this.textBox1.Text);
                        }
                        else
                        {
                            //tc.AddTaobao(ids, a, sellderid);
                            cp.AddTaobao(ids, a, sellderid, this.textBox1.Text);
                        }
                        //cp.AddTaobao(ids, a, sellderid);
                        this.listBox1.Items.Insert(0, "【" + sellderid + "】【任务ID:" + a + "】复制成功(" + DateTime.Now.ToString() + ")");
                    }
                    //}
                }
                sql.ExecuteNonQuery("update crm_productscopy set state='1' where id='" + dt.Rows[0]["id"].ToString() + "'");
                sql.ExecuteNonQuery("update crm_productscopychild set state='2' where id='" + dt.Rows[0]["id"].ToString() + "' and state='3' ");
                sql.ExecuteNonQuery("update crm_productscopychild set state='2' where id='" + dt.Rows[0]["id"].ToString() + "' and state='0' ");
            }
            #endregion

        }

        public string AddChildCopy(string id, string pid, string shoptype, string sellderid, string gl)
        {
            string a = Guid.NewGuid().ToString();
            string res = "0";
            string res1 = "0";
            string mysql = string.Empty;
            if (gl == "1")
            {
                if (shoptype == "0")
                {
                    mysql = "select id from crm_productscopychild where pid='" + pid + "' and bbid='" + id + "' ";
                }
                else
                {
                    DataTable pdt = sql.ExecuteDataTable("select id from crm_productscopy where sellderid=" + sellderid + " ");
                    mysql += "select id from crm_productscopychild where  bbid='" + id + "' and state='1' and pid in ( ";
                    if (pdt != null)
                    {
                        foreach (DataRow dr in pdt.Rows)
                        {
                            mysql += "'" + dr["id"].ToString() + "',";
                        }
                    }
                    mysql += "'0')";
                    //mysql += "'"+pid+"')";
                }

                DataTable dt = sql.ExecuteDataTable(mysql);
                if (dt != null && dt.Rows.Count > 0)
                { res = dt.Rows[0]["id"].ToString(); }
                if (res == "0")
                {
                    res1 = sql.ExecuteNonQuery("insert into crm_productscopychild(id,pid,bbid,state,addtime) values ('" + a + "','" + pid + "','" + id + "','0','" + System.DateTime.Now.ToString() + "')").ToString();
                }
                if (res1 == "1")
                {
                    res1 = a;
                }
            }
            else
            {
                res = sql.ExecuteNonQuery("insert into crm_productscopychild(id,pid,bbid,state,addtime) values ('" + a + "','" + pid + "','" + id + "','0','" + System.DateTime.Now.ToString() + "')").ToString();
                res1 = a;
            }
            return res1;
        }


        /// <summary>
        /// 判断该id是否已执行
        /// </summary>
        /// <param name="id"></param>
        /// <param name="pid"></param>
        /// <returns></returns>
        public bool checkid(string id, string pid, string gl)
        {
            try
            {
                if (gl == "1")
                {
                    int res = sql.ExecuteDataTable("select * from crm_productscopychild where pid='" + pid + "' and bbid='" + id + "' ").Rows.Count;
                    if (res == 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return true;
            }
        }

        public string GetHtml(string Url)
        {
            string str = "";
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls;
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

        private void button1_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(this.textBox1.Text))
            {
                MessageBox.Show("请输入后缀id");
            }
            else
            {
                //this.Text = "全店宝贝复制" + this.textBox1.Text;
                if (this.button1.Text == "开始")
                {
                    //记录心跳程序
                    if (recordtime.AddMinutes(30) <= DateTime.Now)
                    {
                        //ToolsModified.Heartbeat("好多商品全店宝贝复制" + this.textBox1.Text + "正常运行中");
                        ToolsModified.Heartbeat("宝贝复制正常运行中");
                        recordtime = DateTime.Now;
                    }
                    this.button1.Text = "停止";
                    System.Timers.Timer t = new System.Timers.Timer(10000);   //设置间隔时间为10000毫秒；   
                    t.Elapsed += new System.Timers.ElapsedEventHandler(t_Elapsed); //到达时间的时候执行事件；   
                    t.AutoReset = true;   //设置是执行一次（false）还是一直执行(true)；   
                    t.Enabled = true;     //是否执行System.Timers.Timer.Elapsed事件；
                    //this.MainThread();
                }
                else
                {
                    this.state = false;
                    this.button1.Text = "开始";
                }
            }
        }
        private void t_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.state == false)
            {
                this.state = true;
                MainThread();
            }
        }

        #region 主线程
        private void MainThread()
        {
            
            ThreadStart threadStart = delegate
            {
                do
                {
                    if (this.state)
                    {
                        if (!this.isrunning)
                        {
                            this.isrunning = true;
                            this.AutoClear();
                            try
                            {

                                Getids();
                            }
                            catch (Exception e)
                            {

                                MonitorLog.Write(LogType.Exception, Products.CRM, VersionM.Standard, "宝贝复制" + this.textBox1.Text, sellderid, "", "宝贝复制任务异常", e.ToString());
                                //MonitorLog.Write(LogType.Exception, Products.CRM, VersionM.Standard, "宝贝复制", sellderid, "", "全店任务异常", e.ToString());
                                //MonitorLog.Write(LogType.Exception, Products.QNProduct, VersionM.Standard, "好多商品-宝贝复制-全店","","", "全店任务异常", e.ToString());
                                //MonitorLog.Write(LogType.Exception, Products.QNProduct, VersionM.Standard, "好多商品-宝贝复制", sellderid, "", "全店任务异常", e.ToString());
                                this.listBox1.Items.Insert(0, "主线程出错：【" + sellderid + "】" + e.Message + "(" + DateTime.Now.ToString() + ")");

                            }
                            // this.listBox1.Items.Insert(0, "【" + sellderid + "】复制成功(" + DateTime.Now.ToString() + ")");
                            this.isrunning = false;
                            this.state = false;
                        }

                    }
                    else
                    {
                        //this.button1.Text = "开启";

                        break;
                    }
                } while (true);
            };
            //DataTable dt = sql.ExecuteDataTable("select * from crm_productscopy where state='0' and shoptype='1'");//获取未处理的宝贝,全店复制
            //foreach(DataRow dr in dt.Rows)
            //{

            //}
            //Thread thread = null;
            //for (int i = 0; i <5; i++)
            //{
            //    thread = new Thread(threadStart);
            //    thread.Name = "Thread" + i;
            //    thread.SetApartmentState(ApartmentState.STA);
            //    thread.IsBackground = true;
            //    thread.Start();
            //}
            Thread mainThread = new Thread(threadStart);
            mainThread.SetApartmentState(ApartmentState.STA);
            mainThread.IsBackground = true;
            mainThread.Start();
        }
        #endregion

        #region 发送情况显示到界面
        private void ShowSend(string strMsg)
        {
            this.listBox1.Items.Insert(0, strMsg + "(" + DateTime.Now.ToString() + ")");
        }
        #endregion

        #region 自动清除历史记录
        private void AutoClear()
        {
            if (this.listBox1.Items.Count > 100)
            {
                this.listBox1.Items.Clear();
            }
        }
        #endregion

        private void deletes()
        {
            int res = sql.ExecuteNonQuery("delete from  crm_productscopychild where id='88cfe673-e3a2-45e9-a2c5-c2bbed741452'");
            int res1 = sql.ExecuteNonQuery("update crm_productscopy set state=1 where id='07ce2bbd-c072-4a86-9c71-d4f0d4584948'");
        }

        private void button2_Click(object sender, EventArgs e)
        {

            if (this.tbsellderid.Text == "")
            {
                MessageBox.Show("卖家id必填", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                string childsql = "";
                string psql = "";
                //childsql = "delete  from crm_productscopychild where id='" + this.tbchildid + "'";
                //psql = "update crm_productscopy set state=0 where id in('767a4bcb-eb33-4fd5-ba25-d508b82b1adb')";
                if (this.tbchildid.Text == "" && this.tbpid.Text == "")//判断是否全部任务重新复制
                {
                    //true；全部重新复制
                    if (checkBox1.Checked == true)
                    {
                        DataTable dt = sql.ExecuteDataTable("select * from crm_productscopy where sellderid='" + this.tbsellderid.Text.ToString() + "'");
                        if (dt != null && dt.Rows.Count > 0)
                        {
                            foreach (DataRow dr in dt.Rows)
                            {
                                DataTable dt1 = sql.ExecuteDataTable("select * from crm_productscopychild where pid='" + dr["id"].ToString() + "'");
                                if (dt1 != null && dt1.Rows.Count > 0)
                                {
                                    foreach (DataRow dr1 in dt1.Rows)
                                    {
                                        childsql = "delete  from crm_productscopychild where id='" + dr1["id"].ToString() + "'";
                                        sql.ExecuteNonQuery(childsql);
                                    }
                                    psql = "update crm_productscopy set state=0 where id='" + dr["id"].ToString() + "'";
                                    sql.ExecuteNonQuery(psql);
                                }
                            }

                        }
                    }
                }
                else
                {
                    if (this.tbpid.Text != "" && this.tbchildid.Text == "")
                    {//单个任务重新复制
                        childsql = "delete  from crm_productscopychild where pid='" + this.tbpid.Text + "'";
                        psql = "update crm_productscopy set state=0 where id='" + this.tbpid.Text + "'";
                        sql.ExecuteNonQuery(childsql);
                        sql.ExecuteNonQuery(psql);
                    }
                    if (this.tbpid.Text != "" && this.tbchildid.Text != "")
                    {
                        childsql = "delete  from crm_productscopychild where id='" + this.tbchildid.Text + "'";
                        psql = "update crm_productscopy set state=0 where id='" + this.tbpid.Text + "'";
                        sql.ExecuteNonQuery(childsql);
                        sql.ExecuteNonQuery(psql);
                    }
                }
                MessageBox.Show("操作成功!", "提示信息", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        private static void ThreadMethod(object t)
        {
            Thread.Sleep(1500);
            Console.WriteLine(String.Format("我是 Thread{0}", t));
        }

        public void startthread()
        {
            Thread thread = null;
            for (int i = 1; i > 0; i++)
            {
                thread = new Thread(ThreadMethod);
                thread.Name = "Thread" + i;
                Console.WriteLine("创建 Thread" + i);
                thread.Start(i);
                Thread.Sleep(i * 100);
            }
        }
    }
}
