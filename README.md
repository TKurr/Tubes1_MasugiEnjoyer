# Tubes1_MasugiEnjoyer

# Greedy Bots for Robocode Tank Royale

Ini adalah koleksi bot cerdas berbasis strategi **Greedy** yang diimplementasikan untuk turnamen **Robocode Tank Royale** menggunakan bahasa C# (.NET 6).

---

## Strategi Greedy yang Diimplementasikan

### 1. Greedy by Nearest Target (Bot: **Gnearest**)

**Author:** Theo Kurniady

- Bot akan selalu mengejar dan menyerang musuh terdekat berdasarkan jarak.
- Bot memperbarui daftar musuh yang terdeteksi dan memilih musuh dengan jarak paling pendek (menggunakan jarak Euclidean).
- Setelah target ditentukan, bot mengarahkan tubuh dan senjatanya ke arah musuh lalu bergerak menuju jarak optimal untuk menembak.
- Efektif dalam pertempuran cepat, namun bisa menjadi predictable jika banyak musuh.

---

### 2. Greedy by Lowest Health (Bot: **Ghealth**)

**Author:** Sebastian Enrico Nathanael

- Bot akan selalu menyerang musuh dengan _health_ paling rendah.
- Setiap musuh yang terdeteksi akan disimpan bersama data HP dan jaraknya.
- Target selalu dipilih berdasarkan nilai HP terendah.
- Begitu musuh mati, target baru akan langsung dipilih.
- Fokus untuk mengeliminasi lawan secepat mungkin.

---

### 3. Greedy by Evading (Bot: **Gevade**)

**Author:** Nathanael Rachmat

- Bot bergerak **mengelilingi** musuh terakhir yang terdeteksi.
- Menghindari peluru dengan terus bergerak membentuk lingkaran.
- Saat tidak menemukan musuh, bot akan _reset_ dan memutar badan 90°, serta radar 360°.
- Arah gerak akan berubah jika menabrak tembok, tertembak, atau bertabrakan.
- Tembakan menggunakan **Smart Fire**:  
  \[
  \text{power} = 3 \times e^{-0.01 \times \text{distance}}
  \]

---

### 4. Greedy by Aggressive Circling (Bot: **Gaggressive**)

**Author:** Nathanael Rachmat

- Bot memutari musuh pada sudut ±45° dari posisi musuh.
- Radar tetap mengarah ke musuh dengan memantul bolak-balik.
- Jika sudut tembak tepat, maka bot langsung menembak dengan power yang dihitung secara eksponensial.
- Menggunakan hitungan orbit berdasarkan \(\mathrm{atan2}(dy, dx)\) + offset ±45°.
- Kecepatan bot disesuaikan agar perputaran tetap _smooth_ meski sudut besar.

---

## Requirement

- **.NET SDK 9.0+**
- **Robocode Tank Royale**
- Sistem operasi: Windows / Linux / macOS

---

## Cara Install & Compile

### 1. Download GUI

- Buka tautan [GUI Release v1.0](https://github.com/Ariel-HS/tubes1-if2211-starter-pack/releases/tag/v1.0).
- Unduh (**download**) paket GUI dari halaman rilis tersebut dan ekstrak ke folder yang Anda inginkan.

### 2. Clone Repositori Bot

```bash
git clone https://github.com/TKurr/Tubes1_MasugiEnjoyer.git
```

### 3. Compile Bot

```bash
dotnet clean
dotnet build
```

## Author

| Bot           | Nama                       |
| ------------- | -------------------------- |
| NearestBot    | Theo Kurniady              |
| LowHPBot      | Sebastian Enrico Nathanael |
| CircularBot   | Nathanael Rachmat          |
| AggressiveBot | Nathanael Rachmat          |
