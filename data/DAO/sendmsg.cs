using System.Collections.Generic;

namespace rfidWebservice.data.DAO
{
    /// <summary>
    /// 发送到ERP的消息 DTO
    /// </summary>
    public class sendmsg
    {
        public int id { get; set; }
        public string BxUserID { get; set; }
        public string serviceid { get; set; }
        public string sourceappcode { get; set; }
        public string msg { get; set; }
        public string sendtime { get; set; }

        public MessagePack save()
        {
            MessagePack _mp = new MessagePack();

            if (BxUserID.Length == 0)
            {
                _mp.Message = "用户ID为空，不能保存";
                return _mp;
            }

            if (serviceid.Length == 0)
            {
                _mp.Message = "serviceid为空，不能保存";
                return _mp;
            }

            if (sourceappcode.Length == 0)
            {
                _mp.Message = "sourceappcode为空，不能保存";
                return _mp;
            }

            if (msg.Length == 0)
            {
                _mp.Message = "msg为空，不能保存";
                return _mp;
            }

            if (sendtime.Length == 0)
            {
                _mp.Message = "sendtime为空，不能保存";
                return _mp;
            }

            string sqlstr;

            if (id == 0)
            {
                //添加
                sqlstr = "insert into t_sendmsg(userid, serviceid, sourceappcode, msg, sendtime) values(@userid,@serviceid,@sourceappcode,@msg,@sendtime)";
            }
            else
            {
                //更新
                sqlstr = "update t_sendmsg set  userid = @userid, serviceid = @serviceid, sourceappcode = @sourceappcod, msg = @msg, sendtime = @sendtime where id = '" + id+"'";
            }

            List<Sqlparameter> parameter = new List<Sqlparameter>();
            parameter.Add(new Sqlparameter { parameter_name = "@userid", parameter_value = BxUserID });
            parameter.Add(new Sqlparameter { parameter_name = "@serviceid", parameter_value = serviceid });
            parameter.Add(new Sqlparameter { parameter_name = "@sourceappcode", parameter_value = sourceappcode });
            parameter.Add(new Sqlparameter { parameter_name = "@msg", parameter_value = msg });
            parameter.Add(new Sqlparameter { parameter_name = "@sendtime", parameter_value = sendtime });

            ado _ado = new ado();

            if (_ado.ExecuteNonQuery(sqlstr, parameter) < 1)
            {
                _mp.Result = false;
                _mp.Code = -1;
                _mp.Message = "保存数据失败";
            }
            else
            {
                _mp.Result = true;

                _mp.Code = 0;
            }

            return _mp;
        }
    }
}