#
# Copyright (C) 2010 SonarSource SA
# All rights reserved
# mailto:contact AT sonarsource DOT com
#

########################################################

			  C# Toolkit
			  version ${project.version}

########################################################



The C# Toolkit offers 2 functionalities:

- a simple GUI that shows the Abstract Syntax Tree (AST) of a given C# file 
- a command-line parser that can:
	- parse several files/folders at a time
	- dump the AST of the parsed C# files into XML files



Requirements
************

- Java Runtime Environment > 1.5
	-> run "java -version" command to check that requirement



Quick guide to set up the C# Toolkit
************************************

1- Unzip the archive in an installation directory


2- If you just want to use the GUI, then double-click on the JAR file located in the bin folder.
   Otherwise, follow the next steps.


3- Create the SONAR_TOOLKIT environment variable that must point to the "bin" folder located in
   the installation directory. 
   Please use double-quotes if the installation directory contains spaces.
	
	- On Windows: set SONAR_TOOLKIT=myFolder
		- Ex.: set SONAR_TOOLKIT=C:\Programs\CSharpToolkit\bin
	- On Unix/Linux/Mac: export SONAR_TOOLKIT="myFolder"
		- Ex.: export SONAR_TOOLKIT="/opt/programs/CSharpToolkit/bin"


4- Add this SONAR_TOOLKIT variable to your PATH

	- On Windows: set PATH=%SONAR_TOOLKIT%;%PATH%
	- On Unix/Linux/Mac: export PATH=$SONAR_TOOLKIT:$PATH


5- Run the parser on the command line :

	- On Windows: sonar-parse srcDir="MyProjet/src" charset=UTF-8
	- On Unix/Linux/Mac: auto-control.sh MY-FILE srcDir="MyProjet/src" charset=UTF-8

If there are parsing errors, they are logged in the "ParsingErrors" file located in the folder where 
the command was launched.

