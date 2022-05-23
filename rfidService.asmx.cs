using System;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Web.Services;

namespace rfidWebservice
{
    /// <summary>
    /// rfidService 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
    // [System.Web.Script.Services.ScriptService]
    public class rfidService : WebService
    {
        Business bus = new Business();
        int waitTime = 10;
        #region 类内公共方法
        private MessagePack HttpPost(string serviceId, string sourceAppCode, string msgBody, string sendtime)
        {
            MessagePack _mp = new MessagePack();

            Encoding encode = Encoding.GetEncoding("utf-8");
            byte[] arrB = encode.GetBytes(msgBody);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(SysParam.bxUrl);
            myReq.Method = "POST";
            myReq.ContentLength = arrB.Length;
            SetHeaderValue(myReq.Headers, "Connection", "keep-alive");
            SetHeaderValue(myReq.Headers, "ContentType", "application/json");
            SetHeaderValue(myReq.Headers, "sourceAppCode", sourceAppCode);
            SetHeaderValue(myReq.Headers, "serviceId", serviceId);
            SetHeaderValue(myReq.Headers, "msgSendTime", sendtime);

            try
            {
                Stream outStream = myReq.GetRequestStream();
                outStream.Write(arrB, 0, arrB.Length);
                outStream.Close();

                WebResponse myResp = myReq.GetResponse();

                if (myResp.Headers["returnCode"].ToString().Equals("0"))
                {
                    pub.WriteLog("调试--HttpPost", "提交完成，返回0");

                    _mp.Code = 0;
                    _mp.Result = true;
                }
                else
                {
                    Stream ReceiveStream = myResp.GetResponseStream();
                    StreamReader readStream = new StreamReader(ReceiveStream, encode);

                    long contentLength = myResp.ContentLength;
                    char[] read = new char[contentLength];

                    int count = readStream.Read(read, 0, (int)contentLength);
                    string str = null;


                    while (count > 0)
                    {
                        str += new string(read, 0, count);
                        count = readStream.Read(read, 0, (int)contentLength);
                    }
                    _mp.Code = Convert.ToInt32(myResp.Headers["returnCode"]);
                    readStream.Close();

                    _mp.Message = str;
                    _mp.Result = true;
                }

                
                myResp.Close();


            }
            catch (Exception e)
            {
                _mp.Message = e.Message;
                _mp.Result = false;
            }
            return _mp;
        }
        private void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (property != null)
            {
                var collection = property.GetValue(header, null) as NameValueCollection;
                collection[name] = value;
            }
        }
        #endregion

        [WebMethod]
        public MessagePack sendMsg(string _serviceid, string _BxUserID, string _BxUserName, string _BxJobID, DataSet _inDs, out DataSet _outDs)
        {
            service _svr = new service();
            data.DAO.sendmsg _sendmsg = new data.DAO.sendmsg();
            MessagePack _mp;
            _outDs = null;
            string _key = Guid.NewGuid().ToString("N").ToUpper();

            DataTable _dt = _inDs.Tables[0];
            _dt.Columns.Add("userId", typeof(string));
            _dt.Columns["userId"].Caption = "用户工号";
            _dt.Columns.Add("userName", typeof(string));
            _dt.Columns["userName"].Caption = "用户姓名";
            _dt.Columns.Add("jobId", typeof(string));
            _dt.Columns["jobId"].Caption = "用户岗位编码";

            foreach(DataRow _dr in _dt.Rows)
            {
                _dr["userId"] = _BxUserID;
                _dr["userName"] = _BxUserName;
                _dr["jobId"] = _BxJobID;
            }

            string _json;

            _mp = _svr.sendMsgToJson(_serviceid, _key,  _inDs, out _json);

            if (!_mp.Result)
            {
                return _mp;
            }

            _sendmsg.msg = _json;
            _sendmsg.sendtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            _sendmsg.serviceid = _serviceid;
            _sendmsg.sourceappcode = "DV";
            _sendmsg.BxUserID = _BxUserID;

            _mp = _sendmsg.save();

            if (!_mp.Result)//保存到数据库失败
            {
                return _mp;
            }

            _mp = HttpPost(_sendmsg.serviceid, _sendmsg.sourceappcode, _sendmsg.msg, _sendmsg.sendtime);

            if (!_mp.Result)//发送消息失败
            {
                return _mp;
            }

            if (_mp.Code == 0)
            {
                pub.WriteLog("调试--sendMsg", "提交成功，等待返回报文："+ _key);
                //提交成功

                for (int i = 0; i < waitTime; i++)
                {
                    DataSet _recDs = (DataSet)CacheHelper.Get(_key);

                    if (_recDs != null)
                    {
                        _outDs = _recDs;
                        _mp.Message = "成功接收到ERP返回的消息";
                        _mp.Result = true;
                        _mp.Code = 0;
                        CacheHelper.RemoveAllCache(_key);

                        string _waitServiceId = _recDs.Tables[0].TableName.Substring(0,6);

                        switch (_waitServiceId)
                        {
                            case "E1DV01":
                                _mp = _svr.recE1DV01(_recDs);
                                break;
                            case "E1DV02":
                                _mp = _svr.recE1DV02(_inDs, _recDs);
                                break;
                            case "E1DV03":
                                _mp = _svr.recE1DV03(_recDs);
                                break;
                            case "E1DV04":
                                _mp = _svr.recE1DV04(_inDs, _recDs);
                                break;
                            case "E1DV05":
                                _mp = _svr.recE1DV05(_recDs);
                                break;
                            case "E1DV06":
                                _mp = _svr.recE1DV06(_inDs, _recDs);
                                break;
                            case "E1DV07":
                                _mp = _svr.recE1DV07(_recDs);
                                break;
                            case "E1DV08":
                                _mp = _svr.recE1DV08(_inDs, _recDs);
                                break;
                            case "E1DV09":
                                _mp = _svr.recE1DV09(_inDs, _recDs);
                                break;
                            case "E1DV10":
                                _mp = _svr.recE1DV10(_inDs, _recDs);
                                break;
                            case "E1DV11":
                                _mp = _svr.recE1DV12(_recDs);
                                break;
                            case "E1DV17":
                                _mp = _svr.recE1DV17( _recDs);
                                break;
                            case "E1DV18":
                                _mp = _svr.recE1DV18( _recDs);
                                break;
                            case "E1DV19":
                                _mp = _svr.recE1DV19(_inDs, _recDs);
                                break;
                            case "E1DV20":
                                _mp = _svr.recE1DV20(_recDs);
                                break;
                            case "E1DV21":
                                _mp = _svr.recE1DV21(_inDs, _recDs);
                                break;
                            case "E1DV22":
                                _mp = _svr.recE1DV22(_recDs);
                                break;
                            case "E1DV23":
                                _mp = _svr.recE1DV23(_inDs, _recDs);
                                break;
                            case "E1DV24":
                                _mp = _svr.recE1DV24(_recDs);
                                break;
                            case "E1DV25":
                                _mp = _svr.recE1DV25(_inDs, _recDs);
                                break;
                            case "E1DV26":
                                _mp = _svr.recE1DV26(_recDs);
                                break;
                            case "E1DV27":
                                _mp = _svr.recE1DV27(_inDs, _recDs);
                                break;
                            case "E1DV30":
                                _mp = _svr.recE1DV30(_recDs);
                                break;
                            case "E1DV32":
                                _mp = _svr.recE1DV32(_inDs, _recDs);
                                break;
                            case "E1DV33":
                                _mp = _svr.recE1DV33(_inDs, _recDs);
                                break;
                            case "E1DV35":
                                _mp = _svr.recE1DV35(_inDs, _recDs);
                                break;
                            case "E1DV36":
                                _mp = _svr.recE1DV36(_inDs, _recDs);
                                break;
                            case "E1DV31":
                                _mp = _svr.recE1DV31(_recDs);
                                break;
                            case "E1DV37":
                                _mp = _svr.recE1DV37( _recDs);
                                break;
                            case "E1DV38":
                                _mp = _svr.recE1DV38(_recDs);
                                break;
                            case "E1DV39":
                                _mp = _svr.recE1DV39(_recDs);
                                break;
                            case "E1DV40":
                                _mp = _svr.recE1DV40(_recDs);
                                break;
                            case "E1DV41":
                                _mp = _svr.recE1DV41(_recDs);
                                break;
                            case "E1DV29":
                                _mp = _svr.recE1DV29(_recDs);
                                break;
                            case "E1DV42":
                                _mp = _svr.recE1DV42(_recDs);
                                break;
                            case "E1DV43":
                                _mp = _svr.recE1DV43(_recDs);
                                break;
                            case "E1DV44":
                                _mp = _svr.recE1DV44(_recDs);
                                break;
                            case "E1DV45":
                                _mp = _svr.recE1DV45(_recDs);
                                break;
                            case "E1DV46":
                                _mp = _svr.recE1DV46(_recDs);
                                break;
                            case "E1DV47":
                                _mp = _svr.recE1DV47(_recDs);
                                break;
                            case "E1DV48":
                                _mp = _svr.recE1DV48(_recDs);
                                break;
                            case "E1DV49":
                                _mp = _svr.recE1DV49(_recDs);
                                break;
                            case "E1DV50":
                                _mp = _svr.recE1DV50(_recDs);
                                break;
                            case "E1DV51":
                                _mp = _svr.recE1DV51(_recDs);
                                break;
                            case "E1DV52":
                                _mp = _svr.recE1DV52(_recDs);
                                break;
                            case "E1DV53":
                                _mp = _svr.recE1DV53(_recDs);
                                break;
                            case "E1DV54":
                                _mp = _svr.recE1DV54(_recDs);
                                break;
                            case "E1DV56":
                                _mp = _svr.recE1DV56(_recDs);
                                break;
                            case "E1DV57":
                                _mp = _svr.recE1DV57(_recDs);
                                break;
                            case "E1DV58":
                                _mp = _svr.recE1DV58(_recDs);
                                break;
                            case "E1DV59":
                                _mp = _svr.recE1DV59(_recDs);
                                break;
                            case "E1DV60":
                                _mp = _svr.recE1DV60(_recDs);
                                break;
                            case "E1DV61":
                                _mp = _svr.recE1DV61(_recDs);
                                break;
                            case "E1DV62":
                                _mp = _svr.recE1DV62(_recDs);
                                break;
                            case "E1DV63":
                                _mp = _svr.recE1DV63(_recDs);
                                break;
                            case "E1DV64":
                                _mp = _svr.recE1DV64(_recDs);
                                break;
                            case "E1DV65":
                                _mp = _svr.recE1DV65(_recDs);
                                break;
                            case "E1DV66":
                                _mp = _svr.recE1DV66(_recDs);
                                break;
                            case "E1DV67":
                                _mp = _svr.recE1DV67(_recDs);
                                break;
                            case "E1DV68":
                                _mp = _svr.recE1DV68(_recDs);
                                break;
                            default:
                                break;
                        }

                        return _mp;
                    }

                    System.Threading.Thread.Sleep(1000);
                }

                _mp.Message = "等待宝信系统返回消息超时";
                _mp.Result = false;
                _mp.Content = null;
                _mp.Code = -1;
            }

            return _mp;
        }

        [WebMethod]
        public MessagePack sendMsgNotOut(string _serviceid, string _BxUserID, string _BxUserName, string _BxJobID, DataSet _inDs)
        {
            return sendMsg(_serviceid, _BxUserID, _BxUserName, _BxJobID, _inDs, out DataSet _outDs);
        }

        /// <summary>
        /// 系统登录
        /// </summary>
        /// <param name="_username">用户名</param>
        /// <param name="_password">密码</param>
        /// <returns>处理结果</returns>
        [WebMethod]
        public MessagePack Login(string _username, string _password, out string BxUserID, out string BxUserName, out string BxJobId, out privilidge PrivateStr, out string DbConnStr)
        {
            MessagePack pack = new MessagePack();
            PrivateStr = new privilidge();
            DbConnStr = "";
            string ErrMsg;

            if (0 != bus.Login(_username, _password, out BxUserID,out BxUserName, out BxJobId, out PrivateStr, out ErrMsg))
            {
                pack.Code = -1;
                pack.Message = ErrMsg;
                pack.Result = false;
                return pack;
            }

            //DbConnStr = SysParam.mssql;
            DbConnStr = SysParam.oledbMssql;
            pack.Code = 0;
            pack.Result = true;
            pack.Message = "登录成功";
            pub.WriteLog("调试", "登录成功");

            return pack;
        }

        [WebMethod]
        public MessagePack getInvPhysic(out DataSet _outDs)
        {
            MessagePack pack = new MessagePack();

            if (!bus.getInv(out _outDs))
            {
                pack.Result = false;
                pack.Message = "读取物理库区错误";
            }
            else
            {
                pack.Result = true;
            }

            return pack;
        }

        [WebMethod]
        public MessagePack getBxUsers(out DataSet _outDs)
        {
            MessagePack pack = new MessagePack();

            if (!bus.getBxUsers(out _outDs))
            {
                pack.Result = false;
                pack.Message = "读取宝信用户信息错误";
            }
            else
            {
                pack.Result = true;
            }

            return pack;
        }

        [WebMethod]
        public DateTime currDate()
        {
            return DateTime.Now;
        }

        [WebMethod]
        public MessagePack getPrint(out DataSet _outDs)
        {
            MessagePack pack = new MessagePack();

            if (!bus.getPrint(out _outDs))
            {
                pack.Result = false;
                pack.Message = "读取打印机信息错误";
            }
            else
            {
                pack.Result = true;
            }

            return pack;
        }

        [WebMethod]
        public MessagePack getWarehouse(out DataSet _outDs)
        {
            MessagePack pack = new MessagePack();

            if (!bus.getWarehouse(out _outDs))
            {
                pack.Result = false;
                pack.Message = "读取目标库区信息错误";
            }
            else
            {
                pack.Result = true;
            }

            return pack;
        }

        [WebMethod]
        public MessagePack getShiperReceiver(out DataSet _outDs)
        {
            MessagePack pack = new MessagePack();

            if (!bus.getShiperReceiver(out _outDs))
            {
                pack.Result = false;
                pack.Message = "读取单位信息错误";
            }
            else
            {
                pack.Result = true;
            }

            return pack;
        }
    }
}
