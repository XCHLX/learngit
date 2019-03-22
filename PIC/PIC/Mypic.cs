using DbAccess;
using ImageDetection;
using SearchItem;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Top.Api.Domain;
namespace PIC
{
    class Mypic
    {
        /// <summary>
        /// 检测标题和详情
        /// </summary>
        /// <param name="dt">错误表信息</param>
        /// <param name="item">宝贝对象</param>
        /// <param name="titleword">标题错误</param>
        /// <param name="desc">详情错误</param>
        public void DetectionTitelDesc(DataTable dtuser, DataTable dt,Item item,ref string titleword,ref string desc)
        {
            try
            {
                //判断系统违禁词
                foreach (DataRow dr in dt.Rows)
                {
                    //宝贝标题违规词
                    if (!string.IsNullOrEmpty(item.Title) && item.Title.ToString().Contains(dr["proword"].ToString()))
                    {
                        titleword += dr["proword"].ToString() + ",";
                    }
                    //宝贝描述违规词
                    MatchCollection mc =   MatchValues(item.Desc, ">", "<");
                    string x = "";//查出页面可视的文字;
                    foreach (Match s in mc)
                    {
                        x += s;
                    }
                    if (!string.IsNullOrEmpty(x) && x.Contains(dr["proword"].ToString()))
                    {
                        desc += dr["proword"].ToString() + ",";
                    }
                }
                //判断自定义违禁词
                foreach (DataRow dr in dtuser.Rows)
                {
                    //宝贝标题违规词
                    if (!string.IsNullOrEmpty(item.Title) && item.Title.ToString().Contains(dr["proword"].ToString()))
                    {
                        titleword += dr["proword"].ToString() + ",";
                    }
                    //宝贝描述违规词
                    if (!string.IsNullOrEmpty(item.Desc) && item.Desc.ToString().Contains(dr["proword"].ToString()))
                    {
                        desc += dr["proword"].ToString() + ",";
                    }
                }
            }
            catch (Exception ex)
            {
                TxtLog.WriteLine("图片违禁词检测", ex.ToString());
            }
        }
        /// <summary>
        /// 检测图片
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="item"></param>
        /// <param name="ItemImgsError"></param>
        /// <param name="picText"></param>
        public void DetectionPIC(DataTable dtuser, DataTable dt, Item item,ref string ItemImgsError ,ref string picText)
        {
            try
            {
                Detectionpic dpic = new Detectionpic();
                List<string> successlist = new List<string>();//记录执行过的图片
                List<string> picList = new List<string>();//图片链接集合
                string ItemImgsErrors = "";
                MatchCollection obj = this.MatchValues(item.Desc, "src=\"", "\"");
                #region 获取当前宝贝的图片
                for (int i = 0; i < item.ItemImgs.Count; i++)//判断主图违禁词
                {
                    ItemImgsErrors = "";
                    #region 获取每个图片的文字 去比较违禁词 记录错误图片地址和违禁词
                    string pictext = dpic.imageDetection(item.ItemImgs[i].Url.ToString());
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(pictext) && pictext.ToString().Contains(dr["proword"].ToString()))
                        {
                            ItemImgsErrors += dr["proword"].ToString() + ",";
                        }
                    }
                    foreach (DataRow dr in dtuser.Rows)
                    {
                        if (!string.IsNullOrEmpty(pictext) && pictext.ToString().Contains(dr["proword"].ToString()))
                        {
                            ItemImgsErrors += dr["proword"].ToString() + ",";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(ItemImgsErrors))
                        ItemImgsError += " { \"pic\":\"" + item.ItemImgs[i].Url.ToString() + "\",\"cw\":\"" + ItemImgsErrors + "\"},";
                    #endregion
                }
                #endregion
                for (int i = 0; i < obj.Count; i++)
                {
                    if (i > 30)
                    {
                        TxtLog.WriteLine("图片超额", item.NumIid.ToString());
                        return;
                    }
                    string pic = obj[i].ToString();
                    if (obj[i].ToString().Contains("gif") || successlist.Contains(pic))//过滤gif图，和重复的
                    {
                        continue;
                    }
                    successlist.Add(pic);
                    string pictext = dpic.imageDetection(pic);
                    picText = pictext;

                    TxtLog.WriteLine("图片解析", item.NumIid + "文字:" + pictext + "\r\n" + "图片" + pic);
                    ItemImgsErrors = "";
                    foreach (DataRow dr in dt.Rows)
                    {
                        if (!string.IsNullOrEmpty(pictext) && pictext.ToString().Contains(dr["proword"].ToString()))
                        {
                            ItemImgsErrors += dr["proword"].ToString() + ",";
                        }
                    }
                    foreach (DataRow dr in dtuser.Rows)
                    {
                        if (!string.IsNullOrEmpty(pictext) && pictext.ToString().Contains(dr["proword"].ToString()))
                        {
                            ItemImgsErrors += dr["proword"].ToString() + ",";
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(ItemImgsErrors))
                        ItemImgsError += " { \"pic\":\"" + obj[i].ToString() + "\",\"cw\":\"" + ItemImgsErrors + "\"},";
                }
            }
            catch (Exception ex)
            {
                TxtLog.WriteLine("图片违禁词检测", ex.ToString());
            }
            
        }
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
        public string MatchValuestring(string str, string s, string e)
        {
            Regex rg = new Regex("(?<=(" + s + "))[.\\s\\S]*?(?=(" + e + "))", RegexOptions.Multiline | RegexOptions.Singleline);
            return rg.Replace(str, "");
        }
        #endregion
    }
}
