# Compute book hierarchy

## Context

You have multiple editions of a book, each stored as a ZIP file named `{year}.zip` based on the edition year.

Each ZIP contains files named `{PageNumber}.PageJson` (where the page number is three digits, e.g. `001.PageJson`). Each is a JSON file with this structure:

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

1. For each edition, combine the words together into an array as described above.
2. Then scan the array, and any string with a `-` in it should be replaced with an empty string.
3. Compare all editions and produce a **2D table** with:

- **First column:** "Year"  
- **Then a column for each edition's year in chronological order.**  
- **Then a "Base" column.**

4. Each row represents an edition (ordered by year).

5. For each cell:
   - If `row.Year <= column.Year`, leave it blank.
   - Otherwise, show a **percentage (to 2 decimal places)** indicating how similar the row edition is to the column edition.

6. For the final **"Base"** column, indicate the year of the edition the row is most similar to (and therefore most likely to be based off).

---

## Deliverable

Produce the **2D table** as described above.

And also an ascii tree in this format
```
1830
└── 1837
    ├── 1840
    │   └── 1842
    └── 1841
        └── 1849
```

