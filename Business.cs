using System.Data;

namespace rfidWebservice
{
    public class Business
    {

        #region 登录模块
        //登录
        public int Login(string User, string Pass,out string bxUserId, out string bxUserName, out string bxJobId, out privilidge PrivateStr, out string ErrMsg)
        {
            PrivateStr = new privilidge();
            ErrMsg = "";
            bxUserId = "";
            bxUserName = "";
            bxJobId = "";

            data.ado _ado = new data.ado();
            DataTable dt1;

            if (_ado.ExecuteQuery("SELECT userId, userName, postId FROM rfid2021.dbo.bx_E1DV30 where userId = '" + User + "'", out dt1) < 1)
            {
                ErrMsg = "用户登录失败";
                return -1;
            }

            bxUserId = dt1.Rows[0]["userId"].ToString();
            bxUserName = dt1.Rows[0]["userName"].ToString();
            bxJobId = dt1.Rows[0]["postId"].ToString();

            PrivateStr.isAdmin = true;
            PrivateStr.BF = "1";
            PrivateStr.CGRK = "1";
            PrivateStr.CGSH = "1";
            PrivateStr.DGRK = "1";
            PrivateStr.PANDIAN = "1";
            PrivateStr.PrintBin = "1";
            PrivateStr.PrintLabel = "1";
            PrivateStr.SCLL = "1";
            PrivateStr.TH = "1";
            PrivateStr.YCRK = "1";
            PrivateStr.YK = "1";
            PrivateStr.YK = "1";
            PrivateStr.SJ = "1";
            PrivateStr.DBGCK = "1";
            PrivateStr.HWCX = "1";
            PrivateStr.PCCX = "1";
            PrivateStr.BYD = "1";
            PrivateStr.KCTH = "1";
            PrivateStr.DJKTH = "1";

            /*
            OutPass pass = new OutPass();
            Pass = pass.DbPass(Pass);

            data.ado _ado = new data.ado();
            System.Data.DataTable dt1;
            if (_ado.ExecuteQuery("SELECT User_ID, User_Name, PASSWORD, SapRolePoint, IsAdmin, InEffect, username,postid FROM rfid2021.dbo.RF_Users, rfid2021.dbo.bx_E1DV30 where rfid2021.dbo.RF_Users.SapRolePoint = rfid2021.dbo.bx_E1DV30.userId and User_ID = '" + User + "' and PassWord = '" + Pass + "'", out dt1) < 1)
            {
                ErrMsg = "用户登录失败";
                return -1;
            }

            bxUserId = dt1.Rows[0]["SapRolePoint"].ToString();
            bxUserName = dt1.Rows[0]["username"].ToString();
            bxJobId = dt1.Rows[0]["postid"].ToString();

            System.Data.DataTable dt;
            if (_ado.ExecuteQuery("select * from RF_UserRoles where User_ID ='" + User + "'", out dt) < 1)
            {
                ErrMsg = "获取用户权限出现错误";
                return -1;
            }
            System.Data.DataTable dtIsAdmin;
            if (_ado.ExecuteQuery("select * from RF_Users where User_ID ='" + User + "'", out dtIsAdmin) < 1)
            {
                ErrMsg = "获取用户权限出现错误";
                return -1;
            }

            if (dtIsAdmin.Rows[0]["IsAdmin"].ToString().Trim() == "True")
            {
                PrivateStr.isAdmin = true;
            }
            else
            {
                PrivateStr.isAdmin = false;
            }
            System.Data.DataRow[] drs = dt.Select("RoleID='DiscardAsUseless'");
            if (drs.Length <= 0) //报废
            {
                PrivateStr.BF = "0";
            }
            else
            {
                PrivateStr.BF = "1";
            }
            drs = dt.Select("RoleID='StockCheckAndEnterStore'");
            if (drs.Length <= 0) //采购入库
            {
                PrivateStr.CGRK = "0";
            }
            else
            {
                PrivateStr.CGRK = "1";
            }

            drs = dt.Select("RoleID='StockReceive'");
            if (drs.Length <= 0) //采购收货
            {
                PrivateStr.CGSH = "0";
            }
            else
            {
                PrivateStr.CGSH = "1";
            }


            drs = dt.Select("RoleID='SupplyEnterStore'");
            if (drs.Length <= 0) //代管入库
            {
                PrivateStr.DGRK = "0";
            }
            else
            {
                PrivateStr.DGRK = "1";
            }


            drs = dt.Select("RoleID='StoreMakeInventory'");
            if (drs.Length <= 0) //盘点
            {
                PrivateStr.PANDIAN = "0";
            }
            else
            {
                PrivateStr.PANDIAN = "1";
            }


            drs = dt.Select("RoleID='GoodsPositionBarCodePrint'");
            if (drs.Length <= 0) //打印货位
            {
                PrivateStr.PrintBin = "0";
            }
            else
            {
                PrivateStr.PrintBin = "1";
            }


            drs = dt.Select("RoleID='MaterielBarCodePrint'");
            if (drs.Length <= 0) //打印物料标签
            {
                PrivateStr.PrintLabel = "0";
            }
            else
            {
                PrivateStr.PrintLabel = "1";
            }

            drs = dt.Select("RoleID='ManufactureReceiveMaterial'");
            if (drs.Length <= 0) //生产领料
            {
                PrivateStr.SCLL = "0";
            }
            else
            {
                PrivateStr.SCLL = "1";
            }

            drs = dt.Select("RoleID='UntreadGoods'");
            if (drs.Length <= 0) //退货
            {
                PrivateStr.TH = "0";
            }
            else
            {
                PrivateStr.TH = "1";
            }

            drs = dt.Select("RoleID='ArriveStoreException'");
            if (drs.Length <= 0) //异议入库
            {
                PrivateStr.YCRK = "0";
            }
            else
            {
                PrivateStr.YCRK = "1";
            }

            drs = dt.Select("RoleID='GoodsPositionRemove'");
            if (drs.Length <= 0) //移库
            {
                PrivateStr.YK = "0";
            }
            else
            {
                PrivateStr.YK = "1";
            }

            drs = dt.Select("RoleID='StoreInfoSeeAbout'");
            if (drs.Length <= 0) //查询
            {
                PrivateStr.YK = "0";
            }
            else
            {
                PrivateStr.YK = "1";
            }

            drs = dt.Select("RoleID='UpShelf'");
            if (drs.Length <= 0) //上架
            {
                PrivateStr.SJ = "0";
            }
            else
            {
                PrivateStr.SJ = "1";
            }

            drs = dt.Select("RoleID='SupplyLeaveStore'");
            if (drs.Length <= 0) //代保管出库
            {
                PrivateStr.DBGCK = "0";
            }
            else
            {
                PrivateStr.DBGCK = "1";
            }

            drs = dt.Select("RoleID='BinQuery'");
            if (drs.Length <= 0) //货位查询
            {
                PrivateStr.HWCX = "0";
            }
            else
            {
                PrivateStr.HWCX = "1";
            }

            drs = dt.Select("RoleID='PatchQuery'");
            if (drs.Length <= 0) //批次查询
            {
                PrivateStr.PCCX = "0";
            }
            else
            {
                PrivateStr.PCCX = "1";
            }

            drs = dt.Select("RoleID='MaintainQuery'");
            if (drs.Length <= 0) //货位查询
            {
                PrivateStr.BYD = "0";
            }
            else
            {
                PrivateStr.BYD = "1";
            }

            drs = dt.Select("RoleID='105UntreadGoods'");
            if (drs.Length <= 0) //库存退货
            {
                PrivateStr.KCTH = "0";
            }
            else
            {
                PrivateStr.KCTH = "1";
            }

            drs = dt.Select("RoleID='103UntreadGoods'");
            if (drs.Length <= 0) //冻结库退货
            {
                PrivateStr.DJKTH = "0";
            }
            else
            {
                PrivateStr.DJKTH = "1";
            }
            */

            PrivateStr.ServerTime = System.DateTime.Now;

            return 0;
        }
        #endregion

        public bool getInv(out DataSet ds)
        {
            data.ado _ado = new data.ado();
            DataTable dt1;
            ds = null;

            if (_ado.ExecuteQuery("SELECT * from bx_E1DV31", out dt1) < 1)
            {
                return false;
            }

            dt1.TableName = "result";
            ds = new DataSet();
            ds.Tables.Add(dt1);

            return true;
        }

        public bool getBxUsers(out DataSet ds)
        {
            data.ado _ado = new data.ado();
            DataTable dt1;
            ds = null;

            if (_ado.ExecuteQuery("SELECT * from bx_E1DV30", out dt1) < 1)
            {
                return false;
            }

            dt1.TableName = "result";
            ds = new DataSet();
            ds.Tables.Add(dt1);

            return true;
        }

        public bool getPrint(out DataSet ds)
        {
            data.ado _ado = new data.ado();
            DataTable dt1;
            ds = null;

            if (_ado.ExecuteQuery("SELECT * from RF_Database_CZ.dbo.RF_M_Printer", out dt1) < 1)
            {
                return false;
            }

            dt1.TableName = "result";
            ds = new DataSet();
            ds.Tables.Add(dt1);

            return true;
        }

        public bool getWarehouse(out DataSet ds)
        {
            data.ado _ado = new data.ado();
            DataTable dt1;
            ds = null;

            if (_ado.ExecuteQuery("SELECT * from bx_Warehouse", out dt1) < 1)
            {
                return false;
            }

            dt1.TableName = "result";
            ds = new DataSet();
            ds.Tables.Add(dt1);

            return true;
        }

        public bool getShiperReceiver(out DataSet ds)
        {
            data.ado _ado = new data.ado();
            DataTable dt1;
            ds = null;

            if (_ado.ExecuteQuery("SELECT * from bx_ShiperReceiver", out dt1) < 1)
            {
                return false;
            }

            dt1.TableName = "result";
            ds = new DataSet();
            ds.Tables.Add(dt1);

            return true;
        }
    }

}
