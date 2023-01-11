using System;
using System.Net.Http;
using System.IO;

HttpClient client = new HttpClient();
const int TotalPage = 609;
const int sectionIndex = 1;
const string bookIndex = "5//00004617/00004617000";
const string queryString = "201216000956";
client.DefaultRequestHeaders.Add("Accept", "image/webp,image/apng,image/svg+xml,image/*,*/*;q=0.8");
client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
client.DefaultRequestHeaders.Add("Connection", "keep-alive");
client.DefaultRequestHeaders.Add("Cookies", "");
client.DefaultRequestHeaders.Add("Host", "reserves.lib.tsinghua.edu.cn");
client.DefaultRequestHeaders.Add("Referer", $"http://reserves.lib.tsinghua.edu.cn/book{bookIndex}/mobile/index.html");
client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Linux; Android 6.0; Nexus 5 Build/MRA58N) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/108.0.0.0 Mobile Safari/537.36 Edg/108.0.1462.54");
client.Timeout = new TimeSpan(0, 0, 30);

for(int i = 1; i <= TotalPage; i++)
    DownloadPage(i);

// 生成对应LaTeX文件
using(FileStream stream = new FileStream("cache\\publish.tex", FileMode.OpenOrCreate))
using(StreamWriter writer = new StreamWriter(stream))
{
    writer.WriteLine(@"\documentclass{article}
\usepackage{amsmath}
\usepackage{amssymb}
\usepackage{geometry}
\usepackage{graphicx}
\geometry{margin=0pt, headheight=0pt}
\geometry{left=0pt, right=0pt, top=0pt, bottom=0pt}
\begin{document}
");
    for(int i = 1; i <= TotalPage; i++)
        writer.WriteLine($"\\newpage\n\\includegraphics[width=550pt]{{dir{sectionIndex}/{i}.jpg}}");
    writer.WriteLine("\\end{document}");
}

void DownloadPage(int j)
{
    try
    {
        Directory.CreateDirectory($"cache\\dir{sectionIndex}");
        if(File.Exists($"cache\\dir{sectionIndex}\\{j}.jpg"))
        {
            if(IsCompletedImage($"cache\\dir{sectionIndex}\\{j}.jpg"))
                return;
            else
                File.Delete($"cache\\dir{sectionIndex}\\{j}.jpg");
        }
        Console.WriteLine($"Page {j} downloading...");
        var message = client.GetAsync($"http://reserves.lib.tsinghua.edu.cn/book{bookIndex}/files/mobile/{j}.jpg?{queryString}").Result;
        FileStream stream = new FileStream($"cache\\dir{sectionIndex}\\{j}.jpg", FileMode.CreateNew);
        message.Content.ReadAsStream().CopyTo(stream);
        stream.Flush();
        stream.Dispose();
        Console.WriteLine($"Page {j} complete.");
    }
    catch(Exception e)
    {
        using(FileStream error = new FileStream("error.log", FileMode.Append))
        using(StreamWriter writer = new StreamWriter(error))
        {
            writer.WriteLine($"[page {j}]{e.Message}");
        }
    }
}

bool IsCompletedImage(string strFileName)
{
    try
    {
        FileStream fs = new FileStream(strFileName, FileMode.Open);
        BinaryReader reader = new BinaryReader(fs);
        try
        {
            byte[] szBuffer = reader.ReadBytes((int)fs.Length);
            //jpg png图是根据最前面和最后面特殊字节确定. bmp根据文件长度确定
            //png检查
            if (szBuffer[0] == 137 && szBuffer[1] == 80 && szBuffer[2] == 78 && szBuffer[3] == 71 && szBuffer[4] == 13
                && szBuffer[5] == 10 && szBuffer[6] == 26 && szBuffer[7] == 10)
            {
                //&& szBuffer[szBuffer.Length - 8] == 73 && szBuffer[szBuffer.Length - 7] == 69 && szBuffer[szBuffer.Length - 6] == 78
                if (szBuffer[szBuffer.Length - 5] == 68 && szBuffer[szBuffer.Length - 4] == 174 && szBuffer[szBuffer.Length - 3] == 66
                    && szBuffer[szBuffer.Length - 2] == 96 && szBuffer[szBuffer.Length - 1] == 130)
                    return true;
                //有些情况最后多了些没用的字节
                for (int i = szBuffer.Length - 1; i > szBuffer.Length / 2; --i)
                {
                    if (szBuffer[i - 5] == 68 && szBuffer[i - 4] == 174 && szBuffer[i - 3] == 66
                     && szBuffer[i - 2] == 96 && szBuffer[i - 1] == 130)
                        return true;
                }
 
 
            }
            else if (szBuffer[0] == 66 && szBuffer[1] == 77)//bmp
            {
                //bmp长度
                //整数转成字符串拼接
                string str = Convert.ToString(szBuffer[5], 16) + Convert.ToString(szBuffer[4], 16)
                    + Convert.ToString(szBuffer[3], 16) + Convert.ToString(szBuffer[2], 16);
                int iLength = Convert.ToInt32("0x" + str, 16); //16进制数转成整数
                if (iLength <= szBuffer.Length) //有些图比实际要长
                    return true;
            }
            else if (szBuffer[0] == 71 && szBuffer[1] == 73 && szBuffer[2] == 70 && szBuffer[3] == 56)//gif
            {
                //标准gif 检查00 3B
                if (szBuffer[szBuffer.Length - 2] == 0 && szBuffer[szBuffer.Length - 1] == 59)
                    return true;
                //检查含00 3B
                for (int i = szBuffer.Length - 1; i > szBuffer.Length / 2; --i)
                {
                    if (szBuffer[i] != 0)
                    {
                        if (szBuffer[i] == 59 && szBuffer[i - 1] == 0)
                            return true;
                    }
                }
            }
            else if (szBuffer[0] == 255 && szBuffer[1] == 216) //jpg
            {
                //标准jpeg最后出现ff d9
                if (szBuffer[szBuffer.Length - 2] == 255 && szBuffer[szBuffer.Length - 1] == 217)
                    return true;
                else
                {
                    //有好多jpg最后被人为补了些字符也能打得开, 算作完整jpg, ffd9出现在近末端
                    //jpeg开始几个是特殊字节, 所以最后大于10就行了 从最后字符遍历
                    //有些文件会出现两个ffd9 后半部分ffd9才行
                    for (int i = szBuffer.Length - 2; i > szBuffer.Length / 2; --i)
                    {
                        //检查有没有ffd9连在一起的
                        if (szBuffer[i] == 255 && szBuffer[i + 1] == 217)
                            return true;
                    }
                }
            }
        }
        catch
        {
        }
        finally
        {
            if (fs != null)
                fs.Close();
            if (reader != null)
                reader.Close();
        }
    }
    catch
    {
        return false;
    }
    return false;
}