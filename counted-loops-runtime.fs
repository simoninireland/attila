\ $Id$

\ The run-time support needed by counted loops

\ Set up the loop, remembering the limit and initial indices
: (DO) \ ( limit initial -- )
    R> ROT             \ ret limit initial
    SWAP >R >R
    >R ;

\ Increment the index and continue if it is strictly less than the limit
: (+LOOP) \ ( inc -- f )
    R> SWAP            \ ret inc
    1 RPICK R>         \ ret inc limit i
    -ROT +             \ ret limit i'
    DUP -ROT < IF      \ ret i'
        >R >R 0
    ELSE
        R> 2DROP >R 1
    THEN ;

\ Retrieve the index of the loop
: I \ ( -- i )
    1 RPICK ;

\ Retrieve the index of the surrounding loop
: J \ ( -- j )
    3 RPICK ;

\ Retrieve the index of the surrounding, surrounding loop
: K \ ( -- k )
    5 RPICK ;
