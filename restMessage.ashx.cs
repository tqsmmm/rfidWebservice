using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace rfidWebservice
{
    /// <summary>
    /// restMessage 的摘要说明
    /// </summary>
    public class restMessage : IHttpHandler
    {
        Encoding encode = Encoding.GetEncoding("utf-8");
        service _svc = new service();
        MessagePack _mp;
        System.Data.DataSet _ds;

        public void ProcessRequest(HttpContext context)
        {
            string serviceId = context.Request.Headers["serviceId"];
            string sourceAppCode = context.Request.Headers["sourceAppCode"];
            string msgSendTime = context.Request.Headers["msgSendTime"];

            Stream ReceiveStream = context.Request.InputStream;
            StreamReader readStream = new StreamReader(ReceiveStream, encode);
            int contextLength = context.Request.ContentLength;
            char[] read = new char[contextLength];
            int count = readStream.Read(read, 0, contextLength);
            string str = null;

            data.DAO.receivemsg _recMsg;

            while (count > 0)
            {
                str += new string(read, 0, count);
                count = readStream.Read(read, 0, contextLength);
            }

            if (str == null)
            {
                str = "";
            }

            string _request = JsonConvert.SerializeObject(context.Request.Headers);

            pub.WriteLog("到收消息，调试", str);

            string returnCode;
            string returnContent;

            try
            {
                data.ado _ado = new data.ado();

                string _sqlstr = "insert into test_recmsg(request, msg, rectime) values(@request,@msg,@rectime)";
                List<data.Sqlparameter> parameter = new List<data.Sqlparameter>();
                parameter.Add(new data.Sqlparameter { parameter_name = "@request", parameter_value = _request });
                parameter.Add(new data.Sqlparameter { parameter_name = "@msg", parameter_value = str });
                parameter.Add(new data.Sqlparameter { parameter_name = "@rectime", parameter_value = DateTime.Now });

                if (_ado.ExecuteNonQuery(_sqlstr, parameter) < 0)
                {
                    pub.WriteErrLog("记录报文错误。");
                }

                if (serviceId == null || serviceId.Length == 0)
                {
                    pub.WriteLog("调试", "收到消息，serviceId == null");
                    returnCode = "2001";
                    returnContent = "代理服务号为空，无需重发";
                }
                else
                {
                    if (sourceAppCode == null || sourceAppCode.Length == 0)
                    {
                        pub.WriteLog("调试", "收到消息，sourceAppCode == null");
                        returnCode = "2003";
                        returnContent = "源系统代码为空，无需重发";
                    }
                    else
                    {
                        if (msgSendTime == null || msgSendTime.Length == 0)
                        {
                            pub.WriteLog("调试", "收到消息，msgSendTime == null");
                            returnCode = "2002";
                            returnContent = "调用时间为空，无需重发";
                        }
                        else
                        {
                            pub.WriteLog("调试--ProcessRequest", "消息头正常：" + serviceId + ", " + sourceAppCode + ", " + msgSendTime);

                            _recMsg = new data.DAO.receivemsg { msg = str, serviceid = serviceId, sourceappcode = sourceAppCode, sendtime = msgSendTime };
                            _mp = _recMsg.save();

                            if (_mp.Result)
                            {
                                pub.WriteLog("restMessage.ProcessRequest()", "接收到 " + serviceId + " 报文，保存数据库成功。");
                            }
                            else
                            {
                                pub.WriteLog("restMessage.ProcessRequest()", "接收到 " + serviceId + "报文，保存数据库失败。");
                            }

                            string _key;

                            _mp = _svc.recMsgToDataSet(serviceId.ToUpper(), str, out _ds, out _key);

                            pub.WriteLog("调试，收到 " + serviceId + " 报文", "处理 " + serviceId + " 报文结果：" + _mp.Result + ";" + _mp.Message);

                            if (!_mp.Result)
                            {
                                returnCode = _mp.Code.ToString();
                                returnContent = _mp.Message;
                            }
                            else
                            {
                                string sId = serviceId.ToUpper();
                                /*
                                try
                                {
                                    _ds.WriteXml(@"d:\log\" + sId + "-" + System.DateTime.Now.ToString("yyyyMMddHHmmssfff"));
                                }
                                catch { }
                                */

                                string[] strService = new string[]{"E1DV01", "E1DV02", "E1DV03", "E1DV04", "E1DV05", "E1DV06",
                                    "E1DV07","E1DV08","E1DV09","E1DV10","E1DV11","E1DV12","E1DV17","E1DV18","E1DV19","E1DV20",
                                    "E1DV21","E1DV22","E1DV23","E1DV24","E1DV25","E1DV26","E1DV27","E1DV29","E1DV30","E1DV31",
                                    "E1DV32","E1DV33","E1DV35","E1DV36","E1DV37","E1DV38","E1DV39","E1DV40","E1DV41","E1DV42",
                                    "E1DV43","E1DV44","E1DV45","E1DV46","E1DV47","E1DV48","E1DV49","E1DV50","E1DV51","E1DV52",
                                    "E1DV53","E1DV54","E1DV56","E1DV57","E1DV58","E1DV59","E1DV60","E1DV61","E1DV62","E1DV63",
                                    "E1DV64","E1DV65","E1DV66","E1DV67","E1DV68"};

                                if (strService.Contains(sId))
                                {
                                    pub.WriteLog("调试，正确处理 " + serviceId + " 报文", "准备写入Cache");

                                    if (sId.Equals("E1DV11"))
                                    {
                                        service _svr = new service();
                                        _svr.recE1DV11(_ds);
                                    }
                                    else
                                    {
                                        CacheHelper.Set(_key, _ds, TimeSpan.FromSeconds(20));
                                    }

                                    returnCode = "0";
                                    returnContent = "成功";
                                }
                                else
                                {
                                    pub.WriteLog("调试， 接到 default", "接收到其它的消息......");
                                    returnCode = "2202";
                                    returnContent = "代理服务未注册，无需重发";
                                }

                                pub.WriteLog("调试，正确处理 " + serviceId + " 报文", "写入Cache成功");
                                //}
                            }
                        }
                    }
                }

                pub.WriteLog("调试，处理 " + serviceId + " 报文结束", "返回消息响应为: " + returnCode + "; " + returnContent);
            }
            catch (Exception e)
            {
                pub.WriteErrLog("err--ProcessRequest--try : " + e.Message);
                returnCode = "2001";
                returnContent = "处理不正确";
            }

            pub.WriteLog("调试--ProcessRequest", "准备返回消息应答");
            try
            {
                //context.Response.Headers.Add("returnCode", returnCode);
                context.Response.AddHeader("returnCode", returnCode);
                context.Response.Write(returnContent);

                pub.WriteLog("调试，返回消息", "returnCode : " + returnCode + "; returnContent: " + returnContent);
            }
            catch (Exception e)
            {
                pub.WriteErrLog("err--ProcessRequest--响应: " + e.Message);
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}