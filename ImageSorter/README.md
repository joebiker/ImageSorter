# ImageRename
Rename photos from multiple sources based on Picture Taken

Possible re-names of application 

**agora** - easy, quick. 
noun, (in ancient Greece) a public open space used for assemblies and markets.

**allegory** - longer, but more fun
noun, a story, poem, or picture that can be interpreted to reveal a hidden meaning, typically a moral or political one

## Build 

### Release build: 

> dotnet build -c Release

> dotnet build ImageSorter.csproj -c Release

# TOOD

Move prject into it's own subfolder.

> dotnet add reference ../ImageSorter/ImageSorter.csproj

png files need to be tested.

If Readme is not found, then don't process authors. 

add a "reset option" that will remove the prefix and author. Esentially reverting the changes. Maybe it can read the audit file to get the exact text. 

Bug: did not properly order the .mov files in: S:\Pictures\2025_07_19 Uncompahgre Peak with Margs
