# Unity UPM Package — Game Backend SDK

Unity SDK package for Rust-Game-Backend. Based on [StansAssets Unity-Package-Sample](https://github.com/StansAssets/Unity-Package-Sample) template.

## How to use
* Open `package.json` and update package metadata.

## Repository structure
* `init` - CLI init script.
* `.github` - GitHub Settings & Actions.
* `.gitignore` - Git ignore file designed to this specific repository structure.
* `README.md` - This file.
* `PackageSampleProject` - Shared Unity project for package development.
* `com.gamebackend.sdk` - UPM package.

## Package layout
The repository package layout follows [official Unity packages convention](https://docs.unity3d.com/Manual/cus-layout.html).

```
<root>
  ├── package.json
  ├── README.md
  ├── CHANGELOG.md
  ├── LICENSE.md
  ├── Editor
  │   ├── GameBackend.Editor.asmdef
  │   └── EditorExample.cs
  ├── Runtime
  │   ├── GameBackend.asmdef
  │   └── RuntimeExample.cs
  ├── Tests
  │   ├── Editor
  │   │   ├── GameBackend.Editor.Tests.asmdef
  │   │   └── EditorExampleTest.cs
  │   └── Runtime
  │        ├── GameBackend.Tests.asmdef
  │        └── RuntimeExampleTest.cs
  └── Documentation~
       └── com.gamebackend.sdk.md
```
