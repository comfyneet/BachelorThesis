using System;
using System.Collections.Generic;
using RiceDoctor.DatabaseManager;
using RiceDoctor.RetrievalAnalysis;
using RiceDoctor.Shared;

namespace RiceDoctor.ConsoleApp
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var logger = new ConsoleLogger();
            Logger.OnLog += logger.Log;

            var mockWebsite = new Website
            {
                Name = "vinhphuc.gov.vn",
                Url = "http://nnptnt.vinhphuc.gov.vn/Pages/home.aspx",
                Categories = new List<Category>
                {
                    new Category
                    {
                        Url =
                            "http://nnptnt.vinhphuc.gov.vn/csdlnongnghiep/pages/lua.aspx?date=&date1=&chkNgayDang=False&chkNgayHa=False&EventID=0&Page=1",
                        ArticleXPath = @"//a[@class='more' and starts-with(@href, 'lua.aspx')]/@href",
                        TitleXPath = @"//h1[@class='title']",
                        ContentXPath = @"//div[contains(@class, 'fck_detail')]"
                    }
                }
            };
            var articles = WebCrawler.Crawl(mockWebsite);

            // hack
            var mockArticles = new List<Article>
            {
                new Article
                {
                    Id = 1,
                    Title = "qua trinh thu phan, thu tinh va hinh thanh hat lua",
                    Content =
                        @"lua la loai cay tu thu phan. sau khi bong lua tro mot ngay thi bat dau qua trinh thu phan. vo trau vua he mo tu 0-4 phut thi bao phan vo ra, hat phan roi vao dau nhuy  va hop nhat voi noan o ben trong bau nhuy de bau nhuy phat trien thanh hat.
thoi gian thu phan ke tu khi vo trau mo ra den khi khep lai keo dai khoang 50-60 phut. thoi gian thu tinh keo dai 8 gio sau thu phan.
trong ngay thoi gian hoa lua no ro thuong vao 8-9 gio sang khi co dieu kien nhiet do thich hop, du anh sang, quang may, gio nhe. nhung ngay mua he, troi nang to co the no hoa som vao 7 -  8 go sang. nguoc lai neu troi am u, thieu anh sang hoac gap ret hoa phoi mau muon hon, vao 12 - 14 gio.
sau thu tinh phoi nhu phat trien nhanh de thanh hat. khoi luong hat gao tang nhanh trong vong 15- 20 ngay sau tro, dong thoi voi qua trinh van chuyen va tich luy vat chat, hat lua vao chac va chin dan.

qua trinh thu phan, thu tinh va hinh thanh hat lua"
                },
                new Article
                {
                    Id = 2,
                    Title = "giai doan lam hat",
                    Content =
                        @"giai doan chin mot luong lon cac chat tinh bot va duong tich luy trong than, be la duoc van chuyen vao hat, hat lua lon dan ve kich thuoc, khoi luong, vo hat doi mau, gia va chin. la lua cung hoa gia bat dau tu nhung la thap len tren theo giai doan phat trien cua cay lua cung voi qua trinh chin cua hat.
giai doan chin sua
sau phoi mau 5 - 7 ngay, chat du tru trong hat o dang long, trang nhu sua. hinh dang hat da hoan thanh, lung hat co mau xanh. khoi luong hat tang nhanh o thoi ky nay, co the dat 75 - 80 % khoi luong cuoi cung.
giai doan chin sap
giai doan nay chat dich trong hat dan dan dac lai, hat cung. mau xanh o lung hat dan dan chuyen sang mau vang. khoi luong hat tiep tuc tang len.
trong pha khoi dau cua su chac hat, ham luong nuoc cua hat khoang 58% va giam xuong con khoang 20 %. khi nhiet do tang, ham luong nuoc giam nhanh hon.
giai doan chin hoan toan
giai doan nay hat chac cung. vo trau mau vang - vang nhat. khoi luong hat dat toi da."
                }
            };

            var analyzer = new RetrievalAnalyzer();
            var weights = analyzer.AnalyzeArticles(mockArticles);

            Console.ReadKey();
        }
    }
}