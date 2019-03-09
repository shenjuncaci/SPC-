using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class HttpRequest
    {
        public static string SendGet(string strURL)
        {
            //string strURL = "http://localhost/WinformSubmit.php?tel=11111&name=张三";
            System.Net.HttpWebRequest request;
            // 创建一个HTTP请求
            request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
            //request.Method="get";
            System.Net.HttpWebResponse response;
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string responseText = myreader.ReadToEnd();
            myreader.Close();
            return responseText;
        }

        public static string SendPost(string strURL,List<PostParam> postParams)
        {
            //string strURL = "http://localhost/WinformSubmit.php";
            System.Net.HttpWebRequest request;
            request = (System.Net.HttpWebRequest)WebRequest.Create(strURL);
            //Post请求方式
            request.Method = "POST";
            // 内容类型
            request.ContentType = "application/x-www-form-urlencoded";
            // 添加参数
            StringBuilder builder = new StringBuilder();
            if (postParams.Count>0)
            {
                for(int i=0;i<postParams.Count;i++)
                {
                    if (i > 0)
                    {
                        builder.Append("&");
                    }
                    builder.AppendFormat("{0}={1}", postParams[i].Key, postParams[i].Values);
                }
                
            }
            byte[] payload;
            //将URL编码后的字符串转化为字节
            payload = System.Text.Encoding.UTF8.GetBytes(builder.ToString());
            //设置请求的 ContentLength 
            request.ContentLength = payload.Length;
            //获得请 求流
            System.IO.Stream writer = request.GetRequestStream();
            //将请求参数写入流
            writer.Write(payload, 0, payload.Length);
            // 关闭请求流
            writer.Close();
            System.Net.HttpWebResponse response;
            // 获得响应流
            response = (System.Net.HttpWebResponse)request.GetResponse();
            System.IO.StreamReader myreader = new System.IO.StreamReader(response.GetResponseStream(), Encoding.UTF8);
            string responseText = myreader.ReadToEnd();
            myreader.Close();
            return responseText;
        }
    }
}
