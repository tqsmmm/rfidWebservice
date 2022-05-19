using System.Collections.Generic;

namespace rfidWebservice.bxMessage
{
    public class bxMsg
    {
        public List<bxMsgTables> Tables { get; set; }
        public bxMsgSysInfo SysInfo { get; set; }
    }
}