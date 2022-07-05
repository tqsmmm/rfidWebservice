using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace rfidWebservice
{

	/// <summary> 
	/// DESEncryptor 的摘要说明。 
	/// </summary> 
	public class RFdesOperator
	{
		#region 私有成员

		/// <summary> 
		/// 输入字符串 
		/// </summary> 
		private string inputString = null;
		/// <summary> 
		/// 输出字符串 
		/// </summary> 
		private string outString = null;
		/// <summary> 
		/// 加密密钥 
		/// </summary> 
		private string encryptKey = null;
		/// <summary> 
		/// 解密密钥 
		/// </summary> 
		private string decryptKey = "12345678";
		/// <summary> 
		/// 提示信息 
		/// </summary> 
		private string noteMessage = null;

		#endregion

		#region 公共属性

		/// <summary> 
		/// 加密密钥 
		/// </summary> 
		public string EncryptKey
		{
			get { return encryptKey; }
			set { encryptKey = value; }
		}

		/// <summary> 
		/// 解密密钥 
		/// </summary> 
		public string DecryptKey
		{
			get { return decryptKey; }
			set { decryptKey = value; }
		}

		/// <summary> 
		/// 输入字符串 
		/// </summary> 
		public string InputString
		{
			get { return inputString; }
			set { inputString = value; }
		}

		/// <summary> 
		/// 输出字符串 
		/// </summary> 
		public string OutString
		{
			get { return outString; }
			set { outString = value; }
		}

		/// <summary> 
		/// 错误信息 
		/// </summary> 
		public string NoteMessage
		{
			get { return noteMessage; }
			set { noteMessage = value; }
		}

		#endregion

		#region 构造函数

		public RFdesOperator()
		{
			// 
			// TODO: 在此处添加构造函数逻辑 
			// 
		}

		#endregion

		#region DES加密字符串

		/// <summary> 
		/// 加密字符串 
		/// 注意:密钥必须为8位 
		/// </summary> 
		/// <param name="strText">字符串</param> 
		/// <param name="encryptKey">密钥</param> 
		public void DesEncrypt()
		{
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };

			try
			{
                byte[] byKey = System.Text.Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
				byte[] inputByteArray = Encoding.UTF8.GetBytes(inputString);
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
				cs.Write(inputByteArray, 0, inputByteArray.Length);
				cs.FlushFinalBlock();
				outString = Convert.ToBase64String(ms.ToArray());
			}
			catch (Exception error)
			{
				noteMessage = error.Message;
			}
		}

		#endregion

		#region DES解密字符串

		/// <summary> 
		/// 解密字符串 
		/// </summary> 
		/// <param name="this.inputString">被加密的字符串</param> 
		/// <param name="decryptKey">密钥</param>
		public void DesDecrypt()
		{
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            byte[] inputByteArray = new byte[inputString.Length];

			try
			{
                byte[] byKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
				inputByteArray = Convert.FromBase64String(inputString);
				MemoryStream ms = new MemoryStream();
				CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
				cs.Write(inputByteArray, 0, inputByteArray.Length);
				cs.FlushFinalBlock();
                Encoding encoding = new UTF8Encoding();
				outString = encoding.GetString(ms.ToArray());
			}
			catch (Exception error)
			{
				noteMessage = error.Message;
			}
		}

		#endregion

		#region MD5

		/// <summary> 
		/// MD5加密
		/// </summary> 
		/// <param name="strText">需要加密的信息</param> 
		/// <returns>MD5加密的密文</returns> 
		public void MD5Encrypt()
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] result = md5.ComputeHash(Encoding.UTF8.GetBytes(inputString));
			outString = Encoding.UTF8.GetString(result);
		}

		public static string getMd5Hash(string input)
		{
			// Create a new instance of the MD5CryptoServiceProvider object.
			MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();

			// Convert the input string to a byte array and compute the hash.
			byte[] data = md5Hasher.ComputeHash(Encoding.Default.GetBytes(input));

			// Create a new Stringbuilder to collect the bytes
			// and create a string.
			StringBuilder sBuilder = new StringBuilder();

			// Loop through each byte of the hashed data 
			// and format each one as a hexadecimal string.
			for (int i = 0; i < data.Length; i++)
			{
				sBuilder.Append(data[i].ToString("x2"));
			}

			// Return the hexadecimal string.
			return sBuilder.ToString();
		}


		#endregion
	}
}