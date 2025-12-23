# Hack N Slash - Oyun Sistemi

## 📁 Klasör Yapısı

```
Scripts/
├── Core/
│   ├── GameManager.cs       - Oyun durumu, skor, level, can yönetimi
│   ├── CharacterSelector.cs - Karakter seçim sistemi
│   └── GameBootstrap.cs     - Otomatik oyun başlatıcı (test için)
├── Player/
│   ├── PlayerBase.cs        - Tüm karakterlerin temel sınıfı
│   ├── RangedCharacter.cs   - Uzak mesafe saldırı karakteri (Nişancı)
│   ├── MeleeCharacter.cs    - Yakın dövüş karakteri (Savaşçı)
│   └── TrapperCharacter.cs  - Tuzakçı karakter
├── Enemy/
│   ├── Enemy.cs             - Düşman AI ve hasar sistemi
│   └── EnemySpawner.cs      - Düşman spawn sistemi
├── Combat/
│   ├── Projectile.cs        - Mermi sistemi
│   ├── Trap.cs              - Tuzak sistemi
│   └── Explosive.cs         - Patlayıcı sistemi
├── Items/
│   └── PowerUp.cs           - Güç artırıcı eşyalar
├── UI/
│   └── UIManager.cs         - Skor, can, level gösterimi
├── Camera/
│   └── CameraFollow.cs      - Oyuncu takip kamerası
└── Map/
    └── MapGenerator.cs      - Prosedürel harita oluşturucu
```

## 🎮 Karakterler

### 1. Nişancı (Ranged)
- **Sol Tık**: Mermi atar
- **Sağ Tık**: Seri atış (3 mermi)
- **Renk**: Mavi

### 2. Savaşçı (Melee)
- **Sol Tık**: Yakın mesafe saldırı
- **Sağ Tık**: Dönerek saldırı (360°)
- **Renk**: Kırmızı

### 3. Tuzakçı (Trapper)
- **Sol Tık**: Tuzak yerleştirir
- **Sağ Tık**: Patlayıcı yerleştirir
- **E Tuşu**: Tüm patlayıcıları patlatır
- **Renk**: Mor

## 🕹️ Kontroller

| Tuş | Aksiyon |
|-----|---------|
| WASD | Hareket |
| Fare | Yön |
| Sol Tık | Birincil Saldırı |
| Sağ Tık | İkincil Saldırı |
| E | Patlayıcıları patlat (Tuzakçı) |
| ESC | Duraklat |

## ⚙️ Oyun Mekaniği

### Level Sistemi
- Level × 100 puana ulaşınca bir sonraki level'e geçilir
- Level 1: 100 puan
- Level 2: 200 puan
- Level 3: 300 puan ...

### Zorluk Artışı (Her Level)
- Düşman hasarı: +%20
- Düşman sayısı: +2
- Spawn aralığı: -%10 (minimum 1 saniye)

### Can Sistemi
- 3 can
- Her can kaybında sağlık yenilenir
- 0 canda oyun biter

### Loot Sistemi
- Düşmanlar %10 şansla güç artırıcı düşürür
- Güç artırıcılar saldırı gücünü artırır

## 🗺️ Harita Tipleri

1. **Orman (Forest)**: Ağaç engelleri, yeşil zemin
2. **Çöl (Desert)**: Kaya engelleri, kumlu zemin
3. **Şehir (City)**: Bina engelleri, gri zemin

## 🚀 Hızlı Başlangıç

### Yöntem 1: GameBootstrap Kullanarak
1. Boş bir sahne oluşturun
2. Boş bir GameObject oluşturun
3. `GameBootstrap` scriptini ekleyin
4. "Auto Create Systems", "Auto Spawn Player", "Auto Generate Map" seçeneklerini işaretleyin
5. Oyunu çalıştırın

### Yöntem 2: Manuel Kurulum
1. **GameManager**: Boş objeye `GameManager` ekleyin
2. **EnemySpawner**: Boş objeye `EnemySpawner` ekleyin
3. **MapGenerator**: Boş objeye `MapGenerator` ekleyin ve "Generate Map" çalıştırın
4. **CharacterSelector**: Boş objeye `CharacterSelector` ekleyin, "Auto Spawn On Start" işaretleyin
5. **Camera**: Main Camera'ya `CameraFollow` ekleyin
6. **UI**: Canvas oluşturun, `UIManager` ekleyin

## 📝 Layer Ayarları

Aşağıdaki layer'ları oluşturun:
- `Player` (Layer 8)
- `Enemy` (Layer 9)
- `Ground` (Layer 10)

## 🏷️ Tag Ayarları

Aşağıdaki tag'leri oluşturun:
- `Player`
- `Enemy`
- `Ground`
- `Obstacle`
- `Wall`

## 💡 İpuçları

- Prefab oluşturmak için karakterleri/düşmanları sahneye spawn edin, sonra Project'e sürükleyin
- MapGenerator'da "Generate Map" context menüsünü kullanarak harita önizlemesi yapabilirsiniz
- UI için TextMeshPro paketini import etmeniz gerekebilir
- NavMesh kullanmak için haritayı bake edin ve Enemy'lere NavMeshAgent ekleyin

## 🔧 Prefab Gereksinimleri

### Projectile Prefab (Opsiyonel)
- Sphere + Projectile script
- Collider (isTrigger = true)
- Layer: Default veya Projectile

### Enemy Prefab (Opsiyonel)
- Capsule + Enemy script + Rigidbody
- Tag: Enemy
- Layer: Enemy

### PowerUp Prefab (Opsiyonel)
- Cube + PowerUp script
- Collider (isTrigger = true)
