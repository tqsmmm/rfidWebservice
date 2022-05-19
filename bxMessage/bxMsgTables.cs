using System;
using System.Collections.Generic;

namespace rfidWebservice.bxMessage
{
    public class bxMsgTables
    {
        public List<bxMsgTablesColumns> Columns { get; set; }
        public string Name { get; set; }
        public List<Object> Rows { get; set; }
    }
}