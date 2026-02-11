# General info

The `rspec.ps1` script downloads and calls the rule-api JAR.

The `sonarpedia.json` file is used by:
- the releasability check

# How to use

Script must be run from project root directory. For more details, you can read the powershell code inside `rspec.ps1`.

**Usage 1**: Update all rules or language metadata.

Basically, the rule-api JAR will update: `sonarway_profile*`, `*.html` and `*.json`, `sonarpedia.json` and will generate `RspecStrings.resx`.

```
./scripts/rspec/rspec cs
./scripts/rspec/rspec vbnet
```

**Usage 2**: Pull metadata (replace or create) for single rule.

When the rule had been already specified and is on the `master` branch of the [RSPEC repo:](https://github.com/SonarSource/rspec)

`sonarway_profile*` will not be updated!

```
./scripts/rspec/rspec cs S1234
```

When the rule has been specified according to the [new RSPEC process](https://github.com/SonarSource/rspec#create-or-modify-a-rule), you will need to also give the branch, because the RSPEC branch will be merged only after the implementation is finished.

```
./scripts/rspec/rspec -language vbnet -ruleKey S1234 -rspecBranch "rule/add-RSPEC-S1234"
```

**Usage 3**: Like *Usage 2*, but also creates scaffolding.

- Should not be used to update, just to create a new rule (including to create the `vbnet` impl for an existing `cs` rule).
- If it's newly specified, you need to give the `-rspecBranch` parameter (see *Usage 2* above).
  - Specifying the `-rspecBranch` parameter will also copy all test case files from the RSPEC branch to the `TestCases` folder.
- For updates, use *Usage 1* or *Usage 2*.

```
./scripts/rspec/rspec cs S1234 ClassName
```

# Possible improvements

- For *Usage 3*, check in the script if the scaffolding has already been created and stop if so, issuing a warning message. Currently, running *Usage 3* twice messes up the scaffolding.
- Change the hardcoded ruleapi link with the environment variable that is now on all computers.
