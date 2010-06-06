\ $Id$

\ Compilation helpers
\
\ These words are used to assist compilation. They may not be
\ needed on stand-alone embedded systems.

\ ---------- Literals ----------

\ Compile the top of the stack as a literal
: LITERAL \ ( n -- )
    [COMPILE] (LITERAL)
    COMPILE, ; IMMEDIATE

\ Compile the address on the top of the stack as a literal
: ALITERAL \ ( addr -- )
    [COMPILE] (LITERAL)
    ACOMPILE, ; IMMEDIATE

\ Compile the xt on the top of the stack as a literal
: XTLITERAL \ ( xt -- )
    [COMPILE] (LITERAL)
    XTCOMPILE, ; IMMEDIATE

\ Compile the string on the tp of the stack as a literal
: SLITERAL \ ( addr len -- )
    [COMPILE] (SLITERAL)
    SCOMPILE, ; IMMEDIATE

\ Read a "-delimited string form the input and either leave it on the stack (when
\ interpreting) or compile it as a literal (when compiling). In interpretation mode
\ the string will be destroyed when a new line is read, so it needs to be used immediately
: S" \ ( "string" -- )
    32 CONSUME \ spaces
    [CHAR] " PARSE
    ?DUP IF
	4 USERVAR ( STATE ) @ 0 ( INTERPRETATION-STATE ) = IF
	    \ leave the string on the stack
	ELSE
	    POSTPONE SLITERAL \ compile the string as a string literal
	THEN
\    ELSE
\	S" String not delimited" ABORT
    THEN ; IMMEDIATE


\ ---------- Word finding ----------

\ Flat finder
: (FIND-FLAT) \ ( addr n -- 0 | xt 1 | xt -1 )
    LASTXT (FIND) ;

\ Hook
DATA (FIND-BEHAVIOUR) ' (FIND-FLAT) XT, 

\ Top-level word-finder
: FIND \ ( addr n -- 0 | xt 1 | xt -1 )
    (FIND-BEHAVIOUR) XT@ EXECUTE ;

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

\ Check whether we're interpreting
: INTERPRETING? \ ( -- f )
    4 USERVAR ( STATE ) @ 0 ( INTERPRETATION-STATE ) = ;
