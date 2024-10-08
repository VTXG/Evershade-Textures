<h1 align="center">
  <img src=".github/images/img_logo.png" alt="Evershade Textures" width="300">
  <br>
  <b>Evershade Textures</b>
</h1>

<p align="center">
  <b>Evershade Textures</b> is a texture importer and exporter for Luigi's Mansion 2 HD by VTXG.
</p>

<br>

> [!WARNING]
> With the release of [Evershade Editor](https://github.com/Gadd-Modding-Inc/Evershade-Editor), this tool is now obsolete. Use it instead of this.

# How to Use

### 0. Notes

You need [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) to run this application.

Imported textures need to:
- have the same byte size as the original texture.
- have the same compression format. <br>

### 1. Opening and exporting a texture

Start by opening a **LM2HD Data file**. After that, select any texture and export it.<br><br>
<img src=".github/images/img_1.png" alt="Example 1"><br><br>
See those values below the Texture Preview? They will be important for the next step.<br><br>

### 2. Editing and formatting a texture

Open and edit the DDS texture in any image editing software, and export it as a PNG image.<br>
Next, you will need [NVIDIA Texture Tools Exporter](https://developer.nvidia.com/texture-tools-exporter) to format your texture.<br><br>
<img src=".github/images/img_2.png" alt="Example 2"><br>
*TIP: Generally, textures with no transparency use DTX1 (BC1), and textures with transparency use DTX5 (BC3).*<br><br>
After setting the values, export the texture as a DDS.<br><br>

### 3. Importing a texture

After that, click the import button and locate your texture.<br>
If you did everything correctly, it should work. You can now save the DATA file and test your mod!<br><br>

# Plans

- Settings (Light mode, NVTT location, etc.)
- Auto Import
- Texture Dimension Modification
