using System;
using System.Management;
using System.Security.Cryptography;
using System.Text;

namespace rfidWebservice
{
    public class pub
    {
        public static decimal ChangeToDecimal(string strData)
        {
            decimal dData;

            try
            {
                if (strData.Contains("E"))
                {
                    dData = Convert.ToDecimal(decimal.Parse(strData.ToString(), System.Globalization.NumberStyles.Float));
                }
                else
                {
                    dData = Convert.ToDecimal(strData);
                }
            }
            catch
            {
                dData = 99999;
            }

            return Math.Round(dData, 4);
        }

        public static bool IsInteger(string value)
        {
            string pattern = @"^[0-9]*[1-9][0-9]*$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

            return regex.IsMatch(value);
        }

        public static bool isNumber(string strNumber)
        {
            if (strNumber.Length == 0)
            {
                return false;
            }

            string pattern = @"^[+-]?\d*[.]?\d*$";
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

            return regex.IsMatch(strNumber);
        }

        public static void WriteLog(string stype, string strLog)
        {
            try
            {
                string strFilePath = @"d:\log\" + System.DateTime.Now.Year.ToString() + @"\";

                if (!System.IO.Directory.Exists(strFilePath))
                {
                    System.IO.Directory.CreateDirectory(strFilePath);
                }

                strFilePath += "LOG" + Date() + ".log";
                System.IO.StreamWriter LogWriter = new System.IO.StreamWriter(strFilePath, true, Encoding.Default);
                string strToLog = string.Empty;
                strToLog += DateTime() + "  [" + stype + "] " + "\r\n" + strLog + " \r\n" + "----------------------------------------------------------" + "\r\n";
                LogWriter.Write(strToLog);
                LogWriter.Flush();
                LogWriter.Close();
                LogWriter = null;
            }
            catch
            {

            }
        }
        public static void WriteErrLog(string strLog)
        {
            try
            {
                string strFilePath = @"d:\log\" + System.DateTime.Now.Year.ToString() + @"\";

                if (!System.IO.Directory.Exists(strFilePath))
                {
                    System.IO.Directory.CreateDirectory(strFilePath);
                }

                strFilePath += "ERR" + Date() + ".log";
                System.IO.StreamWriter LogWriter = new System.IO.StreamWriter(strFilePath, true, Encoding.Default);
                string strToLog = string.Empty;
                strToLog += DateTime() + "  [错误] " + "\r\n" + strLog + " \r\n" + "----------------------------------------------------------" + "\r\n";
                LogWriter.Write(strToLog);
                LogWriter.Flush();
                LogWriter.Close();
                LogWriter = null;
            }
            catch
            {

            }
        }

        public static string Date()
        {
            return System.DateTime.Now.ToString("yyyy-MM-dd");
        }

        public static string Time()
        {
            return System.DateTime.Now.ToString("HH:mm:ss");
        }

        public static string DateTime()
        {
            return System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public static string GenerateMD5(string txt)
        {
            using (MD5 mi = MD5.Create())
            {
                byte[] buffer = Encoding.Default.GetBytes("!!" + txt + "xx");
                //开始加密
                byte[] newBuffer = mi.ComputeHash(buffer);
                StringBuilder sb = new StringBuilder();

                for (int i = 0; i < newBuffer.Length; i++)
                {
                    sb.Append(newBuffer[i].ToString("x2"));
                }

                return sb.ToString().ToUpper();
            }
        }

        public static string GetNumber()
        {
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher("Select * From Win32_Processor");
                string sNumber = "";

                foreach (ManagementObject mo in searcher.Get())
                {
                    sNumber = mo["ProcessorId"].ToString().Trim();
                    break;
                }

                return GenerateMD5(sNumber);
            }
            catch
            {
                return GenerateMD5("BFEBFBFF000906EA");
            }
        }

    }
}
