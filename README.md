#Diplom ishi


## Tashkilotlar (Organizations)

Hech narsa sozlanmaydi. Foydalanuvchi pochtasini tasdiqlab kirganda (OTP yoki Google):

- `@` belgisidan keyingi butun domen tashkilot hisoblanadi: `ali@tuit.uz` → `tuit.uz`,
  `vali@student.tuit.uz` → `student.tuit.uz` (alohida tashkilot). Umumiy pochta xizmatlari (gmail.com,
  mail.ru, umail.uz va boshqalar) tashkilot emas;
- shu domendan birinchi kirgan odam tashkilot **admini** bo'ladi va nomi domen bilan bir xil bo'lgan
  (`tuit.uz`) **yopiq guruh** ochiladi, uning egasi — shu odam;
- keyin shu domendan kirganlar guruhga o'zi qo'shiladi;
- boshqalar guruhni topa olmaydi va o'zi qo'shila olmaydi: ularni faqat admin qo'shadi yoki ular admin
  bergan **taklif havolasi** orqali kiradi (`POST /api/groups/{chatId}/invite-link`,
  `POST /api/groups/invites/{token}/join`). Havola qayta yaratilsa yoki o'chirilsa, eskisi ishlamaydi.

## Web klient manzillari (CORS va refresh cookie)

Web klient API'dan boshqa saytda ishlasa (masalan klient `*.vercel.app`, API `*.onrender.com`), refresh cookie faqat
klient manzillari sozlanganda ishlaydi. Aks holda foydalanuvchi access token muddati tugashi (15 daqiqa) bilan qayta
kirishi kerak bo'ladi va API ishga tushganda ogohlantirish yozadi.

```
Cors__AllowedOrigins__0=https://<production-klient-manzili>
Cors__AllowedOrigins__1=https://national-chat-client-*-sardors-projects-56e94522.vercel.app
```

- Sozlanganda faqat shu manzillar API'ni cookie bilan chaqira oladi va refresh cookie `SameSite=None; Secure` bo'ladi.
- `*` faqat bitta domen qismi ichida ishlaydi (nuqtani o'z ichiga olmaydi), shuning uchun boshqa domenni qamrab olmaydi.
- Sozlanmasa, avvalgidek hamma manzilga ruxsat beriladi, cookie esa `SameSite=Strict` bo'lib qoladi. Boshqa sayt API'ni
  chaqira oladi, lekin refresh tokenni o'qiy olmaydi.
- `Development` muhitida cookie har doim `SameSite=Strict` (localhost'da klient va API bitta sayt hisoblanadi).
