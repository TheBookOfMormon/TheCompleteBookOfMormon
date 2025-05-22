# Compare Manually Aligned PageJson Files with Controlled Case Sensitivity

This task compares two `.zip` archives containing manually aligned `.PageJson` files. Each file contains a `"Words"` array of OCR tokens where each word may be:

- `null`
- An object with an `"Elements"` array:
  - If it contains 1 element: use `Elements[0].Text`
  - If it contains 3 elements then either
    1. If it has "ShowDashes" set to `true` then use `Elements[0].Text + Elements[1].Text + Elements[2].Text`
    2. If it does not have "ShowDashes" or it is set to `false` then use `Elements[0].Text + Elements[2].Text`
  - If it contains any other structure: treat it as an empty string

## Precondition

The PageJson files are manually aligned from a known anchor point:
- Start processing from 001.PageJson in each file and continue processing the files alphabetically
- Word alignment is global across all files; do not reset state per file.
- Word positions are matched 1:1 across both versions.
- Missing words are explicitly represented as `null`.

## Comparison Logic

1. **Flattening**  
   - Sort filenames in both zips.
   - Starting from the sync point, extract and flatten the `"Words"` array into a single list per version.
   - Retain `null` entries to preserve position.
   - Truncate both lists to the length of the shorter one.

2. **Names**
   - The following list is a collection of names. 

```
Aaron, Abinadi, Abinadom, Abish, Aha, Ahah, Akish, Alma, Amaleki, Amalekite, Amalickiah, Amaron, Aminadab, Amlici,
Amlicite, Ammah, Ammaron, Ammon, Ammonite, Ammoron, Amnigaddah, Amoron, Amos, Amulek, Amulon, Amulonites, Angola,
Antiomno, Antionah, Antionah, Antionum, Antipus, Benjamin, Cezoram, Com, Corianton, Coriantor, Coriantum, Coriantumr,
Corihor, Corom, Cumeni, Cumenihah, Cumorah, Devil, Ethem, Ether, Emer, Enos, Gadianton, Gazelem, Gid, Gideon, Giddianhi,
Gidgiddonah, Gidgiddoni, Hearthom, Helaman, Heth, Himni, Ishmael, Ishmaelite, Jacobite, Jared, Jarom, Jacob, Jeneum,
John, Joseph, Josephite, Kingmen, Kishkumen, Laban, Lachoneus, Laman, Lamanite, Lamoni, Lemuelite, Levi, Lib, Limhah,
Limher, Limhi, Luke, Kib, Kim, Kimnor, Kish, Mammon, Manti, Mark, Matthew, Matt, Morianton, Mormon, Moron, Moroni,
Moronihah, Moses, Mosiah, Mulek, Mulekite, Nehor, Nehorite, Nephi, Nephite, Noah, Omer, Omner, Omni, Onihah, Orihah,
Paanchi, Pachus, Pacumeni, Pahoran, Riplakish, Sam, Sariah, Seeric, Seerstone, Seth, Shez, Shiblon, Shiz, Shule,
Teancum, Teomner, Tubaloth, Zarahemla, Zedekiah, Zeezrom, Zeezromite, Zemnarihah, Zeniff, Zenock, Zenos, Zeram, Zilpah, Zoramite
```
   - If a word is one of these words (case sensitive) then it is a name
   - If a word is one of these words (case sensitive) with a `'` or `'s` appended, then it is a name

3. **Word Comparison Rules**  
   - **Case-insensitive** comparison is used for general words.
   - **Case-sensitive** comparison is used **only** for Names and words that begin with any of the following:

```
Judge, Heaven, House, Church, Chief, Gospel, Scripture,
Kingdom, Captain, Captain, Book, Priest, High, Prophet,
Holy, Lawyer, Bible, Saviour, Savior, Satan, Angel,
Governor, Saint, Sea, Son, Red, Father, North, East, South, West,
Christian
```

4. **Phrase Handling**
   - When a word is a Name, then it is considered only as a single word
   - When a word is not a name then
      - Consecutive differing words are grouped into phrases.
      - Phrases that contain `<null>` are treated specially:
        - If all are `<null>`, both sides collapse to `<null>`.
        - Mixed `<null>` and text removes `<null>` entries from the phrase.

5. **Context Extraction**
   - For each phrase, extract up to 5 non-null words before and after the phrase from each version.
   - The context must always include the phrase itself.

## Output Format

| {filename1} Text | {filename2} Text | Count | {filename1} Context | {filename2} Context | Pages                 |
|------------------|------------------|-------|---------------------|---------------------|-----------------------|
| saith            | said             | 207   | he saith that       | he said that        | (1 & 5), (7 & 9), ... |
| hath             | has              | 215   | he hath none        | he has none         | (2 & 3), (4 & 5), ... |
| Gospel           | gospel           | 1     | read the Gospel of  | read the gospel of  | (1 & 2)               |

 If the `{filename1} Text` or `{filename2} Text` consists of a single word, and contains one of the following words as a substring (case sensitive comparison) then it is a Name Change
     Aaron, Abinadi, Abinadom, Abish, Aha, Ahah, Akish, Alma, Amaleki, Amalekite, Amalickiah, Amaron, Aminadab, Amlici, Amlicite, Ammah, Ammaron, Ammon, Ammonite, Ammoron, Amnigaddah, Amoron, Amos, Amulek, Amulon, Amulonites, Angola, Antiomno, Antionah, Antionah, Antionum, Antipus, Benjamin, Cezoram, Com, Corianton, Coriantor, Coriantum, Coriantumr, Corihor, Corom, Cumeni, Cumenihah, Cumorah, Devil, Ethem, Ether, Emer, Enos, Gadianton, Gazelem, Gid, Gideon, Giddianhi, Gidgiddonah, Gidgiddoni, Hearthom, Helaman, Heth, Himni, Ishmael, Ishmaelite, Jacobite, Jared, Jarom, Jacob, Jeneum, John, Joseph, Josephite, Kingmen, Kishkumen, Laban, Lachoneus, Laman, Lamanite, Lamoni, Lemuelite, Levi, Lib, Limhah, Limher, Limhi, Luke, Kib, Kim, Kimnor, Kish, Mammon, Manti, Mark, Matthew, Matt, Morianton, Mormon, Moron, Moroni, Moronihah, Moses, Mosiah, Mulek, Mulekite, Nehor, Nehorite, Nephi, Nephite, Noah, Omer, Omner, Omni, Onihah, Orihah, Paanchi, Pachus, Pacumeni, Pahoran, Riplakish, Sam, Sariah, Seeric, Seerstone, Seth, Shez, Shiblon, Shiz, Shule, Teancum, Teomner, Tubaloth, Zarahemla, Zedekiah, Zeezrom, Zeezromite, Zemnarihah, Zeniff, Zenock, Zenos, Zeram, Zilpah, Zoramite

The `{filename1}` and `{filename2}` placeholders should be replaced with the names of the zip files in alphabetical order (without the `.zip` extension).
The `Count` column is the number of times the change occurs
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

## Final packaging
I want a csv file with only the above mentioned columns. Any columns not mentioned above should be dropped.