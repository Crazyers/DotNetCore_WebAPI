﻿using Dapper;
using KYOMS.Core20.Application;
using KYOMS.Core20.Common.LogCommon;
using KYOMS.Core20.Common.Utility;
using KYOMS.Core20.DE.Model;
using KYOMS.Core20.Entity.Oracle;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.OracleClient;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace KYOMS.Core20.Test.Cainiao
{
    class Program
    {
        private static readonly LogHandle Log = new LogHandle();
        private static readonly HttpHandle Http = new HttpHandle(BaseInfo.PostOrderUrl);
        static void Main()
        {
            using (var con = new OracleConnection("Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=172.16.36.13)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=R_OMS_C)));Persist Security Info=True;User ID=DEV;Password=DEV;"))
            {
                List<dynamic> list = con.Query("select * from T_SYS_DICS t").AsList();
                for (int i = 0; i < list.Count; i++)
                {
                    Console.Write(list[i].TITLE + ",");//输出用户标识 
                }
            }
            Console.ReadKey();

            Console.WriteLine("批量提交订单接口开始运行");
            var sw = new Stopwatch();
            sw.Start();
            ProcessMain();
            Console.WriteLine("本次运行耗时：{0}毫秒\n此时您可以点击x关闭批量提交订单接口程序！", sw.ElapsedMilliseconds);
            sw.Stop();
            Console.WriteLine("按任意键结束...");
            Console.ReadKey();
        }

        private static async void ProcessMain()
        {
            await Task.Delay(0);
            var orderdata = "{\"chargingInfo\":{\"basicWeight\":{\"fee\":\"5\",\"weight\":\"3\"},\"stepWeight\":{\"fee\":\"2\",\"weight\":\"1\"},\"totalFee\":\"23\"},\"courierInfo\":{\"company\":\"天天快递\",\"courierNo\":\"199900\",\"courierPhone\":\"1381111111\",\"extendFields\":[{\"key\":\"PACKAGE_NO\",\"value\":\"DWD0018\"}],\"name\":\"张三\",\"station\":\"余杭网点\",\"stationPhone\":\"1381111111\"},\"cpCode\":\"SF\",\"dicCode\":\"DCP00098765\",\"expressPrintCode\":\"AeduSjkd\",\"logisticsId\":\"LBXDCP1023892594\",\"mailNo\":\"6201710180538\",\"opRequire\":{\"opDesc\":\"天猫全渠道的作业类型\",\"opType\":\"TMQQD\"},\"orderBizType\":\"1\",\"orderCreateTime\":\"2014-07-28 10:05:34\",\"orderExtendFields\":[{\"key\":\"A001\",\"value\":\"5000\"}],\"orderSource\":\"TB\",\"packageInfo\":{\"itemList\":[{\"extendFields\":\"is_precious:1;itemId:45571155013;serviceFlag:2;\",\"insuredAmount\":\"199900\",\"itemName\":\"毛衣\",\"itemNum\":\"1\"}],\"packageHeight\":\"50\",\"packageLength\":\"50\",\"packageVolume\":\"500000\",\"packageWeight\":\"200000\",\"packageWidth\":\"50\"},\"placeOrderType\":\"1\",\"receiver\":{\"address\":\"四川省成都市高新区新航路283号\",\"city\":\"成都\",\"county\":\"高新\",\"mobile\":\"15956874256\",\"name\":\"李某\",\"phone\":\"024-84332654\",\"province\":\"四川\",\"receiverDivisionId\":\"67991\",\"receiverTown\":\"文一路\",\"userId\":\"123456\",\"zipCode\":\"634569\"},\"remark\":\"\",\"routeInfo\":{\"after\":{\"address\":\"杭州市文一西路999号\",\"city\":\"杭州\",\"contactNumber\":\"13812345678\",\"contacts\":\"张三\",\"county\":\"高兴区\",\"cpCode\":\"CP001\",\"prov\":\"浙江\",\"town\":\"文一西路\"},\"before\":{\"address\":\"669号\",\"city\":\"杭州\",\"contactNumber\":\"138111111989\",\"contacts\":\"张三\",\"county\":\"高兴区\",\"cpCode\":\"T002\",\"prov\":\"浙江\",\"town\":\"文一西路\"},\"carrierCode\":\"T123568\",\"extendFields\":[{\"key\":\"SF_PRODUCT_CODE\",\"value\":\"1\"}],\"shortAddress\":\"浙－杭\",\"siteCode\":\"HX1\",\"siteName\":\"杭萧1站\"},\"sender\":{\"Town\":\"街道\",\"address\":\"浙江省杭州市余杭区文一西路823号\",\"city\":\"杭州\",\"county\":\"余杭\",\"customerId\":\"12792\",\"customerNo\":\"JD007689\",\"mobile\":\"13989203824\",\"name\":\"刘某\",\"phone\":\"0571-84292224\",\"province\":\"浙江\",\"senderAddressId\":\"7638290\",\"senderDivisionId\":\"67991\",\"senderTown\":\"街道\",\"userId\":\"123456\",\"zipCode\":\"638293\"},\"serviceDetail\":{\"codBuyServiceFee\":\"300\",\"codSplitFee\":\"100\",\"codTotalServiceFee\":\"500\",\"customerOrderTime\":\"2014-07-30 14:00:00\",\"endSignTime\":\"2015-09-11 17:00:00\",\"expressScheduleType\":\"101\",\"extendFields\":[{\"key\":\"A001\",\"value\":\"5000\"}],\"goodsValue\":\"10000\",\"gotEndTime\":\"204-11-30 19:00\",\"gotInTime\":\"30\",\"gotStartTime\":\"204-11-30 18:00\",\"grabOrderTime\":\"2014-07-30 14:00:00\",\"prevMailNo\":\"\",\"promiseSignTime\":\"2015-09-11 16:30:00\",\"scheduleDesc\":\"送达时间在2014-07-30 10:00:00到2014-07-30 14:00:00之间\",\"scheduleType\":\"104\",\"sendEndTime\":\"2014-07-30 13:05:34\",\"sendStartTime\":\"2014-07-30 12:05:34\",\"temperatureRequirement\":\"0-18\",\"totalFee\":\"600\",\"totalInsuredAmount\":\"23930\"},\"serviceFlag\":\"34,37\",\"tradeNo\":\"22227880099\"}";
            var order = JsonConvert.DeserializeObject<TmsOrderModel>(orderdata);
            var i = 0;
            var mailno = $"6{DateTime.Now:yyyyMMdd}";
            var outordercode = "LBXDCP";

            //批量生成运单号以及外部订单号
            var dataList = new ConcurrentDictionary<string, string>();
            for (var x = 1; x <= 1; x++)
            {
                dataList.TryAdd($"{outordercode}{(1023892816 + x)}", $"{mailno}{x.ToString().PadLeft(4, '0')}");
            }

            foreach (var p in dataList)
            {
                try
                {
                    Interlocked.Increment(ref i); //计算序号

                    order.logisticsId = p.Key;
                    order.mailNo = p.Value;
                    var logisticsInterface = JsonConvert.SerializeObject(order);
                    var dataDigest = SignHelper.CreateDataDigest(logisticsInterface, BaseInfo.SecretKey);
                    logisticsInterface = HttpUtility.UrlEncode(logisticsInterface);
                    dataDigest = HttpUtility.UrlEncode(dataDigest);
                    var requestModel =
                        new Dictionary<string, string>
                        {
                            ["logistics_interface"] = logisticsInterface,
                            ["data_digest"] = dataDigest,
                            ["msg_type"] = "TMS_CREATE_ORDER_ONLINE_NOTIFY",
                            ["msg_id"] = "DCP00000000000002354032",
                            ["ecCompanyId"] = "TAOBAO"
                        };

                    #region MyRegion

                    var sw = new Stopwatch();
                    sw.Start();
                    var result = Http.PostAsync(requestModel);
                    Log.Set($"第{i}次数据提交总计用时：{sw.ElapsedMilliseconds}");
                    sw.Stop();

                    #endregion



                    var model = JsonConvert.DeserializeObject<JObject>(result);
                    if (model["success"].ToString().ToLower() == "true")
                        Log.Set($"第{i}订单提交成功,运单号：{order.mailNo}，外部订单号：{order.logisticsId}", LogHandle.LogerType.Info);
                    else
                        Log.Set($"第{i}订单提交失败：{model["errorMsg"]}，运单号：{order.mailNo}，外部订单号：{order.logisticsId}", LogHandle.LogerType.Error);
                }
                catch (AggregateException ae)
                {
                    ae.Flatten();
                    //处理并行代码中的异常
                    foreach (var ex in ae.InnerExceptions)
                    {
                        //异常处理代码……
                        var message = new StringBuilder();
                        message.Append("多线程请求新增订单接口时发生错误:\n" + "参数：" + p);
                        message.Append("订单内容：" + JsonConvert.SerializeObject(p));
                        message.Append(ex.Message + ex.StackTrace);
                        Log.Set(message.ToString(), LogHandle.LogerType.Error);
                    }
                }
            }

            /*
            Parallel.ForEach(dataList, new ParallelOptions {MaxDegreeOfParallelism = 1}, (p, state) =>
            {
                try
                {
                    Interlocked.Increment(ref i); //计算序号

                    var sw = new Stopwatch();
                    sw.Start();
                    
                    #region MyRegion

                    order.logisticsId = p.Key;
                    order.mailNo = p.Value;
                    var logisticsInterface = JsonConvert.SerializeObject(order);
                    var dataDigest = SignHelper.CreateDataDigest(logisticsInterface, BaseInfo.SecretKey);
                    logisticsInterface = HttpUtility.UrlEncode(logisticsInterface);
                    dataDigest = HttpUtility.UrlEncode(dataDigest);
                    var requestModel =
                        new Dictionary<string, string>
                        {
                            ["logistics_interface"] = logisticsInterface,
                            ["data_digest"] = dataDigest,
                            ["msg_type"] = "TMS_CREATE_ORDER_ONLINE_NOTIFY",
                            ["msg_id"] = "DCP00000000000002354032",
                            ["ecCompanyId"] = "TAOBAO"
                        };

                    var result = Http.PostAsync(requestModel);
                    #endregion



                    Log.Set($"本次数据提交总计用时：{sw.ElapsedMilliseconds}");
                    sw.Stop();

                    var model = JsonConvert.DeserializeObject<JObject>(result);
                    if (model["success"].ToString().ToLower() == "true")
                        Log.Set($"第{i}订单提交成功,运单号：{order.mailNo}，外部订单号：{order.logisticsId}", LogHandle.LogerType.Info);
                    else
                        Log.Set($"第{i}订单提交失败：{model["errorMsg"]}，运单号：{order.mailNo}，外部订单号：{order.logisticsId}", LogHandle.LogerType.Error);
                }
                catch (AggregateException ae)
                {
                    ae.Flatten();
                    //处理并行代码中的异常
                    foreach (var ex in ae.InnerExceptions)
                    {
                        //异常处理代码……
                        var message = new StringBuilder();
                        message.Append("多线程请求新增订单接口时发生错误:\n" + "参数：" + p);
                        message.Append("订单内容：" + JsonConvert.SerializeObject(p));
                        message.Append(ex.Message + ex.StackTrace);
                        Log.Set(message.ToString(), LogHandle.LogerType.Error);
                    }
                }
            });
            */
            Http.Dispose();
        }

        private static string PostRequest(TmsOrderModel order, KeyValuePair<string, string> item)
        {
            order.logisticsId = item.Key;
            order.mailNo = item.Value;
            var logisticsInterface = JsonConvert.SerializeObject(order);
            var dataDigest = SignHelper.CreateDataDigest(logisticsInterface, BaseInfo.SecretKey);
            logisticsInterface = HttpUtility.UrlEncode(logisticsInterface);
            dataDigest = HttpUtility.UrlEncode(dataDigest);
            var requestModel =
                new Dictionary<string, string>
                {
                    ["logistics_interface"] = logisticsInterface,
                    ["data_digest"] = dataDigest,
                    ["msg_type"] = "TMS_CREATE_ORDER_ONLINE_NOTIFY",
                    ["msg_id"] = "DCP00000000000002354032",
                    ["ecCompanyId"] = "TAOBAO"
                };

            var sw = new Stopwatch();
            sw.Start();

            var result = Http.PostAsync(requestModel);

            Log.Set($"本次数据提交总计用时：{sw.ElapsedMilliseconds}");
            sw.Stop();
            var model = JsonConvert.DeserializeObject<JObject>(result);
            if (model["success"].ToString().ToLower() == "true")
                Log.Set($"订单提交成功,运单号：{order.mailNo}，外部订单号：{order.logisticsId}", LogHandle.LogerType.Info);
            else
                Log.Set($"订单提交失败：{model["errorMsg"]}，运单号：{order.mailNo}，外部订单号：{order.logisticsId}", LogHandle.LogerType.Error);

            return result;
        }
    }
}
