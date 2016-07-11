# d2 mod for OpenRA

1. Install OpenRA or build it from sources

2. Copy or link the following files from your main OpenRA installation into OpenRA.Mods.D2/dependencies directory:
- OpenRA.Game.exe
- OpenRA.Mods.Common.dll
- OpenRA.Mods.RA.dll
- OpenRA.Mods.TS.dll
- Eluant.dll

3. Run the following command in OpenRA.Mods.D2 directory for linux and mac. And open OpenRA.Mods.D2.sln solution in visual studio and build project for windows:
- make all

4. Create subdirectory named 'd2' in main OpenRA installation in directory 'mods'
- mkdir mods/d2

5. Copy everything to this newly created directory 'd2' from this reopository, including OpenRA.Mods.D2.dll. But exclude OpenRA.Mods.D2 directory from copying (this is sources of OpenRA.Mods.D2.dll)

6. Launch your OpenRA choose mod 'd2' in modchooser and play game.

[![Build Status](https://travis-ci.org/OpenRA/d2.svg?branch=master)](https://travis-ci.org/OpenRA/d2)

Consult the [wiki](https://github.com/OpenRA/d2/wiki) for instructions on how to install and use this.
