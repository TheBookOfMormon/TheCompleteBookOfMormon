#!/bin/bash
# Enable recursive globbing and ensure globs expand to nothing if no match
shopt -s globstar nullglob

# Confirm we're inside a Git repository
if ! git rev-parse --is-inside-work-tree > /dev/null 2>&1; then
    echo "Error: Not inside a Git repository. Exiting."
    exit 1
fi

# Loop through numbers 1 to 871, formatted as 3-digit strings
for i in $(seq 1 871); do
    num=$(printf "%03d" "$i")
    
    # Find files matching the pattern
    echo "$(date +"%T"): Searching for files matching $num."

    files=( **/"$num".* )
    if (( ${#files[@]} == 0 )); then
        echo "$(date +"%T"): No files matched for $num, skipping."
        continue
    fi

    echo "$(date +"%T"): Adding files for $num"
    git add "${files[@]}"
    
    # Only attempt to commit if there are staged changes
    if git diff --cached --quiet; then
        echo "$(date +"%T"): No staged changes for $num, skipping commit."
        continue
    fi
    
    echo "$(date +"%T"): Committing files for $num"
    git commit -q -m "Update files $num" 2>/dev/null

    echo "$(date +"%T"): Changes committed for $num, pushing to remote..."
    git push
done
