# Input data
I have supplied a file named All.zip
This zip file contains multiple embedded zip files, for example
   - 1830PalmyraEdition.zip
   - 1837KirtlandEdition.zip
   - 2013SaltLakeCityEdition.zip
Each of these embedded zip files is an edition of the same book.
The filename of each embedded zip is a 4 digit year, then the name of the place (Liverpool, NewYork, etc) followed by the word "Edition" or "Manuscript"
The edition name is the 4 digit year + name of the place. You can drop "Edition.zip" and "Manuscript.zip"
Each of the embedded zip files contains multiple files that have a filename of 3 digits + "PageJson" extension

Each ZIP contains files named `{PageNumber}.PageJson` (where the page number is three digits, e.g. `001.PageJson`).

Each is a JSON file with this structure:
```json
{
  "Words": [
    null,
    {
      "Elements": [
        { "Text": "example" }
      ]
    },
    ...
  ]
}
```

## Additional data
The following list is a collection of names. 
```
Aaron, Abinadi, Abinadom, Abish, Abraham, Adam, Aha, Ahah, Akish, Alma, Amaleki, Amalekite, Amalekites,
Amalickiah, Amalickiahites, Amaron, Aminadab, Aminadi, Amlici, Amlicite, Amlicites, Ammah, Ammaron,
Ammon, Ammonihah, Ammonihahites, Ammonihahits, Ammonite, Ammorite, Ammoron, Amnigaddah, Amnihu, Amnor,
Amoron, Amos, Amulek, Amulon, Amulonites, Anathoth, Angelah, Angola, Anti-anti, Anti-Nephi-Lehi,
Anti-Nephi-Lehies, Antiomno, Antionah, Antionum, Antiparah, Antipas, Antipus, Assyria, Assyrian, Babylon,
Babylonian, Babylonians, Benjamin, Boaz, Bountiful, Camorah, Carchemish, Cezoram, Christ, Christian,
Christians, Cohor, Com, Corianton, Coriantor, Coriantum, Coriantumr, Corihor, Corom, Corum, Cumeni,
Cumenihah, Cumorah, Desolation, Devil, Egypt, Egyptians, Emer, Enos, Ephraim, Esrom, Ethem, Ether,
Gaddianton, Gadianton, Gadiomnah , Galilee, Gallim, Gazelem, Geba, Gid, Giddianhi, Gideon, Gidgiddonah,
Gidgiddoni, Gilgah, Gimgimno, Hearthom, Helaman, Heth, Himni, Isabel, Ishmael, Ishmaelite, Ishmaelites,
Israel, Jacob, Jacobite, Jacobites, Jacom, Jared, Jarom, Jeberechiah, Jeneum, Jeremiah, Jershon,
Jerurusalem, Jerusalem, Jesus, John, Joseph, Josephite, Josephites, Jotham, Judah, Kib, Kim, Kimnor,
Kingmen, Kish, Kishkumen, Korihor, Laban, Lachoneus, Laman, Lamanite, Lamanites, Lamoni, Lebanon, Lehi,
Lehi-Nephi, Lehonti, Lemuel, Lemuelite, Lemuelites, Levi, Liahona, Lib, Limhah, Limher, Limhi, Luke,
Maher-shalal-hash-baz, Malachi, Mammon, Manasseh, Manti, Mark, Matt, Matthew, Melchizedek, Melek, Michmash,
Middoni, Mocum, Moriancumer, Morianton, Mormon, Moron, Moroni, Moronihah, Moses, Mosiah, Mulek, Mulekite,
Nahom, Naphtali, Nazareth, Nehor, Nehorite, Nehors, Nephi, Nephihah, Nephite, Nephites, Nimrah, Noah, Omer,
Omner, Omni, Onidah, Onihah, Orihah, Paanchi, Pachus, Pacumeni, Pahoran, Pharaoh, Rabbanah, Raca, Ramah,
Ramath, Remalia, Remaliah, Rezin, Riplakish, Ripliancum, Salem, Satan, Sam, Samaria, Samuel, Sariah, Seantum,
Seeric, Seezoram, Seth, Sherrizah, Shez, Shemlon, Shiblom, Shiblon, Shimnilom, Shimnilon, Shiz, Shule, Sidom,
Sidon, Sinim, Solomon, Teancum, Teomner, Tubaloth, Zarahemla, Zebulun, Zedekiah, Zeezrom, Zeezromite,
Zemnarihah, Zenephi, Zeniff, Zenoch, Zenock, Zenos, Zerahemla, Zerahemna, Zerahemnah, Zeram, Zilpah, Zion,
Zoram, Zoramite, Zoramites,
```

   - If a word is one of these words (case insensitive) then it is a name
   - If a word is one of these words (case insensitive) with a `'` or `'s` appended, then it is a name

## Precondition

The PageJson files are manually aligned from a known anchor point:
- Start processing from 001.PageJson in each file and continue processing the files alphabetically
- Word alignment is global across all files; do not reset state per file.
- Word positions are matched 1:1 across all versions.
- Missing words are explicitly represented as `null`.

## Processing Logic

- **"Words"** is an array. Each item is either:
  - `null` (meaning an empty word), or
  - an object with an **"Elements"** array.

- Each **Element** has:
  - a `"Text"` field (string).

### Combining Elements to form a word

- If a **Word** has **1 element**, its text is simply the `Text` of that element.

- If a **Word** has **3 elements**:
  - If `ShowDashes == true`:
    - Its text is the concatenation of the Text property of all **three elements** in order.
  - Otherwise:
    - Its text is the concatenation of the Text property of the **first and third elements only** (ignoring the second element).

### Additional Notes

- Words are **aligned across editions**. The index in one edition’s array matches the same index in another edition’s array.
- If a Word is `null`, treat it as an empty string.

### Comparison Logic

   - Sort filenames in all zips.
   - Starting from the sync point, extract and flatten the `"Words"` array into a single list per version.
   - Retain `null` entries to preserve position.
   - Truncate both lists to the length of the shorter one.

# Task 1 - Compute book hierarchy

## Additional processing
For this task only, When processing, replace any word containing `-` with an **empty string**.

## Compare Manually Aligned PageJson Files with Controlled Case Sensitivity


# Task 1 - Determine hierachy
1. For each edition, combine the words together into an array as described above.
2. Then scan the array, and any string with a `-` in it should be replaced with an empty string.
3. Compare all editions and produce a **2D table** with:

- **First column:** "Edition" (the year and place name)
- **Then a column for each edition chronological order.**  
- **Then a "Base" column.**

Note, when 2 editions have the same year, use the place name as a secondary sorting order

4. Each row represents an edition (ordered by year then place name).

5. For each cell:
   - If `row.Year <= column.Year`, leave it blank.
   - Otherwise, show a **percentage (to 2 decimal places)** indicating how similar the row edition is to the column edition.

6. For the final **"Base"** column, indicate the year of the edition the row is most similar to (and therefore most likely to be based off).

---

## Deliverable

Produce the **2D table** as described above.

And also an ascii tree in this format
```
1830 Palymyra
└── 1837 Kirtland
    ├── 1840 Nauvoo
    │   └── 1842 Nauvoo
    └── 1841 Liverpool
        └── 1849 Liverpool
```

# Task 2 - details of changes

This task will use the output of the previous task to determine which editions to compare with each other.
If the output of the previous task was as follows
```
1830 Palymyra
└── 1837 Kirtland
    ├── 1840 Nauvoo
    │   └── 1842 Nauvoo
    └── 1841 Liverpool
        └── 1849 Liverpool
```
Then I would expect the following output files
   - 1830Palmyra-vs-1837Kirtland.csv
   - 1837Kirtland-vs-1840Nauvoo.csv
   - 1837Kirtland-vs-1841Liverpool.csv
   - 1840Nauvoo-vs-1842Nauvoo.csv
   - 1841Liverpool-cs-1849Liverpool.csv

## Rule
1858NewYork is based on 1840Nauvoo despite being more similar to 1842Nauvoo.
Instead of comparing 1842Nauvoo->1848NewYork, compare 1840Nauvoo->1858NewYork

## Word Comparison Rules
   - **Case-insensitive** comparison is used for general words.
   - **Case-sensitive** comparison is used **only** for Names, and also words that begin with any of the following:

```
Judge, Heaven, House, Church, Chief, Gospel, Scripture,
Kingdom, Captain, Captain, Book, Priest, High, Prophet,
Holy, Lawyer, Bible, Saviour, Savior, Satan, Angel,
Governor, Saint, Sea, Son, Red, Father, North, East, South, West,
Christian
```

## Phrase Handling
   - When a word is a Name, then it is considered only as a single word
   - When a word is not a name then
      - Consecutive differing words are grouped into phrases.
      - Phrases that contain `<null>` are treated specially:
        - If all are `<null>`, both sides collapse to `<null>`.
        - Mixed `<null>` and text removes `<null>` entries from the phrase.
        - If a word has a `-` in it and the following word is `<null`> then you should combine the words' texts in both files
            For example, the words here should be "sea-shore" and "sea shore"
                | {filename1} Text | {filename2} Text |
                | sea-shore        | <null>           |
                | sea              | shore            |

## Context Extraction
   - For each phrase, extract up to 5 non-null words before and after the phrase from each version.
   - The context must always include the phrase itself.

## Output Format for each csv file

| {filename1} Text | {filename2} Text | Count | {filename1} Context | {filename2} Context | Pages                 |
|------------------|------------------|-------|---------------------|---------------------|-----------------------|
| saith            | said             | 207   | he saith that       | he said that        | (1 & 5), (7 & 9), ... |
| hath             | has              | 215   | he hath none        | he has none         | (2 & 3), (4 & 5), ... |
| Gospel           | gospel           | 1     | read the Gospel of  | read the gospel of  | (1 & 2)               |

 If the `{filename1} Text` or `{filename2} Text` consists of a single word, and contains one *Names* words (defined in point #2) as a substring (case sensitive comparison) then it is a Name Change

The `{filename1}` and `{filename2}` placeholders should be replaced with the names of the zip files in alphabetical order (without the `.zip` extension).
The `Count` column is the number of times the change occurs (the same change should be grouped, you should only group by the columns `{filename1} Text` and `{filename2} Text`).
The `{filename1} Context` and `{filename2} Context` columns are text representing a context sample for each row in the table
   - (i.e. the file name without the .PageJson extension)
   - Up to 5 words before and after the change
   - The phrase itself must be included in the context string
   - Remove `<null>` from the samples
The `Pages` column should show a list of page number pairs. ({PageNumberOfLeftColumn} & {PageNumberOfRightColumn}) showing which page pairs each change occurred on
   - Always show both page numbers, even if they are identical.

I want them ordered as follows
   Name Change rows should be appear at the top of the table
   Next, rows should be sorted on `Count` (descending)
   then the abs(length({filename1} Text) - length({filename2} Text)) - in descending order
   then `{filename1} Text` ascending
   then `{filename2} Text` ascending

## Deliverable
I want each csv file with only the above mentioned columns. Any columns not mentioned above should be dropped.
Zip up the CSV files and generate a download button.