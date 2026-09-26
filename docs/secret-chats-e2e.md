# Maxfiy chatlar (E2E) — dizayn va reja

> Holat: **1–4-bosqichlar tayyor** (backend, klient kripto qatlami, UI, taymerlar). Keyingisi 5-bosqich: shifrlangan fayllar. Qarorlar 8-bo'limda, backend API 9-bo'limda, klient protokoli 10-bo'limda, UI 11-bo'limda, taymerlar 12-bo'limda.

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

## 8. Qarorlar

1. **Model:** qurilmaga bog'langan (Telegram modeli). ✅ Tasdiqlandi.
2. **1-versiya: faqat matn.** Fayllar keyinroq (5-bosqich). ✅ Tasdiqlandi.
3. **Protokol:** Web Crypto'dagi hash-chain ratchet, tashqi kutubxonasiz. ✅ Tasdiqlandi. Tafsilotlari 10-bo'limda.

## 9. Backend API (1-bosqich natijasi)

Hamma endpointlar **joriy qurilma** nomidan ishlaydi: access token ichidagi `sid` (session). Har so'rovda session faolligi bazadan qayta tekshiriladi, chunki JWT logoutdan keyin ham bir necha daqiqa yaroqli bo'lib qoladi.

| Endpoint | Kim | Nima qiladi |
|---|---|---|
| `GET /api/secret-chats` | — | Shu qurilmaga bog'langan ochiq chatlar va foydalanuvchiga kelgan kutilayotgan so'rovlar |
| `GET /api/secret-chats/{id}` | ikki tomon | Bitta chat |
| `POST /api/secret-chats` `{participantId, publicKey}` | tashabbuskor | So'rov (`Pending`). Chat shu qurilmaga bog'lanadi |
| `POST /api/secret-chats/{id}/accept` `{publicKey}` | qabul qiluvchi | `Active`. Chat qabul qilgan qurilmaga bog'lanadi |
| `POST /api/secret-chats/{id}/close` | ikki tomon | Yopish. Kutilayotgan so'rovni rad etish yoki bekor qilish ham shu |
| `POST /api/secret-chats/{id}/messages` `{seq, ciphertext}` | bog'langan qurilma | Shifrlangan xabar yuborish |
| `GET /api/secret-chats/{id}/messages?afterId&limit` | bog'langan qurilma | Shu qurilmaga kelgan, hali tasdiqlanmagan xabarlar (eskisidan boshlab) |
| `POST /api/secret-chats/{id}/ack` `{upToId}` | bog'langan qurilma | `upToId`gacha bo'lgan xabarlar serverdan o'chiriladi |

**Formatlar va qoidalar**

- `publicKey`: standart Base64 (padding bilan), aynan 32 bayt (X25519 raw).
- `ciphertext`: standart Base64, 1 baytdan 32 KB gacha. Server uni ochmaydi va tuzilishiga qaramaydi. Klient `iv` ni blob ichiga qo'yadi.
- `seq`: har tomonning o'z hisoblagichi, 1 dan boshlanadi va har xabarda o'sishi shart. Kichik yoki takroriy `seq` **409 Conflict** bilan rad etiladi (replay himoyasi). Javobi yo'qolgan so'rovni qayta yuborgan klient 409 dan xabar allaqachon serverda ekanini biladi.
- `status` JSON'da son: `1` Pending, `2` Active, `3` Closed (API'dagi boshqa enumlar kabi).
- `mySessionId`: chat ko'ruvchi tomonda qaysi qurilmaga bog'langan. Klient o'z `sid`i bilan solishtiradi: mos kelmasa, chat boshqa qurilmada qabul qilingan, uni yashirish kerak.

**SignalR hodisalari** (`/hubs/chat`; har ulanish `session:{sid}` guruhiga ham qo'shiladi):

| Hodisa | Kimga | Payload |
|---|---|---|
| `SecretChatRequested` | qabul qiluvchining hamma qurilmalariga | `SecretChatDto` |
| `SecretChatAccepted` | tashabbuskor qurilmasi va qabul qiluvchining hamma qurilmalari | `SecretChatDto` (`peerPublicKey` bilan) |
| `SecretMessageReceived` | faqat oluvchi qurilma | `SecretMessageDto` |
| `SecretChatClosed` | ikki bog'langan qurilma (so'rov bosqichida — qabul qiluvchining hamma qurilmalari) | `{ secretChatId }` |

**Avtomatik yopilish**

- Logout, "boshqa qurilmalarni chiqarish" yoki bitta qurilmani chiqarish shu qurilmalardagi chatlarni darhol yopadi. Yetkazilmagan xabarlar o'chiriladi.
- Fon ishi (`SecretChatCleanup`, har soatda) muddati tugagan sessionlarning chatlarini yopadi. 7 kundan beri javob kutayotgan so'rovlarni yopadi va 7 kundan eski yetkazilmagan blobni o'chiradi.
- Server tarix saqlamaydi: xabar tasdiqlangach (`ack`) o'chiriladi.

**Ma'lumotlar bazasi:** `Add-Secret-Chats-E2E` migratsiyasi `EncryptionKey` ustunini o'chiradi va `messaging.secret_messages` jadvalini yaratadi. Eski `secret_chats` qatorlari o'chiriladi: ularning kaliti serverda edi va bog'lanadigan qurilmasi yo'q.

## 10. Klient protokoli (2-bosqich natijasi)

Kod: `NationalChatClient/src/app/features/chat/secret/`. Kripto qismi (`secret-crypto.ts`) Angular'ga bog'liq emas, kerak bo'lsa Web Worker'ga ko'chirish oson.

- **Handshake:** har qurilma har chat uchun yangi X25519 kalit juftini yaratadi. Maxfiy kalit `extractable: false`: JS uni ishlata oladi, lekin baytlarini o'qiy olmaydi.
- **Kalitlarni chiqarish:** `X25519(men, suhbatdosh)` → HKDF-SHA-256. Salt — `SHA-256("NationalChat secret chat v1")`, info — `nc-secret-v1|chatId|initiatorPub|participantPub`. Natija 64 bayt: ikki yo'nalish uchun ikki zanjir kaliti (tashabbuskor → qabul qiluvchi va teskarisi).
- **Ratchet:** har xabarda `messageKey = HMAC(chainKey, 0x01)`, `chainKey' = HMAC(chainKey, 0x02)`. Eski zanjir kaliti unutiladi (forward secrecy). Xom baytlar faqat bir lahza mavjud bo'ladi va darhol nollanadi.
- **Shifrlash:** AES-256-GCM. `additionalData` = `chatId|yo'nalish|seq`, yo'nalish `i` yoki `p`. Qabul qiluvchi yuboruvchining session id'sini bilmaydi, ikki qurilmali chatda yo'nalish bilan bog'lash unga teng. Blob: `[versiya=1][12 bayt iv][shifrlangan matn + teg]`. Ichida JSON: `{ v: 1, kind, text, replyToSeq, sentAt, ttl }`. `kind` qiymatlari 12-bo'limda.
- **Tartibsiz kelgan xabar:** 200 tagacha o'tkazib yuborilgan xabarning kaliti vaqtincha saqlanadi va bir marta ishlatiladi. Soxta xabar zanjirni siljitmaydi: holat faqat muvaffaqiyatli ochilgandan keyin saqlanadi.
- **Fingerprint:** `SHA-256("nc-secret-fingerprint-v1" | initiatorPub | participantPub)`. U 8 ta emoji va 6 ta 5 xonali raqam guruhi ko'rinishida chiqadi.
- **Saqlash:** IndexedDB (`nationalchat-secret-chats`): kalitlar, zanjir holati va ochilgan tarix. Zanjir holati va xabar bitta tranzaksiyada yoziladi. Logoutda va boshqa login qilinganda hammasi o'chiriladi.
- **Yetkazish:** yuboriladigan xabar avval qurilmada saqlanadi (outbox), keyin qat'iy tartibda yuboriladi. Qabul qilishda xabar faqat ochilgandan yoki tashlab yuborilgandan keyin `ack` qilinadi. Handshake tugamagan bo'lsa, xabar serverda qoladi.
- **Brauzer talabi:** Web Crypto'da X25519 (Chrome/Edge 133+, Firefox 130+, Safari 17+). Qo'llab-quvvatlanmasa `SecretChatService.supported()` false bo'ladi.

## 11. UI (3-bosqich natijasi)

- **Boshlash:** shaxsiy chat menyusida va suhbatdosh ma'lumot panelida "Maxfiy chat boshlash" tugmasi bor. Bu qurilmada o'sha odam bilan maxfiy chat allaqachon bo'lsa, o'shani ochadi.
- **Ro'yxat:** chap panelda "Maxfiy chatlar" bo'limi, har birida qulf belgisi. Kelgan so'rovda belgi (badge) chiqadi va bir marta xabarnoma ko'rsatiladi.
- **Chat oynasi** (`SecretConversationComponent`) oddiy suhbat oynasi o'rnida ochiladi:
  - kelgan so'rov (qabul qilish yoki rad etish) va javob kutish holatlari;
  - E2E haqida eslatma, ochilgan tarix, yuborilish holati (soat yoki ✓) va javob berish;
  - faqat matn yoziladigan maydon: Enter yuboradi, Shift+Enter yangi qator ochadi;
  - kalit tekshirish oynasi: 8 emoji va 6 guruh raqam;
  - tasdiq bilan chatni yopish.
- **Suhbatdosh chatni yopsa yoki logout qilsa:** chat ekrandan olib tashlanadi va "Maxfiy chat yopildi" xabari chiqadi.
- **Tekshiruv:** ikki haqiqiy brauzer foydalanuvchisi bilan (Chromium, haqiqiy backend) so'rov, qabul qilish, ikki tomonlama xabar, javob, bir xil fingerprint, qayta yuklashdan keyingi tarix, yopish va telefon kengligi sinab ko'rildi.

## 12. Taymerlar va tarixni tozalash (4-bosqich natijasi)

- **Protokol:** payload'da `kind` bor: `text` (xabar), `timer` (chat taymerini o'zgartirish) yoki `clear` (tarixni tozalash). `ttl` — soniyalarda, 1 soniyadan 1 haftagacha, yoki `null` (o'chiq). Boshqaruv xabarlari oddiy xabar kabi shifrlanadi, server ularni ajrata olmaydi.
- **Taymer:** har `text` xabari o'z `ttl`ini olib yuradi. Taymer o'zgarsa, ikki tomonda ham xizmat qatori ("Ali taymerni o'rnatdi…") chiqadi.
- **Hisoblash qachon boshlanadi:** yuboruvchida xabar yuborilganda. Qabul qiluvchida xabar birinchi marta ekranda, ochiq tabda ko'ringanda — ko'rilmagan xabar o'chib ketmaydi.
- **O'chirish:** muddati o'tgan xabarlar IndexedDB'dan butunlay o'chiriladi. Bu taymer bilan va har ishga tushishda bajariladi, sahifa qayta yuklanganda xabar qaytib kelmaydi. IndexedDB 2-versiyasida `expiresAt` indeksi bor, 1-versiyadan avtomatik yangilanadi.
- **Tarixni tozalash:** ikki qurilmadagi yozishma o'chiriladi, chat va kalitlar qoladi.
- **Tanlov:** o'chiq, 10 soniya, 1 daqiqa, 1 soat, 1 kun, 1 hafta.
- **Cheklov:** vebda skrinshotni to'sib bo'lmaydi va buni aniqlab ham bo'lmaydi.
