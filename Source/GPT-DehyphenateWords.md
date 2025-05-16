# OCR Hyphenated Word Processing Prompt

This process operates in **two stages** to ensure that hyphenated word corrections are accurate and under my control.

---

## JSON File Structure Overview

Each `*.PageJson` file contains a top-level JSON object with at least the following:

```json
{
  "ImageHeight": 0,
  "ImageWidth": 0,
  "PageNumber": 5,
  "ManuallyEdited": true,
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

The "ManuallyEdited" boolean property is optional, so might not exist.
If it does exist and has the value `true` then the file should not be processed at all. Not read from, nor written to.
The same rule applies for files where "Words" is an empty array.


- Each `Element` contains:
  - `Text`: the actual word or character.
  - `Bounds`: an object with `X`, `Y`, `Width`, and `Height` representing its position on the page.
- Two elements are on different lines if C.Bounds.X < C.Bounds.X or (C.Bounds.Y + C.Bounds.Height) < A.Bounds.Y
- Each item in the `Words` array is either `null` or an object containing an `Elements` array.
- A **simple word** is defined as a `Word` object with exactly **1 `Element`**.
- A **compound word** is defined as a `Word` object with exactly **3 `Element` items** in its array, where the 2nd element .Text is "-"

---

## Stage 1: Identify Hyphenated Word Candidates

Scan all `*.PageJson` files to identify `[word] - [word]` sequences

### Criteria for Candidate Words:
- Each item in the triplet must be a simple word.
- The middle item's element's Text must be a hyphen (`-`).
- The first and third elements must be alphabetic words.
- The first and last element must on different lines 

### Candidate Filtering:
- Recombine the first and third word (e.g., `"command" + "ed" = "commanded"`).
- Accept the recombined word if:
  - It exists in the attached dictionary.zip file (as the file dictionary.txt)
  - It is a derived form using valid suffixes (`ed`, `ing`, `ingly`, `eth`, `s`) and the base word is in the dictionary.
    - Special rule for `"ing"` and `"ingly"`: drop the trailing `"e"` from the base word if present (e.g., `"have" + "ing"` becomes `"having"`).
- Exclude any recombined words that:
  - Are less than 4 characters long (after removing the hyphen)

Output a table of these valid recombined candidates (dehyphenated) for manual confirmation or additional exclusions.
The output should be an alphabetically sorted list of unique words.

I will then either tell you which entries remove from the list of "hyphenated words" or tell you to continue

---

## Stage 2: Apply Hyphenation Merges to PageJson Files

Once the candidate list is confirmed:

- For each `*.PageJson` file:
  - Scan the `Words` array for `[word] - [word]` triplets.
  - Merge the triplet into a single `Word` object with three `Elements` if the recombined form is in the approved list.
  - Do not merge if the recombined word is not in the approved list or violates the structure rules.

### Output:
- A modified `.PageJson` file with updated `Words`. You are modifying only the Words property, all other properties in the PageJson must remain unchanged and their order preserved.
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
- Give me a download link for the zip file, specifically to the sandbox:/mnt/ file