# 🌟 StormPaws: PvP Battle Game

**StormPaws** is a turn-based **PvP animal battle game** built with **Unity**.  
Players build a deck of animal cards and simulate battles against opponents with **dynamic weather and city environments**.  
The game supports both **online server-based battles** and **AI simulations** that can simulate based on weather selection, with a **battle history system**.
Player can enjoy a **luck-based strategy game** with bonus multipliers that vary for each animal depending on the random weather.

---

## 🧩 Features

- 🎴 **Deck-building** system with animal cards
- ⚔️ **Turn-based battle** client/server support
- ☁️ **Random environment simulation** weather + city
- 📜 **Battle result display** with card previews and timestamps
- 📊 **Battle history records** fetched from server
- 🧠 **AI battle mode** for simulation based on weather

---

## 🛠️ Technologies Used

- Unity (C#)
- REST API: `UnityWebRequest`
- JSON serialization: `JsonUtility`
- Modular architecture: `BattleService`, `GameManager`, etc.
- UI system using `TextMeshPro (TMP_Text)` and `UnityEngine.UI`

---

## ✅ How It Works

### 🔹 Deck Selection  
- Player selects their deck and opponent's deck.  
- Deck IDs are saved in `PlayerPrefs`.

### 🔹 Environment Fetch  
- Weather and city are fetched from `/weather/random` API  
  *(or selected manually in simulation mode)*

### 🔹 Battle Simulation  
- `POST /battles/pvp` returns a list of `BattleLog` items.  
- Characters attack in order based on log timestamps.

### 🔹 Damage Handling  
- `TriggerAttack()` updates HP  
- Displays floating damage text  
- Checks for character death

### 🔹 Result Screen  
- After battle ends, result screen shows:  
  - **Result** (`WIN!` or `Lose...`)  
  - **Weather + City**  
  - **Battle duration**  
  - **Card previews of each deck**

---

## ✅ Screen Shot

