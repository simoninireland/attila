\ $Id$

\ Compilation helpers
\
\ These words are used to assist compilation. They may not be
\ needed on stand-alone embedded systems.

\ ---------- Literal creation ----------

: LITERAL
    ['] (LITERAL) CTCOMPILE,
    COMPILE, ; IMMEDIATE
: XTLITERAL
    ['] (XTLITERAL) CTCOMPILE,
    XTCOMPILE, ; IMMEDIATE
: SLITERAL
    ['] (SLITERAL) CTCOMPILE,
    SCOMPILE, ; IMMEDIATE


\ ---------- Word status ----------

\ VARIABLE (FIND-BEHAVIOUR)

\ Basic finder
: (FLAT-FIND) \ ( addr n -- 0 | xt 1 | xt -1 )
    LASTXT (FIND) ;

\ Top-level word-finder
\ : FIND \ ( addr n -- 0 | xt 1 | xt -1 )
\     (FIND-BEHAVIOUR) @ EXECUTE ;
: FIND LASTXT (FIND) ;

\ Look up a word, returning its xt
: ' \ ( "name" -- xt )
    PARSE-WORD 2DUP (FIND) ( sd: should be FIND ) 0= IF
	TYPE S" ?" ABORT
    ELSE
	ROT 2DROP
    THEN ;

\ Compile the execution semantics of an immediate word
: POSTPONE \ ( "name" -- )
    ' CTCOMPILE, ; IMMEDIATE

\ Grab the xt of the next word in the input at compile time and leave
\ it on the stack at run-time
: ['] \ ( "name" -- )
    ' POSTPONE LITERAL ; IMMEDIATE


\ ---------- Compilation ----------

\ At run-time, compile the next word in the input source at compile time
: [COMPILE] \ ( "word" -- )
    POSTPONE [']
    ['] CTCOMPILE, CTCOMPILE, ; IMMEDIATE


\ ---------- Recursion ----------

\ Call the current word recursively. This is needed because the currently-defined
\ word's name need not be present in the search order until the closing ;
: RECURSE \ ( -- )
    LASTXT CTCOMPILE, ; IMMEDIATE

\ Call the current word tail-recursively. This call doesn't return here,
\ so use with care
\ TBD


\ ---------- State management ----------

\ Enter interpretation state
: [ \ ( -- )
    0 ( INTERPRETATION-STATE ) 4 USERVAR ( STATE ) ! ; IMMEDIATE

\ Enter compilation state
: ] \ ( -- )
    1 ( COMPILATION-STATE ) 4 USERVAR ( STATE ) ! ;

\ Check whether we're interpretting
: INTERPRETING? \ ( -- f )
    4 USERVAR ( STATE ) @ 0 ( INTERPRETATION-STATE ) = ;
