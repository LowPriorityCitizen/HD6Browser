# HD6Browser

HD6Browser is a .NET 8 Windows Forms application for browsing HD6 archive indexes and extracting files from their paired DAT files.

## Features

- Opens an HD6 archive and its paired DAT file.
- Displays the archive hierarchy in a tree view.
- Displays the selected folder contents in a details list view.
- Shows file names, sizes, and DAT offsets.
- Supports file and folder icons from Windows.
- Supports multi-selection in the list view.
- Opens folders by:
  - Double-clicking a folder.
  - Selecting a folder and pressing **Enter**.
- Moves to the parent folder by:
  - Double-clicking the `...` entry.
  - Selecting the `...` entry and pressing **Enter**.
  - Pressing **Backspace**.
- Extracts selected files and folders recursively.
- Provides two extraction modes:
  - **Extract Selected (Archive Root)** — preserves each entry's archive path.
  - **Extract Selected (Current Folder)** — uses the folder currently shown in the list view as the base path.
- Validates archive paths and DAT ranges before writing files.

## User interface

The main window is divided into two panes:

- **Left pane:** archive tree.
- **Right pane:** folder contents list.

The top of the right pane contains the extraction buttons. Extraction is disabled when there is no valid DAT file or no extractable selection.

The list view contains these columns:

- Filename
- Size
- Offset

## HD6 and DAT files

The application expects the DAT file to be beside the HD6 file and to use the same base name. For example:

```text
archive.HD6
archive.DAT
```

The HD6 parser reads the archive metadata, decodes file names using code page 932 (Shift-JIS), and produces entries containing:

- Archive path
- DAT byte offset
- File size

The DAT file contains the actual file data. The HD6 file provides the metadata required to locate each file inside the DAT file.

## Building

Requirements:

- Windows
- Visual Studio 2026 or a compatible .NET 8 SDK
- Windows Forms support

Build the solution in Visual Studio or run the project using the standard .NET 8 Windows Forms configuration.

The project file is:

```text
HD6Browser/HD6Browser.csproj
```

The project targets:

```xml
net8.0-windows
```

## Running

1. Build and start the application.
2. Select **Open HD6...**.
3. Choose an `.HD6` file.
4. Browse folders using the tree view or list view.
5. Select files or folders in the list view.
6. Choose one of the extraction modes.
7. Select an output directory.

If the DAT file is missing, the HD6 index can still be browsed, but extraction is unavailable.

## Project structure

```text
HD6Browser.slnx
└── HD6Browser/
	├── Browser.cs
	├── Browser.Designer.cs
	├── Browser.resx
	├── HD6Parser.cs
	├── Program.cs
	└── HD6Browser.csproj
```

### `Program.cs`

Initializes Windows Forms and starts the `Browser` form.

### `Browser.cs`

Contains the current user-interface behavior, archive tree construction, list view navigation, Windows icon loading, selection handling, and extraction logic.

### `Browser.Designer.cs`

Contains the Windows Forms controls and layout.

### `HD6Parser.cs`

Parses HD6 headers, dictionaries, filename tables, and packed entry metadata.

## Current limitations

- Only one HD6 archive can currently be open at a time.
- The Open dialog currently selects one HD6 file at a time.
- Extraction is synchronous and may temporarily block the UI for large archives.
- There is no automated test project.
- The parser assumes the HD6 format metadata is valid and has limited malformed-file diagnostics.

## License

No license has been specified for this repository yet.
