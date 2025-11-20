# CRM Portal - Sunucu Kurulum Rehberi

## HTTP Error 500.30 Çözümü

Bu hata, ASP.NET Core uygulamasının başlatılamadığını gösterir. Aşağıdaki adımları takip edin:

## 1. Yapılandırma Dosyaları

### appsettings.Production.json

Sunucuda `appsettings.Production.json` dosyasını oluşturun veya düzenleyin:

```json
{
  "ConnectionStrings": {
    "Default": "Server=YOUR_SERVER;Database=CRMDb;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;Encrypt=True;"
  },
  "Jwt": {
    "SigningKey": "GÜVENLİ_BİR_ANAHTAR_BURAYA"
  }
}
```

**ÖNEMLİ:** 
- `SigningKey` değerini production ortamında mutlaka değiştirin
- Connection string'i sunucu bilgilerinize göre güncelleyin
- Environment variable kullanarak hassas bilgileri saklayabilirsiniz

### Environment Variables (Önerilen)

Hassas bilgileri environment variable olarak ayarlayın:

```bash
ConnectionStrings__Default="Server=..."
Jwt__SigningKey="..."
```

## 2. Veritabanı Bağlantısı

- SQL Server'ın çalıştığından emin olun
- Veritabanı kullanıcısının gerekli izinlere sahip olduğundan emin olun
- Firewall kurallarının SQL Server portunu (1433) açtığından emin olun

## 3. Log Klasörü İzinleri

Uygulama `logs` klasörüne yazmaya çalışır. IIS veya uygulama pool kullanıcısına yazma izni verin:

```powershell
# IIS App Pool kullanıcısı için
icacls "C:\Path\To\Your\App\logs" /grant "IIS AppPool\YourAppPoolName:(OI)(CI)F"
```

## 4. .NET Runtime

Sunucuda doğru .NET runtime'ın yüklü olduğundan emin olun:

```bash
dotnet --list-runtimes
```

.NET 9.0 runtime'ı yüklü olmalı.

## 5. Hata Ayıklama

### Log Dosyalarını Kontrol Edin

`logs` klasöründeki log dosyalarını kontrol edin:

```
logs/crm-YYYYMMDD.log
```

### Event Viewer

Windows Event Viewer'da Application log'larını kontrol edin.

### stdout Logging (IIS)

IIS'de stdout logging'i etkinleştirin:

1. `web.config` dosyasına ekleyin:
```xml
<aspNetCore processPath="dotnet" 
            arguments=".\CRM.Web.dll" 
            stdoutLogEnabled="true" 
            stdoutLogFile=".\logs\stdout" />
```

## 6. Yaygın Hatalar ve Çözümleri

### Veritabanı Bağlantı Hatası

**Hata:** `Cannot open database "CRMDb"`

**Çözüm:**
- Connection string'i kontrol edin
- SQL Server'ın çalıştığından emin olun
- Kullanıcı adı/şifre doğru mu kontrol edin

### Serilog Dosya Yazma Hatası

**Hata:** `Access to the path 'logs' is denied`

**Çözüm:**
- `logs` klasörüne yazma izni verin
- IIS App Pool kullanıcısına izin verin

### Migration Hatası

**Hata:** `Migration failed`

**Çözüm:**
- Veritabanı kullanıcısının `db_owner` veya migration yapma iznine sahip olduğundan emin olun
- Manuel olarak migration çalıştırabilirsiniz:
```bash
dotnet ef database update --project src/CRM.Infrastructure --startup-project src/CRM.Web
```

## 7. Production Checklist

- [ ] `appsettings.Production.json` oluşturuldu ve yapılandırıldı
- [ ] Connection string doğru ve test edildi
- [ ] JWT SigningKey değiştirildi
- [ ] Log klasörü izinleri ayarlandı
- [ ] .NET 9.0 runtime yüklü
- [ ] SQL Server erişilebilir
- [ ] Firewall kuralları yapılandırıldı
- [ ] HTTPS sertifikası yapılandırıldı (production için)

## 8. Hızlı Test

Uygulama başladıktan sonra şu endpoint'i test edin:

```
https://your-domain/Auth/Login
```

Eğer sayfa açılıyorsa, uygulama çalışıyor demektir.

## Destek

Hata devam ederse:
1. Log dosyalarını kontrol edin
2. Event Viewer'da hata mesajlarını arayın
3. `stdout` log dosyalarını inceleyin
4. Development ortamında çalıştırarak hatayı tekrarlayın

