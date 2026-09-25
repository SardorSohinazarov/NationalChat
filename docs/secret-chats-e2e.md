# Maxfiy chatlar (E2E) — dizayn va reja

> Holat: **muhokama / loyiha**. Kod hali yozilmagan. Pastdagi "Ochiq savollar" bo'limiga javob berilgach, 1-bosqichdan boshlanadi.

## 0. Hozirgi holat

- `Domain/Entities/Chats/ChatModels.cs` da `SecretChat` entity bor, `ChatType.Secret = 4` ham mavjud.
- **Muammo:** `SecretChat.EncryptionKey` serverda saqlanadi. Kalit serverda bo'lsa, bu E2E emas. Bu maydon olib tashlanishi kerak (migratsiya bilan).

## 1. Asosiy qaror: model

| Variant | Qanday ishlaydi | Murakkabligi |
|---|---|---|
| **A. Qurilmaga bog'langan (Telegram modeli)** | Maxfiy chat ikki aniq qurilma (session) o'rtasida. Boshqa qurilmada ko'rinmaydi | O'rtacha |
| B. Ko'p qurilmali (Signal/WhatsApp modeli) | Har xabar foydalanuvchining barcha qurilmalari uchun alohida shifrlanadi | Ancha yuqori |

**Tavsiya: A.** Oddiy chatlar bulutda, maxfiy chatlar faqat qurilmada (Telegramdagidek).

## 2. Ssenariy (foydalanuvchi ko'zi bilan)

1. **Ali** Vali profilini ochadi va **"Maxfiy chat boshlash"** tugmasini bosadi.
2. Ali brauzeri vaqtinchalik X25519 kalit juftini yaratadi. Serverga faqat **ochiq kalit** yuboriladi. Holat: `Pending` ("Vali ulanishini kutyapmiz…").
3. Valining **onlayn qurilmalariga** SignalR orqali `SecretChatRequested` hodisasi keladi.
4. Vali qaysi qurilmada qabul qilsa, chat o'sha qurilmaga bog'lanadi. Vali brauzeri o'z kalit juftini yaratadi, umumiy sirni (ECDH) hisoblaydi va ochiq kalitini qaytaradi. Holat: `Active`.
5. Ali ham shu umumiy sirni hisoblaydi. Ikkalasiga **kalit izi** (fingerprint: emoji qatori yoki rasm) ko'rsatiladi. Ular uni yuzma-yuz yoki qo'ng'iroqda solishtirib, o'rtada "odam" (MITM) yo'qligini tekshiradi.
6. Xabarlar brauzerda shifrlanadi. Server faqat shifrlangan blobni ko'radi va yetkazib beradi.
7. **O'z-o'zini o'chirish taymeri** va "chatni yopish". Chat yopilsa yoki logout qilinsa, qurilmadagi kalitlar va tarix o'chiriladi.

## 3. Kriptografiya (brauzerda)

- **Web Crypto API**: X25519 (kelishish), Ed25519 (imzo), HKDF, AES-256-GCM.
- Kalitlar `extractable: false` qilib **IndexedDB** ga saqlanadi (JS kodi ham xom baytlarni o'qiy olmaydi).
- **Protokol.** O'zimiz yozgan kriptografiyadan qochamiz:
  - **Tavsiya:** tayyor, audit qilingan Double Ratchet kutubxonasi. Masalan, Matrix'ning **vodozemac** (Olm), WASM orqali. Qaysi JS/WASM bog'lamasini olishni o'rnatishdan oldin tekshirish kerak.
  - **MVP uchun oddiyroq yo'l:** boshlang'ich ECDH, keyin HKDF bilan simmetrik hash-chain ratchet (har xabarda kalit yangilanadi, forward secrecy beradi). AES-GCM `additionalData` maydoniga `secretChatId | senderSessionId | seq` qo'yiladi (replay va tartib almashtirishdan himoya).
- Shifrlanadigan payload (bitta JSON):
  `{ text, replyToSeq, attachments: [{ fileId, key, sha256, mime, thumb }], ttl }`

## 4. Server (`NationalChat`) o'zgarishlari

### `SecretChat` qayta tuziladi

```csharp
public class SecretChat
{
    public int Id { get; set; }
    public int InitiatorId { get; set; }
    public int InitiatorSessionId { get; set; }      // qurilma
    public int ParticipantId { get; set; }
    public int? ParticipantSessionId { get; set; }   // qabul qilganda bog'lanadi
    public string InitiatorPublicKey { get; set; }   // faqat OCHIQ kalit
    public string? ParticipantPublicKey { get; set; }
    public SecretChatStatus Status { get; set; }     // Pending, Active, Closed
    public DateTime CreatedAt { get; set; }
    public DateTime? AcceptedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    // EncryptionKey — O'CHIRILADI
}
```

### Alohida `SecretMessage` jadvali

`Message` ichiga qo'shilmaydi: `TextContent`, `SearchText`, `Reactions`, `Views` bu yerda ma'noga ega emas.

```csharp
public class SecretMessage
{
    public long Id { get; set; }
    public int SecretChatId { get; set; }
    public int SenderSessionId { get; set; }
    public long Seq { get; set; }
    public byte[] Ciphertext { get; set; }   // hajmi cheklanadi
    public DateTime CreatedAt { get; set; }
    public DateTime? DeliveredAt { get; set; }
}
```

Server **store-and-forward** vazifasini bajaradi: qabul qiluvchi qurilma `Ack` yuborgach blob o'chiriladi. Yetkazilmaganlari N kundan keyin fon ishida tozalanadi.

### Endpointlar va Hub hodisalari

- `POST /secret-chats` (so'rov)
- `POST /secret-chats/{id}/accept`
- `POST /secret-chats/{id}/close`
- `POST /secret-chats/{id}/messages`
- `POST /secret-chats/{id}/ack`
- SignalR: `SecretChatRequested`, `SecretChatAccepted`, `SecretMessageReceived`, `SecretChatClosed`. Ular **session bo'yicha** guruhga yuboriladi, user bo'yicha emas.

### Ruxsat qoidalari

- Faqat bog'langan `SessionId` o'qiy va yoza oladi.
- Session revoke qilinsa (logout yoki "boshqa qurilmalarni chiqarish"), chat avtomatik `Closed` bo'ladi.

Clean Architecture qoidalari (`AGENTS.md`) saqlanadi: entity Domain'da, use-case, DTO va validatorlar Application'da, EF repository Infrastructure'da, controller va Hub API'da.

## 5. Maxfiy chatda ishlamaydigan narsalar

- **Server qidiruvi va transliteratsiya** (`SearchText`) tatbiq etilmaydi. Lotin/kirill ko'rinishini klientdagi `uzbek-transliterator.ts` bilan klientning o'zida qilish mumkin.
- **Fayllar** brauzerda tasodifiy AES kalit bilan shifrlanib yuklanadi. Shu sababli **ClamAV** va **ffmpeg probe** ishlamaydi. Bu fayllar uchun ular chetlab o'tiladi, lekin hajm va tur baribir cheklanadi. Preview va thumbnail klientda yaratiladi.
- Forward, chat ro'yxatidagi matn preview'i, push'dagi matn (faqat "Yangi maxfiy xabar"), boshqa qurilmaga sinxronizatsiya, guruhli maxfiy chatlar (1-versiyada faqat 1:1).
- **Skrinshotni** vebda to'sib bo'lmaydi.

## 6. Veb uchun muhim ogohlantirish

Klient Angular veb-ilova, JS kodini esa server yuboradi. Server buzilsa, u zararli JS yuborib kalitlarni o'g'irlashi mumkin. Bu vebdagi E2E'ning asosiy zaif nuqtasi. Kamaytirish choralari:

- qat'iy **CSP** va uchinchi tomon skriptlaridan voz kechish;
- kripto kodini alohida modul yoki Web Worker'da saqlash;
- SSR'da kripto kodi umuman ishlamasligi (`isPlatformBrowser`).

Kelajakda desktop yoki mobil ilova chiqsa, himoya kuchliroq bo'ladi.

## 7. Bosqichma-bosqich reja

1. **Backend asosi:** `SecretChat` refaktori (`EncryptionKey`ni olib tashlash va migratsiya), `SecretMessage`, handshake endpointlari, Hub hodisalari, session revoke bilan bog'lash, testlar.
2. **Klient kripto qatlami** (`NationalChatClient`): `SecretCryptoService` (Web Crypto va IndexedDB), handshake, fingerprint.
3. **UI:** "Maxfiy chat boshlash", Pending/Active holatlari, qulf belgisi, fingerprint ekrani.
4. **Taymerlar va yopish:** tarixni tozalash.
5. **Shifrlangan fayllar.**
6. Kerak bo'lsa: Double Ratchet'ga o'tish yoki ko'p qurilmali model.

## 8. Ochiq savollar (kod yozishdan oldin hal qilinadi)

1. **Qurilmaga bog'langan model** (tavsiya) to'g'ri keladimi?
2. Birinchi versiyaga **fayllar** kiradimi yoki faqat matnmi?
3. **Protokol:** tayyor kutubxona (vodozemac/Olm) yoki MVP uchun Web Crypto'dagi oddiy hash-chain ratchet?
