# How to validate the quality of a new rule?

1. Hit `F5` to launch the experimental instance of Visual Studio with the .vsix deployed.

2. Open one of the following solutions in `Debug` configuration from the experimental Visual Studio instance:

  * [Akka.NET](akka.net/src/Akka.sln)
  * [Nancy](Nancy/src/Nancy.sln)

	These solutions have been pre-configured to use the [Validation Ruleset](ValidationRuleset.ruleset) on all their projects.

3. Turn on your new rule in that ruleset, review the results, improve, and setup the regression test once you are satisfied

# How to run the rule regression tests?

From a Developer Prompt for Visual Studio 2015, simply launch [regression-test.bat](regression-test.bat).

# How to visualize issue differences?

`actual` is a local Git repository initialized with the files in `expected`, so:

  1. `cd actual`
  2. `git diff --cached`

If you find regressions, don't forget to first update your rule's unit test before starting to fix anything!
If not, copy the contents of the `actual` folder into `expected` to accept the differences.

# Note

Use `git submodule update --init --recursive` to fetch the Git submodules locally.
