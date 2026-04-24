# 💰 CashFlow Telegram Bot

A cross-platform Telegram bot designed to simplify and automate gameplay for **CashFlow** — a financial education board game.  
The bot helps players track income, expenses, assets, and liabilities, making the game smoother and more engaging.

---

## 🎲 About the Game

**CashFlow 101** is a board game created to teach financial literacy, investing, and money management skills.

👉 https://en.wikipedia.org/wiki/Cashflow_101

![CashFlow Game Screenshot](./readme/cashflow.jpg)

---

## 🌍 Supported Languages

- 🇺🇦 Ukrainian
- 🇬🇧 English
- 🇩🇪 German

---

## 🚀 Installation

Download the appropriate archive from the [**Releases**](https://github.com/varajan/CashFlow/releases) section:

- [CashFlow-2.0.8-beta-linux-x64.zip](https://github.com/varajan/CashFlow/releases/download/v2.0.8-beta/CashFlow-2.0.8-beta-linux-x64.zip) → `CashFlow`
- [CashFlow-2.0.8-beta-osx-x64.zip](https://github.com/varajan/CashFlow/releases/download/v2.0.8-beta/CashFlow-2.0.8-beta-osx-x64.zip) → `CashFlow`
- [CashFlow-2.0.8-beta-win-x64.zip](https://github.com/varajan/CashFlow/releases/download/v2.0.8-beta/CashFlow-2.0.8-beta-win-x64.zip) → `CashFlow.exe`
- [CashFlow-2.0.8-beta-win-x86.zip](https://github.com/varajan/CashFlow/releases/download/v2.0.8-beta/CashFlow-2.0.8-beta-win-x86.zip) → `CashFlow.exe`

---

## 🔧 Bot Setup

1. Open Telegram and search for **BotFather**

2. Start a chat and run:

   ```
   /start
   ```

3. Create a new bot:

   ```
   /newbot
   ```

4. Follow the instructions:

   * Enter a name for your bot
   * Enter a unique username (must end with `bot`)

5. After creation, you will receive a **Bot Token**

6. Open the existing file:

   ```
   BotID.txt
   ```

7. Paste your token into this file (replace its contents):

   ```
   123456789:ABCdefGhIJKlmNoPQRsTUVwxyZ
   ```

8. Save the file and make sure it remains in the same directory as the application executable

---

## ▶️ How to Run

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
* Execute use-case scenarios

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
