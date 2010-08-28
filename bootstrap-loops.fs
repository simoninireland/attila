\ $Id$

\ Loops, used for bootstrapping. These are "basic" in the sense
\ of not supporting proper control architecture with LEAVE, EXIT and other
\ goodies.

: BEGIN
    TOP ; IMMEDIATE

: UNTIL
    [COMPILE] (?BRANCH) JUMP-BACKWARD ; IMMEDIATE

: WHILE
    [COMPILE] (?BRANCH) JUMP-FORWARD ; IMMEDIATE

: AGAIN
    [COMPILE] (BRANCH) JUMP-BACKWARD ; IMMEDIATE

: REPEAT
    SWAP
    [COMPILE] (BRANCH) JUMP-BACKWARD
    JUMP-HERE ; IMMEDIATE


