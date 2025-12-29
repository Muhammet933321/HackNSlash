<p align="center">
  <img src="https://img.shields.io/badge/Unity-2022.3+-black?style=for-the-badge&logo=unity" alt="Unity">
  <img src="https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white" alt="C#">
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20Mobile-blue?style=for-the-badge" alt="Platform">
  <img src="https://img.shields.io/badge/License-MIT-green?style=for-the-badge" alt="License">
</p>

<h1 align="center">Hack'N'Slash</h1>

<p align="center">
  <strong>Unity ile geliştirilmiş aksiyon dolu bir Hack and Slash oyunu</strong>
</p>

<p align="center">
  <a href="#-özellikler">Özellikler</a> •
  <a href="#-oynanış">Oynanış</a> •
  <a href="#-karakterler">Karakterler</a> •
  <a href="#-haritalar">Haritalar</a> •
  <a href="#-kurulum">Kurulum</a> •
  <a href="#-teknik-detaylar">Teknik Detaylar</a>
</p>

---

## Genel Bakış

**Hack'N'Slash**, Unity oyun motoru kullanılarak C# ile geliştirilmiş, dalga bazlı düşman sistemine sahip bir aksiyon oyunudur. Oyuncular üç farklı karakter sınıfından birini seçerek, prosedürel olarak oluşturulan haritalarda hayatta kalmaya çalışır.

Proje, modüler mimari yapısı ve genişletilebilir tasarımı ile hem oynanabilir bir deneyim hem de öğretici bir kod tabanı sunmaktadır.

---


## Özellikler

### Oyun Mekanikleri
- **3 Benzersiz Karakter** — Her biri farklı oyun tarzı sunan savaşçı, nişancı ve tuzakçı sınıfları
- **Dalga Sistemi** — Zorluk seviyesi artan düşman dalgaları
- **Dinamik Zorluk** — Level bazlı hasar, spawn hızı ve düşman sayısı artışı
- **Güç Artırıcılar** — Düşmanlardan düşen geçici buff'lar


### Teknik Özellikler
- **Prosedürel Harita** — Runtime'da oluşturulan engeller ve NavMesh
- **Singleton Yönetim** — Merkezi oyun durumu kontrolü
- **Event-Driven UI** — Loosely coupled arayüz güncellemeleri
- **NavMesh AI** — Engelleri dolaşan akıllı düşman hareketi

---

## Oynanış

### Kontroller

| Girdi | İşlev |
|:---:|:---|
| `W` `A` `S` `D` | Karakter hareketi |
| `Fare` | Yön kontrolü |
| `Sol Tık` | Birincil saldırı |
| `Sağ Tık` | Özel saldırı |
| `E` | Manuel patlama (Tuzakçı) |
| `ESC` | Duraklatma menüsü |

### Oyun Döngüsü

```
Karakter Seç → Harita Seç → Dalgaları Yenmeye Çalış → Level Atla → Tekrar Et
```

Her level geçişinde:
- Düşman hasarı **%20** artar
- Dalga başına düşman sayısı **+2** fazlalaşır
- Spawn aralığı **%10** kısalır

---

## Karakterler

### Savaşçı (Melee)
> *Yakın mesafe uzmanı*

Geniş açılı kılıç saldırıları ile çevresindeki düşmanları biçer. Özel yeteneği olan **Spin Attack**, 360 derece hasar vererek kuşatmalarda etkilidir.

**Avantajlar:** Yüksek alan hasarı, cooldown gerektirmeyen birincil saldırı  
**Dezavantajlar:** Düşmanlara yaklaşma zorunluluğu
<p align="center">
<img width="743" height="415" alt="Screenshot 2025-12-25 111814" src="https://github.com/user-attachments/assets/3ea42bbc-55e6-4e87-921b-ca4f2201e10c" />
</p>

---

### Nişancı (Ranged)
> *Uzak mesafe uzmanı*

Hızlı mermiler fırlatarak güvenli mesafeden savaşır. **Burst Fire** yeteneği, fan şeklinde yayılan üç mermi atar.

**Avantajlar:** Güvenli mesafe, yüksek tek hedef hasarı  
**Dezavantajlar:** Kalabalık düşman gruplarında zorlanma

<p align="center">
<img width="743" height="415" alt="Screenshot 2025-12-25 112756" src="https://github.com/user-attachments/assets/80d8ba55-9408-4cf1-8f4b-2e03e654ff23" />
</p>

---

### Tuzakçı (Trapper)
> *Strateji uzmanı*

Diken tuzakları ve patlayıcılar yerleştirerek alanı kontrol eder. `E` tuşu ile tüm patlayıcıları manuel olarak tetikleyebilir.

**Avantajlar:** Alan kontrolü, stratejik oyun  
**Dezavantajlar:** Reaktif savaşta zayıf
<p align="center">
<img width="743" height="415" alt="Screenshot 2025-12-25 112923" src="https://github.com/user-attachments/assets/a87db2db-bb48-46eb-ad4a-d151dbc1b9d8" />
</p>

---

## Haritalar

| Tema | Açıklama | Engeller |
|:---:|:---|:---|
| **Orman** | Yoğun ağaçlıklı alan | Ağaçlar |
| **Çöl** | Açık kum arazisi | Kayalar |
| **Şehir** | Urban ortam | Binalar |

Her harita **prosedürel olarak** oluşturulur — aynı tema farklı engel dizilimleri sunar.

---

## Kurulum

### Gereksinimler
- Unity 2022.3 LTS veya üzeri
- Universal Render Pipeline (URP)
- AI Navigation paketi

### Adımlar

1. **Repository'yi klonlayın**
```bash
git clone https://github.com/kullanici/HackNSlash.git
```

2. **Unity Hub ile açın**
```
Unity Hub → Add → Proje klasörünü seçin
```

3. **Sahneyi çalıştırın**
```
Assets/Scenes/MainMenuScene.unity → Play
```

---

## Teknik Detaylar

### Proje Yapısı

```
Assets/
├── Scripts/
│   ├── Core/           # GameManager, Bootstrap, CharacterSelector
│   ├── Player/         # PlayerBase, MeleeCharacter, RangedCharacter, TrapperCharacter
│   ├── Enemy/          # Enemy AI, EnemySpawner
│   ├── Combat/         # Projectile, Trap, Explosive
│   ├── Map/            # MapGenerator (Prosedürel)
│   ├── Camera/         # CameraFollow
│   ├── Effects/        # SlashEffect, SpinEffect
│   ├── Items/          # PowerUp
│   └── UI/             # UIManager, MainMenuManager
├── Prefabs/
├── Materials/
├── Scenes/
└── Settings/           # URP Asset, Render Pipeline
```

### Mimari Yaklaşım

```
┌─────────────────┐
│   GameManager   │ ◄─── Singleton, Event Publisher
└────────┬────────┘
         │ Events
         ▼
┌─────────────────┐     ┌─────────────────┐
│   UIManager     │     │  EnemySpawner   │
└─────────────────┘     └─────────────────┘
         ▲                       │
         │                       ▼
┌─────────────────┐     ┌─────────────────┐
│   PlayerBase    │ ◄──►│     Enemy       │
└─────────────────┘     └─────────────────┘
```

### Kullanılan Tasarım Desenleri

| Desen | Kullanım |
|:---|:---|
| **Singleton** | GameManager, UIManager — Tek örnek garantisi |
| **Template Method** | PlayerBase — Alt sınıflar için saldırı şablonu |
| **Observer** | Event sistemi — UI güncellemeleri |
| **Factory** | CharacterSelector — Karakter oluşturma |

### Temel Sistemler

**Hareket Sistemi**  
Rigidbody tabanlı fizik hareketi. `FixedUpdate` döngüsünde velocity ataması yapılır, bu sayede tutarlı çarpışma davranışı sağlanır.

**Saldırı Sistemi**  
Her karakter `PerformPrimaryAttack()` ve `PerformSpecialAttack()` abstract metodlarını implement eder. Cooldown yönetimi base sınıfta gerçekleşir.

**NavMesh AI**  
Düşmanlar `NavMeshAgent` kullanarak oyuncuyu takip eder. `NavMesh.SamplePosition` ile geçerli spawn noktaları belirlenir.

**Prosedürel Harita**  
`MapGenerator` runtime'da zemin, engeller ve duvarları oluşturur. Sonrasında `NavMeshSurface.BuildNavMesh()` çağrılarak navigasyon mesh'i bake edilir.

---

## Geliştirme Notları

### Yeni Karakter Ekleme

1. `PlayerBase` sınıfından türetin
2. `PerformPrimaryAttack()` ve `PerformSpecialAttack()` metodlarını implement edin
3. Prefab oluşturun ve `CharacterSelector`'a ekleyin

### Yeni Harita Teması Ekleme

1. `MapType` enum'una yeni değer ekleyin
2. `MapGenerator.CreateObstacle()` metoduna yeni tema case'i yazın
3. Tema rengini `GetGroundColor()` metoduna ekleyin

