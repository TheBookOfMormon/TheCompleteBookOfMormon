
# Compare Manually Aligned PageJson Files Across Editions

This task compares **multiple editions contained within All.zip**. Each embedded zip is an edition, and comparisons are performed **only between each edition and the edition it was most likely based on**, as determined by the following hierarchy:

```
1830 Palmyra
└─── 1837 Kirtland
     └─── 1840 Nauvoo
          ├─── 1841 Liverpool
          │    └─── 1849 Liverpool
          │         └─── 1854 Liverpool
          │              └─── 1871 SaltLakeCity
          │                   └─── 1877 SaltLakeCity
          └─── 1842 Nauvoo
               └─── 1858 NewYork
                    └─── 1874 Iowa
                         └─── 2013 SaltLakeCity
```

## Names
The following list is a collection of names.

Aaron
Abinadi
Abinadom
Abish
Abraham
Adam
Aha
Ahah
Akish
Alma
Amaleki
Amalekite
Amalekites
Amalickiah
Amalickiahites
Amaron
Aminadab
Aminadi
Amlici
Amlicite
Amlicites
Ammah
Ammaron
Ammon
Ammonihah
Ammonihahites
Ammonihahits
Ammonite
Ammorite
Ammoron
Amnigaddah
Amnihu
Amnor
Amoron
Amos
Amulek
Amulon
Amulonites
Anathoth
Angelah
Angola
Anti-anti
Anti-Nephi-Lehi
Anti-Nephi-Lehies
Antiomno
Antionah
Antionum
Antiparah
Antipas
Antipus
Assyria
Assyrian
Babylon
Babylonian
Babylonians
Benjamin
Boaz
Camorah
Cezoram
Christ
Christian
Christians
Cohor
Com
Corianton
Coriantor
Coriantum
Coriantumr
Corihor
Corom
Cumeni
Cumenihah
Cumorah
Desolation
Devil
Egypt
Egyptians
Emer
Enos
Ephraim
Esrom
Ethem
Ether
Gaddianton
Gadianton
Gadiomnah
Gazelem
Geba
Gid
Giddianhi
Gideon
Gidgiddonah
Gidgiddoni
Gilgah
Gimgimno
Hearthom
Helaman
Heth
Himni
Isabel
Ishmael
Ishmaelite
Ishmaelites
Israel
Jacob
Jacobite
Jacobites
Jacom
Jared
Jarom
Jeberechiah
Jeneum
Jeremiah
Jershon
Jerurusalem
Jerusalem
Jesus
John
Joseph
Josephite
Josephites
Jotham
Judah
Kib
Kim
Kimnor
Kingmen
Kish
Kishkumen
Korihor
Laban
Lachoneus
Laman
Lamanite
Lamanites
Lamoni
Lebanon
Lehi
Lehi-Nephi
Lehonti
Lemuel
Lemuelite
Lemuelites
Levi
Liahona
Lib
Limhah
Limher
Limhi
Luke
Maher-shalal-hash-baz
Malachi
Mammon
Manasseh
Manti
Mark
Matt
Matthew
Melchizedek
Melek
Michmash
Middoni
Mocum
Moriancumer
Morianton
Mormon
Moron
Moroni
Moronihah
Moses
Mosiah
Mulek
Mulekite
Nahom
Nazareth
Nehor
Nehorite
Nehors
Nephi
Nephihah
Nephite
Nephites
Nimrah
Noah
Omer
Omner
Omni
Onihah
Orihah
Paanchi
Pachus
Pacumeni
Pahoran
Rabbanah
Raca
Ramah
Ramath
Remalia
Remaliah
Rezin
Riplakish
Ripliancum
Salem
Satan
Sam
Samaria
Samuel
Sariah
Seeric
Seezoram
Seth
Sherrizah
Shez
Shemlon
Shiblom
Shiblon
Shimnilom
Shiz
Shule
Sidom
Sidon
Sinim
Solomon
Teancum
Teomner
Tubaloth
Zarahemla
Zebulun
Zedekiah
Zeezrom
Zeezromite
Zemnarihah
Zenephi
Zeniff
Zenoch
Zenock
Zenos
Zerahemla
Zerahemna
Zerahemnah
Zeram
Zilpah
Zion
Zoram
Zoramite
Zoramites

## Precondition

- Unzip **All.zip**.
- For each embedded zip file (edition), extract its `.PageJson` files.
- Filenames within each edition are sorted alphabetically.
- Word alignment is global across all files. Words are aligned by index across editions. Missing words are represented as `null`.
- **Store each word with its page filename** for later page tracking.

## Comparison Logic

Compare each edition **only against the edition it was based on (its parent in the tree above)**.

### Word Extraction Rules

- Each "Words" array item is either:
  - `null` → empty string
  - Object with an "Elements" array:
    - If **1 element**: use `Elements[0].Text`
    - If **3 elements**:
      - If "ShowDashes" is `true`: `Elements[0].Text + Elements[1].Text + Elements[2].Text`
      - Otherwise: `Elements[0].Text + Elements[2].Text`
    - Otherwise: empty string

### Additional processing

- Truncate both lists to the shorter length for comparison.
- If a word has "BenefitOfDoubtText" then that should be used for the text
- If Text is "" then treat it as null

## Names

Words in the Names list are treated as Names (case sensitive). If a word has a `'` or `'s` appended to a Name, it is also a Name.

## Word Comparison Rules

- **Case-insensitive** for general words.
- **Case-sensitive** for Names and words starting with:

```
Judge, Heaven, House, Church, Chief, Gospel, Scripture,
Kingdom, Captain, Book, Priest, High, Prophet,
Holy, Lawyer, Bible, Saviour, Savior, Satan, Angel,
Governor, Saint, Sea, Son, Red, Father, North, East, South, West,
Christian
```

## Phrase Handling

- Names are considered single words.
- Non-name differences:
  - Consecutive differing words are grouped into phrases.
  - Phrases containing `<null>`:
    - If all are `<null>`, collapse to `<null>`.
    - Mixed `<null>` and text → remove `<null>` entries.

## Context Extraction

- For each phrase, extract up to **5 non-null words before and after**.
- Context must include the phrase itself.

## Page Tracking

- Each difference records the **page number** it was found on.
- Page numbers are extracted from filenames by stripping extensions and leading zeros.

## Grouping

- Identical `{Edition1} Text` vs `{Edition2} Text` changes are grouped into a single row.
- `Count` reflects total occurrences.
- Context columns show **only the first encountered sample**.
- Pages column contains all page pairs in the format `(ParentPage & ChildPage)`, separated by `; `.

## Output Files

For each comparison:

- Output a CSV file named:

```
{Edition1}-cs-{Edition2}.csv
```

where Edition1 and Edition2 are the compared editions.

### CSV Columns

| {filename1} Text | {filename2} Text | Count | {filename1} Context | {filename2} Context | Pages |
| ---------------- | ---------------- | ----- | ------------------- | ------------------- | ----- |

- Replace `{filename1}` and `{filename2}` with edition names (without .zip and without "Edition" or "Manuscript").
- `Count`: occurrences of the change.
- Context columns: up to 5 words before/after the change (excluding `<null>`).
- `Pages`: list of page number pairs `(ParentPage & ChildPage)`.

### Sorting

1. **Name Change** rows first
2. `Count` descending
3. `{filename1} Text` ascending

## Final packaging

- Zip all output CSV files into a single downloadable archive.
