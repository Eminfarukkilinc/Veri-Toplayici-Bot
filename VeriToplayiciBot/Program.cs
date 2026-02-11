using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace VeriToplayiciBot
{
    internal class Program
    {
        //github tokeni
        private static string githubToken = "github token here";

        //Hedef dosya sayısı
        private static int hedefDosyaSayisi = 5000;

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== GitHub Veri Toplayıcı Bot ===");

            var client = new GitHubClient(new ProductHeaderValue("OdevBotu"));
            var tokenAuth = new Credentials(githubToken);
            client.Credentials = tokenAuth;

            string csvYolu = "human_data.csv";

            //TEKRAR ENGELLEYİCİ SİSTEM
            //Önce mevcut dosyayı okuyup hafızaya alıyoruz ki aynılarını indirmeyelim.
            HashSet<string> kaydedilenKodlar = new HashSet<string>();

            if (File.Exists(csvYolu))
            {
                Console.WriteLine("Mevcut dosya taranıyor, tekrarları önlemek için hafızaya alınıyor...");
                var satirlar = File.ReadAllLines(csvYolu);
                foreach (var satir in satirlar)
                {
                    //Satırı '|' işaretinden bölüp kod kısmını al
                    var parcalar = satir.Split('|');
                    if (parcalar.Length > 0)
                    {
                        kaydedilenKodlar.Add(parcalar[0]);
                    }
                }
            }
            else
            {
                File.AppendAllText(csvYolu, "Kod_Icerigi|Etiket\n", Encoding.UTF8);
            }

            //Başlık satırını sayma
            int toplananSayisi = Math.Max(0, kaydedilenKodlar.Count - 1); //Başlık varsa düş
            Console.WriteLine($"Mevcut Benzersiz Veri Sayısı: {toplananSayisi}");

            
            string[] aramaTerimleri = {
                "class", "void", "public", "private", "return",
                "string", "int", "namespace", "using System", "List<",
                "await", "var", "new", "if", "else", "foreach", "try", "catch",
                "Task", "async", "enum", "interface", "readonly", "static"
            };

            using (HttpClient httpClient = new HttpClient())
            {
                
                foreach (var terim in aramaTerimleri)
                {
                    if (toplananSayisi >= hedefDosyaSayisi) break;

                    Console.WriteLine($"\n>>> Yeni Arama Terimine Geçildi: '{terim}' <<<");
                    int sayfa = 1;

                    
                    while (sayfa <= 15 && toplananSayisi < hedefDosyaSayisi)
                    {
                        try
                        {
                            
                            Console.WriteLine($"   > '{terim}' için Sayfa {sayfa} taranıyor... (Kopyalar atlanacak)");

                            var searchRequest = new SearchCodeRequest(terim)
                            {
                                Language = Language.CSharp,
                                Size = Octokit.Range.GreaterThan(500),
                                Page = sayfa,
                                PerPage = 50
                            };

                            SearchCodeResult result = null;
                            try
                            {
                                result = await client.Search.SearchCode(searchRequest);
                            }
                            catch (RateLimitExceededException)
                            {
                                Console.WriteLine("!!! Hız Sınırı! 65 Saniye Bekleniyor... !!!");
                                Thread.Sleep(65000);
                                continue;
                            }
                            catch
                            {
                                Console.WriteLine($"'{terim}' için limit doldu (1000 sonuç), diğer kelimeye geçiliyor.");
                                break; //sonraki terime geç
                            }

                            if (result.Items.Count == 0) break;

                            foreach (var item in result.Items)
                            {
                                if (toplananSayisi >= hedefDosyaSayisi) break;

                                string rawUrl = item.HtmlUrl.Replace("github.com", "raw.githubusercontent.com").Replace("/blob/", "/");

                                try
                                {
                                    string kodIcerigi = await httpClient.GetStringAsync(rawUrl);

                                    //Temizlik
                                    string temizKod = kodIcerigi.Replace("|", " ");
                                    temizKod = temizKod.Replace("\n", " <NEWLINE> ").Replace("\r", "");


                                    //Eğer bu kod hafızada varsa kaydetme
                                    if (kaydedilenKodlar.Contains(temizKod))
                                    {
                                        continue;
                                    }

                                    //Yeni kodsa kaydet
                                    string csvSatir = $"{temizKod}|0\n";
                                    File.AppendAllText(csvYolu, csvSatir, Encoding.UTF8);

                                    kaydedilenKodlar.Add(temizKod); //Hafızaya ekle
                                    toplananSayisi++;

                                    Console.Write($"\rToplanan (Benzersiz): {toplananSayisi}/{hedefDosyaSayisi} - {item.Name}          ");
                                }
                                catch { }

                                Thread.Sleep(250);
                            }
                            sayfa++;
                            Thread.Sleep(2000);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Hata: {ex.Message}");
                            break;
                        }
                    }
                }
            }

            Console.WriteLine($"\n\n=== İŞLEM TAMAM! {toplananSayisi} adet kod toplandı. ===");
            Console.ReadLine();
        }
    }
}