# Book of Mormon HTML to JSON Page Conversion Prompt

You are given a ZIP file containing HTML pages from the Book of Mormon.  
Each HTML file corresponds to a printed page. Your task is to process them and output structured JSON files with precise formatting.

---

## ✅ Required Input:
- The ZIP contains HTML files named like `007.html`, `008.html`, etc.
- These should be processed in alphabetical order.
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
Find and replace all `<span class="line-break">`
   - where the character before it is a space, replace it with a space
   - where the character before is not a space, replace it with an empty string

---

## ✅ Tokenization Rules:
- Combine contiguous inline text before splitting
- Each piece of output must be either:
  - A word, where `'` and `&` are treated as alphabetic characters
  - A single hyphen (`-`)
  - Apostrophized words like `day's` must remain as:
  ```
  day's
  ```

---

## ✅ Output Format:
For each page, generate **two files**:
1. `{PageNumber}.PageJson`
2. `{PageNumber}.PageMetaJson`

The PageNumber starts at 001 and increments for each `.html` file

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
          "Text": "{word}"
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
