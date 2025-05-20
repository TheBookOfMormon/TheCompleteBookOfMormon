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

2. **Word Comparison Rules**  
   - **Case-insensitive** comparison is used for general words.
   - **Case-sensitive** comparison is used **only** for the following terms:

```
Judge, Judges, Devil, Heaven, House, Church, Chief, Gospel, Scriptures,
Kingdom, Captain, Captains, Book, Priest, Priests, High, Prophet, Prophets,
Holy, Lawyer, Lawyers, Bible, Saviour, Savior, Satan, Angel, Angels,
Governor, Governors, Saint, Saints, Sea, Red, Father, North, East, South, West,
Christian, Christians
```

3. **Phrase Handling**
   - Consecutive differing words are grouped into phrases.
   - Phrases that contain `<null>` are treated specially:
     - If all are `<null>`, both sides collapse to `<null>`.
     - Mixed `<null>` and text removes `<null>` entries from the phrase.

4. **Context Extraction**
   - For each phrase, extract up to 5 non-null words before and after the phrase from each version.
   - The context must always include the phrase itself.

## Output Format

| {filename1} Text | {filename2} Text | Count | {filename1} Context | {filename2} Context | Pages      |
|------------------|------------------|-------|---------------------|---------------------|------------|
| saith            | said             | 207   | he saith that       | he said that        | 1,5,7, ... |
| hath             | has              | 215   | he hath none        | he has none         | 2,3,4, ... |
| Gospel           | gospel           | 1     | read the Gospel of  | read the gospel of  | 1,2        |

 If the `{filename1} Text` or `{filename2} Text` contains one of the following words as a substring (case sensitive comparison) then it is a Name Change
     Aaron, Abinadi, Abinadom, Abish, Aha, Akish, Alma, Amaleki, Amalekite, Amalickiah, Amaron, Aminadab, Amlici, Amlicite, Ammah, Ammaron, Ammon, Ammonite, Ammoron, Amoron, Amos, Amulek, Amulon, Amulonite, Antiomno, Antionah, Antionum, Antipus, Benjamin, Cezoram, Corianton, Coriantor, Coriantum, Coriantumr, Corihor, Cumeni, Ether, Enos, Gazelem, Gid, Gideon, Gidgiddonah, Gidgiddoni, Helaman, Himni, Ishmael, Ishmaelite, Jacob, Jacobite, Jared, Jarom, John, Joseph, Josephite, Kishkumen, Laban, Lachoneus, Laman, Lamanite, Lamoni, Lemuelite, Limher, Limhi, Manti, Morianton, Mormon, Moron, Moroni, Moronihah, Moses, Mosiah, Mulek, Mulekite, Nehor, Nehorite, Nephi, Nephite, Noah, Omner, Omni, Paanchi, Pacumeni, Pahoran, Sam, Sariah, Seeric, Shiblon, Shiz, Teancum, Teomner, Zarahemla, Zedekiah, Zeezrom, Zeezromites, Zeniff, Zenock, Zenos, Zeram, Zilpah, Zoramite, 

The `{filename1}` and `{filename2}` placeholders should be replaced with the names of the zip files (without the `.zip` extension).
The `Count` column is the number of times the change occurs
The `{filename1} Context` and `{filename2} Context` columns are text representing a context sample for each row in the table
   - (i.e. the file name without the .PageJson extension)
   - Up to 5 words before and after the change
   - The phrase itself must be included in the context string
   - Remove `<null>` from the samples
The `Pages` column should show a list of up to 5 page numbers on which this change occurred.
   - If there are more than 5 then and it is not a Name Change then add "..." to the end of the list
   - If it is a Name Change, then list all page numbers on which this change occurred

I want them ordered as follows
   Name Change rows should be appear at the top of the table
   Next, rows should be sorted on `Count` (descending)
   then `{filename1} Text` ascending
   then `{filename2} Text` ascending