/*
 * We want to create a big padding so when the rule looking for the secondary location, we make sure
 * that this file is way bigger that the file were the primary location is.
 * So the secondray location would be out of bound of the first file.
 *
 * ###################################################################################
 * ###                            .i;;;;i.                                         ###
 * ###                          iYcviii;vXY:                                       ###
 * ###                        .YXi       .i1c.                                     ###
 * ###                       .YC.     .    in7.                                    ###
 * ###                      .vc.   ......   ;1c.                                   ###
 * ###                      i7,   ..        .;1;                                   ###
 * ###                     i7,   .. ...      .Y1i                                  ###
 * ###                    ,7v     .6MMM@;     .YX,                                 ###
 * ###                   .7;.   ..IMMMMMM1     :t7.                                ###
 * ###                  .;Y.     ;$MMMMMM9.     :tc.                               ###
 * ###                  vY.   .. .nMMM@MMU.      ;1v.                              ###
 * ###                 i7i   ...  .#MM@M@C. .....:71i                              ###
 * ###                it:   ....   $MMM@9;.,i;;;i,;tti                             ###
 * ###               :t7.  .....   0MMMWv.,iii:::,,;St.                            ###
 * ###              .nC.   .....   IMMMQ..,::::::,.,czX.                           ###
 * ###             .ct:   ....... .ZMMMI..,:::::::,,:76Y.                          ###
 * ###             c2:   ......,i..Y$M@t..:::::::,,..inZY                          ###
 * ###            vov   ......:ii..c$MBc..,,,,,,,,,,..iI9i                         ###
 * ###           i9Y   ......iii:..7@MA,..,,,,,,,,,....;AA:                        ###
 * ###          iIS.  ......:ii::..;@MI....,............;Ez.                       ###
 * ###         .I9.  ......:i::::...8M1..................C0z.                      ###
 * ###        .z9;  ......:i::::,.. .i:...................zWX.                     ###
 * ###        vbv  ......,i::::,,.      ................. :AQY                     ###
 * ###       c6Y.  .,...,::::,,..:t0@@QY. ................ :8bi                    ###
 * ###      :6S. ..,,...,:::,,,..EMMMMMMI. ............... .;bZ,                   ###
 * ###     :6o,  .,,,,..:::,,,..i#MMMMMM#v.................  YW2.                  ###
 * ###    .n8i ..,,,,,,,::,,,,.. tMMMMM@C:.................. .1Wn                  ###
 * ###    7Uc. .:::,,,,,::,,,,..   i1t;,..................... .UEi                 ###
 * ###    7C...::::::::::::,,,,..        ....................  vSi.                ###
 * ###    ;1;...,,::::::,.........       ..................    Yz:                 ###
 * ###     v97,.........                                     .voC.                 ###
 * ###      izAotX7777777777777777777777777777777777777777Y7n92:                   ###
 * ###        .;CoIIIIIUAA666666699999ZZZZZZZZZZZZZZZZZZZZ6ov.                     ###
 * ###                                                                             ###
 * ###                                                                             ###
 * ###                  This file MUST be bigger than                              ###
 * ###       InheritedCollidingInterfaceMembers.DifferentFile.cs in size           ###
 * ###                                                                             ###
 * ###################################################################################
 *
 * A lorem ipsum text
 *
 * Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor
 * incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud
 * exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure
 * dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
 * Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit
 * anim id est laborum.
 *
 * Another one:
 *
 * Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor
 * incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud
 * exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure
 * dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur.
 * Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit
 * anim id est laborum.
 *
 */

using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public partial interface IAnotherFile
    {
        void Method(int i);
    }

    public partial interface IAnotherFile2
    {
        void Method(int i); // Secondary {{This member collides with 'IAnotherFile.Method(int)'}}
    }
}
