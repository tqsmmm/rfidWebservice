namespace rfidWebservice
{
    /// <summary>
    /// privilidge 的摘要说明。
    /// </summary>
    public struct privilidge
	{
		public string PrintBin;//打印货位
		public string PrintLabel;//打印物料标签
		public string CGSH;//采购收货
		public string CGRK;//采购入库
		public string DGRK;//代管入库
		public string YCRK;//异常入库
		public string PANDIAN;//盘点
		public string YK; //移库
		public string SCLL;//生产领料
		public string BF; //报废
		public string TH; //退货
		public string CX;// 仓库信息查询
		public string DGCK;//代管出库
		public string SJ;//上架
		public string DBGCK;//代保管出库
		public string BYD;//查看保养单
		public string HWCX;//货位查询
		public string PCCX;//批次查询
		public string KCTH;//库存退货
		public string DJKTH;//冻结库退货

		/// <summary>
		/// 是否是管理员
		/// </summary>
		public bool isAdmin;

		//		/// <summary>
		//		/// 用户是否有效
		//		/// </summary>
		//		public bool isInEffect;

		/// <summary>
		/// 服务器当前时间，用于同步扫描枪上的时间 myl 2008-09-04
		/// </summary>
		public System.DateTime ServerTime;
	}
}