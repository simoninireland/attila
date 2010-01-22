\ $Id$

\ Compilation helpers
\
\ These words are used to assist compilation. They may not be
\ needed on stand-alone embedded systems.

\ ---------- Literals ----------

\ Compile the top of the stack as a literal
: LITERAL \ ( n -- )
    [COMPILE] XTCOMPILE,
    COMPILE, ; IMMEDIATE

\ Compile the address on the top of the stack as a literal
: ALITERAL \ ( addr -- )
    ['] (LITERAL) XTCOMPILE,
    ACOMPILE, ; IMMEDIATE

\ Compile the xt on the top of the stack as a literal
: XTLITERAL \ ( xt -- )
    ['] (LITERAL) XTCOMPILE,
    XTCOMPILE, ; IMMEDIATE

\ Compile the string on the tp of the stack as a literal
: SLITERAL \ ( addr len -- )
    ['] (SLITERAL) XTCOMPILE,
    SCOMPILE, ; IMMEDIATE

\ Compile a "-delimited string from the input source as a literal
: S" \ ( "string" -- )
    32 CONSUME \ spaces
    [CHAR] " PARSE
    ?DUP IF
	POSTPONE SLITERAL
\    ELSE
\	S" String not delimited" ABORT
    THEN ; IMMEDIATE


\ ---------- Word status ----------

\ VARIABLE (FIND-BEHAVIOUR)

\ Top-level word-finder
\ : FIND \ ( addr n -- 0 | xt 1 | xt -1 )
\     (FIND-BEHAVIOUR) @ EXECUTE ;
: FIND LASTXT (FIND) ;

\ Look up a word, returning its xt
: ' \ ( "name" -- xt )
    PARSE-WORD 2DUP FIND 0= IF
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

\ Extract the first character of the next word in the input stream
: [CHAR] \ ( "word" -- )
    PARSE-WORD CHAR
    POSTPONE LITERAL ; IMMEDIATE


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
