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
- Elements are considered to be on different lines if at least one of the following is true:
  - `C.Bounds.X < A.Bounds.X`: the C element appears to the left of A
  - `(C.Bounds.Y + C.Bounds.Height) < A.Bounds.Y` - the bottom of C is above the top of A
- Each item in the `Words` array is either `null` or an object containing an `Elements` array.
- A **simple word** is defined as a `Word` object with exactly **1 `Element`**.
- A **compound word** is defined as a `Word` object with exactly **3 `Element` items** in its array, where the 2nd element .Text is "-"

---

## Stage 1: Identify Hyphenated Word Candidates

Extract dictionary.txt from dictionary.zip into an in-memory lookup-table (hash table)
For each word, derive ALL APPLICABLE forms and add them to the lookup table. DO NOT MAKE DERIVATIONS OF WORDS THAT CONTAIN APOSTROPHE (`'`)
  - Each word ending with "e" 
      1. add a derivation with "th" appended ("have" => "haveth")
      2. also add a derivation with "e" droped and "ing" appended ("have" => "having")
      3. also add a derivation with "e" dropped and "ings" appended ("engrave" => "engravings")
      4. also add a derivation with "d" appended ("motivate" => "motivated")
      5. also add a derivation with "n" appended ("overtake" => "overtaken")
      6. also add a derivation with "st" appended ("believe" => "believest")
  - Each word ending with "ing" add a derivation with "ly" appended ("exceeding" => "exceedingly") 
  - Each word ending with a single "l"
      1. add a derivation with "leth" appended ("compel" => "compelleth")
      2. also add a derivation with "ling" appended ("complel" => "compelling")
  - Each word ending with a double "ll" add a derivation with "eth" appended ("spell" => "spelleth")
  - Each word ending with a single "n" 
      1. add a derivation with "neth" appended ("begin" => "beginneth")
      2. also add a derivation with append "ning" appended ("begin" => "beginning")
  - Each word ending with "ss" add a derivation with "es" appended ("witness" => "witnesses")
  - Each word ending with "y"
      1. add a derivation with "y" dropped and "ies" appended ("iniquity" => "iniquities")
      2. also add a derivation with "y" dropped and "ieth" appended ("testify" => "testifieth")
      3. also add a derivation with "y" dropped and "ied" appended ("testify" => "testified")
  - For words ending in consonant-vowel-consonant (CVC), double the final consonant and append "ing" (e.g., "commit" → "committing")
  - For words ending in consonant-vowel-"l", double the "l" and append "ed" (e.g., "compel" → "compelled")
  - Each word not ending with "e"
      1. add a derivation with "eth" appended ("say" => "sayeth")
      2. also add a derivation with "ing" appended ("jump" => "jumping")
      3. also add a derivation with "ings" appended ("find" => "findings")
      4. also add a derivation with "est" appended ("great" => "greatest")
      5. also add a derivation with  "es" appended ("establish" => "establishes")
      6. if the word also doesn't end with "ed" then also add a derivation with "ed" appended ("jump" => "jumped")
  - Each word not ending with "s" append "s" ("engraving" => "engravings")

Next, scan all `*.PageJson` files to identify `[word] - [word]` sequences

### Criteria for Combined Words:
- Each item in the triplet must be a simple word.
- The middle item's element's Text must be a hyphen (`-`).
- The first and third elements must be alphabetic words.

### Combined Words Filtering:
- Recombine the first and third word (e.g., `"command" + "ed" = "commanded"`).
- Exclude any recombined words that are less than 4 characters long (after removing the hyphen)

### Valid Composite Words:
Valid Composite Words are Combined Words where the first and last elements are on the same line.
These should be converted to uppercase, and the hyphen preserved.
When the word is split into parts (separated by -) then each part should be in the dictionary lookup table

### Candidate Words:
Candidate words are Combined Words where all of the following are true:
- the first and last elements are on different lines
- when the Candidate Word is converted to uppercase and the hyphen is removed, is not in the set of Valid Composite Words 
- the dehyphenated word is in the dictionary lookup table

### Output
1. Output a table of Valid Composite Words (with hyphens intact) as a sorted set, with the first page on which the word appears.
2. Output a table of Candidate Words (dehyphenated) as a sorted set, with the first page on which the word appears.

I will then either tell you which entries remove from the list of Candidate Words or tell you to continue

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
Zip the updated `.PageJson` and `.PageMetaJson` files and generate a hotlink button for me to click in the browser and download it
Then regenerate the download button 