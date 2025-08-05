Get-ChildItem -Filter *.pagejson | ForEach-Object {
    $filePath = $_.FullName
    $json = Get-Content -Raw -Path $filePath | ConvertFrom-Json

    if ($json.Words) {
        foreach ($word in $json.Words) {
            $bodText = $word.BenefitOfDoubtText
            $elements = $word.Elements

            if ($bodText -and $bodText -ieq 'and' -and $elements.Count -eq 1) {
                $elements[0].Text = $bodText
                $word.PSObject.Properties.Remove('BenefitOfDoubtText')
                $word.PSObject.Properties.Remove('BenefitOfDoubt')
            }
        }
    }

    $json | ConvertTo-Json -Depth 10 | Set-Content -Path $filePath -Encoding UTF8
}
