﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace CrawlerConsole.BLL
{
    class CrawlerBusinessComponent
    {
        #region 定义一些参数
        string port = string.Empty;
        string lineTitle = string.Empty;
        string days = string.Empty;
        string scenic = string.Empty;
        string hotels = string.Empty;
        string supplier = string.Empty;
        string pmRecommendation = string.Empty;
        //string connectionString = ConfigurationManager.ConnectionStrings["DBW"].ConnectionString;
        string connectionString = "server=192.168.0.210; database=CRAWLER; uid=sa; pwd=123qwe!@#";
        int sum = 0;
        int travelNumber = 0;
        int commentNumber = 0;
        string url = string.Empty;
        string urlPrice = string.Empty;
        string datePrice = string.Empty;
        int adultPrice = 0;
        int childPrice = 0;
        #endregion


        public void DownLineInfo()
        {
            string City = "苏州";
            //try
            //{
            //Configuration config = System.Configuration.ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            List<ArriveCityEntity> list = GetArriveCity();
            if (list.Count > 0)
            {
                string cityShorthand = GetCityShorthandByCityName(City);
                string cityCode = GetCityCodeByCityName(City);
                foreach (ArriveCityEntity t1 in list)
                {
                    //if (!string.IsNullOrWhiteSpace(txtSetOutCityId.Text) && !string.IsNullOrWhiteSpace(txtArriveAtCityId.Text) && (txtSetOutCityId.Text != txtArriveAtCityId.Text))
                    //if (!string.IsNullOrWhiteSpace(txtSetOutCityId.Text) && (txtSetOutCityId.Text != list[x].CityName))
                    if (!string.IsNullOrWhiteSpace(City) && !string.IsNullOrWhiteSpace(cityShorthand))
                    {
                        var webclient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
                        //定义以UTF-8格式接收文本信息，规避WebClient接收文本信息时夹带乱码问题


                        for (int j = 1; j <= 35; j++)
                        {
                            url =
                                // string.Format("http://www.tuniu.com/package/api/flight?productId=210177243&departDate=2017-09-19&departCityCode=2500&backCityCode=2500&bookCityCode=2500&adultNum=2&childNum=0&freeChildNum=0");
                                $"http://s.tuniu.com/search_complex/pkg-{cityShorthand}-0-{t1.CityName}/{j}/";
                            //string.Format("http://www.tuniu.com//package/api/calendar?productId=210177038&bookCityCode=1615&departCityCode=2500&backCityCode=2500");
                            var html = webclient.DownloadString(url);

                            #region 先判断是否是自助游线路

                            Regex reg = new Regex(@"<div class=""crumbs"">(?<key>[\s\S]*?)</div>");
                            var str = string.Empty;
                            var matchs = reg.Matches(html);
                            foreach (Match match in matchs)
                            {
                                str += match.Groups["key"].Value;
                            }
                            if (!str.Contains("自助游"))//非自助游线路 直接跳出循环
                            {
                                break;
                            }
                            #endregion

                            #region 循环抓取所有线路的详细信息
                            //循环获取网页中的线路编号
                            reg = new Regex(@"productId=""(?<key>[^""]*)""");
                            str = string.Empty;
                            matchs = reg.Matches(html);
                            foreach (Match match in matchs)
                            {
                                str += match.Groups["key"].Value + ",";
                            }
                            if (string.IsNullOrEmpty(str))//没有获取到线路编号 直接跳出循环
                            {
                                break;
                            }


                            string productId = DeleteDuplicates(str);//当前城市所有的线路id列表

                            string[] arr = productId.Split(',');//转成数组

                            List<string> productIdList = new List<string>(arr);

                            foreach (string t in productIdList)
                            {
                                url = $"http://www.tuniu.com/package/{t}";
                                var response = webclient.DownloadString(url);

                                #region 出发港口
                                reg = new Regex(@"<div class=""resource-city-more-selected"">(?<key>[^<]*)</div>");
                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    port = match.Groups["key"].Value;//出发港口
                                }
                                #endregion

                                #region 线路标题
                                reg = new Regex(@"<title>(?<key>[\s\S]*?)</title>");
                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    lineTitle = match.Groups["key"].Value;//线路标题
                                }
                                #endregion

                                #region 天数
                                int num = lineTitle.LastIndexOf("日");
                                if (num > 1)
                                {
                                    days = lineTitle.Substring(num - 1, 2);
                                }

                                #endregion

                                #region 景点
                                reg = new Regex(@"<li class=""detail-journey-4-nav-sub-item""[^>]*><span>(?<key>[\s\S]*?)</span>");

                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    scenic += match.Groups["key"].Value + ",";//景点
                                }

                                if (string.IsNullOrEmpty(scenic))
                                {
                                    reg = new Regex(@"<li class=""detail-journey-4-nav-group""[^>]*><span>(?<key>[\s\S]*?)</span>");
                                    matchs = reg.Matches(response);
                                    foreach (Match match in matchs)
                                    {
                                        scenic += match.Groups["key"].Value + ",";//景点
                                    }
                                }
                                #endregion

                                #region 住宿标准
                                reg = new Regex(@"<span class=""detail-journey-star"">(?<key>[\s\S]*?)</span>");
                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    hotels = match.Groups["key"].Value;//住宿标准
                                }
                                #endregion

                                #region 供应商
                                reg = new Regex(@"<span class=""reource-vendor"">(?<key>[\s\S]*?)</span>");
                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    supplier = match.Groups["key"].Value;//供应商
                                }
                                #endregion

                                #region 产品经理推荐
                                reg = new Regex(@"<div class=""resource-recommend-content-inner"">(?<key>[\s\S]*?)</div>");
                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    pmRecommendation = match.Groups["key"].Value;//产品经理推荐
                                    pmRecommendation = pmRecommendation.Replace("'", "");
                                }
                                #endregion

                                #region 出游人数
                                reg = new Regex(@"<a class=""resource-people-number"" href=""javascript:;"">(?<key>[\s\S]*?)</a>");
                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    travelNumber = Convert.ToInt32(match.Groups["key"].Value);//出游人数

                                }
                                #endregion

                                #region 点评人数
                                reg = new Regex(@"<a class=""resource-people-number"" href=""#comment"" mm=""点击_头部区_基本信息_查看点评"">(?<key>[\s\S]*?)</a>");
                                matchs = reg.Matches(response);
                                foreach (Match match in matchs)
                                {
                                    commentNumber = Convert.ToInt32(match.Groups["key"].Value);//点评人数

                                }
                                #endregion




                                sum += AddLineInfo(City, t1.CityName, port, lineTitle, days, scenic, hotels, supplier, pmRecommendation, url, travelNumber, commentNumber, t, cityCode);
                            }
                            #endregion

                            if (productIdList.Count < 3 && j > 1)//说明当前页面已是尾页
                            {
                                break;
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("当前数据中无此出发城市！");
                    }
                }
                Console.WriteLine("本批次执行成功，共抓取线路" + sum + "条！");

            }
            //}
            //catch (Exception ex)
            //{
            //    Response.Write("<Script Language=JavaScript>alert('线路url：" + url + "！,出错原因：" + ex + "');</Script>");
            //}
        }

        #region 去除重复的线路id
        /// <summary>
        /// 去除重复的线路id
        /// </summary>
        /// <param name="str">线路id字符串</param>
        /// <returns></returns>
        public static string DeleteDuplicates(string str)
        {
            string[] arr = str.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            Hashtable ht = new Hashtable();
            foreach (string s in arr)
            {
                if (!ht.ContainsKey(s))
                {
                    sb.AppendFormat("{0},", s);
                    ht.Add(s, string.Empty);
                }
            }
            return sb.ToString().TrimEnd(',');
        }

        #endregion

        #region 根据出发城市中文名获取简写
        public string GetCityShorthandByCityName(string cityName)
        {
            string str = string.Empty;

            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();

                string sql = " select Shorthand from T_Tuniu_DepartureCity where CityName ='" + cityName + "'";
                SqlCommand com = new SqlCommand(sql, con);

                str = com.ExecuteScalar().ToString();
                con.Close();

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return str;
        }
        #endregion

        #region 根据出发城市中文名获取CityCode
        public string GetCityCodeByCityName(string cityName)
        {
            string str = string.Empty;

            try
            {
                SqlConnection con = new SqlConnection(connectionString);
                con.Open();

                string sql = " select CityCode from T_Tuniu_DepartureCity where CityName ='" + cityName + "'";
                SqlCommand com = new SqlCommand(sql, con);

                str = com.ExecuteScalar().ToString();
                con.Close();

            }
            catch (Exception ex)
            {

                throw ex;
            }
            return str;
        }
        #endregion

        #region 写入数据
        public int AddLineInfo(string DepartCity, string ArriveCity, string Port, string LineTitle, string Days, string Scenic, string Hotels, string Supplier, string PMRecommendation, string url, int travelNumber, int commentNumber, string lineNo, string cityCode)
        {
            int num = 0;
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            Guid guid = Guid.NewGuid();
            string guids = guid.ToString().Replace("-", "");
            DateTime date = DateTime.Now;

            #region 必要验证
            if (LineTitle.Length >= 500)
            {
                LineTitle = LineTitle.Substring(0, 499);
            }
            if (Scenic.Length >= 4000)
            {
                Scenic = Scenic.Substring(0, 3999);
            }
            if (Hotels.Length >= 500)
            {
                Hotels = Hotels.Substring(0, 499);
            }
            if (Supplier.Length >= 500)
            {
                Supplier = Supplier.Substring(0, 499);
            }
            if (PMRecommendation.Length >= 4000)
            {
                PMRecommendation = PMRecommendation.Substring(0, 3999);
            }
            #endregion

            string sql = "insert into [dbo].[Line] values('" + guids + "','途牛','自助游','" + DepartCity + "','" + ArriveCity + "','" + Port + "','" + LineTitle + "','" + Days + "','" + Scenic + "','','" + Hotels + "','" + Supplier + "','','','" + travelNumber + "','" + url + "','','" + date + "','" + commentNumber + "','" + PMRecommendation + "')";
            SqlCommand com = new SqlCommand(sql, con);

            num = com.ExecuteNonQuery();

            if (num > 0)
            {
                #region 抓取价格日历json

                var webclient = new WebClient { Encoding = System.Text.Encoding.UTF8 };
                //定义以UTF-8格式接收文本信息，规避WebClient接收文本信息时夹带乱码问题
                urlPrice = $"http://www.tuniu.com//package/api/calendar?productId={lineNo}&bookCityCode={cityCode}";
                var htmlPrice = webclient.DownloadString(urlPrice);
                // var dataPrice = JsonConvert.DeserializeObject(htmlPrice);

                ParseJson jobInfoList = JsonConvert.DeserializeObject<ParseJson>(htmlPrice);

                if (jobInfoList != null && jobInfoList.Success == true && jobInfoList.Data != null && jobInfoList.Data.CalendarInfo != null)
                {
                    foreach (Calendarinfo t in jobInfoList.Data.CalendarInfo)
                    {
                        adultPrice = t.AdultPrice;
                        childPrice = t.ChildPrice;
                        datePrice = t.PlanDate;
                        if (!string.IsNullOrEmpty(datePrice))
                        {
                            Guid guidPrice = Guid.NewGuid();
                            string guidsPrice = guidPrice.ToString().Replace("-", "");
                            string sqlPrice = "insert into [dbo].[GroupPrice] values('" + guidsPrice + "','" + guids + "','" + datePrice + "','" + adultPrice + "','" + childPrice + "')";
                            SqlCommand comPrice = new SqlCommand(sqlPrice, con);

                            num = comPrice.ExecuteNonQuery();
                        }
                    }
                }

                #endregion

            }
            con.Close();

            return num;
        }
        #endregion

        #region 获取到达城市
        public List<ArriveCityEntity> GetArriveCity()
        {
            List<ArriveCityEntity> list = new List<ArriveCityEntity>();

            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            string sql = "select [CityName] FROM [CRAWLER].[dbo].[T_Tuniu_ArriveCity] order by [CityName] desc";
            SqlCommand com = new SqlCommand(sql, con);
            SqlDataReader sdr = com.ExecuteReader();
            while (sdr.Read())
            {
                ArriveCityEntity arriveCity = new ArriveCityEntity { CityName = sdr["CityName"].ToString() };


                list.Add(arriveCity);
            }
            con.Close();

            return list;
        }
        #endregion


        #region 写入价格
        public int AddPrice(string lineId, string date, string adultPrice, string childPrice)
        {
            int num = 0;
            SqlConnection con = new SqlConnection(connectionString);
            con.Open();
            Guid guid = Guid.NewGuid();
            string guids = guid.ToString().Replace("-", "");
            string sql = "insert into [dbo].[GroupPrice] values('" + guids + "','" + lineId + "','" + date + "','" + adultPrice + "','" + childPrice + "')";
            SqlCommand com = new SqlCommand(sql, con);

            num = com.ExecuteNonQuery();
            con.Close();
            return num;
        }
        #endregion

    }
}