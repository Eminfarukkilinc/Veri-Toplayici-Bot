# Veri-Toplayici-Bot

Bu proje, GitHub üzerindeki herkese açık repolardan rastgele kod parçacıkları indirerek yapılandırılmış bir veri seti oluşturmak amacıyla tasarlanmıştır. Bu bot, yapay zeka tespiti projemizin "insan yapımı kod" (human-written code) veri sağlama aşamasını oluşturur.

## ⚙️ Gereksinimler

Program.cs içerisinde bulunan token metnine GitHub Token gereklidir.

Projeyi çalıştırmadan önce geçerli bir GitHub Token almalı ve ilgili yapılandırma alanına/çevre değişkenine (environment variable) eklemelisiniz.

## 🚀 Kullanım

1. Repoyu bilgisayarınıza klonlayın.
2. Gerekli bağımlılıkları yükleyin.
3. GitHub Token bilginizi sisteme tanımlayın.
4. Botu çalıştırdığınızda, toplanan kodlar `human_data.csv` içerisine kaydedilecektir. Bu çıktı dosyası, sistemin bir sonraki aşaması olan Ai-Veri-Uretici botu için girdi olarak kullanılacaktır.
