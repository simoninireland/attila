\ $Id$

\ Loops, used for bootstrapping. These are "basic" in the sense
\ of not supporting proper control architecture with LEAVE, EXIT and other
\ goodies.

: BEGIN
    TOP ; IMMEDIATE

: UNTIL
    [COMPILE] (?BRANCH)
    >JUMP COMPILE, ; IMMEDIATE

: WHILE
    [COMPILE] (?BRANCH)
    TOP
    0 COMPILE, ; IMMEDIATE

: AGAIN
    [COMPILE] (BRANCH)
    SWAP >JUMP COMPILE,
    DUP JUMP> SWAP ! ; IMMEDIATE

: REPEAT
    POSTPONE AGAIN ; IMMEDIATE


