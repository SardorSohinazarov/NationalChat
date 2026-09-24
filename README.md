#Diplom ishi


## Tashkilotlar (Organizations)

Tashkilotlar admin paneli orqali emas, konfiguratsiyadan olinadi. Ilova ishga tushganda `OrganizationSync`
ularni bazaga yozadi. Umumiy pochta domenlari (gmail.com, mail.ru, yandex.ru va boshqalar) rad etiladi va
bu logga yoziladi.

`appsettings.json`:

```json
"Organizations": [
  {
    "Name": "Muhammad al-Xorazmiy nomidagi TATU",
    "ShortName": "TATU",
    "Domains": ["tuit.uz", "student.tuit.uz"],
    "AdminEmails": ["rektor@tuit.uz"]
  }
]
```

Muhit o'zgaruvchilari orqali:

```
Organizations__0__Name=Muhammad al-Xorazmiy nomidagi TATU
Organizations__0__ShortName=TATU
Organizations__0__Domains__0=tuit.uz
Organizations__0__Domains__1=student.tuit.uz
Organizations__0__AdminEmails__0=rektor@tuit.uz
```
