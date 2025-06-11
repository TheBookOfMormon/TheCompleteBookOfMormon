# Book of Mormon HTML to JSON Page Conversion Prompt

You are given a ZIP file containing HTML pages from the Book of Mormon.  
Each HTML file corresponds to a printed page. Your task is to process them and output structured JSON files with precise formatting.

---

## Required Input:
- I have attached a ZIP file
    1. It contains HTML files named like `007.html`, `008.html`, etc.
    2. Each *.html file must be processed in alphabetical order
- I have also attached dictionary.txt (utf-8 encoded)
    1. This is the Dictionary.
    2. You must convert every line to uppercase and use it as a lookup of valid words.
    3. For each base word, derive additional valid entries using the following logic:

### Name Entries (lines starting with `name:`)
If the entry is marked as a **name**, apply the following derivations:
- If it ends in `S`, add:
  - `{entry}'`
- Always add:
  - `{entry}'s`

### General Words (lines not starting with `name:`)
If the entry is **not** a name, apply the following transformations:

- If it ends in `E`, add:
  - `{entry}th`
  - `{entry[..^1]}ing`
  - `{entry[..^1]}ings`
  - `{entry}d`
  - `{entry}n`
  - `{entry}st`

- If it ends in `ING`, add:
  - `{entry}ly`

- If it ends in `LL`, add:
  - `{entry}eth`

- If it ends in `L` (but not `LL`), add:
  - `{entry}leth`
  - `{entry}ling`

- If it ends in `N`, add:
  - `{entry}neth`
  - `{entry}ning`

- If it ends in `SS`, add:
  - `{entry}es`

- If it ends in `EY`, add:
  - `{entry}eth`
  - `{entry[..^2]}ies`
  - `{entry[..^2]}ieth`
  - `{entry[..^2]}ied`

- If it ends with `Y` and NOT `EY`, add:
  - `{entry}eth`
  - `{entry[..^1]}ies`
  - `{entry[..^1]}ieth`
  - `{entry[..^1]}ied`

- - If it ends in `ED`, add:
  - `{entry[..^2]}eth`
  - `{entry[..^2]}est`

- If it ends in a consonant-vowel-consonant pattern (CVC), add:
  - `{entry}{lastChar}ing`

- If it ends in a consonant followed by `L`, add:
  - `{entry}led`

- If it does **not** end in `E`, add:
  - `{entry}eth`
  - `{entry}ing`
  - `{entry}ings`
  - `{entry}est`
  - `{entry}es`
  - `{entry}ed` (unless it already ends in `ed`)

- If it does **not** end in `S`, add:
  - `{entry}s`

- If it contains `our`, add:
  - `{entry}` with `our` replaced by `or`

- If it contains `ise`, add:
  - `{entry}` with `ise` replaced by `ize`

---

All base words and derivations are valid dictionary entries and must be used in the hyphenation logic.

## Text Normalization:
- Decode all HTML entities (e.g., `&rsquo;` → `’`, `&amp;` → `&`)
- Replace curly apostrophes `‘` and `’` with `'`
- Replace en dash `–` with `-`
- Replace em dashes `—` with a space
- Replace all `[ ... ]` (including the brackets and contents) with a space
- Replace all `<div>` elements with their inner html followed by a space
    - Note that this should be recursive into the `<div>`s child elements too (no trailing spaces added)
    - A trailing space should only be added after a `<div>` with class `wasptag`
- Only keep these "Allowed Chars", other chars should be replaced with a space
  - A–Z a–z
  - 0–9
  - spaces (` `)
  - `'`, `&`, `-`

---

## Processing:
1. **Locate the Source Div**:
   - Find the `<div>` with `id="paper-summary-transcript"`.

2. **Span Replacement**:
   - Find each `<span>` in the div's inner html (recursively) that has the class `line-break`
   - Replace the span with the marker character ¬.

3. **Text Extraction**:
   - Now find every child `<div>` that has `wasptag` in its class list
   - Traverse all descendant nodes recursively.
   - Concatenate all inline text (e.g., within <span>, <i>, <b>, etc.) into a continuous string without inserting spaces between them.
   - Only insert a space after the entire wasptag <div> content (i.e., between wasptag blocks), or when encountering:
   - Only retain characters from this allow-list:
     - A–Z a–z
     - 0–9
     - space
     - `'`, `&`, `-`
     - the marker character `¬`
   - All others must be replaced with spaces.
   - Collapse multiple spaces into one.

4. **Hyphenated Word Merging**:
   - For every `¬` marker inside a word (e.g., MOR¬MON):
     - Treat the text on either side of the ¬ as word fragments.
     - Concatenate the two fragments to form a Candidate Word, e.g. MORMON.
     - Convert the candidate word to uppercase and check if it exists in the dictionary.
       - If it does, replace the entire match (e.g., MOR¬MON) with the Candidate Word (MORMON).
       - If it does not (e.g., this¬is), replace the marker with a space (e.g., this is).

Do not tokenize or split on spaces before checking for inline ¬ markers. The merging logic must operate on the raw text, not on pre-tokenized words.

   - For any ¬ marker that appears inside or between word fragments (including across HTML tags), attempt to merge the adjacent fragments.
   - Specifically:
      - Search the text for patterns where ¬ is surrounded by alphanumeric characters (even across DOM boundaries).
      - Extract the left and right fragments around the ¬.
      - Combine the two parts and convert to uppercase.
      - If the result exists in the dictionary (base or derived), replace the full fragment (e.g., "behold¬est") with the combined word ("beholdest").
      - If the combined word is not found in the dictionary, replace the ¬ with a space.
Important: This logic must apply before replacing ¬ with spaces and before tokenization. Do not rely on whitespace to find fragments.

5. **Final Word List**:
   - Replace each remaining word marker `¬` with a space
   - Split the final cleaned text by spaces (excluding empty words)
   - Each token is a valid word.
---

## Output Format:
For each page, generate **two files**:
1. `{PageNumber}.PageJson`
2. `{PageNumber}.PageMetaJson`

PageNumber starts at `001` and increments for each `.html` file.

### `{PageNumber}.PageJson`:
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

### `{PageNumber}.PageMetaJson`:
```json
{
  "NumberOfWords": {word count},
  "PageNumber": {page number}
}
```

---

## Output Packaging:
- Generate one `.PageJson` and one `.PageMetaJson` per input page
- Page numbers must be zero-padded (e.g. `001`, `002`, ...)
- Bundle all files into a ZIP for download
- Generate a download button

---

## Final Notes:
- Preserve original case (do NOT lowercase the text)
- If multiple inline spans form one word (e.g., `"day"`, `'s'`), join them
- If a word is split across DOM nodes or tags, ensure it's merged before tokenization
