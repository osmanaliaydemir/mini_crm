# Security Roadmap & Sprint Plan

## Sprint 1 – Güvenlik Temeli
- Threat modeling: Kilit kullanıcı akışlarını OWASP ASVS seviyelerine göre sınıflandır, varlık/yetki matrisini güncelle.
- Konfigürasyon sertleştirme: `appsettings` gizlilerini Secret Manager/Azure Key Vault’a taşı, `AllowedHosts` ve CORS politikalarını kısıtla, `TrustServerCertificate` kullanımını kaldır.
- Parola politikası: Identity parola ve lockout ayarlarını OWASP yönergelerine göre güncelle, yönetici onaylı hesap kilit açma süreci tasarla.
- Güvenlik başlıkları: API ve Razor uygulamasına `Content-Security-Policy`, `X-Frame-Options`, `Referrer-Policy`, `Strict-Transport-Security` ekle; statik içeriklerde MIME türü doğrulaması yap.

## Sprint 2 – Kimlik Doğrulama & Yetkilendirme
- JWT güçlendirme: Refresh token’ları hash’li sakla, cihaz/oturum bağlayıcılığı ekle, token revoke loglarını merkezi depoda tut.
- Rol ve politika gözden geçirme: Rol bazlı yetkileri minimum ayrıcalık ilkesine göre güncelle, politika tabanlı testler yaz.
- MFA yol haritası: Kritik kullanıcılar için MFA desteğini planla (TOTP/FIDO2), en azından MFA’ya hazır konfigürasyonu devreye al.
- Razor güvenliği: Tüm POST Razor sayfalarında antiforgery zorlama, auth cookie için `HttpOnly`, `Secure`, `SameSite=Strict` ve kısa yaşam süresi ayarlarını uygula.

## Sprint 3 – Veri Koruma & Uygulama Sertleştirme
- Veri şifreleme: Hassas alanlar için kolon seviyesinde şifreleme veya Always Encrypted seçeneklerini değerlendirmeye al.
- Girdi doğrulama: DTO seviyesinde `FluentValidation`/`DataAnnotations` ile zorunlu alan, uzunluk ve format kontrollerini genişlet.
- Loglama ve denetim: Serilog/AppInsights ile güvenlik log formatını standardize et, başarısız girişler ve kritik işlemler için denetim izi oluştur, PII maskelemesini uygula.
- Bağımlılık güvenliği: `dotnet list package --vulnerable` taramalarını CI/CD’ye ekle, Dependabot/Snyk entegrasyonu kur, CVE triage süreci tanımla.

## Sprint 4 – Operasyonel Güvenlik & Süreklilik
- CI/CD güvenliği: Pipeline’da secret scanning, SAST (SonarQube/Snyk) ve güvenlik testlerini zorunlu kıl; imzalı build çıktıları ve branch korumaları ekle.
- DAST ve pentest: OWASP ZAP/Burp ile otomatik taramalar, yılda en az bir manuel penetrasyon testi planı oluştur.
- İzleme ve incident response: SIEM entegrasyonu, anomalik giriş ve veri sızıntısı için uyarılar, incident response playbook’u hazırla.
- Eğitim ve dokümantasyon: Geliştiricilere güvenli kodlama eğitimi sun, güvenlik politikalarını ve operasyon runbook’larını güncelle.


