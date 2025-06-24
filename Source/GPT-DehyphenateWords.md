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

### Create Dictionary and Names lookups
Create two in-memory lookup-tables (hash tables); one named "Names" and the other named "Dictionary"

I have attached dictionary.txt (utf-8 encoded)
Read each word in the file, when a word starts with `name:`
   1: Strip off the leading `name:` text
   2: Put it into the Names look-up table
   3: Also put it into the Dictionary look-up table
When the word does not start with `name:`
   - Put it into the Dictionary look-up table

### Add derivations
For each word in the Dictionary lookup-table, derive ALL APPLICABLE forms and add them into the same lookup table. DO NOT MAKE DERIVATIONS OF WORDS THAT CONTAIN APOSTROPHE (`'`)
If the entry is marked as a **name** (it starts with `name:`), apply the following derivations:
- If it ends in `S`, add:
  - `{entry}'`
- Always add:
  - `{entry}'s`

### General Words (lines not starting with `name:`)
For each entry not starting with `name:`, apply the following transformations:

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


## Scan for combinable words
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

If an entry matches all of the following criteria
   1. it is a simple word (Elements.length == 1) 
   2. it's Text contains a hyphen (-)
   3. it's Text is at least 4 characters long (after removing the hyphen)

Then you should
   → add its exact Text value to the Valid Composite Words set
   → and record the page number it appears on.

Pass 2: Compound [word] - [word] Triplets
Scan all [A] [B] [C] triplets in each file:

A, B, C must each be simple words.

B's Text must be a single hyphen (-).

A and C must satisfy all of the following
   1: Be alphabetic
   2: Must be present in the Dictionary lookup-table
   3: Must appear on the same line

→ If all these conditions are met, add the combined A.Text + "-" + C.Text to the Valid Composite Words set.
→ Also record the page number it appears on.

### Candidate Words:
Candidate words are Combined Words where all of the following are true:
- the first and last elements are on different lines
- it is not in the set of Valid Composite Words (case insensitive comparison)
- the dehyphenated word is in the dictionary lookup table

### Output
First output a *table* of Valid Composite Words (with hyphens intact) as a sorted set, with the first page on which the word appears.
I will then either tell you which entries to remove from the list of Candidate Words or Valid Composite words or tell you to continue

Then recalculate the list of Candidate Words and present output a table of Candidate Words (dehyphenated) as a sorted set, with the first page on which the word appears.
---

## Stage 2: Apply Hyphenation Merges to PageJson Files

Once the candidate list is confirmed:

- For each `*.PageJson` file:
  - Scan the `Words` array for `[word] - [word]` triplets that are Candidate Words
  - Merge the triplet into a single `Word` object with three `Elements` if the recombined form is in the approved list.
  - Make sure you do not change the case of the word, the character casing must be preserved
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

#### IMPORTANT!
   If the source PageJson has `"ManuallyEdited": true` or has no objects in its Words array then this file has already been processed manually, so do not generate a PageJson or PageMetaJson file for that page.

### Final Packaging:
Zip the updated `.PageJson` and `.PageMetaJson` files and generate a hotlink button for me to click in the browser and download it
Then regenerate the download button 