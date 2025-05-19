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

- Some objects in the Words array might be null
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
      2. also add a derivation with "e" dropped and "ing" appended ("have" => "having")
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

### Simple Word:
A Simple Word is any object in the Words array that has only a single object in its Elements array.

### Combined Words:
- Each item in the triplet must be a simple word.
- The middle item's element's Text must be a hyphen (`-`).
- The first and third elements must be alphabetic words.

### Combined Words Filtering:
- Recombine the first and third word (e.g., `"command" + "ed" = "commanded"`).
- Exclude any recombined words that are less than 4 characters long (after removing the hyphen)

### Valid Composite Words:
Build a set of Valid Composite Words using two separate passes:

Pass 1: Simple Hyphenated Words
Scan all Words entries in all PageJson files:

If an entry is a simple word (Elements.length == 1) and the Text contains a hyphen (-),
→ add its exact Text value (case-insensitive) to the Valid Composite Words set,
→ and record the page number it appears on.

Pass 2: Compound [word] - [word] Triplets
Scan all [A] [B] [C] triplets in each file:

A, B, C must each be simple words.

B's Text must be a single hyphen (-).

A and C must both be alphabetic and their individual Text values must be present in the dictionary.

A and C must appear on the same line.
→ If all these conditions are met, add the combined A.Text + "-" + C.Text to the Valid Composite Words set.
→ Also record the page number it appears on.

### Candidate Words:
Candidate words are Combined Words where all of the following are true:
- the first and last elements are on different lines
- it is not in the set of Valid Composite Words (case insensitive comparison)
- the dehyphenated word is in the dictionary lookup table

### Output
First output a table of Valid Composite Words (with hyphens intact) as a sorted set, with the first page on which the word appears.
Then output a table of Candidate Words (dehyphenated) as a sorted set, with the first page on which the word appears.

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

If the source PageJson has `"ManuallyEdited": true` or not objects in its Words array then do not generate a PageJson or PageMetaJson file for that page.

### Final Packaging:
Zip the updated `.PageJson` and `.PageMetaJson` files and generate a hotlink button for me to click in the browser and download it
Then regenerate the download button 