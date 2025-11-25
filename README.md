# Batteryless Save Patcher & Repacker for GBA Pokémon games

**Based on AliExpress Bootleg Patch**

A week ago I said “let’s put a few of my favorites hackroms on bootleg cartridges”, but then I found out about.. well, everything about SRAM batteryless patches etc.  
I patched the games with gbata, and tried to use the batteryless patches I found… some almost worked, other didn’t work at all.  
But the bootleg cartridges I had? They worked perfectly. So I dumped them, compared them with the original ROMs, and figured out exactly how they were patched.

And here we are with the result

---

## What this tool does:

This tool is a fully automated and relocatable batteryless save patcher for Pokémon GBA games, based on the patch reverse-engineered from the bootleg Pokémon cartridges sold on AliExpress, it:

1. **(OPTIONAL, ONLY IF NEEDED BECAUSE OF FREE SPACE OR FORCED)** Repacks the ROM by moving all the LZ blocks and repoints them, in order to make free space at the end of the ROM to have enough to put the blob and the save area.  
   **WARNING!!! on heavily modified ROMs, relocating could break something, since LZ blocks detection isn't 100% reliable (even though in my experience it actually fixed a ROM which had broken images by relocating them to correct offsets)**  
2. Applies SRAM patches, no need to use external tools like GBATA  
3. Applies the same patches found on the Aliexpress cartridges  
4. Copies the patch blob found on the Aliexpress cartridges  
5. Repoints all the references to that relocated blob  
6. Edits that blob to relocate the save area to wherever there is free space on the ROM

---

## Tested Working On (as of now):

🟢 Pokémon Ruby based ROMS (Tried with both USA and localized versions)  

🟢 Pokémon Sapphire based ROMS (Tried with both USA and localized versions)  

🟢 Pokémon FireRed / LeafGreen based ROMS (Tried with both USA and localized versions)

---

## Issues:

🟡 Some heavily modified HACK Roms have problems with repacking. It may have something to do with some data (scripts?) placed far in the game data and incorrectly being identified as LZ blocks.  
Not sure yet, but as of now I've observed this behavior only on 32mb roms, which cannot be patched anyway since as far as I know (correct me if I'm wrong) bootleg cartrides use addresses `0x09000000` to write on the SRAM so they wouldn't work anyway.  
But, If by repacking the size went down to 16mb (doubt it, anyway), they could theoretically work.

---


Keep in mind that as of now the patched ROMS haven’t been texted extensively (but I’ve tested first save, multiple save overwrites etc)
