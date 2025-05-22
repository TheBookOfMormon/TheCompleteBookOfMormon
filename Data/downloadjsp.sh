#!/bin/bash

start=7
end=575
output_dir="./Sources/JosephSmithPapers/00-Html"

mkdir -p "$output_dir"

for ((i=start; i<=end; i++)); do
    filename=$(printf "%s/%03d.html" "$output_dir" "$i")
    url="https://www.josephsmithpapers.org/paper-summary/book-of-mormon-1840/$i"

    echo "Downloading $url -> $filename"

    while true; do
        curl -# -L "$url" -o "$filename"
        exit_code=$?

        if [ "$exit_code" -eq 0 ]; then
            echo "Downloaded $filename successfully. Waiting 5 seconds..."
            sleep 5
            break
        else
            echo "Download failed for $filename. Retrying in 30 seconds..."
            sleep 30
        fi
    done
done
