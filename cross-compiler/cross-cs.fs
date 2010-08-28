\ $Id$

\ Start a new structure definition
: (CS-START) \ ( rt -- )
    TOP CS >ST       \ repeat address (here)
    CS >ST            \ elements hidden
    0 LEAVES >ST ;    \ no forward references

\ End a structure definition
: (CS-END) \ ( -- )
    LEAVES ST>
    BEGIN
	?DUP
    WHILE
	    LEAVES ST> DUP JUMP> SWAP !                           \ patch the exit address to here
	    1-
    REPEAT
    CS-DROP ;                                                     \ drop the control structure

\ Compile the address of the start of the current structure
: >BEGIN \ ( -- )
    (CS-BEGIN-ADDR) >JUMP COMPILE, ;

\ Compile a placeholder to the end of the current structure
: >END \ ( -- )
    LEAVES ST>       \ the count of the number of pending exits
    TOP LEAVES >ST   \ push this as an exit
    0 COMPILE,       \ placeholder
    1+ LEAVES >ST ;  \ update the count

\ Compile code to tidy-up the return stack ahead of an early exit
: (LEAVE) \ ( n -- )
    BEGIN
	?DUP
    WHILE
	    RDROP
	    1-
    REPEAT ;


\ ---------- Exits ----------

<WORDLISTS ALSO CROSS ALSO CROSS-COMPILER DEFINITIONS

\ Escape from the current loop, jumping to the end
: LEAVE \ ( -- )
    (CS-R) POSTPONE LITERAL [COMPILE] (LEAVE)
    [COMPILE] (BRANCH) >END ; IMMEDIATE
    
\ Return from this word immediately, unwinding any loops
:  EXIT \ ( -- )
    #CS
    BEGIN
	?DUP
    WHILE
	    DUP 1- CS-PICK
	    (CS-R) POSTPONE LITERAL [COMPILE] (LEAVE)
	    CS-DROP
	    1-
    REPEAT
    NEXT, ; IMMEDIATE

WORDLISTS>
