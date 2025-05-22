# Book of Mormon 1830 HTML to JSON Page Conversion Prompt

You are given a ZIP file containing HTML pages from the 1830 edition of the Book of Mormon.  
Each HTML file corresponds to a printed page. Your task is to process them and output structured JSON files with precise formatting.

---

## ✅ Required Input:
- The ZIP contains HTML files named like `007.html` to `623.html`
- The actual printed page number is `HTML filename - 6`, zero-padded (e.g. `007.html` → `001.PageJson`)

---

## ✅ What To Extract:
- Extract **only** the contents of `<div id="paper-summary-transcript">` from each file

---

## ✅ Text Normalization:
- Decode all HTML entities (e.g., `&rsquo;` → `’`, `&amp;` → `&`)
- Replace curly apostrophes `‘` and `’` with `'`
- Replace en dash `–` with `-`
- Remove em dashes `—`
- Strip out all characters **except**:
  - A–Z a–z
  - 0–9
  - spaces (` `)
  - `'`, `&`, `-`
- Remove all `[ ... ]` including the brackets and contents

---

## ✅ Line Break Hyphens:
If you see `<span class="line-break">`:
- If the character before the `<span>` is a space → **ignore it**
- If the character before the `<span>` is a letter → **treat it as a hyphen (`-`)** (on its own line)

---

## ✅ Tokenization Rules:
- Combine contiguous inline text before splitting
- Each piece of output must be either:
  - A word, where `'` and `&` are treated as alphabetic characters
  - A single hyphen (`-`)
- Hyphenated line-break words like `"re - pent"` must be output as:
  ```
  re
  -
  pent
  ```
- Apostrophized words like `"day's"` must remain as:
  ```
  day's
  ```

---

## ✅ Output Format:
For each page, generate **two files**:
1. `{PageNumber}.PageJson`
2. `{PageNumber}.PageMetaJson`

### 📄 {PageNumber}.PageJson:
```json
{
  "ManuallyEdited": true,
  "ImageHeight": 1,
  "ImageWidth": 1,
  "PageNumber": 1,
  "Words": [
    {
      "Elements": [
        {
          "Bounds": {
            "X": 0,
            "Y": 0,
            "Width": 1,
            "Height": 1
          },
          "Text": "word-or-hyphen"
        }
      ]
    }
  ]
}
```

### 🧾 {PageNumber}.PageMetaJson:
```json
{
  "NumberOfWords": {word count},
  "PageNumber": {page number}
}
```

---

## ✅ Output Packaging:
- Generate one `.PageJson` and one `.PageMetaJson` per input page
- Page numbers must be zero-padded (e.g. `001`, `002`, ...)
- Bundle all files into a ZIP for download

---

## ✅ Final Notes:
- Preserve original case (do NOT lowercase the text)
- If multiple inline spans form one word (e.g., `"day"`, `'s'`), they must be joined into a single word
- If the original HTML encoding separates part of a word across tags, join them

---
