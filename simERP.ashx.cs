using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;

namespace rfidWebservice
{
    /// <summary>
    /// 模拟ERP接收程序
    /// </summary>
    public class simERP : IHttpHandler
    {
        Encoding encode = Encoding.GetEncoding("utf-8");
        service _svc = new service();

        public void ProcessRequest(HttpContext context)
        {
            pub.WriteLog("simERP调试", "收到消息了");
            string serviceId = context.Request.Headers["serviceId"];
            string sourceAppCode = context.Request.Headers["sourceAppCode"];
            string msgSendTime = context.Request.Headers["msgSendTime"];

            if (serviceId.Length == 0 || sourceAppCode.Length == 0 || msgSendTime.Length == 0)
            {
                context.Response.Headers.Add("returnCode", "-9999");
                context.Response.Write("三个参数不完整");
            }

            string returnCode = "";
            string returnContent = "";

            Stream ReceiveStream;
            StreamReader readStream;
            int contextLength;
            char[] read;
            int count;
            string str;

            switch (serviceId.ToUpper())
            {

                case "DVE101":
                    //模拟DVE101
                    pub.WriteLog("simERP调试--------------------------", "收到DVE101消息了");
                    ReceiveStream = context.Request.InputStream;
                    readStream = new StreamReader(ReceiveStream, encode);
                    contextLength = context.Request.ContentLength;
                    read = new char[contextLength];
                    count = readStream.Read(read, 0, contextLength);
                    str = null;
                    while (count > 0)
                    {
                        str += new string(read, 0, count);
                        count = readStream.Read(read, 0, contextLength);
                    }

                    returnCode = "0";
                    returnContent = "已成功接收";

                    sendE1DV01(str);
                    pub.WriteLog("simERP调试--------------------------", "处理DVE101完成");
                    break;

                case "DVE102":
                    //模拟DVE102
                    ReceiveStream = context.Request.InputStream;
                    readStream = new StreamReader(ReceiveStream, encode);
                    contextLength = context.Request.ContentLength;
                    read = new char[contextLength];
                    count = readStream.Read(read, 0, contextLength);
                    str = null;

                    while (count > 0)
                    {
                        str += new string(read, 0, count);
                        count = readStream.Read(read, 0, contextLength);
                    }

                    returnCode = "0";
                    returnContent = "已成功接收";

                    sendE1DV02(str);

                    break;

                case "DVE130":
                    //模拟DVE130
                    ReceiveStream = context.Request.InputStream;
                    readStream = new StreamReader(ReceiveStream, encode);
                    contextLength = context.Request.ContentLength;
                    read = new char[contextLength];
                    count = readStream.Read(read, 0, contextLength);
                    str = null;

                    while (count > 0)
                    {
                        str += new string(read, 0, count);
                        count = readStream.Read(read, 0, contextLength);
                    }

                    returnCode = "0";
                    returnContent = "已成功接收";

                    sendE1DV30(str);

                    break;

                case "DVE104":
                    //模拟DVE104
                    break;

                case "DVE105":
                    //模拟DVE105
                    break;

                case "DVE106":
                    //模拟DVE106
                    break;

                case "DVE107":
                    //模拟DVE107
                    break;

                case "DVE108":
                    //模拟DVE108
                    break;

                case "DVE109":
                    //模拟DVE109
                    break;

                case "DVE110":
                    //模拟DVE110
                    break;
            }

            context.Response.AddHeader("returnCode", returnCode);
            context.Response.Write(returnContent);
        }

        private void sendE1DV01(string _recMsg)
        {
            service _s = new service();

            bool _r = recMsgtoObject_sim(_recMsg, out string _deliveryId, out string _key);

            if (!_r)
                return;

            string _msgBody = E1DV01_toJson(_deliveryId, _key);

            Encoding encode = Encoding.GetEncoding("utf-8");
            byte[] arrB = encode.GetBytes(_msgBody);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(SysParam.rfidUrl);
            myReq.Method = "POST";
            myReq.ContentLength = arrB.Length;
            SetHeaderValue(myReq.Headers, "Connection", "keep-alive");
            SetHeaderValue(myReq.Headers, "ContentType", "application/json");
            SetHeaderValue(myReq.Headers, "sourceAppCode", "E1");
            SetHeaderValue(myReq.Headers, "serviceId", "E1DV01");
            SetHeaderValue(myReq.Headers, "msgSendTime", DateTime.Now.ToString("yyyyMMddHHmmssfff"));

            try
            {
                Stream outStream = myReq.GetRequestStream();
                outStream.Write(arrB, 0, arrB.Length);
                outStream.Close();

                WebResponse myResp = myReq.GetResponse();
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
                int Code = Convert.ToInt32(myResp.Headers["returnCode"]);
                readStream.Close();
                myResp.Close();
            }
            catch (Exception e)
            {
                pub.WriteLog("调试，模拟回数据", e.Message);
            }
        }
        private void sendE1DV02(string _recMsg)
        {
            service _s = new service();

            Encoding encode = Encoding.GetEncoding("utf-8");
            byte[] arrB = encode.GetBytes(_recMsg);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(SysParam.rfidUrl);
            myReq.Method = "POST";
            myReq.ContentLength = arrB.Length;
            SetHeaderValue(myReq.Headers, "Connection", "keep-alive");
            SetHeaderValue(myReq.Headers, "ContentType", "application/json");
            SetHeaderValue(myReq.Headers, "sourceAppCode", "E1");
            SetHeaderValue(myReq.Headers, "serviceId", "E1DV02");
            SetHeaderValue(myReq.Headers, "msgSendTime", DateTime.Now.ToString("yyyyMMddHHmmssfff"));

            try
            {
                Stream outStream = myReq.GetRequestStream();
                outStream.Write(arrB, 0, arrB.Length);
                outStream.Close();

                WebResponse myResp = myReq.GetResponse();
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
                int Code = Convert.ToInt32(myResp.Headers["returnCode"]);
                readStream.Close();
                myResp.Close();
            }
            catch (Exception e)
            {
                pub.WriteLog("调试，模拟回数据", e.Message);
            }
        }

        private void sendE1DV30(string _recMsg)
        {
            service _s = new service();

            bool _r = recMsgtoObject_sim(_recMsg, out string _deliveryId, out string _key);

            if (!_r)
                return;

            string _msgBody = E1DV30_toJson(_deliveryId, _key);

            Encoding encode = Encoding.GetEncoding("utf-8");
            byte[] arrB = encode.GetBytes(_msgBody);
            HttpWebRequest myReq = (HttpWebRequest)WebRequest.Create(SysParam.rfidUrl);
            myReq.Method = "POST";
            myReq.ContentLength = arrB.Length;
            SetHeaderValue(myReq.Headers, "Connection", "keep-alive");
            SetHeaderValue(myReq.Headers, "ContentType", "application/json");
            SetHeaderValue(myReq.Headers, "sourceAppCode", "E1");
            SetHeaderValue(myReq.Headers, "serviceId", "E1DV30");
            SetHeaderValue(myReq.Headers, "msgSendTime", DateTime.Now.ToString("yyyyMMddHHmmssfff"));

            try
            {
                Stream outStream = myReq.GetRequestStream();
                outStream.Write(arrB, 0, arrB.Length);
                outStream.Close();

                WebResponse myResp = myReq.GetResponse();
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
                int Code = Convert.ToInt32(myResp.Headers["returnCode"]);
                readStream.Close();
                myResp.Close();
            }
            catch (Exception e)
            {
                pub.WriteLog("调试，模拟回数据", e.Message);
            }
        }


        private void SetHeaderValue(WebHeaderCollection header, string name, string value)
        {
            var property = typeof(WebHeaderCollection).GetProperty("InnerCollection",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (property != null)
            {
                var collection = property.GetValue(header, null) as System.Collections.Specialized.NameValueCollection;
                collection[name] = value;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
        public string E1DV01_toJson(string _deliveryId, string _key)
        {
            bxMessage.bxMsg _msg = new bxMessage.bxMsg();
            bxMessage.bxMsgSysInfo _sysinfo = new bxMessage.bxMsgSysInfo();
            List<bxMessage.bxMsgTablesColumns> _columns = new List<bxMessage.bxMsgTablesColumns>();
            List<bxMessage.bxMsgTables> _tables = new List<bxMessage.bxMsgTables>();

            _sysinfo.Flag = 0;
            _sysinfo.Msg = _key + "#";

            List<dataStructure> _s = sE1DV01();

            foreach (dataStructure _structure in _s)
            {
                bxMessage.bxMsgTablesColumns _column = new bxMessage.bxMsgTablesColumns();
                _column.Caption = _structure.chineseName;
                _column.DataType = _structure.type;
                _column.Name = _structure.englishName;
                _columns.Add(_column);
            }

            bxMessage.bxMsgTables _table = new bxMessage.bxMsgTables();
            _table.Name = "BODY";
            _table.Columns = _columns;

            _table.Rows = new List<object>();

            List<object> _rowItems = new List<object>();
            _rowItems.Add("代码11");
            _rowItems.Add("名称1122");
            _rowItems.Add("短描述1122");
            _rowItems.Add("替代型规1122");
            _rowItems.Add(100);
            _rowItems.Add(0);
            _rowItems.Add(10);
            _rowItems.Add(0);
            _rowItems.Add(0);
            _rowItems.Add("个11");
            _rowItems.Add("送货单号11");
            _rowItems.Add(_deliveryId);
            _rowItems.Add("订单11");
            _rowItems.Add("订单行号2011");
            _rowItems.Add("申请单行号11");
            _rowItems.Add("Y");
            _rowItems.Add("计划类型11");
            _rowItems.Add("Y");
            _rowItems.Add("供应商代码11");
            _rowItems.Add("供应商名称11");
            _rowItems.Add("采购类型11");
            _rowItems.Add("送货单类型11");
            _rowItems.Add("叶类代码11");
            _rowItems.Add("逻辑库区11");
            _table.Rows.Add(_rowItems);

            _tables.Add(_table);

            _msg.Tables = _tables;
            _msg.SysInfo = _sysinfo;

            return JsonConvert.SerializeObject(_msg);
        }

        public string E1DV30_toJson(string _deliveryId, string _key)
        {
            bxMessage.bxMsg _msg = new bxMessage.bxMsg();
            bxMessage.bxMsgSysInfo _sysinfo = new bxMessage.bxMsgSysInfo();
            List<bxMessage.bxMsgTablesColumns> _columns = new List<bxMessage.bxMsgTablesColumns>();
            List<bxMessage.bxMsgTables> _tables = new List<bxMessage.bxMsgTables>();

            _sysinfo.Flag = 1;
            _sysinfo.Msg = _key + "#";

            List<dataStructure> _s = sE1DV30();

            foreach (dataStructure _structure in _s)
            {
                bxMessage.bxMsgTablesColumns _column = new bxMessage.bxMsgTablesColumns();
                _column.Caption = _structure.chineseName;
                _column.DataType = _structure.type;
                _column.Name = _structure.englishName;
                _columns.Add(_column);
            }

            bxMessage.bxMsgTables _table = new bxMessage.bxMsgTables();
            _table.Name = "BODY";
            _table.Columns = _columns;

            _table.Rows = new List<object>();

            for(int i = 0; i < 30; i++)
            {
                List<object> _rowItems = new List<object>();
                _rowItems.Add("姓名" + i);
                _rowItems.Add("工号" + i);
                _rowItems.Add("岗位编码" + i);
                _rowItems.Add("岗位名称" + i);
                _rowItems.Add("部门" + i);
                _rowItems.Add("领用账套" + i);
                _rowItems.Add("采购账套" + i);
                _rowItems.Add("采购组织" + i);
                _rowItems.Add("联系方式" + i);
                _rowItems.Add("邮件" + i);
                _rowItems.Add("地址" + i);
                _table.Rows.Add(_rowItems);
            }

            _tables.Add(_table);

            _msg.Tables = _tables;
            _msg.SysInfo = _sysinfo;

            return JsonConvert.SerializeObject(_msg);
        }

        public bool recMsgtoObject_sim(string _json, out string _outStr, out string _key)
        {
            _outStr = null;
            _key = "";

            try
            {
                bxMessage.bxMsg _msg = JsonConvert.DeserializeObject<bxMessage.bxMsg>(_json);
                Newtonsoft.Json.Linq.JArray _l = (Newtonsoft.Json.Linq.JArray)_msg.Tables[0].Rows[0];
                if (_msg.SysInfo.Msg.Length > 32)
                    _key = _msg.SysInfo.Msg.Substring(0, 32);
                _outStr = _l[0].ToString();
            }
            catch
            {
                return false;
            }
            return true;
        }

        #region 消息结构（李姣---20210709 ）
        public List<dataStructure> sE1DV01()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料短描述", length = 512, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "altModel", chineseName = "替代型规", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderQty", chineseName = "订单量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shippedTotQty", chineseName = "已发货总量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shippedQty", chineseName = "本次送货量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "qualifiedQty", chineseName = "实收合格量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "unqualifiedQty", chineseName = "实收不合格量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryId", chineseName = "送货单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderId", chineseName = "订单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderLineId", chineseName = "订单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invResponserUserId", chineseName = "库存责任人 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "isDirect", chineseName = "是否直送", length = 1, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "supplierId", chineseName = "供应商代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "supplierName", chineseName = "供应商名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryType", chineseName = "送货单类型", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invLogicCode", chineseName = "逻辑库区", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物理库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shiptoId", chineseName = "到货点", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shipDate", chineseName = "到货日期", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "status", chineseName = "送货单状态", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV02()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV03()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料短描述", length = 512, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderQty", chineseName = "订单量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shippedTotQty", chineseName = "已发货总量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shippedQty", chineseName = "本次送货量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "qualifiedQty", chineseName = "实收合格量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "unqualifiedQty", chineseName = "实收不合格量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryId", chineseName = "送货单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderId", chineseName = "订单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderLindId", chineseName = "订单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invResponserUserId", chineseName = "库存责任人 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "isDirect", chineseName = "是否直送", length = 1, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "supplierId", chineseName = "供应商代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "supplierName", chineseName = "供应商名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryType", chineseName = "送货单类型", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invLogicCode", chineseName = "逻辑库区", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物理库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shiptoId", chineseName = "到货点", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "shipDate", chineseName = "到货日期", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "status", chineseName = "状态", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "tempos", chineseName = "临时存放地点", length = 1, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "recCreateTime", chineseName = "创建时刻", length = 14, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "recCreatorName", chineseName = "创建姓名", length = 32, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV04()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "验收单号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV05()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryId", chineseName = "送货单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "receiveId", chineseName = "收货单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料短描述", length = 512, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invLogicCode", chineseName = "逻辑库区", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invBillTo", chineseName = "库存账套", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invQualifiedQty", chineseName = "送货合格数量", length = 19, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invQty", chineseName = "实收数量", length = 19, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物理库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invResponserUserId", chineseName = "库存责任人 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invResponserDeptId", chineseName = "责任人部门 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "accountTime", chineseName = "收货日期 ", length = 14, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "accountUserId", chineseName = "收货人 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderLineId", chineseName = "订单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "custodianJobId", chineseName = "保管员 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "inspectorJobId", chineseName = "验收员 ", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV06()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "transactionId", chineseName = "库存明细交易号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "receiveId", chineseName = "收货单号", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invQty", chineseName = "库存明细", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV07()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "ddId", chineseName = "领用单号", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "reqDeliverDate", chineseName = "要货期", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物理库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invBin", chineseName = "储位", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invKyQtye", chineseName = "可用量", length = 19, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "applyQty", chineseName = "申请数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "releaseQty", chineseName = "实发数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "reqPosId", chineseName = "申请岗位角色编号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料描述", length = 512, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "priceType", chineseName = "计价方式", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "drawType", chineseName = "领用类型", length = 2, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invLogicCode", chineseName = "逻辑库区", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "supMode", chineseName = "采购供应模式", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "planLineId", chineseName = "计划行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "distribuAddress", chineseName = "配送地址", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemType", chineseName = "物料类型", length = 2, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "checkJobId", chineseName = "生成岗位角色编号", length = 50, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV08()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "ddId", chineseName = "领用单号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV09()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物理库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料描述", length = 512, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invQty", chineseName = "库存数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invBin", chineseName = "储位号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "checkDate", chineseName = "盘点日期", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV10()//空
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            return _listS;
        }

        public List<dataStructure> sE1DV11()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "receiveId", chineseName = "收货单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料短描述", length = 512, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invLogicCode", chineseName = "逻辑库区", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invBillTo", chineseName = "库存账套", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invQualifiedQty", chineseName = "送货合格数量", length = 19, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invOrQty", chineseName = "库存预约量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invQty", chineseName = "库存数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物理库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invResponserUserId", chineseName = "库存责任人", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invResponserDeptId", chineseName = "责任人部门", length = 64, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "accountTime", chineseName = "收货日期 ", length = 14, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "accountUserId", chineseName = "收货人 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "orderLineId", chineseName = "订单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "custodianJobId", chineseName = "保管员 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "inspectorJobId", chineseName = "验收员 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "transactionId", chineseName = "库存明细交易号 ", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invTransactionId", chineseName = "库存主表交易号 ", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV12()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "custodianJobId", chineseName = "保管员", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "totalNumber", chineseName = "库存明细总数", length = 20, type = "N" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV17()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "verifyId", chineseName = "验收单号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "verifyIdStatus", chineseName = "验收单状态", length = 2, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "approvalJobId", chineseName = "验收单审批人", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "approvalInstructions", chineseName = "审批说明", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliveryQty", chineseName = "到货数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "verifyTerm", chineseName = "验收依据", length = 64, decimalLength = 4, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "dateOfManufacture", chineseName = "出厂日期", length = 64, decimalLength = 4, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "factoryNumber", chineseName = "出厂编号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemOriginCertificate", chineseName = "原产地证明", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "customsDeclarations", chineseName = "报关单", length = 64, decimalLength = 4, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "cartonPieceId", chineseName = "箱件号 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "qualifiedCertificate", chineseName = "合格证 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "packingList", chineseName = "装箱单", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "instructions", chineseName = "说明书 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "productCertificate", chineseName = "产品证书 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "drawing", chineseName = "图纸 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "pressureVesselCertificate", chineseName = "压力容器检测证 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "liftingEquipmentCertificate", chineseName = "起重机具检测证 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDisputeDesc", chineseName = "物料异议描述 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "supportServicesDesc", chineseName = "配套情况描述 ", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV18() //空
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            return _listS;
        }

        public List<dataStructure> sE1DV19()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "transferId", chineseName = "移拨单号", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "status", chineseName = "状态", length = 1, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 100, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料描述", length = 100, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "transOutInvBillTo", chineseName = "帐套", length = 100, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "transOutInvLogicCode", chineseName = "移出逻辑库存代码", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "transOutInvPhysicCode", chineseName = "移出物理库存代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "transQty", chineseName = "移出数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "transOutInvBin", chineseName = "移出储位", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invResponserUserId", chineseName = "库存责任人ID", length = 64, decimalLength = 4, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "recCreateTime", chineseName = "创建时间", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV20()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "transferId", chineseName = "移拔单号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV21()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "transferId", chineseName = "移拔单号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sE1DV30()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "userName", chineseName = "姓名", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "userId", chineseName = "工号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "postId", chineseName = "岗位编码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "postCname", chineseName = "岗位名称", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deptId", chineseName = "部门", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "ddBillTo", chineseName = "领用账套", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "purBillTo", chineseName = "采购账套", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "purOrgId", chineseName = "采购组织", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "contact", chineseName = "联系方式", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "email", chineseName = "邮件", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "address", chineseName = "地址", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE101()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE102()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "qualifiedQty", chineseName = "实收合格量", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "unqualifiedQty", chineseName = "实收不合格量", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "jobId", chineseName = "操作人岗位号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE103()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE104()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemOriginCertificate", chineseName = "原产地证明", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "productCertificate", chineseName = "产品证书 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "qualifiedCertificate", chineseName = "合格证 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "verifyAttachment", chineseName = "合格证 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "dateOfManufacture", chineseName = "出厂日期", length = 64, decimalLength = 4, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "cartonPieceId", chineseName = "箱件号 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "packingList", chineseName = "装箱单", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "factoryNumber", chineseName = "出厂编号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "customsDeclarations", chineseName = "报关单", length = 64, decimalLength = 4, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "instructions", chineseName = "说明书 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "drawing", chineseName = "图纸 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "liftingEquipmentCertificate", chineseName = "起重机具检测证 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "pressureVesselCertificate", chineseName = "压力容器检测证 ", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "jobId", chineseName = "操作人岗位号 ", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE105()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE106()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物料库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicName", chineseName = "物理库区名称", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invBin", chineseName = "单储位", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "numBin", chineseName = "多储位1,数量@", length = 250, type = "C" }; _listS.Add(_s);//格式：储位1,数量,生产日期(20210628)!@储位2,数量,生产日期!@储位3,数量,生产日期
            _s = new dataStructure { englishName = "jobId", chineseName = "操作人岗位号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE107()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "ddId", chineseName = "领用单号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE108()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "ddId", chineseName = "领用单号", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "releaseQty", chineseName = "实发数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "deliverComments", chineseName = "送货提示", length = 200, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "jobId", chineseName = "操作人岗位号", length = 20, type = "C" }; _listS.Add(_s);//没写长度、类型

            return _listS;
        }

        public List<dataStructure> sDVE109()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            _s = new dataStructure { englishName = "itemId", chineseName = "物料代码", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemName", chineseName = "物料名称", length = 256, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invPhysicCode", chineseName = "物理库区", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemDesc", chineseName = "物料描述", length = 512, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "itemUom", chineseName = "计量单位", length = 16, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invQty", chineseName = "库存数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "invBin", chineseName = "储位号", length = 64, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "checkDate", chineseName = "盘点日期", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "jobId", chineseName = "操作人岗位号", length = 20, type = "C" }; _listS.Add(_s);//没写长度、类型
            _s = new dataStructure { englishName = "transactionId", chineseName = "库存明细交易号", length = 64, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE110()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "transactionId", chineseName = "库存明细交易号", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "newinvCw", chineseName = "新储位", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "newinvQty", chineseName = "新储位数量", length = 20, decimalLength = 4, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "jobId", chineseName = "操作人岗位号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE111()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "custodianJobId", chineseName = "保管员岗位号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE117()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE118()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "verifyId", chineseName = "验收单号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE119()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "transactionId", chineseName = "库存明细交易号", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "yrPhysicCode", chineseName = "移入物理库区", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "ycQty", chineseName = "移出数量", length = 20, type = "N" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "jobId", chineseName = "操作人岗位号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE120()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "transferId", chineseName = "移拔单号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE121()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "transferId", chineseName = "移拔单号", length = 20, type = "C" }; _listS.Add(_s);
            _s = new dataStructure { englishName = "yrInvCw", chineseName = "移入储位", length = 20, type = "C" }; _listS.Add(_s);//staus=1时，该值必传
            _s = new dataStructure { englishName = "status", chineseName = "确认/拒绝", length = 1, type = "C" }; _listS.Add(_s);//0.拒绝，1.确认

            return _listS;
        }

        public List<dataStructure> sDVE124()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE125()
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();
            _s = new dataStructure { englishName = "deliveryLineId", chineseName = "送货单行号", length = 20, type = "C" }; _listS.Add(_s);

            return _listS;
        }

        public List<dataStructure> sDVE130()//空
        {
            List<dataStructure> _listS = new List<dataStructure>();
            dataStructure _s = new dataStructure();

            return _listS;
        }

        #endregion
    }
}