# ReMarkable SleepScreen Manager

A modern **Windows (WPF, .NET 8)** application that allows you to easily change the sleep screen of your **reMarkable** tablet (Paper Pro).  

![Version](https://img.shields.io/badge/version-0.0.2-blue.svg)
![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey.svg)

---

## ✨ Features

### 🖼️ **Local Image Management**
- Simple SSH/SFTP connection to your tablet (root password)
- **Image preview** with automatic resizing
- **Auto-resize** to tablet resolution:
  - Paper Pro → `2160×1620` (portrait)
- Direct upload and application with UI restart
- **Restore** button to revert to the default bundled image

### 🔄 **Auto Rotation Mode**
- Install automatic sleep screen rotation
- Upload multiple PNG images to the tablet
- Random selection after deep sleep (~12 minutes)
- Easy installation/uninstallation with systemd hooks

### 🌐 **Online Gallery**
- Browse curated sleep screen collection
- Download and install directly from the gallery
- Community-contributed images
- Automatic resolution handling

### 🌍 **Internationalization**
- **English** and **Français** language support
- Easy language switching
- Localized interface and messages
- 📖 [Internationalization Guide](docs/INTERNATIONALIZATION.md)

---

## 🖼️ Screenshots

*Screenshots coming soon...*

---

## 🔧 Installation

### Prerequisites
1. **Enable Developer Mode** on your reMarkable:
   - Settings → General → Software version → Developer Mode → ON
   - Note the **IP address** (usually `10.11.99.1` via USB) and **root password**

### Quick Start
1. **Download** the latest release or compile from source
2. Run `RemarkableSleepScreenManager.exe`
3. Configure connection:
   - **IP Address**: `10.11.99.1`
   - **Username**: `root`
   - **Password**: displayed on your tablet
4. Click **Test** → connection should show `uname -a`
5. Choose an image and click **Upload & Apply**
6. Put your tablet to sleep → your new screen appears 🎉

---

## 🛠️ Development

### Prerequisites
- **Visual Studio 2022+** or **VS Code**
- **.NET 8 SDK**
- **Windows 10/11**

### Build from Source
```bash
git clone https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager.git
cd ReMarkable-Sleep-Screen-Manager
dotnet build
```

### Architecture
The application follows **MVVM pattern** with:
- **Models**: Data structures and business logic
- **ViewModels**: UI logic and data binding
- **Services**: SSH, image processing, gallery management
- **Localization**: Multi-language support with resource files

---

## 📁 Project Structure

```
├── Models/              # Data models and business logic
├── ViewModels/          # MVVM view models
├── Services/            # Business services (SSH, Image, Gallery)
├── Commands/            # Command pattern implementation
├── Localization/        # Internationalization support
├── Resources/           # Localized strings and assets
├── Assets/              # Application assets
└── docs/               # Documentation and gallery
```

---

## 🤝 Contributing

We welcome contributions to improve the project! Here's how you can help:

> 📋 **For detailed contribution guidelines, see [CONTRIBUTING.md](CONTRIBUTING.md)**

### 🐛 **Bug Reports & Feature Requests**
- **🐛 Bug Reports**: Use the [Bug Report Template](https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager/issues/new?template=bug_report.md)
- **💡 Feature Requests**: Use the [Feature Request Template](https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager/issues/new?template=feature_request.md)
- **💬 General Discussion**: [GitHub Discussions](https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager/discussions)

### 💻 **Code Contributions**
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a [Pull Request](https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager/compare) (template will be provided)

**Note**: All code changes are reviewed and approved by the project maintainer.

### 🖼️ **Gallery Images**
- **🖼️ Image Suggestions**: Use the [Gallery Suggestion Template](https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager/issues/new?template=gallery_suggestion.md)
- **Gallery images are managed by the project maintainer**
- Images must be high-quality, appropriate, and follow the project's style guidelines

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- **reMarkable** for creating amazing e-ink tablets
- **SSH.NET** for SSH/SFTP connectivity
- **Community contributors** for gallery images

---

## 📞 Support

- 🐛 **Bug Reports**: [GitHub Issues](https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager/issues)
- 💡 **Feature Requests**: [GitHub Discussions](https://github.com/roropastis/ReMarkable-Sleep-Screen-Manager/discussions)
- 📧 **Contact**: [bigscorestudio@gmail.com]

---

**Buy me a coffee [https://ko-fi.com/bigscorestudio]**

**Made with ❤️ in Switzerland for the reMarkable community**
