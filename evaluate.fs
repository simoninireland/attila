\ $Id$

\ Execute a string
\ sd: this might need to be re-written, as it relies on REFILL etc understanding an
\ INPUTSOURCE of -1

\ Evaluate the given string as though it was entered from the terminal
: EVALUATE \ ( addr n -- )
    SAVE-INPUT N>R
    TIB @ SWAP CMOVE
    0 >IN !
    1 NEGATE INPUTSOURCE !
    EXECUTIVE @ EXECUTE
    NR> RESTORE-INPUT DROP ;


