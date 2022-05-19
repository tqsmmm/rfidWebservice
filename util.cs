using System.Collections.Generic;
using System.Data;

namespace rfidWebservice
{
    public class util
    {
        public static DataTable JsonToTable(string json)
        {
            DataTable _dt = new DataTable();

            return _dt;
        }

        public static string DataTableToJson(DataTable dt)
        {
            bxMessage.bxMsgSysInfo _sysinfo = new bxMessage.bxMsgSysInfo();
            _sysinfo.Flag = 0;
            _sysinfo.Msg = "";

            List<bxMessage.bxMsgTablesColumns> _columns = new List<bxMessage.bxMsgTablesColumns>();

            bxMessage.bxMsgTablesColumns _column = new bxMessage.bxMsgTablesColumns();

            DataTable _dt = new DataTable();
            
            _column.Name = "deliveryId";
            _column.Caption = "标题";
            _column.DataType = "S";
            
            _columns.Add(_column);

            return "";
        }
    }
}