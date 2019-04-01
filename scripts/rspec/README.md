# General info

The `rspec.ps1` script downloads and calls the rule-api JAR.

The `sonarpedia.json` file is used by:
- the releasability check
- rules.sonarsource.com

# How to use

For details, you can read the powershell code inside `rspec.ps1`.

**Usage 1**: update all rules or language metadata. Basically, the rule-api JAR will update: `sonarway_profile*`, `*.html` and `*.json`, `sonarpedia.json`.

```
rspec cs
rspec vbnet
```

**Usage 2**: pull metadata (replace or create) for rule BUT `sonarway_profile*` will not be updated

```
rspec cs S1234
```

**Usage 3**: like *Usage 2*, but also creates scaffolding.

- Should not be used to update, just to create a new rule (including to create the `vbnet` impl for an existing `cs` rule).
- For updates, use *Usage 1* or *Usage 2*

```
rspec cs S1234 ClassName
```

# Possible improvements

- for *Usage 3*, check in the script if the scaffolding has already been created and stop if so, issuing a warning message. Currently, running *Usage 3* twice messes up the scaffolding.
- change the hardcoded ruleapi link with the environment variable that is now on all computers
