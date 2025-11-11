# CRM Portal (Mini CRM)

Modern Razor Pages tabanlı kereste operasyon yönetim portalıdır. Kullanıcı kimlik doğrulaması, rol bazlı yetkilendirme ve üç dilli UI (TR/EN/AR) desteği sunar. Finans, sevkiyat, tedarikçi, müşteri ve depo modülleri için raporlama ekranları içerir.

## Özellikler

- **Kimlik doğrulama**: Cookie tabanlı auth, Admin/Personel rolleri
- **Sevkiyat yönetimi**: Adım adım statüler, grafikler ve detay ekranları
- **Finansal özet**: Ödeme planları, kasa hareketleri ve dashboad grafikleri
- **DataTables entegrasyonu**: Liste sayfalarında arama, sıralama, sayfalama
- **Yerelleştirme**: JSON kaynakları ile Türkçe/İngilizce/Arapça
- **Dummy data scriptleri**: `document/` klasöründe SQL seed dosyaları
- **Dashboard**: Sevkiyat, depo, finans ve CRM göstergeleri Chart.js ile görselleştirildi
- **Migration senaryosu**: EF Core SQL Server 2014 uyumlu migration

## Gereksinimler

- .NET 9 SDK
- SQL Server 2014 veya üstü (geliştirme için LocalDB yeterlidir)
- Node.js (yalnızca client paketleri yönetilecekse)

## Hızlı Başlangıç

```bash
# bağımlılıkları yükle
dotnet restore

# EF Core migration çalıştır
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api

# dummy verileri yüklemek istersen
sqlcmd -S .\SQLEXPRESS -d CRMDb -i document/shipments_fullflow_seed.sql

# test et
dotnet test

# uygulamayı başlat (API + Razor)
dotnet run --project src/CRM.Api    # API
dotnet run --project src/CRM.Web    # Razor Portal
```

## Proje Yapısı

```
src/
├── CRM.Api               # API & Auth giriş noktası
├── CRM.Application       # CQRS ve servisler
├── CRM.Domain            # Entities, value objects
├── CRM.Infrastructure    # DbContext, migrations, repos
└── CRM.Web               # Razor Pages portali

tests/
├── CRM.UnitTests
└── CRM.IntegrationTests

document/
├── shipments_fullflow_seed.sql
└── ...
```

## Komutlar

- `dotnet build` : Derleme
- `dotnet test` : Unit ve Integration testleri
- `dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Api` : Migration uygula

## Katkı & Notlar

- Migration dosyaları SQL Server 2014 ile uyumlu olacak şekilde elden yazılmıştır.
- Dashboard performansı için EF sorguları dikkatlice düzenlenmiştir; yeni sorgular eklerken aynı DbContext üzerinde paralel `Task` kullanımından kaçının.
- Üç dilde yeni metin eklerken `src/CRM.Web/Resources` altındaki tüm JSON dosyalarını güncelleyin.

## Lisans

Bu proje MIT lisansı ile yayınlanmıştır.


