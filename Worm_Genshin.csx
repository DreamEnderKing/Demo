using System.Net.Http;
using System.Threading.Tasks;
using System.Text.Json;

HttpClient client = new HttpClient();
string fUrl = "https://hk4e-api.mihoyo.com/event/gacha_info/api/getGachaLog?win_mode=fullscreen&authkey_ver=1&sign_type=2&auth_appid=webview_gacha&init_type=301&gacha_id=b4ac24d133739b7b1d55173f30ccf980e0b73fc1&lang=zh-cn&device_type=mobile&game_version=CNRELiOS3.0.0_R10283122_S10446836_D10316937&plat_type=ios&game_biz=hk4e_cn&size=20&authkey=Jp9TjXSBIYcOz8pD0oQweNsI4YkVC7IBcH%2F8IuJMC6dY4SK5Ofbj%2BhVOJdEgxreMhtpU1Id8mMOsTskKaMv9DEERbdB4LMLWohIb5VAuvC63ypejFEEnP3Jq7xgh142b1hhjioOGpCIaUzSSAEgx0MS7J%2FYMUsWmHEkE2PdkY%2BItK7qK2xydiN16LWehuQFyZrdBO0d4O9w%2F35LWkF1LJ%2FtuOzm3Z3F1oYkis0D%2BUzVtexoRP9jcXUn9nl590TimcmMaflD1hwBHI7gieQ%2FPoF24f9UL08rpf3IojCAnOSV4rScbe6y%2BxFFJK67HIPbmsHLcWimAKxOPXHjsolAd4w%3D%3D&region=cn_gf01&timestamp=1664481732&gacha_type=301&page=1&end_id=0";
string[] _url = fUrl.Split('?');
string prefix = _url[0];
string[] fInfo = _url[1].Split('&');
Dictionary<string, string> fDic = new Dictionary<string, string>();
foreach (string f in fInfo)
{
    fDic.Add(f.Split('=')[0], f.Split('=')[1]);
}

for(int i = 1; i <= 100; i++)
{
    fDic["page"] = i.ToString();
    string oUrl = 
        prefix + "?" + 
        string.Join("&", fDic.Select(kv => kv.Key + "=" + kv.Value));

    string result = await client.GetStringAsync(oUrl);
    if(result.Contains("frequently"))
        continue;
    /*
    JsonDocument doc = JsonDocument.Parse(result);
    JsonElement root = doc.RootElement;
    JsonElement data = root.GetProperty("data");
    JsonElement items = data.GetProperty("list");
    foreach(var item in items.EnumerateArray())
    {
        string name = item.GetProperty("name").GetRawText();
        string rank = item.GetProperty("rank_type").GetRawText();
        string time = item.GetProperty("time").GetRawText();
        Console.WriteLine($"{name}, {rank}, {time}");
    }
    */
    

    Console.WriteLine(result);
    Console.WriteLine(i);
}
