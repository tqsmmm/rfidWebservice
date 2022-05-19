using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;

namespace rfidWebservice
{

    public class service
    {
        #region json和dataset互转
        /// <summary>
        /// 根据来的json生成dataset，支持多表
        /// </summary>
        /// <param name="_msgName"></param>
        /// <param name="_json"></param>
        /// <param name="_ds"></param>
        /// <returns></returns>
        public MessagePack recMsgToDataSet(string _msgName, string _json, out System.Data.DataSet _ds, out string _key)
        {
            _key = "";
            _ds = new System.Data.DataSet();
            MessagePack _mp = new MessagePack();
            try
            {
                bxMessage.bxMsg _msg = JsonConvert.DeserializeObject<bxMessage.bxMsg>(_json);
                if (_msg.Tables.Count < 1)
                {
                    _mp.Code = 2301;
                    _mp.Message = "接收到的报文中表数量小于1";
                    _mp.Result = false;
                    return _mp;
                }

                foreach(bxMessage.bxMsgTables _jsonDt in _msg.Tables)
                {
                    System.Data.DataTable _dt = new System.Data.DataTable();
                    _dt.TableName = _msgName + "," + _jsonDt.Name;

                    foreach (bxMessage.bxMsgTablesColumns _msgCol in _jsonDt.Columns)
                    {
                        Type _type = typeof(string);
                        if (_msgCol.DataType.Equals("N"))
                            _type = typeof(float);

                        System.Data.DataColumn _col = new System.Data.DataColumn(_msgCol.Name, _type);
                        _col.Caption = _msgCol.Caption;
                        _dt.Columns.Add(_col);
                    }

                    int columnsCount = _dt.Columns.Count;

                    for (int rowIndex = 0; rowIndex < _jsonDt.Rows.Count; rowIndex++)
                    {
                        Newtonsoft.Json.Linq.JArray _item = (Newtonsoft.Json.Linq.JArray)_jsonDt.Rows[rowIndex];
                        if (_item.Count != columnsCount)
                        {
                            _mp.Code = 2301;
                            _mp.Message = "接收到的报文中表《"+ _jsonDt.Name + "》中行[" + rowIndex + "]的列数不正确";
                            _mp.Result = false;
                            return _mp;
                        }

                        System.Data.DataRow _row = _dt.NewRow();

                        for (int columnIndex = 0; columnIndex < columnsCount; columnIndex++)
                        {
                            _row[columnIndex] = _item[columnIndex];
                        }
                        _dt.Rows.Add(_row);
                    }
                    _ds.Tables.Add(_dt);
                }

                System.Data.DataTable _dtInfo = new System.Data.DataTable();
                _dtInfo.TableName = "SysInfo";
                System.Data.DataColumn _col1 = new System.Data.DataColumn("Flag", typeof(int));
                _dtInfo.Columns.Add(_col1);
                _col1 = new System.Data.DataColumn("Msg", typeof(string));
                _dtInfo.Columns.Add(_col1);
                _col1 = new System.Data.DataColumn("Key", typeof(string));
                _dtInfo.Columns.Add(_col1);

                System.Data.DataRow _row1 = _dtInfo.NewRow();
                _row1[0] = _msg.SysInfo.Flag;
                if (_msg.SysInfo.Msg.Length> 32)
                    _row1[1] = _msg.SysInfo.Msg.Substring(33);
                else
                    _row1[1] = _msg.SysInfo.Msg;
                _dtInfo.Rows.Add(_row1);

                _ds.Tables.Add(_dtInfo);


                _key = _msg.SysInfo.Msg.Substring(0, 32);


                _mp.Code = 0;
                _mp.Message = "接收到的报文，数据转换成功";
                _mp.Result = true;
            }
            catch (Exception e)
            {
                _mp.Code = 2301;
                _mp.Message = e.Message;
                _mp.Result = false;
                return _mp;
            }
            return _mp;
        }

        /// <summary>
        /// 根据来的dataset生成json，支持多表
        /// </summary>
        /// <param name="_msgName"></param>
        /// <param name="_inDs"></param>
        /// <param name="_json"></param>
        /// <returns></returns>
        public MessagePack sendMsgToJson(string _msgName, string _key, System.Data.DataSet _inDs, out string _json)
        {
            bxMessage.bxMsg _msg = new bxMessage.bxMsg();
            bxMessage.bxMsgSysInfo _sysinfo = new bxMessage.bxMsgSysInfo();
            List<bxMessage.bxMsgTablesColumns> _columns = new List<bxMessage.bxMsgTablesColumns>();
            List<bxMessage.bxMsgTables> _tables = new List<bxMessage.bxMsgTables>();

            _sysinfo.Flag = 0;
            _sysinfo.Msg = _key+"#";

            _json = null;

            bxMessage.bxMsgTables _table = new bxMessage.bxMsgTables();
            _table.Name = "BODY";
            _table.Columns = _columns;

            MessagePack _mp = new MessagePack();

            if (_inDs.Tables.Count < 1)
            {
                _mp.Result = false;
                _mp.Code = -1;
                _mp.Message = "DataSet中表数量小于1";
                return _mp;
            }

            if (_inDs.Tables[0].Columns.Count < 1)
            {
                _mp.Result = false;
                _mp.Code = -1;
                _mp.Message = "DataSet中表[0]中列数量小于1";
                return _mp;
            }

            try
            {
                foreach(System.Data.DataTable _dsTable in _inDs.Tables)
                {
                    foreach (System.Data.DataColumn _dsCol in _dsTable.Columns)
                    {
                        bxMessage.bxMsgTablesColumns _column = new bxMessage.bxMsgTablesColumns();
                        _column.Caption = _dsCol.Caption;
                        _column.DataType = "N";
                        if (_dsCol.DataType == typeof(string))
                            _column.DataType = "S";

                        _column.Name = _dsCol.ColumnName;
                        _columns.Add(_column);
                    }

                    _table.Rows = new List<Object>();
                    foreach (System.Data.DataRow _dr in _dsTable.Rows)
                    {
                        List<Object> _rowItems = new List<Object>();
                        for (int rowIndex = 0; rowIndex < _dr.ItemArray.Length; rowIndex++)
                        {
                            _rowItems.Add(_dr[rowIndex]);
                        }

                        _table.Rows.Add(_rowItems);
                    }

                    _tables.Add(_table);
                }

                _msg.Tables = _tables;
                _msg.SysInfo = _sysinfo;

                _json = JsonConvert.SerializeObject(_msg);

                _mp.Code = 0;
                _mp.Result = true;
                _mp.Message = "成功";

            }
            catch (Exception e)
            {
                _mp.Result = false;
                _mp.Code = -9005;
                _mp.Message = e.Message;
            }

            return _mp;
        }

        #endregion

        #region 接收报文处理
        /// <summary>
        /// E1DV01 送货单到货登记明细
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV01(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV01";
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string _itemId = dr["itemId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_deliveryLine(deliveryLineId, itemId) values('" + _deliveryLineId + "', '" + _itemId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 =sql2 +  dc.ColumnName + " = '"+ dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '"+ _deliveryLineId + "' and itemId = '"+ _itemId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch(Exception  e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理"+serviceId+"报文错误：" + e.Message;
                pub.WriteErrLog("service.rec"+ serviceId+"错误：" + e.Message);
            }
            return pack;
        }

        /// <summary>
        /// E1DV02 送货单到货登记实绩确认
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV02(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();


                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;

                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV02报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV02错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV03 送货单验收录入明细
        /// </summary>
        /// <param name="_sendDs"></param>
        /// <param name="_recDs"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV03(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV03";

            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string _itemId = dr["itemId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_deliveryLine(deliveryLineId, itemId) values('" + _deliveryLineId + "', '" + _itemId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "' and itemId = '" + _itemId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }
            return pack;
        }

        /// <summary>
        /// E1DV04 送货单验收录入实绩确认
        /// </summary>
        /// <param name="_sendDs"></param>
        /// <param name="_recDs"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV04(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    #region 更新提交的数据
                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();


                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    #endregion 更新提交的数据
                    pack.Code = 1;
                    pack.Result = true;

                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV04报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV04错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV05 送货单收货入库明细
        /// </summary>
        /// <param name="_recDs"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV05(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV05";

            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    /*
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string _itemId = dr["itemId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_deliveryLine(deliveryLineId, itemId) values('" + _deliveryLineId + "', '" + _itemId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "' and itemId = '" + _itemId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    */
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV06 送货单收货入库实绩确认
        /// </summary>
        /// <param name="_sendDs"></param>
        /// <param name="_recDs"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV06(DataSet _sendDs, DataSet _recDs)
        {
            /*
             * 问题：
             * 1.不知道这里返回是单行数据，还是多行数据。
             * 2.不知道主键是什么
             * 3.现在数据库的主键是 transactionId
             * 4.这里没写入提交的数据，只写入返回的数据。
             * 
             */
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    #region 更新返回的数据

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _transactionId = dr["transactionId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_transaction(transactionId) values('" + _transactionId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_transaction set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where transactionId = '" + _transactionId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }


                    #endregion

                    pack.Code = 1;
                    pack.Result = true;

                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV06报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV06错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV07 发料出库明细
        /// </summary>
        /// <param name="_recDs"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV07(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV07";
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _ddId = dr["ddId"].ToString();
                        string _itemId = dr["itemId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_dd(ddId, itemId) values('" + _ddId + "', '" + _itemId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_dd set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where ddId = '" + _ddId + "' and itemId = '" + _itemId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV08 发料单配送出库实绩确认
        /// </summary>
        /// <param name="_sendDs"></param>
        /// <param name="_recDs"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV08(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (!_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();
                    #region 更新提交的数据
                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _ddId = dr["ddId"].ToString();

                        string sql2 = "update RF_Database_CZ.dbo.bx_dd set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where ddId = '" + _ddId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    #endregion 更新提交的数据
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV06报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV06错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV09 盘库实绩确认
        /// </summary>
        /// <param name="_sendDs"></param>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV09(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    /*
                     * E1DV09 不保存到数据库
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _transactionId = dr["transactionId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_transaction(transactionId) values('" + _transactionId + "')";
                        string sql2 = "update RF_Database_CZ.dbo.transactionId set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where transactionId = '" + _transactionId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    */
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV09报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV09错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV10 储位修改请求返回
        /// </summary>
        /// <param name="_sendDs"></param>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV10(DataSet _sendDs, DataSet _recDs)
        {

            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV10报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV10错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV11 保管员库存明细信息
        /// </summary>
        /// <param name="_recDs"></param>
        public void recE1DV11(DataSet _recDs)
        {
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    return;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    string sql = "insert into RF_Database_CZ.dbo.bx_transaction_E1DV11(";

                    foreach (DataColumn dc in _recDs.Tables[0].Columns)
                    {
                        sql += dc.ColumnName + ", ";
                    }
                    sql = sql.Substring(0, sql.Length - 2) + ")";

                    List<string> listSql = new List<string>();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string sql2 = sql + " values(";

                        for (int index = 0; index < dr.ItemArray.Length; index++)
                        {
                            sql2 += "'" + dr[index].ToString().Replace("'","''") + "', ";
                        }

                        sql2 = sql2.Substring(0, sql2.Length - 2) + ")";
                        listSql.Add(sql2);
                    }

                    if (_ado.ExecuteNonQuery(listSql) < 0)
                    {
                        pub.WriteErrLog("E1DV11 保管员库存明细信息保存错误");
                        return;
                    }
                }
            }
            catch (Exception e)
            {
                pub.WriteErrLog("service.recE1DV11错误：" + e.Message);
            }
        }

        /// <summary>
        /// E1DV12 保管员库存明细确认
        /// </summary>
        /// <param name="_recDs"></param>
        public MessagePack recE1DV12(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    string sql = "insert into RF_Database_CZ.dbo.bx_transaction_E1DV12(";

                    foreach (DataColumn dc in _recDs.Tables[0].Columns)
                    {
                        sql += dc.ColumnName + ", ";
                    }
                    sql = sql.Substring(0, sql.Length - 2) + ")";

                    List<string> listSql = new List<string>();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string sql2 = sql + " values(";
                        for (int index = 0; index < dr.ItemArray.Length; index++)
                            sql2 += "'" + dr[index].ToString() + "', ";

                        sql2 = sql2.Substring(0, sql2.Length - 2) + ")";
                        listSql.Add(sql2);
                    }
                    if (_ado.ExecuteNonQuery(listSql) < 0)
                    {
                        pack.Code = -1;
                        pack.Result = false;
                        pack.Message = "本地数据保存错误";
                        return pack;
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV12报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV12错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV17 验收单删除请求明细
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV17(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV17";
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _verifyId = dr["verifyId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_verify(verifyId) values('" + _verifyId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_verify set ";

                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }

                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where verifyId = '" + _verifyId + "'";
                        _ado.ExecuteNonQuery(sql);

                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV18 验收单删除请求确认
        /// </summary>
        /// <param name="_recDs"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV18(DataSet _recDs)
        {
            /*
             * 问题：
             * 1.这里没有更新验收单数据
             * 2.是否需要更新验收单数据？
             * 
             */
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {


                    pack.Code = 1;
                    pack.Result = true;

                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV18报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV18错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV19 移拔单生成请求返回
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV19(DataSet _sendDs, DataSet _recDs)
        {
            string serviceId = "E1DV19";
            /*
             * 问题：
             * 1.是否返回多行数据，这里只处理单行数据
             * 
             */
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    string _transferId = _recDs.Tables[0].Rows[0]["transferId"].ToString();
  
                    pack.Message = "移拨单生成成功\r\n移拨单号："+_transferId;
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV20 移出确认请求返回信息
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV20(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    pack.Code = 1;
                    pack.Result = true;

                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV02报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV02错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV21 移入确认/拒绝请求返回信息
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV21(DataSet _sendDs, DataSet _recDs)
        {
            string serviceId = "E1DV21";
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV22 质检确认请求返回
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV22(DataSet _recDs)
        {
            string serviceId = "E1DV22";
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string _itemId = dr["itemId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_deliveryLine(deliveryLineId, itemId) values('" + _deliveryLineId + "', '" + _itemId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "' and itemId = '" + _itemId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV23 质检确认请求实绩返回
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV23(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV23报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV23错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV24 送货确认请求返回
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV24(DataSet _recDs)
        {
            string serviceId = "E1DV24";
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_deliveryLine(deliveryLineId) values('" + _deliveryLineId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV25 送货确认实绩确认返回
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV25(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV25报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV25错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV26 发料完成确认查询请求回执
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV26(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV26";
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _ddId = dr["ddId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_dd(ddId) values('" + _ddId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_dd set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where ddId = '" + _ddId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV27 发料完成确认回执
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV27(DataSet _sendDs, DataSet _recDs)
        {
            /*
             * 问题：
             * 1.这里是否需要更新发料状态？
             */
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _ddId = dr["ddId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_dd(ddId) values('" + _ddId + "')";

                        string sql2 = "update RF_Database_CZ.dbo.bx_dd set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where ddId = '" + _ddId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;

                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV27报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV27错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV29 查询库存明细请求回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV29(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _transactionId = dr["transactionId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_transaction(transactionId) values('" + _transactionId + "')";
                        string sql2 = "update RF_Database_CZ.dbo.bx_transaction set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where transactionId = '" + _transactionId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV29报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV29错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV30 同步保管员信息返回
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV30(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV30";
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    string sql = "insert into bx_" + serviceId + "(uptime, ";

                    foreach (DataColumn dc in _recDs.Tables[0].Columns)
                    {
                        sql += dc.ColumnName + ", ";
                    }
                    sql = sql.Substring(0, sql.Length - 2) + ")";

                    List<string> listSql = new List<string>();

                    listSql.Add("delete bx_E1DV30");

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string sql2 = sql + " values('" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ";
                        for (int index = 0; index < dr.ItemArray.Length; index++)
                            sql2 += "'" + dr[index].ToString() + "', ";

                        sql2 = sql2.Substring(0, sql2.Length - 2) + ")";
                        listSql.Add(sql2);
                    }
                    data.ado _ado = new data.ado();
                    if (_ado.ExecuteNonQuery(listSql) < 0)
                    {
                        pack.Code = -1;
                        pack.Result = false;
                        pack.Message = "本地数据保存错误";
                        return pack;
                    }
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV31 同步物理库区回执，新增报文
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV31(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            string serviceId = "E1DV31";
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    string sql = "insert into bx_" + serviceId + "(uptime,";

                    foreach (DataColumn dc in _recDs.Tables[0].Columns)
                    {
                        sql += dc.ColumnName + ", ";
                    }
                    sql = sql.Substring(0, sql.Length - 2) + ")";

                    List<string> listSql = new List<string>();

                    listSql.Add("delete bx_E1DV31");

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string sql2 = sql + " values('" + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "', ";
                        for (int index = 0; index < dr.ItemArray.Length; index++)
                            sql2 += "'" + dr[index].ToString() + "', ";

                        sql2 = sql2.Substring(0, sql2.Length - 2) + ")";
                        listSql.Add(sql2);
                    }
                    data.ado _ado = new data.ado();
                    if (_ado.ExecuteNonQuery(listSql) < 0)
                    {
                        pack.Code = -1;
                        pack.Result = false;
                        pack.Message = "本地数据保存错误";
                        return pack;
                    }
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理" + serviceId + "报文错误：" + e.Message;
                pub.WriteErrLog("service.rec" + serviceId + "错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV32 直送现场物料发料完成确认实绩回执
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV32(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _ddId = dr["ddId"].ToString();
                        string sql2 = "update RF_Database_CZ.dbo.bx_dd set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where ddId = '" + _ddId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV32报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV32错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV33 直送现场物料发料完成取消实绩回执
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV33(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _ddId = dr["ddId"].ToString();
                        string sql2 = "update RF_Database_CZ.dbo.bx_dd set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where ddId = '" + _ddId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;

                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV33报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV33错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV35 直送货二次验收确认实绩回执
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV35(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string _ddid = dr["ddId"].ToString();
                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                        pack.Message = "直送货二次确认成功\r\n领用单号：" + _ddid;
                        pack.Code = 1;
                        pack.Result = true;
                    }
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV35报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV35错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV36 直送货二次验收取消实绩回执
        /// </summary>
        /// <param name="ds"></param>
        /// <param name="dbUUID"></param>
        /// <returns></returns>
        public MessagePack recE1DV36(DataSet _sendDs, DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _sendDs.Tables[0].Rows)
                    {
                        string _deliveryLineId = dr["deliveryLineId"].ToString();
                        string sql2 = "update RF_Database_CZ.dbo.bx_deliveryLine set ";
                        foreach (DataColumn dc in _sendDs.Tables[0].Columns)
                        {
                            if (dc.ColumnName.Equals("userId")) continue;
                            if (dc.ColumnName.Equals("userName")) continue;
                            if (dc.ColumnName.Equals("jobId")) continue;
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where deliveryLineId = '" + _deliveryLineId + "'";
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV35报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV35错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV37 取消到货登记实绩回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV37(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }

                pack.Code = 1;
                pack.Result = true;
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV37报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV37错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV38 取消质检确认实绩回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV38(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }

                pack.Code = 1;
                pack.Result = true;
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV38报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV38错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV39 取消到货确认实绩回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV39(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }

                pack.Code = 1;
                pack.Result = true;
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV39报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV39错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV40 收货单删除实绩回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV40(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }

                pack.Code = 1;
                pack.Result = true;
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV40报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV40错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV41 发料单实发量取消实绩回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV41(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }

                pack.Code = 1;
                pack.Result = true;
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV41报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV41错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV42 查询移拨单信息请求回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV42(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    data.ado _ado = new data.ado();

                    foreach (DataRow dr in _recDs.Tables[0].Rows)
                    {
                        string _transferId = dr["transferId"].ToString();
                        string sql = "insert into RF_Database_CZ.dbo.bx_transfer(transferId) values('" + _transferId + "')";
                        string sql2 = "update RF_Database_CZ.dbo.bx_transfer set ";
                        foreach (DataColumn dc in _recDs.Tables[0].Columns)
                        {
                            sql2 = sql2 + dc.ColumnName + " = '" + dr[dc.ColumnName].ToString() + "', ";
                        }
                        sql2 = sql2.Substring(0, sql2.Length - 2) + " where transferId = '" + _transferId + "'";
                        _ado.ExecuteNonQuery(sql);
                        if (_ado.ExecuteNonQuery(sql2) < 0)
                        {
                            pack.Code = -1;
                            pack.Result = false;
                            pack.Message = "本地数据保存错误";
                            return pack;
                        }
                    }
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV42报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV42错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV43 验收单审批确认回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV43(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {
                    
                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV43报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV43错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV44 查询送货单列表请求回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV44(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV44报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV44错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV45 查询领用单列表请求回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV45(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV45报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV45错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV46 查询库存列表请求回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV46(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV46报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV46错误：" + e.Message);
            }

            return pack;
        }

        /// <summary>
        /// E1DV47 查询移拨单列表请求回执
        /// </summary>
        /// <param name="_recDs"></param>
        /// <returns></returns>
        public MessagePack recE1DV47(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV47报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV47错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV48(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV48报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV48错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV49(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV49报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV49错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV50(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV50报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV50错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV51(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV51报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV51错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV52(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV52报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV52错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV53(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV53报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV53错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV54(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV54报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV54错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV56(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV56报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV56错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV57(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV57报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV57错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV58(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV58报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV58错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV59(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV59报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV59错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV60(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV60报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV60错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV61(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV61报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV61错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV62(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV62报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV62错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV63(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV63报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV63错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV64(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV64报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV64错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV65(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV65报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV65错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV66(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV66报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV66错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV67(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV67报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV67错误：" + e.Message);
            }

            return pack;
        }

        public MessagePack recE1DV68(DataSet _recDs)
        {
            MessagePack pack = new MessagePack();
            try
            {
                if (_recDs.Tables["SysInfo"].Rows[0]["Flag"].ToString().Equals("-1"))
                {
                    //当msg.flag为-1时，ERP系统处理业务不成功
                    pack.Code = -1;
                    pack.Result = false;
                    pack.Message = _recDs.Tables["SysInfo"].Rows[0]["Msg"].ToString();
                    return pack;
                }
                else
                {

                    pack.Code = 1;
                    pack.Result = true;
                }
            }
            catch (Exception e)
            {
                pack.Code = -1;
                pack.Result = false;
                pack.Message = "处理E1DV68报文错误：" + e.Message;
                pub.WriteErrLog("service.recE1DV68错误：" + e.Message);
            }

            return pack;
        }

        #endregion
    }
}