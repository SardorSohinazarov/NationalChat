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
