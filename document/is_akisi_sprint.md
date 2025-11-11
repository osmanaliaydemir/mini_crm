# İş Akışı ve Sprint Planı

## İş Akışı
1. Gereksinim netleştirme oturumu (stakeholder görüşmesi, kapsam doğrulama)
2. Alan modeli ve veri tabanı taslağının çıkarılması
3. Proje altyapısının hazırlanması (solution, projeler, CI/CD)
4. Yetkilendirme ve dil desteği altyapısının kurulması
5. Tedarikçi, ürün ve sevkiyat süreçlerinin geliştirilmesi
6. Depo operasyonları ve stok hareketlerinin entegrasyonu
7. CRM müşteri yönetimi ve saha notlarının kurgulanması
8. Ödeme planı, kasa hareketleri ve raporlama fonksiyonlarının tamamlanması
9. Testler, kullanıcı kabulü ve üretime hazırlık

## Kararlaştırılan Sevkiyat Detayları
- Sevkiyat statüleri: Sipariş Verildi → Üretim Başladı → Yola Çıktı → Vagonda → Gemide → Limanda → Gümrükte → Tırda → Depoda → Bayiye Teslim Edildi
- Tüm statü geçişleri manuel tetiklenecek.
- Her statü için başlangıç ve bitiş tarihleri tutulacak, liste ekranında yalnızca son statü gösterilecek.
- Sevkiyat kayıtları hem tedarikçi hem müşteri ile ilişkilendirilebilir; ilişki opsiyoneldir.
- Hacim bilgileri ShipmentItem üzerinden formda manuel girilecek.
- CustomsProcess için temel statü ve belge numarası alanları yeterli.
- Depo ekranı oluşturulana kadar `Depoda` statüsündeki sevkiyatlar stok olarak raporlanacak.
- Statü değişikliklerinde e-posta bildirimi backlog’a eklendi (ilerleyen sprintte uygulanacak).

## Sprint Planı

### Sprint 0 - Hazırlık
- [x] Solution iskeleti ve katmanların oluşturulması
- [x] EF Core ve MSSQL bağlantısının yapılandırılması
- [x] Identity + JWT altyapısının başlatılması
- [x] Localization altyapısı ve dil kaynaklarının iskeleti
- [x] Temel Razor Pages layout ve UI bileşen seti

### Sprint 1 - Lojistik Temel Modüller
- [x] Tedarikçi yönetimi CRUD ekranları
- [x] Kereste varyantı ve ürün kataloğu
- [x] Sevkiyat oluşturma ve taşıma (vagon/gemi/konteyner) kayıtları
- [x] Gümrük süreci girişleri ve durum takibi
- [x] Depo listesi ve tır boşaltma operasyonlarının prototipi
- [x] Domain ve integration testlerin ilk seti

### Sprint 2 - Sevkiyat Operasyonları
- [ ] Sevkiyat durum akışı (`Sipariş Verildi` → `Bayiye Teslim Edildi`) enum ve domain genişletmesi
- [ ] ShipmentStage tablosu ile her adım için başlangıç/bitiş tarihleri
- [ ] Sevkiyat liste ekranı (DataTable, müşteri/tedarikçi/statü filtreleri)
- [ ] Sevkiyat detay/timeline ekranı ve manuel durum güncelleme akışı
- [ ] Create/Edit formları (manuel hacim girişi, tedarikçi/müşteri opsiyonel bağlantı)
- [ ] Depoda durumundaki sevkiyatlardan stok özetinin türetilmesi
- [ ] CustomsProcess temel alanlarının (statü, belge numarası) UI entegrasyonu
- [ ] E-posta bildirim altyapısının backlog’a eklenmesi ve tasarımı

### Sprint 3 - CRM ve Finans
- Müşteri kartı domain, API ve Razor CRUD
- Saha etkileşim kayıtları ve raporlaması
- Ödeme planı (peşin/taksit) domain ve servisleri
- Kasa hareket ekranı ve API
- Excel/PDF rapor exportları
- UI'nin üç dilde tamamlanması ve validasyon mesajları
- CRM ve finans modülleri için testler

### Sprint 4 - Sertleştirme ve Yayına Hazırlık
- Performans iyileştirmeleri, güvenlik gözden geçirme
- Eksik test senaryolarının tamamlanması
- Son kullanıcı geri bildirimlerinin işlenmesi
- Yayın dökümantasyonu ve operasyon runbook'u
- Canlıya geçiş planının netleştirilmesi
