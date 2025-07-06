# Compute book hierarchy

## Context
I have supplied a file named All.zip
This zip file contains multiple embedded zip files, for example
   - 1830PalmyraEdition.zip
   - 1837KirtlandEdition.zip
   - 2013SaltLakeCityEdition.zip
Each of these embedded zip files is an edition of the same book.
The filename of each embedded zip is a 4 digit year, then the name of the place (Liverpool, NewYork, etc) followed by the word "Edition" or "Manuscript"
The edition name is the 4 digit year + name of the place. You can drop "Edition.zip" and "Manuscript.zip"
Each of the embedded zip files contains multiple files that have a filename of 3 digits + "PageJson" extension

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

### Processing Logic

- **"Words"** is an array. Each item is either:
  - `null` (meaning an empty word), or
  - an object with an **"Elements"** array.

- Each **Element** has:
  - a `"Text"` field (string).

#### Combining Elements to form a word

- If a **Word** has **1 element**, its text is simply the `Text` of that element.

- If a **Word** has **3 elements**:
  - If `ShowDashes == true`:
    - Its text is the concatenation of all **three elements** in order.
  - Otherwise:
    - Its text is the concatenation of the **first and third elements only** (ignoring the second).

### Additional Notes

- Words are **aligned across editions**. The index in one edition’s array matches the same index in another edition’s array.
- When processing, replace any word containing `-` with an **empty string**.
- If a Word is `null`, treat it as an empty string.

---

## Task

Note: Order editions by year, and then by their name as a secondary sorting column.

1. For each edition, combine the words together into an array as described above.
2. Then scan the array, and any string with a `-` in it should be replaced with an empty string.
3. Compare all editions and produce a **2D table** with:

- **First column:** "Edition"  
- **Then a column for each edition in chronological order.**  
- **Then a "Base" column.**

4. Each row represents an edition (ordered by year).

5. For each cell:
   - If `row.Year <= column.Year`, leave it blank.
   - Otherwise, show a **percentage (to 2 decimal places)** indicating how similar the row edition is to the column edition.

6. For the final **"Base"** column, indicate the year of the edition the row is most similar to (and therefore most likely to be based off).

---

## Deliverable

Produce the **2D table** as described above. I want to download it with the filename "edition-similarity-table.csv"

And also an ascii tree in this format, which I want to download with the filename "edition-similarity-tree.txt"
```
1830 Palymyra
└── 1837 Kirland
    ├── 1840 Nauvoo
    │   └── 1842 Nauvoo
    │   └── 1858 New York
    └── 1841 Liverpool
        └── 1849 Liverpool
```

