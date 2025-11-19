'use strict';

(function () {
    const navToggle = document.querySelector('.nav-toggle');
    const navList = document.getElementById('nav-list');

    if (navToggle && navList) {
        navToggle.addEventListener('click', () => {
            const isOpen = navList.classList.toggle('open');
            navToggle.setAttribute('aria-expanded', isOpen.toString());
        });

        navList.querySelectorAll('a').forEach((link) => {
            link.addEventListener('click', () => {
                navList.classList.remove('open');
                navToggle.setAttribute('aria-expanded', 'false');
            });
        });
    }

    const translations = {
        tr: {
            'brand.tagline': 'Geleceği Tasarlıyoruz',
            'nav.home': 'Anasayfa',
            'nav.about': 'Hakkımızda',
            'nav.vision': 'Vizyonumuz',
            'nav.mission': 'Misyonumuz',
            'nav.services': 'Çözümler',
            'nav.contact': 'İletişim',
            'header.portal': 'Portal Girişi',
            'hero.eyebrow': 'Kurumsal dönüşümde öncü',
            'hero.title': 'Geleceğin iş dünyasını bugün inşa ediyoruz',
            'hero.subtitle': 'GMS Group, teknoloji ve stratejiyi birleştirerek kurumların sürdürülebilir büyümesini mümkün kılar.',
            'hero.ctaPrimary': 'Bizimle iletişime geç',
            'hero.ctaSecondary': 'Daha fazla keşfet',
            'hero.statClients': 'Global müşteri',
            'hero.statProjects': 'Ödüllü proje',
            'hero.statYears': 'Yıllık deneyim',
            'hero.media.title': 'Akıllı Operasyon Merkezi',
            'hero.media.caption': 'Gerçek zamanlı görünürlük',
            'about.eyebrow': 'Bizi yakından tanıyın',
            'about.title': 'Güvenilir iş ortağınız',
            'about.text': 'Teknoloji, strateji ve tasarım disiplinlerini birleştirerek markaların dijital dönüşüm yolculuklarında yanında oluyoruz. Ölçeklenebilir ve güvenli sistemler kuruyor, sürdürülebilir büyüme için yol arkadaşlığı yapıyoruz.',
            'about.item1Title': '360° yaklaşım',
            'about.item1Text': 'Danışmanlıktan ürüne uzanan uçtan uca hizmet modelimizle fark yaratıyoruz.',
            'about.item2Title': 'Yüksek erişilebilirlik',
            'about.item2Text': 'Çok bölgeli mimarilerle kesintisiz hizmet sağlayan altyapılar tasarlıyoruz.',
            'about.item3Title': 'Güvenlik odağı',
            'about.item3Text': 'Sıfır güven prensipleriyle verinizi, reputasyonunuzu ve kullanıcılarınızı koruyoruz.',
            'vision.eyebrow': 'Vizyonumuz',
            'vision.title': 'Kurumların dijital sınırlarını genişletmek',
            'vision.text': 'Yarınların iş modellerini bugünden deneyimlemek isteyen kurumlara, veri odaklı karar mekanizmaları ve akıllı otomasyon altyapıları kuruyoruz.',
            'vision.card1Title': 'Bağlantılı ekosistemler',
            'vision.card1Text': 'API-first yaklaşımıyla hizmetleri birbirine bağlayan platformlar inşa ediyoruz.',
            'vision.card2Title': 'Veriden değer üretme',
            'vision.card2Text': 'Gelişmiş analitik çözümleriyle operasyonel veriyi stratejik içgörüye dönüştürüyoruz.',
            'mission.eyebrow': 'Misyonumuz',
            'mission.title': 'İş sonuçlarına odaklanan teknolojiler üretmek',
            'mission.text': 'Teknik mükemmeliyetin ötesinde, her çözümümüzün kurumlara ölçülebilir değer sağlamasına odaklanıyoruz. İnsan ve teknolojiyi uyumla buluşturuyoruz.',
            'mission.item1Title': 'Stratejik ortaklık',
            'mission.item1Text': 'Müşterilerimizin hedefleri doğrultusunda esnek, uzun soluklu iş birlikleri kuruyoruz.',
            'mission.item2Title': 'Sürdürülebilir inovasyon',
            'mission.item2Text': 'Her teslimatta ölçülebilir performans artışı sağlarken bakım maliyetlerini azaltıyoruz.',
            'mission.item3Title': 'İnsan odaklı tasarım',
            'mission.item3Text': 'Deneyim tasarımı ile teknolojiyi birleştirerek kullanıcı memnuniyetini artırıyoruz.',
            'services.eyebrow': 'Çözümlerimiz',
            'services.title': 'Uçtan uca kurumsal hizmetler',
            'services.card1Title': 'Dijital strateji',
            'services.card1Text': 'Pazar dinamiklerine uygun iş modelleri ve yol haritaları tasarlarız.',
            'services.card2Title': 'Kurumsal yazılım',
            'services.card2Text': 'Mikroservis, bulut ve hibrit mimarilerle yüksek ölçeklenebilir platformlar geliştiririz.',
            'services.card3Title': 'Veri & yapay zeka',
            'services.card3Text': 'Veri mühendisliği, BI ve yapay zeka çözümleriyle operasyonlarınızı dönüştürürüz.',
            'services.card4Title': 'Deneyim tasarımı',
            'services.card4Text': 'Kullanıcı araştırmalarıyla desteklenen akıcı deneyimler oluştururuz.',
            'highlight.eyebrow': 'Neden GMS?',
            'highlight.title': 'Veriyle güçlenen karar mekanizmaları',
            'highlight.text': 'Ölçülebilir KPI\'lar, gerçek zamanlı gösterge panelleri ve predictive modeller ile liderlere canlı durum farkındalığı sağlıyoruz.',
            'highlight.item1': 'Çapraz platform entegrasyon',
            'highlight.item2': '360° güvenlik yaklaşımı',
            'highlight.item3': 'Çevik teslimat metodolojisi',
            'contact.eyebrow': 'İletişime geçin',
            'contact.title': 'Beraber değer üretelim',
            'contact.text': 'Projenizi, vizyonunuzu ya da aklınızdaki fikri paylaşın. 24 saat içinde size dönüş yapalım.',
            'contact.phoneLabel': 'Telefon',
            'contact.mailLabel': 'E-posta',
            'contact.addressLabel': 'Adres',
            'contact.addressValue': 'Maslak, İstanbul',
            'contact.form.nameLabel': 'Ad Soyad',
            'contact.form.namePlaceholder': 'Örn. Ayşe Yılmaz',
            'contact.form.emailLabel': 'E-posta',
            'contact.form.emailPlaceholder': 'ornek@domain.com',
            'contact.form.companyLabel': 'Şirket',
            'contact.form.companyPlaceholder': 'GMS Group',
            'contact.form.messageLabel': 'Mesajınız',
            'contact.form.messagePlaceholder': 'Projenizi anlatın',
            'contact.form.submit': 'Gönder',
            'footer.copy': '© 2025 GMS Group. Tüm hakları saklıdır.',
            'footer.privacy': 'Gizlilik',
            'footer.security': 'Güvenlik',
            'footer.careers': 'Kariyer'
        },
        en: {
            'brand.tagline': 'Designing Tomorrow',
            'nav.home': 'Home',
            'nav.about': 'About',
            'nav.vision': 'Vision',
            'nav.mission': 'Mission',
            'nav.services': 'Solutions',
            'nav.contact': 'Contact',
            'header.portal': 'Portal Login',
            'hero.eyebrow': 'Pioneering digital growth',
            'hero.title': 'Building tomorrow’s business today',
            'hero.subtitle': 'GMS Group merges technology and strategy so enterprises can scale sustainably.',
            'hero.ctaPrimary': 'Talk to us',
            'hero.ctaSecondary': 'Discover more',
            'hero.statClients': 'Global clients',
            'hero.statProjects': 'Awarded projects',
            'hero.statYears': 'Years of expertise',
            'hero.media.title': 'Smart Operations Center',
            'hero.media.caption': 'Real-time visibility',
            'about.eyebrow': 'Get to know us',
            'about.title': 'Your trusted partner',
            'about.text': 'We unite technology, strategy, and design to guide brands through digital transformation with scalable, secure systems.',
            'about.item1Title': '360° approach',
            'about.item1Text': 'End-to-end services from advisory to products.',
            'about.item2Title': 'High availability',
            'about.item2Text': 'Multi-region architectures for uninterrupted service.',
            'about.item3Title': 'Security focus',
            'about.item3Text': 'Zero-trust principles safeguard your data and reputation.',
            'vision.eyebrow': 'Our vision',
            'vision.title': 'Expanding the digital frontier',
            'vision.text': 'We build data-driven decision engines and automation platforms for forward-looking enterprises.',
            'vision.card1Title': 'Connected ecosystems',
            'vision.card1Text': 'API-first platforms that interlink every service.',
            'vision.card2Title': 'Value from data',
            'vision.card2Text': 'Advanced analytics turning operations into insight.',
            'mission.eyebrow': 'Our mission',
            'mission.title': 'Technology with business outcomes',
            'mission.text': 'We marry people and technology to deliver measurable value, not just technical excellence.',
            'mission.item1Title': 'Strategic partnership',
            'mission.item1Text': 'Long-term collaborations aligned to your goals.',
            'mission.item2Title': 'Sustainable innovation',
            'mission.item2Text': 'Measurable performance gains with lower maintenance.',
            'mission.item3Title': 'Human-centered design',
            'mission.item3Text': 'Experiences that keep users delighted.',
            'services.eyebrow': 'Our services',
            'services.title': 'End-to-end enterprise delivery',
            'services.card1Title': 'Digital strategy',
            'services.card1Text': 'Design market-ready business models and roadmaps.',
            'services.card2Title': 'Enterprise software',
            'services.card2Text': 'Cloud, microservice, and hybrid platforms for scale.',
            'services.card3Title': 'Data & AI',
            'services.card3Text': 'Data engineering, BI, and AI that reinvent operations.',
            'services.card4Title': 'Experience design',
            'services.card4Text': 'Research-backed experiences users love.',
            'highlight.eyebrow': 'Why GMS?',
            'highlight.title': 'Data-powered decisions',
            'highlight.text': 'KPIs, live dashboards, and predictive models give leaders real-time awareness.',
            'highlight.item1': 'Cross-platform integration',
            'highlight.item2': '360° security posture',
            'highlight.item3': 'Agile delivery methods',
            'contact.eyebrow': 'Contact',
            'contact.title': 'Let’s create value together',
            'contact.text': 'Share your project or idea and we’ll reply in 24 hours.',
            'contact.phoneLabel': 'Phone',
            'contact.mailLabel': 'Email',
            'contact.addressLabel': 'Address',
            'contact.addressValue': 'Maslak, Istanbul',
            'contact.form.nameLabel': 'Name & Surname',
            'contact.form.namePlaceholder': 'e.g. Jane Doe',
            'contact.form.emailLabel': 'Email',
            'contact.form.emailPlaceholder': 'sample@domain.com',
            'contact.form.companyLabel': 'Company',
            'contact.form.companyPlaceholder': 'GMS Group',
            'contact.form.messageLabel': 'Message',
            'contact.form.messagePlaceholder': 'Tell us about your project',
            'contact.form.submit': 'Send message',
            'footer.copy': '© 2025 GMS Group. All rights reserved.',
            'footer.privacy': 'Privacy',
            'footer.security': 'Security',
            'footer.careers': 'Careers'
        },
        ar: {
            'brand.tagline': 'نصمم المستقبل',
            'nav.home': 'الرئيسية',
            'nav.about': 'من نحن',
            'nav.vision': 'الرؤية',
            'nav.mission': 'الرسالة',
            'nav.services': 'الحلول',
            'nav.contact': 'اتصل بنا',
            'header.portal': 'دخول البوابة',
            'hero.eyebrow': 'رواد التحول المؤسسي',
            'hero.title': 'نبني عالم الأعمال القادم اليوم',
            'hero.subtitle': 'تجمع GMS Group بين التقنية والاستراتيجية لتمكين النمو المستدام.',
            'hero.ctaPrimary': 'تواصل معنا',
            'hero.ctaSecondary': 'اكتشف المزيد',
            'hero.statClients': 'عملاء عالميون',
            'hero.statProjects': 'مشاريع حائزة على جوائز',
            'hero.statYears': 'سنوات الخبرة',
            'hero.media.title': 'مركز العمليات الذكي',
            'hero.media.caption': 'رؤية فورية',
            'about.eyebrow': 'تعرف علينا',
            'about.title': 'شريككم الموثوق',
            'about.text': 'نجمع بين التقنية والاستراتيجية والتصميم لدعم العلامات التجارية في رحلة التحول الرقمي.',
            'about.item1Title': 'منهج شامل',
            'about.item1Text': 'خدمات متكاملة من الاستشارات إلى المنتجات.',
            'about.item2Title': 'توافر عالٍ',
            'about.item2Text': 'بنى متعددة المناطق لخدمة دون انقطاع.',
            'about.item3Title': 'تركيز على الأمان',
            'about.item3Text': 'مبادئ الثقة الصفرية لحماية بياناتكم.',
            'vision.eyebrow': 'رؤيتنا',
            'vision.title': 'توسيع الحدود الرقمية للمؤسسات',
            'vision.text': 'نبني منصات ذكية مدفوعة بالبيانات للمؤسسات الطموحة.',
            'vision.card1Title': 'أنظمة مترابطة',
            'vision.card1Text': 'منصات API-first تربط كل خدمة.',
            'vision.card2Title': 'قيمة من البيانات',
            'vision.card2Text': 'تحليلات متقدمة لتحويل العمليات إلى رؤى.',
            'mission.eyebrow': 'رسالتنا',
            'mission.title': 'تقنيات تحقق النتائج',
            'mission.text': 'نركز على القيمة القابلة للقياس من خلال مواءمة الإنسان بالتقنية.',
            'mission.item1Title': 'شراكة استراتيجية',
            'mission.item1Text': 'شراكات طويلة الأمد مرنة بحسب الأهداف.',
            'mission.item2Title': 'ابتكار مستدام',
            'mission.item2Text': 'أداء محسّن وتكاليف صيانة أقل.',
            'mission.item3Title': 'تصميم متمحور حول الإنسان',
            'mission.item3Text': 'تجارب تعزز رضا المستخدمين.',
            'services.eyebrow': 'حلولنا',
            'services.title': 'خدمات متكاملة',
            'services.card1Title': 'الاستراتيجية الرقمية',
            'services.card1Text': 'تصميم نماذج أعمال وخطط ملائمة للسوق.',
            'services.card2Title': 'برمجيات مؤسسية',
            'services.card2Text': 'منصات سحابية وميكروسيرفس قابلة للتوسع.',
            'services.card3Title': 'البيانات والذكاء الاصطناعي',
            'services.card3Text': 'هندسة بيانات وذكاء اصطناعي يعيد ابتكار العمليات.',
            'services.card4Title': 'تصميم التجربة',
            'services.card4Text': 'تجارب سلسة مدعومة بأبحاث المستخدمين.',
            'highlight.eyebrow': 'لماذا GMS؟',
            'highlight.title': 'قرارات مدعومة بالبيانات',
            'highlight.text': 'مؤشرات أداء، لوحات لحظية، ونماذج تنبؤية تمنح القادة الوعي الفوري.',
            'highlight.item1': 'تكامل متعدد المنصات',
            'highlight.item2': 'نهج أمني شامل',
            'highlight.item3': 'منهجيات تسليم رشيقة',
            'contact.eyebrow': 'تواصل معنا',
            'contact.title': 'لنخلق قيمة معاً',
            'contact.text': 'شارك مشروعك أو فكرتك وسنعاود الاتصال خلال 24 ساعة.',
            'contact.phoneLabel': 'الهاتف',
            'contact.mailLabel': 'البريد الإلكتروني',
            'contact.addressLabel': 'العنوان',
            'contact.addressValue': 'مسلك، إسطنبول',
            'contact.form.nameLabel': 'الاسم الكامل',
            'contact.form.namePlaceholder': 'مثال: أحمد علي',
            'contact.form.emailLabel': 'البريد الإلكتروني',
            'contact.form.emailPlaceholder': 'example@domain.com',
            'contact.form.companyLabel': 'الشركة',
            'contact.form.companyPlaceholder': 'GMS Group',
            'contact.form.messageLabel': 'رسالتك',
            'contact.form.messagePlaceholder': 'صف مشروعك',
            'contact.form.submit': 'إرسال',
            'footer.copy': '© 2025 مجموعة GMS. جميع الحقوق محفوظة.',
            'footer.privacy': 'الخصوصية',
            'footer.security': 'الأمان',
            'footer.careers': 'الوظائف'
        }
    };

    const applyTranslations = (lang) => {
        const dict = translations[lang];
        if (!dict) {
            return;
        }

        document.querySelectorAll('[data-i18n]').forEach((el) => {
            const key = el.getAttribute('data-i18n');
            if (key && dict[key]) {
                el.textContent = dict[key];
            }
        });

        document.querySelectorAll('[data-i18n-placeholder]').forEach((el) => {
            const key = el.getAttribute('data-i18n-placeholder');
            if (key && dict[key]) {
                el.setAttribute('placeholder', dict[key]);
            }
        });

        document.documentElement.lang = lang;
        document.documentElement.dir = lang === 'ar' ? 'rtl' : 'ltr';
    };

    const langButtons = document.querySelectorAll('.lang-switcher button');

    langButtons.forEach((button) => {
        button.addEventListener('click', () => {
            const lang = button.getAttribute('data-lang');
            langButtons.forEach((btn) => btn.classList.toggle('active', btn === button));
            applyTranslations(lang);
        });
    });

    applyTranslations('tr');
})();


