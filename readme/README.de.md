
---

# 📄 `README.de.md` (German)

```markdown
# 💰 CashFlow Telegram Bot

🇬🇧 [English](../README.md) | 🇺🇦 [Українська](README.ua.md) | 🇩🇪 Deutsch

Ein plattformübergreifender Telegram-Bot zur Automatisierung des Spiels **CashFlow**.

---

## 🎲 Über das Spiel

**CashFlow 101** ist ein Brettspiel zur Vermittlung von Finanzwissen und Investitionsgrundlagen.

👉 [Wiki](https://en.wikipedia.org/wiki/Cashflow_101)

👉 [Cashflow-Clubs Deutschland](https://blog.cashflowclub-magdeburg.de/cash-flow-clubs-deutschland/)

![CashFlow](./readme/cashflow.jpg)

---

## 🌍 Unterstützte Sprachen

- 🇺🇦 Ukrainisch  
- 🇬🇧 Englisch  
- 🇩🇪 Deutsch  

---

## 🚀 Installation

Lade die passende Datei aus dem [**Releases**](https://github.com/varajan/CashFlow/releases)-Bereich herunter:

- `CashFlow*.linux-x64.zip`
- `CashFlow*.osx-x64.zip`
- `CashFlow*.win-x64.zip`
- `CashFlow*.win-x86.zip`

---

## 🔧 Bot Einrichtung

1. Telegram öffnen → **BotFather**

2. Ausführen:

   ```
   /start
   ```

3. Bot erstellen:

   ```
   /newbot
   ```

4. Anweisungen folgen

5. Token kopieren

6. Datei öffnen:

   ```
   BotID.txt
   ```

7. Token einfügen

   ```
   123456789:ABCdefGhIJKlmNoPQRsTUVwxyZ
   ```

---

## ▶️ Start

### 🐧 Linux

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

---

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

## 🏗️ Tech Stack

* **.NET 8.0**
* **SQLite** (lightweight embedded database)
* **NetTelegramBotApi** (Telegram integration)

---

## 🧪 Testing

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

## 📄 License

MIT License
