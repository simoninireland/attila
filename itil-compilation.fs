\ $Id$

\ Words needed to support compilation, indirect-threaded version

\ Compile the ct of an xt
: CTCOMPILE, XTCOMPILE, ;

\ Compile the end of a word
: NEXT, 0 COMPILE, ;

\ Prepare for a forward jump
: JUMP-FORWARD ( -- a )
    TOP
    0 COMPILE, ;

\ Resolve a jump from the address on the stack to here
: JUMP-HERE ( a -- )
    TOP OVER -
    SWAP ! ;

\ Resolve a jump from here backwards to the address on the stack
: JUMP-BACKWARD ( a -- )
    TOP -
    COMPILE, ;
