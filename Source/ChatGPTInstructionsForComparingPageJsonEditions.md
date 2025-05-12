# Compare Manually Aligned PageJson Files with Controlled Case Sensitivity

This task compares two `.zip` archives containing manually aligned `.PageJson` files. Each file contains a `"Words"` array of OCR tokens where each word may be:

- `null`
- An object with an `"Elements"` array:
  - If it contains 1 element: use `Elements[0].Text`
  - If it contains 3 elements: use `Elements[0].Text + Elements[2].Text`

## Precondition

The PageJson files are manually aligned:
- Each zip has corresponding filenames.
- Word positions are aligned.
- Missing words are explicitly represented as `null`.

## Comparison Logic

1. **Flattening**  
   - Sort filenames in both zips.
   - Extract and flatten the `"Words"` array into a single list for each version.
   - Truncate both lists to the length of the shorter one.

2. **Word Comparison Rules**  
   - **Case-insensitive** comparison is used for general words.
   - **Case-sensitive** comparison is used **only** for the following terms:

```
Judge, Judges, Devil, Heaven, House, Church, Chief, Gospel, Scriptures,
Kingdom, Captain, Captains, Book, Priest, Priests, High, Prophet, Prophets,
Holy, Lawyer, Lawyers, Bible, Saviour, Savior, Satan, Angel, Angels,
Governor, Governors, Saint, Saints, Sea, Red, Feather, North, East, South, West,
Christian, Christians
```

3. **Phrase Handling**
   - Consecutive differing words are grouped into phrases.
   - Phrases that contain `<null>` are treated specially:
     - If all are `<null>`, both sides collapse to `<null>`.
     - Mixed `<null>` and text removes `<null>` entries from the phrase.

4. **Output Format**

| 1830 Text           | 1837 Text         | Count |
|---------------------|------------------|-------|
| saith               | said             | 207   |
| hath                | has              | 215   |
| Gospel              | gospel           | 11    |

Additionally, each entry includes a context sample:
- Up to 5 words before and after the change.
- `<null>` tokens are excluded from context.

## Output

Generate a table with the following columns:

- `1830 Text`
- `1837 Text`
- `Count` (number of times the change occurs)
- `1830 Context`
- `1837 Context`
