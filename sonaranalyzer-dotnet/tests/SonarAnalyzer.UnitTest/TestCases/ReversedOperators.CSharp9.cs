nint nintTarget = -5;
nint nintNum = 3;

nintTarget = -nintNum; // Compliant - FN
nintTarget = +nintNum; // Compliant - FN

nintTarget = -nintNum; // Compliant; intent to assign inverse value of num is clear

nintTarget += nintNum;
nintTarget += -nintNum;
nintTarget =
    +nintNum;

nuint nuintTarget = +5;
nuint nuintNum = 3;

nuintTarget = +nuintNum; // Compliant - FN

nuintTarget += nuintNum;
nuintTarget += +nuintNum;
nuintTarget =
    +nuintNum;
