# DocBookKeeping

A modern desktop application built with **Avalonia UI** and **.NET 10** for efficient document handling and bookkeeping management.

## 🚀 Features
* **Cross-Platform Support:** Runs seamlessly on Windows, macOS, and Linux thanks to Avalonia UI.
* **Modern MVVM Architecture:** Built with clean separation of concerns using Models, Views, and ViewModels.
* **Robust Data Access:** Organized data persistence layers and models tailored for financial/document tracking.
* **Custom Converters:** Built-in XAML UI value converters to handle specialized data-binding transformations.

---

## 📂 Project Structure

The project strictly follows the standard Avalonia MVVM pattern:

```text
DocBookKeeping/
├── Assets/          # Icons, images, and static application assets
├── Converters/      # UI Value Converters for XAML bindings
├── Data/            # Database contexts, repositories, or local file storage logic
├── Models/          # Core business entities and data structures
├── Services/        # Business logic, helpers, and API/File handlers
├── ViewModels/      # Application state and UI logic handlers
└── Views/           # XAML files defining the User Interface layouts
```

---

## 🛠️ Prerequisites

Before you begin, ensure you have the following installed on your local machine:
* [**.NET 10 SDK**](https://microsoft.com) (Required)
* An IDE with Avalonia support: [Visual Studio 2022](https://microsoft.com), [JetBrains Rider](https://jetbrains.com), or [VS Code](https://visualstudio.com) (with Avalonia Extension).

---

## ⚡ Getting Started

Follow these steps to get a local copy up and running:

### 1. Clone the Repository
```bash
git clone https://github.com
cd DocBookKeeping
```

### 2. Restore Dependencies
Navigate into the main project directory and restore the NuGet packages:
```bash
dotnet restore
```

### 3. Run the Application
Execute the app using the .NET CLI:
```bash
dotnet run --project DocBookKeeping.csproj
```

---

## 🛠️ Built With

* **[.NET 10](https://microsoft.com)** - The development platform.
* **[Avalonia UI](https://avaloniaui.net)** - The cross-platform XAML-based UI framework.

---

## 📄 License

This project is open-source. Feel free to use, modify, and distribute it as needed.
