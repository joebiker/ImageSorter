# Allegory
Rename photos in chronological order. This will assist when photos come from multiple sources (authors). Will include the author name in the file rename.

Possible re-names of application 

**agora** - easy, quick. 
noun, (in ancient Greece) a public open space used for assemblies and markets.

**allegory** - longer, but more fun
noun, a story, poem, or picture that can be interpreted to reveal a hidden meaning, typically a moral or political one

## Usage

no parameters
Creates a _Readme.txt file in the folder containing photos.

`-audit` 
Creates a CSV extracting dates and filename ordering output. Don't make filename changes.

`-go`
Make filename changes.

`-undo`
Will restore file name to original. This removes the ordering number prefix and author postfix.


## _Readme.txt

This file is a configuration for authors of the photos along with various settings.

## Testing

1) Start with -audit to see if all the CSV data is included and correct.
2) Make the renames with -go
3) Undo the renames wtih -undo

## Development

Magick.NET-Q8-x64 - used for EXIF extraction
MetadataExtractor - for QuickTime Date extraction

## Build 

### Release build: 

> dotnet build -c Release

> dotnet build Allegory.csproj -c Release

# TOOD

Bug: Mexico videos have odd dates, needs further review.

Start program, shoul identify how many 'user' images there are.