# OCR Hyphenated Word Processing Prompt

This process operates in **two stages** to ensure that hyphenated word corrections are accurate and under your control.

---

## JSON File Structure Overview

Each `*.PageJson` file contains a top-level JSON object with at least the following:

```json
{
  "Words": [
    {
      "Elements": [
        {
          "Text": "example",
          "Bounds": {
            "X": 100,
            "Y": 200,
            "Width": 50,
            "Height": 20
          }
        }
      ]
    },
    ...
  ]
}
```

- Each item in the `Words` array is either `null` or an object containing an `Elements` array.
- A **simple word** is defined as a `Word` object with exactly **1 `Element`**.
- Each `Element` contains:
  - `Text`: the actual word or character.
  - `Bounds`: an object with `X`, `Y`, `Width`, and `Height` representing its position on the page.

---

## Stage 1: Identify Hyphenated Word Candidates

Scan all `*.PageJson` files to identify `[word] - [word]` sequences, whether they occur on the same line or across lines.

### Criteria for Candidate Words:
- Each item in the triplet must be a simple word.
- The middle item's element's Text must be a hyphen (`-`).
- The first and third elements must be alphabetic words.
- If the third word is on a new line (its `Bounds.X` < first word’s `X`, or its (`Bounds.Y` + `Bounds.Height`) < first word’s `Y`), flag it as a **cross-line** candidate.

### Candidate Filtering:
- Recombine the first and third word (e.g., `"command" + "ed" = "commanded"`).
- Accept the recombined word if:
  - It exists in the attached dictionary.txt
  - It exists in a custom list of acceptable Book of Mormon names OR
  - It is a derived form using valid suffixes (`ed`, `ing`, `eth`, `s`) and the base word is in the dictionary.
    - Special rule for `"ing"`: drop the trailing `"e"` from the base word if present (e.g., `"have" + "ing"` becomes `"having"`).
- Exclude any recombined words that:
  - Are less than 4 characters long (after removing the hyphen)

Output a list of these valid recombined candidates (dehyphenated) for manual confirmation or additional exclusions.

---

## Stage 2: Apply Hyphenation Merges to PageJson Files

Once the candidate list is confirmed:

- For each `*.PageJson` file:
  - Scan the `Words` array for `[word] - [word]` triplets.
  - Merge the triplet into a single `Word` object with three `Elements` if the recombined form is in the approved list.
  - Do not merge if the recombined word is not in the approved list or violates the structure rules.

### Output:
- A modified `.PageJson` file with updated `Words`.
- A companion `.PageMetaJson` file for each page:
  ```json
  {
    "NumberOfWords": [actual number of words after merging],
    "PageNumber": [numeric page number]
  }
  ```
Note that those values are both integers, so the values must remain unquoted.

### Final Packaging:
- Zip the updated `.PageJson` and `.PageMetaJson` files for download.
