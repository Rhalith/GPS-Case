# GPS-Case — Unity GPS Mini-Map & Movement Simulation

This project was developed as part of the **CodeMiner Technical Assessment Task**, simulating character movement in a 3D environment based on GPS coordinates. It includes a mini-map overlay, FPS camera mode, WebGL deployment, and mobile testing.

---

## ✨ Features
- **3D Model Integration**: 1:1 scaled real-world area (10,000 m²).
- **Character Simulation**: 180 cm tall character updated every 3 seconds from GPS data.
- **Mini-Map Overlay**:
  - Current Coordinates (live, per-frame updates).
  - Last Updated Coordinates (refreshed every 3 seconds).
- **FPS Camera**: immersive first-person view.
- **Reset Location Button**: snaps character back to the recorded start position.
- **Starting Position Handling**: first GPS coordinate at launch is stored as spawn point.
- **Location Permissions**: requested on mobile app start (iOS & Android).
- **Accuracy Filtering**: jitter handling with thresholds and smoothing.
- **Optional Path Snap System**: aligns character to nearest path when accuracy is low.
- **WebGL Deployment**: tested and hosted on itch.io.

---

## 📁 Project Scope
From the CodeMiner task document:
- Verify coordinates & display in UI.
- Character updates every 3 seconds from GPS.
- Correct movement simulation in FPS mode.
- WebGL build deployed on itch.io.
- Bonus: accuracy handling & snapping, creative improvements.

---

## 🧩 Tech Stack
- **Unity**: 2022.3.62f1 (LTS)
- **Language**: C# (LTS compatible)
- **Platforms**: Android, iOS, WebGL
- **Architecture**: SOLID, composition-first, dependency injection
- **Config**: ScriptableObjects for tunable values

---

## 🚀 Getting Started
### Requirements
- Unity **2022.3.62f1**
- Android SDK/NDK (via Unity Hub) and/or Xcode (iOS)

### Clone & Open
```bash
git clone https://github.com/Rhalith/GPS-Case.git
cd GPS-Case
# Open with Unity 2022.3.62f1
```

### Run in Editor
1. Open **Scenes/Main.unity**.
2. Press **Play**.
   - Editor: mock GPS provider.
   - Device: live GPS input.

---

## 📱 Mobile Build
### Android
- Switch platform in Build Settings → Android.
- IL2CPP scripting backend, ARM64 architecture.
- Configure package name & keystore.
- Build APK/AAB.

### iOS
- Switch platform to iOS and build.
- Open Xcode project, configure signing.
- Add in Info.plist:
  - `NSLocationWhenInUseUsageDescription` = "App needs location to simulate movement."

### WebGL
- Build with Gzip/Brotli compression.
- Host on itch.io with correct MIME types.

---

## ⚙️ Configuration
**GpsSettings** (ScriptableObject):
- Accuracy Threshold (m): 10–25
- Jitter Cutoff (m): 3–8
- Update Interval: 3s
- Smoothing Window: 3–5 samples

**MovementSettings**:
- Character Height: 1.8 m
- Max Speed: 5 m/s

---

## 🧠 Architecture
- No public fields: `[SerializeField] private` + properties.
- No `Find*` APIs: use serialized refs or `[RequireComponent]` caching.
- Events/coroutines > heavy Update loops.
- Interfaces for services (time, GPS provider) → testability.

**Core Components**:
- `GpsService`: wraps `Input.location`, permissions.
- `GpsFilter`: accuracy + smoothing.
- `PlayerController`: updates character transform.
- `MiniMapView`: mini-map overlay + heading.
- `CoordinateHud`: coordinate UI labels.
- `CompositionRoot`: scene-level wiring.

---

## 🧪 Testing
- **EditMode**: GPS math, smoothing, thresholds.
- **PlayMode**: mobile permissions, coordinate updates.
- **Mock GPS Provider**: used in Editor.

---

## 🔧 Troubleshooting
- **Character jitter while stationary**: increase jitter cutoff.
- **No GPS updates**: check permissions (Info.plist/AndroidManifest).
- **WebGL build errors**: confirm server MIME setup.

---

## 📷 Demo
- WebGL Build: *https://rhalith.itch.io/gps-case*
