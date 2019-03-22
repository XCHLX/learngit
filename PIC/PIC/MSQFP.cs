using CrmCommon;
using DbAccess;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using ImageDetection;
using ToolsControls;
using Newtonsoft.Json;
namespace PIC
{
    public partial class MSQFP : Form
    {
        private Thread mainThread;
        private bool state = false;
        private bool isrunning = false;
        private int QueryTime = 10000;
        private DateTime runTime = DateTime.Now;
        private bool isFirst = true;
        MySqlHelper db = new MySqlHelper();
        MySqlHelper dbtool = new MySqlHelper(DbAccess.DataType.ToolsProducts);
        public MSQFP()
        {
            InitializeComponent();
            Control.CheckForIllegalCrossThreadCalls = false;
            #region 自动部署
            //TxtLog.WriteLine("picyc", "开始执行");
            //try
            //{
            //    string[] CmdArgs = System.Environment.GetCommandLineArgs();
            //    if (CmdArgs.Length > 1)
            //    {
            //        TxtLog.WriteLine("picyc", "有工具");
            //        try
            //        {
            //            Dictionary<string, object> obj = ToolsJson.GetToolJson();
            //            txtToolCount.Text = obj["number"].ToString();
            //            ToolsModified.Heartbeat("违禁词测新MQS分配", ToolsJson.toolmainid);
            //            TxtLog.WriteLine("picyc", "json参数"+obj["number"].ToString());
            //            button1_Click(btnBegin, new EventArgs());
            //        }
            //        catch(Exception e)
            //        {
            //            TxtLog.WriteLine("picyc",e.ToString());
            //        }
            //    }
            //    else
            //    {
            //        TxtLog.WriteLine("picyc", "没有工具");
            //    }
            //}
            //catch (Exception e)
            //{
            //    TxtLog.WriteLine("picyc", e.ToString());
            //} 
            #endregion
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.btnBegin.Text == "开启")
                {
                    if (txtToolCount.Text.Trim() == "")
                    {
                        MessageBox.Show("工具数不能为空");
                    }
                    int ii = Convert.ToInt32(txtToolCount.Text);

                    for (int i = 0; i < ii; i++)
                    {
                        Form1 f1 = new Form1(i.ToString());
                        f1.Show();
                    }

                    this.state = true;
                    this.btnBegin.Text = "停止";
                    this.MainThread();
                }
                else
                {
                    this.state = false;
                    this.btnBegin.Text = "开启";
                }
            }
            catch (Exception ex)
            {
                TxtLog.WriteLine("picyc", ex.ToString());
            }
        }
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
                            try
                            {
                                ProAttriuMqs();
                            }
                            catch (Exception e)
                            {
                                this.ShowSend("主线程出错：" + e.Message);
                            }
                            this.isrunning = false;
                        }
                        Thread.Sleep(this.QueryTime);
                    }
                    else
                    {
                        this.btnBegin.Text = "开启";

                        break;
                    }
                } while (true);
            };
            mainThread = new Thread(threadStart);
            mainThread.IsBackground = true;
            mainThread.Start();
        }
        #region 违禁词检测文字分配
        private void ProAttriuMqs()
        {
            try
            {
                
                string toolsql = string.Empty;
                DataTable dtMqs = dbtool.ExecuteDataTable("select * from crm_attributesprodetect where tools <>-1");
                List<string> listMqs = new List<string>();
                foreach (DataRow dr in dtMqs.Rows)
                {
                    listMqs.Add( dr["tools"].ToString());
                }
                DataTable dtInp = dbtool.ExecuteDataTable("select * from crm_attributesprodetect where check_state=4;");//获取图片检测和全部检测的任务
                if (dtInp != null && dtInp.Rows.Count > 0)
                {
                    //导入任务中站点索引
                    for (int c = 0; c < dtInp.Rows.Count; c++)
                    {
                        if (listMqs.Count == Convert.ToInt32(txtToolCount.Text.Trim()))
                        {
                            ShowSend("没有空余工具。");
                            break;
                        }
                        for (int i = 0; i < Convert.ToInt32(txtToolCount.Text.Trim()); i++)
                        {
                            if (!listMqs.Contains(i.ToString()))
                            {
                                ShowSend("开始分配"+ dtInp.Rows[c]["id"].ToString() + "。用户:"+ dtInp.Rows[c]["seller_id"].ToString());
                                toolsql += "update crm_attributesprodetect set check_state=1,tools="+i.ToString()+" where id='" + dtInp.Rows[c]["id"].ToString() + "' and useplatform = '"+ dtInp.Rows[c]["useplatform"].ToString() + "';\r\n";
                                dbtool.ExecuteNonQuery(toolsql);
                                listMqs.Add(i.ToString());
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ShowSend("主线程出错：" + ex.ToString());
                TxtLog.WriteLine("检测宝贝违规词MQS_ErrorLog", ex.ToString());
            }
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
    }
}
