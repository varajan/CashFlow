
---

# 📄 `README.ua.md` (Ukrainian)

```markdown
# 💰 CashFlow Telegram Bot

🇬🇧 [English](../README.md) | 🇺🇦 Українська | 🇩🇪 [Deutsch](README.de.md)

Кросплатформенний Telegram-бот для автоматизації гри **CashFlow** — настільної гри про фінансову грамотність.

---

## 🎲 Про гру

**CashFlow 101** — гра, яка навчає фінансовому мисленню, інвестуванню та управлінню грошима.

👉 https://en.wikipedia.org/wiki/Cashflow_101

![CashFlow](./readme/cashflow.jpg)

---

## 🌍 Підтримувані мови

- 🇺🇦 Українська  
- 🇬🇧 Англійська  
- 🇩🇪 Німецька  

---

## 🚀 Встановлення

Завантажте відповідний архів із розділу [**Releases**](https://github.com/varajan/CashFlow/releases):

- `CashFlow*.linux-x64.zip`
- `CashFlow*.osx-x64.zip`
- `CashFlow*.win-x64.zip`
- `CashFlow*.win-x86.zip`

---

## 🔧 Налаштування бота

1. Відкрити Telegram → **BotFather**

2. Виконати:

   ```
   /start
   ```

3. Створити бота:

   ```
   /newbot
   ```

4. Вказати ім'я та username (має закінчуватись на `bot`)

5. Скопіювати токен

6. Відкрити файл:

   ```
   BotID.txt
   ```

7. Вставити токен (замінити вміст)

   ```
   123456789:ABCdefGhIJKlmNoPQRsTUVwxyZ
   ```

8. Зберегти файл у папці з програмою

---

## ▶️ Запуск

### Linux / macOS


```bash
chmod +x CashFlow
./CashFlow
```

---

### 🍏 macOS

```bash
chmod +x CashFlow
./CashFlow
```

If you see a warning about an unidentified developer:

1. Open **System Settings → Privacy & Security**
2. Scroll down and allow the app to run
3. Re-run the application

### 🪟 Windows

Simply run:

```
CashFlow.exe
```

If SmartScreen blocks the app:

1. Click **More info**
2. Click **Run anyway**

---

## 🤖 Features

* 📊 Player financial tracking
* 💰 Automatic cashflow calculation
* 🏦 Asset and liability management
* 👥 Multiplayer game support
* 🌐 Multi-language interface
* ⚡ Telegram-based interaction

---

## 🏗️ Технології

* **.NET 8.0**
* **SQLite** (lightweight embedded database)
* **NetTelegramBotApi** (Telegram integration)

---

## 🧪 Тестування

The project includes a comprehensive testing setup:

### Unit Tests

* Built with **NUnit**
* Cover core business logic and calculations

### System Tests

* Built with **SpecFlow**
* Run against a **bot emulator**
* Execute real-world gameplay scenarios

---

## 📁 Project Structure

```
.
├── CashFlow/
├── CashFlowBot/
├── CashFlowBotEmulator/
├── CashFlowBotSystemTests/
├── CashFlowUnitTests/
└── README.md
```

---

## 🔧 Development Notes

* Designed as a cross-platform standalone application
* No external database setup required (uses SQLite)
* Easily extendable for new game rules or features
* Focused on testability and clean architecture

---

## 🤝 Contributing

Contributions are welcome!
Feel free to open issues or submit pull requests.

---

## 📄 Ліцензія

MIT License
