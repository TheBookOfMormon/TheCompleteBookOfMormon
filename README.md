# TheCompleteBookOfMormon

1. Install Tesseract https://github.com/tesseract-ocr/tesseract/releases
1. Install GIT for windows
1. Download WinGet - https://learn.microsoft.com/en-us/windows/package-manager/winget/download
1. `winget install ezwinports.make`
1. `winget install wget`
1. `git clone https://github.com/tesseract-ocr/tesstrain.git`
1. Create a directory in tesstrain named `Data`
1. Create a sub directory `1830PalmyraEdition-ground-truth`
1. Copy OCR training data *.tif and *.gt.txt to that folder
1. Make a sibling directory of tesstrain called tessdata
1. Copy eng.traineddata into it
1. Open GIT Bash
1. CD to data directory
1. ./trainocr.sh

Models will be trained and the traineddata copied to the correct folder.