Set-StrictMode -Version Latest

function Get-ContractText {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [string]$MissingMessage
    )

    $path = Join-Path $RepositoryRoot $RelativePath
    if (-not (Test-Path $path -PathType Leaf)) {
        if (-not [string]::IsNullOrWhiteSpace($MissingMessage)) {
            throw $MissingMessage
        }
        throw "Contract file was not found: $RelativePath"
    }

    return [System.IO.File]::ReadAllText($path)
}

function Assert-TextContains {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][string]$Token,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if (-not $Text.Contains($Token)) {
        throw $Message
    }
}

function Assert-TextNotContains {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyString()][string]$Text,
        [Parameter(Mandatory = $true)][string]$Token,
        [Parameter(Mandatory = $true)][string]$Message
    )

    if ($Text.Contains($Token)) {
        throw $Message
    }
}

function Assert-ContractMap {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)][string]$RepositoryRoot,
        [Parameter(Mandatory = $true)][System.Collections.IDictionary]$Contracts,
        [Parameter(Mandatory = $true)][string]$ContractName
    )

    foreach ($contract in $Contracts.GetEnumerator()) {
        $relativePath = [string]$contract.Key
        $text = Get-ContractText -RepositoryRoot $RepositoryRoot -RelativePath $relativePath -MissingMessage "$ContractName file was not found: $relativePath"

        foreach ($token in @($contract.Value)) {
            Assert-TextContains -Text $text -Token ([string]$token) -Message "$ContractName token is missing from ${relativePath}: $token"
        }
    }
}

Export-ModuleMember -Function Get-ContractText, Assert-TextContains, Assert-TextNotContains, Assert-ContractMap
